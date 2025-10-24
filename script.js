(() => {
  const Scenes = {
    MAIN_MENU: "main-menu",
    GAME: "game",
  };

  const STORAGE_KEYS = {
    SETTINGS: "arcade-orbit.settings.v1",
    GAME: "arcade-orbit.game-state.v1",
  };

  const defaultSettings = Object.freeze({
    audioEnabled: true,
    sensitivity: 1.25,
  });

  const sceneSections = Array.from(document.querySelectorAll(".scene"));
  const mainMenu = document.querySelector('[data-scene="main-menu"]');
  const pauseOverlay = document.getElementById("pauseOverlay");
  const settingsModal = document.getElementById("settingsModal");
  const creditsModal = document.getElementById("creditsModal");
  const startButton = mainMenu.querySelector('[data-action="start-game"]');
  const menuSettingsButton = mainMenu.querySelector('[data-action="open-settings"]');
  const creditsButton = mainMenu.querySelector('[data-action="open-credits"]');
  const pauseButton = document.getElementById("pauseButton");
  const resumeButton = pauseOverlay.querySelector('[data-action="resume-game"]');
  const pauseSettingsButton = pauseOverlay.querySelector('[data-action="open-settings"]');
  const quitButton = pauseOverlay.querySelector('[data-action="quit-game"]');
  const settingsCloseButton = settingsModal.querySelector('[data-action="close-settings"]');
  const creditsCloseButton = creditsModal.querySelector('[data-action="close-credits"]');
  const audioToggle = document.getElementById("audioToggle");
  const sensitivitySlider = document.getElementById("sensitivitySlider");
  const sensitivityValue = document.getElementById("sensitivityValue");
  const scoreValue = document.getElementById("scoreValue");
  const dpadButtons = Array.from(document.querySelectorAll(".control-btn"));

  let currentScene = Scenes.MAIN_MENU;
  let settingsContext = null;
  let resumePromptDismissed = false;

  const settings = loadSettings();
  let savedSnapshot = loadGameState();

  const game = new GameController(
    document.getElementById("gameCanvas"),
    scoreValue
  );

  game.applySettings(settings);
  game.setStateChangeListener((state) => {
    if (state) {
      storeGameState({ ...state }, currentScene);
    }
  });

  applySettingsUI();
  updateStartButtonLabel();

  startButton.addEventListener("click", () => {
    handleStartRequest();
  });

  menuSettingsButton.addEventListener("click", () => {
    openSettings("menu");
  });

  creditsButton.addEventListener("click", () => {
    openCredits();
  });

  pauseButton.addEventListener("click", () => {
    openPauseMenu();
  });

  resumeButton.addEventListener("click", () => {
    closePauseMenu();
  });

  pauseSettingsButton.addEventListener("click", () => {
    openSettings("pause");
  });

  quitButton.addEventListener("click", () => {
    hidePauseOverlay();
    game.stop();
    showScene(Scenes.MAIN_MENU);
    storeGameState({ active: false, paused: false, score: game.getScore() }, Scenes.MAIN_MENU);
  });

  settingsCloseButton.addEventListener("click", () => {
    closeSettings();
  });

  creditsCloseButton.addEventListener("click", () => {
    closeCredits();
  });

  settingsModal.addEventListener("click", (event) => {
    if (event.target === settingsModal) {
      closeSettings();
    }
  });

  creditsModal.addEventListener("click", (event) => {
    if (event.target === creditsModal) {
      closeCredits();
    }
  });

  audioToggle.addEventListener("change", () => {
    settings.audioEnabled = audioToggle.checked;
    game.setAudioEnabled(settings.audioEnabled);
    saveSettings(settings);
  });

  sensitivitySlider.addEventListener("input", () => {
    const value = Number.parseFloat(sensitivitySlider.value);
    sensitivityValue.textContent = `${value.toFixed(2)}×`;
    game.setSensitivity(value);
  });

  sensitivitySlider.addEventListener("change", () => {
    const value = clamp(
      Number.parseFloat(sensitivitySlider.value) || defaultSettings.sensitivity,
      Number(sensitivitySlider.min),
      Number(sensitivitySlider.max)
    );
    settings.sensitivity = value;
    sensitivitySlider.value = value.toString();
    sensitivityValue.textContent = `${value.toFixed(2)}×`;
    game.setSensitivity(value);
    saveSettings(settings);
  });

  dpadButtons.forEach((button) => {
    button.addEventListener("pointerdown", (event) => {
      event.preventDefault();
      button.setPointerCapture(event.pointerId);
      const dir = button.dataset.dir;
      game.setDirectionActive(dir, true);
    });
    const stop = (event) => {
      const dir = button.dataset.dir;
      game.setDirectionActive(dir, false);
    };
    button.addEventListener("pointerup", stop);
    button.addEventListener("pointercancel", stop);
    button.addEventListener("lostpointercapture", stop);
    button.addEventListener("contextmenu", (event) => {
      event.preventDefault();
    });
  });

  const keyMap = {
    ArrowUp: "up",
    ArrowDown: "down",
    ArrowLeft: "left",
    ArrowRight: "right",
    w: "up",
    a: "left",
    s: "down",
    d: "right",
  };

  window.addEventListener("keydown", (event) => {
    if (event.key === "Escape") {
      if (currentScene === Scenes.GAME && pauseOverlay.classList.contains("hidden")) {
        openPauseMenu();
      } else if (!pauseOverlay.classList.contains("hidden")) {
        closePauseMenu();
      }
    }
    const direction = keyMap[event.key];
    if (direction) {
      event.preventDefault();
      game.setDirectionActive(direction, true);
    }
  });

  window.addEventListener("keyup", (event) => {
    const direction = keyMap[event.key];
    if (direction) {
      game.setDirectionActive(direction, false);
    }
  });

  window.addEventListener(
    "pointerdown",
    () => {
      game.unlockAudio();
    },
    { once: true }
  );

  document.addEventListener("visibilitychange", () => {
    if (document.hidden && currentScene === Scenes.GAME) {
      openPauseMenu();
    }
  });

  window.addEventListener("beforeunload", () => {
    if (currentScene === Scenes.GAME) {
      const snapshot = game.exportState();
      storeGameState(snapshot ? { ...snapshot } : null, Scenes.GAME);
    } else {
      storeGameState({ active: false, paused: false, score: game.getScore() }, Scenes.MAIN_MENU);
    }
    saveSettings(settings);
  });

  if (savedSnapshot && savedSnapshot.scene === Scenes.GAME && savedSnapshot.active) {
    resumePromptDismissed = false;
  } else {
    savedSnapshot = null;
  }

  function handleStartRequest() {
    if (savedSnapshot && savedSnapshot.scene === Scenes.GAME && savedSnapshot.active && !resumePromptDismissed) {
      const resume = window.confirm("Resume your previous session?");
      resumePromptDismissed = true;
      if (resume) {
        resumeFromSnapshot(savedSnapshot);
        return;
      }
    }
    startNewGame();
  }

  function startNewGame() {
    showScene(Scenes.GAME);
    hidePauseOverlay();
    game.startNew(settings);
    const snapshot = game.exportState();
    storeGameState(snapshot ? { ...snapshot } : null, Scenes.GAME);
  }

  function resumeFromSnapshot(snapshot) {
    showScene(Scenes.GAME);
    game.restore(snapshot, settings);
    if (snapshot.paused) {
      openPauseMenu();
    }
  }

  function showScene(scene) {
    if (scene === currentScene) {
      return;
    }
    sceneSections.forEach((section) => {
      const isActive = section.dataset.scene === scene;
      section.toggleAttribute("hidden", !isActive);
      section.classList.toggle("active", isActive);
    });
    currentScene = scene;
    if (scene === Scenes.MAIN_MENU) {
      hidePauseOverlay();
      closeSettings(true);
      closeCredits(true);
      game.stop();
      storeGameState({ active: false, paused: false, score: game.getScore() }, Scenes.MAIN_MENU);
      resumePromptDismissed = false;
    }
  }

  function openPauseMenu() {
    if (!game.isActive()) {
      return;
    }
    game.pause();
    pauseOverlay.classList.remove("hidden");
    pauseOverlay.setAttribute("aria-hidden", "false");
    storeGameState(game.exportState(), Scenes.GAME);
    resumeButton.focus({ preventScroll: true });
  }

  function hidePauseOverlay() {
    pauseOverlay.classList.add("hidden");
    pauseOverlay.setAttribute("aria-hidden", "true");
  }

  function closePauseMenu() {
    if (pauseOverlay.classList.contains("hidden")) {
      return;
    }
    hidePauseOverlay();
    game.resume();
    storeGameState(game.exportState(), Scenes.GAME);
  }

  function openSettings(context) {
    settingsContext = context;
    if (context === "pause") {
      hidePauseOverlay();
    }
    settingsModal.classList.remove("hidden");
    settingsModal.setAttribute("aria-hidden", "false");
    audioToggle.focus({ preventScroll: true });
  }

  function closeSettings(silent = false) {
    if (settingsModal.classList.contains("hidden")) {
      return;
    }
    settingsModal.classList.add("hidden");
    settingsModal.setAttribute("aria-hidden", "true");
    if (!silent) {
      if (settingsContext === "pause") {
        pauseOverlay.classList.remove("hidden");
        pauseOverlay.setAttribute("aria-hidden", "false");
        resumeButton.focus({ preventScroll: true });
      } else if (settingsContext === "menu") {
        startButton.focus({ preventScroll: true });
      }
    }
    settingsContext = null;
    saveSettings(settings);
  }

  function openCredits() {
    creditsModal.classList.remove("hidden");
    creditsModal.setAttribute("aria-hidden", "false");
    creditsCloseButton.focus({ preventScroll: true });
  }

  function closeCredits(silent = false) {
    if (creditsModal.classList.contains("hidden")) {
      return;
    }
    creditsModal.classList.add("hidden");
    creditsModal.setAttribute("aria-hidden", "true");
    if (!silent) {
      startButton.focus({ preventScroll: true });
    }
  }

  function applySettingsUI() {
    audioToggle.checked = settings.audioEnabled;
    sensitivitySlider.value = settings.sensitivity.toString();
    sensitivityValue.textContent = `${settings.sensitivity.toFixed(2)}×`;
  }

  function updateStartButtonLabel() {
    if (savedSnapshot && savedSnapshot.scene === Scenes.GAME && savedSnapshot.active) {
      startButton.textContent = "Resume / Start New";
    } else {
      startButton.textContent = "Start Game";
    }
  }

  function saveSettings(next) {
    try {
      localStorage.setItem(STORAGE_KEYS.SETTINGS, JSON.stringify(next));
    } catch (error) {
      console.warn("Unable to store settings", error);
    }
  }

  function loadSettings() {
    try {
      const stored = localStorage.getItem(STORAGE_KEYS.SETTINGS);
      if (!stored) {
        return { ...defaultSettings };
      }
      const parsed = JSON.parse(stored);
      return {
        audioEnabled: Boolean(parsed.audioEnabled ?? defaultSettings.audioEnabled),
        sensitivity: clamp(
          Number.parseFloat(parsed.sensitivity) || defaultSettings.sensitivity,
          0.5,
          3
        ),
      };
    } catch (error) {
      console.warn("Unable to read settings", error);
      return { ...defaultSettings };
    }
  }

  function storeGameState(state, sceneOverride) {
    try {
      if (!state) {
        localStorage.removeItem(STORAGE_KEYS.GAME);
        savedSnapshot = null;
        updateStartButtonLabel();
        return;
      }
      const payload = {
        scene: sceneOverride ?? currentScene,
        timestamp: Date.now(),
        ...state,
      };
      localStorage.setItem(STORAGE_KEYS.GAME, JSON.stringify(payload));
      savedSnapshot = payload;
      updateStartButtonLabel();
    } catch (error) {
      console.warn("Unable to store game", error);
    }
  }

  function loadGameState() {
    try {
      const stored = localStorage.getItem(STORAGE_KEYS.GAME);
      if (!stored) {
        return null;
      }
      const parsed = JSON.parse(stored);
      if (!parsed || !parsed.scene) {
        return null;
      }
      return parsed;
    } catch (error) {
      console.warn("Unable to read game state", error);
      return null;
    }
  }
})();

class GameController {
  constructor(canvas, scoreElement) {
    this.canvas = canvas;
    this.ctx = canvas.getContext("2d");
    this.scoreElement = scoreElement;
    this.width = canvas.width;
    this.height = canvas.height;
    this.player = {
      x: this.width / 2,
      y: this.height / 2,
      size: 28,
    };
    this.target = {
      x: 0,
      y: 0,
      size: 20,
    };
    this.inputs = {
      up: false,
      down: false,
      left: false,
      right: false,
    };
    this.baseSpeed = 130;
    this.sensitivity = 1.25;
    this.active = false;
    this.paused = false;
    this.lastTime = 0;
    this.frameHandle = null;
    this.stateListener = null;
    this.stateEmitQueued = false;
    this.stateEmitTimeout = null;
    this.score = 0;
    this.sound = new SoundController();
    this.spawnTarget();
    this.render();
  }

  setStateChangeListener(listener) {
    this.stateListener = listener;
  }

  applySettings(settings) {
    this.setSensitivity(settings.sensitivity);
    this.setAudioEnabled(settings.audioEnabled);
  }

  setSensitivity(value) {
    this.sensitivity = clamp(value, 0.5, 3);
  }

  setAudioEnabled(enabled) {
    this.sound.setEnabled(enabled);
  }

  unlockAudio() {
    this.sound.unlock();
  }

  startNew(settings) {
    this.applySettings(settings);
    this.score = 0;
    this.updateScore();
    this.resetPositions();
    this.active = true;
    this.paused = false;
    this.clearInputs();
    this.lastTime = performance.now();
    this.render();
    this.startLoop();
    this.emitState(true);
  }

  restore(snapshot, settings) {
    this.applySettings(settings);
    this.score = snapshot.score ?? 0;
    this.updateScore();
    if (snapshot.player) {
      this.player.x = clamp(snapshot.player.x, this.player.size / 2, this.width - this.player.size / 2);
      this.player.y = clamp(snapshot.player.y, this.player.size / 2, this.height - this.player.size / 2);
    } else {
      this.resetPlayer();
    }
    if (snapshot.target) {
      this.target.x = clamp(snapshot.target.x, this.target.size / 2, this.width - this.target.size / 2);
      this.target.y = clamp(snapshot.target.y, this.target.size / 2, this.height - this.target.size / 2);
    } else {
      this.spawnTarget();
    }
    this.active = Boolean(snapshot.active);
    this.paused = Boolean(snapshot.paused);
    this.clearInputs();
    this.lastTime = performance.now();
    this.render();
    this.startLoop();
    if (this.paused) {
      this.pause();
    }
    this.emitState(true);
  }

  exportState() {
    if (!this.active) {
      return {
        active: false,
        paused: false,
        score: this.score,
      };
    }
    return {
      active: this.active,
      paused: this.paused,
      score: this.score,
      player: { x: this.player.x, y: this.player.y },
      target: { x: this.target.x, y: this.target.y },
    };
  }

  isActive() {
    return this.active;
  }

  getScore() {
    return this.score;
  }

  pause() {
    if (!this.active) {
      return;
    }
    this.paused = true;
    this.clearInputs();
    this.emitState(true);
  }

  resume() {
    if (!this.active) {
      return;
    }
    this.paused = false;
    this.lastTime = performance.now();
    this.emitState(true);
  }

  stop() {
    this.active = false;
    this.paused = false;
    this.clearInputs();
    if (this.frameHandle) {
      cancelAnimationFrame(this.frameHandle);
      this.frameHandle = null;
    }
    this.emitState(true);
  }

  setDirectionActive(direction, isActive) {
    if (!this.inputs.hasOwnProperty(direction)) {
      return;
    }
    this.inputs[direction] = Boolean(isActive);
  }

  startLoop() {
    if (this.frameHandle) {
      cancelAnimationFrame(this.frameHandle);
    }
    this.frameHandle = requestAnimationFrame((timestamp) => this.step(timestamp));
  }

  step(timestamp) {
    if (!this.active) {
      this.frameHandle = null;
      return;
    }
    const delta = (timestamp - this.lastTime) / 1000 || 0;
    this.lastTime = timestamp;
    if (!this.paused) {
      this.update(delta);
      this.render();
    }
    this.frameHandle = requestAnimationFrame((next) => this.step(next));
  }

  update(deltaSeconds) {
    const vector = { x: 0, y: 0 };
    if (this.inputs.left) vector.x -= 1;
    if (this.inputs.right) vector.x += 1;
    if (this.inputs.up) vector.y -= 1;
    if (this.inputs.down) vector.y += 1;
    if (vector.x !== 0 || vector.y !== 0) {
      const length = Math.hypot(vector.x, vector.y) || 1;
      vector.x /= length;
      vector.y /= length;
      const velocity = this.baseSpeed * this.sensitivity * deltaSeconds;
      this.player.x = clamp(
        this.player.x + vector.x * velocity,
        this.player.size / 2,
        this.width - this.player.size / 2
      );
      this.player.y = clamp(
        this.player.y + vector.y * velocity,
        this.player.size / 2,
        this.height - this.player.size / 2
      );
      this.emitState();
    }
    if (this.checkCollision()) {
      this.score += 1;
      this.updateScore();
      this.spawnTarget();
      this.sound.playCollect();
      this.emitState(true);
    }
  }

  render() {
    const ctx = this.ctx;
    ctx.clearRect(0, 0, this.width, this.height);

    const gradient = ctx.createRadialGradient(
      this.player.x,
      this.player.y,
      10,
      this.player.x,
      this.player.y,
      220
    );
    gradient.addColorStop(0, "rgba(77, 210, 255, 0.45)");
    gradient.addColorStop(1, "rgba(5, 8, 18, 0.85)");
    ctx.fillStyle = gradient;
    ctx.fillRect(0, 0, this.width, this.height);

    ctx.save();
    ctx.shadowColor = "rgba(255, 225, 120, 0.6)";
    ctx.shadowBlur = 20;
    ctx.fillStyle = "#ffe378";
    ctx.beginPath();
    ctx.arc(this.target.x, this.target.y, this.target.size / 2, 0, Math.PI * 2);
    ctx.fill();
    ctx.restore();

    ctx.save();
    ctx.shadowColor = "rgba(77, 210, 255, 0.65)";
    ctx.shadowBlur = 18;
    ctx.fillStyle = "#4dd2ff";
    ctx.beginPath();
    ctx.arc(this.player.x, this.player.y, this.player.size / 2, 0, Math.PI * 2);
    ctx.fill();
    ctx.restore();
  }

  checkCollision() {
    const dx = this.player.x - this.target.x;
    const dy = this.player.y - this.target.y;
    const distance = Math.hypot(dx, dy);
    return distance <= (this.player.size + this.target.size) / 2;
  }

  spawnTarget() {
    const margin = this.target.size;
    this.target.x = randomBetween(margin, this.width - margin);
    this.target.y = randomBetween(margin, this.height - margin);
  }

  resetPlayer() {
    this.player.x = this.width / 2;
    this.player.y = this.height / 2;
  }

  resetPositions() {
    this.resetPlayer();
    this.spawnTarget();
  }

  updateScore() {
    this.scoreElement.textContent = this.score.toString();
  }

  clearInputs() {
    Object.keys(this.inputs).forEach((key) => {
      this.inputs[key] = false;
    });
  }

  emitState(force = false) {
    if (!this.stateListener) {
      return;
    }
    if (force) {
      if (this.stateEmitTimeout) {
        clearTimeout(this.stateEmitTimeout);
        this.stateEmitTimeout = null;
      }
      this.stateEmitQueued = false;
      this.stateListener(this.exportState());
      return;
    }
    if (this.stateEmitQueued) {
      return;
    }
    this.stateEmitQueued = true;
    this.stateEmitTimeout = setTimeout(() => {
      this.stateEmitQueued = false;
      this.stateEmitTimeout = null;
      this.stateListener(this.exportState());
    }, 200);
  }
}

class SoundController {
  constructor() {
    this.enabled = true;
    this.context = null;
  }

  setEnabled(enabled) {
    this.enabled = enabled;
    if (!enabled && this.context && this.context.state === "running") {
      this.context.suspend().catch(() => {});
    }
    if (enabled) {
      this.unlock();
    }
  }

  unlock() {
    const AudioCtx = window.AudioContext || window.webkitAudioContext;
    if (!AudioCtx) {
      return;
    }
    if (!this.context) {
      this.context = new AudioCtx();
    } else if (this.context.state === "suspended") {
      this.context.resume().catch(() => {});
    }
  }

  playCollect() {
    if (!this.enabled) {
      return;
    }
    this.unlock();
    if (!this.context) {
      return;
    }
    const ctx = this.context;
    const now = ctx.currentTime;
    const oscillator = ctx.createOscillator();
    const gain = ctx.createGain();
    oscillator.type = "triangle";
    oscillator.frequency.setValueAtTime(660, now);
    oscillator.frequency.exponentialRampToValueAtTime(990, now + 0.18);
    gain.gain.setValueAtTime(0.0001, now);
    gain.gain.linearRampToValueAtTime(0.28, now + 0.03);
    gain.gain.exponentialRampToValueAtTime(0.0001, now + 0.4);
    oscillator.connect(gain);
    gain.connect(ctx.destination);
    oscillator.start(now);
    oscillator.stop(now + 0.4);
  }
}

function clamp(value, min, max) {
  return Math.min(Math.max(value, min), max);
}

function randomBetween(min, max) {
  return Math.random() * (max - min) + min;
}
