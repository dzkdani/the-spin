# the-spin

Top-down 2D beyblade battle game built with Unity 6.3 (`6000.3.9f1`).

## Controls

- Move the mouse to steer the player bey.
- Left-click to dash. Dashing consumes RPM.
- Press `Escape` to pause or resume.

RPM is both health and stamina. Reduce each enemy's RPM to zero to defeat it;
the player loses when their own RPM reaches zero.

## Scenes

- `TitleScene` — title screen and menu.
- `BattleScene` — playable multi-enemy battle.
- `ExploreScene` — Fungus narrative content; not included in build settings.

## Project structure

- `Assets/Game/Scripts/Bey` — bey data, physics, combat, input coordination, and visuals.
- `Assets/Game/Scripts/Core` — match flow and scene service registration.
- `Assets/Game/Scripts/Enemy` — enemy AI and spawning.
- `Assets/Game/Scripts/UI` — title, HUD, pause, and game-over UI.
- `Assets/Game/Datas` — ScriptableObject tuning instances.
- `Assets/Tests` — Unity EditMode and PlayMode regression tests.

## Verification

The Unity Test Framework is included in `Packages/manifest.json`. Run EditMode
and PlayMode tests from Unity's Test Runner, or use Unity batch mode with
`-runTests -testPlatform EditMode` / `PlayMode`.
