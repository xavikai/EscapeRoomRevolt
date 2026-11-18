# Receptes pas a pas

**Idioma:** [català](RECEPTES.md) · [castellano](RECETAS.md) · [English](RECIPES.md)

Les mateixes receptes es poden seguir amb caselles de progrés a la [guia interactiva](https://github.com/xavikai/EscapeRoomRevolt/releases/download/escaperoom-v0.2.2/guia-escape-room-0.2.2.html), a *Receptes pas a pas*.

## Abans de començar a Unity

1. Obre la teva escena en mode edició. Si el botó `Play` està actiu, torna'l a prémer abans de crear o configurar res. Si no veus els panells, obre'ls des de `Window > General > Hierarchy`, `Project` i `Inspector`.
2. `Hierarchy` conté els objectes de l'escena. Selecciona-hi un objecte amb un clic i mira els seus components a `Inspector`. L'arrel del puzle és l'objecte pare que conté el controlador. Clica el triangle del seu nom per veure els fills. Després de crear un puzle, prem `F` amb el cursor a `Scene` per centrar la vista en l'objecte seleccionat.
3. `Project` conté fitxers i assets. Crea els teus dins d'una carpeta pròpia sota `Assets`. Per assignar una referència, deixa seleccionat a `Hierarchy` l'objecte que rep el valor i arrossega l'origen des de `Project` o `Hierarchy` fins al camp que diu `None` a `Inspector`. Clicar l'origen en comptes d'arrossegar-lo canvia la selecció.
4. Per a una prova de PC, la teva escena necessita terra amb collider, un `GameManager` i un jugador. Si falten, crea el gestor i el jugador amb `Escape Room Framework > Setup > Instantiate Game Manager` i `Escape Room Framework > Setup > Instantiate PC Player`. Col·loca el jugador sobre el terra, a prop del puzle. Si l'escena ja els té, fes servir els existents.
5. Quan treguis elements, revisa la llista del controlador i actualitza els textos, les pistes i la solució. Comença una partida nova per provar la configuració. Si prefereixes experimentar, duplica l'escena de prova des de `Project` abans d'editar-la.

- Puzles: [Teclat de codi](#code) · [Seqüència i melodia](#seq) · [Palanques i interruptors (StatePuzzle)](#state) · [Rodets numèrics (cadenat)](#wheels) · [Canonades](#pipe) · [Trencaclosques lliscant](#slide) · [Circuit de llums](#lights) · [Llançament a dianes](#throw) · [Col·locació de peces](#place) · [Puzles encadenats](#multi)
- Sistemes de la sala: [Clau que obre una porta](#key) · [Objectes, inventari, combinació i examen](#items) · [Portes, calaixos i armaris](#doors) · [Pistes i narració](#hints) · [Objectius i final de partida](#objectives) · [Temporitzador i perill mòbil](#hazard) · [Canvi de sala o d'escena](#rooms)

## Puzles

<a id="code"></a>
### Teclat de codi

Menú: `Escape Room Framework > Create > Puzzles > Keypad Panel` · Sala del museu 2

**Passos**

1. Al menú superior executa `Escape Room Framework > Create > Puzzles > Keypad Panel`. Selecciona `NewKeypad_Logic` a `Hierarchy` i mou l'objecte sencer a la paret amb l'eina `Move`. Els botons ja estan connectats.
2. Al component `CodePanelPuzzle` de l'`Inspector`, posa `Correct Code` a `3142` i `Max Code Length` a `4`. Deixa `Auto Check When Full` activat i `Randomize Code` desactivat. Si vols un codi més curt, canvia els dos camps. Per exemple, `42` i `2`. No cal eliminar botons.
3. Per donar una pista, crea una nota amb `Escape Room Framework > Create > Interactables > Note`. Selecciona `NewNote_Logic` a `Hierarchy` i escriu la pista a `Note Content`, dins d'`InteractableNote` a l'`Inspector`. Col·loca la nota on el jugador la pugui llegir.
4. A `Project`, obre la carpeta d'`Assets` on guardes el teu puzle. Fes clic dret en un espai buit i tria `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Anomena l'asset `Def_sala1_puzle` o posa-li un nom que reconeguis.
5. Selecciona la definició a `Project`. A `Inspector`, escriu un `Persistent Id` únic, com `sala1_teclat` si és el teclat de la sala 1. Omple `Display Name` i `Objective`. Selecciona l'arrel del puzle a `Hierarchy` i arrossega l'asset del `Project` a `Definition`, al component del puzle de l'`Inspector`.
6. Si vols pistes, a la mateixa carpeta de `Project` fes clic dret i tria `Create > Escape Room Framework > Hints > Hint Data`. Selecciona el `Hint Data` i afegeix els textos a la seva llista `Hints`. Després selecciona la `Puzzle Definition` i arrossega el `Hint Data` del `Project` al camp `Hints` de la definició.
7. Per obrir una porta en resoldre el puzle, crea-la des del menú superior `Escape Room Framework > Create > Interactables > Door`. A `Hierarchy`, selecciona `NewDoor_Logic` i, al component `Door` de l'`Inspector`, marca `Is Locked`. Col·loca la porta on vols que estigui a l'escena.
8. A `Hierarchy`, selecciona l'arrel del puzle. Mantén `Ctrl` a Windows o `Cmd` a Mac i clica `NewDoor_Logic` perquè quedin seleccionats tots dos. Executa `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Torna a seleccionar només l'arrel i comprova que `On Solved` conté `Door.Unlock` i `Door.ForceOpen`. Desa l'escena amb `Ctrl+S` o `Cmd+S` i entra a `Play`.

**Com ho proves**

- Un codi erroni fa parpellejar el LED vermell i esborra l'intent; el correcte el posa verd i obre la porta.
- Prova-ho a Play: guarda amb F5 a mig puzle i un cop resolt, surt i carrega amb F9. L'estat ha de ser el mateix i la porta ha de continuar oberta.

**Compte amb**

- Randomize Code canvia el codi a cada partida: activa'l només si la pista també és dinàmica.
- On Solved només s'executa quan el jugador resol el puzle, no en carregar una partida: la porta recupera el seu estat pel seu propi guardat.

<a id="seq"></a>
### Seqüència i melodia

Menú: `Escape Room Framework > Create > Puzzles > Sequence Puzzle` · Sala del museu 4 · 9

**Passos**

1. Executa `Escape Room Framework > Create > Puzzles > Sequence Puzzle`. A `Hierarchy`, selecciona `NewSequencePuzzle` i obre el seu triangle per veure els tres botons. Selecciona l'arrel i desplega `SequencePuzzle > Correct Sequence` a l'`Inspector`. L'ordre inicial és `red`, `green`, `blue`.
2. Per afegir un botó, duplica un botó sencer a `Hierarchy` i edita el `Step Id` del seu `SequenceStepButton`. A l'arrel, afegeix el mateix text a la llista `Correct Sequence`. Per treure el botó verd, elimina totes les entrades `green` de la llista i el seu objecte `greenButton_Logic` de `Hierarchy`. Per escurçar només la seqüència, treu-ne passos i deixa els botons com a opcions. Conserva com a mínim un pas.
3. Si vols una melodia, selecciona cada botó a `Hierarchy`, afegeix `Audio Source` amb `Add Component`, arrossega el clip de `Project` al seu camp de clip i desmarca `Play On Awake`. Al `On Interact Event` del botó, afegeix una entrada amb `+`, arrossega el botó a l'entrada i tria `AudioSource > Play`. Conserva l'entrada que comunica el pas al puzle.
4. Per reproduir la melodia de referència, afegeix `MelodyPlayer` a l'arrel amb `Add Component`. A `Notes`, crea una entrada per cada pas i arrossega-hi el botó corresponent, en ordre. Crea un `Generic Trigger` des del menú `Create > Interactables`. Al seu `On Interact Event`, prem `+`, arrossega-hi `NewSequencePuzzle` des de `Hierarchy` i tria `MelodyPlayer > Play`. Quan treguis passos, actualitza també `Notes`.
5. A `Project`, obre la carpeta d'`Assets` on guardes el teu puzle. Fes clic dret en un espai buit i tria `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Anomena l'asset `Def_sala1_puzle` o posa-li un nom que reconeguis.
6. Selecciona la definició a `Project`. A `Inspector`, escriu un `Persistent Id` únic, com `sala1_teclat` si és el teclat de la sala 1. Omple `Display Name` i `Objective`. Selecciona l'arrel del puzle a `Hierarchy` i arrossega l'asset del `Project` a `Definition`, al component del puzle de l'`Inspector`.
7. Si vols pistes, a la mateixa carpeta de `Project` fes clic dret i tria `Create > Escape Room Framework > Hints > Hint Data`. Selecciona el `Hint Data` i afegeix els textos a la seva llista `Hints`. Després selecciona la `Puzzle Definition` i arrossega el `Hint Data` del `Project` al camp `Hints` de la definició.
8. Per obrir una porta en resoldre el puzle, crea-la des del menú superior `Escape Room Framework > Create > Interactables > Door`. A `Hierarchy`, selecciona `NewDoor_Logic` i, al component `Door` de l'`Inspector`, marca `Is Locked`. Col·loca la porta on vols que estigui a l'escena.
9. A `Hierarchy`, selecciona l'arrel del puzle. Mantén `Ctrl` a Windows o `Cmd` a Mac i clica `NewDoor_Logic` perquè quedin seleccionats tots dos. Executa `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Torna a seleccionar només l'arrel i comprova que `On Solved` conté `Door.Unlock` i `Door.ForceOpen`. Desa l'escena amb `Ctrl+S` o `Cmd+S` i entra a `Play`.

**Com ho proves**

- Un pas equivocat reinicia l'intent i dispara On Failed.
- Guarda després del primer pas correcte i carrega: has de poder continuar pel segon.
- Prova-ho a Play: guarda amb F5 a mig puzle i un cop resolt, surt i carrega amb F9. L'estat ha de ser el mateix i la porta ha de continuar oberta.

**Compte amb**

- Si només hi ha pista sonora, afegeix-ne una de visual: no tothom pot distingir les notes.
- Randomize Order barreja l'ordre a cada partida; la pista ha de mostrar l'ordre real.

<a id="state"></a>
### Palanques i interruptors (StatePuzzle)

Menú: `Escape Room Framework > Create > Puzzles > State Puzzle` · Sala del museu 5

**Passos**

1. Executa `Escape Room Framework > Create > Puzzles > State Puzzle`. Obre `NewStatePuzzle` a `Hierarchy`. Clica `Lever1_Logic` i desplega `SteppedPositioner > Positions` a l'`Inspector`. L'entrada `Element 0` és la primera posició. `Starting Index` indica com comença la palanca.
2. Selecciona l'arrel i obre `StatePuzzle > Conditions` a l'`Inspector`. Cada fila té un `Positioner` i un `Required Index`. Si hi afegeixes una palanca, duplica el seu objecte `_Logic`, prem `+` a `Conditions` i arrossega la palanca nova de `Hierarchy` al camp `Positioner` de la fila nova. Tria la posició requerida.
3. Per deixar dues palanques, selecciona a `Conditions` la fila que referencia `Lever3_Logic` i prem `–`. Després elimina `Lever3_Logic` de `Hierarchy`. Si redueixes les posicions d'una palanca, comprova que `Starting Index` i `Required Index` continuen dins la llista. Amb dues posicions només poden ser `0` o `1`.
4. A `Project`, obre la carpeta d'`Assets` on guardes el teu puzle. Fes clic dret en un espai buit i tria `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Anomena l'asset `Def_sala1_puzle` o posa-li un nom que reconeguis.
5. Selecciona la definició a `Project`. A `Inspector`, escriu un `Persistent Id` únic, com `sala1_teclat` si és el teclat de la sala 1. Omple `Display Name` i `Objective`. Selecciona l'arrel del puzle a `Hierarchy` i arrossega l'asset del `Project` a `Definition`, al component del puzle de l'`Inspector`.
6. Si vols pistes, a la mateixa carpeta de `Project` fes clic dret i tria `Create > Escape Room Framework > Hints > Hint Data`. Selecciona el `Hint Data` i afegeix els textos a la seva llista `Hints`. Després selecciona la `Puzzle Definition` i arrossega el `Hint Data` del `Project` al camp `Hints` de la definició.
7. Per obrir una porta en resoldre el puzle, crea-la des del menú superior `Escape Room Framework > Create > Interactables > Door`. A `Hierarchy`, selecciona `NewDoor_Logic` i, al component `Door` de l'`Inspector`, marca `Is Locked`. Col·loca la porta on vols que estigui a l'escena.
8. A `Hierarchy`, selecciona l'arrel del puzle. Mantén `Ctrl` a Windows o `Cmd` a Mac i clica `NewDoor_Logic` perquè quedin seleccionats tots dos. Executa `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Torna a seleccionar només l'arrel i comprova que `On Solved` conté `Door.Unlock` i `Door.ForceOpen`. Desa l'escena amb `Ctrl+S` o `Cmd+S` i entra a `Play`.

**Com ho proves**

- Només es resol quan totes les palanques són a la seva posició; l'ordre no importa.
- Guarda amb palanques a mitges i carrega: les posicions es conserven.
- Prova-ho a Play: guarda amb F5 a mig puzle i un cop resolt, surt i carrega amb F9. L'estat ha de ser el mateix i la porta ha de continuar oberta.

**Compte amb**

- Una condició sense Positioner impedeix resoldre el puzle.

<a id="wheels"></a>
### Rodets numèrics (cadenat)

Menú: `Escape Room Framework > Create > Puzzles > Number Wheels Puzzle` · Sala del museu 13

**Passos**

1. Executa `Escape Room Framework > Create > Puzzles > Number Wheels Puzzle`. A la finestra que apareix tria entre 2 i 8 rodets, posa una xifra per rodet i prem `Create puzzle`. L'objecte `NumberWheelsPuzzle` apareix a `Hierarchy` amb càmera i controls ja connectats.
2. Per afegir o treure rodets, selecciona l'arrel a `Hierarchy`. A `Inspector > NumberWheelsPuzzleAuthoring`, canvia `Wheel Count`, revisa totes les xifres i prem `Rebuild wheels and layout`. Per passar de quatre a tres, posa `Wheel Count = 3` i verifica les tres xifres abans de reconstruir. El botó refà el tauler i les seves condicions. Actualitza la pista del codi.
3. A `Project`, obre la carpeta d'`Assets` on guardes el teu puzle. Fes clic dret en un espai buit i tria `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Anomena l'asset `Def_sala1_puzle` o posa-li un nom que reconeguis.
4. Selecciona la definició a `Project`. A `Inspector`, escriu un `Persistent Id` únic, com `sala1_teclat` si és el teclat de la sala 1. Omple `Display Name` i `Objective`. Selecciona l'arrel del puzle a `Hierarchy` i arrossega l'asset del `Project` a `Definition`, al component del puzle de l'`Inspector`.
5. Si vols pistes, a la mateixa carpeta de `Project` fes clic dret i tria `Create > Escape Room Framework > Hints > Hint Data`. Selecciona el `Hint Data` i afegeix els textos a la seva llista `Hints`. Després selecciona la `Puzzle Definition` i arrossega el `Hint Data` del `Project` al camp `Hints` de la definició.
6. Per obrir una porta en resoldre el puzle, crea-la des del menú superior `Escape Room Framework > Create > Interactables > Door`. A `Hierarchy`, selecciona `NewDoor_Logic` i, al component `Door` de l'`Inspector`, marca `Is Locked`. Col·loca la porta on vols que estigui a l'escena.
7. A `Hierarchy`, selecciona l'arrel del puzle. Mantén `Ctrl` a Windows o `Cmd` a Mac i clica `NewDoor_Logic` perquè quedin seleccionats tots dos. Executa `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Torna a seleccionar només l'arrel i comprova que `On Solved` conté `Door.Unlock` i `Door.ForceOpen`. Desa l'escena amb `Ctrl+S` o `Cmd+S` i entra a `Play`.

**Com ho proves**

- A PC: E per entrar a la vista d'aproximació, clic a ▲/▼ i clic dret per sortir. A VR: botons físics ▲/▼ amb el gatell.
- Prova-ho a Play: guarda amb F5 a mig puzle i un cop resolt, surt i carrega amb F9. L'estat ha de ser el mateix i la porta ha de continuar oberta.

**Compte amb**

- Després de canviar el nombre de rodets, comprova que la combinació continua sent la que vols.

<a id="pipe"></a>
### Canonades

Menú: `Escape Room Framework > Create > Puzzles > Pipe Puzzle` · Sala del museu 10

**Passos**

1. Executa `Escape Room Framework > Create > Puzzles > Pipe Puzzle`. A `Hierarchy`, selecciona `NewPipePuzzle` i obre `PipePuzzle > Tiles` a l'`Inspector`. El menú crea el controlador amb `pipe_a` a fila 0 columna 0 i `pipe_b` a fila 0 columna 1. Ara construiràs els dos trams visibles.
2. Des del menú superior crea un `Generic Trigger` a `Escape Room Framework > Create > Interactables`. Selecciona `NewTrigger_Logic` a `Hierarchy`, anomena'l `Tram A` i posa'l a l'escena. A l'`Inspector`, prem `Add Component` i cerca `PipeTileButton`. Amb `Tram A` encara seleccionat, arrossega `NewPipePuzzle` de `Hierarchy` al camp `Puzzle` i escriu `pipe_a` a `Tile Id`.
3. A `Tram A`, desplega `InteractableTrigger > On Interact Event` i prem `+`. Arrossega `Tram A` de `Hierarchy` al camp d'objecte de la fila nova. Obre `No Function` i tria `PipeTileButton > Rotate`. Repeteix-ho per al `Tram B` amb `Tile Id = pipe_b` i posa'l al costat del primer.
4. Per veure el gir, afegeix un `SteppedPositioner` a l'arrel de cada tram. A `Positions`, prepara quatre rotacions de 0°, 90°, 180° i 270° al voltant del mateix eix. Arrossega el fill amb el model a `Visual Transform`. Al `PipeTileButton`, arrossega l'arrel d'aquell tram al camp `Visual Positioner`. El model ha de mostrar les obertures que has definit a `Open Sides`.
5. Si afegeixes un tram, crea el control i una fila amb el mateix `Tile Id` a `Tiles`. Tria una cel·la lliure amb `Row` i `Column`. Per treure'l, elimina el control i la fila del seu ID. Conserva un origen i un destí diferents, actualitza `Source Tile Id` i `Sink Tile Id` si cal, i comprova que les obertures encara permeten un camí. A l'exemple de dos trams, gira cada tram dues vegades.
6. A `Project`, obre la carpeta d'`Assets` on guardes el teu puzle. Fes clic dret en un espai buit i tria `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Anomena l'asset `Def_sala1_puzle` o posa-li un nom que reconeguis.
7. Selecciona la definició a `Project`. A `Inspector`, escriu un `Persistent Id` únic, com `sala1_teclat` si és el teclat de la sala 1. Omple `Display Name` i `Objective`. Selecciona l'arrel del puzle a `Hierarchy` i arrossega l'asset del `Project` a `Definition`, al component del puzle de l'`Inspector`.
8. Si vols pistes, a la mateixa carpeta de `Project` fes clic dret i tria `Create > Escape Room Framework > Hints > Hint Data`. Selecciona el `Hint Data` i afegeix els textos a la seva llista `Hints`. Després selecciona la `Puzzle Definition` i arrossega el `Hint Data` del `Project` al camp `Hints` de la definició.
9. Per obrir una porta en resoldre el puzle, crea-la des del menú superior `Escape Room Framework > Create > Interactables > Door`. A `Hierarchy`, selecciona `NewDoor_Logic` i, al component `Door` de l'`Inspector`, marca `Is Locked`. Col·loca la porta on vols que estigui a l'escena.
10. A `Hierarchy`, selecciona l'arrel del puzle. Mantén `Ctrl` a Windows o `Cmd` a Mac i clica `NewDoor_Logic` perquè quedin seleccionats tots dos. Executa `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Torna a seleccionar només l'arrel i comprova que `On Solved` conté `Door.Unlock` i `Door.ForceOpen`. Desa l'escena amb `Ctrl+S` o `Cmd+S` i entra a `Play`.

**Com ho proves**

- Es resol quan hi ha un camí continu d'obertures de l'origen al destí; un cop resolt, els trams deixen de girar.
- Prova-ho a Play: guarda amb F5 a mig puzle i un cop resolt, surt i carrega amb F9. L'estat ha de ser el mateix i la porta ha de continuar oberta.

**Compte amb**

- Randomize Rotations barreja els girs inicials i mai no comença resolt.

<a id="slide"></a>
### Trencaclosques lliscant

Menú: `Escape Room Framework > Create > Puzzles > Sliding Puzzle` · Sala del museu 8

**Passos**

1. Executa `Escape Room Framework > Create > Puzzles > Sliding Puzzle`. A `Hierarchy`, obre `NewSlidingPuzzle` i selecciona `Board`. Per posar una imatge, importa-la arrossegant el fitxer a una carpeta teva d'`Assets` dins de `Project`. Amb `Board` seleccionat, arrossega la imatge de `Project` a `SlidingBoardView > Source Image` a l'`Inspector`. Prem `Re-apply layout and image` si cal actualitzar-la.
2. Per reduir el tauler 3×3 a 2×2, selecciona `NewSlidingPuzzle` i posa `Columns = 2`, `Rows = 2` i `Hole Cell` a `X = 1`, `Y = 1`. Selecciona `Board` i prem `Rebuild board`. Obtindràs tres fitxes i un espai buit. Usa el mateix botó quan ampliïs la graella. Si has activat `Custom Target Order`, revisa l'ordre perquè encaixi amb la mida nova.
3. A `Project`, obre la carpeta d'`Assets` on guardes el teu puzle. Fes clic dret en un espai buit i tria `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Anomena l'asset `Def_sala1_puzle` o posa-li un nom que reconeguis.
4. Selecciona la definició a `Project`. A `Inspector`, escriu un `Persistent Id` únic, com `sala1_teclat` si és el teclat de la sala 1. Omple `Display Name` i `Objective`. Selecciona l'arrel del puzle a `Hierarchy` i arrossega l'asset del `Project` a `Definition`, al component del puzle de l'`Inspector`.
5. Si vols pistes, a la mateixa carpeta de `Project` fes clic dret i tria `Create > Escape Room Framework > Hints > Hint Data`. Selecciona el `Hint Data` i afegeix els textos a la seva llista `Hints`. Després selecciona la `Puzzle Definition` i arrossega el `Hint Data` del `Project` al camp `Hints` de la definició.
6. Per obrir una porta en resoldre el puzle, crea-la des del menú superior `Escape Room Framework > Create > Interactables > Door`. A `Hierarchy`, selecciona `NewDoor_Logic` i, al component `Door` de l'`Inspector`, marca `Is Locked`. Col·loca la porta on vols que estigui a l'escena.
7. A `Hierarchy`, selecciona l'arrel del puzle. Mantén `Ctrl` a Windows o `Cmd` a Mac i clica `NewDoor_Logic` perquè quedin seleccionats tots dos. Executa `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Torna a seleccionar només l'arrel i comprova que `On Solved` conté `Door.Unlock` i `Door.ForceOpen`. Desa l'escena amb `Ctrl+S` o `Cmd+S` i entra a `Play`.

**Com ho proves**

- Només es mou una fitxa veïna del forat; les altres fan un petit rebot.
- Prova-ho a Play: guarda amb F5 a mig puzle i un cop resolt, surt i carrega amb F9. L'estat ha de ser el mateix i la porta ha de continuar oberta.

**Compte amb**


<a id="lights"></a>
### Circuit de llums

Menú: `Escape Room Framework > Create > Puzzles > Linked Lights Puzzle` · Sala del museu 14

**Passos**

1. Executa `Escape Room Framework > Create > Puzzles > Linked Lights Puzzle`. A `Hierarchy`, selecciona `LinkedLights_Kit`. Obre el seu triangle per trobar els cinc botons i `ResetButton`. Selecciona l'arrel i obre `LinkedLightsPuzzle > Nodes` a l'`Inspector`. Els botons es numeren des d'1 i els índexs des de 0.
2. Per afegir una llum, duplica un botó a `Hierarchy`, dona-li el nou `Index` a `LinkedLightButton` i afegeix una fila a `Nodes`. Selecciona l'arrel i arrossega el botó duplicat al camp `Indicator` de la nova fila. Revisa també `Connections` a les files que vulguis connectar-hi.
3. Per passar de cinc a quatre, treu `Element 4` de `Nodes` i elimina `LightButton_5` i `Number_5` a `Hierarchy`. A `Element 3 > Connections`, deixa `2, 3`. Si treus una llum del mig, refés els `Index` i totes les `Connections` que canviïn de número. A la configuració de quatre llums d'aquest exemple, desmarca `Initially On` a totes quatre, marca `Target On` a totes i prova els botons 1 i 4.
4. Si encara tens la 0.2.1 i no trobes el menú, obre `ShowcaseMuseum` des de `Project > Assets > _EscapeRoomTemplate > Scenes`. A `Hierarchy`, obre `Room14_LinkedLights` i arrossega `LinkedLights_Kit` a una carpeta teva de `Project` per fer-ne un prefab. Reobre la teva escena i arrossega-hi el prefab. Treu les referències de la demo que quedin a `On Solved` i al `ResetButton`. Mantén `LinkedLightsPuzzle.ResetPuzzle` al botó. Assigna una definició pròpia al circuit.
5. A `Project`, obre la carpeta d'`Assets` on guardes el teu puzle. Fes clic dret en un espai buit i tria `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Anomena l'asset `Def_sala1_puzle` o posa-li un nom que reconeguis.
6. Selecciona la definició a `Project`. A `Inspector`, escriu un `Persistent Id` únic, com `sala1_teclat` si és el teclat de la sala 1. Omple `Display Name` i `Objective`. Selecciona l'arrel del puzle a `Hierarchy` i arrossega l'asset del `Project` a `Definition`, al component del puzle de l'`Inspector`.
7. Si vols pistes, a la mateixa carpeta de `Project` fes clic dret i tria `Create > Escape Room Framework > Hints > Hint Data`. Selecciona el `Hint Data` i afegeix els textos a la seva llista `Hints`. Després selecciona la `Puzzle Definition` i arrossega el `Hint Data` del `Project` al camp `Hints` de la definició.
8. Per obrir una porta en resoldre el puzle, crea-la des del menú superior `Escape Room Framework > Create > Interactables > Door`. A `Hierarchy`, selecciona `NewDoor_Logic` i, al component `Door` de l'`Inspector`, marca `Is Locked`. Col·loca la porta on vols que estigui a l'escena.
9. A `Hierarchy`, selecciona l'arrel del puzle. Mantén `Ctrl` a Windows o `Cmd` a Mac i clica `NewDoor_Logic` perquè quedin seleccionats tots dos. Executa `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Torna a seleccionar només l'arrel i comprova que `On Solved` conté `Door.Unlock` i `Door.ForceOpen`. Desa l'escena amb `Ctrl+S` o `Cmd+S` i entra a `Play`.

**Com ho proves**

- Amb la configuració de mostra es resol prement 1, 3 i 5. El botó de reinici torna a l'estat inicial.
- Prova-ho a Play: guarda amb F5 a mig puzle i un cop resolt, surt i carrega amb F9. L'estat ha de ser el mateix i la porta ha de continuar oberta.

**Compte amb**

- Assegura't que té solució: parteix de l'objectiu i aplica unes quantes pulsacions per obtenir l'estat inicial.

<a id="throw"></a>
### Llançament a dianes

Menú: `Escape Room Framework > Create > Puzzles > Throw Puzzle` · Sala del museu 6

**Passos**

1. Executa `Escape Room Framework > Create > Puzzles > Throw Puzzle`. A `Hierarchy`, obre el triangle de `NewThrowPuzzle`. Hi trobaràs `Target1_Logic`, `Target2_Logic` i `Target3_Logic`. Cada diana conté un `ThrowTarget` i un collider sòlid.
2. Per quedar-te una sola diana, selecciona `Target2_Logic` i `Target3_Logic` a `Hierarchy` i elimina'ls amb `Delete`. Deixa `Target1_Logic` i el seu fill visual. Torna a seleccionar `NewThrowPuzzle`. A `Inspector > ThrowPuzzle > Target Ids`, posa la mida de la llista a `1` o elimina les dues files sobrants amb `–`. Ha de quedar només `Element 0 = target_1`.
3. Selecciona `Target1_Logic` i comprova a l'`Inspector` que `ThrowTarget > Target Id` diu `target_1`. Si el camp `Puzzle` és buit, arrossega-hi `NewThrowPuzzle` des de `Hierarchy`. Si un altre dia vols una diana més, duplica l'objecte `_Logic`, canvia `Target Id` i afegeix el mateix ID a `Target Ids` de l'arrel.
4. Crea un objecte per llançar amb `Escape Room Framework > Create > Interactables > Physics Grabbable`. Selecciona `NewGrabbable_Logic` a `Hierarchy`. A l'`Inspector`, comprova `Can Be Thrown` a `PhysicsGrabbable` i que el collider estigui activat, amb `Is Trigger` desmarcat, i que el `Rigidbody` tingui `Is Kinematic` desmarcat. Col·loca l'objecte a l'abast. En `Play` a PC, mira'l i prem `E`, apunta a la diana i llança amb clic esquerre. Amb una sola diana, un impacte que superi `Min Impact Speed` ha de resoldre el puzle.
5. A `Project`, obre la carpeta d'`Assets` on guardes el teu puzle. Fes clic dret en un espai buit i tria `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Anomena l'asset `Def_sala1_puzle` o posa-li un nom que reconeguis.
6. Selecciona la definició a `Project`. A `Inspector`, escriu un `Persistent Id` únic, com `sala1_teclat` si és el teclat de la sala 1. Omple `Display Name` i `Objective`. Selecciona l'arrel del puzle a `Hierarchy` i arrossega l'asset del `Project` a `Definition`, al component del puzle de l'`Inspector`.
7. Si vols pistes, a la mateixa carpeta de `Project` fes clic dret i tria `Create > Escape Room Framework > Hints > Hint Data`. Selecciona el `Hint Data` i afegeix els textos a la seva llista `Hints`. Després selecciona la `Puzzle Definition` i arrossega el `Hint Data` del `Project` al camp `Hints` de la definició.
8. Per obrir una porta en resoldre el puzle, crea-la des del menú superior `Escape Room Framework > Create > Interactables > Door`. A `Hierarchy`, selecciona `NewDoor_Logic` i, al component `Door` de l'`Inspector`, marca `Is Locked`. Col·loca la porta on vols que estigui a l'escena.
9. A `Hierarchy`, selecciona l'arrel del puzle. Mantén `Ctrl` a Windows o `Cmd` a Mac i clica `NewDoor_Logic` perquè quedin seleccionats tots dos. Executa `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Torna a seleccionar només l'arrel i comprova que `On Solved` conté `Door.Unlock` i `Door.ForceOpen`. Desa l'escena amb `Ctrl+S` o `Cmd+S` i entra a `Play`.

**Com ho proves**

- A PC: E per agafar, clic esquerre per llançar i Q per deixar. A VR: agafa amb el grip i deixa anar fent el gest.
- Prova-ho a Play: guarda amb F5 a mig puzle i un cop resolt, surt i carrega amb F9. L'estat ha de ser el mateix i la porta ha de continuar oberta.

**Opcional · Clavar un o diversos ganivets 3D a la diana (sense tocar la plantilla)**

1. **Prepara el ganivet (`PhysicsGrabbable` + `GrabbablePiece`).** Al teu objecte llançable (`Ganivet_Logic`), afegeix el component `GrabbablePiece` amb `Add Component`. A més de permetre clavar-lo, `GrabbablePiece` té `Return When Lost` activat per defecte: si el tir falla i el ganivet cau lluny o sota el mapa, torna sol a la posició inicial.
2. **Marca el punt i angle finals (`PuntClavat`).** Fes clic dret sobre `Target1_Logic` a `Hierarchy` → `Create Empty`, anomena'l `PuntClavat` i situa'l i gira'l amb la punta mirant cap a la diana.
3. **Connecta l'esdeveniment (`GrabbablePiece.LockInPlace`).** Per a 1 diana i 1 ganivet, a `ThrowPuzzle > On Solved` prem `+`, arrossega `Ganivet_Logic`, selecciona `GrabbablePiece > LockInPlace (Transform)` i arrossega `PuntClavat` al camp `Transform`. `LockInPlace` congela el `Rigidbody` (`isKinematic = true`), el fa fill de la diana i desactiva l'outline i l'agafada (PC i VR).
4. **Múltiples dianes i ganivets sense codi (`MultiStagePuzzle`).** Si tens 3 dianes i 3 ganivets i vols que cadascun es clavi en encertar la seva diana sense codi, agrupa 3 `ThrowPuzzle` d'1 diana dins d'un `MultiStagePuzzle` (amb `Require Order` desmarcat) i connecta l'`On Solved` de cada fill al `LockInPlace` del seu ganivet.
5. **Múltiples dianes amb qualsevol ganivet a l'instant (micro-script a la teva carpeta).** Si vols que qualsevol ganivet es clavi a l'instant a qualsevol diana en xocar-hi, afegeix a cada diana un script propi (a `Assets/ElMeuJoc/StickKnifeOnTarget.cs`) que a `OnCollisionEnter` comprovi `collision.relativeVelocity.magnitude >= _minImpactSpeed` i cridi `piece.LockInPlace(_snapPoint)`.

**Compte amb**


<a id="place"></a>
### Col·locació de peces

Menú: `Escape Room Framework > Create > Puzzles > Placement Puzzle` · Sala del museu 7

**Passos**

1. Executa `Escape Room Framework > Create > Puzzles > Placement Puzzle`. Obre `NewPlacementPuzzle` a `Hierarchy`. Els objectes `PieceA_Logic` i `PieceB_Logic` són les peces, i `SocketA_Logic`, `SocketB_Logic` i `SocketC_Logic` són els encaixos. Selecciona l'arrel i obre `PlacementPuzzle > Rules` a l'`Inspector`.
2. Per afegir una parella, duplica una peça i un encaix sencers a `Hierarchy`. Al component `GrabbablePiece` de la peça nova, escriu `Piece Id = piece_c`. Al `PieceSocketReceiver` de l'encaix nou, posa `Socket Id = socket_d`. Si falta `Puzzle`, arrossega-hi `NewPlacementPuzzle`. A l'arrel, prem `+` a `Rules` i escriu `piece_c` a `Piece Id` i `socket_d` a `Correct Socket Id`.
3. Per quedar-te la peça A, elimina de `Rules` la fila `piece_b → socket_b`. A `Hierarchy`, elimina `PieceB_Logic` i `SocketB_Logic`. Si vols treure també l'esquer, elimina `SocketC_Logic`. Com que aquest últim no té cap regla, no cal canviar la llista. Deixa `piece_a → socket_a` i col·loca tots dos objectes a l'abast.
4. A `Project`, obre la carpeta d'`Assets` on guardes el teu puzle. Fes clic dret en un espai buit i tria `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Anomena l'asset `Def_sala1_puzle` o posa-li un nom que reconeguis.
5. Selecciona la definició a `Project`. A `Inspector`, escriu un `Persistent Id` únic, com `sala1_teclat` si és el teclat de la sala 1. Omple `Display Name` i `Objective`. Selecciona l'arrel del puzle a `Hierarchy` i arrossega l'asset del `Project` a `Definition`, al component del puzle de l'`Inspector`.
6. Si vols pistes, a la mateixa carpeta de `Project` fes clic dret i tria `Create > Escape Room Framework > Hints > Hint Data`. Selecciona el `Hint Data` i afegeix els textos a la seva llista `Hints`. Després selecciona la `Puzzle Definition` i arrossega el `Hint Data` del `Project` al camp `Hints` de la definició.
7. Per obrir una porta en resoldre el puzle, crea-la des del menú superior `Escape Room Framework > Create > Interactables > Door`. A `Hierarchy`, selecciona `NewDoor_Logic` i, al component `Door` de l'`Inspector`, marca `Is Locked`. Col·loca la porta on vols que estigui a l'escena.
8. A `Hierarchy`, selecciona l'arrel del puzle. Mantén `Ctrl` a Windows o `Cmd` a Mac i clica `NewDoor_Logic` perquè quedin seleccionats tots dos. Executa `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Torna a seleccionar només l'arrel i comprova que `On Solved` conté `Door.Unlock` i `Door.ForceOpen`. Desa l'escena amb `Ctrl+S` o `Cmd+S` i entra a `Play`.

**Com ho proves**

- Prova peces intercanviades, treure-les abans de resoldre i carregar un cop resolt: les peces queden fixades.
- Si guardes a mig puzle, en carregar les peces soltes tornen al seu lloc inicial.

**Compte amb**

- Randomize Mapping canvia quin suport és el correcte a cada partida: la pista ha de ser dinàmica.

<a id="multi"></a>
### Puzles encadenats

Menú: `Escape Room Framework > Create > Puzzles > Multi-Stage Puzzle` · Sala del museu 11

**Passos**

1. Executa `Escape Room Framework > Create > Puzzles > Multi-Stage Puzzle`. A `Hierarchy`, obre `MultiStageChainPuzzle` i després `MultiStage_Generated`. Hi ha un puzle de seqüència i un de palanques. Per editar-los, selecciona el fill de la fase corresponent. Per canviar l'ordre, selecciona l'arrel i obre `MultiStagePuzzle > Puzzles` a l'`Inspector`.
2. Per afegir una fase, crea un puzle amb el seu menú i situa'l a l'escena. A l'arrel del grup, prem `+` a `Puzzles` i posa un `Id` diferent de les altres fases. Arrossega l'arrel del puzle nou de `Hierarchy` a `Puzzle`. Assigna a `Interaction Root` l'objecte que conté els controls, normalment la mateixa arrel.
3. Per treure una fase, elimina la seva fila de `Puzzles` i retira el seu objecte de `Hierarchy` si no el vols a la sala. Conserva almenys un puzle al grup. Amb `Require Order` activat, les fases se segueixen en l'ordre de la llista. `Lock Future Puzzles` evita que els controls de fases posteriors responguin abans del seu torn. Connecta la porta a l'arrel del grup.
4. A `Project`, obre la carpeta d'`Assets` on guardes el teu puzle. Fes clic dret en un espai buit i tria `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Anomena l'asset `Def_sala1_puzle` o posa-li un nom que reconeguis.
5. Selecciona la definició a `Project`. A `Inspector`, escriu un `Persistent Id` únic, com `sala1_teclat` si és el teclat de la sala 1. Omple `Display Name` i `Objective`. Selecciona l'arrel del puzle a `Hierarchy` i arrossega l'asset del `Project` a `Definition`, al component del puzle de l'`Inspector`.
6. Si vols pistes, a la mateixa carpeta de `Project` fes clic dret i tria `Create > Escape Room Framework > Hints > Hint Data`. Selecciona el `Hint Data` i afegeix els textos a la seva llista `Hints`. Després selecciona la `Puzzle Definition` i arrossega el `Hint Data` del `Project` al camp `Hints` de la definició.
7. Per obrir una porta en resoldre el puzle, crea-la des del menú superior `Escape Room Framework > Create > Interactables > Door`. A `Hierarchy`, selecciona `NewDoor_Logic` i, al component `Door` de l'`Inspector`, marca `Is Locked`. Col·loca la porta on vols que estigui a l'escena.
8. A `Hierarchy`, selecciona l'arrel del puzle. Mantén `Ctrl` a Windows o `Cmd` a Mac i clica `NewDoor_Logic` perquè quedin seleccionats tots dos. Executa `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Torna a seleccionar només l'arrel i comprova que `On Solved` conté `Door.Unlock` i `Door.ForceOpen`. Desa l'escena amb `Ctrl+S` o `Cmd+S` i entra a `Play`.

**Com ho proves**

- La porta s'obre només quan tots els fills estan resolts.
- Prova-ho a Play: guarda amb F5 a mig puzle i un cop resolt, surt i carrega amb F9. L'estat ha de ser el mateix i la porta ha de continuar oberta.

**Compte amb**


## Sistemes de la sala

<a id="key"></a>
### Clau que obre una porta

Menú: `Create > Escape Room Framework > Inventory > Item` + `Escape Room Framework > Create > Interactables > Pickable Item · Door` · Sala del museu 1 · 3

**Passos**

1. **Crea la fitxa de la clau (`InventoryItemData`).** Al panell `Project`, entra a la teva carpeta sota `Assets`. Fes clic dret en un espai buit i tria `Create > Escape Room Framework > Inventory > Item`. Anomena l'asset `Item_ClauDespatx`. Selecciona'l i, a l'`Inspector`, escriu a `Item Id` un identificador únic en minúscules i sense espais, com `clau_despatx`. Omple `Display Name` (per exemple `Clau del despatx`) i `Description`. Si no tens cap icona pròpia per a `Icon`, pots generar-la automàticament més tard amb `Escape Room Framework > Maintenance > Generate Missing Item Icons`.
2. **Registra la clau al catàleg (`DefaultItemCatalog`).** Al panell `Project`, selecciona `Assets/_EscapeRoomTemplate/ScriptableObjects/DefaultItemCatalog.asset` (el catàleg que fa servir `InventoryManager` dins del `GameManager`). A l'`Inspector`, desplega `Items`, prem `+` i arrossega-hi el teu asset `Item_ClauDespatx` des de `Project`. Això permet que el guardat i la càrrega reconeguin la clau.
3. **Col·loca la clau a l'escena (`Pickable Item`).** Al menú superior executa `Escape Room Framework > Create > Interactables > Pickable Item`. A `Hierarchy`, selecciona `NewPickableItem_Logic` (pots anomenar-lo `ClauDespatx_Logic`) i mou-lo on el jugador l'hagi de trobar (sobre una taula, a terra o dins d'un calaix). Amb `ClauDespatx_Logic` seleccionat a `Hierarchy`, arrossega l'asset `Item_ClauDespatx` des de `Project` fins al camp `Item Data` del component `Pickable Item` a l'`Inspector`. Deixa `Quantity` a `1`.
4. **Crea la porta i tanca-la amb la clau (`Door`).** Al menú superior executa `Escape Room Framework > Create > Interactables > Door` (o selecciona una porta, armari o calaix existent). A `Hierarchy`, selecciona `NewDoor_Logic` i mira el component `Door` a l'`Inspector`. Marca `Is Locked` i, al camp `Required Item Id`, escriu exactament el mateix text que has posat a `Item Id` de la clau: `clau_despatx`.
5. **Tria com s'utilitza la clau (`Item Use Policy`).** Al mateix component `Door`, deixa `Item Use Policy` a `Offer Compatible` (per defecte: en prémer `E` davant la porta tancada, s'obre un selector al HUD amb la clau per confirmar-ne l'ús, i el següent `E` obre la porta) o canvia'l a `Auto Use Single` si vols que en prémer `E` es desbloquegi i s'obri directament sense menú quan el jugador porta la clau. Marca `Consume Required Item` si la clau s'ha de gastar, o desmarca-ho si serveix per a més d'una porta.
6. **Alternativa amb pany separat (`Item Receiver`).** Si prefereixes un pany o pedestal separat on es vegi aparèixer el model 3D de la clau, executa `Escape Room Framework > Create > Interactables > Item Receiver`. Arrossega `Item_ClauDespatx` al camp `Required Item` (aquí s'arrossega l'asset, no el text) i, a `On Item Accepted`, prem `+`, arrossega la porta des de `Hierarchy` i selecciona `Door.Unlock` (i una segona fila amb `Door.ForceOpen` si vols que s'obri sola).

**Com ho proves**

- Entra a `Play`, apropa't a la porta abans d'agafar la clau i prem `E`: ha de continuar tancada i mostrar el missatge de bloqueig.
- Mira la clau i prem `E` per recollir-la (pots comprovar que és a l'inventari amb `I`). Torna a la porta i prem `E`: amb `Offer Compatible`, confirma la clau al selector i prem `E` per obrir; amb `Auto Use Single`, s'obre directament.
- Guarda amb `F5` després d'obrir la porta i carrega amb `F9`: la clau recollida no torna a aparèixer a l'escena i la porta continua desbloquejada i oberta.

**Compte amb**

- `Required Item Id` a `Door` és un text i ha de coincidir **exactament** (majúscules, minúscules i guions) amb `Item Id` de l'`InventoryItemData`, no amb el nom del fitxer ni amb `Display Name`.
- Si no afegeixes l'asset al `DefaultItemCatalog`, la clau desapareixerà de l'inventari en carregar una partida guardada.

<a id="items"></a>
### Objectes, inventari, combinació i examen

Menú: `Create > Escape Room Framework > Inventory > Item` · Sala del museu 3

**Passos**

1. Crea l'objecte: al Project, Create > Escape Room Framework > Inventory > Item. Omple Item Id (únic, en minúscules, per exemple clau_despatx), Display Name, Description i Icon.
2. Assigna World Prefab (el model 3D) i activa Can Examine si s'ha de poder examinar. Per a una nota, activa Is Readable i escriu Note Content.
3. Afegeix-lo a l'ItemCatalog que fa servir InventoryManager (al GameManager; per defecte ScriptableObjects/DefaultItemCatalog).
4. Posa'l al món: Escape Room Framework > Create > Interactables > Pickable Item i assigna'l a Item Data.
5. Combinació: a l'objecte A, afegeix una entrada a Combinations amb Combine With = B i Result Item = C (afegeix C també al catàleg). Destroy This / Destroy Other decideixen què desapareix.
6. Punts d'examen: obre el World Prefab, selecciona'l i executa Escape Room Framework > Create > Inventory > Examine Hotspot. Posa Hotspot Id, Unrevealed Prompt, Revealed Description i, si cal, Revealed Item.
7. Per fer servir l'objecte en un pany: a la porta, Is Locked + Required Item Id = l'Item Id. O bé Escape Room Framework > Create > Interactables > Item Receiver amb Required Item i On Item Accepted.

**Com ho proves**

- I obre l'inventari. Prova combinar amb l'inventari ple i amb quantitats de més d'una unitat.
- Guarda abans i després de fer servir una clau: l'objecte recollit no torna a aparèixer.

**Compte amb**

- No reutilitzis un Item Id per a objectes diferents. Per a diverses unitats del mateix objecte, fes servir el mateix asset.

<a id="doors"></a>
### Portes, calaixos i armaris

Menú: `Escape Room Framework > Create > Interactables > Door · Drawer · Cabinet` · Sala del museu 1

**Passos**

1. Executa Escape Room Framework > Create > Interactables > Door (o Drawer o Cabinet). Tots fan servir el component Door.
2. Movement Type: Pivot per a portes i armaris (gira sobre Custom Pivot amb Open Angle), Slide per a calaixos (Slide Offset).
3. Per tancar-la: Is Locked. Amb clau: Required Item Id i Consume Required Item. Item Use Policy decideix com s'ofereix la clau (Offer Compatible, Selected Only o Auto Use Single).
4. Des d'un esdeveniment pots cridar Unlock, Lock, ForceOpen o ForceClose.
5. Canvia el model dins del fill visual; no toquis el collider ni els components de l'arrel.

**Com ho proves**

- La porta guarda si està tancada amb clau i quant d'oberta està.

**Compte amb**

- Si dupliques una porta, canvia'n el Save Id.

<a id="hints"></a>
### Pistes i narració

Menú: `Create > Escape Room Framework > Hints > Hint Data`

**Passos**

1. Crea un Hint Data: Delay Before First Hint (segons fins a la primera pista), Delay Between Hints i la llista Hints, de la més vaga a la solució. Cada pista pot portar àudio.
2. Assigna'l al camp Hints de la PuzzleDefinition: s'activa quan el jugador comença el puzle i s'apaga quan el resol.
3. Per a pistes per zones: Escape Room Framework > Create > Triggers > Hint Zone, assigna Puzzle Hint Data i, si vols, Clear On Exit.
4. Per a narració en entrar a un lloc: Escape Room Framework > Create > Triggers > Narrative Trigger amb Play Mode Once, Always o ProgressiveHints.

**Com ho proves**

- El jugador pot demanar la pista següent amb H. Les pistes surten com a subtítols i respecten l'opció Subtítols.

**Compte amb**

- Escriu les pistes en castellà (la clau del catàleg) i tradueix-les al DefaultLocalizationCatalog si el joc és multilingüe.

<a id="objectives"></a>
### Objectius i final de partida

Menú: `Escape Room Framework > Create > Flow > Objective Manager · Game End Trigger`

**Passos**

1. Crea un Ending Definition (Create > Escape Room Framework > Ending Definition): Ending Id, Outcome (Victory o Defeat), Title i Message.
2. Crea un Objective Definition per objectiu: Objective Id, Title, Trigger (PuzzleSolved, ItemCollected, NoteRead, InteractionPerformed o Manual), Target Id (el Persistent Id del puzle, l'Item Id…) i Prerequisites.
3. Crea un Objective Set amb la llista d'objectius i el Completion Ending. Opcionalment, Next Room Scene i Next Room Spawn Id per passar a una altra sala.
4. Executa Escape Room Framework > Create > Flow > Objective Manager i assigna-hi l'Objective Set.
5. Alternativa senzilla: Escape Room Framework > Create > Flow > Game End Trigger amb un Ending. Activa Activate On Player Enter o crida Trigger des de l'On Solved de l'últim puzle.

**Com ho proves**

- La partida no acaba abans de l'últim objectiu. A la pantalla final, Reintentar torna a començar la sala.

**Compte amb**

- Evita prerequisits circulars.

<a id="hazard"></a>
### Temporitzador i perill mòbil

Menú: `Escape Room Framework > Create > Flow > Game Over Timer (HUD) · Moving Hazard (Any Direction)` · Sala del museu 12

**Passos**

1. Temporitzador: Escape Room Framework > Create > Flow > Game Over Timer (HUD). A la finestra tria durada, inici automàtic, si es veu al HUD i el text.
2. Perill: Escape Room Framework > Create > Flow > Moving Hazard (Any Direction). Tria direcció, distància, durada i mida. Després pots moure lliurement StartPoint i EndPoint (sostre, paret, terra, plataforma o aigua).
3. Per iniciar-los: un Generic Trigger amb On Interact Event → GameOverTimer.StartTimer o MovingHazard.StartHazard, o una zona Escape Room Framework > Create > Triggers > Event Trigger Zone amb On Entered.
4. Per aturar-los en resoldre: a l'On Solved del puzle, crida StopTimer o StopHazard.
5. Assigna un Ending de derrota si vols un missatge propi.

**Com ho proves**

- En pausa el temps es congela. En acabar-se, apareix la pantalla de resultats amb Reintentar.

**Compte amb**


<a id="rooms"></a>
### Canvi de sala o d'escena

Menú: `RoomPortal + RoomSpawnPoint`

**Passos**

1. A l'escena de destinació, crea un objecte buit i afegeix-hi RoomSpawnPoint amb un Spawn Id (per exemple entrada).
2. A l'escena d'origen, crea un objecte amb collider i afegeix-hi RoomPortal: Target Scene, Target Spawn Id i Load Mode (Single o Additive). Pot estar tancat (Is Locked) o demanar un objecte (Required Item Id).
3. Afegeix totes dues escenes a File > Build Profiles.
4. Un puzle pot obrir el portal amb Link Selected Objects To Puzzle (crida RoomPortal.Unlock).

**Com ho proves**

- En mode Single, l'inventari i l'estat viatgen amb el jugador. En mode Additive les escenes conviuen: no dupliquis GameManager ni jugador.

**Compte amb**

