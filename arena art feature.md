# Arena Art — Future Slice

**Status: implemented.** This was the last future slice. The flat placeholder was
replaced with a textured sand/clay oval; the geometry is unchanged.

Goal: replace the procedural placeholder oval with real art **without changing
the arena geometry**.

The AI image generator was unavailable (insufficient credits), so the asset was
drawn procedurally: a deterministic noise-based clay texture with a soft radial
gradient, scuff arcs, speckles, the two white rings, the center disc, and 24
radial ticks. A true artist asset can still be dropped in with the checklist
below.

---

## 1. Current placeholder

| Property | Value |
|---|---|
| Asset | `Assets/Game/Art/ArenaOval.png` |
| Image | 2048 × 1100 px (aspect 1.862) |
| Import | Single sprite, PPU 100 |
| Arena transform scale | 0.9 |
| Outer white ring | 88.9% of the image half-extents |
| World radii | 8.2 × 4.4 |
| Ring thickness | ~30 px (~0.27 world units) |
| Inner ring | 60% of half-extents |
| Center disc | 18% of half-extents |
| Background | full-bleed sand, no alpha |

Geometry source of truth is `ArenaBounds`, not the art. See `arena system.md`.

---

## 2. Art brief

- Keep the **same aspect ratio** (2048 × 1100) and place the outer white ring at
  **88.9% of the half-extents** so it lands on the 8.2 × 4.4 boundary.
- Sand or clay stadium floor with subtle texture; avoid high-frequency noise that
  fights the element-tinted beys.
- White outer ring stays the strongest visual cue — it is the future ring-out
  boundary.
- Inner ring and center disc are decorative; keep them lighter than the outer.
- Outside the outer ring can stay sand; if the new art uses transparency, set the
  camera clear color to the same sand tone (`#C49B70`).
- Keep contrast with all six element colors: Pyre red, Crye cyan, Gyre green,
  Hydre blue, Sparke yellow, Neutral grey.

---

## 3. Handoff checklist

1. Export at 2048 × 1100 (or same 1.862 aspect).
2. Ring at 88.9% of half-width and half-height.
3. Import as **Single**, PPU **100**, filter Bilinear, max size 2048.
4. Replace `ArenaOval.png` in place (keeps GUID and scene reference), or supply a
   new file and update the Arena SpriteRenderer.
5. The old placeholder is kept in `Assets/Game/Art/Placeholder/ArenaOval-placeholder.png`
   for rollback; git history is the other backup.
6. If the aspect or ring fraction changes, update `ArenaBounds.arenaRadii` and
   `ArenaFraming.margin` — the art must not redefine the physics boundary.

---

## 4. No code changes expected

The arena system is data-driven: `ArenaBounds` owns center/radii, `ArenaFraming`
owns camera fit, and the edge collider is generated from radii. An art-only swap
touches no scripts.

---

## 5. Validation

- Screenshots at **16:9, 16:10, 4:3** — full oval visible with margins on all
  sides; no camera background strip.
- Beys remain readable against the floor for every element.
- Impact particles remain visible against the new floor.
- Ring edge aligns with where beys bounce (edge collider at 1.05 × radii).

---

## 6. Risks

- Different aspect ratio silently shifts the boundary away from the visible ring.
- Bright or saturated art can wash out element tints.
- Transparent art with the current dark-blue camera background produces a
  visible seam.

---

## 7. Slice checklist

1. Produce the asset to the brief — done (procedural texture).
2. Import and swap; keep placeholder for rollback — done.
3. Capture the three aspect screenshots — pending manual play check.
4. Update `arena system.md` art section and changelog — done.
