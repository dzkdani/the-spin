# Elemental System

Reference for the-spin's element roster, matchup matrix, on-hit effects, claim
resolution, and tuning. Reflects HEAD `72083f7`. See also `battle system.md`.

Elements are **not** cosmetic. Two mechanics are live:

1. **Matchup multiplier** — outgoing damage is scaled by the attacker/defender
   element pair (1.25 strong, 0.8 weak, 1.0 otherwise).
2. **On-hit status** — a short stat modifier applied to the defender or the
   attacker after each resolved impact.

**Type is separate.** ATK / DEF / BAL modify attack, defense, weight, and decay.
Type currently has the larger gameplay effect; element is the matchup + status
layer.

---

## 1. Roster

| Element | Id | Neutral | Color | Asset |
|---|---|---|---|---|
| Neutral | `neutral` | yes | `#D9D9D9` (0.85, 0.85, 0.85) | `Assets/Game/Datas/Neutral.asset` |
| Pyre | `pyre` | no | `#EB522E` (0.92, 0.32, 0.18) | `Assets/Game/Datas/Pyre.asset` |
| Crye | `crye` | no | `#59B8F2` (0.35, 0.72, 0.95) | `Assets/Game/Datas/Crye.asset` |
| Gyre | `gyre` | no | `#4DCC8C` (0.30, 0.80, 0.55) | `Assets/Game/Datas/Gyre.asset` |
| Hydre | `hydre` | no | `#4D73E6` (0.30, 0.45, 0.90) | `Assets/Game/Datas/Hydre.asset` |
| Sparke | `sparke` | no | `#F2D140` (0.95, 0.82, 0.25) | `Assets/Game/Datas/Sparke.asset` |

The five non-neutral elements form a ring. Neutral sits outside the ring and is
always 1.0 in both directions.

---

## 2. Matchup matrix

Rows are the attacker, columns the defender. Values are the outgoing damage
multiplier.

| Attacker ↓ / Defender → | Neutral | Pyre | Crye | Gyre | Hydre | Sparke |
|---|---|---|---|---|---|---|
| **Neutral** | 1.00 | 1.00 | 1.00 | 1.00 | 1.00 | 1.00 |
| **Pyre** | 1.00 | 1.00 | **1.25** | 1.00 | **0.80** | 1.00 |
| **Crye** | 1.00 | **0.80** | 1.00 | **1.25** | 1.00 | 1.00 |
| **Gyre** | 1.00 | 1.00 | **0.80** | 1.00 | 1.00 | **1.25** |
| **Hydre** | 1.00 | **1.25** | 1.00 | 1.00 | 1.00 | **0.80** |
| **Sparke** | 1.00 | 1.00 | 1.00 | **0.80** | **1.25** | 1.00 |

Ring summary:

| Element | Strong vs (1.25×) | Weak vs (0.8×) |
|---|---|---|
| Pyre | Crye | Hydre |
| Crye | Gyre | Pyre |
| Gyre | Sparke | Crye |
| Sparke | Hydre | Gyre |
| Hydre | Pyre | Sparke |

Only these 10 directed pairs are authored. Every other pair is 1.00.

### Rules

- `MatchupResolver.Get` returns 1.0 when the table, attacker, or defender is
  missing, or when **either side is Neutral**.
- `MatchupRule` scales both directions independently:
  `PowerA *= matchup(A → B)`, `PowerB *= matchup(B → A)`.
- `StatId.MatchupScale` reshapes the deviation:
  `1 + (multiplier - 1) × scale`. Default `1`. A value of `0` disables the
  matchup for that bey; `0.5` halves it.
- `ContentValidator` rejects entries with a missing element, a self-pair, a
  Neutral side, or a duplicate attacker/defender pair.

### Where it is wired

- Table: `Assets/Game/Datas/MatchupTable.asset`
- Manifest: `Assets/Game/Datas/ContentManifest.asset` → `matchupTable`
- Rule: `Assets/Game/Scripts/Battle/Combat/MatchupRule.cs` (order 200)
- Lookup: `Assets/Game/Scripts/Battle/Ai/MatchupResolver.cs`
- HUD line: `BattleHUD.DrawMatchup` prints `from -> to xN.NN`

---

## 3. On-hit status

Applied after each resolved impact by `ElementStatusResolver.Apply`. Each side's
current element is applied to the other side (or to itself when
`onHitAffectsSelf` is set).

| Element | Target | Stat | Op | Value | Duration |
|---|---|---|---|---|---|
| Pyre | Defender | `RpmDecay` | Multiply | **1.80** | 2 s |
| Crye | Defender | `Attack` | Multiply | **0.975** | 2 s |
| Gyre | Defender | `Defense` | Multiply | **0.98** | 2 s |
| Hydre | Attacker (self) | `RpmDecay` | Multiply | **0.97** | 2 s |
| Sparke | Attacker (self) | `Attack` | Multiply | **1.02** | 2 s |
| Neutral | Defender | — | — | — | — |

Plain-language:

- **Pyre** — burn: the target loses RPM faster to passive decay.
- **Crye** — chill: the target's attacks are weaker.
- **Gyre** — gust: the target's defense is lower.
- **Hydre** — siphon: the attacker's own decay is lower.
- **Sparke** — surge: the attacker's own attack is higher.
- **Neutral** — no status. The asset currently holds `stat: -1, value: 0`, which
  is a no-op; treat it as a cleanup candidate.

### Stacking and lifetime

- Source id is `element:<id>`.
- `StackPolicy.Refresh`, `maxStacks = 1`: a re-hit refreshes the expiry; it does
  not add a second copy.
- `StatBlock.Tick(now)` removes expired modifiers.
- `GameManager.ResetPlayerForNextWave` removes every `element:` source between
  waves, so statuses do not carry into the next matchup.

### Where it is wired

- Data: `ElementDefinition.onHitModifiers`, `onHitAffectsSelf`,
  `onHitDuration` (`Assets/Game/ScriptableObjects/ElementDefinition.cs`)
- Applier: `Assets/Game/Scripts/Battle/Combat/ElementStatusResolver.cs`
- Call sites: `BeyCombat.ResolveImpact` (both directions), `BalanceSimulator`
- Debug: `BattleHUD.DrawStatuses` prints the live `element:` sources

---

## 4. Effective element resolution

A bey's element is the highest-priority live claim in its `ElementStack`, not
just the blueprint's base element.

| Source | Priority | Lifetime |
|---|---|---|
| Chassis base element (`base:chassis`) | 0 | Infinite |
| Bit part claim | 10 | Part claim duration |
| Ratchet part claim | 20 | Part claim duration |
| Blade part claim | 30 | Part claim duration |
| Ability claim | Ability-defined | Ability duration |

- Higher priority wins. Equal priority goes to the **newest** claim (higher
  sequence).
- Removing a claim reverts to the next live claim.
- Expired claims are ignored by `ElementStack.Current` and removed by `Tick`.
- Neutral is the floor: a bey with no live claim has no element and all
  matchups resolve to 1.0.

Files:

- `Assets/Game/Scripts/Battle/Elements/ElementStack.cs`
- `Assets/Game/Scripts/Battle/Elements/ElementClaim.cs`
- `Assets/Game/Scripts/Bey/BeyData.cs` (`ApplyIdentity` adds the base claim)
- `Assets/Game/Scripts/Battle/Parts/BeyBuilder.cs` (`ApplyPart` adds part claims)
- `Assets/Game/Scripts/Battle/Abilities/AbilityRunner.cs` (`ClaimElement`)

Part slot assets carry `elementPriority`: Bit 10, Ratchet 20, Blade 30.

---

## 5. Where element enters a hit

```text
ImpactInput
  ElementA, ElementB, Matchups
        ↓
BaseHitPowerRule      speed × attack × Lerp(minRpmPower, maxRpmPower, RPM)
        ↓
MatchupRule           PowerA ×= matchup(A → B); PowerB ×= matchup(B → A)
        ↓
SpinRule / SameSpinRule / SpinStealRule / PendingImpactRule
        ↓
ClashRule / DashHitRule
        ↓
damage = power × damageMultiplier
        ↓
defense mitigation: damage × K / (K + defense)
        ↓
ReduceRPM on both sides
        ↓
ElementStatusResolver.Apply(A) and Apply(B)
```

So element affects both the **immediate hit** (matchup) and the **next ~2 s of
stats** (status).

Relevant files:

- `Assets/Game/Scripts/Battle/Combat/ImpactSolver.cs`
- `Assets/Game/Scripts/Battle/Combat/MatchupRule.cs`
- `Assets/Game/Scripts/Bey/BeyCombat.cs`
- `Assets/Game/Scripts/Battle/Combat/ElementStatusResolver.cs`

---

## 6. Demo coverage

Player: **Crye**, type **BAL**.

| Wave | Enemy | Enemy element | Player deals | Player takes | Status applied to player |
|---|---|---|---|---|---|
| 1 | Warmup | Neutral | 1.00 | 1.00 | none |
| 2 | Ember Press | Pyre | **0.80** | **1.25** | `RpmDecay ×1.8` |
| 3 | Tidal Wall | Hydre | 1.00 | 1.00 | none (Hydre buffs the enemy) |
| 4 | Frost Counter | Crye | 1.00 | 1.00 | `Attack ×0.975` |
| 5 | Final Surge | Sparke | 1.00 | 1.00 | none (Sparke buffs the enemy) |
| 6 | Vortex Spin | Gyre | **1.25** | **0.80** | `Defense ×0.98` |

The player's own Crye status lands on every enemy the player hits:
`Attack ×0.975` for 2 s.

### Current feel

The matchup swing is visible on waves 2 and 6. The statuses are numerically
small on a 5000 RPM pool:

- Pyre burn ≈ **12 extra RPM** per proc (base decay 7.5 RPM/s × 1.8 for 2 s).
- Crye chill ≈ **2.5%** weaker attacks for 2 s.
- Gyre gust ≈ **2%** less defense for 2 s.
- Hydre siphon ≈ **3%** less passive decay for 2 s.
- Sparke surge ≈ **2%** more attack for 2 s.

That is why element currently reads as a tag in normal play.

---

## 7. Tuning knobs

| Knob | Location | Effect |
|---|---|---|
| `onHitModifiers` | element asset | stat, op, value of the status |
| `onHitAffectsSelf` | element asset | apply to attacker instead of defender |
| `onHitDuration` | element asset | status lifetime in seconds |
| `entries[].multiplier` | `MatchupTable.asset` | strong/weak values |
| `MatchupScale` | `StatId` modifiers | per-bey scaling of matchup deviation |
| `damageMultiplier` | `CombatTuning.asset` | global damage scale |

### Suggested stronger pass (not implemented)

| Element | Proposed |
|---|---|
| Pyre | `RpmDecay ×2.5`, 4 s |
| Crye | `Attack ×0.90`, 3 s |
| Gyre | `Defense ×0.90`, 3 s |
| Hydre | `RpmDecay ×0.90`, 3 s |
| Sparke | `Attack ×1.08`, 3 s |
| Matchups | 1.35 strong / 0.75 weak |

Re-run the status-aware balance matrix after any change; the 18 reference builds
are tuned around the current values.

---

## 8. Validation and tests

- `ContentValidator.ValidateMatchups` — missing element, self-pair, Neutral,
  duplicates.
- `Assets/Tests/EditMode/ElementStackTests.cs` — priority, newest-wins,
  revert-on-remove, expiry, explain order.
- `Assets/Tests/EditMode/ElementStatusResolverTests.cs` — opponent status
  applied and expires; self status targets the attacker.
- `Assets/Tests/EditMode/ContentAssetsTests.cs` — player and enemy blueprint
  elements.
- `Assets/Tests/EditMode/BalanceMatrixTests.cs` — status-aware 18-build matrix.
- `Assets/Tests/EditMode/BalanceSimulatorTests.cs` — simulator behavior.

---

## 9. Authoring a new element

1. Create an `ElementDefinition` asset under `Assets/Game/Datas/`.
2. Set `id`, `displayName`, `color`, `isNeutral = false`.
3. Add `onHitModifiers` (or leave empty for a pure matchup element).
4. Add it to `ContentManifest.assets`.
5. Add matchup entries: one strong and one weak against the intended ring
   neighbours. No Neutral, no self-pairs.
6. Reference it from a blueprint's `baseElement`, a part's `elementClaim`, or an
   ability's `ClaimElement`.
7. Run EditMode tests; the validator and balance matrix will flag omissions.

---

## 10. Known gaps

- Statuses are tuned low enough to be nearly invisible.
- Only ring strong/weak pairs differ; 3 of 5 opponents are neutral for any given
  element.
- Neutral has no bonus and a no-op status asset.
- No element-specific hit VFX; feedback is the HUD `STATUS` line and bar decay.
- Type (ATK/DEF/BAL) currently dominates element in practice.
