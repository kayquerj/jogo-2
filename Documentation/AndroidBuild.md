# Android Build Pipeline

This document describes the current Android build setup for the project and the
steps required to produce builds locally or through our CI pipeline.

## Unity editor requirements

- **Unity version:** 2022.3 LTS (the exact version is listed in
  `ProjectSettings/ProjectVersion.txt`).
- **Build support:** Install *Android Build Support* with Android SDK & NDK tools
  and OpenJDK from the Unity Hub installer.

## Local build checklist

1. **Clone LFS assets** – run `git lfs pull` after cloning to make sure textures
   and audio assets are downloaded.
2. **Open the project in Unity** and verify the active build target is Android
   (`File > Build Settings…` and select *Android*).
3. **Scenes in build** – ensure `Assets/Scenes/MainMenu.unity` and
   `Assets/Scenes/Game.unity` remain in the *Scenes In Build* list.
4. **Player settings** – confirm that `com.amazas.game` is the Android package
   identifier and that orientation is locked to portrait.
5. **Build** – from *Build Settings* choose *Build* (for an APK or AAB) or
   *Build And Run* when testing on a device.

## CI automation

A GitHub Actions workflow (`.github/workflows/android-build.yml`) produces
Android builds for pull requests and the `main` branch. Configure the following
repository secrets before running the workflow:

- `UNITY_LICENSE` – the base64-encoded Unity license.
- `UNITY_EMAIL` – account email used for the license activation.
- `UNITY_PASSWORD` – password or access token associated with the Unity account.

The job performs these steps:

1. Checks out the repository with Git LFS support enabled.
2. Caches the `Library/` folder keyed by package manifests for faster repeats.
3. Uses [`game-ci/unity-builder`](https://game.ci/docs/github/getting-started)
   to export an Android player into the `Builds/` directory.
4. Uploads the generated artifacts so they can be downloaded from the workflow
   summary page.

To trigger the workflow manually, use the *Run workflow* button available under
*Actions → Android Build*.

## Troubleshooting

- If the CI job fails with a license error, rotate the license and update the
  `UNITY_LICENSE` secret.
- When builds fail locally after upgrading packages, delete the `Library/`
  directory and reopen Unity to force it to reimport assets.
- Ensure LFS tracked binary assets (textures, audio, 3D models) are committed;
  Git history should not include `Library/`, `Temp/`, or `Build/` folders.
