# Mobile Player Controller Sample

This project contains a Unity setup for a mobile-friendly top-down controller using the Unity Input System. It includes:

- A `MobilePlayerController` component that drives a `CharacterController` using touch or gamepad movement input.
- A configurable `TopDownCameraRig` that orbits and follows a target with optional zoom support.
- A floating joystick implementation under `Assets/ThirdParty/MobileJoystick` released under the MIT license.
- Input System actions asset located at `Assets/Input/PlayerControls.inputactions` with bindings for mobile touch, joystick, and desktop fallback controls.
- Prefabs under `Assets/Prefabs` that assemble the player controller and camera rig ready for scene placement.

## Getting Started

1. Open the project in Unity 2022.3 LTS or later.
2. Load the `Player` prefab into your scene and ensure the `actionsAsset` field is assigned to `PlayerControls`.
3. Drop the `CameraRig` prefab into the scene and assign the player's transform to the `followTarget` field.
4. Place the `FloatingJoystick` UI (background and handle images) in a `Canvas` and attach the `FloatingJoystick` component, binding the background/handle references.
5. When you run on a touch device, the joystick will emit values on `<Gamepad>/leftStick`, driving the player through the Input System.

Movement speeds and responsiveness can be tuned in the inspector on the `MobilePlayerController` component to match your game's feel.
