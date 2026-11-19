# Ring-Out Feature — Future Slice

**Status: design only. Not implemented.** The live game currently has no
ring-out: `arenaRadius` is not serialized in `BattleScene` (so it loads as 0),
and `ResolveBattle` only runs on RPM depletion, burst, or enemy defeat.

Companion docs: `battle system.md` §17, `elemental system.md`.

---

## Goal

A spatial lose/win condition tied to the visible arena boundary. A bey pushed
outside the ring loses immediately, regardless of remaining RPM. In beyblade
terms: getting knocked out of the stadium.

---

## Boundary

`Assets/Game/Art/Arena.png` is 2160×2160, imported at PPU 100 with sprite size
21.6, and the arena GameObject is scaled **0.9**. A pixel scan across the center
gives these white-ring radii:

| Ring | Pixels from center | World units |
|---|---|---|
| Outer white ring | ~884 px | **7.96** |
| Inner white ring | ~650 px | 5.86 |
| Center disc | ~220 px | 1.98 |

Use the **outer ring** as the boundary and keep `arenaRadius = 8`. It already
matches the art within 0.04 units, so a bey crossing the white ring reads as a
ring-out.

The sprite half-width is 9.72, but that is the outer edge of the decorative
background, not the play boundary. Do not use 9.72.

---

## Rules

1. Player distance from `ArenaCenter` > `ArenaRadius` → **Lose**,
   `FinishReason.RingOut`.
2. Enemy distance > `ArenaRadius` → **Win**; counts as that wave defeated and
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

### 1. Serialize the arena fields

`GameManager` has `arenaRadius = 8f` in code, but `BattleScene.unity` was saved
before that field existed. Missing serialized fields load as `0`, so ring-out is
disabled. Write both fields explicitly into the scene's GameManager block:

```yaml
arenaRadius: 8
arenaCenter: {x: 0, y: 0}
```

This is the same class of bug that disabled the wave reset and the AI orbit.

### 2. Move spawn points inside the ring

Current enemy spawn points:

| Point | Position | Radius |
|---|---|---|
| SpawnPoint | (7.0, -0.3) | 7.01 |
| SpawnPoint (1) | (-7.7, 3.4) | **8.42** — outside |
| SpawnPoint (2) | (7.2, -3.4) | 7.96 — on the ring |
| SpawnPoint (3) | (-7.5, -3.6) | **8.32** — outside |

Two are outside the boundary and one sits exactly on it. With enemy ring-out
enabled, those spawns would instantly end the wave. Move all four to radius
**≤ 6.8** (between the inner ring at 5.86 and the outer ring at 7.96), or clamp
at spawn time.

### 3. Player reset position

`ResetPlayerForNextWave` already puts the player at `(0,0)` and zeroes velocity,
so wave starts are safe. Keep it that way.

---

## Code points

| File | Change |
|---|---|
| `BattleContext.cs` | Add `EnemyPosition` and `ArenaCenter`. |
| `RingOutCondition.cs` | Split into player (Lose) and enemy (Win) variants, or add a side flag. Keep `FinishReason.RingOut`. |
| `GameManager.cs` | Add a `RingOutTick()` accumulator in `Update`; gate on match state and `Time.timeScale`. |
| `GameManager.cs` | Extract `DefeatEnemy()` from `EnemyDefeated()` so RPM depletion and ring-out share the wave-advance path. |
| `GameUIManager.cs` | No change needed — already prints `RING OUT!` for `FinishReason.RingOut`. |
| `BeyPhysics.cs` | No change; knockback already moves bodies through the boundary. |
| `BattleScene.unity` | Serialize `arenaRadius`, `arenaCenter`; move spawn points inward. |

`BattleSnapshot` already carries `ArenaCenter`/`ArenaRadius` for AI; keep those
values sourced from the same GameManager fields.

---

## Edge cases

- **Spawn safety** — clamp spawn points inside `radius - 1`.
- **Wave reset** — player starts at center; 0.15 s grace is optional but cheap.
- **Enemy AI retreat** — already clamps targets to `ArenaRadius - 0.8` (7.2), so
  it will not intentionally run out. Knockback can still push it out, which is a
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

- Truth table: player in/out, enemy in/out, both out.
- `BattleSceneDemoTests`: assert `arenaRadius: 8` and every spawn point inside
  radius 8.
- Keep `BalanceSimulator` ring-out runs (already simulate radius 8).

**PlayMode**

- Teleport player beyond radius → Lose + `RING OUT!`.
- Teleport enemy beyond radius → Win and next wave spawns.
- Teleport both → player loses.
- Paused game → no ring-out.

---

## Risks

- Knockback ring-outs can feel unfair. Mitigate with the visible outer ring, an
  optional edge tint, and a short post-reset grace.
- Ring-out ends fights before RPM depletion, shifting balance. Re-run the 18-build
  matrix and the six-wave playtest.
- Enemy ring-out adds a new win path; confirm wave progression and `Finished`
  latches never double-advance.

---

## Slice checklist

1. Serialize `arenaRadius` / `arenaCenter` in the scene.
2. Move spawn points inside radius 6.8.
3. Extend `BattleContext` and split the conditions.
4. Add `GameManager.RingOutTick()` and `DefeatEnemy()`.
5. Add EditMode + PlayMode tests.
6. Run suites, balance matrix, and the six-wave playtest.
7. Update `battle system.md` §17 and changelog.

**Estimate:** 1–2 hours plus test and playtest time.
