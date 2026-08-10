# Escape Room / Survival Horror Framework — Commercial Readiness

For what's built and what's pending, see the living document: **[ROADMAP.md](ROADMAP.md)**. The latest Escape Room verification is **[AUDITORIA_ESCAPE_ROOM_2026-08-09.md](AUDITORIA_ESCAPE_ROOM_2026-08-09.md)**. This file stays a short, stable pre-publish checklist and authoring workflow — it doesn't track day-to-day status.

## Flujo recomendado para crear un nivel

1. Instanciar `GameManager.prefab` y `Player_PC.prefab` (o `Player_VR.prefab` tras `Setup > Create or Update VR Player Prefab`).
2. Crear `InventoryItemData`, `PuzzleDefinition`, `HintData` y perfiles de Survival necesarios desde `Escape Room Framework > Create`.
3. Mantener la lógica en el objeto raíz y sustituir únicamente el hijo visual o `World Prefab` (vía `ReplaceableModelSlot`).
4. Asignar identificadores persistentes únicos (`SaveId`, `ItemId`, `PersistentId` se generan solos al crear desde el menú del framework).
5. Ejecutar **Escape Room Framework > Validation > Run Framework Smoke Tests** y **Validate Current Scene**.
6. Probar guardar, cerrar el juego y cargar cada escena incluida en Build Settings.

## Checklist de publicación

- [x] La consola está limpia en Edit Mode y Play Mode tras actualizar las llamadas de Editor obsoletas de Unity 6.4. La falta de un runtime OpenXR activo puede generar mensajes externos al probar VR en este equipo.
- [x] No existen Canvas heredados ni referencias rotas — UI Toolkit en todas las pantallas.
- [x] Todas las escenas jugables contienen cámara, iluminación, GameManager y jugador.
- [x] Todos los puzles de `ShowcaseMuseum`, incluidas las fases internas multi-stage, tienen `PuzzleDefinition` y `HintData` apropiados.
- [x] Los `SaveId`, `ItemId` y `PersistentId` son únicos.
- [x] Los modelos pueden sustituirse sin modificar scripts ni colliders lógicos (`ReplaceableModelSlot`).
- [x] La escena `SurvivalHorrorDemo` es una vertical slice completa y verificada: objetivos encadenados, enemigo, escondites, evidencias, checkpoints y final.
- [ ] **Cobertura parcial** — hay 12/12 tests EditMode y 13/13 PlayMode reales pasando. El objetivo PlayMode ya está superado con puzles, autoría dinámica, peligros móviles, temporizador HUD, menú y Save/Load; falta ampliar EditMode hasta 20 (`P0-001`).
- [ ] **Sin `ThirdPartyNotices.md`** — falta inventariar el origen/licencia de los audios de `Assets/_EscapeRoomTemplate/Audio` y confirmar que son redistribuibles (`P0-005`).
- [ ] **Localización parcial** — menú principal/pausa y selector ES/EN funcionan; HUD, inventario, puzles y prompts todavía contienen literales (`P0-007`).
- [ ] VR es funcionalmente completo pero no ha pasado QA en hardware real (`VR-007`/`SH-016`).

Última revisión: 10 de agosto de 2026, sesión de verificación en vivo dentro del Editor (Play Mode + validadores), no solo compilación.
