# Battle System Plan (v4 — re-baselined)

| | |
|---|---|
| **Project** | the-spin (Unity 6000.3.9f1, C# 9, netstandard2.1) |
| **Revision reviewed** | `63a0a0e8faa23f64d2a551d963e3835c603641ca` |
| **Supersedes** | v3 (reviewed at `f010177`) |
| **Status** | Draft v4 — audit re-run and re-baselined against current HEAD |
| **Scope** | Complete battle-system rework (4 tracks, 12 phases) |

Names are used as written in the brief: elements **Pyre, Crye, Gyre, Hydre, Sparke**, plus **Neutral**. Part slots use **Blade, Ratchet, Bit** as the default set. All names need confirmation before content is authored (Open Questions 1 and 7).

**Extension docs:** `elemental system.md` (elements), `arena system.md` (oval arena), and three future-slice specs in priority order: `ring out feature.md`, `wave ramp feature.md`, `arena art feature.md`.

---

## 1. Summary

Two beys collide, damage and knockback resolve, and running out of RPM ends the match. v4 keeps the v3 design and re-baselines it against the committed code.

Every bey gets an identity built from four layers:

- **Type** (ATK, DEF, BAL) sets the baseline stat profile.
- **Parts** (Blade, Ratchet, Bit) customize the bey. A part can change stats, claim an element, grant skills, or tune how skills work.
- **Element** (Neutral, Pyre, Crye, Gyre, Hydre, Sparke) sets its matchups. Neutral is the default and is even against everything.
- **Abilities** (passives and actives) are the skills. They come from three sources: innate, part-granted, and equipped.

Everything still flows through source-tagged mechanisms: stat modifiers, element claims, and pending impact modifiers. Each type, part, aura, and ability is data. It can be added, removed, or explained without touching the others.

---

## 2. What Changed in v4 (re-baseline)

v3 was authored against `f010177` and explicitly left ten files unreviewed. Since then, `63a0a0e` landed:

- **ServiceLocator** replaced static `Instance` singletons for `GameUIManager`, `SFXPlayer`, `CameraShake`, `HitEffectSpawner`, and `TitleAudioManager`.
- **Input gating** now exists in both `PlayerBeyInput` and `EnemyInput` via `GameUIManager.CanReceiveGameplayInput`.
- **Reference validation** and **event unsubscription** were added across `BeyController`, `BeyData`, `BeyPhysics`, `BeyCombat`, `BeyUI`, `GameManager`, `EnemySpawner`, `GameUIManager`, `SFXPlayer`, and `HitEffectSpawner`.
- **Dash rules** were corrected: `BeyPhysics.Dash` returns success, and only a successful dash banks a charged hit.
- **Tests** exist: `Assets/Tests/EditMode` (ServiceLocator) and `Assets/Tests/PlayMode` (input gating, malformed bey init), with test asmdefs.
- **Managed stripping** was lowered from High to Low.

Consequences for the plan:

| v3 assumption | v4 state |
|---|---|
| B3 (one-frame dash flag can be missed by a `FixedUpdate` consumer) | **Retired.** The consumer is `BeyController.Update`; the flag persists until the producer's next `Update`. No miss, no double-spend. Edge-triggered `ConsumeDash()` remains a design goal, not a bug fix. |
| M3 (EnemyInput checks `GameUIManager` every frame) | **Confirmed and wider.** `PlayerBeyInput` does it too. The fix is a battle-state gate, not an EnemyInput-only change. |
| M5 (`?.` on Unity objects) | **Corrected.** Current `ServiceLocator.Get<T>()?.Method()` is safe: `Get` returns a genuine null. The rule is "never `?.` a possibly-Destroyed `MonoBehaviour` reference." |
| M5 (possible double subscription in `Initialize()`) | **Retired.** `BeyPhysics.Initialize` guards with `-=` then `+=`; `BeyCombat` subscribes once and now unsubscribes in `OnDestroy`. |
| Seven unreviewed files | **Reviewed.** All battle files were read at `63a0a0e`; five new findings were added (§4). |

One structural gap was found in the v3 stat model and is resolved in v4: **movement tuning has no home** in `StatId`. `BeyBaseSO` carries `maxVelocity`, `acceleration`, `dashImpulse`, `minMoveAccelerationMultiplier`, plus two dead fields. v4 adds a `BeyStats.movement` block (§9.2) and does not route movement through `StatBlock` in this rework.

---

## 3. Goals and Non-Goals

**Goals**

- Fix the confirmed collision, charge, and contact bugs.
- Separate combat from presentation and from scene services.
- Make types, elements, parts, and abilities assets. Adding one needs no core code.
- Any part can be removed without leaving stat or element residue.
- Neutral is a stable baseline designers can rely on.
- Every stat value, element decision, and impact can explain its source.

**Non-Goals (v4)**

- No mid-battle part swapping (possible later, §13.7).
- No economy, rarity, or progression systems.
- No element-typed damage (a Phase 11 candidate).
- No multiplayer or networking. Seeded RNG is for tests and replays.
- No art, UI, or audio redesign.
- No namespaces or assembly definitions (project rule; layering stays a convention in `Assembly-CSharp`).
- No movement-stat modifiers in v4 (movement stays chassis data).

---

## 4. Current State and Audit (re-run at `63a0a0e`)

**Files reviewed:** `BeyCombat`, `BeyController`, `BeyPhysics`, `BeyData`, `PlayerBeyInput`, `EnemyInput`, `EnemySpawner`, `GameManager`, `GameUIManager`, `ServiceLocator`, `SFXPlayer`, `CameraShake`, `HitEffectSpawner`, `BeyUI`, `BeyVisual`, `BeyBaseSO`, both prefabs (component wiring), both `BeyBaseSO` instances, and the BattleScene wiring diagram.

**Not reviewed:** scene YAML beyond component wiring, Fungus flows, art/audio assets.

| ID | Type | Where | Finding at `63a0a0e` | Status | Phase |
|---|---|---|---|---|---|
| B1 | Bug | `BeyCombat.IsCollisionOwner` | Compares a `BeyCombat` instance ID with the other's `BeyController` ID. Each side computes a different pair, so both can claim ownership (double damage) or neither can (hit dropped). | **Confirmed** | 1 |
| B2 | Bug | `BeyCombat.ApplyChargeBonus` | Only the collision owner's outgoing hit calls `ApplyChargeBonus`. The non-owner's banked charge is neither applied nor spent. | **Confirmed** | 1 |
| B3 | Bug | `EnemyInput.DashRequested` | Claimed one-frame miss by a `FixedUpdate` consumer. The consumer is `BeyController.Update` and the flag persists until the producer's next `Update`. | **Retired** (design preference only) | — |
| B4 | Bug | `BeyCombat.ResolveEffect` | `collision.GetContact(0)` throws when `contactCount == 0`. | **Confirmed** | 1 |
| B5 | Cleanup | `GameManager.Update` | Dead stub: returns unless `finished`, then checks Escape with an empty body. | **New** | 1 |
| D1 | Design | `BeyCombat.ResolveGrind` | Raw RPM difference × `0.001` × `fixedDeltaTime`, applied equally to both. Equal RPM grinds do nothing; a 3000-RPM bey grinds 30× harder than a 100-RPM one; no normalization. | **Confirmed** (adds D7) | 4 |
| D2 | Design | `BeyCombat.ResolveImpact` | No defense stat. | **Confirmed** | 3, 6 |
| D3 | Design | `BeyCombat.ResolveImpact` | Knockback is unclamped and uses center-to-center direction instead of the contact normal. | **Confirmed** | 4 |
| D4 | Design | `EnemyInput` | No input from the player's state or the enemy's own RPM. Chases with jitter. | **Confirmed** | 9 |
| D5 | Design | `EnemyInput` | Dash rolls on a 1.5–3 s timer at fixed probability (0.2 near, 0.7 far). No wind-up. | **Confirmed** | 9 |
| D6 | Scope gap | Whole battle | No type, element, ability, or part system. Charges are the only special impact effect. | **Confirmed** | 6–8 |
| D7 | Design | `BeyCombat.ResolveGrind` | Raw RPM scale makes grind damage depend on chassis `maxRPM`, not on normalized spin. Folded into the Phase 4 grind decision. | **New** | 4 |
| M1 | Maintainability | `BeyCombat` | Depends on `BeyController`, `BeyPhysics`, `BeyData`, and `ServiceLocator` (`Get<SFXPlayer/CameraShake/HitEffectSpawner>`). | **Confirmed** (scene coupling reduced, not removed) | 5 |
| M2 | Maintainability | Both | About 20 magic numbers (§9.5 and Phase 2). | **Confirmed** | 2 |
| M3 | Maintainability | `EnemyInput` **and** `PlayerBeyInput` | Both resolve `GameUIManager` through `ServiceLocator` and gate input every `Update`. Duplicated gating, scene-service coupling in input producers. | **Confirmed, wider** | 1 |
| M4 | Maintainability | `EnemyInput` | Unseeded `UnityEngine.Random`. Runs can't be reproduced. | **Confirmed** | 0 |
| M5 | Cleanup | `BeyCombat` | Unused `opposite` field (serialized in both prefabs), unused `using TMPro`, dead `dashChance = 0.3f` initializer. | **Confirmed** (two items retired in §2) | 1 |
| M6 | Cleanup | `BeyData` | Serialized `_currentRPM` mirror is written on every change but never read. Duplicated state. | **New** | 1 |
| M7 | Cleanup | `BeyBaseSO` | `dashStrength` and `minDashMultiplier` are unused by any script (only asset YAML). Dead tuning fields. | **New** | 2 |
| M8 | Bug (tuning) | `BeyVisual` | A second serialized `maxRPM` (3000) drives spin speed instead of `BeyData.Base.maxRPM`, so visual speed can drift from the bey's tuning. | **New** | 1 |
| M9 | Maintainability | `EnemyInput.Start` | Falls back to `GameObject.FindWithTag("Player")`; scene lookup in an input component. | **New** | 9 |

**Current status.** All findings above are resolved or retired at the current HEAD: B1/B2/B4 fixed in Phase 1, B3 retired, B5/M6/M7/M8 removed in code, D1–D7 resolved in Phases 3–9, and M1–M9 resolved through the ServiceLocator and input-gate refactor. The Status column records each finding at `63a0a0e`; §23 records the resolutions.

**Phase 1 scope changed accordingly:** B1, B2, B4, B5, M3, M5, M6, M8. B3 and the double-subscription item are removed.

---

## 5. Corrections Carried Forward

- **Spin direction is partly implemented.** `BeyCombat` multiplies both sides by 1.5 on opposite spin. Same-direction behavior and spin steal are still missing.
- **Base stats already exist.** `BeyData.Base` holds `attack`, `weight`, `spinDirection`, and movement tuning. Defense, tip friction, and RPM decay are still missing.
- **The enemy doesn't wander.** It chases the player's position with a random offset of up to 1.2 units.
- **Hardening already landed.** Do not re-add null guards, event unsubscription, charged-hit gating, or reference validation; they exist. Phase 1 works from the current code, not from `f010177`.

---

## 6. Design Decisions

**New in v3, retained**

| Decision | Rationale |
|---|---|
| Neutral is a real element. It is 1.0 against every element in both directions. | A stable middle that designers can build around. |
| Neutral is the default element and the lowest-priority claim. | Every bey has an element, and the baseline is explicit. |
| Element is never stored directly. It resolves from claims. | Parts, auras, and temporary effects can all set it without special cases. |
| Parts contribute only through the existing modifier and claim paths. | A part can't write to a bey directly, so removal is clean. |
| Parts tune skills through scoped stats. | A part can strengthen a skill class without editing the skills. |
| Auras apply effects only while their condition holds, and remove them by source when it stops. | Conditional and temporary effects use the same cleanup path as everything else. |
| Granted skills don't use equipped loadout slots. They have their own limits. | A part's skill is a reward for choosing it, not a cost against the player's own loadout. |
| Parts are fixed at build time in v4. | Keeps battle state simple. Mid-battle swapping is a later design change. |

**New in v4**

| Decision | Rationale |
|---|---|
| Movement tuning lives in `BeyStats.movement`, outside `StatBlock`. | `maxVelocity`, `acceleration`, and `dashImpulse` are chassis feel, not combat modifiers; routing them through modifiers adds risk with no v4 use. |
| B3 is not a bug; `ConsumeDash()` is still adopted. | The current flag cannot be missed, but an explicit consume API removes the Update-order question for AI and abilities. |
| Input gating moves behind a battle-state gate. | Both producers currently reach into `GameUIManager`; one gate keeps pause/game-over semantics in one place. |
| `ServiceLocator` is banned from combat and AI files. | Combat must run headless in tests; presentation subscribes to events instead. |

**Retained from v2**

- Attack raises outgoing hit power, which drives both RPM damage and knockback.
- Defense reduces incoming RPM damage through `K / (K + Defense)`, with `K = 40` by default.
- Weight reduces incoming knockback.
- Element matchups multiply outgoing hit power, before spin rules.
- Self-costs bypass defense and matchups.
- Stat results don't depend on the order modifiers were applied.
- Pending impact modifiers are spent on commit, not on computation.

Open decisions are listed in §21.

---

## 7. Architecture

### 7.1 Principles

1. **Data over code.** Anything a designer might tune is an asset.
2. **One modifier path.** Stats and element claims both carry source tags. Nothing writes them directly.
3. **One mutation point.** Impacts are solved by a pure function and applied in one place.
4. **IDs, not references.** Content refers to content by stable ID through the registry.
5. **Events for side effects.** Presentation reacts to events. Gameplay never calls presentation.
6. **Everything is explainable.** Any stat value, element decision, or impact can list its contributors.
7. **Layering is by convention.** No namespaces or asmdefs (project rule); reviews enforce the module map.

### 7.2 Module Map

| Module | Responsibility | May depend on | Must not know about |
|---|---|---|---|
| Content and registry | Asset base, ID lookup, validation | Nothing | Battle |
| Events | Plain C# event definitions | Nothing | Anything else |
| Stats and claims | `StatBlock` with scopes, `ElementStack` | Nothing | Abilities, parts, scene |
| Types, elements, matchups | Profiles, element identity, neutral rules, matchup tables | Stats and claims, content | Battle runtime |
| Abilities | Triggers, conditions, effects, aura and active lifecycle | Stats and claims, events, `IBeyActions` | Combat internals, parts |
| Parts | Slots, part definitions, contribution rules, budget | Stats and claims, abilities, types | Battle runtime |
| Blueprints | Assemble and validate a bey from its parts | All of the above | Battle runtime |
| Battle runtime | Physics, combat solver, AI, flow, `BeyRuntime` | All of the above | Presentation |
| Presentation | SFX, shake, VFX, UI | Events only | Combat, AI, abilities |

### 7.3 Data Flow

```
Designer assets ──► Compile (IDs to indices) ──► BeyBlueprint ──► BeyBuilder ──► BeyRuntime
                                                  chassis, type, parts, equipped loadout
                                                  └─► installs stats, element claims, abilities

Input ──► BeyPhysics ──contact──► BattleCombat ──► ImpactSolver (pure)
  ▲                                   │                  │
  │                                   ├── apply once ──► BeyRuntime (RPM, knockback, pending mods)
  │                                   └── ImpactResolved ──► Presentation, AI sensors, ability triggers

Snapshot ◄── sensors ◄── BeyRuntime (stats, current element, abilities)
  ├──► EnemyBrain ──► IBeyInput
  └──► BattleController ──► IWinCondition ──► BattleResult
```

### 7.4 Layers

Each layer may depend only on layers above it in this list, never below.

0. Content, registry, events
1. Stats and element claims, matchup tables
2. Types, elements, abilities, parts, blueprints
3. Battle runtime (physics, combat, AI, flow)
4. Presentation (events only)

Abilities and parts reach the battle runtime only through `IBeyActions`, an interface defined in layer 2 and implemented by `BeyRuntime` in layer 3.

---

## 8. Modifier System

This is the backbone. Stats, types, parts, auras, and actives all use it. Element claims use the same pattern.

### 8.1 Stats and Scopes

```csharp
public enum StatId
{
    Attack, Defense, Weight, RpmDecay, TipFriction, DashCost, MatchupScale,
    AbilityCost, AbilityCooldown, AbilityPower    // scoped by ability tag
}

public enum ModOp { Add, PercentAdd, Multiply }
public enum StackPolicy { Refresh, Stack, Ignore }

public readonly struct StatModifier
{
    public readonly StatId Stat;
    public readonly ModOp Op;
    public readonly float Value;
    public readonly string SourceId;    // e.g. "part:heavy_ratchet", "type:atk"
    public readonly string Scope;       // null = all abilities; a tag otherwise
    public readonly float ExpiresAt;    // float.PositiveInfinity = permanent
    public readonly StackPolicy Stacking;
    public readonly int MaxStacks;
}

public interface IStatView
{
    float Get(StatId stat, string scope = null);
}
```

**Scope rules**

- Only `AbilityCost`, `AbilityCooldown`, and `AbilityPower` accept a scope. The validator rejects a scope on any other stat.
- A scoped read includes modifiers whose scope is null (global) or equal to the requested scope.
- Each ability has a `tags` list, for example `dash`, `charge`, `guard`.

**Example.** Heavy Ratchet applies `AbilityCooldown × 0.9` with no scope, so every active recharges faster. Turbo Bit applies `AbilityPower × 1.2` with scope `dash`, so only dash-tagged abilities get stronger.

**Movement is not a stat (v4).** `maxVelocity`, `acceleration`, `dashImpulse`, and `minMoveAccelerationMultiplier` live in `BeyStats.movement` and are read directly by `BeyPhysics`. No modifier targets them in v4.

### 8.2 Formula and Invariants

```
final = (base + sum of Add) × (1 + sum of PercentAdd) × product of Multiply
```

- The same formula applies to scoped reads, over the matching modifiers.
- The result doesn't depend on application order. A test checks this.
- Use `Add` for any stat whose base can be 0. Multiplying zero does nothing. Defense is the main case.

### 8.3 Stacking Policies

| Policy | If the same source is already present | Typical use |
|---|---|---|
| `Refresh` | Resets the timer | Timed buffs (Counter Spin) |
| `Stack` | Adds another copy, up to `MaxStacks` | Cumulative bonuses |
| `Ignore` | Does nothing | Effects that fire repeatedly (Last Spin) |

### 8.4 Source Naming Convention

| Prefix | Meaning | Example |
|---|---|---|
| `base:` | Chassis stats and default element claim | `base:chassis_hammer` |
| `type:` | Type profile | `type:def` |
| `innate:` | Innate passive from a type | `innate:steady_core` |
| `part:` | Part contribution (stats and element claim) | `part:pyre_blade` |
| `grant:` | Ability granted by a part | `grant:ember_bit/ember_aura` |
| `passive:` / `active:` | Equipped ability while it runs | `passive:counter_spin` |

`RemoveBySource` is the single cleanup path. Removing a part, type, aura, or ability removes all of its effects, stats and element claims included.

### 8.5 Element Claims

Element is resolved, never stored. Each source that wants a say adds a claim.

```csharp
public readonly struct ElementClaim
{
    public readonly ElementDefinition Element;   // Neutral is a valid claim
    public readonly int Priority;                // higher wins
    public readonly string SourceId;
    public readonly float ExpiresAt;             // float.PositiveInfinity = permanent
    public readonly long Sequence;               // newer claims win ties
}

public sealed class ElementStack
{
    public ElementDefinition Current { get; }    // highest-priority live claim
    public void Add(ElementClaim claim);
    public void RemoveBySource(string sourceId);
    public void Tick(float now);
    public IEnumerable<ElementClaim> Explain();  // all live claims, sorted by priority
}
```

**Rules**

- A base claim of Neutral at priority 0 is always installed by the blueprint. It's the floor.
- The highest-priority live claim wins.
- Ties go to the newest claim and are logged as a warning. Designers should avoid them.
- Removing a claim's source reverts the element to the next live claim.
- Because priorities are unique among live claims, the resolved element doesn't depend on the order claims were added. Ties are the only exception, which is why the validator blocks ties between parts.

### 8.6 Explainability

- `Explain(stat)` lists each contributing modifier with its source, operation, value, and scope.
- `ElementStack.Explain()` lists each live claim with its priority and source.
- The debug overlay (§20.3) shows both for any bey on screen.

---

## 9. Content Model

### 9.1 Base Definition and Registry

```csharp
public abstract class ContentDefinition : ScriptableObject
{
    public string id;           // stable key used by saves, AI profiles, and the registry
    public string displayName;
}

public static class ContentRegistry
{
    public static T Get<T>(string id) where T : ContentDefinition;
    public static IEnumerable<T> All<T>() where T : ContentDefinition;
}
```

**Asset loading (v4 decision).** The registry loads from an explicit `ContentManifest` asset referenced by the BattleScene, not `Resources` or Addressables. The manifest lists every content asset; the compile step builds the ID map. This keeps loads deterministic and testable without package changes.

### 9.2 Asset Kinds

| Asset | Purpose | Authored by |
|---|---|---|
| `BeyStats` | Chassis base numbers, including `movement` (velocity, acceleration, dash impulse) | Designer |
| `BeyTypeDefinition` | Stat profile and innate passive for a type | Designer |
| `ElementDefinition` | Element identity and color. Neutral is flagged. | Designer |
| `MatchupTable` | Non-neutral matchup entries (§11.4) | Designer |
| `AbilityDefinition` | A passive or active ability, with tags and mode | Designer (composition). Programmer for new classes. |
| `PartSlotDefinition` | A slot (Blade, Ratchet, Bit) and its element priority | Designer |
| `PartDefinition` | A part: stats, element claim, granted abilities, cost | Designer |
| `BeyBlueprint` | A complete bey: chassis, type, parts, equipped loadout | Designer (presets). Players (saved builds). |
| `AiProfile` | Enemy behavior weights and difficulty | Designer |
| `CombatTuning` | Global constants: multipliers, K, caps, limits, intervals | Designer |
| `ContentManifest` | The explicit list the registry compiles | Designer |

### 9.3 Compile Step

At battle load:

- Element and type IDs become dense integer indices. Neutral is index 0 by convention.
- The matchup table becomes a dense `float[,]`, pre-filled with 1.0.
- Ability triggers are grouped into per-trigger lists.
- Slot priorities are collected into a lookup used by `BeyBuilder`.

The runtime never does string lookups during a fight.

### 9.4 Validation

Runs in the editor and again at battle load:

- Duplicate IDs, missing references, self-pairs in the matchup table, duplicate matchup pairs.
- Any matchup entry that involves Neutral.
- Slot priorities that aren't unique.
- A part whose slot doesn't exist, or a blueprint with more than one part in the same slot.
- A `scope` set on a stat that doesn't accept scopes.
- Loadout and granted-ability limits (§12.9).
- Blueprint part budgets above the cap (§13.6).
- Stat modifiers that target a stat with no consumer.
- A `ContentManifest` entry with the wrong asset type or a missing reference.

### 9.5 Literals to Extract in Phase 2

Current literals move into assets **without changing their values**.

| Current literal | Source | Proposed field |
|---|---|---|
| `0.5f`, `1f` (RPM power range) | `BeyCombat` | `minRpmPower`, `maxRpmPower` |
| `2f` (damage multiplier) | `BeyCombat` | `damageMultiplier` |
| `1.5f` (opposite spin) | `BeyCombat` | `oppositeSpinMultiplier` |
| `1.75f` (charge bonus) | `BeyCombat` | Default for `ScaleNextImpact` in the Charge Strike migration |
| `0.02f`, `0.05f`, `0.35f` (shake) | `BeyCombat` | `shakePerSpeed`, `shakeMin`, `shakeMax` |
| `0.001f` (grind rate) | `BeyCombat` | `grindRate` |
| `0.5f`, `1.5f` (weight vs RPM) | `BeyCombat` | `minWeightFactor`, `maxWeightFactor` |
| `1.2f` (wander radius, serialized) | `EnemyInput` | `AiProfile.jitterRadius` |
| `1f` (offset re-roll interval) | `EnemyInput` | `AiProfile.jitterInterval` |
| `4f` (far-distance threshold) | `EnemyInput` | `AiProfile.farDistance` |
| `0.7f`, `0.2f` (dash chance far, near) | `EnemyInput` | `AiProfile.dashChanceFar`, `dashChanceNear` |
| `1.5f`, `3f` (dash interval) | `EnemyInput` | Replaced by the decision tick in Phase 9 |
| `0.3f` (initial dash chance) | `EnemyInput` | Removed (dead value) |
| `maxVelocity`, `acceleration`, `dashImpulse`, `dashRPMCost`, `minMoveAccelerationMultiplier` | `BeyBaseSO` | `BeyStats.movement` |
| `dashStrength`, `minDashMultiplier` | `BeyBaseSO` | **Deleted** (unused, M7) |
| `maxRPM` (duplicated in `BeyVisual`) | `BeyVisual` | Read from `BeyData.Base.maxRPM` (M8) |

New in v4, all in `CombatTuning` unless noted:

| Field | Default | Used by |
|---|---|---|
| `defenseConstant` (K) | 40 | Mitigation (§14.3) |
| `maxEquippedPassives` / `maxEquippedActives` | 2 / 1 | Loadout (§12.7) |
| `maxGrantedPassives` / `maxGrantedActives` | 2 / 1 | Part-granted skills (§12.7) |
| `partBudgetCap` | 10 | Blueprint validation (§13.6) |
| `auraTickInterval` | 0.1 s | Aura passives (§12.5) |
| `maxPendingImpactMods` | 3 | Pending impact modifiers (§14.2) |
| `maxKnockback` | 12 | Knockback clamp (§14.3) |
| `grindMode` | `Symmetric` | Grind decision (§14.4) |

---

## 10. Bey Types: ATK, DEF, BAL

A type is a stat profile plus one innate passive. It's data (`BeyTypeDefinition.profile`), applied with source `type:<id>`. Types set the baseline, and parts modify it. Neutral has no effect on types.

| Trait | ATK | DEF | BAL |
|---|---|---|---|
| Attack | ×1.25 | ×0.81 | ×1.065 |
| Defense | +0 | +20 | +8 |
| Weight | ×0.9 | ×1.2 | ×1.0 |
| RPM decay | ×1.15 | ×0.85 | ×1.0 |
| Innate passive | Surge Strike | Hard Shell | Steady Core |
| Identity | Hits hard, drains fast | Absorbs, heavy, slow to drain | Never hard-countered, never hard-counters |

*All numbers are placeholders for balancing in Phase 6b. Tuned values come from the 25-seed matrix (§20.4).*

**Live decay and friction.** `BeyStats.rpmDecayRate` (BasePlayer/BaseEnemy `0.0015`) drives passive decay: `maxRPM × rate × RpmDecay` per second. `RpmDecay` type multipliers and Last Spin now affect real RPM loss. `TipFriction` multiplies effective weight in `ImpactSolver`, so Grip Bit reduces knockback. `BeyUI` only pulses for losses larger than 1 RPM, so passive decay does not flash the bar.

**Innate passives** are normal abilities with source `innate:`. They don't count against any loadout limit.

- **Surge Strike (ATK):** the first impact after a dash is 15% stronger.
- **Hard Shell (DEF):** +10% defense, permanently.
- **Steady Core (BAL):** matchup scale 0.6. This softens both advantages and disadvantages (§11.4).

A new type is an asset. It needs code only if its innate passive needs an effect that doesn't exist yet.

---

## 11. Elements

Full roster, matchup matrix, on-hit status table, claim priorities, tuning knobs,
and authoring checklist: **`elemental system.md`**.

### 11.1 Roster

| Element | Role |
|---|---|
| **Neutral** | Default. Even against every element. The floor of element resolution. |
| **Pyre** | Ring member |
| **Crye** | Ring member |
| **Gyre** | Ring member |
| **Hydre** | Ring member |
| **Sparke** | Ring member |

### 11.2 Ring

The five non-neutral elements form a ring. Each is strong against one and weak against another. This is a placeholder for designers to review.

| Element | Strong vs (×1.35 when attacking) | Weak vs (×0.75 when attacking) | Rationale |
|---|---|---|---|
| Pyre | Crye | Hydre | Heat melts ice. Water douses fire. |
| Crye | Gyre | Pyre | Ice wedge stalls the vortex. Heat melts ice. |
| Gyre | Sparke | Crye | Wind scatters charge. Ice stalls the spin. |
| Sparke | Hydre | Gyre | Current runs through water. Wind scatters charge. |
| Hydre | Pyre | Sparke | Water douses fire. Current runs through water. |

A Pyre bey attacking a Crye bey uses ×1.35. A Crye bey attacking a Pyre bey uses ×0.75.

### 11.3 Neutral Rules

- Neutral is ×1.0 against every element, in both directions. Neutral has no matchup entries. The validator rejects any entry involving Neutral, so this can't be broken by accident.
- Neutral is the default element and the lowest-priority claim (§8.5).
- A part with a Neutral claim can override a lower-priority part's element (§11.5).
- Neutral works in conditions like any other element: `OwnElement(neutral)` and `OpponentElement(neutral)`.
- **If Neutral feels too weak to choose,** don't change the matrix. Add a conditional passive instead, for example `OwnElement(neutral)` with `AddModifier(Attack, Multiply 1.05)`. This keeps the matrix pure and makes the bonus visible in the explain tools.

### 11.4 Matchup Formula

Pairs not listed in the matchup table are ×1.0.

```
raw       = matchupTable[attacker, defender]     (1.0 if not listed)
effective = 1 + (raw − 1) × attacker.MatchupScale
```

`MatchupRule` scales each side's outgoing power with **that side's** `MatchupScale`; it
does not take the minimum of the two. `MatchupScale` defaults to 1.0, and lower values
dampen that bey's own matchup deviation (both advantages and disadvantages).

Example: a Pyre bey (scale 1.0) attacking a Steady Core bey uses raw ×1.35, so its
outgoing power stays ×1.35. A Steady Core bey (scale 0.6) attacking a Pyre bey uses raw
×0.75, so its outgoing power becomes `1 + (−0.25 × 0.6) = ×0.85`.

### 11.5 Deciding the Element

The element is whichever claim wins (§8.5). The typical claim sources and priorities:

| Source | Priority | Example |
|---|---|---|
| Base | 0 | Neutral default |
| Bit | 10 | Ember Aura (Pyre while RPM is above 50%) |
| Ratchet | 20 | Crye Ratchet (Crye, always) |
| Blade | 30 | Pyre Blade (Pyre, always) |
| Equipped or granted active | 50 | Ignition (Pyre for 4 s, overrides everything while active) |

What follows from this table:

- A Pyre Bit only decides the element when the Blade and Ratchet make no claim.
- A Neutral Blade (priority 30) completely blocks a Pyre Bit (priority 10). This is an intended interaction, not a bug.
- An aura that stops holding lets the next claim show through.

### 11.6 Elements, Abilities, and AI

- Abilities can read the element through conditions. Element on-hit statuses can change stats.
- The `element` tag on an ability is for conditions and visuals only.
- The AI reads the current element each tick. If an aura flips an opponent's element mid-fight, the AI sees it on the next tick.

### 11.7 On-Hit Statuses

Every non-neutral element carries a short stat status, applied when it lands an impact and refreshed on each hit. `ElementStatusResolver` applies `ElementDefinition.onHitModifiers` for `onHitDuration` seconds (3–4 in the tuned demo). `onHitAffectsSelf` chooses the attacker or defender.

| Element | Status | Effect |
|---|---|---|
| Pyre | Burn | Target `RpmDecay ×2.5`, 4 s |
| Crye | Chill | Target `Attack ×0.90`, 3 s |
| Gyre | Gust | Target `Weight ×0.90`, 3 s |
| Hydre | Siphon | Self `RpmDecay ×0.90`, 3 s |
| Sparke | Surge | Self `Attack ×1.08`, 3 s |
| Neutral | None | — |

The reference matrix now includes these statuses on per-duel stat clones.

---

## 12. Abilities

### 12.1 One Model for All Abilities

Passives and actives use the same parts. Actives add a cost, cooldown, and cast time. A passive can run in `Triggered` mode (react to events) or `Aura` mode (apply while a condition holds).

```csharp
public enum AbilityKind { Passive, Active }
public enum PassiveMode { Triggered, Aura }

public enum TriggerId
{
    Always, BattleStart,
    ImpactPending, ImpactDealt, ImpactReceived,
    DashStarted, DashEnded, OpponentDashStarted,
    RpmChanged,
    AbilityActivated,   // an active starts casting
    CastComplete        // an active's effects run
}

public sealed class AbilityDefinition : ContentDefinition
{
    public AbilityKind kind;
    public PassiveMode mode = PassiveMode.Triggered;   // passives only
    public string[] tags;                              // e.g. "dash", "charge"; scopes stats (§8.1)
    public ElementDefinition element;                  // optional; conditions and visuals only
    public TriggerId trigger;                          // Triggered mode, and actives' CastComplete
    [SerializeReference] public List<IAbilityCondition> conditions = new();
    [SerializeReference] public List<IAbilityEffect> effects = new();
    public ActiveSpec active;                          // used when kind == Active
}

[Serializable]
public sealed class ActiveSpec
{
    public float rpmCost;          // fraction of max RPM, paid on activation
    public float cooldown;         // seconds, starts on activation
    public float castTime;         // seconds
    public bool interruptOnHit;
}

public interface IAbilityCondition { bool Evaluate(in AbilityContext ctx); }
public interface IAbilityEffect   { void Apply(in AbilityContext ctx); }

// Layer 2 interface. The only surface abilities can change. Implemented by BeyRuntime.
public interface IBeyActions
{
    IStatView Stats { get; }
    ElementStack Elements { get; }
    float RpmNormalized { get; }
    void AddPendingImpact(PendingImpactMod mod);
    void DrainRpm(float amount);                 // self-cost, bypasses mitigation
    void RequestForcedDash(float power);
    string OwnerId { get; }
}

// Read-only view of the opponent.
public interface IBeyView
{
    ElementDefinition Element { get; }
    float RpmNormalized { get; }
    bool IsDashing { get; }
}

public readonly struct ImpactEvent
{
    public readonly string AttackerId;
    public readonly string DefenderId;
    public readonly ImpactResult Result;          // defined in §14.1
    public readonly Vector2 Point;
}

public readonly struct AbilityContext
{
    public readonly IBeyActions Self;
    public readonly IBeyView Opponent;
    public readonly BattleSnapshot Snapshot;
    public readonly ImpactEvent? Impact;          // set for impact triggers
    public readonly float Now;
    public readonly string SourceId;              // e.g. "passive:counter_spin"
}
```

Concrete conditions and effects are small `[Serializable]` classes that designers pick from a list in the inspector. `[SerializeReference]` lets them sit inline in the asset.

### 12.2 Triggers

| Trigger | Fires when | Typical use |
|---|---|---|
| `Always` | Installed at battle start. Never fires again. | Static bonuses |
| `BattleStart` | Battle begins | Opening setup |
| `ImpactPending` | Impact is computed, not yet committed | Add to the next impact |
| `ImpactDealt` | This bey's impact commits as attacker | Reward hits |
| `ImpactReceived` | This bey's impact commits as defender | Counters |
| `DashStarted` / `DashEnded` | Dash state changes | Surge after dash |
| `OpponentDashStarted` | Opponent begins a dash | Reactive defense |
| `RpmChanged` | RPM changes | Low-RPM effects (use `Ignore` stacking) |
| `AbilityActivated` | An active starts casting | Combos, interrupts |
| `CastComplete` | An active's cast finishes | Where active effects run |

### 12.3 Conditions

| Condition | True when |
|---|---|
| `OppositeSpin` / `SameSpin` | Spin directions differ / match |
| `OwnElement(id)` / `OpponentElement(id)` | Current element matches. Neutral is a valid id. |
| `RpmAbove(x)` / `RpmBelow(x)` | Normalized RPM is above or below x |
| `OpponentDashing` | Opponent is in `WindUp` or `Dashing` |
| `ImpactSpeedAbove(x)` | Relative impact speed exceeds x |

### 12.4 Effects

| Effect | Does |
|---|---|
| `AddModifier(spec, duration, stacking)` | Adds a `StatModifier` with the ability's source tag. Scope can be set for ability stats. |
| `ScaleNextImpact(multiplier, count, duration)` | Adds a `PendingImpactMod`. Replaces the old charge. The bonus is scaled by `AbilityPower` at creation. |
| `ForceDash(power)` | Starts a dash through the normal dash state machine. Power is scaled by `AbilityPower` at activation. |
| `ClaimElement(element, priority, duration)` | Adds an element claim with the ability's source tag |
| `DrainRpm(amount)` | Self-cost. Bypasses mitigation. |
| `EmitEvent(id)` | Publishes a presentation event. Never changes state. |

Removing an aura or an ability removes all of its effects by source. This includes claims and pending modifiers.

**Scaling rules.** A multiplier is scaled as `1 + (value − 1) × AbilityPower`. A magnitude, such as dash power, is scaled as `value × AbilityPower`.

### 12.5 Passive Behavior

**Triggered mode (default).** On each matching trigger, conditions run in order. If they all pass, effects run.

**Aura mode.** Conditions are checked every `auraTickInterval` (default 0.1 s).

- When the conditions start to hold, the effects are applied.
- While they keep holding, nothing is re-applied.
- When they stop holding, every effect from this aura is removed by source.

**Other rules**

- A passive with trigger `Always` and an `AddModifier` effect with infinite duration is a static bonus.
- Repeated triggers respect the stacking policy. Last Spin uses `Ignore` so an RPM change doesn't re-apply it.

### 12.6 Active Lifecycle

```
Ready ──activate──► Casting ──castTime──► Active (CastComplete runs effects) ──► Cooling ──cooldown──► Ready
                       │
                       └── hit while interruptOnHit ──► Cooling (no refund)
```

**Activation requires:**

- State is `Ready`.
- `RpmNormalized ≥ rpmCost × AbilityCost(scope)`, where scope is the ability's tags.
- The bey is not in dash recovery.
- No other active is currently casting. Only one active can cast at a time.

**Then:**

- The cost is paid on activation.
- The cooldown is `cooldown × AbilityCooldown(scope)`, and it starts on activation.
- The cast takes `castTime`. Its effects run on `CastComplete`.
- `AbilityPower(scope)` scales the effects when they apply.

**Input** arrives through `IBeyInput.ConsumeAbilityRequest(slot)`, the same edge-triggered pattern as dash (§15). AI sets `AiCommand.AbilitySlot` to request the same thing.

### 12.7 Ability Sources and Limits

| Source | Granted by | Default limit | Uses equipped loadout slots? |
|---|---|---|---|
| Innate | Type | One innate passive per type | No |
| Granted | Parts | 2 passives and 1 active, total across all parts | No |
| Equipped | Player's loadout | 2 passives and 1 active | Yes (by definition) |

- Limits live in `CombatTuning`, so they can be changed without code.
- Abilities from all sources can be held together. Only one active can cast at a time.
- The validator enforces the limits and rejects over-limit blueprints with a clear message.
- No two abilities in one bey can share an ID, regardless of source.

### 12.8 Example Ability Kit

| Name | Source | Kind and mode | Trigger | Conditions | Effects | Notes |
|---|---|---|---|---|---|---|
| Surge Strike | Innate (ATK) | Passive | `DashEnded` | none | `ScaleNextImpact(1.15, 1, 2 s)` | |
| Hard Shell | Innate (DEF) | Passive | `Always` | none | `AddModifier(Defense, PercentAdd +10%)` | Static |
| Steady Core | Innate (BAL) | Passive | `Always` | none | `AddModifier(MatchupScale, Multiply 0.6)` | Static |
| Counter Spin | Equipped | Passive | `ImpactReceived` | `OppositeSpin` | `AddModifier(Attack, Multiply 1.15, 3 s, Refresh)` | |
| Ember Core | Equipped | Passive | `ImpactDealt` | `OpponentElement(crye)` | `AddModifier(Attack, Multiply 1.10, 2 s, Refresh)` | Element-conditional |
| Last Spin | Equipped | Passive | `RpmChanged` | `RpmBelow(0.25)` | `AddModifier(RpmDecay, Multiply 0.7, Ignore)` | Fires once |
| Charge Strike | Equipped | Active | `CastComplete` | none | `ScaleNextImpact(1.75, 1, 10 s)` | Cost 0.20, cooldown 6 s, cast 0.3 s, interruptible. Replaces the old charge. |
| Guard Spin | Equipped | Active | `CastComplete` | none | `AddModifier(Defense, Add +30, 3 s)`, `AddModifier(Weight, Multiply 1.3, 3 s)` | Cost 0.15, cooldown 8 s |
| Ignition | Equipped | Active | `CastComplete` | none | `ClaimElement(pyre, 50, 4 s)` | Cost 0.20, cooldown 10 s. Overrides all part claims while active. |
| Frost Guard | Granted (Crye Ratchet) | Passive | `ImpactReceived` | `OpponentElement(pyre)` | `AddModifier(Defense, PercentAdd +10%, 2 s, Refresh)` | |
| Ember Aura | Granted (Ember Bit) | Passive, aura | Aura | `RpmAbove(0.5)` | `ClaimElement(pyre, 10)` | Element flips with RPM. Claim lasts while the aura holds. |
| Burst Dash | Granted (Burst Bit) | Active | `CastComplete` | none | `ForceDash(1.5)`, `ScaleNextImpact(1.1, 1, 2 s)` | Tags: `dash`. Cost 0.10, cooldown 5 s |

### 12.9 Loadout Rules

- Equipped: 2 passives and 1 active by default (Open Question 4).
- Granted: 2 passives and 1 active total across all parts (Open Question 5).
- Innate: one per type. Not counted.
- No duplicate ability IDs in one bey.
- An equipped active's base cost must be at most 0.5. Validation rejects anything higher.
- Only one active can cast at a time. The rest are held and wait for their cooldowns.

---

## 13. Parts

### 13.1 Concept

A bey is built from parts, one per slot. A part is a bundle of contributions. It can contribute four things, all through the mechanisms already defined:

1. **Stats.** Always-on stat modifiers.
2. **Element.** An element claim at the slot's priority. Static, or conditional through a granted aura, or temporary through a granted active.
3. **Granted skills.** Passive or active abilities the part provides, held within the granted limits.
4. **Skill tuning.** Scoped `AbilityCost`, `AbilityCooldown`, or `AbilityPower` modifiers that change how matching skills behave.

Any combination is allowed. A plain part may only have stats.

### 13.2 Slots

| Slot | Element priority | Typical role |
|---|---|---|
| Blade | 30 | Attack and element identity |
| Ratchet | 20 | Weight and defense. Often recharges skills. |
| Bit | 10 | Grip and tip. Often a skill source. |

- **Priorities** decide element conflicts (§11.5). They must be unique across slots.
- **Slots are data.** A new slot, such as a Lock Chip or an Assist, is a `PartSlotDefinition` plus a unique priority. It needs no core code.
- **Names** are placeholders for Blade, Ratchet, and Bit (Open Question 7).

### 13.3 Definitions

```csharp
public sealed class PartSlotDefinition : ContentDefinition
{
    public int elementPriority;                       // unique across slots
}

public sealed class PartDefinition : ContentDefinition
{
    public PartSlotDefinition slot;
    public List<StatModifierSpec> statModifiers;      // installed with source part:<id>, permanent
    public ElementDefinition elementClaim;            // null means no claim. Neutral is valid.
    public List<AbilityDefinition> grantedAbilities;  // installed with source grant:<partId>/<abilityId>
    public int budgetCost;                            // designer estimate, verified by simulation
}

public sealed class BeyBlueprint : ContentDefinition
{
    public BeyStats chassis;
    public BeyTypeDefinition type;
    public ElementDefinition baseElement;             // defaults to Neutral
    public List<PartDefinition> parts;                // at most one per slot. Empty slots allowed.
    public List<AbilityDefinition> equippedPassives;
    public List<AbilityDefinition> equippedActives;
}

public static class BeyBuilder
{
    // Installs, in order: chassis, type, base element, parts, innate, granted, equipped.
    // Stat results don't depend on this order (§8.2). Element is resolved from claims (§8.5).
    public static BeyRuntime Build(BeyBlueprint bp, BattleContext ctx);
}
```

### 13.4 Contribution Rules

1. **Stats.** Each `statModifiers` entry is installed with source `part:<id>` and permanent duration. Removal is `RemoveBySource("part:<id>")`.
2. **Element.** If `elementClaim` is set, it's added to the element stack at the slot's priority with source `part:<id>`. A part with no claim leaves the element alone.
3. **Granted skills.** Each granted ability is installed with source `grant:<partId>/<abilityId>` and counted against the granted limits (§12.7). Granted passives run like equipped ones, including aura mode.
4. **Skill tuning.** Stat modifiers with `AbilityCost`, `AbilityCooldown`, or `AbilityPower` and a scope tag change matching abilities. They're part of the stat contributions, not a separate system.

A part can't write to another part or directly to an ability. It only adds contributions through the paths above.

### 13.5 Example Parts

Abilities referenced here are defined in §12.8.

| Part | Slot | Stats | Element claim | Granted skills | Skill tuning | Budget |
|---|---|---|---|---|---|---|
| Pyre Blade | Blade | Attack ×1.10 | Pyre | none | none | 3 |
| Neutral Blade | Blade | Attack ×1.05 | Neutral | none | none | 2 |
| Heavy Ratchet | Ratchet | Weight ×1.2, Defense +10 | none | none | `AbilityCooldown ×0.9` (all abilities) | 3 |
| Crye Ratchet | Ratchet | Defense +15 | Crye | Frost Guard | none | 3 |
| Grip Bit | Bit | Tip friction ×1.05 | none | none | `AbilityPower ×1.1` (scope `dash`) | 3 |
| Ember Bit | Bit | RPM decay ×0.95 | none | Ember Aura | none | 3 |
| Burst Bit | Bit | none | none | Burst Dash | `AbilityCost ×0.9` (scope `dash`) | 4 |

What these show:

- **Neutral Blade** is cheap and gives up element identity. Its Neutral claim (priority 30) always outranks a Bit's claim (priority 10), so an Ember Aura never shows through it. That's the intended trade-off: choosing a Neutral Blade deliberately suppresses the Bit's element.
- **Heavy Ratchet** changes every active's recharge, so it's a build-wide choice.
- **Burst Bit** grants a skill and also makes dash-tagged skills cheaper, so its skills and its tuning reinforce each other.

### 13.6 Budget Guardrails

- Each part has a `budgetCost` set by the designer.
- `CombatTuning.partBudgetCap` (default 10) is the maximum total cost for one blueprint's parts.
- The validator rejects blueprints over the cap.
- Costs are an estimate. The balance simulation verifies them (§20.4). Part power is hard to compute from modifiers alone, so simulation is the authority.

### 13.7 Timing

- In v4, parts are fixed at build time. `BeyBuilder` installs everything at battle start.
- Claims and granted skills run during the battle, so auras and temporary effects still change over the fight.
- If mid-battle swapping is added later, the cleanup is `RemoveBySource("part:<id>")` plus removing the granted abilities. The source model already supports it.

### 13.8 Synergies

Two parts with the same non-neutral element unlock a set bonus at assembly: `Attack +5%` and `Defense +5%`, source `synergy:<element>`. Static claims and permanent granted claims count; Ember Aura's Pyre claim therefore pairs with Pyre Blade.

`PartSynergyResolver` applies and removes the source. `BeyBuilder.RemovePart` clears all `synergy:` sources; removing one part of a pair breaks the bonus.

---

## 14. Combat Pipeline

### 14.1 Stages

Rules run in `Order`. Output, mitigation, and knockback are fixed final stages after the rules.

| Order | Stage | Purpose |
|---|---|---|
| 100 | `BaseHitPower` | `speed × Attack × Lerp(minRpmPower, maxRpmPower, rpmNormalized)` |
| 200 | `MatchupRule` | Element matchup from each side's current element (§11.4) |
| 300 | `SpinRule` | Opposite spin multiplier (current 1.5×). Same-spin behavior is Phase 11. |
| 400 | `PendingImpactRule` | Applies and records pending impact modifiers (§14.2) |
| 500 | (Phase 11) `DashHitRule` | Bonus for a dash that lands |
| Final | Output | `damage = outgoing × damageMultiplier` |
| Final | Mitigation | `damageTaken = raw × K / (K + Defense)` |
| Final | Knockback | `impulse = outgoing / effectiveWeight(target)`, clamped to `maxKnockback` |

The solver takes an `ImpactInput` containing each side's stat view, current element index, pending modifiers, RPM, and the compiled matchup view. It returns an `ImpactResult`:

```csharp
public struct ImpactResult
{
    public float DamageToA, DamageToB;
    public Vector2 ImpulseOnA, ImpulseOnB;
    public IReadOnlyList<string> PendingModsUsedA, PendingModsUsedB;
    public float Intensity;               // 0..1, drives shake and VFX
}
```

### 14.2 Pending Impact Modifiers

```csharp
public struct PendingImpactMod
{
    public string SourceId;     // e.g. "active:charge_strike", "grant:burst_bit/burst_dash"
    public float Multiplier;    // already scaled by AbilityPower at creation
    public int Remaining;       // impacts left before the mod is spent
    public float ExpiresAt;     // seconds; dropped after this time
}
```

- `PendingImpactRule` applies each live mod to the side that owns it and records which mods it used.
- `BattleCombat` removes the used mods when the impact commits, not when it's computed.
- A `Remaining` of 1 with multiplier 1.75 is the old charge. The migration is exact.
- A cap on simultaneous pending mods per bey is set in `CombatTuning` (default 3).

### 14.3 Output, Mitigation, Knockback

- **Output** is the outgoing power after all rules, scaled by `damageMultiplier`.
- **Mitigation** applies only to RPM damage taken. Defense doesn't change knockback.
- **Knockback** uses the contact normal from the contact point, with center-to-center as the fallback. It's clamped to `maxKnockback` (D3).
- Self-costs skip the whole pipeline and call `DrainRpm` directly (§6).

### 14.4 Grind (Option B chosen)

| Option | Behavior | Trade-off |
|---|---|---|
| **A. Symmetric** | Both beys take damage from the normalized RPM gap. Rate in tuning. | Keeps v1 intent. Equal-RPM grinds still do nothing. |
| **B. Damage scales with own RPM (chosen)** | Each bey takes grind damage proportional to its own normalized RPM. | Faster spinners lose more while grinding. Makes spin management matter. |

**Chosen: B.** `GrindSolver` computes `normalized × maxRPM × rate × deltaTime` per bey; `CombatTuning.grindMode` selects B. Option A remains implemented for comparison.

**v4 note.** Whatever the choice, the formula must use normalized RPM, not raw RPM (D7). Current code `|rpmA − rpmB| × 0.001 × fixedDeltaTime` is chassis-`maxRPM`-dependent.

### 14.5 Pair Resolution

Unchanged from v1. One `BattleCombat` per battle dedupes contacts by unordered bey pair, replacing `IsCollisionOwner`.

**v4 note.** The dedupe key must be the pair of `BeyRuntime` IDs, not `GetInstanceID()`. This is the B1 fix. Each pair resolves once per contact, and both sides receive their result from the same `ImpactResult`.

---

## 15. Input and Dash

- `IBeyInput` has `ConsumeDash()` and `ConsumeAbilityRequest(int slot)`. Both are edge-triggered.
- Slots index the held actives, equipped and granted together.
- Requests come from player buttons or from `AiCommand`.
- Consumers read requests in the same loop that drives physics (Open Question 20).
- Dash state machine: `Idle → Dashing → Recover`. There is no wind-up or telegraph; a dash request applies on the same update.
- Dash cost is a self-cost. It goes through `DrainRpm` and bypasses mitigation.

**v4 note.** `ConsumeDash()` replaces the current one-frame `DashRequested` flag. This is a design improvement, not a bug fix (B3 retired). The gate moves from `GameUIManager` to an `IBattleStateGate` service with `CanReceiveGameplayInput`, resolved once per battle rather than per input component.

---

## 16. Enemy Brain

**Sensors** build a `BattleSnapshot` every decision tick (about 0.15 s). The AI reads only the snapshot. v4 adds:

- Each bey's current element. Auras can flip it mid-fight, so the AI reads it every tick.
- Matchup from the enemy's point of view (`Matchup`).
- Ready state and cooldown for each held active (`ActiveReady[slot]`).

**Behaviors** implement `IAiBehavior` and return an `AiCommand`:

```csharp
public struct AiCommand
{
    public Vector2 MoveTarget;
    public bool RequestDash;
    public int AbilitySlot;     // -1 for none
}
```

| Behavior | Scores high when | Does |
|---|---|---|
| Pressure | Own RPM is high and opponent RPM is low | Closes distance and attacks |
| Exploit Element | Matchup is above 1 and the enemy is in range | Pushes the engagement |
| Bait | Opponent is about to dash or has just finished | Holds mid-range for the counter window |
| Strike | In range and opponent is in `Recover` | Winds up, then dashes |
| Use Active | An active is ready and the enemy is in range | Requests the slot |
| Retreat | Own RPM below 15%, with a 2 s cooldown | Brief inward spiral; re-engages instead of circling |

**Scoring** picks the highest score with a minimum commit time (about 0.6 s).

**Profiles.** `AiProfile` holds behavior weights, reaction delay, dash aggression, and random error. `retreatOrbit` sets the orbit direction (sign) and step scale (magnitude); an unserialized 0 falls back to 1. All six demo profiles use `retreatRpm: 0.15` and `retreatWeight: 0.6` so low-RPM drift is rare. Behaviors are referenced by ID. `Legacy` is kept during transition for comparison.

`BattleSnapshot` carries `ArenaCenter` and `ArenaRadius`; `GameManager` passes them to `EnemyInput` on spawn. Retreat clamps its target inside `ArenaRadius - 0.8` and skips random jitter. Tangential speed drops and inward pull rises as RPM falls, so a dying bey spirals toward the center and stops instead of pinning itself in a corner or pacing the ring. A 2 s cooldown prevents chained retreats.

**Randomness** uses `IRandom`, seeded in tests (M4).

**v4 note.** The target for the enemy is injected by the spawner/battle controller; `GameObject.FindWithTag("Player")` is removed (M9).

---

## 17. Battle Flow

Arena geometry, art, camera framing, and physics boundary are documented in
`arena system.md`. Ring-out is designed but **not implemented**; the future-slice
spec lives in `ring out feature.md`.

`BattleController` runs `Countdown → Fighting → Finished(result)` and evaluates the `IWinCondition` list each tick.

- `RpmDepletedCondition` reproduces current behavior.
- `RingOutCondition` is a Phase 11 feature.
- `GameManager` keeps wave orchestration. Win and lose logic moves to `BattleController`.

### 17.1 Migration Map (current → new)

| Current type | New home | Notes |
|---|---|---|
| `BeyController` | `BeyRuntime` (hub) + `BeyPhysics` (unchanged role) | `BeyRuntime` owns stats, element stack, pending mods, and ability runner. |
| `BeyData` | `BeyRuntime.Rpm` | Keep `BeyData` during Phase 0–4 as the RPM holder; fold into `BeyRuntime` in Phase 5. |
| `BeyCombat` | `BattleCombat` (one per battle) + `ImpactSolver` | Pair dedupe replaces `IsCollisionOwner`. |
| `BeyBaseSO` | `BeyStats` + `BeyTypeDefinition` + `PartDefinition` | Movement fields move to `BeyStats.movement`; dead fields deleted. |
| `PlayerBeyInput` | `PlayerBeyInput` + `IBattleStateGate` | Gating moves out of the input component. |
| `EnemyInput` | `EnemyInput` (thin adapter) + `EnemyBrain` | Sensors, behaviors, `AiProfile`. |
| `GameManager` | `GameManager` (waves) + `BattleController` (flow) | Win/lose leaves `GameManager`. |
| `ServiceLocator` (combat use) | `BattleEvents` subscribers | SFX/shake/VFX are presentation subscribers. |
| `BeyUI` | `BeyUI` + `AbilityHud` | Reads events/snapshot, never combat internals. |
| `BeyVisual` | `BeyVisual` | Reads `BeyData.Base.maxRPM` (M8 fix). |

### 17.2 Kept as-is

- `ServiceLocator` stays for scene services and UI. Only combat/AI stop using it.
- `EnemySpawner` keeps instantiating prefabs; it gains a `BeyBlueprint` reference per spawn.
- `BeyVisualRandomizer` and palette logic are untouched.
- Legacy `Input` API stays (project rule).

---

## 18. Extension Model

### 18.1 Extension Points

| Extension | How | Code needed? | Owner |
|---|---|---|---|
| New bey | `BeyBlueprint` asset | No | Designer |
| New part | `PartDefinition` asset | No | Designer |
| New element | `ElementDefinition` asset, plus matchup entries if it's in the ring | No | Designer |
| New type | `BeyTypeDefinition` asset | No, unless the innate needs a new effect | Designer |
| New part slot | `PartSlotDefinition` asset with a unique priority | No | Designer |
| New ability from existing parts | `AbilityDefinition` asset | No | Designer |
| New scope tag | Add it to ability `tags` and to modifiers that use it | No | Designer |
| New condition | Class implementing `IAbilityCondition` | Yes | Programmer |
| New effect | Class implementing `IAbilityEffect` | Yes | Programmer |
| New trigger | `TriggerId` value, plus one router mapping | Yes | Programmer |
| New stat | `StatId` value, and consumers read it through `Get` | Yes | Programmer |
| New ability source | Source enum value and its limits in `CombatTuning` | Yes | Programmer |
| New impact rule | Class implementing `IImpactRule` with an `Order` | Yes | Programmer |
| New AI behavior | Class implementing `IAiBehavior`, registered by ID | Yes | Programmer |
| New win condition | Class implementing `IWinCondition` | Yes | Programmer |
| New presentation reaction | Subscribe to events | Yes | Either |

### 18.2 Design Rules That Keep It Modular

1. Nothing writes a stat or an element directly. Everything goes through `StatBlock` or `ElementStack`.
2. Impacts are resolved in the solver and applied in `BattleCombat` only.
3. Content references content by ID through the registry. Never by scene reference.
4. Systems talk through events and interfaces. A part never calls a matchup rule. It adds a stat or a claim.
5. Every tunable number lives in an asset.
6. Every modifier, claim, and impact is explainable.
7. Removing content removes its effects, because everything is keyed by source.

### 18.3 Recipes

- **New part:** add a `PartDefinition` with its stats, claim, granted abilities, and budget. Add one validation test. No code.
- **New element:** add an `ElementDefinition`. If it joins the ring, add its matchup entries. Run validation. No code.
- **New part slot:** add a `PartSlotDefinition` with a unique priority. Run validation. No code.
- **New ability from existing parts:** add an `AbilityDefinition` and compose triggers, conditions, and effects. No code.
- **New ability needing new behavior:** add a `[Serializable]` condition or effect class. The runner doesn't change.
- **New trigger:** add a `TriggerId` value and map one battle event to it in the router.
- **New stat:** add a `StatId`, have its consumer read it through `Get`, and add an `Explain` test.
- **New scope tag:** add it to the abilities that carry it, then reference it from modifiers. The stat system needs no change.
- **New impact rule:** implement `IImpactRule`, choose an `Order`, register it, add an EditMode test.
- **New AI behavior:** implement `IAiBehavior`, register it by ID, add it to a profile.
- **New win condition:** implement `IWinCondition` and add it to the battle configuration.

---

## 19. Phased Plan

Four tracks. Each phase is one reviewable branch. Don't start a phase until the previous exit criteria pass.

### Track A: Foundation

**Phase 0: Safety net (no gameplay change)**
- Tag the current revision (`63a0a0e`).
- Add `IRandom`. Use a seeded implementation in tests (M4).
- Reuse the existing `Assets/Tests` EditMode/PlayMode asmdefs.
- Capture golden output from a sandbox scene with scripted two-bey collisions at fixed speeds and RPMs. Log power, damage, knockback, and RPM after each impact.
- **Exit:** the golden file reproduces byte for byte across two runs.
- **Status: complete.** Tag `battle-rework-baseline`; `IRandom`/`SeededRandom`/`UnityRandom` in `Core/`; `EnemyInput` takes an optional seed (0 = unchanged `UnityRandom`). Golden: `Assets/Tests/PlayMode/BattleGoldenTests.cs` (equal, opposite-spin, charged-both scenarios), identical across two runs. EditMode 8/8, PlayMode 3/3.

**Phase 1: Bug fixes in place**
- B1: pair dedupe (§14.5). B2: each side spends its own pending modifiers. B4: guard `GetContact`. B5: delete the dead `GameManager.Update`. M3: input gating through `IBattleStateGate`. M5: remove `opposite`, unused `using`, dead initializer. M6: remove the `_currentRPM` mirror. M8: `BeyVisual` reads `BeyData.Base.maxRPM`.
- **Removed from v3 scope:** B3 (retired) and the double-subscription item (retired).
- **Exit:** the golden diff contains only the intended charge change. Recorded in §23.
- **Status: complete.** B1 now compares the two `BeyData` instance IDs symmetrically (the `BattleCombat` pair key lands in Phase 5). B2 consumes both sides' charges. B4 guards zero-contact impacts. B5 deleted. M3 gated through `IBattleStateGate` (GameUIManager registers it). M5/M6/M8 cleaned; `BeyVisual` guards missing data/pivot. Golden diff: charged row only, `560/320 → 560/560`.

**Phase 2: Content foundation and tuning (no gameplay change)**
- Create `ContentDefinition`, `ContentRegistry`, `ContentManifest`, `BeyStats` (including `movement`), `BeyTypeDefinition` (empty profile), `ElementDefinition` with Neutral flagged as index 0, an empty `MatchupTable`, `CombatTuning`, and `AiProfile`.
- Move every literal from §9.5 into these assets. Include the granted and equipped limits, the part budget cap, the aura tick interval, and the pending mod cap. Delete `dashStrength` and `minDashMultiplier`.
- **Exit:** no tuning literals in combat or AI code. Golden unchanged.
- **Status: complete.** Added `ContentDefinition`, `ContentManifest`, `ContentRegistry`, `CombatTuning`, `AiProfile`, `ElementDefinition`, `MatchupTable`, `BeyTypeDefinition`. Renamed `BeyBaseSO` → `BeyStats` (file + meta GUID preserved) and deleted `dashStrength`/`minDashMultiplier`. `BeyCombat` and `EnemyInput` read tuning from assets; tests use identical lazy defaults. Assets wired through `ContentManifest` (BattleScene) and `AiProfile` (Enemy prefab). Golden unchanged.

**Phase 3: Modifier core (no gameplay change)**
- `StatId`, `StatBlock` with scopes, stacking policies, `Tick`, `RemoveBySource`, and `Explain`.
- `ElementClaim` and `ElementStack`, with the Neutral base claim and `Explain`.
- Route attack and weight through `StatBlock`. Add defense at 0. Route the element through `ElementStack` with only the Neutral base claim.
- **Exit:** EditMode tests cover the formula, order independence, scope rules, stacking, expiry, claim priority, and claim removal. Golden unchanged.
- **Status: complete.** `StatBlock` (base + Add/PercentAdd/Multiply, scoped ability stats, Refresh/Stack/Ignore, `Tick`, `RemoveBySource`, `Explain`) and `ElementStack` (`ElementClaim`, priority/sequence resolution, expiry, `Explain`). Attack and weight now resolve through `StatBlock`; defense is seeded at 0. `BeyStats.baseElement` installs the Neutral base claim in `BeyData.Elements`. EditMode 26/26 (14 new), PlayMode 4/4, golden unchanged.

**Phase 4: Pure solver and impact rules**
- `ImpactInput`, `ImpactResult`, `ImpactSolver`.
- Rules: `BaseHitPower`, `MatchupRule` (neutral, so 1.0 everywhere), `SpinRule`, `PendingImpactRule`.
- Mitigation at defense 0 (no change). Knockback uses the contact normal and is clamped.
- Apply the grind decision (§14.4), using normalized RPM (D7).
- **Exit:** EditMode tests pass. The golden diff contains only documented changes.
- **Status: complete.** `ImpactInput`/`ImpactResult`/`ImpactSolver` with ordered rules (`BaseHitPower` 100, `Matchup` 200, `Spin` 300, `PendingImpact` 400). Final stages add defense mitigation (`K/(K+Defense)`), contact-normal knockback clamped to `maxKnockback`, and intensity. `GrindSolver` implements Option B (own normalized RPM) with Option A retained; `CombatTuning.grindMode = 1`. Charge is expressed as a `PendingImpactMod` with source `charge`. Golden diff: knockback clamp only — damages unchanged, velocities now `±12`.

**Phase 5: Events and presentation**
- `BattleEvents` for `ImpactPending`, `ImpactResolved`, `DashStarted`, `DashEnded`, `RpmChanged`, `AbilityActivated`, `CastComplete`.
- Move SFX, shake, and VFX to subscribers. Remove `ServiceLocator` from combat.
- **Exit:** combat tests run with no presentation attached.
- **Status: complete.** `BattleEvents` + `ImpactEvent` (attacker/defender/result/point/shake). `SFXPlayer`, `CameraShake`, and `HitEffectSpawner` subscribe in `OnEnable` and unsubscribe in `OnDisable`; `BeyData` raises `RpmChanged`, `BeyPhysics` raises `DashStarted`/`DashEnded`, and `BeyCombat` raises `ImpactPending`/`ImpactResolved`. No `ServiceLocator` reference remains in combat. Golden unchanged.

### Track B: Systems

**Phase 6: Types and elements**
- **6a:** Infrastructure with neutral defaults. Empty type profiles, empty matrix. Golden unchanged.
- **6a status: complete.** `BeyTypeDefinition.ApplyTo` installs `type:<id>` modifiers; identity `Atk`/`Def`/`Bal` assets exist (unassigned). Multiplicative baselines seeded (`RpmDecay`, `MatchupScale`, `TipFriction` = 1). Golden unchanged.
- **6b:** Populate the type profiles (§10), innate passives, the ring (§11.2), and the matchup values. Golden changes, documented.
- **6b status: complete.** Type profiles populated with innate passives; Surge Strike, Hard Shell, and Steady Core shipped as ability assets. Five ring elements + 10 matchup entries added; validator reports a clean manifest. Player = BAL/Crye, enemy = ATK/Pyre. The synthetic golden harness is asset-independent and stays unchanged; real scene beys now use types/elements. The 18×18 matrix runs (§20.4) with innates and per-exchange variance, and **all 18 reference builds are inside the 40–60% band** after tuning BAL to ×1.065 and DEF to ×0.81 attack. Hard Shell and Steady Core now affect stats because `StatBlock` normalizes empty scopes and `MatchupRule` applies `MatchupScale`.
- Validator rules for Neutral (§11.3) and matchup entries (§9.4).
- **Exit (6b):** the matchup and type simulations run (§20.4). Every reference build is inside the balance band. *(Met.)*

**Phase 7: Ability system**
- `AbilityDefinition` with tags and mode. Conditions and effects, including `ClaimElement`.
- `TriggerRouter`, `AbilityRunner` per bey, and the active lifecycle with scoped cost, cooldown, and power.
- Aura mode with per-tick evaluation and source-based cleanup.
- Ability sources and limits (§12.7), enforced for equipped and innate abilities.
- Migrate the charge call sites to `ScaleNextImpact`. Golden unchanged for the migrated path.
- Implement the equipped and innate examples from §12.8.
- **Exit:** ability tests pass, including aura apply and cleanup. Each equipped example works in the sandbox.
- **Status: complete.** Added `AbilityDefinition`, triggers, conditions, effects, `AbilityContext`, `IBeyActions`/`IBeyView`, and `AbilityRunner` (triggered passives, aura mode with source cleanup, active lifecycle with scoped cost/cooldown/power, source limits). Innate abilities install from `BeyTypeDefinition.innatePassiveId`; equipped abilities install from `BeyStats.equippedAbilities`. Player ability requests (`1/2/3`) flow through `IBeyInput.ConsumeAbilityRequest` and `BeyController`. All twelve §12.8 abilities are authored: Surge Strike, Hard Shell, Steady Core, Counter Spin, Ember Core, Last Spin, Charge Strike, Guard Spin, Ignition, Frost Guard, Ember Aura, and Burst Dash.

**Phase 8: Parts and blueprints**
- **8a: Parts as contributions.** `PartSlotDefinition` and `PartDefinition`. Stats, static element claims, and scoped skill tuning.
  - **Exit:** a part changes stats and the element. Removing it restores the exact previous values, with no residue.
- **8b: Granted skills, budget, assembly.** Granted abilities with their limits. The budget validator. `BeyBlueprint` and `BeyBuilder`, with full loadout validation across all three sources.
  - **Exit:** any bey can be assembled from assets. Invalid builds are rejected with clear messages. The reference parts (§13.5) work in the sandbox.
- **Status: complete.** Added `PartSlotDefinition`, `PartDefinition`, `StatModifierSpec`, `BeyBlueprint`, `BeyBuilder`, and `BeyBlueprintValidator`. Parts install stat/element contributions with `part:<id>` sources; granted abilities install with `grant:<part>/<ability>` and are removed with the part. Validation covers duplicate slots, budget, granted/equipped limits, duplicate ability ids, and active cost. Reference content added: 3 slots, 7 parts, and Frost Guard / Ember Aura / Burst Dash. 3 PlayMode part tests. A deterministic `BalanceSimulator` (seeded impact exchanges) and 2 tests are in. `BalanceCatalog` generates the 18 type × element reference builds and installs innate passives; `BalanceMatrix` runs the symmetrized round-robin and emits CSV; `PartSimulation` swaps each reference part into a control build, simulates granted actives, applies ring-out, and reports win-rate deltas. `Bey/Run Balance Matrix` and `Bey/Run Part Simulation` write `Logs/BalanceMatrix.csv` and `Logs/PartSimulation.csv`. All 18 builds pass the band and all 7 parts pass their slot band. `PlayerBlueprint` and `EnemyBlueprint` now drive the live player/enemy prefabs through `BeyBuilder`; the enemy carries the Pyre pair synergy and an equipped Ignition active. Golden unchanged.

### Track C: Intelligence and Flow

**Phase 9: Enemy brain**
- Sensors (§16), behaviors, scoring with commit time, immediate dash requests, `AiProfile`.
- Inject the target; remove `GameObject.FindWithTag` (M9).
- **Exit:** enemy dashes depend on the player's state and fire immediately. The enemy reacts to element changes, including aura flips. Profiles produce measurably different results in simulation.
- **Status: complete.** Added `BattleSnapshot`, `AiCommand`, `IAiBehavior`, five behaviors (Pressure, Exploit Element, Bait, Strike, Retreat), `EnemyBrain` with decision/commit timers, and expanded `AiProfile` weights. `EnemyInput` is now a sensor/adapter that dashes immediately on request; `BeyVisual` only spins the visual pivot. Target injection replaces `FindWithTag` (M9). 3 brain tests cover aggression, retreat, and seeded determinism. `UseActiveBehavior` requests an active slot when the opponent is below 45% RPM and a real active is ready; `BattleSnapshot` carries active readiness and cooldowns. `EnemyInput` throttles requests. `AiSimulation` runs headless profile comparisons. 6 PlayMode brain tests. Input producers now receive the battle-state gate from `BeyController` and no longer resolve `ServiceLocator` themselves.

**Phase 10: Battle flow**
- `BattleController`, `IWinCondition`, migration of `RpmDepletedCondition` from `GameManager`.
- **Exit:** outcomes come from conditions. `GameManager` only orchestrates waves.
- **Status: core complete.** Added `BattleContext`, `IWinCondition`, `RpmDepletedCondition`, `EnemiesClearedCondition`, and `BattleController`. `GameManager` now builds the context and resolves outcomes through conditions; it retains wave spawning/destruction only. 4 EditMode tests cover lose/win conditions and controller ordering. `RingOutCondition` remains a Phase 11 feature.

### Track D: Features

**Phase 11: Feature backlog (each independent)**
- Rules: `SameSpinRule`, `SpinStealRule`, `DashHitRule`, `ClashRule`.
- Conditions: `RingOutCondition` with a stadium node.
- Burst finish as a `FinishReason` value.
- Element-typed damage (optional).
- Part synergies (optional). Pairs of parts that unlock a bonus.
- **Exit per feature:** adds new files only. The solver, `BattleCombat`, and the existing rules are unchanged.
- **Status: partial - seven features landed.** `SameSpinRule` (tuning default neutral), `ClashRule`, `DashHitRule` (contact flags from `BeyPhysics.IsDashing`), `SpinStealRule` (opposite-spin RPM gain), `RingOutCondition` with an arena radius, `FinishReason` plumbing (`RpmDepleted`/`RingOut`/`Burst`), and a `BurstCondition` triggered when one impact deals at least 80% of the target's max RPM (raised from 50% after dash-clash one-shots). Element-typed damage is implemented as §11.7 on-hit statuses, and part synergies are implemented in §13.8. Both are in the status-aware balance matrix.

---

## 20. Testing and Tooling

### 20.1 EditMode

- **StatBlock:** formula, order independence, scope rules, each stacking policy, expiry through `Tick`, `RemoveBySource` leaves no residue, `Explain` matches `Get`.
- **ElementStack:** highest priority wins, releasing a claim restores the next, the Neutral base is the floor, ties go to the newest claim and log a warning.
- **Neutral:** ×1.0 in both directions. The validator rejects matchup entries involving Neutral.
- **Matchups:** every ordered pair resolves to exactly one value (listed, or 1.0 if not listed). The `min` scaling rule (§11.4) holds.
- **Solver:** opposite spin symmetry, zero speed gives zero damage, pending mods are spent only on commit, knockback decreases with weight and is monotonic in RPM, mitigation decreases as defense rises.
- **Triggers:** each `TriggerId` reaches only the abilities subscribed to it.
- **Abilities:** conditions and effects run in order. Cost and cooldown use the scoped stats. Interrupts go to cooldown. Passives install and clean up. Aura applies on entry, removes everything on exit, and doesn't double-apply on re-entry.
- **Parts:** removing a part restores the exact previous stat and element values. Slot priorities are unique. The budget cap and the granted limits are enforced.
- **Blueprints:** duplicate IDs, unknown references, over-limit loadouts, and multiple parts in one slot are rejected.
- **AI:** commit time respected, deterministic under a seed.

### 20.2 PlayMode

- Sandbox scene with two beys at fixed positions and a fixed seed.
- One physical contact produces exactly one `ImpactResolved` event.
- A neutral Blade blocks a Pyre Bit's element. Replacing the Blade changes the matchup on the next impact.
- A passive changes the outcome of a fixed scenario in the expected direction.
- Existing coverage stays green: input gating while paused, malformed bey init, ServiceLocator replacement semantics.

### 20.3 Debug Tooling

**Status: implemented.** `Bey/Battle Debug` opens an editor window with the build inspector, element claims, stat contributions, abilities, current matchup, and a 30-entry impact trace. `Bey/Add Debug Overlay` adds a runtime overlay that shows both beys' RPM, element, abilities, and the matchup.

- **Build inspector:** for any bey, shows its parts, the element claim stack with priorities and sources, its innate, granted, and equipped abilities with their sources, and each stat's contributions.
- **Impact trace:** for each impact, shows each rule's effect on outgoing power and on damage. First place to look when a fight feels wrong.
- **Ability HUD:** state, cooldown, and pending modifiers for each ability on screen.
- **Matchup overlay:** the current element matchup between the two beys in the scene.
- **Content validator:** runs from the editor menu and at battle load (§9.4).

### 20.4 Balance Simulation

**Reference builds.** Three types × six elements = **18 reference builds**. Current reference builds use **no parts** so the matrix isolates type × element. Round-robin is 18 × 18 = 324 pairings, with **25 seeds in each direction**; each cell symmetrizes the two runs so row and column rates agree.

**Tooling.** `BalanceCatalog.CreateReferenceBuilds()` builds the 18 references from the content manifest, applies each type's innate passive, and stores the innate for dash handling. `BalanceMatrix.Run(...)` fills the win/loss/draw matrices through `BalanceSimulator`, `WinRate(i)` aggregates each build, `BuildsOutsideBand(min, max)` lists offenders, and `ToCsv()` emits the matrix. Run `Bey/Run Balance Matrix` from the editor; the CSV lands at `Logs/BalanceMatrix.csv` (gitignored).

**Simulation model.** Each duel fixes the spin relationship for all exchanges, then rolls per-exchange dash states and a 0.85–1.15 contact-quality multiplier per side. Dashes set `DashingA/B`, which activates the clash and dash-hit rules; ATK's Surge Strike adds its pending 1.15 impact modifier after a dash. Innate `AddModifierEffect`s (Hard Shell's +10% defense, Steady Core's ×0.6 matchup scale) are installed onto the build's stats. `MatchupRule` now applies `MatchupScale`, so Steady Core softens both advantages and disadvantages. Per-duel stat clones apply passive RPM decay and the element on-hit statuses after each impact, so the matrix covers the live status layer.

**Band.** Every reference build between 40% and 60% overall.

**Reference run (25 seeds per direction, no parts).** All 18 builds pass the band with statuses on: ATK 46.4–55.3%, BAL 42.5–57.5%, DEF 41.4–59.5%. Tuned type attacks: ATK ×1.25, BAL ×1.065, DEF ×0.81. The matrix stays exactly antisymmetric. These figures predate the stronger element pass; the suite was re-run after it and all 18 builds still pass the band.

**Part simulation.** `PartSimulation.Run(...)` swaps each part into the `bal_neutral` control, applies its stat modifiers, element claim, granted `AddModifierEffect`/`ClaimElementEffect` values, and grants any active abilities. `BalanceActives` now simulates those actives: RPM cost, cooldown, `ForceDash`, `ScaleNextImpact`, and `DrainRpm`, with `AbilityCost`, `AbilityCooldown`, and scoped `AbilityPower` from the build's stats. `Bey/Run Part Simulation` writes `Logs/PartSimulation.csv`. Slot-band check: every part's delta within ±5 points of its slot average.

**Part run with actives, statuses, and ring-out (25 seeds, start variance 0.2, arena radius 8).** Control 48.7%. Heavy Ratchet +51.1, Crye Ratchet +47.0, Pyre Blade +27.7, Neutral Blade +20.8, Grip Bit +3.9, Ember Bit +2.4, Burst Bit 0.0. All seven pass the ±5 slot band.

**Mobility and ring-out model.** `BalanceSimulator` accepts an arena radius for the part run. It starts the beys at ±25% of the radius, applies forced-dash approach/push, converts impact impulses into displacement, and ends the duel when either bey leaves the radius. The 18-build reference matrix keeps the radius at zero.

**Adjusted from playtest.** Player and enemy `maxRPM` normalized, first to 3000 (the earlier 5200/600 split made damage-to-health feel inconsistent), then raised to 5000 to lengthen fights, and finally the **player alone** raised to **9000** for an 80% easier demo while enemy HP and time-to-kill stay at 5000. Damage per hit does not scale with max RPM. At 9000 the player's 80% burst threshold is 7200, so burst is effectively disabled; the enemy threshold stays 4000.

**Part exit criteria met.** Every part sits inside the Bit/Blade/Ratchet slot band, the 18-build reference matrix stays 40–60%, and the golden row is unchanged. `budgetCost` remains a design estimate; the simulation does not price it directly.

Simulation drives numbers. Playtests decide whether fights feel readable. Both are needed.

### 20.5 Game Feel Test (run after all features are online)

**Preconditions:** player and enemy prefabs assemble from `BeyBlueprint`; all 18 reference builds and 7 parts still pass their bands; `Bey/Battle Debug` is open. Demo slice wires `BattleScene` with six enemy blueprints — one per element (Neutral plus all five ring members) — a battle HUD, and a hidden title Continue button. Player RPM, elemental statuses, and position reset unconditionally just before the next enemy spawns, so every element matchup starts from the same state regardless of scene serialization.

**A. Core loop**

1. Mouse follow is smooth: no jitter, no overshoot, no stuck acceleration.
2. One click = one dash; RPM cost is visible; exactly one charged hit lands.
3. Every collision drops both RPM bars once. Four drops means double resolution.
4. First enemy dies in roughly 3–6 clean hits; one full wave stays under ~90s.
5. Pause and post-match clicks do not drain RPM or bank charged hits.

**B. Elements**

1. Pyre burn visibly speeds RPM loss; the bar does not flash every frame.
2. Crye chill makes enemy hits noticeably softer while it lasts.
3. Gyre gust produces visibly stronger knockback.
4. Hydre siphon slows the attacker's own decay; confirm in the debug stats.
5. Sparke surge makes the next hits harder and refreshes on re-hit; statuses expire after 3–4 s.

**C. Parts and synergies**

1. A two-piece same-element build shows `synergy:<element>` and +5% attack/defense.
2. Remove one part: the synergy and that part's stats disappear exactly.
3. Granted abilities install and remove with their part; no duplicate IDs.
4. An over-budget blueprint is rejected with a readable message.
5. Pyre Blade + Ember Bit counts as a Pyre pair; Ember Aura's element flip is visible.

**D. Abilities and AI**

1. Player 1/2/3 casts only equipped actives; RPM cost and cooldown match the asset.
2. Guard Spin, Charge Strike, Ignition, Frost Guard, Ember Aura, and Burst Dash each produce a visible effect in their intended situation.
3. Cast interrupts happen only when `interruptOnHit` is set.
4. Enemy dashes immediately, uses an active when the player is low, and retreats when its own RPM is low.
5. Aggressive and passive profiles produce measurably different dash rates in `AiSimulation`.

**E. Flow, UI, and feedback**

1. RPM depletion, ring-out, and burst each show the correct end text.
2. Enemy death waits ~1s, spawns the next enemy, and the final kill shows the win panel.
3. The debug overlay updates RPM, element, ability cooldowns, and matchup live.
4. The impact trace has exactly one entry per collision with plausible damage and impulse.
5. Hit SFX, camera shake, and particles fire once per impact; no double audio.

**Pass criteria:** no blocker (double damage, stuck input, missing end state); all five sections pass; feel notes recorded per section.

---

## 21. Open Questions

Defaults are in brackets. Each one should be answered before the phase that depends on it.

**Design decisions**

1. Element names and spelling, including Neutral. [Names as written.] *(Phase 6)*
2. Ring and matchup multipliers 1.35 and 0.75. [Applied; raised from 1.25/0.8 in the stronger-element pass.] *(Phase 6)*
3. Type profiles and innate passives (§10). [Placeholders.] *(Phase 6)*
4. Equipped loadout limits. [2 passive, 1 active.] *(Phase 7)*
5. Granted limits across all parts. [2 passive, 1 active.] *(Phase 8)*
6. Do granted skills count against the equipped loadout? [No.] *(Phase 8)*
7. Part slots: names and count. [Blade, Ratchet, Bit.] *(Phase 8)*
8. Slot element priorities. [Bit 10, Ratchet 20, Blade 30.] A Neutral claim from a higher slot overrides a lower slot's element. *(Phase 8)*
9. Can parts be swapped mid-battle? [No.] *(Phase 8)*
10. Part budget cap and costs. [Cap 10.] *(Phase 8)*
11. Should Neutral get a bonus of its own? [No. Use a conditional passive if needed (§11.3).] *(Phase 6)*
12. Can more than one active be held? [Yes. Only one can cast at a time.] *(Phase 7)*
13. Does an element change interrupt a cast? [No.] *(Phase 7)*
14. Refund on interrupted casts? [No.] *(Phase 7)*
15. Aura evaluation interval. [0.1 s.] *(Phase 7)*
16. Defense constant `K`. [40.] *(Phase 3)*
17. Pending impact cap. [3.] *(Phase 4)*
18. Grind: Option A or B (§14.4). **Resolved: Option B.** *(Phase 4)*

**Verify in code — resolved at `63a0a0e`**

19. ~~Does `OnImpact` fire on collision Enter, Stay, or both?~~ **Answered:** `OnImpact` on `OnCollisionEnter2D` only; `OnGrind` on `OnCollisionStay2D`.
20. ~~Where do dash/ability requests get read, `Update` or `FixedUpdate`?~~ **Answered:** requests are read in `BeyController.Update`; movement runs in `BeyPhysics.FixedUpdate`; the dash impulse is applied from `Update`.
21. ~~Where is `AddChargedHit` called from, and does a charge expire?~~ **Answered:** `BeyController.Update` after `Physics.Dash` returns true; charges never expire and decrement one per resolved impact.
22. ~~Range of `RPMNormalized`, whether `ReduceRPM` clamps at zero, and whether `OnRPMDepleted` fires once.~~ **Answered:** `0..1` (guarded when `Base` is missing), `SetRPM` clamps `[0, maxRPM]`, `OnRPMDepleted` fires once on the alive→zero transition.
23. Should `DrainRpm` go through `ReduceRPM` and emit `OnRPMDepleted`? [Yes.] *(Phase 4 — design, not code)*
24. ~~Type of `spinDirection` (enum or int).~~ **Answered:** `enum SpinDirection { Clockwise, CounterClockwise }` in `BeyBaseSO.cs`.

**Resolved in v4 (re-baseline)**

- B3 retired; `ConsumeDash()` retained as a design improvement.
- `?.` rule corrected: only unsafe on possibly-Destroyed `MonoBehaviour` references.
- Double-subscription item retired.
- Movement tuning stays outside `StatBlock` (§8.1).
- Asset loading uses an explicit `ContentManifest` (§9.1).

---

## 22. Definition of Done

- Every audit item is resolved, or explicitly deferred with a note in §4.
- No combat, AI, ability, or part file references `ServiceLocator`, UI, or scene objects directly.
- Every tuning value lives in an asset.
- Neutral behaves as specified (§11.3) and is rejected from the matchup table.
- A designer can create a new part, an element, and a bey with a granted aura, without code. The build passes validation and the balance simulation.
- Removing a part, type, or aura leaves no stat or element residue. A test verifies this.
- Every stat, element decision, and impact can be explained in the debug tools.
- The golden test passes, or every diff is recorded in §23.
- The existing test suite (EditMode + PlayMode) stays green at every phase boundary.

---

## 23. Changelog of Intended Behavior Changes

Fill in as each phase lands.

| Phase | Change | Reason |
|---|---|---|
| 1 | Each side spends its own pending modifiers | Fixes B2 |
| 1 | Collision ownership compares the two `BeyData` IDs symmetrically | Fixes B1 in place; `BattleCombat` pair key lands in Phase 5 |
| 1 | Input gating resolves `IBattleStateGate` instead of `GameUIManager` | Fixes M3 |
| 1 | Visual spin reads `BeyData.Base.maxRPM` | Fixes M8 |
| 7 | Dash and ability requests are edge-triggered | Arrives with `ConsumeDash()`/`ConsumeAbilityRequest` |
| 2 | Neutral becomes element index 0 | §11.3 |
| 3 | Element resolves through claims, with Neutral as the floor. Golden unchanged. | §8.5 |
| 4 | Knockback uses the contact normal and is clamped | Fixes D3 |
| 4 | Grind uses Option B (own normalized RPM) | Fixes D1, D7 |
| 6b | Type profiles, element ring, and neutral rules take effect | §10, §11 |
| 7 | Auras, scoped skill stats, ability sources. Charge Strike replaces charges. | §12 |
| 8a | Parts take effect: stats and element claims | §13 |
| 8b | Granted skills take effect. Budget cap enforced. | §12.7, §13.6 |
| 8b | 18×18 balance matrix tooling: catalog, matrix, CSV, band report | §20.4 |
| 8b | Simulator installs innates, models dashes/contact variance, symmetrizes pairings; all 18 builds inside 40–60% | §20.4 |
| 8b | Part simulation: per-part swap, slot-band report, CSV | §20.4 |
| 8b | Active abilities simulated: cost, cooldown, forced dash, next-impact scaling | §20.4 |
| 8b | Deferred: position/ring-out model to value mobility and Burst Dash | §20.4 |
| 8b | Mobility/ring-out model in part sim; all seven parts pass the slot band | §20.4 |
| 11 | Burst condition: one impact ≥ 80% of max RPM (raised from 50%) | §17 |
| 7 | Counter Spin, Ember Core, Last Spin, Ignition authored | §12.8 |
| 9 | AI Use Active behavior and headless profile simulation | §16 |
| 20 | Battle Debug window, impact trace, overlay | §20.3 |
| 6b | Live RPM decay (`BeyStats.rpmDecayRate`) and Tip Friction knockback resistance | §10 |
| 8b | Grip Bit tip friction tuned to ×1.05 after the live friction change | §13.5 |
| 11 | Element on-hit statuses: Pyre burn, Crye chill, Gyre gust, Hydre siphon, Sparke surge | §11.7 |
| 6b | Stronger element pass: matchups 1.35/0.75, statuses 3–4 s at ×2.5/×0.9/×0.9/×0.9/×1.08 | §11 |
| 20 | Demo difficulty: player maxRPM 9000, enemy stays 5000; ring-out spec written as a future slice (`ring out feature.md`) | §20.4 |
| 20 | Remove enemy dash wind-up and its punch-scale telegraph; pool hit particles (8 prewarm, 32 cap) | §15, §20.3 |
| 8b | Same-element 2-piece part synergy: +5% attack and defense | §13.8 |
| 8b | Player/enemy prefabs assemble from `BeyBlueprint` through `BeyBuilder` | §13.3 |
| 9 | Snapshot carries active readiness/cooldowns; input gate injected instead of resolved per input | §16 |
| 20 | Balance simulator applies passive decay and element statuses on per-duel stat clones | §20.4 |
| 20 | Battle demo slice: six enemy blueprints, AI profiles, HUD, six-wave scene, wave checkpoint reset | §20.5 |
| 6b | Type attack tuned: BAL ×1.065, DEF ×0.81 | §10 |
| 8b | Playtest: maxRPM normalized to 3000, both raised to 5000, then player alone to 9000 for an easier demo | §20.4 |
| 9 | Enemy dash depends on player state, fires immediately, and reads the current element | Fixes D4, D5, M9 |
| 20 | Wave reset always runs at next spawn; drop the unserialized toggle that disabled it | Fix wave reset |
| 9 | Retreat is a short inward spiral with a 2 s cooldown; orbit 0 falls back to 1; jitter skipped | §16 |
| 11 | Each feature recorded as it lands | Phase 11 |
| 11 | Same-spin, clash, and dash-hit multipliers added (same-spin neutral by default) | §14.1, §15 |
| 11 | Opposite spin steals RPM | §14.1 |
| 11 | Finish reasons: RPM depleted, ring-out, burst | §17 |
| 11 | Ring-out lose condition with arena radius | §17 |

**v4 re-baseline note (no runtime change):** audit re-run at `63a0a0e`; B3 and the double-subscription item retired; M3 widened to both input producers; M5 corrected; B5, D7, M6, M7, M8, M9 added; Phase 1 scope updated; Open Questions 19–22 and 24 answered from code.
