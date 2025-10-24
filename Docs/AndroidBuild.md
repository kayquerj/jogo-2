# Android Build Guide

This project is configured for mobile-first performance on Android. Follow the steps below to produce a signed build and to understand the assumptions made in the project configuration.

## Prerequisites

- Unity 2022.3 LTS (or newer) with the **Android Build Support** modules installed
- Android SDK / NDK and OpenJDK installed through the Unity Hub
- Access to a Unity Pro/Plus license for CI (see the optional GameCI workflow template)
- A signing keystore (replace the placeholder paths + passwords in the player settings)

## Performance Defaults

- **Quality**: Single `Mobile` tier with 2x MSAA, reduced shadow distance, lowered LOD bias (0.7), and VSync disabled to lock the target frame rate via player settings.
- **Rendering**: Universal Render Pipeline profile tuned for a single shadow cascade, SRP Batcher, dynamic batching, and no expensive post-processing by default.
- **Player Settings**: IL2CPP + ARM64 only, ASTC texture compression, sustained performance mode, and 60 FPS targets for both default and Android-specific frame rates.
- **Lighting**: Reflection probes, real-time GI, and transparency reception are pared back to keep GPU cost low on lower-end devices.

## Build Profile

A dedicated Android build profile named **`AndroidRelease`** is defined via the Unity Build Profiles system. It targets the Android platform, uses the `URP Mobile Pipeline` asset, and applies the 60 FPS cap with IL2CPP/ARM64 output.

1. Open the **Build Profiles** window (`File → Build Profiles`).
2. Select the **AndroidRelease** profile.
3. Ensure the desired scenes are included in the profile (by default the profile pulls from the active Editor Build Settings list).
4. Click **Build** (or **Build and Run**) to generate an Android App Bundle (AAB) in the `Builds/Android` directory.

> **Note:** If you do not see the AndroidRelease profile, re-import the `ProjectSettings/BuildProfile.json` file or create one manually as described in the next section.

## Manual Build Profile Creation (if required)

If your Unity version predates the Build Profiles window or you prefer the legacy flow:

1. Open **File → Build Settings…**.
2. Switch the platform to **Android**.
3. Add your gameplay scenes to the **Scenes In Build** list.
4. Set the **Build System** to **Gradle (Android App Bundle)**.
5. Enable **Development Build** only for debugging.
6. Click **Build** to create an `.aab` package.

## Build Output

- Output target: **Android App Bundle (`.aab`)** for Play Store distribution
- Scripting backend: **IL2CPP**
- Architecture: **ARM64 only**
- Texture compression: **ASTC** (fallbacks handled automatically)

## Keystore Configuration

- Placeholder keystore path: `UserProvided.keystore`
- Placeholder alias: `PlaceholderAlias`

Replace both the path and alias with your signing credentials before publishing. Keystore passwords are intentionally left blank inside the settings and should be supplied locally or via CI secrets.

## CI/CD (Optional)

The repository contains a commented GameCI workflow template in `.github/workflows/android-build.yml`. It outlines the required Unity license secrets and the steps necessary to run headless Android builds in GitHub Actions using the configured settings.
