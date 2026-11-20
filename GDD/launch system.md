# Launch System — Future Slice

**Status: implemented.** The future-slice queue is empty again.

Goal: a ripcord launch before each wave, replacing the automatic wave reset. The
player pulls the cord with the mouse; pull distance sets starting RPM.

---

## 1. Concept

Real beyblade launches are a skill check before the fight:

1. Insert the ripcord.
2. Pull it out fast.
3. The bey spins up and drops into the stadium.

The game version keeps that beat but stays simple: **hold, pull down, release**.

---

## 2. V1 interaction

1. **Hold** left mouse anywhere during the launch phase.
2. **Pull down**. A cord line follows the cursor.
3. **Release**. The cord snaps out and the bey launches.

```text
power = clamp01(pullDistanceY / tuning.playerPullMax)
```

- Distance-based power for v1. A speed-based curve can replace it later behind
  the same `ILaunchPowerModel` interface.
- Start position: `arena.Center + (0, arena.Radii.y * launchSpotFactor)`, near the
  top of the oval.
- Launch impulse toward the center, scaled by power.
- Starting RPM: `maxRPM × Lerp(minStartFactor, 1, power)`.
- Bad pull floor is 70%, so a bad launch never loses the wave by itself.

---

## 3. Modular architecture

The core launch does one thing: set starting RPM and raise an event. Everything
else subscribes.

```text
LaunchController
  phase state machine
  ILaunchInteraction  (v1: hold + pull down + release)
  ILaunchPowerModel   (v1: distance)
  resolve  →  BeyData.Launch(power)
           →  BattleEvents.LaunchResolved(bey, power)
```

Events:

```text
BattleEvents.LaunchStarted(BeyData bey)
BattleEvents.LaunchResolved(BeyData bey, float power)
```

| Future mechanic | How it plugs in |
|---|---|
| Perfect-launch charged hit | subscriber: power ≥ `perfectThreshold` → `Combat.AddChargedHit` |
| Launch aura / status | subscriber: add a `StatModifier` with source `launch:` |
| Swipe / timing-bar input | replace the mouse block in `LaunchController.Update` |
| Speed-based power | replace `LaunchMath.Power`; it is a pure static helper |
| Per-wave launch personality | read an `AiProfile` override; core unchanged |

Input and power interfaces (`ILaunchInteraction`, `ILaunchPowerModel`) are
deliberately deferred until a second implementation exists. `LaunchMath` keeps the
math testable, and the launch events are the stable seam.

This matches the source-tagged extension model in `GDD/battle system.md` §18:
new behavior is a new subscriber, not an edit to the core loop.

---

## 4. LaunchTuning asset

One ScriptableObject holds every knob, satisfying the "every tuning value lives
in an asset" rule.

| Field | Default | Meaning |
|---|---|---|
| `enabled` | true | fall back to the old auto-reset when false |
| `playerPullMax` | 3.0 world units | distance that maps to power 1.0 |
| `minStartFactor` | 0.7 | worst-pull RPM floor |
| `perfectThreshold` | 0.9 | charged-hit bonus cutoff |
| `perfectChargedHits` | 1 | charged hits granted on a perfect launch |
| `enemyPower` | 0.9 | base AI launch power |
| `enemyVariance` | 0.05 | `clamp01(enemyPower ± variance)` |
| `launchImpulse` | 3.0 | impulse at power 1.0 |
| `launchSpotFactor` | 0.75 | start position as a fraction of `arena.Radii.y` |

Enemy launch:

```text
power = clamp01(enemyPower + Random.Range(-enemyVariance, enemyVariance))
```

`enemyVariance = 0` makes AI launches deterministic. Later, per-wave overrides
(`AiProfile.launchPower`, `launchVariance`) can layer on top without touching the
controller.

---

## 5. Overlay prefab

`LaunchOverlay.prefab` — default Unity sprites only, centered, with a darkened
background.

```text
LaunchOverlay (Canvas: Screen Space Overlay, high sorting order)
├── Darken   Image, default Background sprite, full-screen stretch,
│            color (0, 0, 0, 0.55)
├── Panel    Image, default UISprite, ~400 × 240, anchored center
│   ├── PowerBg    Image, default UISprite, ~320 × 24
│   ├── PowerFill  Image, default UISprite, type Filled, horizontal,
│   │              fillOrigin 0, 0 → 1 with pull
│   └── Hint       TMP text: "HOLD · PULL DOWN · RELEASE"
└── LaunchOverlay.cs  listens to launch events, drives fill, shows/hides root
```

No custom art. The panel and bar use the built-in default sprites so the prefab
is immediately usable and easy to re-skin later.

---

## 6. Flow and gating

```text
Wave cleared
  → 1 s delay
  → LaunchController.Begin()
      player GameObject disabled (no movement, dash, or ability input)
      overlay shown
      wait for hold + pull down + release
  → resolve
      start RPM + position + impulse
      player GameObject enabled
      enemy spawned via existing SpawnNextEnemy
      overlay hidden
  → battle continues unchanged
```

- The player object is disabled during the overlay, so the existing gate does not
  need a new state.
- No enemy exists during the launch, so no combat can resolve early.
- If `LaunchTuning.enabled` is false, `GameManager` keeps the current automatic
  `ResetPlayerForNextWave` path.

---

## 7. Code points

| File | State |
|---|---|
| `LaunchController.cs` | Done. Phase, input, power, resolve, events. |
| `LaunchMath.cs` | Done. Power, start RPM, enemy power. |
| `LaunchOverlay.cs` | Done. Show/hide and power fill. |
| `LaunchTuning.cs` + `LaunchTuning.asset` | Done. All knobs, including enemy variance. |
| `BattleEvents.cs` | Done. `LaunchStarted`, `LaunchResolved`. |
| `BeyData.cs` | Done. `Launch(float power, float minStartFactor)`. |
| `BeyPhysics.cs` | Done. `Launch(direction, impulse)`. |
| `GameManager.cs` | Done. `BeginWave()` replaces the auto-reset when launch is enabled. |
| `BattleScene.unity` | Done. `LaunchController` on the GameManager object, overlay prefab instance, tuning reference. |
| `LaunchOverlay.prefab` | Done. Canvas + darken + centered panel + filled power bar + TMP hint. |

`PlayerBeyInput` stays unchanged: the controller owns the launch mouse input and
the player object is disabled for the phase.

---

## 8. Balance

- Worst pull: 70% RPM. Best pull: 100% + one charged hit.
- Enemy power 0.9 ± 0.05 by default; later waves can raise the base slightly.
- The charged-hit reward reuses `Combat.AddChargedHit`, so no new combat path.
- Because every wave starts with a launch, the checkpoint reset becomes the
  fallback rather than the default.

---

## 9. Tests

**EditMode — done** (`LaunchSystemTests`)

- Power clamps to 0..1 and maps distance correctly.
- `BeyData.Launch` produces `maxRPM × Lerp(0.7, 1, power)`.
- Enemy power with zero variance equals the base value.

**PlayMode — pending manual check**

- Hold + pull down + release resolves the launch.
- No dash or charged hit is banked during the phase.
- Wave advance still spawns the next enemy after resolve.
- `enabled = false` keeps the old auto-reset behavior.

**Verification:** EditMode 106/106, PlayMode 20/20, `dotnet build` clean.

---

## 10. Risks

- Pull feel depends on `playerPullMax`; tune in world units, not pixels.
- Disabling the player object must not break the wave-reset coroutine; resolve
  before `SpawnNextEnemy`.
- The overlay must use unscaled time if the game is ever paused during launch.
- Cord visuals can be a simple `LineRenderer`; do not block the slice on art.

---

## 11. Slice checklist

1. Add `LaunchTuning` asset and defaults — done.
2. Add `LaunchController`, `LaunchMath`, and mouse input — done.
3. Add overlay prefab with default sprites and dark background — done via
   `Assets/Game/Editor/LaunchOverlayPrefabBuilder.cs`.
4. Add `BeyData.Launch` and `BeyPhysics` impulse — done.
5. Insert the phase into `GameManager` between wave clear and `SpawnNextEnemy` — done.
6. Add events and the perfect-launch charged hit — done.
7. Add EditMode tests — done.
8. Run suites — EditMode 106/106, PlayMode 20/20.
9. Manual six-wave playtest — pending.
