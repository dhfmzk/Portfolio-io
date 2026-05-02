# Player 1 Portfolio MVP Design

## Purpose

Build a small Unity WebGL portfolio that visitors can play instead of scroll. The project should feel like a compact side-scrolling gallery: the visitor moves through a stage, discovers portfolio objects, and opens focused project panels with a single interaction key.

The goal is not to make a full platformer. The game layer exists to make the portfolio memorable while keeping the portfolio content readable within 30-90 seconds.

## Inferred Intent

The repo history and current scene point to a playful, game-native portfolio direction:

- README frames the project as `Loading... Player 1's Portfolio`.
- The stated concept is a `side-scrolling art gallery style portfolio project built with Unity WebGL`.
- The current scene already has `Jump`, `Interaction`, `Backward`, `Foward`, `Stage 00`, and a key indicator UI.
- The deployment path now targets Unity WebGL on GitHub Pages, which favors a lightweight, self-contained experience.

The intended user experience is: "I walk through the developer's work as a little playable world."

## Product Shape

The first screen is the playable scene. There is no marketing landing page before the game.

The visitor starts in a short intro area, learns controls through the existing control indicator, then walks through a single horizontal gallery stage. Portfolio sections are represented as interactable exhibits.

Initial sections:

1. Intro spawn
   - Shows the portfolio title and a compact creator label.
   - Teaches movement and interaction through UI, not long prose.

2. About exhibit
   - Explains who the developer is and what kind of work they do.
   - Keeps text short enough to read in one modal/panel.

3. Project gallery
   - Contains three to five project exhibits.
   - Each exhibit opens a detail panel with title, role, stack, short description, and links.

4. Skill wall
   - Shows technology groups as readable tags.
   - Prioritizes context of use over a long tool list.

5. Contact gate
   - Ends the stage with email/GitHub/blog/LinkedIn style links.
   - Gives a clear final action after exploration.

## MVP Scope

The MVP is complete when a visitor can:

- Open the GitHub Pages URL and load the WebGL build.
- Move left/right.
- Jump.
- See a camera follow the playable character.
- Approach exhibits.
- Press `E` to open a portfolio panel.
- Close the panel and continue moving.
- Visit at least one About exhibit, three Project exhibits, one Skill wall, and one Contact gate.

## Out Of Scope For MVP

These are intentionally excluded from the first playable version:

- Enemies, combat, score, lives, damage, checkpoints, or fail states.
- Inventory, quests, branching dialogue, save/load, or account features.
- Procedural levels or multiple scenes.
- Mobile-first controls.
- CMS-style remote content editing.
- Heavy animation systems or cinematic cutscenes.

## Interaction Model

Controls:

- `A` / left arrow: move left.
- `D` / right arrow: move right.
- `Space`: jump.
- `E`: interact with the nearest exhibit.
- `Esc` or `E`: close an open panel.

When a panel is open:

- Player movement pauses.
- The panel receives input focus.
- External links are shown as explicit buttons.
- Closing the panel returns to the previous gameplay state.

## Content Model

Portfolio content should start as Unity-authored data so the project stays simple.

Recommended data unit: `PortfolioExhibit`.

Fields:

- `id`
- `category`: About, Project, Skill, Contact
- `title`
- `subtitle`
- `body`
- `stackTags`
- `links`
- `displayOrder`

For MVP, this can be represented as ScriptableObjects or serialized MonoBehaviour fields on exhibit prefabs. ScriptableObjects are preferred once there are more than three project entries because content can be edited without opening scene object details.

## Scene Architecture

Recommended scene structure:

```text
Main
|- Systems
|  |- PlayerInput
|  |- InteractionSystem
|  `- PortfolioPanelController
|- World
|  |- Ground
|  |- CameraBounds
|  `- Exhibits
|- Player
|- UI
|  |- ControlIndicator
|  |- PortfolioPanel
|  `- DebugOverlay
`- Build
   `- WebGLPagesBuild editor script
```

The current debug overlay can stay during development. It should be disabled or hidden behind a development flag before a public portfolio release.

## Visual Direction

Use a restrained pixel/arcade style that keeps text legible:

- Dark or neutral stage background.
- Simple platform silhouettes.
- Exhibit markers that clearly invite interaction.
- Small, readable UI panels.
- Minimal animation: idle, walk, jump, hover highlight, panel open/close.

The visual personality should feel handmade and playful, but the content panels should remain professional and scannable.

## Technical Approach

Use Unity's built-in 2D physics and UI stack.

Recommended implementation units:

- `PlayerController`: handles movement and jump.
- `PlayerInputReader`: converts keyboard state into movement/interact signals.
- `InteractableExhibit`: marks an exhibit and exposes its portfolio data.
- `InteractionSystem`: tracks nearest exhibit and opens panels.
- `PortfolioPanelController`: renders exhibit content and external links.
- `ControlIndicatorUI`: reflects input state in the existing key UI.
- `CameraFollow`: follows the player with simple bounds.

Avoid introducing large frameworks until the core loop is playable.

## Deployment Approach

Keep the low-overhead deployment loop already added:

- Build locally with `Portfolio > Build WebGL for GitHub Pages`.
- Output to `docs/`.
- Publish GitHub Pages from `main` and `/docs`.
- Keep WebGL compression disabled for GitHub Pages compatibility.

This makes every milestone deployable without adding GitHub Actions, Unity license automation, or external hosting.

## Success Criteria

The MVP succeeds if:

- It loads from GitHub Pages on desktop Chrome/Safari/Firefox.
- First meaningful interaction is possible within a few seconds after load.
- A visitor can understand who the developer is and inspect core projects without reading separate documentation.
- The playful movement does not block access to the portfolio content.
- The build remains small enough to commit and host comfortably in GitHub Pages.

## Risks

- Unity WebGL build size can grow quickly. Keep assets small and avoid unnecessary packages.
- PWA service worker caching can make updated builds look stale. Test hard refresh and version changes after deployment.
- Text-heavy portfolio panels can feel awkward in a game. Keep copy short and link out for details.
- Mobile support is limited for Unity WebGL. The first public target should be desktop.

## First Implementation Milestone

The first milestone should produce a playable vertical slice:

- One horizontal stage.
- Basic player movement.
- Camera follow.
- Three interactable draft exhibits with temporary content.
- One reusable portfolio detail panel.
- WebGL build regenerated into `docs/`.

This milestone proves the intended portfolio format before investing in final art or full content.
