# AGENTS.md

Guidance for AI agents working on **the-spin** — a top-down 2D beyblade battle game.

## Project

- **Engine:** Unity `6000.3.9f1` (Unity 6.3), **2D URP** (`Assets/Settings/Renderer2D.asset`, `UniversalRP.asset`)
- **Product name:** `just-spin` · **Company:** `CaptainCat`
- **Gameplay:** mouse position = move target, left click = dash. RPM is stamina *and* health — drain the opponent's RPM to zero to win.
- **Scenes:** `TitleScene` (build index 0) → `BattleScene` (index 1). `ExploreScene` holds Fungus narrative and is **not** in the build settings.
- **Third-party:** DOTween (tweening), Fungus (narrative flows), TextMeshPro, usfxr.

## Layout

| Path | Contents |
|---|---|
| `Assets/Game/Scripts/<Feature>/` | Game code — features: `Audio`, `Bey`, `Bey/Visual`, `Camera`, `Core`, `Data`, `Enemy`, `Input`, `Interfaces`, `UI` |
| `Assets/Game/ScriptableObjects/` | ScriptableObject **classes** |
| `Assets/Game/Datas/` | ScriptableObject **instances** (`.asset`) |
| `Assets/Game/Prefabs/` | `Player.prefab`, `Enemy.prefab`, `HitParticle.prefab` |
| `Assets/Game/Scenes/` | The three scenes |
| `Assets/Game/Art`, `Audio`, `Materials` | Assets only |
| `Assets/Plugins/**` | **Vendored third-party — never edit** |
| `Assets/Settings/`, `Assets/Resources/` | URP config, DOTween settings |

## Architecture

### Wiring pattern

Two mechanisms, no overlap: **per-bey components** are wired by hand, **scene services** go through `ServiceLocator`.

`BeyController` is the hub. In `Awake` it caches its sibling components off the same `GameObject` and calls `Initialize(...)` on them by hand. Beys do **not** use the service locator.

```
BeyController (hub)
├── BeyData      — RPM / stamina state, events
├── BeyPhysics   — Rigidbody2D movement + collisions
├── BeyCombat    — impact & grind resolution
├── IBeyInput    — PlayerBeyInput (mouse) | EnemyInput (AI)
└── BeyVisual    — child: spin animation
```

Add new per-bey behavior as a **sibling component** plus one `Initialize` call in `BeyController`. Do not grow `BeyController` into a god object, and do not let components pull each other with `GetComponent` outside the hub.

### Scene services (`ServiceLocator`)

`Core/ServiceLocator.cs` is a static, type-keyed registry that replaced the old per-class `static Instance` singletons. Services register themselves in `Awake` and unregister in `OnDestroy`, so adding one needs **no scene or prefab wiring**.

```csharp
// Provider
private void Awake()
{
    if (!ServiceLocator.TryRegister(this))
    {
        Destroy(gameObject);
        return;
    }
}

private void OnDestroy()
{
    ServiceLocator.Unregister(this);
}

// Consumer — resolve lazily at the call site, never cache in Awake
ServiceLocator.Get<SFXPlayer>()?.PlayHit();
```

| Member | Semantics |
|---|---|
| `Register<T>` | Overwrites any existing entry (last wins). Used by `CameraShake`, `HitEffectSpawner`. |
| `TryRegister<T>` | First wins; returns `false` when already registered. Used by `GameUIManager`, `SFXPlayer`, `TitleAudioManager`, which destroy the duplicate `GameObject`. |
| `Get<T>` | Returns `null` when nothing is registered — always null-propagate with `?.`. |
| `Unregister<T>` | Clears the slot **only** while it still points at that exact instance. |

That last rule is what keeps scene reloads safe: on `Retry()`, the outgoing instance's `OnDestroy` cannot wipe the incoming instance's registration, whichever destruction order Unity uses. Don't "simplify" it into an unconditional remove.

Services are **scene-scoped**, and each lives in exactly one scene:

| Service | Scene |
|---|---|
| `GameUIManager`, `SFXPlayer`, `CameraShake`, `HitEffectSpawner` | `BattleScene` |
| `TitleAudioManager` | `TitleScene` |

Cross-scene lookups therefore legitimately return `null` — `GameUIManager.Pause()` resolves `TitleAudioManager` and silently no-ops in `BattleScene`. Null-propagate those rather than copying the service into every scene.

### Per-component map

**Core**

- `ServiceLocator` — static type-keyed registry for scene services. See "Scene services" above for the registration contract.
- `GameManager` — owns match flow. Holds the player, `EnemySpawner`, and both `BeyUI`s. Subscribes to `player.Data.OnRPMDepleted` (→ lose) and the current enemy's `OnRPMDepleted` (→ defeat, destroy, spawn next after 1s, or win once `enemiesDefeated >= totalEnemies`). A `finished` latch prevents double resolution. Unsubscribes in `OnDestroy`.

**Bey**

- `BeyController` — hub. `Update` gates on `GameUIManager.CanReceiveGameplayInput`, then turns `IBeyInput.DashRequested` into `Physics.Dash(dir)` and banks a charged hit **only if the dash succeeded**.
- `BeyData` — **single source of truth for RPM.** Exposes `CurrentRPM`, `RPMNormalized`, and events `OnRPMChanged(float)` / `OnRPMDepleted`. `SetRPM` clamps to `0..maxRPM` and raises `OnRPMDepleted` only on the alive→zero transition. **All damage must route through `ReduceRPM`.**
- `BeyPhysics` — `Rigidbody2D` steering toward `input.MoveTarget`; acceleration scales with `RPMNormalized`; velocity clamped to `Base.maxVelocity`. `Dash()` returns `false` (doing nothing) when RPM is below `dashRPMCost`; otherwise it costs that RPM, suppresses steering for 0.2s, and returns `true`. Publishes `OnImpact` (collision enter) and `OnGrind` (collision stay).
- `BeyCombat` — resolves impacts: hit power = `relativeVelocity × attack × Lerp(0.5,1,RPM)`, ×1.75 for a charged hit, ×1.5 when spin directions oppose; weight divides knockback; RPM damage = hit power × 2 to the opponent. Grind applies small continuous damage from the RPM difference. Emits SFX, camera shake, and a hit particle.
- `Visual/BeyVisual` — purely cosmetic: rotates `visualPivot` proportional to RPM.
- `Visual/BeyUI` — binds to a `BeyData` and drives the RPM bar/text; pulses and reddens on damage. `Bind(null)` unbinds. Unsubscribes in `OnDestroy`.
- `Visual/BeyVisualRandomizer` — randomizes layer sprites + a `BeyColorPalette` on `Awake`. Declares the `BeyColorPalette` type (`outer`/`middle`/`inner`).
- `BeyBaseSO` — per-bey tuning (velocity, dash, RPM, attack, weight, spin direction). Instances: `BasePlayer.asset`, `BaseEnemy.asset`.

**Input & AI**

- `Interfaces/IBeyInput` — the whole input contract: `MoveTarget` (world space) and `DashRequested`. **New control schemes implement this interface — never touch `BeyPhysics`.**
- `Input/PlayerBeyInput` — mouse position → `MoveTarget`, left click → `DashRequested`.
- `Enemy/EnemyInput` — AI: wanders around a target with a random offset refreshed each second, and rolls a dash on a 1.5–3s timer (70% chance when >4 units away, else 20%). Auto-finds the `Player` tag if `target` is unset.
- `Enemy/EnemySpawner` — instantiates `enemyPrefab` at a random `spawnPoints` entry.

**Presentation / UI**

- `UI/GameUIManager` — scene service. Esc toggles pause via `Time.timeScale`; shows the game-over panel; exposes `CanReceiveGameplayInput`, which beys must gate their input on. `Retry` reloads the scene, `MainMenu` loads `"TitleScene"`, `Quit` stops play mode in the editor.
- `Audio/SFXPlayer` — scene service; random-pitch one-shot hits (`PlayHit`).
- `Audio/TitleAudioManager` — scene service; BGM + hover/click SFX used by menus and pause.
- `Camera/CameraShake` — scene service; decaying `Random.insideUnitCircle` offset in `LateUpdate`.
- `HitEffectSpawner` — scene service; spawns `HitParticle` at a contact point.
- `UI/TitleUIManager`, `RingAnimator`, `TitleAnimator`, `UIButtonSFX` — title screen presentation (DOTween + button SFX).

**Data**

- `BeyBaseSO` — see above; the tuning entry point for balance changes.

### Non-negotiable rules

1. **RPM is the only health system.** Never add a parallel HP/stamina field — route damage through `BeyData.ReduceRPM` and let `OnRPMDepleted` end the match.
2. **De-duplicate collisions.** Two beys produce two collision callbacks, so `BeyCombat` guards with `IsCollisionOwner(other)` (compares `GetInstanceID()`). Every new collision/impact handler needs the same guard, or damage double-applies.
3. **Events:** always `-=` before `+=`, and unsubscribe in `OnDestroy`. Existing subscription leaks are the thing most likely to break scene reloads.
4. **Scene services:** register in `Awake`, unregister in `OnDestroy`, resolve with `ServiceLocator.Get<T>()`, and always null-propagate with `?.`. Never cache a resolved service in `Awake` — the provider may register later.
5. **Input stays behind `IBeyInput`.** `BeyPhysics` must not read `Input` directly.
6. **Gate input reads on `GameUIManager.CanReceiveGameplayInput`.** `Update` keeps running while paused and after game over — `timeScale = 0` only stops `FixedUpdate` — so an ungated click still mutates state (it drains RPM through `BeyPhysics.Dash`). `BeyController.Update` checks this before acting.
7. **A dash costs RPM and awards exactly one charged hit.** `BeyPhysics.Dash` returns whether the dash actually happened; only then does `BeyController` bank the hit via `Combat.AddChargedHit()`. Calling `AddChargedHit` unconditionally lets a player spam-click below the dash cost to bank 1.75× hits that were never paid for.
8. **New narrative content goes in Fungus flows,** not C#.

### Known warts

- `TitleUIManager.gameplayScene` defaults to `"Game"` in code, but `TitleScene` serializes `BattleScene`. Trust the scene value; the C# default is stale.

## Code style

- **No namespaces, no assembly definitions** for game code — everything compiles into `Assembly-CSharp`. Don't add either unasked.
- **Script file names must match their class name.** Unity requires this for `MonoBehaviour`/`ScriptableObject`, and a mismatch breaks attachment and asset creation. Two violations were already fixed by renaming the file *and* its `.meta` together (preserving the GUID, so scene/prefab references survive) — don't reintroduce them.
- **C# 9 / `netstandard2.1`** (Unity's defaults).
- Inspector fields: `[SerializeField] private` for plain values; `[field: SerializeField] public T P { get; private set; }` when other components need read access. Public fields only on ScriptableObjects.
- `[Header]` groups on serialized blocks in prefabs/SOs.
- Group features by nested folder with a matching flat script folder (`Bey/`, `Bey/Visual/`).
- Match the surrounding formatting, including the airy multi-line invocation style used throughout (arguments on their own lines, closing paren on its own line). Don't reformat untouched code.
- Private field casing is **mixed** (`_currentRPM` in `BeyData`, bare `rb` / `data` in `BeyPhysics`). Follow the file you're editing; don't stage a repo-wide rename.
- Keep logging meaningful: `Debug.LogError` for missing references, no new `Debug.Log` in hot paths.

## Verification

There is **no test project**; verify with a compile check plus play-mode testing. The one exception is `Core/ServiceLocator.cs`, which has no `UnityEngine` dependency — if you change it, it can be exercised in an isolated console project pinned to `LangVersion 9.0` to match Unity.

**Fast compile check** (from the repo root, ~7s, currently 0 errors / 2 pre-existing Fungus warnings):

```powershell
dotnet build Assembly-CSharp.csproj -v m
```

> **Trap:** the `.csproj` files are Unity-generated. A brand-new `.cs` file is not in any csproj until Unity re-imports, so `dotnet build` will silently **skip** it. Focus the Unity editor (regenerating the csprojs) before trusting the build after adding files.

**Play-mode checklist for gameplay changes** — open `BattleScene` and confirm:

1. Player follows the mouse; left click dashes and drains RPM.
2. Colliding with the enemy damages **both** beys' RPM (not one, not four times).
3. Enemy RPM bar drains to zero → enemy is destroyed and the next one spawns after ~1s.
4. Clearing all enemies shows the win panel; player RPM hitting zero shows the lose panel.
5. Pause (Esc) freezes the action and can be resumed.
6. Clicking while paused — and after the match ends — does **not** drain RPM or bank charged hits.

Unity is installed at `D:\Projects\Unity\6000.3.9f1\Editor\Unity.exe`. Batch-mode compilation (`-batchmode -quit -projectPath .`) is possible but needs a license, so prefer the `dotnet build` check above.

## Guardrails

- **Never edit `Assets/Plugins/**`** (DOTween, Fungus, usfxr, TextMeshPro) — vendored code.
- **Editing `.unity`, `.prefab`, and `.asset` YAML by hand is allowed** — it's often the fastest way to inspect or wire up content. One trap: a prefab instance placed in a scene is rebuilt from its prefab file, so a change to it only sticks if it's expressed as an `m_Modifications` override rather than an inline edit.
- **Never hand-edit an existing `.meta`.** Changing or duplicating a `guid` silently breaks every reference to that asset across scenes and prefabs. Adding a `.meta` for a brand-new file with a fresh unique `guid` is fine — the format here is just `fileFormatVersion` + `guid`. Editing or deleting one that something already references is not.
- Don't add packages, assembly definitions, namespaces, or third-party libraries without asking first.
- Keep using the **legacy `Input` API** already wired throughout the game code (project is `activeInputHandler: 2` / Both, and the Input System package is unused in game scripts).
- Don't commit `Library/`, `Temp/`, `Logs/`, `obj/`, or `UserSettings/` — `.gitignore` covers these; keep it that way.
