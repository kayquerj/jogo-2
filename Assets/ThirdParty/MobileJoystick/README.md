# Mobile Joystick Package

This directory hosts a lightweight, open-source floating joystick UI implementation that is suitable for mobile projects. The joystick integrates with the Unity Input System through the `OnScreenControl` API and can be used as a drop-in virtual joystick for touch-based movement.

## Contents

- `Runtime/FloatingJoystick.cs` – A floating joystick control that exposes an Input System compatible stick control.
- `LICENSE.md` – MIT license for redistribution and modification.

To use the joystick, place the `FloatingJoystick` prefab (create a `Canvas` with two `Image` children for background and handle) in your UI and assign the `FloatingJoystick` component the appropriate references. The component will emit stick values to any bound `Vector2` actions (e.g., `<Gamepad>/leftStick`).
