# Wave Ramp — Future Slice

**Status: design only. Not implemented. Priority 2**, after `ring out feature.md`,
before `arena art feature.md`.

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

1. **AI profile data only (recommended)** — zero code. Each wave already has its
   own `AiProfile`, so tune `dashChanceFar`, `dashChanceNear`, `decisionInterval`,
   `commitTime`, and behavior weights per wave.
2. **Per-wave chassis HP** — needs code: clone `BeyStats` at spawn via
   `ScriptableObject.Instantiate` and add `BeyData.SetChassis(BeyStats)` to update
   `Base` and `CurrentRPM`. Six new assets or a runtime multiplier.
3. **Stage hazards / arena shrink** — out of scope; would fight the oval geometry.

---

## 4. Proposed data-only ramp

Keep the personality names; make the aggression numbers monotonic:

| Wave | Profile | Dash far | Dash near | Commit | Decision |
|---|---|---|---|---|---|
| 1 | Calm | 0.30 | 0.05 | 0.90 | 0.18 |
| 2 | Pressure | 0.40 | 0.08 | 0.78 | 0.17 |
| 3 | Tank | 0.50 | 0.12 | 0.70 | 0.16 |
| 4 | Counter | 0.60 | 0.16 | 0.62 | 0.15 |
| 5 | Climax | 0.70 | 0.20 | 0.54 | 0.14 |
| 6 | Gyre | 0.80 | 0.24 | 0.48 | 0.13 |

Keep `retreatRpm: 0.15` and `retreatWeight: 0.6` for all profiles so the softer
retreat stays consistent. Type and element assets stay unchanged.

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

- EditMode: `AiSimulation` dash counts rise monotonically across the six profiles.
- PlayMode: full six-wave run; record clear time per wave and player RPM left.
- Regression: checkpoint reset still restores full RPM at each wave.

---

## 7. Risks

- Higher aggression shortens fights; player HP 9000 may make later waves trivial
  anyway. Measure before adding HP.
- Renaming profile personalities to match numbers is cosmetic; keep names.
- Balance matrix uses fixed reference builds, not these profiles — it will not
  catch this change. Play-mode measurement is the gate.

---

## 8. Slice checklist

1. Decide data-only vs data + HP ramp.
2. Apply per-profile values.
3. Add `AiSimulation` monotonic test.
4. Run the six-wave playtest and record clear times.
5. Update `battle system.md` §16 and changelog.
