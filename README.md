# Inventory & HUD Systems

This repository contains the runtime scripts for an inventory workflow that supports:

- Centralised inventory data via `InventoryService`
- Resource harvesting nodes that feed rewards into the inventory
- A lightweight HUD that displays resource counts, harvest progress, and interaction prompts
- Visual feedback when resources are gained
- Mobile-ready UI scaling for both portrait and landscape orientations

## Inventory Service

`InventoryService` is a singleton responsible for:

- Tracking resource counts (`ResourceType` enum)
- Dispatching change events (`ResourceChangedEvent`) for UI/UX hooks
- Loading/saving inventory snapshots through an `IInventoryPersistence` implementation

Use the `InventoryServiceBehaviour` bootstrap component in your first loaded scene to configure persistence (for example, `PlayerPrefsInventoryPersistence`). The behaviour calls `Load()` on start and saves during application pause/quit.

### Persistence

Implement `IInventoryPersistence` to integrate with a custom storage backend. A ready-to-use `PlayerPrefsInventoryPersistence` component serialises snapshots to JSON with Unity's `JsonUtility`.

Snapshots are represented by the serialisable `InventorySnapshot` class, which can be converted to and from dictionaries if you need lower-level access.

## Resource Nodes

`ResourceNode` drives harvest timing and reward distribution:

- Supports adjustable harvest durations, variance, respawn delays, and maximum harvest counts
- Provides events for harvest start/completion, cancellation, node respawn, and interaction gating
- Automatically deposits rewards into the `InventoryService`

Each node exposes a structured interaction prompt that the HUD can surface contextually.

## HUD

The HUD layer is composed of modular behaviours under `Assets/Scripts/UI/HUD`:

- `HUDController` subscribes to inventory events, keeps resource counters in sync, and orchestrates prompts and harvest progress tracking.
- `HUDResourceCounter` updates numeric labels and plays a pulse animation when resources increase.
- `HarvestProgressDisplay` visualises harvest timing on a `Slider` with smooth fade in/out transitions.
- `InteractionPromptView` presents context-sensitive prompts with crossfaded visibility.
- `ResourceGainFeedback` spawns floating text for resource gains.
- `HUDCanvasScaler` configures `CanvasScaler` to scale correctly across mobile aspect ratios.

Wire the controller to your scene by assigning counters, the progress view, interaction prompt, and feedback prefabs. Call `HUDController.ObserveNode` when the local player focuses a resource node.

## Feedback Loop

When a harvest completes:

1. The `ResourceNode` adds rewards to the inventory service.
2. `InventoryService` raises a `ResourceChangedEvent`.
3. `HUDController` updates the matching `HUDResourceCounter` and triggers pulse/floating text feedback.
4. The harvest progress display fades away while prompts reappear if the node remains available.

## Mobile Scaling

Attach `HUDCanvasScaler` to the HUD canvas to automatically apply portrait/landscape reference resolutions and match settings so the interface works on a wide range of mobile devices.

## Project Structure

```
Assets/
  Scripts/
    Gameplay/          <- Resource harvesting logic
    Inventory/         <- Inventory core and persistence hooks
    UI/HUD/            <- HUD controllers and widgets
```

## Getting Started

1. Drop `InventoryServiceBehaviour` and (optionally) `PlayerPrefsInventoryPersistence` into your bootstrap scene.
2. Create a HUD canvas using the provided scripts (`HUDCanvasScaler`, `HUDController`, etc.).
3. Place `ResourceNode` components in the world and call `ObserveNode` on the HUD when the player targets one.

These scripts are designed to be extended—add new `ResourceType` values, provide richer persistence, or skin the HUD to match your project without changing core logic.
