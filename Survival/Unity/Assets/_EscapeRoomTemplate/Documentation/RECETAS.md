# Recetas paso a paso

**Idioma:** [català](RECEPTES.md) · [castellano](RECETAS.md) · [English](RECIPES.md)

Las mismas recetas tienen casillas de progreso en la [guía interactiva](https://github.com/xavikai/EscapeRoomRevolt/releases/download/escaperoom-v0.2.2/guia-escape-room-0.2.2.html), en *Recetas paso a paso*.

## Antes de empezar en Unity

1. Abre tu escena en modo edición. Si `Play` está activo, vuelve a pulsarlo antes de crear o configurar nada. Si faltan paneles, ábrelos desde `Window > General > Hierarchy`, `Project` e `Inspector`.
2. `Hierarchy` contiene los objetos de la escena. Selecciona uno y mira sus componentes en `Inspector`. La raíz del puzle es el objeto padre que contiene el controlador. Pulsa su triángulo para ver los hijos. Tras crear un puzle, pulsa `F` con el cursor sobre `Scene` para centrar la vista en la selección.
3. `Project` contiene archivos y assets. Crea los tuyos en una carpeta propia bajo `Assets`. Para asignar una referencia, mantén seleccionado en `Hierarchy` el objeto receptor y arrastra el origen desde `Project` o `Hierarchy` al campo `None` de `Inspector`. Si haces clic en el origen en vez de arrastrarlo, cambia la selección.
4. Para una prueba de PC, necesitas suelo con collider, un `GameManager` y un jugador. Si faltan, créalos con `Escape Room Framework > Setup > Instantiate Game Manager` y `Escape Room Framework > Setup > Instantiate PC Player`. Coloca al jugador sobre el suelo, cerca del puzle. Si ya están en la escena, usa los existentes.
5. Al quitar elementos, revisa la lista del controlador y actualiza los textos, las pistas y la solución. Empieza una partida nueva para probar. Si quieres experimentar, duplica la escena de prueba desde `Project` antes de editarla.

- Puzles: [Teclado de código](#code) · [Secuencia y melodía](#seq) · [Palancas e interruptores (StatePuzzle)](#state) · [Rodillos numéricos (candado)](#wheels) · [Tuberías](#pipe) · [Rompecabezas deslizante](#slide) · [Circuito de luces](#lights) · [Lanzamiento a dianas](#throw) · [Colocación de piezas](#place) · [Puzles encadenados](#multi)
- Sistemas de la sala: [Llave que abre una puerta](#key) · [Objetos, inventario, combinación y examen](#items) · [Puertas, cajones y armarios](#doors) · [Pistas y narración](#hints) · [Objetivos y final de partida](#objectives) · [Temporizador y peligro móvil](#hazard) · [Cambio de sala o de escena](#rooms)

## Puzles

<a id="code"></a>
### Teclado de código

Menú: `Escape Room Framework > Create > Puzzles > Keypad Panel` · Sala del museo 2

**Pasos**

1. En el menú superior ejecuta `Escape Room Framework > Create > Puzzles > Keypad Panel`. Selecciona `NewKeypad_Logic` en `Hierarchy` y mueve el objeto entero a la pared con `Move`. Los botones ya están conectados.
2. En `CodePanelPuzzle` de `Inspector`, pon `Correct Code` en `3142` y `Max Code Length` en `4`. Deja `Auto Check When Full` activado y `Randomize Code` desactivado. Para un código más corto, cambia ambos campos, por ejemplo a `42` y `2`. Puedes conservar los botones.
3. Para dar una pista, crea una nota con `Escape Room Framework > Create > Interactables > Note`. Selecciona `NewNote_Logic` en `Hierarchy` y escribe la pista en `Note Content` del componente `InteractableNote` en `Inspector`. Colócala donde el jugador pueda leerla.
4. En `Project`, abre tu carpeta bajo `Assets`. Haz clic derecho en un espacio vacío y elige `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Llama al asset `Def_sala1_puzle` o usa un nombre fácil de reconocer.
5. Selecciona la definición en `Project`. En `Inspector`, escribe un `Persistent Id` único, por ejemplo `sala1_teclat` para el teclado de la sala 1. Rellena `Display Name` y `Objective`. Selecciona la raíz del puzle en `Hierarchy` y arrastra el asset desde `Project` a `Definition`, en el componente del puzle en `Inspector`.
6. Para añadir pistas, haz clic derecho en la misma carpeta de `Project` y elige `Create > Escape Room Framework > Hints > Hint Data`. Selecciona el `Hint Data` y añade los textos a su lista `Hints`. Selecciona la `Puzzle Definition` y arrastra el `Hint Data` de `Project` al campo `Hints` de la definición.
7. Para abrir una puerta al resolver el puzle, créala desde `Escape Room Framework > Create > Interactables > Door`. En `Hierarchy`, selecciona `NewDoor_Logic` y, en el componente `Door` de `Inspector`, marca `Is Locked`. Coloca la puerta en la escena.
8. En `Hierarchy`, selecciona la raíz del puzle. Mantén `Ctrl` en Windows o `Cmd` en Mac y pulsa `NewDoor_Logic` para seleccionar ambos. Ejecuta `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Selecciona solo la raíz y comprueba que `On Solved` contiene `Door.Unlock` y `Door.ForceOpen`. Guarda con `Ctrl+S` o `Cmd+S` y entra en `Play`.

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

1. Ejecuta `Escape Room Framework > Create > Puzzles > Sequence Puzzle`. En `Hierarchy`, abre el triángulo de `NewSequencePuzzle` para ver los tres botones. Selecciona la raíz y despliega `SequencePuzzle > Correct Sequence` en `Inspector`. El orden inicial es `red`, `green`, `blue`.
2. Para añadir un botón, duplica un botón completo en `Hierarchy` y edita el `Step Id` de su `SequenceStepButton`. En la raíz, añade ese texto a `Correct Sequence`. Para quitar el botón verde, elimina todas las entradas `green` y el objeto `greenButton_Logic` de `Hierarchy`. Para acortar solo la secuencia, quita pasos y deja los botones como opciones. Conserva al menos un paso.
3. Para una melodía, selecciona cada botón en `Hierarchy`, añade `Audio Source` con `Add Component`, arrastra el clip de `Project` al campo de audio y desmarca `Play On Awake`. En `On Interact Event`, añade una entrada con `+`, arrastra el botón a la entrada y elige `AudioSource > Play`. Conserva la entrada que comunica el paso al puzle.
4. Para reproducir la melodía de referencia, añade `MelodyPlayer` a la raíz con `Add Component`. En `Notes`, crea una entrada por paso y arrastra el botón correspondiente, en orden. Crea un `Generic Trigger` desde `Create > Interactables`. En su `On Interact Event`, pulsa `+`, arrastra `NewSequencePuzzle` desde `Hierarchy` y elige `MelodyPlayer > Play`. Al quitar pasos, actualiza `Notes`.
5. En `Project`, abre tu carpeta bajo `Assets`. Haz clic derecho en un espacio vacío y elige `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Llama al asset `Def_sala1_puzle` o usa un nombre fácil de reconocer.
6. Selecciona la definición en `Project`. En `Inspector`, escribe un `Persistent Id` único, por ejemplo `sala1_teclat` para el teclado de la sala 1. Rellena `Display Name` y `Objective`. Selecciona la raíz del puzle en `Hierarchy` y arrastra el asset desde `Project` a `Definition`, en el componente del puzle en `Inspector`.
7. Para añadir pistas, haz clic derecho en la misma carpeta de `Project` y elige `Create > Escape Room Framework > Hints > Hint Data`. Selecciona el `Hint Data` y añade los textos a su lista `Hints`. Selecciona la `Puzzle Definition` y arrastra el `Hint Data` de `Project` al campo `Hints` de la definición.
8. Para abrir una puerta al resolver el puzle, créala desde `Escape Room Framework > Create > Interactables > Door`. En `Hierarchy`, selecciona `NewDoor_Logic` y, en el componente `Door` de `Inspector`, marca `Is Locked`. Coloca la puerta en la escena.
9. En `Hierarchy`, selecciona la raíz del puzle. Mantén `Ctrl` en Windows o `Cmd` en Mac y pulsa `NewDoor_Logic` para seleccionar ambos. Ejecuta `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Selecciona solo la raíz y comprueba que `On Solved` contiene `Door.Unlock` y `Door.ForceOpen`. Guarda con `Ctrl+S` o `Cmd+S` y entra en `Play`.

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

1. Ejecuta `Escape Room Framework > Create > Puzzles > State Puzzle`. Abre `NewStatePuzzle` en `Hierarchy`. Selecciona `Lever1_Logic` y despliega `SteppedPositioner > Positions` en `Inspector`. `Element 0` es la primera posición. `Starting Index` indica cómo empieza la palanca.
2. Selecciona la raíz y abre `StatePuzzle > Conditions` en `Inspector`. Cada fila tiene `Positioner` y `Required Index`. Para añadir una palanca, duplica su objeto `_Logic`, pulsa `+` en `Conditions` y arrastra la nueva palanca de `Hierarchy` al campo `Positioner` de la nueva fila. Elige la posición requerida.
3. Para dejar dos palancas, selecciona en `Conditions` la fila que referencia `Lever3_Logic` y pulsa `–`. Después elimina `Lever3_Logic` de `Hierarchy`. Si reduces las posiciones de una palanca, revisa que `Starting Index` y `Required Index` estén dentro de la lista. Con dos posiciones pueden ser `0` o `1`.
4. En `Project`, abre tu carpeta bajo `Assets`. Haz clic derecho en un espacio vacío y elige `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Llama al asset `Def_sala1_puzle` o usa un nombre fácil de reconocer.
5. Selecciona la definición en `Project`. En `Inspector`, escribe un `Persistent Id` único, por ejemplo `sala1_teclat` para el teclado de la sala 1. Rellena `Display Name` y `Objective`. Selecciona la raíz del puzle en `Hierarchy` y arrastra el asset desde `Project` a `Definition`, en el componente del puzle en `Inspector`.
6. Para añadir pistas, haz clic derecho en la misma carpeta de `Project` y elige `Create > Escape Room Framework > Hints > Hint Data`. Selecciona el `Hint Data` y añade los textos a su lista `Hints`. Selecciona la `Puzzle Definition` y arrastra el `Hint Data` de `Project` al campo `Hints` de la definición.
7. Para abrir una puerta al resolver el puzle, créala desde `Escape Room Framework > Create > Interactables > Door`. En `Hierarchy`, selecciona `NewDoor_Logic` y, en el componente `Door` de `Inspector`, marca `Is Locked`. Coloca la puerta en la escena.
8. En `Hierarchy`, selecciona la raíz del puzle. Mantén `Ctrl` en Windows o `Cmd` en Mac y pulsa `NewDoor_Logic` para seleccionar ambos. Ejecuta `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Selecciona solo la raíz y comprueba que `On Solved` contiene `Door.Unlock` y `Door.ForceOpen`. Guarda con `Ctrl+S` o `Cmd+S` y entra en `Play`.

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

1. Ejecuta `Escape Room Framework > Create > Puzzles > Number Wheels Puzzle`. En la ventana, elige entre 2 y 8 ruedas, escribe una cifra por rueda y pulsa `Create puzzle`. `NumberWheelsPuzzle` aparece en `Hierarchy` con la cámara y los controles conectados.
2. Para añadir o quitar ruedas, selecciona la raíz en `Hierarchy`. En `Inspector > NumberWheelsPuzzleAuthoring`, cambia `Wheel Count`, revisa las cifras y pulsa `Rebuild wheels and layout`. Para pasar de cuatro a tres, pon `Wheel Count = 3` y verifica las tres cifras antes de reconstruir. El botón rehace el tablero y sus condiciones. Actualiza la pista del código.
3. En `Project`, abre tu carpeta bajo `Assets`. Haz clic derecho en un espacio vacío y elige `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Llama al asset `Def_sala1_puzle` o usa un nombre fácil de reconocer.
4. Selecciona la definición en `Project`. En `Inspector`, escribe un `Persistent Id` único, por ejemplo `sala1_teclat` para el teclado de la sala 1. Rellena `Display Name` y `Objective`. Selecciona la raíz del puzle en `Hierarchy` y arrastra el asset desde `Project` a `Definition`, en el componente del puzle en `Inspector`.
5. Para añadir pistas, haz clic derecho en la misma carpeta de `Project` y elige `Create > Escape Room Framework > Hints > Hint Data`. Selecciona el `Hint Data` y añade los textos a su lista `Hints`. Selecciona la `Puzzle Definition` y arrastra el `Hint Data` de `Project` al campo `Hints` de la definición.
6. Para abrir una puerta al resolver el puzle, créala desde `Escape Room Framework > Create > Interactables > Door`. En `Hierarchy`, selecciona `NewDoor_Logic` y, en el componente `Door` de `Inspector`, marca `Is Locked`. Coloca la puerta en la escena.
7. En `Hierarchy`, selecciona la raíz del puzle. Mantén `Ctrl` en Windows o `Cmd` en Mac y pulsa `NewDoor_Logic` para seleccionar ambos. Ejecuta `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Selecciona solo la raíz y comprueba que `On Solved` contiene `Door.Unlock` y `Door.ForceOpen`. Guarda con `Ctrl+S` o `Cmd+S` y entra en `Play`.

**Cómo lo pruebas**

- En PC: E para entrar en la vista de aproximación, clic en ▲/▼ y clic derecho para salir. En VR: botones físicos ▲/▼ con el gatillo.
- Pruébalo en Play: guarda con F5 a mitad del puzle y una vez resuelto, sal y carga con F9. El estado debe ser el mismo y la puerta debe seguir abierta.

**Ojo con**

- Después de cambiar el número de rodillos, comprueba que la combinación sigue siendo la que quieres.

<a id="pipe"></a>
### Tuberías

Menú: `Escape Room Framework > Create > Puzzles > Pipe Puzzle` · Sala del museo 10

**Pasos**

1. **Entiende cómo funciona (el controlador lógico + las piezas 3D).** A diferencia del teclado o los rodillos, `Escape Room Framework > Create > Puzzles > Pipe Puzzle` crea **solo el cerebro del puzle** (`NewPipePuzzle` con el componente `PipePuzzle`). Las piezas 3D que el jugador ve y pulsa en la escena las creas aparte y las enlazas por su nombre (`Tile Id`). El puzle imagina un **tablero o cuadrícula 2D** (como una tabla o un Excel) y comprueba si el agua puede ir desde la casilla de inicio (`Source Tile Id`) hasta la casilla final (`Sink Tile Id`).
2. **Qué son `Row` (fila) y `Column` (columna) y cómo se orienta la cuadrícula.** El puzle no mira las coordenadas X/Y/Z de Unity para saber qué pieza toca con cuál: **solo mira los números `Row` y `Column`** de la lista `Tiles`:
   - **`Row` (Fila = vertical, de arriba abajo):** `Row = 0` es la fila superior (`North`), `Row = 1` es la fila de debajo (`South`), `Row = 2` una más abajo...
   - **`Column` (Columna = horizontal, de izquierda a derecha):** `Column = 0` es la primera columna a la izquierda (`West`), `Column = 1` es la siguiente columna a la derecha (`East`), `Column = 2` una más a la derecha...
   - **¿Cuándo se conectan dos piezas?** Dos piezas son vecinas si están en la misma fila y columnas seguidas (ej. `(Row 0, Column 0)` y `(Row 0, Column 1)`), o en la misma columna y filas seguidas (ej. `(Row 0, Column 1)` y `(Row 1, Column 1)`). Para que el agua pase entre dos casillas vecinas, **ambas deben apuntarse mutuamente con bocas opuestas**: si la pieza izquierda `(0,0)` abre hacia `East` (derecha), la derecha `(0,1)` debe abrir hacia `West` (izquierda); si la pieza de arriba `(0,1)` abre hacia `South` (abajo), la de abajo `(1,1)` debe abrir hacia `North` (arriba).
3. **Cómo funcionan `Open Sides` (tipo de pieza) y `Starting Rotation Steps` (giro inicial).** En cada entrada de `PipePuzzle > Tiles` configuras cinco campos:
   - **`Tile Id`**: el nombre único de la pieza (ej. `pipe_a`). Debe ser idéntico al `Tile Id` de su botón 3D.
   - **`Row` y `Column`**: la casilla que ocupa en la cuadrícula (sin repetir la misma pareja `Row`/`Column` en dos piezas).
   - **`Open Sides`**: las bocas que tiene la pieza **en su posición base (0°, paso 0)**. En el desplegable de Unity puedes marcar varias direcciones a la vez:
     - **Tubo recto**: 2 lados opuestos, `North | South` (vertical `│`) o `East | West` (horizontal `─`).
     - **Codo (curva de 90°)**: 2 lados contiguos, como `North | East` (`└`), `East | South` (`┌`), `South | West` (`┐`) o `West | North` (`┘`).
     - **Pieza en T**: 3 lados, como `North | East | West` (`┴`). **Cruz**: los 4 lados (`Everything`). **Inicio / tapón**: 1 solo lado (`East`).
   - **`Starting Rotation Steps` (`0`, `1`, `2` o `3`)**: cuántos cuartos de vuelta (90° en sentido horario) tiene aplicados la pieza al empezar la partida. Cada clic del jugador suma `+1` (de `3` vuelve a `0`) y rota las bocas en sentido horario: **`North` → `East` → `South` → `West` → `North`**. *(Ejemplo: si `Open Sides = East` y pones `Starting Rotation Steps = 2`, la pieza empieza girada 180° mirando a `West`; al pulsarla 2 veces, vuelve a `0` mirando a `East`).*
4. **Crea el controlador y revisa las 2 piezas de ejemplo.** Ejecuta `Escape Room Framework > Create > Puzzles > Pipe Puzzle`. En `Hierarchy`, selecciona `NewPipePuzzle` y mira `PipePuzzle` en `Inspector`. Verás que ya incluye 2 piezas de ejemplo en `Tiles`:
   - `Element 0`: `Tile Id = pipe_a`, `Row = 0`, `Column = 0` (izquierda), `Open Sides = East` (mira a la derecha en el paso 0), `Starting Rotation Steps = 2` (empieza girada 180°, mirando a la izquierda).
   - `Element 1`: `Tile Id = pipe_b`, `Row = 0`, `Column = 1` (derecha), `Open Sides = West` (mira a la izquierda en el paso 0), `Starting Rotation Steps = 2` (empieza girada 180°, mirando a la derecha).
   - `Source Tile Id = pipe_a` (origen) y `Sink Tile Id = pipe_b` (destino). Cuando ambas lleguen al paso `0`, `pipe_a` (`East`) y `pipe_b` (`West`) se mirarán frente a frente y el puzle quedará resuelto.
5. **Construye la primera pieza 3D (`Tram A`) y conéctala al controlador.** Desde el menú superior ejecuta `Escape Room Framework > Create > Interactables > Generic Trigger`. Selecciona `NewTrigger_Logic` en `Hierarchy`, renómbralo a `Tram A` y colócalo en la pared o mesa:
   - Con `Tram A` seleccionado, pulsa `Add Component` en `Inspector` y añade **`PipeTileButton`**.
   - En `PipeTileButton`, arrastra `NewPipePuzzle` desde `Hierarchy` al campo **`Puzzle`**, y escribe **`pipe_a`** exactamente en **`Tile Id`**.
   - En `InteractableTrigger` del mismo `Tram A`, despliega **`On Interact Event`**, pulsa **`+`**, arrastra el propio `Tram A` al campo de objeto y en `No Function` elige **`PipeTileButton > Rotate ()`**.
6. **Haz que la pieza 3D gire visualmente al pulsarla (`SteppedPositioner`).** Para que el modelo 3D de `Tram A` rote 90° con cada clic en sincronía con el puzle:
   - Con `Tram A` seleccionado, pulsa `Add Component` y añade **`SteppedPositioner`**. Deja `Movement Type = Rotate`.
   - Despliega **`Positions`** y crea **4 elementos** (`Element 0` a `Element 3`, para los pasos `0`, `1`, `2` y `3`): si la tubería está en una pared vertical, pon las rotaciones `(0, 0, 0)`, `(0, 0, -90)`, `(0, 0, -180)` y `(0, 0, -270)`; si está sobre una mesa horizontal, usa el eje Y: `(0, 0, 0)`, `(0, 90, 0)`, `(0, 180, 0)` y `(0, 270, 0)`.
   - Arrastra el hijo visual (`NewTrigger_Visuals`, o el modelo 3D de la tubería) al campo **`Visual Transform`** de `SteppedPositioner` (así gira el modelo y no el collider). **Importante:** orienta el modelo 3D hijo para que a `0°` (`Element 0`) sus bocas apunten hacia los mismos lados que marcaste en `Open Sides`.
   - En **`PipeTileButton`** de `Tram A`, arrastra el `SteppedPositioner` al campo **`Visual Positioner`**. Así al empezar o cargar partida el modelo se pondrá solo en su giro inicial.
   - **Crea `Tram B` duplicando:** duplica `Tram A` en `Hierarchy` (`Ctrl+D`), llámalo `Tram B`, muévelo a la derecha de `Tram A`, cambia su `PipeTileButton > Tile Id` a **`pipe_b`** y comprueba que su `Visual Transform` apunta al hijo visual de `Tram B`.
7. **Ejemplo práctico para ampliar el puzle (cuadrícula 2×2 de 4 piezas).** Si quieres hacer un recorrido de 4 piezas en un cuadrado 2×2 (desde arriba a la izquierda hasta abajo a la derecha), configura estas 4 entradas en `PipePuzzle > Tiles` y duplica hasta tener 4 tramos 3D (`pipe_a`, `pipe_b`, `pipe_c`, `pipe_d`) colocados igual en la escena:
   - **`pipe_a` (Arriba-Izquierda, inicio)**: `Row = 0, Column = 0` · `Open Sides = East` (sale hacia la derecha) · `Starting Rotation Steps = 1`.
   - **`pipe_b` (Arriba-Derecha, codo)**: `Row = 0, Column = 1` · `Open Sides = West | South` (entra por la izquierda y baja hacia abajo) · `Starting Rotation Steps = 2`.
   - **`pipe_c` (Abajo-Izquierda, señuelo opcional)**: `Row = 1, Column = 0` · `Open Sides = North | East` · `Starting Rotation Steps = 0`.
   - **`pipe_d` (Abajo-Derecha, final)**: `Row = 1, Column = 1` · `Open Sides = North` (recibe el agua que baja de `pipe_b`) · `Starting Rotation Steps = 3`.
   - Pon **`Source Tile Id = pipe_a`** y **`Sink Tile Id = pipe_d`**. Cuando `pipe_a`, `pipe_b` y `pipe_d` lleguen al paso `0`, el camino `(0,0) → (0,1) → (1,1)` quedará unido y el puzle se resolverá.
8. En `Project`, abre tu carpeta bajo `Assets`. Haz clic derecho en un espacio vacío y elige `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Llama al asset `Def_sala1_puzle` o usa un nombre fácil de reconocer.
9. Selecciona la definición en `Project`. En `Inspector`, escribe un `Persistent Id` único, por ejemplo `sala1_teclat` para el teclado de la sala 1. Rellena `Display Name` y `Objective`. Selecciona la raíz del puzle en `Hierarchy` y arrastra el asset desde `Project` a `Definition`, en el componente del puzle en `Inspector`.
10. Para añadir pistas, haz clic derecho en la misma carpeta de `Project` y elige `Create > Escape Room Framework > Hints > Hint Data`. Selecciona el `Hint Data` y añade los textos a su lista `Hints`. Selecciona la `Puzzle Definition` y arrastra el `Hint Data` de `Project` al campo `Hints` de la definición.
11. Para abrir una puerta al resolver el puzle, créala desde `Escape Room Framework > Create > Interactables > Door`. En `Hierarchy`, selecciona `NewDoor_Logic` y, en el componente `Door` de `Inspector`, marca `Is Locked`. Coloca la puerta en la escena.
12. En `Hierarchy`, selecciona la raíz del puzle. Mantén `Ctrl` en Windows o `Cmd` en Mac y pulsa `NewDoor_Logic` para seleccionar ambos. Ejecuta `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Selecciona solo la raíz y comprueba que `On Solved` contiene `Door.Unlock` y `Door.ForceOpen`. Guarda con `Ctrl+S` o `Cmd+S` y entra en `Play`.

**Cómo lo pruebas**

- En el ejemplo inicial de 2 piezas (`pipe_a` y `pipe_b`), entra en Play y pulsa dos veces cada pieza: al llegar al paso 0 se conectan y se abre la puerta. Una vez resuelto, las piezas dejan de girar.
- Pruébalo en Play: guarda con F5 a mitad del puzle y una vez resuelto, sal y carga con F9. El estado debe ser el mismo y la puerta debe seguir abierta.

**Ojo con**

- Si una pieza gira pero el puzle nunca se resuelve, revisa 3 cosas: 1) que `Source Tile Id` y `Sink Tile Id` existan en `Tiles`, 2) que las casillas vecinas tengan `Row`/`Column` contiguos y bocas opuestas (`East` con `West`, `North` con `South`), y 3) que el modelo 3D en el paso 0 (`Element 0` del `SteppedPositioner`) apunte realmente hacia los lados marcados en `Open Sides`.
- Randomize Rotations baraja los giros iniciales en cada partida y se asegura de que nunca empiece ya resuelto.

<a id="slide"></a>
### Rompecabezas deslizante

Menú: `Escape Room Framework > Create > Puzzles > Sliding Puzzle` · Sala del museo 8

**Pasos**

1. Ejecuta `Escape Room Framework > Create > Puzzles > Sliding Puzzle`. En `Hierarchy`, abre `NewSlidingPuzzle` y selecciona `Board`. Importa una imagen arrastrando el archivo a tu carpeta de `Assets` en `Project`. Con `Board` seleccionado, arrastra la imagen de `Project` a `SlidingBoardView > Source Image` en `Inspector`. Pulsa `Re-apply layout and image` si necesitas actualizarla.
2. Para reducir el tablero 3×3 a 2×2, selecciona `NewSlidingPuzzle` y pon `Columns = 2`, `Rows = 2` y `Hole Cell` en `X = 1`, `Y = 1`. Selecciona `Board` y pulsa `Rebuild board`. Quedarán tres fichas y un hueco. Usa el mismo botón para ampliar la cuadrícula. Con `Custom Target Order`, adapta el orden al tamaño nuevo.
3. En `Project`, abre tu carpeta bajo `Assets`. Haz clic derecho en un espacio vacío y elige `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Llama al asset `Def_sala1_puzle` o usa un nombre fácil de reconocer.
4. Selecciona la definición en `Project`. En `Inspector`, escribe un `Persistent Id` único, por ejemplo `sala1_teclat` para el teclado de la sala 1. Rellena `Display Name` y `Objective`. Selecciona la raíz del puzle en `Hierarchy` y arrastra el asset desde `Project` a `Definition`, en el componente del puzle en `Inspector`.
5. Para añadir pistas, haz clic derecho en la misma carpeta de `Project` y elige `Create > Escape Room Framework > Hints > Hint Data`. Selecciona el `Hint Data` y añade los textos a su lista `Hints`. Selecciona la `Puzzle Definition` y arrastra el `Hint Data` de `Project` al campo `Hints` de la definición.
6. Para abrir una puerta al resolver el puzle, créala desde `Escape Room Framework > Create > Interactables > Door`. En `Hierarchy`, selecciona `NewDoor_Logic` y, en el componente `Door` de `Inspector`, marca `Is Locked`. Coloca la puerta en la escena.
7. En `Hierarchy`, selecciona la raíz del puzle. Mantén `Ctrl` en Windows o `Cmd` en Mac y pulsa `NewDoor_Logic` para seleccionar ambos. Ejecuta `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Selecciona solo la raíz y comprueba que `On Solved` contiene `Door.Unlock` y `Door.ForceOpen`. Guarda con `Ctrl+S` o `Cmd+S` y entra en `Play`.

**Cómo lo pruebas**

- Solo se mueve una ficha vecina del hueco; las demás hacen un pequeño rebote.
- Pruébalo en Play: guarda con F5 a mitad del puzle y una vez resuelto, sal y carga con F9. El estado debe ser el mismo y la puerta debe seguir abierta.

**Ojo con**


<a id="lights"></a>
### Circuito de luces

Menú: `Escape Room Framework > Create > Puzzles > Linked Lights Puzzle` · Sala del museo 14

**Pasos**

1. Ejecuta `Escape Room Framework > Create > Puzzles > Linked Lights Puzzle`. Selecciona `LinkedLights_Kit` en `Hierarchy`. Abre su triángulo para ver los cinco botones y `ResetButton`. Selecciona la raíz y abre `LinkedLightsPuzzle > Nodes` en `Inspector`. Los botones empiezan en 1 y los índices en 0.
2. Para añadir una luz, duplica un botón en `Hierarchy`, dale un `Index` nuevo en `LinkedLightButton` y añade una fila en `Nodes`. Selecciona la raíz y arrastra el botón duplicado al campo `Indicator` de la fila nueva. Revisa `Connections` de las filas que quieras enlazar.
3. Para pasar de cinco luces a cuatro, elimina `Element 4` de `Nodes` y borra `LightButton_5` y `Number_5` en `Hierarchy`. En `Element 3 > Connections`, deja `2, 3`. Si quitas una luz intermedia, corrige los `Index` y las `Connections` que cambien de número. Para este ejemplo de cuatro luces, desmarca todos los `Initially On`, marca todos los `Target On` y prueba los botones 1 y 4.
4. Si aún tienes la 0.2.1 y falta el menú, abre `ShowcaseMuseum` desde `Project > Assets > _EscapeRoomTemplate > Scenes`. En `Hierarchy`, abre `Room14_LinkedLights` y arrastra `LinkedLights_Kit` a tu carpeta en `Project` para crear un prefab. Vuelve a tu escena y arrástralo. Quita referencias de la demo de `On Solved` y `ResetButton`. Conserva `LinkedLightsPuzzle.ResetPuzzle` en el botón. Asigna tu propia definición.
5. En `Project`, abre tu carpeta bajo `Assets`. Haz clic derecho en un espacio vacío y elige `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Llama al asset `Def_sala1_puzle` o usa un nombre fácil de reconocer.
6. Selecciona la definición en `Project`. En `Inspector`, escribe un `Persistent Id` único, por ejemplo `sala1_teclat` para el teclado de la sala 1. Rellena `Display Name` y `Objective`. Selecciona la raíz del puzle en `Hierarchy` y arrastra el asset desde `Project` a `Definition`, en el componente del puzle en `Inspector`.
7. Para añadir pistas, haz clic derecho en la misma carpeta de `Project` y elige `Create > Escape Room Framework > Hints > Hint Data`. Selecciona el `Hint Data` y añade los textos a su lista `Hints`. Selecciona la `Puzzle Definition` y arrastra el `Hint Data` de `Project` al campo `Hints` de la definición.
8. Para abrir una puerta al resolver el puzle, créala desde `Escape Room Framework > Create > Interactables > Door`. En `Hierarchy`, selecciona `NewDoor_Logic` y, en el componente `Door` de `Inspector`, marca `Is Locked`. Coloca la puerta en la escena.
9. En `Hierarchy`, selecciona la raíz del puzle. Mantén `Ctrl` en Windows o `Cmd` en Mac y pulsa `NewDoor_Logic` para seleccionar ambos. Ejecuta `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Selecciona solo la raíz y comprueba que `On Solved` contiene `Door.Unlock` y `Door.ForceOpen`. Guarda con `Ctrl+S` o `Cmd+S` y entra en `Play`.

**Cómo lo pruebas**

- Con la configuración de muestra se resuelve pulsando 1, 3 y 5. El botón de reinicio vuelve al estado inicial.
- Pruébalo en Play: guarda con F5 a mitad del puzle y una vez resuelto, sal y carga con F9. El estado debe ser el mismo y la puerta debe seguir abierta.

**Ojo con**

- Asegúrate de que tiene solución: parte del objetivo y aplica unas cuantas pulsaciones para obtener el estado inicial.

<a id="throw"></a>
### Lanzamiento a dianas

Menú: `Escape Room Framework > Create > Puzzles > Throw Puzzle` · Sala del museo 6

**Pasos**

1. Ejecuta `Escape Room Framework > Create > Puzzles > Throw Puzzle`. En `Hierarchy`, abre el triángulo de `NewThrowPuzzle`. Verás `Target1_Logic`, `Target2_Logic` y `Target3_Logic`. Cada diana tiene `ThrowTarget` y un collider sólido.
2. Para quedarte con una diana, selecciona `Target2_Logic` y `Target3_Logic` en `Hierarchy` y elimínalos con `Delete`. Conserva `Target1_Logic` y su hijo visual. Selecciona `NewThrowPuzzle`. En `Inspector > ThrowPuzzle > Target Ids`, pon el tamaño de lista en `1` o elimina las dos filas restantes con `–`. Debe quedar solo `Element 0 = target_1`.
3. Selecciona `Target1_Logic` y verifica en `Inspector` que `ThrowTarget > Target Id` dice `target_1`. Si falta `Puzzle`, arrastra `NewThrowPuzzle` desde `Hierarchy` a ese campo. Para añadir otra diana, duplica el objeto `_Logic`, cambia `Target Id` y añade el mismo ID en `Target Ids` de la raíz.
4. Crea un objeto para lanzar con `Escape Room Framework > Create > Interactables > Physics Grabbable`. Selecciona `NewGrabbable_Logic` en `Hierarchy`. En `Inspector`, comprueba `Can Be Thrown` en `PhysicsGrabbable` y que el collider esté activo, con `Is Trigger` desmarcado, y que `Is Kinematic` esté desmarcado en `Rigidbody`. Colócalo al alcance. En `Play` en PC, míralo y pulsa `E`, apunta a la diana y lanza con clic izquierdo. Con una diana, un impacto por encima de `Min Impact Speed` debe resolver el puzle.
5. En `Project`, abre tu carpeta bajo `Assets`. Haz clic derecho en un espacio vacío y elige `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Llama al asset `Def_sala1_puzle` o usa un nombre fácil de reconocer.
6. Selecciona la definición en `Project`. En `Inspector`, escribe un `Persistent Id` único, por ejemplo `sala1_teclat` para el teclado de la sala 1. Rellena `Display Name` y `Objective`. Selecciona la raíz del puzle en `Hierarchy` y arrastra el asset desde `Project` a `Definition`, en el componente del puzle en `Inspector`.
7. Para añadir pistas, haz clic derecho en la misma carpeta de `Project` y elige `Create > Escape Room Framework > Hints > Hint Data`. Selecciona el `Hint Data` y añade los textos a su lista `Hints`. Selecciona la `Puzzle Definition` y arrastra el `Hint Data` de `Project` al campo `Hints` de la definición.
8. Para abrir una puerta al resolver el puzle, créala desde `Escape Room Framework > Create > Interactables > Door`. En `Hierarchy`, selecciona `NewDoor_Logic` y, en el componente `Door` de `Inspector`, marca `Is Locked`. Coloca la puerta en la escena.
9. En `Hierarchy`, selecciona la raíz del puzle. Mantén `Ctrl` en Windows o `Cmd` en Mac y pulsa `NewDoor_Logic` para seleccionar ambos. Ejecuta `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Selecciona solo la raíz y comprueba que `On Solved` contiene `Door.Unlock` y `Door.ForceOpen`. Guarda con `Ctrl+S` o `Cmd+S` y entra en `Play`.

**Cómo lo pruebas**

- En PC: E para coger, clic izquierdo para lanzar y Q para soltar. En VR: coge con el grip y suelta haciendo el gesto.
- Pruébalo en Play: guarda con F5 a mitad del puzle y una vez resuelto, sal y carga con F9. El estado debe ser el mismo y la puerta debe seguir abierta.

**Opcional · Clavar uno o varios cuchillos 3D en la diana (sin tocar la plantilla)**

1. **Prepara el cuchillo (`PhysicsGrabbable` + `GrabbablePiece`).** En tu objeto lanzable (`Ganivet_Logic`), añade el componente `GrabbablePiece` con `Add Component`. Además de permitir clavarlo, `GrabbablePiece` trae `Return When Lost` activo por defecto: si el tiro falla y cae lejos o bajo el escenario, vuelve solo a su posición inicial.
2. **Marca el punto y ángulo finales (`PuntClavat`).** Haz clic derecho sobre `Target1_Logic` en `Hierarchy` → `Create Empty`, llámalo `PuntClavat` y sitúalo y gíralo con la punta mirando hacia la diana.
3. **Conecta el evento (`GrabbablePiece.LockInPlace`).** Para 1 diana y 1 cuchillo, en `ThrowPuzzle > On Solved` pulsa `+`, arrastra `Ganivet_Logic`, selecciona `GrabbablePiece > LockInPlace (Transform)` y arrastra `PuntClavat` al campo `Transform`. `LockInPlace` congela el `Rigidbody` (`isKinematic = true`), lo hace hijo de la diana y desactiva el outline y el agarre (PC y VR).
4. **Múltiples dianas y cuchillos sin código (`MultiStagePuzzle`).** Si tienes 3 dianas y 3 cuchillos y quieres que cada uno se clave al acertar su diana sin código, agrupa 3 `ThrowPuzzle` de 1 diana dentro de un `MultiStagePuzzle` (con `Require Order` desmarcado) y conecta el `On Solved` de cada hijo al `LockInPlace` de su cuchillo.
5. **Múltiples dianas con cualquier cuchillo al instante (micro-script en tu carpeta).** Si quieres que cualquier cuchillo se clave al instante en cualquier diana al chocar, añade en cada diana un script propio (en `Assets/MiJuego/StickKnifeOnTarget.cs`) que en `OnCollisionEnter` compruebe `collision.relativeVelocity.magnitude >= _minImpactSpeed` y llame a `piece.LockInPlace(_snapPoint)`.

**Ojo con**


<a id="place"></a>
### Colocación de piezas

Menú: `Escape Room Framework > Create > Puzzles > Placement Puzzle` · Sala del museo 7

**Pasos**

1. Ejecuta `Escape Room Framework > Create > Puzzles > Placement Puzzle`. Abre `NewPlacementPuzzle` en `Hierarchy`. `PieceA_Logic` y `PieceB_Logic` son las piezas y `SocketA_Logic`, `SocketB_Logic` y `SocketC_Logic` son los encajes. Selecciona la raíz y abre `PlacementPuzzle > Rules` en `Inspector`.
2. Para añadir una pareja, duplica una pieza y un encaje completos en `Hierarchy`. En el `GrabbablePiece` de la pieza nueva, escribe `Piece Id = piece_c`. En el `PieceSocketReceiver` del encaje nuevo, escribe `Socket Id = socket_d`. Si falta `Puzzle`, arrastra `NewPlacementPuzzle`. En la raíz, pulsa `+` en `Rules` y escribe `piece_c` en `Piece Id` y `socket_d` en `Correct Socket Id`.
3. Para quedarte con la pieza A, elimina de `Rules` la fila `piece_b → socket_b`. En `Hierarchy`, borra `PieceB_Logic` y `SocketB_Logic`. Si quieres quitar el señuelo, borra `SocketC_Logic`. Como este no tiene regla, la lista queda igual. Conserva `piece_a → socket_a` y coloca ambos objetos al alcance.
4. En `Project`, abre tu carpeta bajo `Assets`. Haz clic derecho en un espacio vacío y elige `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Llama al asset `Def_sala1_puzle` o usa un nombre fácil de reconocer.
5. Selecciona la definición en `Project`. En `Inspector`, escribe un `Persistent Id` único, por ejemplo `sala1_teclat` para el teclado de la sala 1. Rellena `Display Name` y `Objective`. Selecciona la raíz del puzle en `Hierarchy` y arrastra el asset desde `Project` a `Definition`, en el componente del puzle en `Inspector`.
6. Para añadir pistas, haz clic derecho en la misma carpeta de `Project` y elige `Create > Escape Room Framework > Hints > Hint Data`. Selecciona el `Hint Data` y añade los textos a su lista `Hints`. Selecciona la `Puzzle Definition` y arrastra el `Hint Data` de `Project` al campo `Hints` de la definición.
7. Para abrir una puerta al resolver el puzle, créala desde `Escape Room Framework > Create > Interactables > Door`. En `Hierarchy`, selecciona `NewDoor_Logic` y, en el componente `Door` de `Inspector`, marca `Is Locked`. Coloca la puerta en la escena.
8. En `Hierarchy`, selecciona la raíz del puzle. Mantén `Ctrl` en Windows o `Cmd` en Mac y pulsa `NewDoor_Logic` para seleccionar ambos. Ejecuta `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Selecciona solo la raíz y comprueba que `On Solved` contiene `Door.Unlock` y `Door.ForceOpen`. Guarda con `Ctrl+S` o `Cmd+S` y entra en `Play`.

**Cómo lo pruebas**

- Prueba piezas intercambiadas, sacarlas antes de resolver y cargar una vez resuelto: las piezas quedan fijadas.
- Si guardas a mitad del puzle, al cargar las piezas sueltas vuelven a su sitio inicial.

**Ojo con**

- Randomize Mapping cambia qué soporte es el correcto en cada partida: la pista debe ser dinámica.

<a id="multi"></a>
### Puzles encadenados

Menú: `Escape Room Framework > Create > Puzzles > Multi-Stage Puzzle` · Sala del museo 11

**Pasos**

1. Ejecuta `Escape Room Framework > Create > Puzzles > Multi-Stage Puzzle`. En `Hierarchy`, abre `MultiStageChainPuzzle` y luego `MultiStage_Generated`. Hay un puzle de secuencia y otro de palancas. Para editarlos, selecciona el hijo de la fase. Para cambiar el orden, selecciona la raíz y abre `MultiStagePuzzle > Puzzles` en `Inspector`.
2. Para añadir una fase, crea un puzle con su menú y colócalo. En la raíz del grupo, pulsa `+` en `Puzzles` y pon un `Id` diferente. Arrastra la raíz del puzle nuevo de `Hierarchy` a `Puzzle`. Asigna en `Interaction Root` el objeto que contiene los controles, normalmente esa misma raíz.
3. Para quitar una fase, elimina su fila de `Puzzles` y retira su objeto de `Hierarchy` si no lo quieres en la sala. Conserva al menos un puzle en el grupo. Con `Require Order`, las fases siguen el orden de la lista. `Lock Future Puzzles` evita usar fases posteriores antes de tiempo. Conecta la puerta a la raíz del grupo.
4. En `Project`, abre tu carpeta bajo `Assets`. Haz clic derecho en un espacio vacío y elige `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Llama al asset `Def_sala1_puzle` o usa un nombre fácil de reconocer.
5. Selecciona la definición en `Project`. En `Inspector`, escribe un `Persistent Id` único, por ejemplo `sala1_teclat` para el teclado de la sala 1. Rellena `Display Name` y `Objective`. Selecciona la raíz del puzle en `Hierarchy` y arrastra el asset desde `Project` a `Definition`, en el componente del puzle en `Inspector`.
6. Para añadir pistas, haz clic derecho en la misma carpeta de `Project` y elige `Create > Escape Room Framework > Hints > Hint Data`. Selecciona el `Hint Data` y añade los textos a su lista `Hints`. Selecciona la `Puzzle Definition` y arrastra el `Hint Data` de `Project` al campo `Hints` de la definición.
7. Para abrir una puerta al resolver el puzle, créala desde `Escape Room Framework > Create > Interactables > Door`. En `Hierarchy`, selecciona `NewDoor_Logic` y, en el componente `Door` de `Inspector`, marca `Is Locked`. Coloca la puerta en la escena.
8. En `Hierarchy`, selecciona la raíz del puzle. Mantén `Ctrl` en Windows o `Cmd` en Mac y pulsa `NewDoor_Logic` para seleccionar ambos. Ejecuta `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Selecciona solo la raíz y comprueba que `On Solved` contiene `Door.Unlock` y `Door.ForceOpen`. Guarda con `Ctrl+S` o `Cmd+S` y entra en `Play`.

**Cómo lo pruebas**

- La puerta se abre solo cuando todos los hijos están resueltos.
- Pruébalo en Play: guarda con F5 a mitad del puzle y una vez resuelto, sal y carga con F9. El estado debe ser el mismo y la puerta debe seguir abierta.

**Ojo con**


## Sistemas de la sala

<a id="key"></a>
### Llave que abre una puerta

Menú: `Create > Escape Room Framework > Inventory > Item` + `Escape Room Framework > Create > Interactables > Pickable Item · Door` · Sala del museo 1 · 3

**Pasos**

1. **Crea la ficha de la llave (`InventoryItemData`).** En el panel `Project`, entra en tu carpeta bajo `Assets`. Haz clic derecho en un espacio vacío y elige `Create > Escape Room Framework > Inventory > Item`. Llama al asset `Item_LlaveDespacho`. Selecciónalo y, en `Inspector`, escribe en `Item Id` un identificador único en minúsculas y sin espacios, como `clau_despatx` o `llave_despacho`. Rellena `Display Name` (por ejemplo `Llave del despacho`) y `Description`. Si no tienes un icono propio para `Icon`, puedes generarlo automáticamente más tarde con `Escape Room Framework > Maintenance > Generate Missing Item Icons`.
2. **Registra la llave en el catálogo (`DefaultItemCatalog`).** En el panel `Project`, selecciona `Assets/_EscapeRoomTemplate/ScriptableObjects/DefaultItemCatalog.asset` (el catálogo que usa `InventoryManager` dentro de `GameManager`). En `Inspector`, despliega `Items`, pulsa `+` y arrastra tu asset `Item_LlaveDespacho` desde `Project`. Esto permite que el guardado y la carga reconozcan la llave.
3. **Coloca la llave en la escena (`Pickable Item`).** En el menú superior ejecuta `Escape Room Framework > Create > Interactables > Pickable Item`. En `Hierarchy`, selecciona `NewPickableItem_Logic` (puedes renombrarlo a `LlaveDespacho_Logic`) y muévelo donde el jugador deba encontrarlo (sobre una mesa, en el suelo o dentro de un cajón). Con `LlaveDespacho_Logic` seleccionado en `Hierarchy`, arrastra el asset `Item_LlaveDespacho` desde `Project` hasta el campo `Item Data` del componente `Pickable Item` en `Inspector`. Deja `Quantity` en `1`.
4. **Crea la puerta y ciérrala con la llave (`Door`).** En el menú superior ejecuta `Escape Room Framework > Create > Interactables > Door` (o selecciona una puerta, armario o cajón existente). En `Hierarchy`, selecciona `NewDoor_Logic` y mira el componente `Door` en `Inspector`. Marca `Is Locked` y, en el campo `Required Item Id`, escribe exactamente el mismo texto que pusiste en `Item Id` de la llave (`clau_despatx` o `llave_despacho`).
5. **Elige cómo se usa la llave (`Item Use Policy`).** En el mismo componente `Door`, deja `Item Use Policy` en `Offer Compatible` (por defecto: al pulsar `E` ante la puerta cerrada, se abre un selector en el HUD con la llave para confirmar su uso, y la siguiente pulsación de `E` abre la puerta) o cámbialo a `Auto Use Single` si quieres que al pulsar `E` se desbloquee y se abra directamente sin menú cuando el jugador lleva la llave. Marca `Consume Required Item` si la llave debe gastarse, o desmárcalo si sirve para más de una puerta.
6. **Alternativa con cerradura separada (`Item Receiver`).** Si prefieres una cerradura o pedestal separado donde aparezca el modelo 3D de la llave, ejecuta `Escape Room Framework > Create > Interactables > Item Receiver`. Arrastra `Item_LlaveDespacho` al campo `Required Item` (aquí se arrastra el asset, no el texto) y, en `On Item Accepted`, pulsa `+`, arrastra la puerta desde `Hierarchy` y selecciona `Door.Unlock` (y una segunda fila con `Door.ForceOpen` si quieres que se abra sola).

**Cómo lo pruebas**

- Entra en `Play`, acércate a la puerta antes de coger la llave y pulsa `E`: debe seguir cerrada y mostrar el mensaje de bloqueo.
- Mira la llave y pulsa `E` para recogerla (puedes comprobar que está en el inventario con `I`). Vuelve a la puerta y pulsa `E`: con `Offer Compatible`, confirma la llave en el selector y pulsa `E` para abrir; con `Auto Use Single`, se abre directamente.
- Guarda con `F5` tras abrir la puerta y carga con `F9`: la llave recogida no vuelve a aparecer en la escena y la puerta sigue desbloqueada y abierta.

**Ojo con**

- `Required Item Id` en `Door` es un texto y debe coincidir **exactamente** (mayúsculas, minúsculas y guiones) con `Item Id` del `InventoryItemData`, no con el nombre del archivo ni con `Display Name`.
- Si no añades el asset al `DefaultItemCatalog`, la llave desaparecerá del inventario al cargar una partida guardada.

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

