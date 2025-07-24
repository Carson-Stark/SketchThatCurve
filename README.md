# Calculus Multiplayer Graphing Game

A multiplayer web game that allows players to practice calculus concepts and compete against their classmates in a format similar to Kahoot. Players sketch the graph of a function, and the game automatically grades their sketch based on accuracy, players are ranked on a leaderboard. The game was created as part of a high school calculus project. The project was built with Unity and Photon (PUN) networking.

## Features

- **Multiplayer Support:** Join lobbies and play with others using Photon (PUN) networking.
- **Graphing Gameplay:** Answer calculus and graph-based questions by drawing or submitting graphs.
- **Leaderboard:** Track player scores and rankings in real time.
- **Lobby System:** Create or join game lobbies before starting a match.
- **Main Menu and UI:** Intuitive navigation with main menu, tips, and player listings.
- **Customizable Questions:** Easily extend or modify the question set.
- **Audio and Visual Assets:** Includes themed backgrounds, fonts, and sound effects.

## Installation

1. **Clone the Repository:**
   ```sh
   git clone https://github.com/yourusername/calculus-multiplayer-game.git
   ```
2. **Open in Unity:**
   - Use Unity Hub or open the project folder directly in Unity (recommended version: 2019.4 LTS or later).
3. **Install Dependencies:**
   - Photon Unity Networking (PUN) v2.30 (April 13, 2021) is included in `Assets/Photon/`.
   - Input field plugin for WebGL/mobile is in `Assets/input-field-plugin-for-unity-mobile-webgl-master/`.
4. **Photon Account Integration:**
   - Open Unity, then go to **Window → Photon Unity Networking** (shortcut: ALT+P) to launch the PUN Wizard.
   - Register or log in at [Photon Dashboard](https://dashboard.photonengine.com).
   - Create a new Photon App (type: PUN), copy your App ID.
   - In Unity, use the PUN Wizard to paste your App ID into the PhotonServerSettings.
   - Save your settings. The project is now linked to your Photon Cloud account.
5. **Build and Run:**
   - Open the main menu scene (see below).
   - Press Play in the Unity Editor, or build for your target platform.

## Photon Networking

- **Current Version:** Photon Unity Networking (PUN) v2.30 (April 13, 2021)
- **Integration:** Use the PUN Wizard (Window → Photon Unity Networking) to set up your Photon Cloud App ID.
- **Upgrading Photon:** It is recommended to periodically upgrade to the latest PUN version for security and feature updates.
    - To upgrade, manually delete the entire `Assets/Photon/` folder, then import the latest PUN package from the [Photon website](https://www.photonengine.com/en-US/PUN).
    - **Warning:** Upgrading may require changes to your networking scripts to match new APIs or behaviors.

## Usage

- **Main Menu:** Start the game, view tips, or join a lobby.
- **Lobby:** Wait for other players, then start the game together.
- **Game:** Answer graphing questions, submit your answers, and compete for the highest score.
- **Leaderboard:** View rankings at the end of the game.

## Project Structure

```
Assets/
  Images/                # UI and background images
  input-field-plugin.../ # Third-party input field plugin for WebGL/mobile
  ParrelSync/            # Multi-instance testing tool
  Photon/                # Photon Unity Networking (PUN) assets
  Prefabs/               # Reusable game objects (see below)
  Scenes/                # Main Unity scenes
  Scripts/               # Core gameplay and UI scripts
  UNetScripts/           # Legacy UNet networking scripts
Library/                 # Unity-generated, ignored by git
Logs/                    # Editor and build logs
Packages/                # Unity package manifest
ProjectSettings/         # Unity project settings
```

### Key Scenes

- **Main Menu.unity:** Entry point, navigation, and tips.
- **GameLobby.unity:** Multiplayer lobby for player matching.
- **Game.unity:** Main gameplay scene.

### Important Prefabs

- **LineMaker.prefab:** Handles graph drawing.
- **PlayerListing.prefab:** Displays players in the lobby.
- **Tips.prefab:** UI for gameplay tips.
- **Place.prefab:** Likely used for graph points or markers.

### Core Scripts

- **GameManager.cs:** Central game logic and state management.
- **GameSettings.cs:** Stores and manages game configuration.
- **QuestionManager.cs, Question.cs:** Handles question logic and data.
- **DisplayGraph.cs, Graphing.cs:** Graph rendering and visualization.
- **GraphSubmission.cs:** Manages player graph submissions.
- **Leaderboard.cs:** Score and ranking logic.
- **Lobby.cs, MainMenu.cs:** UI and flow for lobby and main menu.
- **Functions.cs:** Utility or math functions for gameplay.

### Networking Scripts

- **Photon:** Integrated via `Assets/Photon/` for modern multiplayer.
- **UNetScripts:** Contains `Client.cs`, `Lobby_unet.cs`, `MainMenu_unet.cs` for legacy UNet networking.
