# Puzzle Definition i Hint Data, pas a pas

Un puzle té una part a l'escena i dues fitxes de dades al Project. La part de l'escena conté les peces, la solució i les accions de recompensa. Les fitxes permeten identificar el puzle i preparar les pistes que rebrà el jugador.

```text
Hierarchy: arrel del puzle
  StatePuzzle / CodePanelPuzzle / ThrowPuzzle / …
    Definition ──► Project: Def_sala1_candau
                     Hints ──► Project: Hint_sala1_candau
                                  Hints [0]: context
                                  Hints [1]: indicació
                                  Hints [2]: solució
```

Les fletxes representen camps on has d'arrossegar un asset. `Definition` apunta a una `Puzzle Definition`. El camp `Hints` d'aquesta definició apunta a **un sol `Hint Data`**, que conté una llista de pistes. Per a tres pistes crees un Hint Data amb tres elements, no tres assets.

## 1. Què va a cada lloc

| Element | On és | Què hi configures |
| --- | --- | --- |
| Component del puzle | Hierarchy, a l'arrel | Mecànica, combinació o condicions, peces i `On Solved`. A les rodetes, la combinació es modifica a `NumberWheelsPuzzleAuthoring` i s'aplica amb `Rebuild wheels and layout`. |
| `Puzzle Definition` | Asset al Project | Identitat persistent, nom, categoria, descripció de l'objectiu, referència a pistes i penalització de fallada. |
| `Hint Data` | Asset al Project | Textos, àudios opcionals i temps de les pistes. |

Escriure «el codi és 427» a `Objective` o a una pista no configura la combinació del candau. La combinació s'ha de configurar al puzle de l'escena. Tampoc s'obre una porta només per assignar una definició: cal connectar la recompensa a `On Solved`.

## 2. Crear la definició

Fes-ho fora de Play, perquè els canvis de l'escena es conservin.

1. Al Project, crea una carpeta teva, per exemple `Assets/NomDelGrup/NomAlumne/Puzzles`.
2. Dins d'aquesta carpeta, clic dret > `Create > Escape Room Framework > Puzzles > Puzzle Definition`.
3. Anomena l'asset `Def_sala1_candau`. Selecciona'l al Project per editar-lo a l'Inspector.
4. Omple els camps:

| Camp | Exemple | Per a què serveix |
| --- | --- | --- |
| `Persistent Id` | `sala1_candau` | Identificador del puzle en el guardat i en els esdeveniments de resolució. Ha de ser únic entre els puzles de la partida. |
| `Display Name` | `El candau de l'armari` | Nom llegible del puzle. Pot ser diferent del nom de l'asset i del GameObject. |
| `Category` | `State` | Classifica aquest puzle de rodetes. No canvia les regles ni la solució. |
| `Objective` | `Troba les tres xifres i obre l'armari.` | Descripció del que ha de fer el jugador. Aquest camp no crea per si sol una missió visible al HUD; els objectius de partida es configuren amb `ObjectiveDefinition` i `ObjectiveManager`. |
| `Hints` | `Hint_sala1_candau` | Aquí arrossegaràs el Hint Data que crearàs al pas següent. |
| `Sanity Penalty` | `0` | Penalització enviada als mòduls de joc quan el controlador declara una fallada. No és un temporitzador ni una penalització per cada clic. El seu efecte depèn dels mòduls actius, com Survival. |

El nom del fitxer `Def_sala1_candau` no substitueix el `Persistent Id`: són camps diferents. Si dupliques un puzle per crear-ne un altre independent, duplica també la definició i canvia el Persistent Id de la còpia. Si comparteixen la mateixa definició, comparteixen l'ID i poden confondre el guardat i els objectius que escolten aquest ID. Mantén els IDs estables quan ja proves partides guardades.

## 3. Crear les pistes

1. A la mateixa carpeta del Project, clic dret > `Create > Escape Room Framework > Hints > Hint Data`.
2. Anomena l'asset `Hint_sala1_candau` i selecciona'l.
3. Per provar, posa `Delay Before First Hint = 5` i `Delay Between Hints = 5`. Els dos valors són segons. Quan funcioni, pots tornar als valors per defecte: 120 i 60 segons.
4. Desplega `Hints` i afegeix tres elements amb `+` o posa `Size = 3`, segons l'Inspector. Omple el `Hint Text` de cada element, de menys a més explícit:

| Element | Intenció | Exemple per a un candau amb codi fix 427 |
| --- | --- | --- |
| `Element 0` | Context | `La nota de la taula parla de tres objectes.` |
| `Element 1` | Indicació | `Compta els objectes en l'ordre que marca la nota.` |
| `Element 2` | Solució | `Posa les rodetes a 4, 2 i 7, d'esquerra a dreta.` |

`Hint Audio` és opcional: hi pots arrossegar un AudioClip de veu. Deixar-lo buit permet mostrar només text. Amb la interfície de la plantilla, la pista es demana com a subtítol i queda visible uns cinc segons; comprova que l'opció Subtítols estigui activada. La veu necessita l'AudioManager i el volum de veu audible.

Els exemples són en català per explicar el contingut. Si el joc té diversos idiomes, afegeix els textos al catàleg de localització que utilitzi el projecte. No escriguis una solució fixa en una pista si el puzle genera una combinació aleatòria: el text del Hint Data no calcula el codi de la partida.

## 4. Enllaçar els dos assets amb el puzle

1. Selecciona `Def_sala1_candau` **al Project**. Arrossega `Hint_sala1_candau` al seu camp `Hints`.
2. Selecciona **l'arrel del puzle a la Hierarchy**. A les rodetes creades pel menú, busca el component `StatePuzzle`, apartat `Puzzle Identity`.
3. Arrossega `Def_sala1_candau` al camp `Definition` d'aquest component. No l'assignis al model visual, al botó ni a cada rodeta.
4. Desa l'escena amb `Ctrl+S`. Comprova que cap dels dos enllaços sigui `None`.

Amb una definició que tingui `Persistent Id`, aquest ID té prioritat sobre el camp antic `Puzzle Id` del component. Canviar només el camp antic no evita una col·lisió d'IDs si has deixat la mateixa definició en dues còpies.

## 5. Quan apareixen les pistes

Les pistes necessiten un context actiu. Crear l'asset i entrar a Play no inicia el temporitzador.

- **Inici des de la mecànica:** el controlador activa el Hint Data en passar d'`Unsolved` a `InProgress`. A les rodetes passa en canviar un número; mirar el candau o entrar a la càmera de detall encara no ho fa. Al Throw Puzzle passa amb el primer impacte vàlid; al teclat, en introduir un dígit.
- **Inici en acostar-se:** crea `Escape Room Framework > Create > Triggers > Hint Zone`, col·loca el volum davant del candau i assigna el mateix `Hint_sala1_candau` al camp `Puzzle Hint Data`. Així el context s'activa quan hi entra el jugador. El volum ha de cobrir una zona per on passi el jugador, no només el model del candau.
- **Sortida de zona:** amb `Clear On Exit` activat es neteja aquest context quan el jugador surt del volum. Si torna a entrar, les pistes comencen de nou. Desactivat, el context pot continuar després de sortir.
- **Temps i tecla:** la primera pista automàtica espera `Delay Before First Hint`; les següents esperen `Delay Between Hints` des de l'última pista. `H`, amb els controls PC per defecte, avança a la pista següent sense esperar i reinicia l'espera. No repeteix l'anterior. Quan s'acaba la llista, no hi ha més pistes.
- **Canvi de puzle:** el HintManager manté un sol context actiu. Un altre puzle o zona pot substituir-lo. Tornar a activar el mateix Hint Data mentre continua actiu no reinicia el comptador; activar-ne un de diferent sí. Per a puzles independents, crea Hint Data independents.
- **Finalització:** resoldre o reiniciar el puzle neteja les pistes si el seu Hint Data és el context actiu. Canviar d'escena amb càrrega Single també neteja el context. Sortir de la càmera de detall, per si sol, no equival a resoldre el puzle ni atura les pistes.

Una Hint Zone només activa un Hint Data: no comprova si el puzle ja està resolt. Si no vols que es reactivin les pistes en tornar-hi després de resoldre'l, connecta a `On Solved` la desactivació del GameObject de la zona, per exemple amb `GameObject.SetActive(false)`.

El temporitzador utilitza temps de joc: amb el temps pausat a zero no avança. Els comptadors i la pista següent no es desen amb l'estat del puzle. Després de carregar, pot caldre tornar a activar el context; comprova-ho al teu recorregut.

## 6. Provar-ho i detectar què falta

1. En una partida nova, comprova que el Bootstrapper de la plantilla ha creat un `HintManager` i que tens el HUD de joc amb subtítols. No afegeixis un manager nou a cada puzle.
2. Entra al candau. Sense Hint Zone, prem primer una fletxa de rodeta per activar el context. Amb Hint Zone, entra dins del volum.
3. Prem `H`: ha d'aparèixer la pista 1. Prem-la dues vegades més: han d'aparèixer la 2 i la 3, en ordre. Una quarta pulsació no en crea cap més.
4. Surt de Play i torna a començar. Activa el context i espera cinc segons sense prémer H: ha de sortir la primera pista automàtica. Cinc segons després, la següent.
5. En una altra prova, resol el puzle abans dels cinc segons: les seves pistes han d'aturar-se. Comprova també la sortida i reentrada a la zona, i guardar/carregar a mig puzle.
6. Torna els temps als que vulguis per al joc i executa `Escape Room Framework > Validation > Validate Current Scene`.

| Símptoma | Què has de revisar |
| --- | --- |
| `H` no fa res des del primer moment | Els dos enllaços `Definition → Hints`, llista no buida, HintManager i context actiu. Obrir la càmera del candau no activa les pistes. |
| Hi ha veu però no text | L'opció Subtítols i el HUD de la plantilla. |
| Hi ha text però no veu | `Hint Audio`, AudioManager i volum de veu. L'àudio és opcional. |
| Surten pistes d'un altre puzle | Una altra zona o interacció ha canviat el context, o dues definicions apunten al mateix Hint Data. |
| En sortir continuen les pistes | Sortir de la vista no les atura; revisa `Clear On Exit` si utilitzes una Hint Zone. |
| Dos puzles comparteixen estat o recompensa d'objectiu | Revisa els `Persistent Id` i les definicions duplicades. |
| La combinació no funciona tot i haver-la escrit a la fitxa | Configura-la al controlador o a `NumberWheelsPuzzleAuthoring`; Objective i Hint Text són textos. |
