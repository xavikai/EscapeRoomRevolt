# Receptes pas a pas

**Idioma:** [català](RECEPTES.md) · [castellano](RECETAS.md) · [English](RECIPES.md)

Cada recepta parteix d'una escena buida amb el jugador ja posat (`Setup > Instantiate Game Manager` i el jugador PC o VR). Per provar-les de manera interactiva, obre la guia [`docs/guia/index.html`](../../../docs/guia/index.html) > *Receptes pas a pas*.

- Puzles: [Teclat de codi](#code) · [Seqüència i melodia](#seq) · [Palanques i interruptors (StatePuzzle)](#state) · [Rodets numèrics (cadenat)](#wheels) · [Canonades](#pipe) · [Trencaclosques lliscant](#slide) · [Circuit de llums](#lights) · [Llançament a dianes](#throw) · [Col·locació de peces](#place) · [Puzles encadenats](#multi)
- Sistemes de la sala: [Objectes, inventari, combinació i examen](#items) · [Portes, calaixos i armaris](#doors) · [Pistes i narració](#hints) · [Objectius i final de partida](#objectives) · [Temporitzador i perill mòbil](#hazard) · [Canvi de sala o d'escena](#rooms)

## Puzles

<a id="code"></a>
### Teclat de codi

Menú: `Escape Room Framework > Create > Puzzles > Keypad Panel` · Sala del museu 2

**Passos**

1. Executa Escape Room Framework > Create > Puzzles > Keypad Panel. Es crea NewKeypad amb CodePanelPuzzle, InteractableKeypad, una pantalla, un LED i onze botons 3D (1–9, C, 0) ja connectats al puzle.
2. Col·loca'l a la paret movent només l'arrel NewKeypad. No escalis els botons per separat.
3. A CodePanelPuzzle escriu Correct Code (per exemple 3142), posa Max Code Length amb el mateix nombre de xifres i deixa Auto Check When Full activat.
4. Crea les dades del puzle: al panell Project, clic dret > Create > Escape Room Framework > Puzzles > Puzzle Definition. Posa-hi un Persistent Id únic (per exemple sala1_teclat), un Display Name i l'Objective. Després crea les pistes amb Create > Escape Room Framework > Hints > Hint Data (tres pistes: context, indicació i solució) i arrossega-les al camp Hints de la definició.
5. Selecciona l'arrel del puzle i arrossega la definició al camp Definition del component.
6. Crea la recompensa: Escape Room Framework > Create > Interactables > Door i marca Is Locked a la porta. Selecciona el puzle, fes Ctrl+clic a la porta i executa Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. A On Solved hi apareixeran Door.Unlock i Door.ForceOpen.
7. Amaga la pista del codi: Escape Room Framework > Create > Interactables > Note i escriu-la a Note Content.

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

1. Executa Escape Room Framework > Create > Puzzles > Sequence Puzzle. Crea tres botons (vermell, verd, blau) amb un SequenceStepButton cadascun, i l'ordre correcte vermell → verd → blau.
2. La llista Correct Sequence de SequencePuzzle conté els identificadors en ordre. El Step Id de cada SequenceStepButton ha de coincidir exactament amb un d'aquests identificadors.
3. Per afegir un pas, duplica un botó, canvia'n el Step Id i afegeix aquest ID a Correct Sequence. Un ID pot sortir més d'una vegada a la llista.
4. Melodia (opcional): posa un AudioSource amb la seva nota a cada botó, afegeix un MelodyPlayer al puzle i omple Notes en l'ordre correcte. Crea un Generic Trigger «Escoltar la melodia» i, al seu On Interact Event, crida MelodyPlayer.Play.
5. Crea les dades del puzle: al panell Project, clic dret > Create > Escape Room Framework > Puzzles > Puzzle Definition. Posa-hi un Persistent Id únic (per exemple sala1_teclat), un Display Name i l'Objective. Després crea les pistes amb Create > Escape Room Framework > Hints > Hint Data (tres pistes: context, indicació i solució) i arrossega-les al camp Hints de la definició.
6. Selecciona l'arrel del puzle i arrossega la definició al camp Definition del component.
7. Crea la recompensa: Escape Room Framework > Create > Interactables > Door i marca Is Locked a la porta. Selecciona el puzle, fes Ctrl+clic a la porta i executa Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. A On Solved hi apareixeran Door.Unlock i Door.ForceOpen.

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

1. Executa Escape Room Framework > Create > Puzzles > State Puzzle. Crea tres palanques de tres posicions (SteppedPositioner + InteractableCycler) que resolen quan marquen 0, 1 i 2.
2. A cada palanca, la llista Positions defineix cada posició: el text que veu el jugador (Prompt) i la rotació o el desplaçament. Starting Index és la posició inicial.
3. A StatePuzzle, la llista Conditions uneix cada palanca (Positioner) amb la posició que ha de tenir (Required Index, comença a 0).
4. Per a un interruptor de dues posicions fes servir Escape Room Framework > Create > Interactables > Multi-Position Lever amb dues posicions. L'InteractableToggle (Lever/Switch) no alimenta aquest puzle.
5. Crea les dades del puzle: al panell Project, clic dret > Create > Escape Room Framework > Puzzles > Puzzle Definition. Posa-hi un Persistent Id únic (per exemple sala1_teclat), un Display Name i l'Objective. Després crea les pistes amb Create > Escape Room Framework > Hints > Hint Data (tres pistes: context, indicació i solució) i arrossega-les al camp Hints de la definició.
6. Selecciona l'arrel del puzle i arrossega la definició al camp Definition del component.
7. Crea la recompensa: Escape Room Framework > Create > Interactables > Door i marca Is Locked a la porta. Selecciona el puzle, fes Ctrl+clic a la porta i executa Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. A On Solved hi apareixeran Door.Unlock i Door.ForceOpen.

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

1. Executa Escape Room Framework > Create > Puzzles > Number Wheels Puzzle. A la finestra tria el nombre de rodets (de 2 a 8) i la xifra de cada un, i prem Create puzzle.
2. Es crea un StatePuzzle amb un PuzzleFocusPoint (càmera d'aproximació), el component NumberWheelsPuzzleAuthoring i els botons ▲/▼ de cada rodet.
3. Per canviar la combinació o el nombre de rodets, edita NumberWheelsPuzzleAuthoring i prem Rebuild wheels and layout. Es conserven la definició i On Solved.
4. Per canviar l'aspecte (maleta, caixa forta), substitueix els models dins dels ReplaceableModelSlot de la carcassa i dels rodets.
5. Crea les dades del puzle: al panell Project, clic dret > Create > Escape Room Framework > Puzzles > Puzzle Definition. Posa-hi un Persistent Id únic (per exemple sala1_teclat), un Display Name i l'Objective. Després crea les pistes amb Create > Escape Room Framework > Hints > Hint Data (tres pistes: context, indicació i solució) i arrossega-les al camp Hints de la definició.
6. Selecciona l'arrel del puzle i arrossega la definició al camp Definition del component.
7. Crea la recompensa: Escape Room Framework > Create > Interactables > Door i marca Is Locked a la porta. Selecciona el puzle, fes Ctrl+clic a la porta i executa Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. A On Solved hi apareixeran Door.Unlock i Door.ForceOpen.

**Com ho proves**

- A PC: E per entrar a la vista d'aproximació, clic a ▲/▼ i clic dret per sortir. A VR: botons físics ▲/▼ amb el gatell.
- Prova-ho a Play: guarda amb F5 a mig puzle i un cop resolt, surt i carrega amb F9. L'estat ha de ser el mateix i la porta ha de continuar oberta.

**Compte amb**

- Després de canviar el nombre de rodets, comprova que la combinació continua sent la que vols.

<a id="pipe"></a>
### Canonades

Menú: `Escape Room Framework > Create > Puzzles > Pipe Puzzle` · Sala del museu 10

**Passos**

1. Executa Escape Room Framework > Create > Puzzles > Pipe Puzzle. Es crea NewPipePuzzle amb dos trams d'exemple (pipe_a i pipe_b) girats 180°. Només és la lògica: encara no hi ha res visible.
2. A la llista Tiles, cada tram té Tile Id, Row, Column, Open Sides (costats oberts sense girar: North, East, South, West) i Starting Rotation Steps (0–3, en passos de 90°). Posa Source Tile Id i Sink Tile Id.
3. Per a cada tram, crea un objecte clicable (Escape Room Framework > Create > Interactables > Generic Trigger) i afegeix-hi un PipeTileButton amb el puzle i el seu Tile Id. A On Interact Event del trigger crida PipeTileButton.Rotate.
4. Perquè el model giri, afegeix al tram visual un SteppedPositioner amb quatre posicions (0°, 90°, 180°, 270°) i assigna'l a Visual Positioner del PipeTileButton.
5. Crea les dades del puzle: al panell Project, clic dret > Create > Escape Room Framework > Puzzles > Puzzle Definition. Posa-hi un Persistent Id únic (per exemple sala1_teclat), un Display Name i l'Objective. Després crea les pistes amb Create > Escape Room Framework > Hints > Hint Data (tres pistes: context, indicació i solució) i arrossega-les al camp Hints de la definició.
6. Selecciona l'arrel del puzle i arrossega la definició al camp Definition del component.
7. Crea la recompensa: Escape Room Framework > Create > Interactables > Door i marca Is Locked a la porta. Selecciona el puzle, fes Ctrl+clic a la porta i executa Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. A On Solved hi apareixeran Door.Unlock i Door.ForceOpen.

**Com ho proves**

- Es resol quan hi ha un camí continu d'obertures de l'origen al destí; un cop resolt, els trams deixen de girar.
- Prova-ho a Play: guarda amb F5 a mig puzle i un cop resolt, surt i carrega amb F9. L'estat ha de ser el mateix i la porta ha de continuar oberta.

**Compte amb**

- Randomize Rotations barreja els girs inicials i mai no comença resolt.

<a id="slide"></a>
### Trencaclosques lliscant

Menú: `Escape Room Framework > Create > Puzzles > Sliding Puzzle` · Sala del museu 8

**Passos**

1. Executa Escape Room Framework > Create > Puzzles > Sliding Puzzle. Es crea un tauler 3×3 ja jugable amb vuit fitxes clicables.
2. Selecciona el fill Board (SlidingBoardView) i assigna una imatge a Source Image: es talla en fragments i indica al jugador l'ordre correcte.
3. Per canviar la mida, modifica Columns, Rows i Hole Cell a SlidingPuzzle i prem Rebuild board a Board.
4. Shuffle Move Count indica quants moviments legals es fan per barrejar: sempre té solució.
5. Crea les dades del puzle: al panell Project, clic dret > Create > Escape Room Framework > Puzzles > Puzzle Definition. Posa-hi un Persistent Id únic (per exemple sala1_teclat), un Display Name i l'Objective. Després crea les pistes amb Create > Escape Room Framework > Hints > Hint Data (tres pistes: context, indicació i solució) i arrossega-les al camp Hints de la definició.
6. Selecciona l'arrel del puzle i arrossega la definició al camp Definition del component.
7. Crea la recompensa: Escape Room Framework > Create > Interactables > Door i marca Is Locked a la porta. Selecciona el puzle, fes Ctrl+clic a la porta i executa Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. A On Solved hi apareixeran Door.Unlock i Door.ForceOpen.

**Com ho proves**

- Només es mou una fitxa veïna del forat; les altres fan un petit rebot.
- Prova-ho a Play: guarda amb F5 a mig puzle i un cop resolt, surt i carrega amb F9. L'estat ha de ser el mateix i la porta ha de continuar oberta.

**Compte amb**


<a id="lights"></a>
### Circuit de llums

Menú: `Assets/_EscapeRoomTemplate/Prefabs/LinkedLightsPuzzleKit.prefab` · Sala del museu 14

**Passos**

1. Arrossega Prefabs/LinkedLightsPuzzleKit.prefab a l'escena. Inclou el panell, cinc botons (LinkedLightButton), etiquetes i un botó de reinici.
2. A LinkedLightsPuzzle > Nodes, cada llum té Initially On, Target On, Connections (índexs que commuta el seu botó, començant per 0; inclou el propi si s'ha de commutar) i Indicator.
3. El prefab comparteix la definició demo_linked_lights: duplica-la, canvia'n el Persistent Id i assigna la còpia. Fes-ho per a cada circuit de l'escena.
4. Crea la recompensa: Escape Room Framework > Create > Interactables > Door i marca Is Locked a la porta. Selecciona el puzle, fes Ctrl+clic a la porta i executa Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. A On Solved hi apareixeran Door.Unlock i Door.ForceOpen.

**Com ho proves**

- Amb la configuració de mostra es resol prement 1, 3 i 5. El botó de reinici torna a l'estat inicial.
- Prova-ho a Play: guarda amb F5 a mig puzle i un cop resolt, surt i carrega amb F9. L'estat ha de ser el mateix i la porta ha de continuar oberta.

**Compte amb**

- Assegura't que té solució: parteix de l'objectiu i aplica unes quantes pulsacions per obtenir l'estat inicial.

<a id="throw"></a>
### Llançament a dianes

Menú: `Escape Room Framework > Create > Puzzles > Throw Puzzle` · Sala del museu 6

**Passos**

1. Executa Escape Room Framework > Create > Puzzles > Throw Puzzle. Es creen tres dianes amb ThrowTarget (target_1, target_2, target_3) llistades a Target Ids.
2. Posa objectes per llançar: Escape Room Framework > Create > Interactables > Physics Grabbable, amb Can Be Thrown activat.
3. A cada diana, Min Impact Speed (m/s) evita que un cop suau compti.
4. Tanca l'espai amb parets amb collider perquè els objectes no es perdin.
5. Crea les dades del puzle: al panell Project, clic dret > Create > Escape Room Framework > Puzzles > Puzzle Definition. Posa-hi un Persistent Id únic (per exemple sala1_teclat), un Display Name i l'Objective. Després crea les pistes amb Create > Escape Room Framework > Hints > Hint Data (tres pistes: context, indicació i solució) i arrossega-les al camp Hints de la definició.
6. Selecciona l'arrel del puzle i arrossega la definició al camp Definition del component.
7. Crea la recompensa: Escape Room Framework > Create > Interactables > Door i marca Is Locked a la porta. Selecciona el puzle, fes Ctrl+clic a la porta i executa Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. A On Solved hi apareixeran Door.Unlock i Door.ForceOpen.

**Com ho proves**

- A PC: E per agafar, clic esquerre per llançar i Q per deixar. A VR: agafa amb el grip i deixa anar fent el gest.
- Prova-ho a Play: guarda amb F5 a mig puzle i un cop resolt, surt i carrega amb F9. L'estat ha de ser el mateix i la porta ha de continuar oberta.

**Compte amb**


<a id="place"></a>
### Col·locació de peces

Menú: `Escape Room Framework > Create > Puzzles > Placement Puzzle` · Sala del museu 7

**Passos**

1. Executa Escape Room Framework > Create > Puzzles > Placement Puzzle. Es creen dues peces (piece_a, piece_b) i tres suports (socket_a, socket_b i socket_c, que fa d'esquer).
2. A PlacementPuzzle, cada regla (Rules) diu a quin suport va cada peça: pieceId → correctSocketId.
3. A cada peça, GrabbablePiece > Piece Id ha de coincidir amb la regla. Return When Lost la torna al lloc si cau fora de l'abast.
4. A cada suport, PieceSocketReceiver té el puzle, el Socket Id i un Snap Point. Els esdeveniments On Piece Seated / Removed / Locked serveixen per a so i animació.
5. Crea les dades del puzle: al panell Project, clic dret > Create > Escape Room Framework > Puzzles > Puzzle Definition. Posa-hi un Persistent Id únic (per exemple sala1_teclat), un Display Name i l'Objective. Després crea les pistes amb Create > Escape Room Framework > Hints > Hint Data (tres pistes: context, indicació i solució) i arrossega-les al camp Hints de la definició.
6. Selecciona l'arrel del puzle i arrossega la definició al camp Definition del component.
7. Crea la recompensa: Escape Room Framework > Create > Interactables > Door i marca Is Locked a la porta. Selecciona el puzle, fes Ctrl+clic a la porta i executa Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. A On Solved hi apareixeran Door.Unlock i Door.ForceOpen.

**Com ho proves**

- Prova peces intercanviades, treure-les abans de resoldre i carregar un cop resolt: les peces queden fixades.
- Si guardes a mig puzle, en carregar les peces soltes tornen al seu lloc inicial.

**Compte amb**

- Randomize Mapping canvia quin suport és el correcte a cada partida: la pista ha de ser dinàmica.

<a id="multi"></a>
### Puzles encadenats

Menú: `Escape Room Framework > Create > Puzzles > Multi-Stage Puzzle` · Sala del museu 11

**Passos**

1. Executa Escape Room Framework > Create > Puzzles > Multi-Stage Puzzle. Es crea un grup amb dos fills visibles: Puzzle01_Sequence i Puzzle02_Levers.
2. A MultiStagePuzzle > Puzzles, cada entrada té un id, el puzle fill i la seva arrel d'interacció. Hi pots afegir qualsevol altre puzle de l'escena.
3. Require Order obliga a resoldre'ls en ordre; Lock Future Puzzles fa que els següents es vegin però no responguin fins al seu torn.
4. Cada fill necessita la seva PuzzleDefinition amb un ID únic. El grup també en té una.
5. Connecta la porta només a l'On Solved del grup, no als dels fills (usa Link Selected Objects To Puzzle amb el grup seleccionat).

**Com ho proves**

- La porta s'obre només quan tots els fills estan resolts.
- Prova-ho a Play: guarda amb F5 a mig puzle i un cop resolt, surt i carrega amb F9. L'estat ha de ser el mateix i la porta ha de continuar oberta.

**Compte amb**


## Sistemes de la sala

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

