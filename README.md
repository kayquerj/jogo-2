# Arcade Orbit

Arcade Orbit is a lightweight browser prototype that demonstrates a complete menu-to-game flow with pause handling and persistent settings.

## Features

- **Main Menu** with options to start or resume the game, open the settings panel, and review credits & license information.
- **Interactive Game Scene** where you guide your energy orb to collect targets using keyboard or on-screen touch controls.
- **Pause Menu** offering resume, settings, and quit-to-menu actions. The game automatically pauses when you leave the tab or press <kbd>Esc</kbd>.
- **Settings Panel** that lets you toggle sound effects and adjust movement sensitivity. These preferences are stored in `localStorage` so they persist between sessions.
- **Session Persistence** that keeps your progress, score, and previous session state so you can resume where you left off.
- **Touch-friendly UI** with large action buttons and a virtual D-pad to support play on mobile devices.

## Getting Started

1. Open `index.html` in your browser.
2. Choose **Start Game** to begin a new run. If you have an unfinished session, you'll be prompted to resume it.
3. Use the on-screen controls or the arrow/WASD keys to move.
4. Pause the game with the in-game button or by pressing <kbd>Esc</kbd>.
5. Adjust settings at any time from the main menu or pause overlay.

No build step is required. All assets are self-contained in the repository.
