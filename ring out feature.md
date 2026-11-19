# Ring-Out Feature — Future Slice

**Status: design only. Not implemented. Priority 1 of the future-slice queue**,
followed by `wave ramp feature.md` and `arena art feature.md`.

`BattleContext` and `RingOutCondition`
already use elliptical math, but `GameManager.Start` deliberately does **not**
register `RingOutCondition`, so no ring-out occurs. `ResolveBattle` still runs only
on RPM depletion, burst, or enemy defeat.

The arena itself has been reworked to an ellipse; see `arena system.md`.

Companion docs: `battle system.md` §17, `elemental system.md`.

---

## Goal

A spatial lose/win condition tied to the visible arena boundary. A bey pushed
outside the ring loses immediately, regardless of remaining RPM. In beyblade
terms: getting knocked out of the stadium.

---

## Boundary

The arena is an **ellipse** centered at `(0, 0)` with radii **8.2 × 4.4** world
units. `ArenaBounds` is the single source of truth; see `arena system.md` for the
placeholder art, camera framing, and physics wall.

Crossing the outer white ring means crossing normalized distance 1:

```text
d = sqrt((x / 8.2)² + (y / 4.4)²)
outside = d > 1
```

---

## Rules

1. Player normalized distance from `ArenaBounds.Center` > 1 → **Lose**,
   `FinishReason.RingOut`.
2. Enemy normalized distance > 1 → **Win**; counts as that wave defeated and
   advances to the next spawn (or shows the win panel on the last wave).
3. Both out on the same tick → **player loses** (defender priority).
4. Evaluate on a light tick — every **0.05–0.1 s** in `GameManager`, not only on
   combat events.
5. Do not zero RPM on ring-out. The finish reason carries the result.
6. No ring-out while paused (`Time.timeScale == 0`) or after the match ends.

### Resolve priority

When several conditions land together:

```text
Burst → Ring-out → RPM depleted → Enemies cleared
```

---

## Blockers to fix before implementation

### 1. Arena fields

Resolved by the arena rework: `ArenaBounds` holds center and radii, and
`GameManager` holds a serialized reference to it. No unserialized scalar radius
remains. `BattleContext` carries `ArenaRadii`.

### 2. Spawn points

Resolved: all four spawn points now sit at `(±5.2, ±2.6)`, normalized distance
≈ 0.77, safely inside the ellipse.

### 3. Player reset position

`ResetPlayerForNextWave` already puts the player at `(0,0)` and zeroes velocity,
so wave starts are safe. Keep it that way.

---

## Code points

| File | State / change |
|---|---|
| `ArenaBounds.cs` | Done. Ellipse math, `NormalizedDistance`, `Clamp`, edge wall. |
| `BattleContext.cs` | Done. Carries `ArenaRadii`. Add `EnemyPosition` in the slice. |
| `RingOutCondition.cs` | Done (player lose, ellipse math). Add an enemy-win variant. |
| `GameManager.cs` | Add a `RingOutTick()` accumulator in `Update`; gate on match state and `Time.timeScale`. |
| `GameManager.cs` | Extract `DefeatEnemy()` from `EnemyDefeated()` so RPM depletion and ring-out share the wave-advance path. |
| `GameManager.cs` | Register `RingOutCondition` in `Start()` (currently deliberately omitted). |
| `GameUIManager.cs` | No change needed — already prints `RING OUT!` for `FinishReason.RingOut`. |

`BattleSnapshot` already carries `ArenaCenter`/`ArenaRadii` for AI, sourced from
`ArenaBounds`.

---

## Edge cases

- **Spawn safety** — current spawns are at normalized distance ≈ 0.77.
- **Wave reset** — player starts at center; 0.15 s grace is optional but cheap.
- **Enemy AI retreat** — clamps targets to 0.92 of the ellipse, so it will not
  intentionally run out. Knockback can still push it past the ring, which is a
  legitimate player win.
- **Simultaneous burst and ring-out** — burst wins (impact resolves before the
  tick).
- **Pause / game over** — skip the tick when `Time.timeScale == 0` or
  `battle.Finished`.
- **Last enemy ring-out** — `EnemiesClearedCondition` fires after the defeat;
  show the win panel with reason `RingOut` if the ring-out condition is
  evaluated first.

---

## Tests

**EditMode**

- Truth table: player in/out, enemy in/out, both out, using ellipse math.
- `BattleSceneDemoTests`: assert the oval sprite and spawn points inside the
  ellipse.

**PlayMode**

- Teleport player past the ring → Lose + `RING OUT!`.
- Teleport enemy past the ring → Win and next wave spawns.
- Teleport both → player loses.
- Paused game → no ring-out.

---

## Risks

- Knockback ring-outs can feel unfair. Mitigate with the visible outer ring, an
  optional edge tint, and a short post-reset grace.
- The physics wall sits at 1.05 × radii, so a bey can cross the ring before
  bouncing. Confirm ring-out fires before the wall stops it.
- Ring-out ends fights before RPM depletion, shifting balance. Re-run the 18-build
  matrix and the six-wave playtest.

---

## Slice checklist

1. Add `EnemyPosition` to `BattleContext`.
2. Add enemy-win ring-out condition.
3. Add `GameManager.RingOutTick()` and `DefeatEnemy()`.
4. Register the conditions in `Start()`.
5. Add EditMode + PlayMode tests.
6. Run suites, balance matrix, and the six-wave playtest.
7. Update `battle system.md` §17 and changelog.

**Estimate:** 1–2 hours plus test and playtest time.
