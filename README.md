# Mobile Performance & Android Build Setup

This repository is configured for a mobile-focused Unity project. The Project Settings, Universal Render Pipeline assets, and Android Player configuration are tuned to prioritise predictable 60 FPS rendering on mid-range hardware while keeping GPU and CPU costs low.

## Highlights

- **Quality Settings**: A single `Mobile` tier disables VSync, reduces shadow distance, trims LOD bias, and targets 60 FPS.
- **Universal Render Pipeline**: Lightweight renderer configuration with SRP Batcher and dynamic batching enabled, soft shadows disabled, and cascade count reduced to one.
- **Android Player Settings**: IL2CPP scripting backend targeting ARM64, ASTC texture compression defaults, and placeholder keystore entries for signed builds.
- **Build Automation**: Android build profile plus documentation in `Docs/AndroidBuild.md`. A commented GameCI workflow template (`.github/workflows/android-build.yml`) outlines the secrets required to enable CI builds.

Refer to the documentation for the exact steps needed to produce builds locally or in CI.
