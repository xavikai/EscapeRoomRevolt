# Escape Room Revolt 0.2.1

Update for the first class sessions. Same Unity version (`6000.4.9f1`) and project layout as 0.2.0.

## What's new

**Menu images without UI Toolkit.** The menu theme (`Menu Theme Settings`) has a new **Imágenes** section:

| Field | What it does |
|---|---|
| `Screen Background` | Full-screen image behind the main menu (cropped, never distorted). |
| `Screen Background In Pause` | Also show it behind the pause menu. |
| `Panel Image` + `Panel Image Slice` | Image for the menu frame; the slice keeps decorated borders crisp. |
| `Button Image` + `Button Hover Image` + `Button Image Slice` | Images for every menu button and its mouse-over state. |

Images must be imported as `Sprite (2D and UI)`. High-contrast mode hides them for readability.

**Interactive guide** (`EscapeRoom/Framework/docs/guia/index.html`, CA/ES/EN):

- The start section now begins with the template already open in Unity. Unity installation and editor navigation are no longer explained.
- Unity screenshots for every menu and Inspector field used, with click-to-zoom.
- New phase **Voices, hints, time and endings**: voice and subtitle zones, automatic hint zones, zones that trigger any action, countdown, moving hazard, endings, objectives and other useful pieces.
- Menu design: steps for the new theme images.

## Fixes

- The Note Inspector no longer throws an error that hid the `Note Content` field.
- The Game Over Timer wizard's default HUD label is now translated in all three languages.

## Upgrading from 0.2.0

No scene or asset changes are needed. Existing themes keep working; the new image fields start empty.
