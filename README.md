# Unity 2022.3 URP Starter Project

This repository contains a clean Unity **2022.3 LTS** project configured with the Universal Render Pipeline (URP), Android build support, and the new Input System.

## Requirements

- Unity **2022.3 LTS** (tested with `2022.3.20f1`)
- Unity modules:
  - **Android Build Support** (including Android SDK & NDK Tools and OpenJDK)
  - Desired desktop platform support (e.g., Windows, macOS, Linux) for editing
- [Git Large File Storage](https://git-lfs.github.com/)
- Git 2.30+

## Getting Started

1. **Install Git LFS (one-time machine setup):**
   ```bash
   git lfs install
   ```
2. **Clone the repository and pull LFS assets:**
   ```bash
   git clone <repository-url>
   cd <repository-folder>
   git lfs pull
   ```
3. **Open the project in Unity Hub:**
   - Add the project folder to Unity Hub.
   - When prompted, allow Unity to upgrade the project using version `2022.3.20f1` or newer in the 2022.3 LTS line.
4. **Verify Android build support:**
   - Open **File → Build Settings…** and ensure the **Android** platform is selected.
   - Minimum SDK version is set to **API Level 24 (Android 7.0)**.
5. **Scene bootstrap:**
   - Default scenes are located in `Assets/Scenes/`
   - `MainMenu.unity` and `Game.unity` are pre-created and empty placeholders.

## Project Configuration Highlights

- **Render Pipeline:** Configured to use URP with pipeline assets stored in `Assets/Settings/`.
- **Input:** Legacy input is disabled. The project uses the new Input System package exclusively.
- **Android Player Settings:**
  - Placeholder package identifier `com.company.product`.
  - Minimum SDK version 24.
  - Permissions configured to only request those explicitly enabled in Player Settings.
- **Source Control:**
  - `.gitignore` tailored for Unity projects.
  - `.gitattributes` initializes Git LFS for common binary asset types (textures, audio, video, models, etc.).
  - Meta files are versioned for all tracked assets.

## Folder Structure

```
Assets/
  Scenes/
    Game.unity
    MainMenu.unity
  Settings/
    UniversalRenderPipelineAsset.asset
    UniversalRenderPipelineAsset_Renderer.asset
Packages/
  manifest.json
  packages-lock.json
ProjectSettings/
```

Feel free to build on this foundation for production work. Update the package identifier, company name, and product name in **Project Settings → Player** before shipping.
