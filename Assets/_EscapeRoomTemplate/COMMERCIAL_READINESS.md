# Escape Room / Survival Horror Framework — Commercial Readiness

## Arquitectura entregada

- UI Toolkit unificado: menú principal, HUD, interacción, inventario, examinador, notas, keypad, subtítulos, pausa, ajustes, rebinding de controles y Save/Load.
- Persistencia versionada por `ISaveable`, con tres ranuras manuales, metadatos, miniaturas y escritura atómica.
- Puzles configurables con `PuzzleDefinition` y lógica especializada en componentes reutilizables.
- Inventario con cantidades, hotbar, examen 3D, equipamiento y combinación guiada.
- Supervivencia mediante linterna, batería, cordura y eventos de terror configurables.
- Pistas progresivas mediante `HintData`, activación automática y solicitud manual con `H`.
- Outline URP mediante Renderer Feature y rendering layers.

## Flujo recomendado para crear un nivel

1. Instanciar `GameManager.prefab` y `Player_PC.prefab`.
2. Crear `InventoryItemData`, `PuzzleDefinition`, `HintData` y perfiles de Survival necesarios.
3. Mantener la lógica en el objeto raíz y sustituir únicamente el hijo visual o `World Prefab`.
4. Asignar identificadores persistentes únicos.
5. Ejecutar **Escape Room Framework > Validation > Validate Current Scene**.
6. Probar guardar, cerrar el juego y cargar cada escena incluida en Build Settings.

## Checklist de publicación

- [x] La consola está limpia en Edit Mode y Play Mode.
- [x] No existen Canvas heredados ni referencias rotas.
- [x] Todas las escenas contienen cámara, iluminación, GameManager y jugador.
- [x] Todos los puzles tienen `PuzzleDefinition` y `HintData` apropiados.
- [x] Los `SaveId`, `ItemId` y `PersistentId` son únicos.
- [x] Los modelos pueden sustituirse sin modificar scripts ni colliders lógicos.
- [ ] Las miniaturas, iconos, audios y fuentes tienen licencia redistribuible.
- [x] La escena Showcase demuestra interacción, combinación, puzles, guardado, linterna, cordura y un evento ambiental.

## QA verificada — 02/08/2026

- `MainMenu`, `ShowcaseMuseum` y `LockedOffice`: 0 errores de compilación; las escenas jugables no contienen Canvas heredados.
- Build Profile: `MainMenu` primero, seguido de las dos escenas jugables.
- Recorrido automatizado: tres puzles resueltos, puerta con llave, evento de terror y restauración Save/Load completa.
- Linterna: recarga con `batteries`, consumo de una unidad y carga restaurada al 100 %.
- UI: 0 Canvas; HUD y menús servidos por UI Toolkit.
- Inventario: combinación guiada y selector contextual `OfferCompatible` verificados en Play Mode.
- VR: `Player_VR.prefab` generado con XRI; 47 interactuables preparados en Showcase y 26 en LockedOffice sin colliders registrados por duplicado.
- Controles: rebinding de teclado, persistencia y restauración de valores predeterminados verificados en Play Mode.
- Validadores comerciales, Save IDs y smoke tests: PASS sin advertencias en ambas escenas jugables.
- Pendiente antes de publicar: sustituir arte temporal y confirmar licencias redistribuibles de iconos, fuentes, audio y modelos finales.
