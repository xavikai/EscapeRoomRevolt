# Third-Party Notices

Inventory of every non-authored asset bundled with this template, its origin and its redistribution license. Required before publishing (`P0-005` in `ROADMAP.md`). Entries marked **PENDIENTE** are asset files that exist in the project but whose origin/license could not be determined from the repository alone — they need to be confirmed by the project owner before this document can be considered complete, and the checklist item in `COMMERCIAL_READINESS.md` stays unchecked until then.

## Audio

| Archivo | Origen | Autor / licencia | Redistribuible |
|---|---|---|---|
| `Assets/_EscapeRoomTemplate/Audio/BGM/soundtrack01.mp3` | PENDIENTE | PENDIENTE | PENDIENTE |
| `Assets/_EscapeRoomTemplate/Audio/BGM/soundtrack02.mp3` | PENDIENTE | PENDIENTE | PENDIENTE |
| `Assets/_EscapeRoomTemplate/Audio/Footsteps/footstepWood01.wav` | PENDIENTE | PENDIENTE | PENDIENTE |
| `Assets/_EscapeRoomTemplate/Audio/Footsteps/footstepWood02.wav` | PENDIENTE | PENDIENTE | PENDIENTE |
| `Assets/_EscapeRoomTemplate/Audio/Voice/audio01.mp3` | PENDIENTE | PENDIENTE | PENDIENTE |

## Materiales y texturas

`Assets/_EscapeRoomTemplate/Art/Materials/*.mat` son materiales URP procedurales (color/rugosidad/metálico sobre el shader `Universal Render Pipeline/Lit`), sin texturas ni imágenes de terceros. No requieren entrada de licencia.

## Fuentes

No hay ninguna fuente propia del proyecto en uso — ningún `Font`/`FontAsset` está asignado en el menú (`MenuThemeSettings.titleFont`/`bodyFont` quedan vacíos por defecto) ni en el HUD. Las únicas fuentes `.ttf` presentes en el repositorio (`Assets/TextMesh Pro/Examples & Extras/Fonts/*`, `Assets/TextMesh Pro/Fonts/LiberationSans.ttf`) forman parte del paquete oficial de Unity TextMesh Pro (importado como Essentials/Examples) y no son contenido de terceros — se rigen por la licencia del propio paquete de Unity. Si nunca se les asigna un uso real, considera eliminar la carpeta `Examples & Extras` para reducir el tamaño del paquete publicado.

## Paquetes de Unity

Todas las dependencias en `Packages/manifest.json` son paquetes oficiales `com.unity.*`, cubiertos por la licencia de Unity y no requieren aviso de terceros independiente:

`com.unity.ai.navigation`, `com.unity.inputsystem`, `com.unity.multiplayer.center`, `com.unity.render-pipelines.universal`, `com.unity.ugui`, `com.unity.xr.interaction.toolkit`, `com.unity.xr.management`, `com.unity.xr.openxr`, más los módulos estándar del motor.

**Excepción a eliminar antes de empaquetar:** `com.coplaydev.unity-mcp` es una dependencia de desarrollo (el puente MCP usado para editar el proyecto asistido por IA durante esta sesión). No aporta nada al comprador final y no debería ir incluida en el paquete comercial — quitarla de `manifest.json` antes de exportar.

## Modelos 3D

No se han encontrado modelos `.fbx`/`.obj` de terceros en el proyecto en el momento de este inventario; toda la geometría vista en las escenas de demo (`ShowcaseMuseum`, `SurvivalHorrorDemo`, `LockedOffice`) es geometría primitiva de Unity (`Cube`, `Cylinder`...) usada como *blockout*, marcada explícitamente como sustituible (`Placeholder_ReplaceMe`, `ReplaceableModelSlot`). Si se añade arte final con modelos de terceros, añadir su entrada aquí antes de publicar.
