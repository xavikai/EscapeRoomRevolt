# Manual de usuario - Escape Room Framework

**Empieza aquí:** [guía práctica de Escape Room PC/VR](Documentation/ESCAPE_ROOM_QUICKSTART.md), con recetas de cada mecánica, guardado, preparación VR y matriz de aceptación. Consulta [la auditoría del 9 de septiembre de 2026](AUDITORIA_ESCAPE_ROOM_2026-09-09.md) para los resultados actuales; el informe de agosto es histórico.

Esta guía cubre el flujo de trabajo para diseñadores. La arquitectura y las APIs están documentadas en [PROGRAMMING_GUIDE.md](PROGRAMMING_GUIDE.md). La referencia exhaustiva, con tutoriales, ejemplos y resolución de problemas, está en [DOCUMENTACIO_COMPLETA.md](DOCUMENTACIO_COMPLETA.md). El estado verificado de cada sala se encuentra en [AUDITORIA_ESCAPE_ROOM_2026-08-09.md](AUDITORIA_ESCAPE_ROOM_2026-08-09.md).

## 1. Menú del framework

Todas las herramientas soportadas están en `Escape Room Framework`:

- `Configuration`: selecciona el perfil Escape Room, Survival Horror o una combinación personalizada.
- `Setup`: instala instancias seguras del Game Manager o jugador y genera las escenas/prefabs de plataforma.
- `Create`: crea interactuables, puzles, hotspots de examen, triggers y componentes de flujo sin modificar otros objetos. `Multi-Stage Puzzle` crea un grupo de dos puzles físicamente separados y visibles; la lista admite tantos puzles como se necesiten y permite terminarlos en orden libre u obligatorio antes de activar una única puerta o mecanismo. `Number Wheels Puzzle` abre un configurador para elegir entre 2 y 8 ruedas y definir la combinación. Después puede redimensionarse desde `NumberWheelsPuzzleAuthoring > Rebuild wheels and layout`; carcasa, título, condiciones, botones y cámara se adaptan sin perder la definición ni los eventos del puzle. En PC se abre la vista enfocada y se pulsa con el botón izquierdo sobre ▲/▼; `E` solo abre el panel o actúa como alternativa de interacción. En VR se usan los controles ▲/▼ equivalentes con el gatillo. En `Create > Flow`, `Moving Hazard (Any Direction)` crea por separado una pared, techo, suelo, plataforma o volumen móvil entre dos marcadores 3D, mientras que `Game Over Timer (HUD)` crea un límite de tiempo opcional visible en la interfaz. `Pipe Puzzle` sigue requiriendo completar su presentación interactiva.
- Tanto `MovingHazard.StartHazard` como `GameOverTimer.StartTimer` pueden conectarse desde el Inspector a un interruptor (`InteractableTrigger`) o a una zona de paso (`EventTriggerZone`). La zona admite filtro por tag, modo de un solo uso, eventos de entrada/salida y rearme mediante `ResetZone`.
- `Demo`: abre las escenas de ejemplo tras ofrecer guardar los cambios actuales. `Apply Escape Room Closure Fixes` reaplica de forma idempotente definiciones, payoff de Pipe, prompts y nomenclatura semántica en `ShowcaseMuseum` y `LockedOffice`.
- `Validation`: comprueba IDs, dependencias, escena activa y preparación comercial.
- `Maintenance`: previsualiza problemas antes de permitir una reparación con Undo.
- `Documentation`: abre este manual, la guía de programación, la documentación completa o localiza el HUD de UI Toolkit.

Los antiguos generadores destructivos y la instalación automática de paquetes ya no forman parte del menú.

### Elegir el género del proyecto

- `Configuration/Use Escape Room Profile`: mantiene interacción, inventario, puzles, pistas, objetivos, finales, Save/Load, PC y VR. Desactiva y oculta linterna, batería, estabilidad/cordura y eventos de terror.
- `Configuration/Use Survival Horror Profile`: activa todas las mecánicas comunes y también linterna, cordura y eventos de terror.
- `Configuration/Use Custom Hybrid Profile`: permite escoger por separado `Flashlight`, `Sanity` y `Horror Events` en `GenreFeatureSettings.asset`.

Las escenas de demostración `ShowcaseMuseum` y `ShowcaseMuseumVR` incluyen una excepción local para la linterna, porque la sala 3 demuestra la combinación de una linterna vacía con baterías. Por eso la linterna funciona en esas escenas incluso con el perfil `Escape Room`; las escenas nuevas siguen respetando el perfil y no activan la linterna salvo que se use `Custom Hybrid`.

El cambio se aplica al iniciar Play de nuevo. Los componentes opcionales pueden seguir presentes en escenas y prefabs: el perfil evita que se ejecuten o aparezcan en la UI cuando no corresponden.

## 2. Escena jugable

1. Abre o crea una escena.
2. Usa `Setup/Instantiate Game Manager`.
3. Usa `Setup/Instantiate PC Player` o coloca `Player_VR`.
4. Crea interactuables desde `Create/Interactables` y configura sus campos en el Inspector.
5. Ejecuta `Validation/Validate Current Scene`.

El modelo visual de los prefabs reemplazables vive bajo un `ModelSocket`. Sustituye únicamente sus hijos visuales para conservar colliders, IDs, eventos y programación.

## 3. Menú inicial y fin del juego

Usa `Setup/Create or Update Main Menu Scene` para generar el menú inicial. Queda primero en Build Settings, salvo que ya exista una `Intro` habilitada: entonces se conserva el orden `Intro → MainMenu`.

`Nueva partida` carga la escena indicada por `Resources/GameFlowSettings.asset`. En el perfil de muestra Escape Room debe apuntar a `ShowcaseMuseum`; cámbiala explícitamente cuando empieces el juego definitivo.

Para terminar una partida puedes:

- crear un `Objective Set` y asignarlo a un `ObjectiveManager`;
- crear `Create/Flow/Game End Trigger` y conectarlo a un puzle o volumen;
- llamar a `GameFlowManager.CompleteGame` o `FailGame` desde código.

La pantalla final permite reintentar, volver al menú principal o salir.

## 4. Personalizar el aspecto y los textos del menú

### Cambiar colores, fuentes y logo

1. En el panel de Proyecto, botón derecho → `Create > Escape Room Framework > Menu Theme Settings`. Dale un nombre, por ejemplo `MiTemaDeMenu`.
2. En el Inspector del nuevo asset, ajusta los colores (fondo del panel, acento, título, botones) y, si quieres, arrastra una fuente ya importada (`.ttf`/`.otf`) en `Title Font`/`Body Font` y una imagen en `Logo`.
3. En `MainMenu.unity`, selecciona `MainMenuUI`. En una escena jugable, selecciona `MenuUI`, dentro del `GameManager`. Ambos tienen el componente `UI Toolkit Menu Controller`; arrastra tu asset a su campo `_theme`.
4. Si tienes varias escenas jugables con instancias independientes del `GameManager`, asigna el mismo asset en cada una o en el prefab compartido.
5. Entra en Play — el menú ya usa tu paleta, tipografías y logo. Sin asignar nada, el menú conserva el diseño original de la plantilla.

Si prefieres editar directamente el archivo de estilos en vez de crear un asset, `EscapeRoomMenu.uss` tiene los colores más repetidos como variables al principio del fichero (`--color-accent`, `--color-text`...), así que cambiar la paleta base es editar unas pocas líneas en vez de buscar cada color suelto.

El propio jugador puede activar un modo de alto contraste desde Ajustes; ese modo siempre tiene prioridad sobre tu tema, para que la accesibilidad nunca dependa de la personalización visual.

### Diseñar los botones a partir de imágenes

Puedes usar imágenes propias para el fondo de los botones. En la versión actual, `Menu Theme Settings` controla colores, fuentes y logo, pero las imágenes de los botones se asignan desde `EscapeRoomMenu.uss`.

Prepara en una carpeta propia, por ejemplo `Assets/UI/Menu/`, una imagen para cada estado:

- `ButtonNormal.png`: estado normal;
- `ButtonHover.png`: al pasar el ratón por encima;
- `ButtonPressed.png`: mientras se pulsa;
- opcionalmente, `ButtonDisabled.png`: botón desactivado.

Recomendaciones:

- usa PNG con transparencia cuando sea necesario;
- conserva la misma proporción en todas las variantes;
- no dibujes el texto dentro de la imagen: el texto lo genera el menú y puede cambiarse mediante el catálogo de localización;
- importa las imágenes como `Sprite (2D and UI)`;
- si la imagen tiene un marco que debe conservar las esquinas al cambiar de tamaño, prepárala para 9-slice.

Para asignarlas:

1. Abre `Assets/_EscapeRoomTemplate/UI/Toolkit/EscapeRoomMenu.uss` con UI Builder.
2. Selecciona el selector `.menu-button` y asigna la imagen normal en **Background > Image**.
3. Selecciona o crea `.menu-button:hover` y asigna la imagen hover.
4. Selecciona o crea `.menu-button:active` y asigna la imagen pulsada.
5. Si tienes botones desactivables, configura también `.menu-button:disabled`.

Es preferible asignar las imágenes desde UI Builder para que Unity escriba correctamente las referencias de los assets. Si el botón conserva un color por debajo de la imagen, pon el color de fondo del `Menu Theme Settings` con alfa 0 o utiliza un fondo opaco en la propia imagen.

Mantén el texto separado de la imagen. Así seguirán funcionando las traducciones y el modo de alto contraste.

### Cambiar los textos que aparecen

Los textos del menú principal y del menú de pausa (título de cada pantalla y sus botones) viven en un catálogo editable sin tocar código:

1. Selecciona `Assets/_EscapeRoomTemplate/Resources/DefaultLocalizationCatalog.asset`.
2. En el Inspector verás una lista de entradas; cada una tiene una clave (el texto español original, por ejemplo `"Nueva partida"`) y una lista de traducciones por idioma.
3. Para cambiar un texto, edita el campo `Text` de la fila `es` de la entrada correspondiente.
4. Para añadir un idioma (o completar las traducciones al inglés que ya incluye), añade una fila nueva con su código (`en`, `fr`...) y su traducción — aparecerá automáticamente en el desplegable de idioma de Ajustes, sin tocar ningún script.

**Importante**: por ahora este catálogo solo cubre el menú principal y el de pausa. El resto de textos del juego (HUD, inventario, notas, mensajes de puzles, prompts de interacción como "Amagar-se" o "Sortir") todavía están escritos directamente en el código C# de cada sistema — cambiarlos significa editar ese texto en el script correspondiente. Ampliar el catálogo a todo el juego es trabajo pendiente (`P0-007` en `ROADMAP.md`).

## 5. Inventario

El inventario se abre con `I` en PC. El almacenamiento ya no está limitado por la barra rápida.

- Selecciona un objeto para ver solo las acciones válidas: leer, sostener/equipar, consumir, examinar, combinar o soltar.
- `ACCESO RÁPIDO N` asigna el objeto a la posición rápida activa.
- Las teclas `1-4`, la rueda del ratón o los hombros del mando cambian el acceso rápido.
- Al interactuar con una cerradura en modo `Offer Compatible`, la interfaz muestra únicamente objetos válidos. No utiliza ninguno sin confirmación.

Cada puerta o receptor puede cambiar su política a `Selected Only` o `Auto Use Single` desde el Inspector.

### Examinar un objeto en 3D

La plantilla permite inspeccionar en 3D un objeto que ya está en el inventario:

1. Abre el inventario con `I`.
2. Selecciona el objeto.
3. Pulsa `EXAMINAR`.
4. Arrastra sobre la imagen del objeto para rotarlo.
5. Usa la rueda del ratón para acercar o alejar la vista.
6. Pulsa `ESC` o `CERRAR` para volver al inventario.

Para que el botón aparezca activo, el `InventoryItemData` debe tener:

- un `WorldPrefab` asignado;
- `Can Examine` activado.

El modelo que aparece es una copia visual temporal: examinarlo no elimina ni modifica el objeto real del inventario. Si el objeto tiene `ExamineHotspot`, el jugador puede pasar el cursor por encima de esa zona para ver una pista y hacer clic para revelarla. Los hotspots pueden conceder otro objeto, lanzar un evento o mostrar una descripción, y su estado se conserva con las partidas guardadas.

Las notas legibles (`Is Readable`) utilizan el lector de texto del inventario y no necesitan un modelo 3D. En VR, el mismo panel se presenta como UI 3D y utiliza los eventos de puntero del controlador; aun así, el soporte VR continúa siendo experimental y debe probarse con el visor final.

## 6. Controles PC predeterminados

- WASD: movimiento.
- Ratón: mirar.
- Shift izquierdo: correr.
- Ctrl izquierdo: agacharse.
- E: interactuar o guardar un objeto físico sostenido.
- I: inventario.
- F: encender/apagar la linterna equipada.
- R: recargar la linterna.
- Q: soltar un objeto físico sostenido.
- G: soltar equipamiento.
- H: solicitar pista.
- Alt izquierdo + A/D: inclinarse en Survival Horror.
- X: mirar atrás en Survival Horror.
- V mientras corres hacia delante: slide en Survival Horror.
- Esc: cerrar el panel actual o pausar.
- F5/F9: guardado/carga rápida.

Los controles principales se pueden reasignar durante el juego desde `Ajustes > Controles`; los cambios se guardan fuera de las partidas. Para modificar bindings de mando o XR, edita `Resources/Input/EscapeRoomInputActions.inputactions`.

## 7. Preparación VR

**Experimental**: el soporte VR es funcionalmente completo (rig, manos, hápticos, UI 3D, confort) pero todavía no ha pasado QA en un visor físico real — solo en el simulador de XRI. No asumas paridad total con PC hasta validarlo en hardware.

`ShowcaseMuseumVR` contiene la versión VR del museo y conserva los mismos puzles de las salas 11 y 13. `VRTemplate` es una escena mínima de arranque y no contiene las habitaciones del museo.

1. Espera a que Package Manager termine de importar OpenXR, XR Plug-in Management y XRI.
2. Configura OpenXR para los destinos deseados en Project Settings.
3. Ejecuta `Setup/Create or Update VR Player Prefab`.
4. Ejecuta `Setup/Prepare Current Scene Interactables for VR` en cada escena.
5. Ejecuta las comprobaciones de Project Validation de OpenXR/XRI.

El prefab VR lo genera la versión instalada de XRI e incorpora adaptadores de manos, hápticos y UI Toolkit 3D. Los modelos de mando/mano se sustituyen bajo sus `ModelSocket`.

## 8. Accesibilidad y ritmo de terror

Desde el menú de ajustes del propio juego (no del Editor), el jugador puede activar:

- reducir destellos, tremor de cámara y sonidos fuertes;
- asistencia en persecuciones (el enemigo va algo más lento y olvida antes);
- reducción de gore, disponible como opción aunque la plantilla base no incluya contenido de gore todavía.

Ninguna de estas opciones sustituye a la dificultad: son independientes, así que un jugador puede combinar `Nightmare` con `chaseAssistance` si lo necesita.

Si añades un `TensionDirector` a la escena, limita cuántos eventos de terror pueden dispararse seguidos (cooldown global y presupuesto por ventana de tiempo), por encima del cooldown propio de cada evento. Es opcional: sin él, todo funciona igual que antes.

## 9. Publicación

Antes de distribuir el asset:

1. Ejecuta `Validation/Run Framework Smoke Tests`.
2. Ejecuta `Validation/Validate Save IDs` en cada escena.
3. Comprueba PC y VR por separado.
4. No cambies `SaveId` ni `ItemId` en una actualización publicada sin añadir una migración.
5. Ejecuta `Validation/Validate Current Scene` y resuelve todos los puzles de cada escena desde una build limpia.
