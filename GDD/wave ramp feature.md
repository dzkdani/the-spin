# Wave Ramp — Future Slice

**Status: implemented.** Ring-out is done; `GDD/arena art feature.md` remains the
last future slice.

Goal: a difficulty curve across the six demo waves that does not break the
element comparison that the checkpoint reset was built for.

---

## 1. Current state

| Wave | Enemy | Element | Type | AI profile | Dash far / near |
|---|---|---|---|---|---|
| 1 | Warmup | Neutral | BAL | Calm | 0.30 / 0.05 |
| 2 | Ember Press | Pyre | ATK | Pressure | 0.90 / 0.35 |
| 3 | Tidal Wall | Hydre | DEF | Tank | 0.20 / 0.05 |
| 4 | Frost Counter | Crye | BAL | Counter | 0.60 / 0.20 |
| 5 | Final Surge | Sparke | ATK | Climax | 1.00 / 0.50 |
| 6 | Vortex Spin | Gyre | DEF | Gyre | 0.55 / 0.18 |

Difficulty is currently **not monotonic** — it follows element personality.
Wave 3 (Tank) is the calmest; wave 5 (Climax) is the most aggressive. The
element matchups add another swing: the Crye player is weak into Pyre (0.75×)
and strong into Gyre (1.35×).

Enemy HP is flat at 5000; player HP is 9000. Every wave starts from a full RPM
reset.

---

## 2. Constraint

A wave ramp changes the thing being compared. If wave 6 has more HP or more
aggression than wave 2, a slower clear can mean "harder element" or "later
wave" — the demo can no longer isolate element behavior.

Prefer a **small, legible ramp** that keeps HP equal so element comparisons stay
fair. Any HP scaling should be a separate, explicit follow-up.

---

## 3. Levers, ranked

1. **AI profile data only (chosen)** — zero code. Each wave already has its own
   `AiProfile`. Note: `dashChanceFar` / `dashChanceNear` were **dead fields** — no
   behavior read them — so they were removed from `AiProfile` and all six assets.
   The live aggression knobs are `strikeWeight`, `dashAggression`,
   `pressureWeight`, `strikeRange`, and `dashIntervalMin/Max`.
2. **Per-wave chassis HP** — needs code: clone `BeyStats` at spawn via
   `ScriptableObject.Instantiate` and add `BeyData.SetChassis(BeyStats)` to update
   `Base` and `CurrentRPM`. Six new assets or a runtime multiplier.
3. **Stage hazards / arena shrink** — out of scope; would fight the oval geometry.

---

## 4. Proposed data-only ramp

Applied values. Personality names stay; the live aggression numbers are monotonic:

| Wave | Profile | Strike | Aggression | Pressure | Strike range | Dash interval |
|---|---|---|---|---|---|---|
| 1 | Calm | 0.6 | 0.20 | 1.2 | 4.0 | 2.5–4.0 |
| 2 | Pressure | 0.8 | 0.36 | 1.1 | 4.2 | 2.2–3.6 |
| 3 | Tank | 1.0 | 0.52 | 1.0 | 4.4 | 1.9–3.2 |
| 4 | Counter | 1.2 | 0.68 | 0.9 | 4.6 | 1.6–2.8 |
| 5 | Climax | 1.4 | 0.84 | 0.8 | 4.8 | 1.3–2.4 |
| 6 | Gyre | 1.6 | 1.00 | 0.7 | 5.0 | 1.0–2.0 |

`commitTime` (0.6) and `decisionInterval` (0.15) are now **equal across profiles**:
a shorter commit actually lowers the simulated dash count because the brain
repeats the cached command, so ramping them broke monotonicity. `activeWeight`
is 1.0 everywhere. `retreatRpm: 0.15` and `retreatWeight: 0.6` stay fixed.
Type and element assets are unchanged.

This raises pressure without changing HP or damage, so fight length falls a
little on later waves. Verify against the 40–60 s target.

---

## 5. Optional HP ramp (separate decision)

If aggression alone is not enough:

- `maxRPM = 5000 × (1 + 0.05 × (wave − 1))` → wave 6 ≈ 6250.
- Requires runtime chassis clone + `BeyData.SetChassis`.
- Breaks strict element comparison; use only if the demo needs it.

---

## 6. Tests

- EditMode `WaveRampTests.DashPressureNeverFallsAcrossWaves` — `AiSimulation`
  dash counts are non-decreasing across the six profiles. RED before the ramp
  (AI_Tank 12 vs AI_Pressure 36), GREEN after.
- `ContentFoundationTests` updated after removing the dead dash-chance fields.
- EditMode 98/98, PlayMode 20/20.
- Remaining: full six-wave playtest to record clear time per wave.

---

## 7. Risks

- Higher aggression shortens fights; player HP 9000 may make later waves trivial
  anyway. Measure before adding HP.
- Renaming profile personalities to match numbers is cosmetic; keep names.
- Balance matrix uses fixed reference builds, not these profiles — it will not
  catch this change. Play-mode measurement is the gate.

---

## 8. Slice checklist

1. Remove dead dash-chance fields — done.
2. Apply per-profile values — done.
3. Add `AiSimulation` monotonic test — done.
4. Run the six-wave playtest and record clear times — pending.
5. Update `GDD/battle system.md` §16 and changelog — done.
