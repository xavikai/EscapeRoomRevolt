# Escape Room: guia de creació i lliurament

**Idioma:** [castellano](ESCAPE_ROOM_QUICKSTART.md) · català · [English](ESCAPE_ROOM_QUICKSTART.en.md) · Receptes pas a pas: [RECEPTES.md](RECEPTES.md)

Aquesta guia s'adreça a qui utilitza la plantilla per crear el seu propi joc. L'abast d'aquesta edició és **Escape Room per a PC i VR amb comandaments**. Survival Horror continua en desenvolupament i no forma part de la validació comercial d'aquesta edició. El resultat de les comprovacions és a [l'auditoria](../AUDITORIA_ESCAPE_ROOM_2026-09-09.md); la referència extensa és a [DOCUMENTACIO_COMPLETA.md](../DOCUMENTACIO_COMPLETA.md).

## 1. Obrir el projecte i jugar la mostra

1. Instal·la Unity **6000.4.9f1**. Per a Quest, instal·la també Android Build Support i les seves eines SDK, NDK i OpenJDK des d'Unity Hub.
2. Obre la carpeta que conté `Assets`, `Packages` i `ProjectSettings`. Espera que acabin la importació i la compilació.
3. Conserva les versions de `Packages/manifest.json` i `packages-lock.json`. L'assemblat Player fa referència a XRI i XR Core Utils fins i tot en compilar per a PC; no eliminis els paquets XR d'aquest projecte.
4. Obre `Assets/_EscapeRoomTemplate/Scenes/ShowcaseMuseum.unity` per a PC, o `ShowcaseMuseumVR.unity` per al museu VR. `VRTemplate` és una escena mínima, no la demostració completa.
5. Executa `Escape Room Framework > Validation > Run Framework Smoke Tests` i `Validate Current Scene`. Llegeix el resultat a la Console, no només la confirmació que el menú s'ha executat.
6. Prem Play. A PC, utilitza WASD i el ratolí, E per interactuar, I per a l'inventari, H per a les pistes i Esc per tancar el panell o fer pausa. F5/F9 desen/carreguen l'slot ràpid. Els tres slots manuals són independents.

El museu PC conté 13 controladors de puzle (el tretzè és la sala 14, `LinkedLightsPuzzle`); `ShowcaseMuseumVR` conté els 12 anteriors i encara no inclou la sala 14. Per a una visita guiada i interactiva de cada mecànica, obre [`docs/guia/index.html`](../../../docs/guia/index.html) al navegador. Les rodes numèriques utilitzen `StatePuzzle`; la melodia utilitza `SequencePuzzle`. No són solvers separats. `LockedOffice` i `LockedOfficeVR` contenen una mostra alternativa amb dos panells de codi.

## 2. Crear una habitació pròpia

1. Crea una carpeta pròpia, per exemple `Assets/MyEscapeRoom`, i desa-hi la teva escena, les dades i l'art. Conserva les escenes de mostra com a referència.
2. Selecciona `Configuration > Use Escape Room Profile`. Els museus inclouen una excepció local de llanterna per a la seva demostració de combinació; una escena nova no hereta aquesta excepció.
3. Utilitza `Setup > Instantiate Game Manager` i el jugador PC, o instancia `Prefabs/Player_VR.prefab`. Hi ha d'haver un sol jugador actiu de la plataforma triada. Afegeix un terra amb collider, il·luminació i límits físics.
4. Crea objectes amb el menú `Create`. Mantén components, colliders i esdeveniments a l'arrel lògica. Canvia l'art sota `ModelSocket` o el fill visual indicat pel prefab.
5. Crea un `PuzzleDefinition` i un `HintData` propis per a cada puzle. Assigna la definició al controlador i les pistes a la definició. Escriu pistes graduals: context, indicació i solució.
6. Comprova els IDs: cada entitat persistent ha de tenir un `SaveId` únic. Un `PuzzleDefinition.PersistentId` identifica el puzle en desar. Duplicar un objecte o una definició pot copiar-ne l'ID: revisa i canvia aquest ID abans d'utilitzar les dues còpies juntes. Reutilitza un `InventoryItemData` per a diverses unitats del mateix ítem; crea un altre `ItemId` per a un altre tipus d'objecte.
7. Afegeix l'escena a la llista d'escenes habilitades de Build Profiles. Configura `Resources/GameFlowSettings.asset`: `First Gameplay Scene` per a Nova partida i `Main Menu Scene` per al retorn al menú. Utilitza noms únics o rutes completes.
8. Valida l'escena i prova el recorregut de principi a fi, inclòs desar a mig de cada mecanisme, sortir i carregar.

### Exemple: un codi que obre una porta

1. Crea un panell de codi i una porta des del menú `Create`.
2. A `CodePanelPuzzle`, posa `Correct Code = 3142`, `Max Code Length = 4` i activa `Auto Check When Full`.
3. Cada botó ha de cridar `InputDigit` amb un únic caràcter. El botó d'esborrar utilitza `C` o `ClearInput`; si desactives la comprovació automàtica, afegeix un botó que cridi `SubmitCode`.
4. A `On Solved` del controlador, afegeix la porta i selecciona el seu mètode públic d'obertura al desplegable. La porta ha de tenir el seu propi ID persistent.
5. Afegeix una pista visible que permeti deduir 3142. No activis la variant aleatòria si la pista escrita continua mostrant una solució fixa.
6. Prova un codi incorrecte, el correcte, desar amb la porta oberta i tornar a carregar. `On Solved` representa una resolució nova; no es torna a emetre en carregar. La porta ha de restaurar el seu estat mitjançant el seu propi desament.

## 3. Triar i configurar les mecàniques

Els noms entre accents greus són camps o mètodes del component. Pots connectar els mètodes públics mitjançant UnityEvents de l'Inspector quan la seva signatura sigui compatible.

| Mecànica | Configuració i entrada | Comprovació mínima |
|---|---|---|
| Codi: `CodePanelPuzzle` | Codi, longitud, comprovació automàtica. Botons → `InputDigit`; confirmar → `SubmitCode`. | Un error neteja l'intent; l'encert activa l'esdeveniment una vegada. |
| Seqüència: `SequencePuzzle` | `Correct Sequence` amb IDs ordenats. Cada botó/palanca → `InputStep(id)` o un `SequenceStepButton`. | Un error reinicia l'intent. Desar després del primer pas i carregar permet continuar pel segon. |
| Estats i rodes: `StatePuzzle` | Llista `Conditions`: un `SteppedPositioner` i el seu `Required Index`, començant per zero. | Totes les posicions han de coincidir. Una referència absent impedeix resoldre. El puzle desa la posició de cada palanca/roda: en carregar, la combinació a mitges es conserva. |
| Rodes numèriques | `Create > Number Wheels Puzzle`; entre 2 i 8 rodes. Edita amb `NumberWheelsPuzzleAuthoring` i reconstrueix des del seu Inspector. | PC: entra en focus i prem ▲/▼. VR: botons físics equivalents. Revisa el codi després de redimensionar. |
| Ítem d'inventari: `SocketPuzzle` | `Required Item Id`, consum opcional i prefab visual amb punt de col·locació. El teu control d'inventari crida `TryInsertItem` després de comprovar que el jugador té l'ítem. | Rebutja un ID incorrecte; en carregar resolt apareix una sola peça visual. |
| Receptor amb UI: `ItemReceiver` | Assigna `Required Item`, la política de selecció i `On Item Accepted`. És l'opció d'autoria amb selecció d'inventari integrada. | Sense l'ítem no s'obre; utilitzar-lo només el consumeix quan correspon. No connectis dos consums per a una mateixa acció. |
| Objectes físics: `PhysicsGrabbable` + `PhysicsSocket` | Rigidbody, collider, `PickableItem.Data` i `Required Item Id` coincidents; el socket té un trigger i un punt d'encaix. | Només encaixa en deixar-lo anar. Un objecte fixat no es pot tornar a agafar. Aquest socket no implementa `ISaveable`: per a un resultat persistent, connecta un estat desable propi o utilitza el puzle de col·locació. |
| Col·locació: `PlacementPuzzle` | Regles `pieceId → correctSocketId`; peces amb `GrabbablePiece` i receptors `PieceSocketReceiver` enllaçats al mateix puzle. | Prova peces intercanviades, retirada abans de resoldre i càrrega després de resoldre. Els IDs de peces i sockets han de coincidir exactament. Les peces soltes no es desen: en carregar un tauler sense resoldre tornen al seu lloc inicial. |
| Llançament: `ThrowPuzzle` | Configura les dianes `ThrowTarget` requerides i objectes físics que es puguin llançar. | S'han d'encertar totes les dianes requerides. Assegura't que les peces es puguin recuperar. |
| Lliscant: `SlidingPuzzle` | Files, columnes, cel·la buida, moviments de barreja; presentació amb `SlidingBoardView` i `SlidingTileButton`. | Només es mou una fitxa veïna del forat; l'estat carregat coincideix amb el desat. |
| Canonades: `PipePuzzle` | Tiles amb ID, fila, columna, connexions i gir inicial; configura l'origen i la destinació. `PipeTileButton` → `RotateTile`. | Comprova la connectivitat real de l'origen a la destinació, no només l'aspecte. El solver necessita una presentació interactiva. |
| Circuit de llums: `LinkedLightsPuzzle` | Llista `Nodes`: estat inicial, estat objectiu, índexs que commuta cada botó i indicador. Cada botó → `Press(index)` (`LinkedLightButton`). | Desa el progrés parcial; `ResetPuzzle` torna a l'estat inicial. Comprova que l'objectiu és assolible. |
| Grup: `MultiStagePuzzle` | Llista de puzles fills i arrels d'interacció; ordre lliure o obligatori i bloqueig dels següents. | Els fills continuen visibles; el grup es resol una vegada en completar-los tots. El premi final es connecta al grup. |
| Melodia: `MelodyPlayer` | Presentació sonora dels passos d'una seqüència. | Afegeix una pista visual equivalent; evita dependre únicament de l'oïda. |
| Notes i examen | `InteractableNote` per a la lectura; `InventoryItemData` amb dades de lectura/examen i `ExamineHotspot` per a zones del model. | Text llegible, tancar retorna el control i un hotspot no es dispara a través d'un altre panell. |
| Interruptors, portes i calaixos | `InteractableTrigger`, `InteractableToggle`, `SteppedPositioner`, `Door`; configura els esdeveniments i el recorregut. | Sense duplicar esdeveniments, sense travessar límits, mateix ús a PC i VR. `InteractableToggle` i `InteractableTrigger` desen el seu estat; activa `Invoke Event On Load` a l'interruptor si un llum sense desament propi ha de recuperar el seu estat. Per girar sobre una frontissa, utilitza com a `Visual Transform` un objecte buit situat a la frontissa. |
| Pistes | `HintData` a la definició; zones opcionals `HintZoneTrigger`. | Pistes pertinents al puzle actiu i retirada en resoldre'l. |
| Temps i perill mòbil | `GameOverTimer.StartTimer` i `MovingHazard.StartHazard`, activats per un botó o `EventTriggerZone`. Marcadors 3D defineixen el recorregut del perill. | La pausa congela l'avanç, el HUD reflecteix el temps, la derrota mostra els resultats. Són mecanismes independents. |
| Objectius i final | `ObjectiveSet`/`ObjectiveManager`, prerequisits sense cicles; `GameEndTrigger` i `EndingDefinition`. | No s'acaba abans de l'últim objectiu; Reintentar permet tornar a recollir els objectes. |
| Canvi d'habitació | `RoomPortal`, escena de destinació habilitada, `RoomSpawnPoint` amb ID. | En mode Single no hi ha memòria cau completa de les habitacions anteriors. En Additive evita gestors i jugadors duplicats. |

### Reiniciar un puzle no equival a reiniciar el món

`ResetPuzzle` torna el controlador a no resolt i neteja el seu estat específic. No reverteix automàticament una porta connectada per esdeveniment, no retorna ítems consumits i no reconstrueix cap objecte destruït per lògica pròpia. Si ofereixes un botó de reinici, connecta-hi també la restauració del seu entorn. Per començar de zero, utilitza Nova partida o Reintentar de l'escena.

## 4. Inventari, combinació i desament

1. Crea un `InventoryItemData` amb ID, nom, icona i les accions que hagi de permetre. Afegeix-lo a l'`ItemCatalog` que s'utilitzi al joc; les referències han d'arribar a la build.
2. Per recollir-lo, assigna les seves dades a `PickableItem`. Per combinar, configura `Combinations` amb l'altre ítem i el resultat. Inclou també el resultat al catàleg.
3. Prova la combinació amb l'inventari ple i amb quantitats múltiples. No utilitzis un objecte imprescindible que es pugui perdre sense una manera de recuperar-lo.
4. Desa abans i després de consumir una clau, resoldre, obrir una porta o completar un objectiu. Tanca l'executable i carrega, a més de provar la càrrega dins de la mateixa sessió.
5. Els fitxers són a `Application.persistentDataPath/SaveSlots`. Cada slot té un JSON, una miniatura i, després d'una sobreescriptura, una còpia `.bak`. No esborris IDs de contingut ja publicat si vols mantenir la compatibilitat amb les partides.
6. Una còpia de seguretat pot recuperar un JSON principal absent o estructuralment invàlid. Això no repara estats arbitraris incorrectes dins de cada component. Conserva còpies externes per a les proves de migració.
7. Els slots PC i VR d'aquestes mostres no s'han d'anunciar com a intercanviables: les seves rutes d'escena i alguns components del jugador són diferents.

## 5. Preparar VR amb comandaments

1. Parteix de `ShowcaseMuseumVR` per estudiar les mecàniques o de `VRTemplate` per a una escena mínima.
2. Comprova OpenXR amb `Setup > Configure OpenXR (PC + Android)` i la finestra Project Validation. Mantén els perfils dels comandaments que vulguis admetre.
3. Per a una escena pròpia, prepara els interactuables amb `Setup > Prepare Current Scene Interactables for VR`. Revisa després les referències de cada objecte i no regeneris una demo personalitzada sense desar-ne una còpia.
4. El rig utilitza `VRPlayerPlatformAdapter`, la presentació UI Toolkit en 3D i els seus ponts de punter. Conserva les referències del cap i de les dues mans. El jugador VR ha de ser l'únic jugador actiu.
5. La ruta `VRHardwareInteractor` utilitza el gallet per activar i el grip/gallet per subjectar objectes físics. Deixar anar tots dos allibera l'objecte; el llançament depèn del moviment de la mà i de `Can Be Thrown`. Els sockets esperen que es deixi anar l'objecte.
6. La ruta XRI utilitza `VRInteractionBridge`. No configuris dues accions diferents perquè una sola pulsació activi dues vegades un mecanisme. Verifica especialment interruptors, botons ▲/▼ i recollida.
7. Per provar sense visor, activa l'objecte de simulador indicat a `VRTemplate`. La simulació no verifica els botons físics ni substitueix la prova de l'executable al dispositiu. Desactiva-la per al lliurament en visor.
8. Revisa `Resources/VRComfortSettings.asset`: locomoció, gir per salts/continu i preferències. Prova alçades assegut i dret, l'accessibilitat de tots els controls i l'escala del panell.
9. No utilitzis càmeres de feedback ni desplaçaments forçats del cap com a requisit per resoldre. La base de puzles omet els seus talls de càmera a VR; qualsevol càmera o cinemàtica pròpia necessita el mateix tractament.

El paquet Quest de `ReleaseBuilder` arrenca directament a `ShowcaseMuseumVR` i no inclou cap escena de menú inicial. El menú de pausa/resultats omet el retorn al menú quan aquesta escena no està disponible. Per a un joc VR amb menú inicial, crea una escena de menú compatible amb VR, inclou-la a la build i configura'n les rutes: no reutilitzis sense més el menú PC.

`Build > Release > Build Windows` genera la demo d'escriptori sense iniciar XR i restaura la configuració XR de l'editor en acabar. Un executable PCVR requereix la seva pròpia llista d'escenes VR i mantenir activa la inicialització d'XR; no utilitzis aquesta ordre d'escriptori per a un lliurament PCVR. A Android s'ha aplicat la prioritat de lectura d'input indicada per la validació de Meta Quest/OpenXR.

## 6. Abans de lliurar a un client

| Verificació | Criteri d'acceptació |
|---|---|
| Instal·lació neta | Importar el paquet en una altra carpeta amb la versió documentada; no depèn de Library ni d'eines privades de desenvolupament. |
| Recorregut PC | Completar el museu i l'habitació pròpia sense consola de depuració; provar l'error i l'encert a cada mecànica. |
| Desament | Desar, tancar l'executable i carregar a mig de cada mecanisme, després de recollir/consumir i després de canviar d'escena. |
| VR en dispositiu | Registrar el visor, els comandaments, el runtime, la versió de build i el resultat de cada fila de la matriu VR. |
| Rendiment | Mesurar CPU/GPU, memòria i estabilitat dels frames al maquinari objectiu amb l'escena final. Aquesta auditoria no certifica cap pressupost. |
| Contingut | Completar la procedència i les condicions de redistribució a `ThirdPartyNotices.md`; substituir el material sense procedència verificable. |
| Idiomes | Revisar també notes, prompts, pistes, puzles i finals; el selector d'idioma no garanteix que tot el contingut propi estigui traduït. |
| Paquet | Mantenir `.meta`, escenes, dades, prefabs, shaders, UI i les dependències necessàries. Excloure builds, memòries cau, logs i eines MCP del lliurament al comprador. |

No eliminis la carpeta Survival de manera aïllada per empaquetar: actualment hi ha referències de compilació des de sistemes compartits. El perfil Escape Room en desactiva les funcions opcionals; separar físicament aquests assemblats requereix feina addicional.

### Matriu VR que s'ha d'omplir per dispositiu

| Cas | Resultat / dispositiu / build |
|---|---|
| Arrencada, tracking, àudio i recentrament | Pendent |
| Totes dues mans: interacció, agafar, deixar anar i llançament | Pendent |
| Totes les sales; dianes, col·locació i rodes | Pendent |
| Inventari, combinació, notes, examen i hotspots | Pendent |
| Pausa, configuració, desar/carregar i tancar el panell sense activar el món | Pendent |
| Teletransport, gir i locomoció triada; assegut/dret | Pendent |
| Final, derrota, Reintentar i objectes recollibles restaurats | Pendent |
| Pèrdua de tracking, comandament desconnectat i suspensió/represa | Pendent |
| Rendiment sostingut durant una partida completa | Pendent |

## 7. Resoldre problemes freqüents

| Símptoma | Què cal revisar |
|---|---|
| No s'obre des del Hub | Comprova que cap altra instància o prova d'Unity no tingui obert el mateix projecte. No esborris el lock mentre continuï activa. |
| «Missing script» després d'importar | Versió de l'editor, paquets resolts i errors de compilació; conserva els `.meta` i les referències d'assemblat. |
| El puzle no es resol | IDs, ordre, índexs des de zero, definició i condicions/referències completes. |
| Una porta es torna a tancar en carregar | El seu propi desament/ID i la restauració visual; no depenguis que `On Solved` es torni a emetre. |
| VR no mostra el museu | Obre `ShowcaseMuseumVR`; `VRTemplate` és només l'arrencada mínima. |
| Un control VR s'activa dues vegades | Revisa els esdeveniments duplicats i les rutes XRI/maquinari del rig. |
| Falta un objecte necessari | Recuperació de peces, límits físics i opcions de llançament; revisa IDs duplicats i regles de consum. |
| El menú no pot carregar una escena | Inclou la destinació a la llista utilitzada per aquesta build; comprova la configuració de flux. |

Per a camps avançats i extensions de codi, consulta [la guia de programació](../PROGRAMMING_GUIDE.md) i [la referència completa](../DOCUMENTACIO_COMPLETA.md).
