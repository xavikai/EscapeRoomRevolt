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

El proyecto incluye recursos de TextMesh Pro y fuentes de sus muestras. Estar dentro de una muestra de Unity no demuestra que un recurso carezca de condiciones propias. Conserva los avisos incluidos, entre ellos `Assets/TextMesh Pro/Examples & Extras/Fonts/Roboto-Bold - License.txt`, e inventaría las fuentes y atlas que realmente exportes. La revisión de procedencia de toda la entrega sigue pendiente.

## Paquetes de Unity

El manifiesto incluye estos paquetes oficiales `com.unity.*`. Conserva los archivos de licencia y avisos de terceros que acompañen a cada paquete y revisa también sus muestras importadas; esta lista no sustituye esa revisión:

`com.unity.ai.navigation`, `com.unity.inputsystem`, `com.unity.multiplayer.center`, `com.unity.render-pipelines.universal`, `com.unity.ugui`, `com.unity.xr.interaction.toolkit`, `com.unity.xr.management`, `com.unity.xr.openxr`, más los módulos estándar del motor.

**Excepción a eliminar antes de empaquetar:** `com.coplaydev.unity-mcp` es una dependencia de desarrollo (el puente MCP usado para editar el proyecto asistido por IA durante esta sesión). No aporta nada al comprador final y no debería ir incluida en el paquete comercial — quitarla de `manifest.json` antes de exportar.

La exclusión de MCP se hace en la copia de distribución, conservando la conexión del proyecto de desarrollo. Mantén las dependencias XR en la edición PC mientras el ensamblado Player las referencie.

### Muestras XRI modificadas

El 09/09/2026 se corrigieron nueve referencias al asset de acciones de manos en `Assets/Samples/XR Interaction Toolkit/3.3.0/XR Interaction Simulator/XR Interaction Simulator.prefab`. Es una modificación local de la muestra importada, no un recurso original del template. Conserva su procedencia y los avisos del paquete XRI en la distribución que la incluya.

## Modelos 3D

No se han encontrado modelos `.fbx`/`.obj` de terceros en el proyecto en el momento de este inventario; toda la geometría vista en las escenas de demo (`ShowcaseMuseum`, `SurvivalHorrorDemo`, `LockedOffice`) es geometría primitiva de Unity (`Cube`, `Cylinder`...) usada como *blockout*, marcada explícitamente como sustituible (`Placeholder_ReplaceMe`, `ReplaceableModelSlot`). Si se añade arte final con modelos de terceros, añadir su entrada aquí antes de publicar.
