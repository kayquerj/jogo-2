# PR Audit – Branch Consolidation (2025-10-24)

## Backup snapshot
- [x] Created remote backup branch `backup/pre-consolidation-20251024` from `origin/main` on 2025-10-24 to preserve the pre-merge baseline. (Command used: `git branch backup/pre-consolidation-20251024 origin/main` followed by `git push origin backup/pre-consolidation-20251024`.)

## Branch inventory

| Branch | Head commit | Author | Date | Feature scope summary | Stacking / dependency notes |
| --- | --- | --- | --- | --- | --- |
| chore/mobile-performance-android-build-setup | 86ddc500ab6c3a908d6cef8c9c2507c3354eb8e9 | engine-labs-app[bot] | 2025-10-24 | Configures Android CI workflow, URP mobile renderer tuning, and documentation for performance practices. | Baseline infrastructure branch; later work should rebase on it to inherit pipeline + URP settings. |
| feat-inventory-service-hud-resources | 4d4d5148ba70c9b223ab8b5c5a7788cd4399cb5d | engine-labs-app[bot] | 2025-10-24 | Introduces inventory domain (service, persistence, events) and HUD resource feedback behaviours. | Potential overlap with `feat/resource-node-harvest-system-scriptable-objects-respawn` (separate `ResourceNode` implementations). |
| feature-main-menu-pause-settings | 7645a8ee924ed45c61d1d82b28ccb88bb13b3465 | engine-labs-app[bot] | 2025-10-24 | Adds main menu, pause/settings UI, and prototype web assets illustrating interactions. | Depends on latest scene layout; merge after core systems to avoid rework. |
| feat/mobile-player-controller-camera-virtual-joystick-inputsystem | b8b711b945bc4efc10b3d66164cd483baec83c81 | engine-labs-app[bot] | 2025-10-24 | Implements mobile player controller, camera rig, Input System maps, and third-party joystick assets. | Forms the base for mobile play; `feature/json-save-load-game-state` includes the same prefabs/assets. |
| feature/json-save-load-game-state | b8d4dd24313f6565031de88ca0c71a53241d6fe7 | engine-labs-app[bot] | 2025-10-24 | Adds serialisable JSON save/load system plus game-state data models. | Stacked on the mobile controller branch (shares joystick + prefab assets); merge only after it lands. |
| feat/resource-node-harvest-system-scriptable-objects-respawn | 6c4fe9cd7f2594899b65e4a148aa276ad70b462e | engine-labs-app[bot] | 2025-10-24 | Creates harvesting system with ScriptableObject configs, resource pickups, and play mode tests. | Touches the same scenes/resources as inventory branch; coordinate scripting overlaps. |
| design-initial-island-level-greybox-expansion-tiles-mobile | c61c1ec73261e051f41b044c7fa6657ac0e6a500 | engine-labs-app[bot] | 2025-10-24 | Expands island greybox, adds materials, runtime navmesh baking, and mobile lighting helpers. | Relies on stable scene setup; schedule after systems/UI to minimise churn. |

## Dependency & stacking observations
- `feature/json-save-load-game-state` carries forward assets from `feat/mobile-player-controller-camera-virtual-joystick-inputsystem` (player prefab, camera rig, joystick package). Treat it as a dependent branch or expect redundant edits if merged independently.
- Both `feat-inventory-service-hud-resources` and `feat/resource-node-harvest-system-scriptable-objects-respawn` introduce their own `ResourceNode` logic (under different namespaces/folders). Consolidation work should reconcile these to avoid duplication and event conflicts.
- `feature-main-menu-pause-settings` and `design-initial-island-level-greybox-expansion-tiles-mobile` modify `Assets/Scenes/Game.unity` and `Assets/Scenes/MainMenu.unity` extensively. Merging them out of order with gameplay branches will exacerbate scene merge conflicts.

## Touched files and hotspot analysis

### Conflict hotspot matrix

| Area / File | Branch coverage | Notes |
| --- | --- | --- |
| Assets/Scenes/Game.unity | All 7 branches | Highest conflict surface; consider exporting critical objects to prefabs before merging. |
| Assets/Scenes/MainMenu.unity | All 7 branches | Same conflict level as the game scene; coordinate UI merges carefully. |
| Packages/manifest.json & Packages/packages-lock.json | All 7 branches | Package additions/removals differ; resolve version skew centrally. |
| ProjectSettings/*.asset (Build, Editor, Graphics, Input, Quality, etc.) | All 7 branches | Project settings will thrash without a canonical baseline; merge infra branch first. |
| Assets/Settings/UniversalRenderPipelineAsset*.asset | All 7 branches | URP renderer tuning varies per branch; capture desired defaults before merging. |
| .gitignore and .gitattributes | All 7 branches | Rule collisions likely; compose a unified ignore/attribute strategy. |
| README.md | All 7 branches | Messaging/documentation updates need consolidation to avoid regressions. |

### New folders and assets introduced
- **chore/mobile-performance-android-build-setup:** `.github/workflows/android-build.yml`, `Docs/AndroidBuild.md`, URP mobile renderer assets.
- **feat/mobile-player-controller-camera-virtual-joystick-inputsystem:** `Assets/Input/`, `Assets/Prefabs/`, `Assets/ThirdParty/MobileJoystick/`, `THIRD_PARTY_NOTICES.md`, new camera/player scripts.
- **feature/json-save-load-game-state:** `Assets/Scripts/GameState/` plus reuse of joystick/mobile controller assets.
- **feat-inventory-service-hud-resources:** `Assets/Scripts/Gameplay/`, `Assets/Scripts/Inventory/`, `Assets/Scripts/UI/HUD/`, resource change events, persistence layer.
- **feat/resource-node-harvest-system-scriptable-objects-respawn:** `Assets/Scripts/Harvesting/`, `Assets/Tests/PlayMode/`.
- **design-initial-island-level-greybox-expansion-tiles-mobile:** `Assets/Materials/`, `Assets/Scripts/Level/` (lighting, navmesh helpers).
- **feature-main-menu-pause-settings:** Adds standalone prototype files (`index.html`, `script.js`, `styles.css`) alongside scene/UI updates.

### Non-Unity assets requiring special handling
- `feature-main-menu-pause-settings` introduces `index.html`, `script.js`, and `styles.css` at the repository root. These web prototype files should be isolated from Unity build output (e.g., move under Docs/ or exclude via `.gitignore`) before shipping.
- `feat/mobile-player-controller-camera-virtual-joystick-inputsystem` (and the stacked JSON save branch) add `Assets/ThirdParty/MobileJoystick` plus `THIRD_PARTY_NOTICES.md`. Ensure licence requirements are satisfied during integration.

## Recommended merge order
1. **chore/mobile-performance-android-build-setup** – establishes CI, URP, and baseline project settings for all later merges.
2. **feat/mobile-player-controller-camera-virtual-joystick-inputsystem** – foundational mobile control assets used by multiple gameplay branches.
3. **feature/json-save-load-game-state** – stacked on the mobile controller branch; merge immediately afterwards while contexts align.
4. **feat/resource-node-harvest-system-scriptable-objects-respawn** – core harvesting gameplay that other systems (inventory/HUD) need to reference.
5. **feat-inventory-service-hud-resources** – hooks into resource flows; merge after harvesting logic to reconcile `ResourceNode` implementations.
6. **feature-main-menu-pause-settings** – UI/UX layer that ties into previously merged systems and touches shared scenes.
7. **design-initial-island-level-greybox-expansion-tiles-mobile** – level polish and lighting; finalise once mechanics and UI are stable to minimise scene churn.

## Per-branch touched file inventories

<details>
<summary><code>chore/mobile-performance-android-build-setup</code></summary>

```
.gitattributes
.github.meta
.github/workflows.meta
.github/workflows/android-build.yml
.gitignore
Assets.meta
Assets/Scenes/Game.unity
Assets/Scenes/MainMenu.unity
Assets/Settings.meta
Assets/Settings/URP-Mobile-Pipeline.asset
Assets/Settings/URP-Mobile-Pipeline.asset.meta
Assets/Settings/URP-Mobile-Renderer.asset
Assets/Settings/URP-Mobile-Renderer.asset.meta
Assets/Settings/UniversalRenderPipelineAsset.asset
Assets/Settings/UniversalRenderPipelineAsset_Renderer.asset
Docs.meta
Docs/AndroidBuild.md
Docs/AndroidBuild.md.meta
Packages.meta
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings.meta
ProjectSettings/BuildProfile.json
ProjectSettings/EditorBuildSettings.asset
ProjectSettings/EditorSettings.asset
ProjectSettings/EditorUserBuildSettings.asset
ProjectSettings/GraphicsSettings.asset
ProjectSettings/InputManager.asset
ProjectSettings/InputSystemPackageSettings.asset
ProjectSettings/PackageManagerSettings.asset
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
README.md
```

</details>

<details>
<summary><code>feat-inventory-service-hud-resources</code></summary>

```
.gitattributes
.gitignore
Assets.meta
Assets/Scenes.meta
Assets/Scenes/Game.unity
Assets/Scenes/Game.unity.meta
Assets/Scenes/MainMenu.unity
Assets/Scenes/MainMenu.unity.meta
Assets/Scripts/Gameplay/ResourceNode.cs
Assets/Scripts/Inventory/IInventoryPersistence.cs
Assets/Scripts/Inventory/InventoryService.cs
Assets/Scripts/Inventory/InventoryServiceBehaviour.cs
Assets/Scripts/Inventory/InventorySnapshot.cs
Assets/Scripts/Inventory/PlayerPrefsInventoryPersistence.cs
Assets/Scripts/Inventory/ResourceChangedEvent.cs
Assets/Scripts/Inventory/ResourceType.cs
Assets/Scripts/UI/HUD/HUDCanvasScaler.cs
Assets/Scripts/UI/HUD/HUDController.cs
Assets/Scripts/UI/HUD/HUDResourceCounter.cs
Assets/Scripts/UI/HUD/HarvestProgressDisplay.cs
Assets/Scripts/UI/HUD/InteractionPromptView.cs
Assets/Scripts/UI/HUD/ResourceGainFeedback.cs
Assets/Settings.meta
Assets/Settings/UniversalRenderPipelineAsset.asset
Assets/Settings/UniversalRenderPipelineAsset.asset.meta
Assets/Settings/UniversalRenderPipelineAsset_Renderer.asset
Assets/Settings/UniversalRenderPipelineAsset_Renderer.asset.meta
Packages.meta
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/EditorBuildSettings.asset
ProjectSettings/EditorSettings.asset
ProjectSettings/EditorUserBuildSettings.asset
ProjectSettings/GraphicsSettings.asset
ProjectSettings/InputManager.asset
ProjectSettings/InputSystemPackageSettings.asset
ProjectSettings/PackageManagerSettings.asset
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
README.md
```

</details>

<details>
<summary><code>feature-main-menu-pause-settings</code></summary>

```
.gitattributes
.gitignore
Assets.meta
Assets/Scenes.meta
Assets/Scenes/Game.unity
Assets/Scenes/Game.unity.meta
Assets/Scenes/MainMenu.unity
Assets/Scenes/MainMenu.unity.meta
Assets/Settings.meta
Assets/Settings/UniversalRenderPipelineAsset.asset
Assets/Settings/UniversalRenderPipelineAsset.asset.meta
Assets/Settings/UniversalRenderPipelineAsset_Renderer.asset
Assets/Settings/UniversalRenderPipelineAsset_Renderer.asset.meta
Packages.meta
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/EditorBuildSettings.asset
ProjectSettings/EditorSettings.asset
ProjectSettings/EditorUserBuildSettings.asset
ProjectSettings/GraphicsSettings.asset
ProjectSettings/InputManager.asset
ProjectSettings/InputSystemPackageSettings.asset
ProjectSettings/PackageManagerSettings.asset
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
README.md
index.html
script.js
styles.css
```

</details>

<details>
<summary><code>feat/mobile-player-controller-camera-virtual-joystick-inputsystem</code></summary>

```
.gitattributes
.gitignore
Assets.meta
Assets/Animations.meta
Assets/Animations/PlayerPlaceholder.controller
Assets/Animations/PlayerPlaceholder.controller.meta
Assets/Input.meta
Assets/Input/PlayerControls.inputactions
Assets/Input/PlayerControls.inputactions.meta
Assets/Materials.meta
Assets/Materials/README.md
Assets/Materials/README.md.meta
Assets/Prefabs.meta
Assets/Prefabs/CameraRig.prefab
Assets/Prefabs/CameraRig.prefab.meta
Assets/Prefabs/Player.prefab
Assets/Prefabs/Player.prefab.meta
Assets/Scenes/Game.unity
Assets/Scenes/MainMenu.unity
Assets/Scripts.meta
Assets/Scripts/Camera.meta
Assets/Scripts/Camera/TopDownCameraRig.cs
Assets/Scripts/Camera/TopDownCameraRig.cs.meta
Assets/Scripts/Player.meta
Assets/Scripts/Player/MobilePlayerController.cs
Assets/Scripts/Player/MobilePlayerController.cs.meta
Assets/Settings/UniversalRenderPipelineAsset.asset
Assets/Settings/UniversalRenderPipelineAsset_Renderer.asset
Assets/ThirdParty.meta
Assets/ThirdParty/MobileJoystick.meta
Assets/ThirdParty/MobileJoystick/Editor.meta
Assets/ThirdParty/MobileJoystick/Editor/.gitkeep
Assets/ThirdParty/MobileJoystick/Editor/.gitkeep.meta
Assets/ThirdParty/MobileJoystick/LICENSE.md
Assets/ThirdParty/MobileJoystick/LICENSE.md.meta
Assets/ThirdParty/MobileJoystick/Prefabs.meta
Assets/ThirdParty/MobileJoystick/Prefabs/README.md
Assets/ThirdParty/MobileJoystick/Prefabs/README.md.meta
Assets/ThirdParty/MobileJoystick/README.md
Assets/ThirdParty/MobileJoystick/README.md.meta
Assets/ThirdParty/MobileJoystick/Runtime.meta
Assets/ThirdParty/MobileJoystick/Runtime/FloatingJoystick.cs
Assets/ThirdParty/MobileJoystick/Runtime/FloatingJoystick.cs.meta
Packages.meta
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings.meta
ProjectSettings/EditorBuildSettings.asset
ProjectSettings/EditorSettings.asset
ProjectSettings/EditorUserBuildSettings.asset
ProjectSettings/GraphicsSettings.asset
ProjectSettings/InputManager.asset
ProjectSettings/InputSystemPackageSettings.asset
ProjectSettings/PackageManagerSettings.asset
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/ProjectVersion.txt.meta
ProjectSettings/QualitySettings.asset
README.md
THIRD_PARTY_NOTICES.md
```

</details>

<details>
<summary><code>feature/json-save-load-game-state</code></summary>

```
.gitattributes
.gitignore
Assets.meta
Assets/Animations.meta
Assets/Animations/PlayerPlaceholder.controller
Assets/Animations/PlayerPlaceholder.controller.meta
Assets/Input.meta
Assets/Input/PlayerControls.inputactions
Assets/Input/PlayerControls.inputactions.meta
Assets/Materials.meta
Assets/Materials/README.md
Assets/Materials/README.md.meta
Assets/Prefabs.meta
Assets/Prefabs/CameraRig.prefab
Assets/Prefabs/CameraRig.prefab.meta
Assets/Prefabs/Player.prefab
Assets/Prefabs/Player.prefab.meta
Assets/Scenes/Game.unity
Assets/Scenes/MainMenu.unity
Assets/Scripts.meta
Assets/Scripts/Camera.meta
Assets/Scripts/Camera/TopDownCameraRig.cs
Assets/Scripts/Camera/TopDownCameraRig.cs.meta
Assets/Scripts/GameState.meta
Assets/Scripts/GameState/GameSaveManager.cs
Assets/Scripts/GameState/GameSaveManager.cs.meta
Assets/Scripts/GameState/GameStateData.cs
Assets/Scripts/GameState/GameStateData.cs.meta
Assets/Scripts/GameState/GateState.cs
Assets/Scripts/GameState/GateState.cs.meta
Assets/Scripts/GameState/InventorySystem.cs
Assets/Scripts/GameState/InventorySystem.cs.meta
Assets/Scripts/GameState/ResourceNodeState.cs
Assets/Scripts/GameState/ResourceNodeState.cs.meta
Assets/Scripts/Player.meta
Assets/Scripts/Player/MobilePlayerController.cs
Assets/Scripts/Player/MobilePlayerController.cs.meta
Assets/Settings/UniversalRenderPipelineAsset.asset
Assets/Settings/UniversalRenderPipelineAsset_Renderer.asset
Assets/ThirdParty.meta
Assets/ThirdParty/MobileJoystick.meta
Assets/ThirdParty/MobileJoystick/Editor.meta
Assets/ThirdParty/MobileJoystick/Editor/.gitkeep
Assets/ThirdParty/MobileJoystick/Editor/.gitkeep.meta
Assets/ThirdParty/MobileJoystick/LICENSE.md
Assets/ThirdParty/MobileJoystick/LICENSE.md.meta
Assets/ThirdParty/MobileJoystick/Prefabs.meta
Assets/ThirdParty/MobileJoystick/Prefabs/README.md
Assets/ThirdParty/MobileJoystick/Prefabs/README.md.meta
Assets/ThirdParty/MobileJoystick/README.md
Assets/ThirdParty/MobileJoystick/README.md.meta
Assets/ThirdParty/MobileJoystick/Runtime.meta
Assets/ThirdParty/MobileJoystick/Runtime/FloatingJoystick.cs
Assets/ThirdParty/MobileJoystick/Runtime/FloatingJoystick.cs.meta
Packages.meta
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings.meta
ProjectSettings/EditorBuildSettings.asset
ProjectSettings/EditorSettings.asset
ProjectSettings/EditorUserBuildSettings.asset
ProjectSettings/GraphicsSettings.asset
ProjectSettings/InputManager.asset
ProjectSettings/InputSystemPackageSettings.asset
ProjectSettings/PackageManagerSettings.asset
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/ProjectVersion.txt.meta
ProjectSettings/QualitySettings.asset
README.md
THIRD_PARTY_NOTICES.md
```

</details>

<details>
<summary><code>feat/resource-node-harvest-system-scriptable-objects-respawn</code></summary>

```
.gitattributes
.gitignore
Assets.meta
Assets/Scenes.meta
Assets/Scenes/Game.unity
Assets/Scenes/Game.unity.meta
Assets/Scenes/MainMenu.unity
Assets/Scenes/MainMenu.unity.meta
Assets/Scripts/Harvesting/ResourceNode.cs
Assets/Scripts/Harvesting/ResourceNodeConfig.cs
Assets/Scripts/Harvesting/ResourcePickup.cs
Assets/Scripts/Harvesting/ResourceYield.cs
Assets/Settings.meta
Assets/Settings/UniversalRenderPipelineAsset.asset
Assets/Settings/UniversalRenderPipelineAsset.asset.meta
Assets/Settings/UniversalRenderPipelineAsset_Renderer.asset
Assets/Settings/UniversalRenderPipelineAsset_Renderer.asset.meta
Assets/Tests/PlayMode/ResourceNodePlayModeTests.cs
Packages.meta
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/EditorBuildSettings.asset
ProjectSettings/EditorSettings.asset
ProjectSettings/EditorUserBuildSettings.asset
ProjectSettings/GraphicsSettings.asset
ProjectSettings/InputManager.asset
ProjectSettings/InputSystemPackageSettings.asset
ProjectSettings/PackageManagerSettings.asset
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
README.md
```

</details>

<details>
<summary><code>design-initial-island-level-greybox-expansion-tiles-mobile</code></summary>

```
.gitattributes
.gitignore
Assets.meta
Assets/Materials.meta
Assets/Materials/BridgeDeck.mat
Assets/Materials/BridgeDeck.mat.meta
Assets/Materials/ExpansionPlatform.mat
Assets/Materials/ExpansionPlatform.mat.meta
Assets/Materials/GateBarrier.mat
Assets/Materials/GateBarrier.mat.meta
Assets/Materials/IslandBase.mat
Assets/Materials/IslandBase.mat.meta
Assets/Materials/ResourceNode.mat
Assets/Materials/ResourceNode.mat.meta
Assets/Scenes.meta
Assets/Scenes/Game.unity
Assets/Scenes/Game.unity.meta
Assets/Scenes/MainMenu.unity
Assets/Scripts.meta
Assets/Scripts/Level.meta
Assets/Scripts/Level/IslandLevelGreybox.cs
Assets/Scripts/Level/IslandLevelGreybox.cs.meta
Assets/Scripts/Level/LevelMarkers.cs
Assets/Scripts/Level/LevelMarkers.cs.meta
Assets/Scripts/Level/MobileLightingSetup.cs
Assets/Scripts/Level/MobileLightingSetup.cs.meta
Assets/Scripts/Level/RuntimeNavMeshBaker.cs
Assets/Scripts/Level/RuntimeNavMeshBaker.cs.meta
Assets/Settings/UniversalRenderPipelineAsset.asset
Assets/Settings/UniversalRenderPipelineAsset_Renderer.asset
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/EditorBuildSettings.asset
ProjectSettings/EditorSettings.asset
ProjectSettings/EditorUserBuildSettings.asset
ProjectSettings/GraphicsSettings.asset
ProjectSettings/InputManager.asset
ProjectSettings/InputSystemPackageSettings.asset
ProjectSettings/PackageManagerSettings.asset
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
README.md
```

</details>
