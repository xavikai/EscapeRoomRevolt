# Recetas paso a paso

**Idioma:** [català](RECEPTES.md) · [castellano](RECETAS.md) · [English](RECIPES.md)

Cada receta parte de una escena vacía con el jugador ya colocado (`Setup > Instantiate Game Manager` y el jugador PC o VR). Para probarlas de forma interactiva, abre la guía [`docs/guia/index.html`](../../../docs/guia/index.html) > *Recetas paso a paso*.

- Puzles: [Teclado de código](#code) · [Secuencia y melodía](#seq) · [Palancas e interruptores (StatePuzzle)](#state) · [Rodillos numéricos (candado)](#wheels) · [Tuberías](#pipe) · [Rompecabezas deslizante](#slide) · [Circuito de luces](#lights) · [Lanzamiento a dianas](#throw) · [Colocación de piezas](#place) · [Puzles encadenados](#multi)
- Sistemas de la sala: [Objetos, inventario, combinación y examen](#items) · [Puertas, cajones y armarios](#doors) · [Pistas y narración](#hints) · [Objetivos y final de partida](#objectives) · [Temporizador y peligro móvil](#hazard) · [Cambio de sala o de escena](#rooms)

## Puzles

<a id="code"></a>
### Teclado de código

Menú: `Escape Room Framework > Create > Puzzles > Keypad Panel` · Sala del museo 2

**Pasos**

1. Ejecuta Escape Room Framework > Create > Puzzles > Keypad Panel. Se crea NewKeypad con CodePanelPuzzle, InteractableKeypad, una pantalla, un LED y once botones 3D (1–9, C, 0) ya conectados al puzle.
2. Colócalo en la pared moviendo solo la raíz NewKeypad. No escales los botones por separado.
3. En CodePanelPuzzle escribe Correct Code (por ejemplo 3142), pon Max Code Length con el mismo número de cifras y deja Auto Check When Full activado.
4. Crea los datos del puzle: en el panel Project, clic derecho > Create > Escape Room Framework > Puzzles > Puzzle Definition. Ponle un Persistent Id único (por ejemplo sala1_teclado), un Display Name y el Objective. Después crea las pistas con Create > Escape Room Framework > Hints > Hint Data (tres pistas: contexto, indicación y solución) y arrástralas al campo Hints de la definición.
5. Selecciona la raíz del puzle y arrastra la definición al campo Definition del componente.
6. Crea la recompensa: Escape Room Framework > Create > Interactables > Door y marca Is Locked en la puerta. Selecciona el puzle, haz Ctrl+clic en la puerta y ejecuta Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. En On Solved aparecerán Door.Unlock y Door.ForceOpen.
7. Esconde la pista del código: Escape Room Framework > Create > Interactables > Note y escríbela en Note Content.

**Cómo lo pruebas**

- Un código erróneo hace parpadear el LED rojo y borra el intento; el correcto lo pone verde y abre la puerta.
- Pruébalo en Play: guarda con F5 a mitad del puzle y una vez resuelto, sal y carga con F9. El estado debe ser el mismo y la puerta debe seguir abierta.

**Ojo con**

- Randomize Code cambia el código en cada partida: actívalo solo si la pista también es dinámica.
- On Solved solo se ejecuta cuando el jugador resuelve el puzle, no al cargar una partida: la puerta recupera su estado por su propio guardado.

<a id="seq"></a>
### Secuencia y melodía

Menú: `Escape Room Framework > Create > Puzzles > Sequence Puzzle` · Sala del museo 4 · 9

**Pasos**

1. Ejecuta Escape Room Framework > Create > Puzzles > Sequence Puzzle. Crea tres botones (rojo, verde, azul) con un SequenceStepButton cada uno, y el orden correcto rojo → verde → azul.
2. La lista Correct Sequence de SequencePuzzle contiene los identificadores en orden. El Step Id de cada SequenceStepButton debe coincidir exactamente con uno de ellos.
3. Para añadir un paso, duplica un botón, cambia su Step Id y añade ese ID a Correct Sequence. Un ID puede aparecer más de una vez en la lista.
4. Melodía (opcional): pon un AudioSource con su nota en cada botón, añade un MelodyPlayer al puzle y rellena Notes en el orden correcto. Crea un Generic Trigger «Escuchar la melodía» y, en su On Interact Event, llama a MelodyPlayer.Play.
5. Crea los datos del puzle: en el panel Project, clic derecho > Create > Escape Room Framework > Puzzles > Puzzle Definition. Ponle un Persistent Id único (por ejemplo sala1_teclado), un Display Name y el Objective. Después crea las pistas con Create > Escape Room Framework > Hints > Hint Data (tres pistas: contexto, indicación y solución) y arrástralas al campo Hints de la definición.
6. Selecciona la raíz del puzle y arrastra la definición al campo Definition del componente.
7. Crea la recompensa: Escape Room Framework > Create > Interactables > Door y marca Is Locked en la puerta. Selecciona el puzle, haz Ctrl+clic en la puerta y ejecuta Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. En On Solved aparecerán Door.Unlock y Door.ForceOpen.

**Cómo lo pruebas**

- Un paso equivocado reinicia el intento y dispara On Failed.
- Guarda tras el primer paso correcto y carga: debes poder seguir por el segundo.
- Pruébalo en Play: guarda con F5 a mitad del puzle y una vez resuelto, sal y carga con F9. El estado debe ser el mismo y la puerta debe seguir abierta.

**Ojo con**

- Si solo hay pista sonora, añade una visual: no todo el mundo distingue las notas.
- Randomize Order baraja el orden en cada partida; la pista debe mostrar el orden real.

<a id="state"></a>
### Palancas e interruptores (StatePuzzle)

Menú: `Escape Room Framework > Create > Puzzles > State Puzzle` · Sala del museo 5

**Pasos**

1. Ejecuta Escape Room Framework > Create > Puzzles > State Puzzle. Crea tres palancas de tres posiciones (SteppedPositioner + InteractableCycler) que resuelven cuando marcan 0, 1 y 2.
2. En cada palanca, la lista Positions define cada posición: el texto que ve el jugador (Prompt) y la rotación o el desplazamiento. Starting Index es la posición inicial.
3. En StatePuzzle, la lista Conditions une cada palanca (Positioner) con la posición que debe tener (Required Index, empieza en 0).
4. Para un interruptor de dos posiciones usa Escape Room Framework > Create > Interactables > Multi-Position Lever con dos posiciones. El InteractableToggle (Lever/Switch) no alimenta este puzle.
5. Crea los datos del puzle: en el panel Project, clic derecho > Create > Escape Room Framework > Puzzles > Puzzle Definition. Ponle un Persistent Id único (por ejemplo sala1_teclado), un Display Name y el Objective. Después crea las pistas con Create > Escape Room Framework > Hints > Hint Data (tres pistas: contexto, indicación y solución) y arrástralas al campo Hints de la definición.
6. Selecciona la raíz del puzle y arrastra la definición al campo Definition del componente.
7. Crea la recompensa: Escape Room Framework > Create > Interactables > Door y marca Is Locked en la puerta. Selecciona el puzle, haz Ctrl+clic en la puerta y ejecuta Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. En On Solved aparecerán Door.Unlock y Door.ForceOpen.

**Cómo lo pruebas**

- Solo se resuelve cuando todas las palancas están en su posición; el orden no importa.
- Guarda con palancas a medias y carga: las posiciones se conservan.
- Pruébalo en Play: guarda con F5 a mitad del puzle y una vez resuelto, sal y carga con F9. El estado debe ser el mismo y la puerta debe seguir abierta.

**Ojo con**

- Una condición sin Positioner impide resolver el puzle.

<a id="wheels"></a>
### Rodillos numéricos (candado)

Menú: `Escape Room Framework > Create > Puzzles > Number Wheels Puzzle` · Sala del museo 13

**Pasos**

1. Ejecuta Escape Room Framework > Create > Puzzles > Number Wheels Puzzle. En la ventana elige el número de rodillos (de 2 a 8) y la cifra de cada uno, y pulsa Create puzzle.
2. Se crea un StatePuzzle con un PuzzleFocusPoint (cámara de aproximación), el componente NumberWheelsPuzzleAuthoring y los botones ▲/▼ de cada rodillo.
3. Para cambiar la combinación o el número de rodillos, edita NumberWheelsPuzzleAuthoring y pulsa Rebuild wheels and layout. Se conservan la definición y On Solved.
4. Para cambiar el aspecto (maleta, caja fuerte), sustituye los modelos dentro de los ReplaceableModelSlot de la carcasa y de los rodillos.
5. Crea los datos del puzle: en el panel Project, clic derecho > Create > Escape Room Framework > Puzzles > Puzzle Definition. Ponle un Persistent Id único (por ejemplo sala1_teclado), un Display Name y el Objective. Después crea las pistas con Create > Escape Room Framework > Hints > Hint Data (tres pistas: contexto, indicación y solución) y arrástralas al campo Hints de la definición.
6. Selecciona la raíz del puzle y arrastra la definición al campo Definition del componente.
7. Crea la recompensa: Escape Room Framework > Create > Interactables > Door y marca Is Locked en la puerta. Selecciona el puzle, haz Ctrl+clic en la puerta y ejecuta Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. En On Solved aparecerán Door.Unlock y Door.ForceOpen.

**Cómo lo pruebas**

- En PC: E para entrar en la vista de aproximación, clic en ▲/▼ y clic derecho para salir. En VR: botones físicos ▲/▼ con el gatillo.
- Pruébalo en Play: guarda con F5 a mitad del puzle y una vez resuelto, sal y carga con F9. El estado debe ser el mismo y la puerta debe seguir abierta.

**Ojo con**

- Después de cambiar el número de rodillos, comprueba que la combinación sigue siendo la que quieres.

<a id="pipe"></a>
### Tuberías

Menú: `Escape Room Framework > Create > Puzzles > Pipe Puzzle` · Sala del museo 10

**Pasos**

1. Ejecuta Escape Room Framework > Create > Puzzles > Pipe Puzzle. Se crea NewPipePuzzle con dos tramos de ejemplo (pipe_a y pipe_b) girados 180°. Solo es la lógica: todavía no hay nada visible.
2. En la lista Tiles, cada tramo tiene Tile Id, Row, Column, Open Sides (lados abiertos sin girar: North, East, South, West) y Starting Rotation Steps (0–3, en pasos de 90°). Pon Source Tile Id y Sink Tile Id.
3. Para cada tramo, crea un objeto clicable (Escape Room Framework > Create > Interactables > Generic Trigger) y añádele un PipeTileButton con el puzle y su Tile Id. En On Interact Event del trigger llama a PipeTileButton.Rotate.
4. Para que el modelo gire, añade al tramo visual un SteppedPositioner con cuatro posiciones (0°, 90°, 180°, 270°) y asígnalo a Visual Positioner del PipeTileButton.
5. Crea los datos del puzle: en el panel Project, clic derecho > Create > Escape Room Framework > Puzzles > Puzzle Definition. Ponle un Persistent Id único (por ejemplo sala1_teclado), un Display Name y el Objective. Después crea las pistas con Create > Escape Room Framework > Hints > Hint Data (tres pistas: contexto, indicación y solución) y arrástralas al campo Hints de la definición.
6. Selecciona la raíz del puzle y arrastra la definición al campo Definition del componente.
7. Crea la recompensa: Escape Room Framework > Create > Interactables > Door y marca Is Locked en la puerta. Selecciona el puzle, haz Ctrl+clic en la puerta y ejecuta Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. En On Solved aparecerán Door.Unlock y Door.ForceOpen.

**Cómo lo pruebas**

- Se resuelve cuando hay un camino continuo de aberturas del origen al destino; una vez resuelto, los tramos dejan de girar.
- Pruébalo en Play: guarda con F5 a mitad del puzle y una vez resuelto, sal y carga con F9. El estado debe ser el mismo y la puerta debe seguir abierta.

**Ojo con**

- Randomize Rotations baraja los giros iniciales y nunca empieza resuelto.

<a id="slide"></a>
### Rompecabezas deslizante

Menú: `Escape Room Framework > Create > Puzzles > Sliding Puzzle` · Sala del museo 8

**Pasos**

1. Ejecuta Escape Room Framework > Create > Puzzles > Sliding Puzzle. Se crea un tablero 3×3 ya jugable con ocho fichas clicables.
2. Selecciona el hijo Board (SlidingBoardView) y asigna una imagen a Source Image: se corta en fragmentos e indica al jugador el orden correcto.
3. Para cambiar el tamaño, modifica Columns, Rows y Hole Cell en SlidingPuzzle y pulsa Rebuild board en Board.
4. Shuffle Move Count indica cuántos movimientos legales se hacen para mezclar: siempre tiene solución.
5. Crea los datos del puzle: en el panel Project, clic derecho > Create > Escape Room Framework > Puzzles > Puzzle Definition. Ponle un Persistent Id único (por ejemplo sala1_teclado), un Display Name y el Objective. Después crea las pistas con Create > Escape Room Framework > Hints > Hint Data (tres pistas: contexto, indicación y solución) y arrástralas al campo Hints de la definición.
6. Selecciona la raíz del puzle y arrastra la definición al campo Definition del componente.
7. Crea la recompensa: Escape Room Framework > Create > Interactables > Door y marca Is Locked en la puerta. Selecciona el puzle, haz Ctrl+clic en la puerta y ejecuta Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. En On Solved aparecerán Door.Unlock y Door.ForceOpen.

**Cómo lo pruebas**

- Solo se mueve una ficha vecina del hueco; las demás hacen un pequeño rebote.
- Pruébalo en Play: guarda con F5 a mitad del puzle y una vez resuelto, sal y carga con F9. El estado debe ser el mismo y la puerta debe seguir abierta.

**Ojo con**


<a id="lights"></a>
### Circuito de luces

Menú: `Assets/_EscapeRoomTemplate/Prefabs/LinkedLightsPuzzleKit.prefab` · Sala del museo 14

**Pasos**

1. Arrastra Prefabs/LinkedLightsPuzzleKit.prefab a la escena. Incluye el panel, cinco botones (LinkedLightButton), etiquetas y un botón de reinicio.
2. En LinkedLightsPuzzle > Nodes, cada luz tiene Initially On, Target On, Connections (índices que conmuta su botón, empezando por 0; incluye el propio si debe conmutarse) e Indicator.
3. El prefab comparte la definición demo_linked_lights: duplícala, cambia su Persistent Id y asigna la copia. Hazlo para cada circuito de la escena.
4. Crea la recompensa: Escape Room Framework > Create > Interactables > Door y marca Is Locked en la puerta. Selecciona el puzle, haz Ctrl+clic en la puerta y ejecuta Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. En On Solved aparecerán Door.Unlock y Door.ForceOpen.

**Cómo lo pruebas**

- Con la configuración de muestra se resuelve pulsando 1, 3 y 5. El botón de reinicio vuelve al estado inicial.
- Pruébalo en Play: guarda con F5 a mitad del puzle y una vez resuelto, sal y carga con F9. El estado debe ser el mismo y la puerta debe seguir abierta.

**Ojo con**

- Asegúrate de que tiene solución: parte del objetivo y aplica unas cuantas pulsaciones para obtener el estado inicial.

<a id="throw"></a>
### Lanzamiento a dianas

Menú: `Escape Room Framework > Create > Puzzles > Throw Puzzle` · Sala del museo 6

**Pasos**

1. Ejecuta Escape Room Framework > Create > Puzzles > Throw Puzzle. Se crean tres dianas con ThrowTarget (target_1, target_2, target_3) listadas en Target Ids.
2. Pon objetos para lanzar: Escape Room Framework > Create > Interactables > Physics Grabbable, con Can Be Thrown activado.
3. En cada diana, Min Impact Speed (m/s) evita que un golpe suave cuente.
4. Cierra el espacio con paredes con collider para que los objetos no se pierdan.
5. Crea los datos del puzle: en el panel Project, clic derecho > Create > Escape Room Framework > Puzzles > Puzzle Definition. Ponle un Persistent Id único (por ejemplo sala1_teclado), un Display Name y el Objective. Después crea las pistas con Create > Escape Room Framework > Hints > Hint Data (tres pistas: contexto, indicación y solución) y arrástralas al campo Hints de la definición.
6. Selecciona la raíz del puzle y arrastra la definición al campo Definition del componente.
7. Crea la recompensa: Escape Room Framework > Create > Interactables > Door y marca Is Locked en la puerta. Selecciona el puzle, haz Ctrl+clic en la puerta y ejecuta Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. En On Solved aparecerán Door.Unlock y Door.ForceOpen.

**Cómo lo pruebas**

- En PC: E para coger, clic izquierdo para lanzar y Q para soltar. En VR: coge con el grip y suelta haciendo el gesto.
- Pruébalo en Play: guarda con F5 a mitad del puzle y una vez resuelto, sal y carga con F9. El estado debe ser el mismo y la puerta debe seguir abierta.

**Ojo con**


<a id="place"></a>
### Colocación de piezas

Menú: `Escape Room Framework > Create > Puzzles > Placement Puzzle` · Sala del museo 7

**Pasos**

1. Ejecuta Escape Room Framework > Create > Puzzles > Placement Puzzle. Se crean dos piezas (piece_a, piece_b) y tres soportes (socket_a, socket_b y socket_c, que hace de señuelo).
2. En PlacementPuzzle, cada regla (Rules) dice a qué soporte va cada pieza: pieceId → correctSocketId.
3. En cada pieza, GrabbablePiece > Piece Id debe coincidir con la regla. Return When Lost la devuelve a su sitio si cae fuera de alcance.
4. En cada soporte, PieceSocketReceiver tiene el puzle, el Socket Id y un Snap Point. Los eventos On Piece Seated / Removed / Locked sirven para sonido y animación.
5. Crea los datos del puzle: en el panel Project, clic derecho > Create > Escape Room Framework > Puzzles > Puzzle Definition. Ponle un Persistent Id único (por ejemplo sala1_teclado), un Display Name y el Objective. Después crea las pistas con Create > Escape Room Framework > Hints > Hint Data (tres pistas: contexto, indicación y solución) y arrástralas al campo Hints de la definición.
6. Selecciona la raíz del puzle y arrastra la definición al campo Definition del componente.
7. Crea la recompensa: Escape Room Framework > Create > Interactables > Door y marca Is Locked en la puerta. Selecciona el puzle, haz Ctrl+clic en la puerta y ejecuta Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. En On Solved aparecerán Door.Unlock y Door.ForceOpen.

**Cómo lo pruebas**

- Prueba piezas intercambiadas, sacarlas antes de resolver y cargar una vez resuelto: las piezas quedan fijadas.
- Si guardas a mitad del puzle, al cargar las piezas sueltas vuelven a su sitio inicial.

**Ojo con**

- Randomize Mapping cambia qué soporte es el correcto en cada partida: la pista debe ser dinámica.

<a id="multi"></a>
### Puzles encadenados

Menú: `Escape Room Framework > Create > Puzzles > Multi-Stage Puzzle` · Sala del museo 11

**Pasos**

1. Ejecuta Escape Room Framework > Create > Puzzles > Multi-Stage Puzzle. Se crea un grupo con dos hijos visibles: Puzzle01_Sequence y Puzzle02_Levers.
2. En MultiStagePuzzle > Puzzles, cada entrada tiene un id, el puzle hijo y su raíz de interacción. Puedes añadir cualquier otro puzle de la escena.
3. Require Order obliga a resolverlos en orden; Lock Future Puzzles hace que los siguientes se vean pero no respondan hasta su turno.
4. Cada hijo necesita su PuzzleDefinition con un ID único. El grupo también tiene una.
5. Conecta la puerta solo al On Solved del grupo, no al de los hijos (usa Link Selected Objects To Puzzle con el grupo seleccionado).

**Cómo lo pruebas**

- La puerta se abre solo cuando todos los hijos están resueltos.
- Pruébalo en Play: guarda con F5 a mitad del puzle y una vez resuelto, sal y carga con F9. El estado debe ser el mismo y la puerta debe seguir abierta.

**Ojo con**


## Sistemas de la sala

<a id="items"></a>
### Objetos, inventario, combinación y examen

Menú: `Create > Escape Room Framework > Inventory > Item` · Sala del museo 3

**Pasos**

1. Crea el objeto: en Project, Create > Escape Room Framework > Inventory > Item. Rellena Item Id (único, en minúsculas, por ejemplo llave_despacho), Display Name, Description e Icon.
2. Asigna World Prefab (el modelo 3D) y activa Can Examine si se tiene que poder examinar. Para una nota, activa Is Readable y escribe Note Content.
3. Añádelo al ItemCatalog que usa InventoryManager (en el GameManager; por defecto ScriptableObjects/DefaultItemCatalog).
4. Ponlo en el mundo: Escape Room Framework > Create > Interactables > Pickable Item y asígnalo a Item Data.
5. Combinación: en el objeto A, añade una entrada a Combinations con Combine With = B y Result Item = C (añade C también al catálogo). Destroy This / Destroy Other deciden qué desaparece.
6. Puntos de examen: abre el World Prefab, selecciónalo y ejecuta Escape Room Framework > Create > Inventory > Examine Hotspot. Pon Hotspot Id, Unrevealed Prompt, Revealed Description y, si hace falta, Revealed Item.
7. Para usar el objeto en una cerradura: en la puerta, Is Locked + Required Item Id = el Item Id. O bien Escape Room Framework > Create > Interactables > Item Receiver con Required Item y On Item Accepted.

**Cómo lo pruebas**

- I abre el inventario. Prueba a combinar con el inventario lleno y con cantidades de más de una unidad.
- Guarda antes y después de usar una llave: el objeto recogido no vuelve a aparecer.

**Ojo con**

- No reutilices un Item Id para objetos distintos. Para varias unidades del mismo objeto, usa el mismo asset.

<a id="doors"></a>
### Puertas, cajones y armarios

Menú: `Escape Room Framework > Create > Interactables > Door · Drawer · Cabinet` · Sala del museo 1

**Pasos**

1. Ejecuta Escape Room Framework > Create > Interactables > Door (o Drawer o Cabinet). Todos usan el componente Door.
2. Movement Type: Pivot para puertas y armarios (gira sobre Custom Pivot con Open Angle), Slide para cajones (Slide Offset).
3. Para cerrarla: Is Locked. Con llave: Required Item Id y Consume Required Item. Item Use Policy decide cómo se ofrece la llave (Offer Compatible, Selected Only o Auto Use Single).
4. Desde un evento puedes llamar a Unlock, Lock, ForceOpen o ForceClose.
5. Cambia el modelo dentro del hijo visual; no toques el collider ni los componentes de la raíz.

**Cómo lo pruebas**

- La puerta guarda si está cerrada con llave y cuánto está abierta.

**Ojo con**

- Si duplicas una puerta, cambia su Save Id.

<a id="hints"></a>
### Pistas y narración

Menú: `Create > Escape Room Framework > Hints > Hint Data`

**Pasos**

1. Crea un Hint Data: Delay Before First Hint (segundos hasta la primera pista), Delay Between Hints y la lista Hints, de la más vaga a la solución. Cada pista puede llevar audio.
2. Asígnalo al campo Hints de la PuzzleDefinition: se activa cuando el jugador empieza el puzle y se apaga cuando lo resuelve.
3. Para pistas por zonas: Escape Room Framework > Create > Triggers > Hint Zone, asigna Puzzle Hint Data y, si quieres, Clear On Exit.
4. Para narración al entrar en un lugar: Escape Room Framework > Create > Triggers > Narrative Trigger con Play Mode Once, Always o ProgressiveHints.

**Cómo lo pruebas**

- El jugador puede pedir la siguiente pista con H. Las pistas salen como subtítulos y respetan la opción Subtítulos.

**Ojo con**

- Escribe las pistas en castellano (la clave del catálogo) y tradúcelas en el DefaultLocalizationCatalog si el juego es multilingüe.

<a id="objectives"></a>
### Objetivos y final de partida

Menú: `Escape Room Framework > Create > Flow > Objective Manager · Game End Trigger`

**Pasos**

1. Crea un Ending Definition (Create > Escape Room Framework > Ending Definition): Ending Id, Outcome (Victory o Defeat), Title y Message.
2. Crea un Objective Definition por objetivo: Objective Id, Title, Trigger (PuzzleSolved, ItemCollected, NoteRead, InteractionPerformed o Manual), Target Id (el Persistent Id del puzle, el Item Id…) y Prerequisites.
3. Crea un Objective Set con la lista de objetivos y el Completion Ending. Opcionalmente, Next Room Scene y Next Room Spawn Id para pasar a otra sala.
4. Ejecuta Escape Room Framework > Create > Flow > Objective Manager y asígnale el Objective Set.
5. Alternativa sencilla: Escape Room Framework > Create > Flow > Game End Trigger con un Ending. Activa Activate On Player Enter o llama a Trigger desde el On Solved del último puzle.

**Cómo lo pruebas**

- La partida no acaba antes del último objetivo. En la pantalla final, Reintentar vuelve a empezar la sala.

**Ojo con**

- Evita prerrequisitos circulares.

<a id="hazard"></a>
### Temporizador y peligro móvil

Menú: `Escape Room Framework > Create > Flow > Game Over Timer (HUD) · Moving Hazard (Any Direction)` · Sala del museo 12

**Pasos**

1. Temporizador: Escape Room Framework > Create > Flow > Game Over Timer (HUD). En la ventana elige duración, inicio automático, si se ve en el HUD y el texto.
2. Peligro: Escape Room Framework > Create > Flow > Moving Hazard (Any Direction). Elige dirección, distancia, duración y tamaño. Después puedes mover libremente StartPoint y EndPoint (techo, pared, suelo, plataforma o agua).
3. Para iniciarlos: un Generic Trigger con On Interact Event → GameOverTimer.StartTimer o MovingHazard.StartHazard, o una zona Escape Room Framework > Create > Triggers > Event Trigger Zone con On Entered.
4. Para detenerlos al resolver: en el On Solved del puzle, llama a StopTimer o StopHazard.
5. Asigna un Ending de derrota si quieres un mensaje propio.

**Cómo lo pruebas**

- En pausa el tiempo se congela. Al terminarse, aparece la pantalla de resultados con Reintentar.

**Ojo con**


<a id="rooms"></a>
### Cambio de sala o de escena

Menú: `RoomPortal + RoomSpawnPoint`

**Pasos**

1. En la escena de destino, crea un objeto vacío y añádele RoomSpawnPoint con un Spawn Id (por ejemplo entrada).
2. En la escena de origen, crea un objeto con collider y añádele RoomPortal: Target Scene, Target Spawn Id y Load Mode (Single o Additive). Puede estar cerrado (Is Locked) o pedir un objeto (Required Item Id).
3. Añade las dos escenas en File > Build Profiles.
4. Un puzle puede abrir el portal con Link Selected Objects To Puzzle (llama a RoomPortal.Unlock).

**Cómo lo pruebas**

- En modo Single, el inventario y el estado viajan con el jugador. En modo Additive las escenas conviven: no dupliques GameManager ni jugador.

**Ojo con**

