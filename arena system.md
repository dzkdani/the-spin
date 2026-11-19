# Arena System

Reference for the-spin's arena geometry, art, camera framing, physics boundary,
and AI/spawn placement. Companion to `battle system.md` and
`ring out feature.md`.

---

## 1. Geometry

The arena is an **ellipse**, not a circle or a box.

| Field | Value |
|---|---|
| Center | `(0, 0)` |
| Radii | `(8.2, 4.4)` world units |
| Normalized distance | `sqrt(((x−cx)/rx)² + ((y−cy)/ry)²)` |
| Inside | normalized distance ≤ 1 |
| Outside | normalized distance > 1 |

`ArenaBounds` (`Assets/Game/Scripts/Battle/Arena/ArenaBounds.cs`) is the single
source of truth for center and radii. It exposes `Center`, `Radii`,
`NormalizedDistance`, and `Clamp`.

### Why an ellipse

The old arena was four invisible `BoxCollider2D` walls at roughly `x = ±10.8`,
`y = ±6.4`, while the visible white ring was a circle of radius 7.96. The
camera's vertical view is only ±5, so the top and bottom of the ring were never
visible. An ellipse with radii 8.2 × 4.4 fits inside a 16:9 view with a margin
on all four sides.

---

## 2. Art

`Assets/Game/Art/ArenaOval.png` is a textured sand/clay oval. The earlier flat
placeholder is kept at `Assets/Game/Art/Placeholder/ArenaOval-placeholder.png`.

| Property | Value |
|---|---|
| Image | 2048 × 1100 px |
| PPU | 100 |
| Sprite mode | Single |
| Arena scale | 0.9 |
| Outer white ring | 88.9% of the image half-extents → 8.2 × 4.4 world |
| Inner white ring | 60% of the half-extents |
| Center disc | 18% of the half-extents |

The old `Arena.png` is kept for reference but is no longer referenced by the
scene. Replace `ArenaOval.png` with a real asset later; keep the same aspect
ratio and ring fraction so the geometry stays aligned.

---

## 3. Camera framing

`ArenaFraming` (`Assets/Game/Scripts/Battle/Arena/ArenaFraming.cs`) sets the
orthographic camera size from the screen aspect:

```text
orthographicSize = max(ry + margin, (rx + margin) / aspect)
```

With `margin = 0.5` and radii 8.2 × 4.4:

| Aspect | Size | Visible half-width |
|---|---|---|
| 16:9 | 4.90 | 8.71 |
| 16:10 | 5.45 | 8.72 |
| 4:3 | 6.53 | 8.70 |

The full oval, including top/bottom and sides, stays visible. The camera itself
does not follow anything; `CameraShake` only offsets and restores it.

---

## 4. Physics boundary

- `ArenaBounds.Awake` builds an `EdgeCollider2D` with **64 segments** at
  `1.05 × radii`.
- Points are generated in world space and converted with
  `transform.InverseTransformPoint`, so the collider is correct even with the
  arena's 0.9 scale.
- `edgeRadius = 0.1`.
- The four legacy `BoxCollider2D` walls on the Arena children are disabled; they
  remain in the scene but do nothing.

The 5% margin means a bey can cross the visible ring before hitting the wall.
That leaves room for a future ring-out condition to fire first.

---

## 5. Movement, AI, and spawns

- `BattleSnapshot` carries `ArenaCenter` and `ArenaRadii` instead of a scalar
  radius. `EnemyInput.SetArena(center, radii)` receives them from `GameManager`.
- `RetreatBehavior` uses elliptical normalized distance:
  - edge bias starts at 0.9 and ramps to the boundary;
  - final target is clamped to `1 − 0.08` of the ellipse;
  - jitter stays suppressed during retreat.
- Enemy spawn points are `(±5.2, ±2.6)`, normalized distance ≈ 0.77 — safely
  inside the ring.
- `GameManager.ResetPlayerForNextWave` still puts the player at `(0, 0)` with
  zero velocity.

---

## 6. Ring-out

`RingOutCondition` uses elliptical normalized distance and **is registered** in
`BattleController`. `GameManager` checks both beys every 0.05 s; player out loses,
enemy out wins or advances the wave. See `ring out feature.md`.

No RPM is lost when a bey crosses the ring; the finish reason would carry the
result.

---

## 7. Tuning knobs

| Knob | Location | Effect |
|---|---|---|
| `arenaRadii` | `ArenaBounds` | playable ellipse |
| `wallScale` | `ArenaBounds` | physics wall outside the ring |
| `wallSegments` | `ArenaBounds` | edge collider smoothness |
| `margin` | `ArenaFraming` | screen padding around the ellipse |
| `ClampMargin` | `RetreatBehavior` | how far inside AI targets stay |

---

## 8. Tests

- `ArenaMathTests` — normalized distance, clamp, camera size across aspects.
- `BattleFlowTests` — ring-out condition truth table on an ellipse.
- `RetreatBehaviorTests` — snapshots carry radii; targets stay inside.
- `BattleSceneDemoTests` — arena sprite, spawn placement, no active box walls.

---

## 9. Known gaps

- The oval PNG is a procedural placeholder; real art should replace it.
- The camera background is dark blue; the placeholder art is full-bleed tan, so
  any aspect extreme can reveal a thin strip of background.
- The disabled legacy wall children are still in the scene.
- Ring-out is enabled; manual six-wave playtest still pending.
- There is no visible edge warning; the white ring is the only cue.
