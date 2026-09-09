# Escape Room: guía de creación y entrega

Esta guía está dirigida a quien usa el template para crear su propio juego. El alcance de esta edición es **Escape Room para PC y VR con mandos**. Survival Horror sigue en desarrollo y no forma parte de la validación comercial de esta edición. El resultado de las comprobaciones está en [la auditoría](../AUDITORIA_ESCAPE_ROOM_2026-09-09.md); la referencia extensa está en [DOCUMENTACIO_COMPLETA.md](../DOCUMENTACIO_COMPLETA.md).

## 1. Abrir el proyecto y jugar la muestra

1. Instala Unity **6000.4.9f1**. Para Quest, instala también Android Build Support y sus herramientas SDK, NDK y OpenJDK desde Unity Hub.
2. Abre la carpeta que contiene `Assets`, `Packages` y `ProjectSettings`. Espera a que terminen la importación y la compilación.
3. Conserva las versiones de `Packages/manifest.json` y `packages-lock.json`. El ensamblado Player referencia XRI y XR Core Utils incluso al compilar para PC; no elimines los paquetes XR de este proyecto.
4. Abre `Assets/_EscapeRoomTemplate/Scenes/ShowcaseMuseum.unity` para PC, o `ShowcaseMuseumVR.unity` para el museo VR. `VRTemplate` es una escena mínima, no la demostración completa.
5. Ejecuta `Escape Room Framework > Validation > Run Framework Smoke Tests` y `Validate Current Scene`. Lee el resultado en Console, no solo la confirmación de que el menú se ha ejecutado.
6. Pulsa Play. En PC, usa WASD y ratón, E para interactuar, I para inventario, H para pistas y Esc para cerrar el panel o pausar. F5/F9 guardan/cargan el slot rápido. Los tres slots manuales son independientes.

El museo contiene 12 controladores de puzle en cada plataforma. Las ruedas numéricas utilizan `StatePuzzle`; la melodía utiliza `SequencePuzzle`. No son solvers separados. `LockedOffice` y `LockedOfficeVR` contienen una muestra alternativa con dos paneles de código.

## 2. Crear una habitación propia

1. Crea una carpeta propia, por ejemplo `Assets/MyEscapeRoom`, y guarda allí tu escena, datos y arte. Conserva las escenas de muestra como referencia.
2. Selecciona `Configuration > Use Escape Room Profile`. Los museos incluyen una excepción local de linterna para su demostración de combinación; una escena nueva no hereda esa excepción.
3. Usa `Setup > Instantiate Game Manager` y el jugador PC, o instancia `Prefabs/Player_VR.prefab`. Debe existir un solo jugador activo de la plataforma elegida. Añade suelo con collider, iluminación y límites físicos.
4. Crea objetos con el menú `Create`. Mantén componentes, colliders y eventos en la raíz lógica. Cambia el arte bajo `ModelSocket` o el hijo visual indicado por el prefab.
5. Crea un `PuzzleDefinition` y un `HintData` propios para cada puzle. Asigna la definición al controlador y las pistas a la definición. Escribe pistas graduales: contexto, indicación y solución.
6. Comprueba IDs: cada entidad persistente debe tener un `SaveId` único. Un `PuzzleDefinition.PersistentId` identifica el puzle al guardar. Duplicar un objeto o una definición puede copiar su ID: revisa y cambia ese ID antes de usar ambas copias juntas. Reutiliza un `InventoryItemData` para varias unidades del mismo ítem; crea otro `ItemId` para otro tipo de objeto.
7. Añade la escena a la lista de escenas habilitadas de Build Profiles. Configura `Resources/GameFlowSettings.asset`: `First Gameplay Scene` para Nueva partida y `Main Menu Scene` para el retorno al menú. Usa nombres únicos o rutas completas.
8. Valida la escena y prueba el recorrido de principio a fin, incluido guardar en mitad de cada mecanismo, salir y cargar.

### Ejemplo: un código que abre una puerta

1. Crea un panel de código y una puerta desde el menú `Create`.
2. En `CodePanelPuzzle`, pon `Correct Code = 3142`, `Max Code Length = 4` y activa `Auto Check When Full`.
3. Cada botón debe llamar a `InputDigit` con un único carácter. El botón de borrar utiliza `C` o `ClearInput`; si desactivas la comprobación automática, añade un botón que llame a `SubmitCode`.
4. En `On Solved` del controlador, añade la puerta y selecciona su método público de apertura en el desplegable. La puerta debe tener su propio ID persistente.
5. Añade una pista visible que permita deducir 3142. No actives la variante aleatoria si la pista escrita sigue mostrando una solución fija.
6. Prueba un código incorrecto, el correcto, guardar con la puerta abierta y volver a cargar. `On Solved` representa una resolución nueva; no se vuelve a emitir al cargar. La puerta debe restaurar su estado mediante su propio guardado.

## 3. Elegir y configurar las mecánicas

Los nombres entre comillas invertidas son campos o métodos del componente. Puedes conectar los métodos públicos mediante UnityEvents del Inspector cuando su firma sea compatible.

| Mecánica | Configuración y entrada | Comprobación mínima |
|---|---|---|
| Código: `CodePanelPuzzle` | Código, longitud, comprobación automática. Botones → `InputDigit`; confirmar → `SubmitCode`. | Error limpia el intento; acierto activa el evento una vez. |
| Secuencia: `SequencePuzzle` | `Correct Sequence` con IDs ordenados. Cada botón/lever → `InputStep(id)` o un `SequenceStepButton`. | Un error reinicia el intento. Guardar tras el primer paso y cargar permite continuar por el segundo. |
| Estados y ruedas: `StatePuzzle` | Lista `Conditions`: un `SteppedPositioner` y su `Required Index`, empezando en cero. | Todas las posiciones deben coincidir. Una referencia ausente impide resolver. |
| Ruedas numéricas | `Create > Number Wheels Puzzle`; entre 2 y 8 ruedas. Edita con `NumberWheelsPuzzleAuthoring` y reconstruye desde su Inspector. | PC: entra en foco y pulsa ▲/▼. VR: botones físicos equivalentes. Revisa el código después de redimensionar. |
| Ítem de inventario: `SocketPuzzle` | `Required Item Id`, consumo opcional y prefab visual con punto de colocación. Tu control de inventario llama a `TryInsertItem` tras comprobar que el jugador posee el ítem. | Rechaza un ID incorrecto; al cargar resuelto aparece una sola pieza visual. |
| Receptor con UI: `ItemReceiver` | Asigna `Required Item`, política de selección y `On Item Accepted`. Es la opción de autoría con selección de inventario integrada. | Sin ítem no abre; usarlo consume solo cuando corresponda. No conectes dos consumos para una misma acción. |
| Objetos físicos: `PhysicsGrabbable` + `PhysicsSocket` | Rigidbody, collider, `PickableItem.Data` y `Required Item Id` coincidentes; el socket tiene un trigger y punto de encaje. | Solo encaja al soltar. Un objeto fijado no vuelve a agarrarse. Este socket no implementa `ISaveable`: para un resultado persistente, conecta un estado guardable propio o utiliza el puzle de colocación. |
| Colocación: `PlacementPuzzle` | Reglas `pieceId → correctSocketId`; piezas con `GrabbablePiece` y receptores `PieceSocketReceiver` enlazados al mismo puzle. | Prueba piezas intercambiadas, retirada antes de resolver y carga tras resolver. Los IDs de piezas y sockets deben coincidir exactamente. |
| Lanzamiento: `ThrowPuzzle` | Configura las dianas `ThrowTarget` requeridas y objetos físicos lanzables. | Deben acertarse todas las dianas requeridas. Asegura que las piezas puedan recuperarse. |
| Deslizante: `SlidingPuzzle` | Filas, columnas, celda vacía, movimientos de mezcla; presentación con `SlidingBoardView` y `SlidingTileButton`. | Solo se mueve una ficha vecina del hueco; el estado cargado coincide con el guardado. |
| Tuberías: `PipePuzzle` | Tiles con ID, fila, columna, conexiones y giro inicial; configura origen y destino. `PipeTileButton` → `RotateTile`. | Comprueba conectividad real de origen a destino, no solo el aspecto. El solver necesita una presentación interactiva. |
| Grupo: `MultiStagePuzzle` | Lista de puzles hijos y raíces de interacción; orden libre u obligatorio y bloqueo de futuros. | Los hijos permanecen visibles; el grupo resuelve una vez al completar todos. El premio final se conecta al grupo. |
| Melodía: `MelodyPlayer` | Presentación sonora de pasos de una secuencia. | Añade una pista visual equivalente; evita depender únicamente de la audición. |
| Notas y examen | `InteractableNote` para lectura; `InventoryItemData` con datos de lectura/examen y `ExamineHotspot` para zonas del modelo. | Texto legible, cerrar devuelve el control y un hotspot no se dispara a través de otro panel. |
| Interruptores, puertas y cajones | `InteractableTrigger`, `InteractableToggle`, `SteppedPositioner`, `Door`; configura eventos y recorrido. | Sin duplicar eventos, sin atravesar límites, mismo uso en PC y VR. |
| Pistas | `HintData` en la definición; zonas opcionales `HintZoneTrigger`. | Pistas pertinentes al puzle activo y retirada al resolver. |
| Tiempo y peligro móvil | `GameOverTimer.StartTimer` y `MovingHazard.StartHazard`, activados por un botón o `EventTriggerZone`. Marcadores 3D definen el recorrido del peligro. | Pausa congela el avance, HUD refleja el tiempo, fallo muestra resultados. Son mecanismos independientes. |
| Objetivos y final | `ObjectiveSet`/`ObjectiveManager`, prerrequisitos sin ciclos; `GameEndTrigger` y `EndingDefinition`. | No se termina antes del último objetivo; Reintentar permite volver a recoger los objetos. |
| Cambio de habitación | `RoomPortal`, escena destino habilitada, `RoomSpawnPoint` con ID. | En modo Single no hay caché completa de habitaciones anteriores. En Additive evita gestores y jugadores duplicados. |

### Reiniciar un puzle no equivale a reiniciar el mundo

`ResetPuzzle` devuelve el controlador a no resuelto y limpia su estado específico. No revierte automáticamente una puerta conectada por evento, no devuelve ítems consumidos y no reconstruye cualquier objeto destruido por lógica propia. Si ofreces un botón de reinicio, conecta también la restauración de su entorno. Para empezar de cero, utiliza Nueva partida o Reintentar de la escena.

## 4. Inventario, combinación y guardado

1. Crea `InventoryItemData` con ID, nombre, icono y las acciones que deba permitir. Añádelo al `ItemCatalog` que se use en el juego; las referencias deben llegar a la build.
2. Para recogerlo, asigna sus datos a `PickableItem`. Para combinar, configura `Combinations` con el otro ítem y el resultado. Incluye también el resultado en el catálogo.
3. Prueba la combinación con inventario lleno y con cantidades múltiples. No uses un objeto imprescindible que pueda perderse sin un modo de recuperación.
4. Guarda antes y después de consumir una llave, resolver, abrir una puerta o completar un objetivo. Cierra el ejecutable y carga, además de probar la carga dentro de la misma sesión.
5. Los archivos están en `Application.persistentDataPath/SaveSlots`. Cada slot tiene JSON, miniatura y, tras una sobrescritura, una copia `.bak`. No borres IDs de contenido ya publicado si quieres conservar compatibilidad con partidas.
6. Una copia de seguridad puede recuperar un JSON principal ausente o estructuralmente inválido. Esto no repara estados arbitrarios incorrectos dentro de cada componente. Conserva copias externas para las pruebas de migración.
7. Los slots PC y VR de estas muestras no deben anunciarse como intercambiables: sus rutas de escena y algunos componentes de jugador son distintos.

## 5. Preparar VR con mandos

1. Parte de `ShowcaseMuseumVR` para estudiar las mecánicas o de `VRTemplate` para una escena mínima.
2. Comprueba OpenXR con `Setup > Configure OpenXR (PC + Android)` y la ventana Project Validation. Mantén los perfiles de los mandos que vayas a soportar.
3. Para una escena propia, prepara los interactuables con `Setup > Prepare Current Scene Interactables for VR`. Revisa después las referencias de cada objeto y no regeneres una demo personalizada sin guardar una copia.
4. El rig usa `VRPlayerPlatformAdapter`, la presentación UI Toolkit en 3D y sus puentes de puntero. Conserva las referencias de cabeza y ambas manos. El jugador VR debe ser el único jugador activo.
5. La ruta `VRHardwareInteractor` usa el gatillo para activar y grip/gatillo para sujetar objetos físicos. Soltar ambos libera el objeto; el lanzamiento depende del movimiento de la mano y de `Can Be Thrown`. Los sockets esperan a que se suelte.
6. La ruta XRI utiliza `VRInteractionBridge`. No configures dos acciones distintas para que una sola pulsación active dos veces un mecanismo. Verifica especialmente interruptores, botones ▲/▼ y recogida.
7. Para probar sin visor, activa el objeto de simulador indicado en `VRTemplate`. La simulación no verifica los botones físicos ni sustituye la prueba del ejecutable en el dispositivo. Desactívala para la entrega en visor.
8. Revisa `Resources/VRComfortSettings.asset`: locomoción, giro por saltos/continuo y preferencias. Prueba alturas sentado y de pie, accesibilidad de todos los controles y escala del panel.
9. No uses cámaras de feedback ni desplazamientos forzados de la cabeza como requisito para resolver. La base de puzles omite sus cortes de cámara en VR; cualquier cámara o cinemática propia necesita el mismo tratamiento.

El paquete Quest de `ReleaseBuilder` arranca directamente en `ShowcaseMuseumVR` y no incluye una escena de menú inicial. El menú de pausa/resultados omite el retorno al menú cuando esa escena no está disponible. Para un juego VR con menú inicial, crea una escena de menú compatible con VR, inclúyela en la build y configura sus rutas: no reutilices sin más el menú PC.

`Build > Release > Build Windows` genera la demo de escritorio sin iniciar XR y restaura la configuración XR del editor al finalizar. Un ejecutable PCVR requiere su propia lista de escenas VR y mantener activa la inicialización de XR; no uses este comando de escritorio para una entrega PCVR. En Android se ha aplicado la prioridad de lectura de input indicada por la validación de Meta Quest/OpenXR.

## 6. Antes de entregar a un cliente

| Verificación | Criterio de aceptación |
|---|---|
| Instalación limpia | Importar el paquete en otra carpeta con la versión documentada; no depende de Library ni de herramientas privadas de desarrollo. |
| Recorrido PC | Completar el museo y la habitación propia sin consola de depuración; probar error y acierto en cada mecánica. |
| Guardado | Guardar, cerrar ejecutable y cargar en mitad de cada mecanismo, después de recoger/consumir y tras cambiar de escena. |
| VR en dispositivo | Registrar visor, mandos, runtime, versión de build y resultado de cada fila de la matriz VR. |
| Rendimiento | Medir CPU/GPU, memoria y estabilidad de frames en el hardware objetivo con la escena final. No hay un presupuesto certificado por esta auditoría. |
| Contenido | Completar procedencia y condiciones de redistribución en `ThirdPartyNotices.md`; sustituir material sin procedencia verificable. |
| Idiomas | Revisar también notas, prompts, pistas, puzles y finales; el selector de idioma no garantiza que todo contenido propio esté traducido. |
| Paquete | Mantener `.meta`, escenas, datos, prefabs, shaders, UI y dependencias necesarias. Excluir builds, caches, logs y herramientas MCP de la entrega al comprador. |

No elimines la carpeta Survival de forma aislada para empaquetar: actualmente hay referencias de compilación desde sistemas compartidos. El perfil Escape Room desactiva sus funciones opcionales; separar físicamente esos ensamblados requiere trabajo adicional.

### Matriz VR que debe rellenarse por dispositivo

| Caso | Resultado / dispositivo / build |
|---|---|
| Arranque, tracking, audio y recentrado | Pendiente |
| Ambas manos: interacción, agarre, soltar y lanzamiento | Pendiente |
| Todas las salas; dianas, colocación y ruedas | Pendiente |
| Inventario, combinación, notas, examen y hotspots | Pendiente |
| Pausa, ajustes, guardar/cargar y cerrar panel sin activar el mundo | Pendiente |
| Teletransporte, giro y locomoción elegida; sentado/de pie | Pendiente |
| Final, derrota, Reintentar y objetos recogibles restaurados | Pendiente |
| Pérdida de tracking, mando desconectado y suspensión/reanudación | Pendiente |
| Rendimiento sostenido durante una partida completa | Pendiente |

## 7. Resolver problemas frecuentes

| Síntoma | Qué revisar |
|---|---|
| No se abre desde Hub | Comprueba que otra instancia o prueba de Unity no tenga abierto el mismo proyecto. No borres el lock mientras siga activa. |
| «Missing script» tras importar | Versión del editor, paquetes resueltos y errores de compilación; conserva `.meta` y referencias de ensamblado. |
| El puzle no resuelve | IDs, orden, índices desde cero, definición y condiciones/referencias completas. |
| Una puerta vuelve a cerrarse al cargar | Su propio guardado/ID y la restauración visual; no dependas de que vuelva a emitirse `On Solved`. |
| VR no muestra el museo | Abre `ShowcaseMuseumVR`; `VRTemplate` es solo el arranque mínimo. |
| Un control VR se activa dos veces | Revisa eventos duplicados y las rutas de XRI/hardware del rig. |
| Falta un objeto necesario | Recuperación de piezas, límites físicos y opciones de lanzamiento; revisa IDs duplicados y reglas de consumo. |
| El menú no puede cargar una escena | Incluye el destino en la lista utilizada por esa build; comprueba la configuración de flujo. |

Para campos avanzados y extensiones de código, consulta [la guía de programación](../PROGRAMMING_GUIDE.md) y [la referencia completa](../DOCUMENTACIO_COMPLETA.md).
