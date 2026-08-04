# Roadmap viu — Escape Room / Survival Horror Framework

Data de l'auditoria: 2 d'agost de 2026  
Projecte auditat: Unity `6000.4.9f1`, URP `17.4.0`, Input System `1.20.0`, XRI `3.3.0`  
Perfil actiu actual: `SurvivalHorror`

## Com utilitzar aquest document

Aquest és un backlog viu, no un historial. Quan una tasca compleixi tots els seus criteris d'acceptació i hagi estat verificada a Unity, s'ha d'eliminar de la taula corresponent. Els sistemes ja consolidats es documenten a la secció «Base funcional verificada» i no s'han de tornar a afegir al backlog si no apareix una regressió.

Regles de manteniment:

1. No marcar una tasca com a acabada només perquè compila.
2. Verificar-la almenys en una escena mínima i en la demo corresponent.
3. Afegir tests EditMode o PlayMode quan hi hagi lògica determinista o flux crític.
4. Comprovar els perfils `EscapeRoom`, `SurvivalHorror` i `CustomHybrid` quan afecti una mecànica opcional.
5. Comprovar PC, comandament i VR quan afecti entrada, UI o interacció.
6. Eliminar la fila només després de superar els criteris d'acceptació.

## 1. Resum executiu

La plantilla ja és una base funcional i bastant completa per crear Escape Rooms en primera persona. Té interacció, inventari contextual, combinació, examen 3D, puzles, pistes, objectius, finals, Save/Load i UI Toolkit. La separació per perfils de gènere funciona i evita mostrar cordura o llanterna quan no corresponen.

Com a Survival Horror ja existeix una vertical slice tècnica inspirada en el tipus de tensió d'Outlast: patrulla, percepció visual i auditiva, persecució, amagatalls, dany, mort, checkpoints i recorregut d'escapada. El treball pendent és convertir aquesta base funcional en una experiència de durada comercial i completar QA real de VR i hardware.

Veredicte actual:

| Àrea | Estat | Valoració |
|---|---|---|
| Escape Room | Funcional | Base sòlida per construir jocs complets, amb mancances de varietat i eines d'autor. |
| Survival Horror ambiental | Funcional parcial | Llanterna, cordura i esdeveniments configurables funcionen. |
| Survival Horror tipus Outlast | Vertical slice tècnica funcional | El bucle enemic–detecció–persecució–amagatall–escapada ja funciona; falta profunditat, dificultats, feedback final i una durada comercial validada. |
| PC | Funcional | Moviment, interacció, UI i controls principals disponibles. |
| VR | Base funcional | Rig XRI complet, OpenXR, locomoció configurable, mans, interactors, simulador i escena de prova disponibles; queda QA amb visors reals i paritat completa d'UI/física. |
| Preparació comercial | Parcial | Bona documentació i validació manual, però sense tests reals, localització, inventari de llicències ni separació per assemblies. |

## 2. Evidència recollida durant l'auditoria

### Mida i estructura

- 384 fitxers dins de `_EscapeRoomTemplate`.
- 93 scripts C# i aproximadament 11.028 línies de codi.
- 47 scripts de sistemes, 30 de Core, 8 de Player i 8 d'UI.
- 5 escenes de build: `MainMenu`, `ShowcaseMuseum`, `LockedOffice`, `SurvivalHorrorDemo` i `VRTemplate`.
- Prefabs específics de PC i VR, més els prefabs modulars de Survival Horror i sistemes compartits.
- No hi ha cap `asmdef` propi; tot el runtime acaba principalment a `Assembly-CSharp`.

### Escenes verificades dins Unity

| Escena | GameObjects | Components | Missing scripts | Canvas | UIDocument | Saveables | IDs buits/duplicats |
|---|---:|---:|---:|---:|---:|---:|---:|
| MainMenu | 3 | 7 | 0 | 0 | 1 | — | — |
| ShowcaseMuseum | 254 | 929 | 0 | 0 | 2 | 57 | 0 / 0 |
| LockedOffice | 93 | 333 | 0 | 0 | 2 | 32 | 0 / 0 |

Build Settings verificat:

1. `MainMenu.unity`
2. `ShowcaseMuseum.unity`
3. `LockedOffice.unity`

La Renderer Feature `Focused Interaction Outline` està instal·lada i activa al renderer URP.

### Prefabs verificats

- `GameManager.prefab`: sense scripts perduts; conté Bootstrap, inventari i els dos documents UI Toolkit.
- `Player_PC.prefab`: CharacterController, moviment, càmera, InteractionManager i equipament.
- `Flashlight_Modular.prefab`: lògica separada del `ModelSocket`, llum, collider i model substituïble.
- `Player_VR.prefab`: derivat dels Starter Assets oficials d'XRI; inclou head tracking, controladors esquerra/dreta, poke, near/far i teleport interactors, haptics, CharacterController, locomoció i `ModelSocket` per mà.
- `VRTemplate.unity`: escena mínima amb rig, XR Interaction Manager, XR UI Input Module, terra teleportable, grab, interacció simple i simulador opcional.

### Tests

Unity Test Framework detecta les suites EditMode i PlayMode, però totes dues contenen `0` tests reals. El resultat «Passed» actual només indica que la suite buida no ha fallat; no valida cap mecànica.

## 3. Base funcional verificada

### 3.1 Sistemes compartits pels dos gèneres

- Bootstrap de serveis persistents i flux de canvi d'escena.
- Entrada centralitzada amb Input System i rebinding de teclat.
- Moviment FPS amb caminar, córrer, saltar, ajupir-se i passes segons superfície.
- Head bob de càmera (PC) additiu i reduïble per accessibilitat, sense competir amb el crouch/mouse-look pel transform de la càmera.
- Interacció per raycast amb focus, prompt i outline URP.
- Dispatcher compartit entre interacció PC i ponts VR.
- Portes rotatòries o lliscants, panys, calaixos, interruptors i triggers.
- Notes, documents, subtítols i seqüències narratives.
- Manipulació d'objectes físics, llançament i sockets.
- Inventari amb quantitats, categories, accés ràpid i accions contextuals.
- Combinació d'objectes amb resultat i consum configurable.
- Examen 3D amb rotació i zoom.
- Equipament i models substituïbles mitjançant `ModelSocket`.
- Puzles de codi, seqüència, estat i socket.
- `PuzzleDefinition`, pistes progressives i penalització opcional.
- Objectius amb prerequisits i esdeveniments de final de partida.
- Menú principal, pausa, resultats, crèdits, Save/Load i ajustos amb UI Toolkit.
- Personalització visual del menú sense tocar codi ni USS: `MenuThemeSettings` (asset `ScriptableObject`) amb color de fons, accent, títol, botons, dues fonts i un logo, assignable des de l'Inspector al camp `_theme` d'`UIToolkitMenuController`. Sense assignar-lo (per defecte) el menú es renderitza exactament com el `EscapeRoomMenu.uss` autoria't. `EscapeRoomMenu.uss` també incorpora variables (`--color-accent`, `--color-text`...) per als colors realment repetits, per si algú prefereix editar el fitxer a mà. Pendent (fora d'abast d'aquesta passada): recolorir etiquetes individuals (metadades de ranura, etiquetes de reassignació) que encara defineixen el seu propi color via USS i no hereten del tema.
- Mode d'alt contrast (`ER-008b`) al menú: toggle a Ajustes (`GameSettingsData.highContrastMode`) que força una paleta negre/blanc/groc fixa a totes les pantalles, sempre per sobre de qualsevol `MenuThemeSettings` assignat (l'accessibilitat guanya a la marca). Nota d'autoria: si mai s'afegeix un puzle cronometrat, el seu temps haurà de ser ampliable — cap puzle actual ho és, així que no aplica encara.
- Tres ranures manuals, quick save/load, metadades, captures i persistència d'entitats destruïdes.
- Escriptura de partida atòmica (`File.Replace`) amb còpia de seguretat `.bak` recuperable automàticament si el fitxer principal es corromp.
- Perfil central `EscapeRoom`, `SurvivalHorror` o `CustomHybrid`.

### 3.2 Escape Room

El perfil Escape Room és el més complet actualment. Permet construir un recorregut jugable amb exploració, recollida de pistes, combinació d'objectes, panys, puzles, pistes progressives i final de sala. Les dues escenes jugables no tenen Canvas ni scripts perduts i els IDs persistents són únics.

A més dels quatre tipus de puzle existents (codi, seqüència, estat i socket), ara hi ha `MultiStagePuzzle`: puzles per fases ordenades o ramificades (`AdvanceStage`/`AdvanceToStage(id)`), amb feedback `UnityEvent` propi d'entrada/sortida per fase, rollback opcional a la fase anterior i Save/Load des de qualsevol fase.

L'examen 3D (`GameplayUIController`) ara admet `ExamineHotspot`: punts clicables sobre el model examinat que canvien el text de descripció en fer-hi hover (prompt no revelat) i en clicar-hi (descripció revelada), poden concedir un item d'inventari i persisteixen entre partides via `ExamineHotspotRegistry`. Funciona igual a PC i VR perquè reutilitza els mateixos events de punter de UI Toolkit que ja s'usaven per rotar/fer zoom.

Sisè tipus de puzle: `SlidingPuzzle` (menú `Create > Puzzles > Sliding Puzzle`), un 15-puzzle clàssic — graella amb un forat, `TryMoveTile(tileId)` només mou la peça adjacent al forat. Comença sempre pre-barrejat amb moviments legals aleatoris des de la posició resolta (garanteix que sempre és resoluble, a diferència d'una permutació purament aleatòria), seedejat igual que la resta de variants i amb Save/Load de l'estat actual complet (no només si està resolt).

Cinquè tipus de puzle: `WirePuzzle`, connectar cables a sockets (`Connect(wireId, socketId)`/`Disconnect(wireId)`), amb exclusivitat de socket (endollar-hi un cable en desendolla qualsevol altre que hi hagués), verificació automàtica en omplir totes les connexions i Save/Load de l'estat de cablejat. Els kits de dials/safe i símbols ja estaven coberts per `CodePanelPuzzle` (accepta qualsevol token de text, no només xifres).

Setè tipus de puzle (`ER-002c`): `PipePuzzle` (menú `Create > Puzzles > Pipe Puzzle`), canonades rotables en una graella — `RotateTile(tileId)` gira una peça 90° i el puzle valida per BFS que hi hagi un camí continu d'obertures coincidents entre la peça font i la peça destí. Data-driven (`PipeTileDefinition` per peça: fila, columna, costats oberts abans de rotar), amb variant seedejada opcional (`_randomizeRotations`, evita generar-se ja resolta) i Save/Load de la rotació de cada peça.

`PuzzleController` ara exposa `ResetPuzzle()`: torna qualsevol puzle (dels set tipus) a `Unsolved` de manera segura, netejant pistes i l'estat transitori propi (codi introduït, seqüència, connexions, fase actual o rotacions) via el nou hook `OnPuzzleReset()`. Útil per a un "torna-ho a provar" sense recarregar l'escena. `CodePanelPuzzle` també guanya un LED que batega mentre no està resolt (no només un color fix vermell/verd), perquè no depengui exclusivament del color per llegir l'estat.

`SaveManager` ara guarda un `RunSeed` estable per partida (es renova a `StartNewGame`, es restaura en carregar). `PuzzleController.ResolveVariantSeed()` el combina amb el `SaveId` de cada puzle perquè cadascun tingui la seva pròpia variant determinista. Quatre dels set tipus ja ho aprofiten amb un toggle opcional (desactivat per defecte, sense canviar res del contingut ja autoria't): `CodePanelPuzzle` (`_randomizeCode`, codi diferent cada partida), `SequencePuzzle` (`_randomizeOrder`, mateixos passos en ordre barrejat), `WirePuzzle` (`_randomizeMapping`, mateixos cables/sockets amb aparellament barrejat) i `PipePuzzle` (`_randomizeRotations`, mateixes peces amb rotació inicial barrejada). Tots desen la variant triada dins l'estat propi del puzle perquè carregar una partida mai la torni a sortejar per sota d'un puzle ja resolt o d'una pista ja llegida.

### 3.3 Survival Horror disponible ara

- Llanterna equipable amb consum i recàrrega mitjançant piles de l'inventari.
- HUD de bateria condicionat al perfil i a l'equipament.
- Cordura/estabilitat persistent amb quatre estats i recuperació configurable.
- Penalització de cordura per errors de puzle.
- Esdeveniments de terror d'un sol ús o amb cooldown.
- Activació d'esdeveniments per entrada a zona, llindar de cordura o crida manual.
- Hooks mitjançant `UnityEvent`, so i subtítols.
- Soroll de gameplay per passes, sprint, portes, accions i impactes físics, amb gizmos de depuració.
- Percepció enemiga amb FOV, distància, oclusió, visibilitat contextual, memòria i investigació.
- Director de persecució amb events d'inici/final, període de gràcia i zones segures.
- Director de tensió opcional que limita la freqüència global d'esdeveniments de terror (cooldown entre qualsevol parell d'esdeveniments, pressupost per finestra mòbil i zones segures per checkpoint/zona de persecució), sense substituir l'autor de nivell ni el cooldown propi de cada esdeveniment.
- Camera shake additiu basat en trauma i hàptics de tensió lligats a cordura crítica i entrada en persecució; el shake es desactiva automàticament en VR per evitar mareig.
- Accessibilitat horror ampliada: reducció real de sorolls forts (esdeveniments i tells d'IA) i assistència en persecucions (enemic més lent i amb menys memòria) ja aplicades i disponibles al menú; reducció de tremolor de càmera també aplicada. Reducció de gore queda exposada com a toggle/hook per a quan hi hagi contingut de gore.
- Amagatalls inspeccionables per la IA i anchors separats d'entrada, sortida i inspecció. Tres prefabs d'exemple (`HidingLocker_Modular`, `HidingBed_Modular`, `HidingContainer_Modular`) i una eina d'autoria (`Escape Room Framework/Create/Survival/Hiding Spot`) que genera el mateix esquelet (col·lisió trigger, `NavMeshObstacle`, anchors, `ModelSocket` reemplaçable) per a qualsevol dels quatre tipus (`Locker`, `UnderBed`, `Container`, `Custom`). Mentre s'està amagat, un vinyeta de pantalla (`HidingViewFeedback`, creat automàticament pel Bootstrapper) tanca la visió i s'intensifica amb la respiració, sense tocar el FOV real de la càmera (segur en VR); usa una prioritat de `Volume` més alta que la de cordura perquè es superposi mentre s'està amagat i deixi veure el vinyeta de cordura en sortir, en lloc de forçar el paràmetre compartit a zero permanentment.
- Portes operables per la IA segons perfil, amb bloqueig NavMesh alliberat en obrir-se.
- Portes amb obertura per fases (peek), mode curós i slam, cadascun amb soroll i durada diferenciats, compatible amb la interacció d'IA existent.
- Checkpoints amb snapshot independent de les ranures manuals i restauració de l'estat `ISaveable`.
- Dany tipificat, fonts ambientals reutilitzables, finestra d'invulnerabilitat, retard de mort i events per feedback extern.
- Derrota sense checkpoint o respawn fiable amb recuperació definida pel preset actiu.
- Tres dificultats data-driven que modifiquen recursos, stamina, IA, dany, amagatalls, checkpoints i guardat manual.
- Selector de dificultat integrat al menú d'ajustos UI Toolkit.
- Checkpoints que restauren també activació, transform, Rigidbody i IDs destruïts d'entitats de món.
- Traversal compartit PC/VR amb vault, climb, ladder i squeeze bidireccionals, anchors, corbes, gizmos, cancel·lació segura i polítiques de ruta per enemic.
- Recorregut enemic visible sobre obstacles autoritzats, amb restauració segura del `NavMeshAgent`; alternativa per NavMesh o bloqueig configurable per obstacle.
- Confort de traversal VR configurable entre moviment animat i canvi instantani, sense dependència d'un SDK propietari de Meta.
- Càmera modular equipable, independent de la llanterna, amb pujar/baixar, zoom, visió nocturna, bateria específica i controls PC/XR rebindables. L'art final de visió nocturna (`SH-015`) ja està connectat: `NightVisionFeedbackController` construeix un `Volume` URP (tint verd, gra de pel·lícula i vinyeta) que es difumina segons `NightVisionController.StateChanged`, i limita gra/vinyeta quan l'ajust d'accessibilitat "reduir destells" està actiu (mateix patró que `SanityFeedbackController`/`HidingViewFeedback`). Verificat en Play Mode: amb l'ajust desactivat es mantenen els valors configurats (gra 0,7 / vinyeta 0,35); activat, es limiten a 0,25 / 0,2.
- Evidències gravables per temps d'enquadrament, diari persistent i integració directa amb objectius data-driven.
- Evasió opcional amb lean, mirada enrere i slide, col·lisions de càmera, postura segura i controls rebindables; en Quest el lean/look-back és físic i el slide artificial està desactivat per defecte.
- Bridge d'interacció VR que detecta la mà real de l'interactor (esquerra/dreta) a hover/select, en lloc d'assumir sempre la dreta; vinyeta de confort (tunneling) oficial d'XRI connectada al moviment i gir continus.
- Escena `SurvivalHorrorDemo` (build index 3) amb una vertical slice original completa i verificada en Play Mode: cadena de quatre objectius data-driven encadenats per prerequisit (`recover_batteries` → `restore_power` → `record_anomaly` → `escape_facility`, via `ObjectiveManager`/`ObjectiveSet`/`ObjectiveDefinition`), enemic, dues zones d'amagatall, generador d'emergència interactuable, subjecte gravable amb el camcorder, quatre repte de traversal (vault/climb/ladder/squeeze), llaunes llançables com a distracció de soroll, zona de checkpoint doble, `ChaseSafeZone` a la sortida i un `EndingDefinition` de victòria (`DemoEscapeEnding`) que `GameFlowManager` dispara en completar l'últim objectiu. Verificat: `manage_scene validate` sense incidències, cap error/warning en entrar en Play Mode, i cada identificador de disparador (`_itemId`, nom del GameObject, `_evidenceId`) confirmat contra l'objectiu que l'espera. Pendent: cronometrar-la amb un jugador real per confirmar el recorregut de 10–15 minuts (el que no es pot validar sense una persona jugant-hi).

Aquests sistemes ja constitueixen un bucle tècnic complet, però la demo encara necessita profunditat, dificultats, feedback i QA per assolir qualitat comercial.

## 4. Problemes i riscos detectats

### 4.1 Bloquejadors comercials

1. No hi ha tests automatitzats reals.
2. No hi ha inventari de llicències, `ThirdPartyNotices` ni crèdits verificables per a àudios, fonts, icones i altres assets.
3. El README descriu una arquitectura i un roadmap antics, inclou carpetes que no existeixen i diu que els sistemes actuals encara estan pendents.
4. La promesa de VR és superior al que ofereix el prefab actual.
5. No hi ha localització: molts textos d'UI i missatges estan escrits directament en castellà dins del codi.
6. No hi ha migrador per versió del format global de `SaveGameData` (el camp `version` existeix però `LoadGame` no en fa cap ús); és un problema teòric fins que existeixi una segona versió real del format. L'escriptura ja és atòmica amb `File.Replace` i backup recuperable (vegeu `P0-003`).

### 4.2 Ajustos que existeixen però no estan connectats completament

- `reduceGore` es desa i té toggle al menú, però no hi ha cap contingut de gore al projecte que el consulti; és un punt d'integració intencionat (vegeu secció 9), no un bug.

`mouseSensitivity`, `musicVolume`, `sfxVolume`, `subtitles`, `reduceFlashes` i `qualityLevel` ja s'apliquen de veritat (`PlayerMovement`, `AudioManager`, `GameplayUIController` i `QualitySettings` els llegeixen en viu de `GameSettingsService`), tenen sliders/toggles/dropdown al menú, i un `settings.json` corrupte o il·legible ja no trenca l'arrencada (es registra un avís i es carreguen els valors per defecte).

### 4.3 Deute d'arquitectura

- `GameplayUIController` té 886 línies i concentra HUD, inventari, notes, keypad, examinador, subtítols i rig de render.
- `UIToolkitMenuController` té 509 línies i construeix totes les pantalles des de codi.
- `InventoryManager` té 500 línies i combina emmagatzematge, hotbar, ús, combinació, drop, migració i equipament.
- `TemplateSceneBuilder` té 1.103 línies de generació legacy i no forma part del flux comercial actual.
- Hi ha sis scripts legacy a `UI/PC`; cinc són wrappers obsolets i `UIManager` continua com a pont de compatibilitat al GameManager.
- El moviment del jugador depèn simultàniament del `UIManager` legacy i dels controladors UI Toolkit.
- Es fan servir nombrosos singletons i cerques globals. És acceptable per a una plantilla petita, però complica tests, múltiples jugadors i càrrega additiva.
- Sense `asmdef`, els mòduls no es poden compilar, provar o distribuir de manera independent.
- No existeix una capa de localització ni una interfície de serveis injectable per a tests.

### 4.4 Problemes del controlador de jugador

- El delta del ratolí es multiplica per `Time.deltaTime`; això pot fer que la sensibilitat depengui del framerate segons el dispositiu d'entrada.
- L'ajupiment no comprova si hi ha sostre abans de tornar a l'alçada normal.
- No hi ha stamina, cansament, dany per caiguda ni feedback respiratori. Inclinació i mirar enrere ja existeixen (evasió avançada). Head bob configurable ja existeix (`HeadBobController`, PC-only, additiu sobre `PlayerMovement` sense competir pel transform).
- Les passes només reprodueixen àudio: no publiquen un estímul de soroll reutilitzable per a IA.
- El `SaveId` del jugador és fix (`Player`), fet que limita multijugador o rigs simultanis.

### 4.5 Deute VR restant

`VRInteractionBridge` ja funciona amb callbacks XRI de hover/select i no fa polling ni reflexió a `Update`; a més, ara resol la mà real (esquerra/dreta) de l'interactor en cada event en lloc d'assumir sempre la dreta, així que els hàptics i qualsevol lògica de gameplay per mà ja disparen al costat correcte. La locomoció utilitza l'asset d'accions oficial d'XRI, l'entrada de gameplay compartida incorpora botons XR per interacció, pausa, inventari, llanterna i camcorder, i la vinyeta de confort (tunneling) oficial d'XRI ja està connectada al moviment i gir continus per reduir el mareig.

`PhysicsSocket` ja detecta correctament si l'objecte concret que hi ha al trigger encara s'està sostenint (via `PhysicsGrabber` en PC o `VRInteractionBridge.IsSelected` en VR) abans d'encaixar-lo — abans, en VR (on `PhysicsGrabber.Instance` és sempre `null`), un objecte agafat físicament podia ser arrencat de la mà del jugador i encaixat sol en entrar en un trigger de socket.

L'equipament ja es pot posar a la mà esquerra: `InteractionDispatcher` exposa quina mà ha disparat cada interacció (`LastHand`) i `EquipmentController` l'usa per triar entre el seu socket principal i un `_leftEquipmentSocket` opcional (que `Setup > Create or Update VR Player Prefab` ja connecta sol quan hi ha dues mans).

Encara queda separar físicament la capa XR de les escenes exclusivament PC, validar tots els controls de UI Toolkit amb ambdues mans, permetre el dual-grab opcional, provar Save/Load d'objectes agafats/equipats i executar QA en almenys un visor PCVR i un visor standalone.

### 4.6 Robustesa i authoring

- Els validadores actuals comproven assets bàsics, IDs i algunes dependències, però no les referències obligatòries de cada component.
- No hi ha preflight complet de build, migracions generals de saves ni recuperació d'una ranura corrupta.
- Les captures de les ranures es generen de manera asíncrona i la UI pot intentar llegir-les abans que acabin d'escriure's.
- No hi ha validació de grafs d'objectius cíclics, receptes de combinació impossibles o puzles sense sortida.
- La majoria dels inspectors són els genèrics de Unity; falta una experiència d'autor professional guiada.

## 5. Referència de disseny: Outlast

El nucli que convé prendre com a inspiració no és l'estètica ni la propietat intel·lectual, sinó la relació entre vulnerabilitat, informació limitada i mobilitat.

Les fonts oficials descriuen Outlast com una experiència sense capacitat de lluita on el jugador ha de córrer o amagar-se, amb sigil, moviments inspirats en parkour i enemics imprevisibles. El blog oficial de PlayStation, amb explicacions de Red Barrels, també destaca una IA que busca activament el jugador i un mode de dificultat amb poca salut, poques piles i sense checkpoints ni guardat manual.

Patrons aplicables a la plantilla:

1. El jugador no domina l'enemic; sobreviu llegint l'espai.
2. La IA alterna patrulla, sospita, investigació, cerca i persecució.
3. El so i la línia de visió creen informació parcial i decisions de risc.
4. Els amagatalls no són invulnerabilitat automàtica; l'enemic pot inspeccionar-los.
5. Les portes, obstacles i dreceres són eines de fugida.
6. El moviment avançat converteix la persecució en gameplay: vault, climb, squeeze i mirar enrere.
7. Una càmera amb visió nocturna transforma la foscor en gestió de recurs.
8. Gravar evidències pot alimentar el diari i la narrativa sense convertir-se en un shooter.
9. Dificultat i checkpoints poden modificar salut, recursos, agressivitat i tolerància de detecció.

Fonts:

- [Outlast — pàgina oficial de Steam publicada per Red Barrels](https://store.steampowered.com/app/238320/Outlast/)
- [Outlast Will Scare the S*** Out of You on PS4 — PlayStation Blog, Philippe Morin/Red Barrels](https://blog.playstation.com/2013/06/11/outlast-will-scare-the-s-out-of-you-on-ps4/)
- [Outlast Out Today: Survival Horror on PS4 — PlayStation Blog](https://blog.playstation.com/2014/02/04/outlast-out-today-survival-horror-on-ps4/)
- [Camcorder — referència mecànica secundària d'Outlast Wiki](https://outlastwikia.fandom.com/wiki/Camcorder)

No s'han de copiar noms, interfícies, assets, personatges, sons ni presentació visual d'Outlast. La implementació ha de ser genèrica, configurable i comercialment original.

## 6. Arquitectura objectiu dels perfils

El sistema actual de flags és adequat per a tres mòduls, però quedarà curt quan s'afegeixin IA, amagatalls, persecució, stamina, càmera nocturna i moviment avançat.

Objectiu recomanat:

- Mantenir `GenreFeatureSettings` com a punt d'entrada per al dissenyador.
- Afegir packs de funcionalitat amb dependències validades.
- Permetre activar un pack complet o característiques individuals en `CustomHybrid`.
- Afegir un `FeatureGate` per activar/desactivar arrels d'escena, prefabs i UI.
- Fer que cada mòdul exposi una interfície petita i no depengui d'una escena concreta.

Matriu objectiu:

| Mòdul | Escape Room | Survival Horror | Custom Hybrid |
|---|---:|---:|---:|
| Interacció, inventari, examen, puzles, Save/Load i finals | Sí | Sí | Configurable però recomanat |
| Pistes progressives | Sí | Opcional | Opcional |
| Llanterna | Opcional | Opcional | Opcional |
| Cordura | No per defecte | Opcional | Opcional |
| Camcorder i visió nocturna | No | Sí en preset Outlast-like | Opcional |
| Stamina i vitals | No | Sí | Opcional |
| IA hostil i percepció | No | Sí | Opcional |
| Amagatalls i persecucions | No | Sí | Opcional |
| Moviment avançat | Opcional | Sí | Opcional |
| Gore o efectes intensos | No | Opcional | Opcional |

## 7. Roadmap pendent

### P0 — Bloquejadors abans de publicar l'asset

| ID | Àrea | Implementació pendent | Criteri d'acceptació |
|---|---|---|---|
| P0-001 | QA | Fase 1 feta: `EscapeRoomRevolt.Core.asmdef` separat (Save, Settings, Flow, Input, Localització, EventBus/GameContext) amb `EscapeRoomRevolt.Core.Tests.asmdef` referenciant-lo — la suite ja no és 0: 12 tests EditMode reals (`EventBus`, `LocalizationCatalog`) passant al Test Runner. Fase 2a feta: moguda l'abstracció player-platform (`IPlayerPlatformAdapter`, `PlayerHand`, `PlayerPlatformRegistry`) a `Core/Runtime/Player/`, resolent ~9 de les ~15 citacions `Systems`→`Player`. Fase 2b feta: les ~12 citacions `Systems`/`Player`→`UI` (`UIManager.Instance`/`UIToolkitMenuController.Instance`) desacoblades amb `EventBus`. Dos events d'estat nous (`OnGameplayUIBlockingChanged`, `OnMenuUIBlockingChanged`) publicats per `GameplayUIController`/`UIToolkitMenuController` en cada canvi real del seu modal/pantalla, cachejats per un nou `EscapeRoomRevolt.Core.GameplayBlockState` (resubscrit per `GameContext` just després de cada `EventBus.Clear()`, ja que és un subscriptor estàtic sense `OnEnable` de MonoBehaviour on penjar-se) que `PlayerMovement`, `PlayerInputHandler`, `VRUIPanelColliderController`, `InteractionManager`, `PhysicsGrabber`, `EquipmentController`, `CamcorderEvidenceRecorder`, `NightVisionController` i `EvasionController` llegeixen en lloc de consultar els singletons de UI. Set events de petició nous (`RequestShowSubtitle/Hide`, `RequestShowNoteReader`, `RequestShowKeypad`, `RequestToggleInventory`, `RequestCloseTopPanel`, `RequestTogglePause`) publicats per `HintManager`, `HorrorEventTrigger`, `NarrativeTrigger`, `InteractableNote`, `InteractableKeypad`, `PlayerInputHandler`, i consumits per `GameplayUIController`/`UIToolkitMenuController`, que ara mai reben una crida directa des de `Systems`/`Player`. El crosshair (abans `InteractionManager` empenyent `UIManager.SetCrosshair`) ara el llegeix `GameplayUIController` cada frame des de `InteractionManager.Instance.CurrentTarget` (patró pull, ja existent per al text del prompt d'interacció). Verificat: compila net (0 errors/warnings), 12/12 tests EditMode, i Play Mode complet amb `execute_code` confirmant en viu que cada event de petició obre/tanca el modal correcte i que `GameplayBlockState` reflecteix el modal de joc i la pantalla de menú de manera independent. Fase 3 feta: `IPlayerPlatformAdapter` ampliat amb `SetMovementBlocked`/`SetLookBlocked`/`TeleportTo` (implementats a `PCPlayerPlatformAdapter`/`VRPlayerPlatformAdapter`, aquest últim delegant a `VRComfortController`/`TeleportRig` ja existents). `PlayerVitals`, `CheckpointManager`, `PhysicsGrabber` i `HorrorEventTrigger` ja no toquen cap tipus concret de `Player.PC`/`Player.VR` (aquest últim substituint `GetComponentInParent<PlayerMovement>()` per `GetComponentInParent<IPlayerPlatformAdapter>()`, vàlid també per VR). `TraversalController` i `HidingSpot` ja usen la interfície per bloqueig de moviment/mirada i teleport, però mantenen deliberadament una dependència directa i acceptada: `TraversalController` cap a `VRComfortController.Settings`/`TeleportRig` (semàntica de moviment continu multi-frame durant el travessament, no un teleport puntual — unificar-ho hauria reactivat el `CharacterController` cada frame i hauria trencat el travessament) i `HidingSpot` cap a `PlayerMovement.SetForcedCrouch` (sense equivalent VR, és opcional i afecta només l'autoria PC). `EvasionController` (lean/look-back via `CameraPitch`/`ViewTransform`, sense equivalent VR real) i `PlayerVisibility` (`IsCrouching`/`IsSprinting`, VR no exposa cap senyal equivalent) queden com a dependència `Player.PC`/`Player.VR` explícitament acceptada — forçar-hi l'abstracció només hauria afegit mètodes no-op sense cap benefici real. `PhysicsSocket` manté `VRInteractionBridge` per detectar si un objecte concret encara s'sosté en VR (estat per-objecte, no per-jugador; no encaixa a `IPlayerPlatformAdapter`). Verificat: compila net, 12/12 tests EditMode, `manage_scene validate` net a `ShowcaseMuseum` i `SurvivalHorrorDemo`, i Play Mode a `SurvivalHorrorDemo` amb `execute_code` confirmant en viu que `SetMovementBlocked`/`SetLookBlocked`/`TeleportTo` funcionen a través de la interfície i que `HidingSpot.Enter/ExitImmediately` i `CheckpointManager.TryRespawn` desbloquegen i teleporten el jugador correctament. **Correcció important a l'auditoria de fase 1**: només comptava citacions `Systems`→`Player`/`UI`; en revisar la direcció inversa (`Player`→`Systems`/`UI`) hi ha una dependència circular real que cap `asmdef` de dues bandes pot resoldre tal qual. Fase 4 feta: `IInteractable`, `CursorType`, `InteractableUtility` i `InteractionDispatcher` (`Systems/Interaction/`) eren igual de autocontinguts que `IPlayerPlatformAdapter` — només depenien de `Core`/`Player`, mai d'un altre tipus de `Systems` — així que s'han mogut a `Core/Runtime/Interaction/` mantenint el namespace `EscapeRoomRevolt.Systems.Interaction` (idèntic patró que la fase 2a: moviment pur, zero canvis de codi a cap altre fitxer, GUIDs preservats amb `git mv`). Això resol per complet la dependència `Player`→`Systems` de `VRInteractionBridge`. Fase 5 feta — les 3 dependències `Player`→`Systems`/`UI` que quedaven, cadascuna amb la solució que millor encaixava (no totes via `EventBus`, malgrat la intenció inicial): `AudioManager`/`SurfaceAudioData` (`Systems/Audio/`) i `GameplayNoise`/`GameplayNoiseType` (`Systems/Survival/GameplayNoise.cs`) eren igual d'autocontinguts que `IInteractable` — només depenien de `Core.Settings` — així que s'han mogut a `Core/Runtime/Audio/` i `Core/Runtime/GameplayNoise.cs` mantenint el namespace (mateix patró de moviment pur que les fases 2a/4); això resol `PlayerMovement`→so de passes/soroll de gameplay sense tocar cap altre fitxer dels 12 que usen `GameplayNoise`. `PlayerVitals.CanSprint` (exhaustion/mort) i l'informe invers `PlayerMovement`→`PlayerVitals.SetSprinting` (estat de sprint per al drenatge d'estamina) sí que són genuí estat/comandament bidireccional entre dos MonoBehaviours amb cicle de vida propi, encaix natural per `EventBus`: nous `OnPlayerCanSprintChanged` (publicat per `PlayerVitals` a `SetExhausted`/en morir, cachejat per `PlayerMovement` igual que `GameplayBlockState`) i `RequestSetSprinting` (publicat per `PlayerMovement`, subscrit per `PlayerVitals`). `PlayerInputHandler`→`InventoryManager` (accés ràpid) també és un comandament net, resolt amb `RequestSetActiveQuickSlot`/`RequestNavigateQuickAccess`. `VRUIToolkitPresenter`→`UIToolkitMenuController`, en canvi, és una classificació estructural d'un `UIDocument` trobat via `FindObjectsByType` (no un esdeveniment ni un estat continu), així que forçar-hi `EventBus` hauria complicat el flux sense guanyar res: s'ha resolt amb un marcador buit `EscapeRoomRevolt.Core.IMenuPanel` que `UIToolkitMenuController` implementa, i `VRUIToolkitPresenter` el detecta amb `GetComponent<IMenuPanel>()` en lloc del tipus concret. De pas, s'ha trobat i corregit un `GameplayUIController.Instance?.ShowNote(...)` a `InventoryManager` que la fase 2b havia deixat passar (la cerca original només buscava `UIManager`/`UIToolkitMenuController`, no `GameplayUIController` directe) — ara publica `RequestShowNoteReader` igual que la resta. Verificat: compila net, 12/12 tests EditMode, `manage_scene validate` net, i Play Mode a `SurvivalHorrorDemo` amb `execute_code` confirmant en viu (via reflexió sobre camps privats per no dependir de input real) que `PlayerVitals.SetExhausted` propaga `OnPlayerCanSprintChanged` fins al camp cachejat de `PlayerMovement`, que `RequestSetSprinting`/`RequestSetActiveQuickSlot`/`RequestNavigateQuickAccess` arriben als seus gestors, i que `UIToolkitMenuController is IMenuPanel`. Amb això, `Player` ja no té cap dependència real cap a `Systems`/`UI` (namespaces com `Systems.Interaction`/`Systems.Audio`/`Systems.Survival` es mantenen per compatibilitat però els tipus que `Player` en necessita ara viuen físicament a `Core`). Les ~2 referències `UI`→`UI` (`GameplayUIController`↔`UIToolkitMenuController`) no són una violació de capa. Fase 6 feta — `ARC-001` tancat: creats `EscapeRoomRevolt.Player.asmdef`, `EscapeRoomRevolt.Systems.asmdef` i `EscapeRoomRevolt.UI.asmdef` (detall complet a `ARC-001` més avall). El compilador de Unity confirma el que l'auditoria manual ja apuntava: zero cicles amagats. `Systems`/`Player`/`UI` ja no són `Assembly-CSharp` — només `Core/Editor` i la resta d'scripts d'Editor hi continuen (fora d'abast). | Mínim 20 EditMode i 10 PlayMode cobrint inventari, combinació, puzles, IDs, perfils, Save/Load, flux de menú i UI crítica; Test Runner amb tests reals i zero errors. El bloquejador estructural ja no hi és (`Systems`/`Player`/`UI` tenen `asmdef` propi, un test ara SÍ pot referenciar-los); escriure els tests reals en si segueix pendent — la suite continua a 12 EditMode / 0 PlayMode. |
| P0-003b | Save/Load | Crear un migrador real per versió de `SaveGameData` quan aparegui una segona versió del format global (escriptura atòmica, backup i detecció de corrupció ja fets). | Un save antic es carrega igual després de canviar el format; test de migració entre versions. |
| P0-005 | Llicències | `ThirdPartyNotices.md` creat amb l'inventari complet d'assets, materials, fonts, packages i models. Pendent: confirmar amb el propietari l'origen/llicència real dels 5 àudios de `Assets/_EscapeRoomTemplate/Audio` (marcats `PENDIENTE`); treure `com.coplaydev.unity-mcp` de `manifest.json` abans d'exportar el paquete comercial. | Cada font, icona, àudio, textura, model i package té origen, autor, llicència i permís de redistribució; qualsevol asset dubtós queda substituït. |
| P0-006 | Documentació | README arrel reescrit i `PROGRAMMING_GUIDE.md` posat al dia (puzles nous, amagatalls, temes de menú). `estat_projecte.md` i `COMMERCIAL_READINESS.md` deixen de duplicar l'estat i apunten a aquest roadmap. Pendent: auditar `DOCUMENTACIO_COMPLETA.md` (1926 línies) i `UserManual.md`, que encara no s'han revisat contra l'estat actual. | README, UserManual, Programming Guide, documentació completa i roadmap descriuen la mateixa arquitectura, requisits i estat real. |
| P0-007 | Localització | Infraestructura feta i verificada (`LocalizationCatalog`, `LocalizationService`, selector d'idioma a Ajustes, `DefaultLocalizationCatalog.asset` amb 16 entrades ES/EN). Convertit `UIToolkitMenuController.Show()` (tots els títols de pantalla) i els botons de `ShowMain`/`ShowPause`. Pendent: la resta de botons/etiquetes del menú, tot `GameplayUIController` (HUD, inventari, notes, keypad), i els prompts/errors definits directament a C# arreu del codi (`HidingSpot._enterPrompt`, missatges de `SaveManager`, etc.). Per convenció d'aquesta passada, la clau del catàleg és el text castellà original — una migració completa podria introduir claus estructurades. | Taules o catàleg localitzable, idioma de fallback i castellà/anglès de mostra; UXML, menús, prompts, objectius, pistes i errors no depenen de literals C#. |
| P0-008 | Validació | `CommercialReadinessValidator` ara detecta cicles de prerequisits entre `ObjectiveDefinition` (DFS, verificat creant un cicle de prova real), receptes de combinació trencades (`CombineWith`/`ResultItem` nuls) i el contracte UXML del menú (clona `EscapeRoomMenu.uxml` i comprova que existeixin els elements `title`/`screen-content` que `UIToolkitMenuController` consulta per nom). Ja detectava referències null, IDs duplicats, escenes de build i existència de perfil. Pendent: comprovar `GameplayHUD.uxml`, completesa del rig XR i assets sense llicència registrada (aquest últim depèn de `ThirdPartyNotices.md`, `P0-005`). | Detecta referències obligatòries null, IDs, cicles d'objectius, receptes trencades, scenes de build, UXML contract, perfils, XR incomplet i assets sense llicència registrada. |

### P2 — QA de traversal en hardware

| ID | Sistema | Implementació pendent | Criteri d'acceptació |
|---|---|---|---|
| SH-016 | Traversal | Tancar QA de traversal en Meta Quest. | Les quatre variants, els dos modes de confort i la cancel·lació es verifiquen en visor; sense mareig greu, clipping ni pèrdua de tracking origin. |

### P2 — Profunditat específica d'Escape Room

| ID | Sistema | Implementació pendent | Criteri d'acceptació |
|---|---|---|---|
| ER-001 | Graf de puzles | Crear una vista d'autor de dependències i estat. | Mostra prerequisits, sortides, cicles, bloquejos i objectius; valida que una sala tingui almenys una ruta de solució. |
| ER-004 | Quadern | Crear un casebook amb notes, evidències, objectius i pistes. | Cerca/filtre, novetats, estat persistent i integració amb documents i evidències gravades. |
| ER-007 | Multi-room | Fet: `RoomPortal` (interactuable opcionalment bloquejat per un objecte, com `Door`) envia el jugador a un `RoomSpawnPoint` amb id concret en una altra escena; `RoomLoadMode.Single` (per defecte) preserva inventari, objectius i estat de món via una nova `SaveManager.CaptureSnapshot()`/`RestoreSnapshot()` en memòria (extreta de la lògica ja existent de `SaveGame`/`LoadGame`, sense tocar disc), `RoomLoadMode.Additive` carrega l'escena en paral·lel sense descarregar l'actual (backtracking gratuït, sense cau per sala). `ObjectiveSet` guanya `NextRoomScene`/`NextRoomSpawnId` opcionals (buit = comportament actual, acaba el joc); si s'omplen, `ObjectiveManager.TryFinishRoom()` transiciona de sala en lloc de completar la partida. Cap escena existent (`LockedOffice`, `ShowcaseMuseum`) canvia de comportament perquè els camps nous per defecte són buits. Verificat: compila net, 12/12 tests, `manage_scene validate` net a totes dues escenes, i en viu (Play Mode, `execute_code`) `CaptureSnapshot`/`RestoreSnapshot` recuperant un ítem eliminat, una transició real `ShowcaseMuseum`→`LockedOffice` amb l'inventari intacte a l'altra banda, i `PositionPlayerAtSpawn` col·locant el jugador exactament a la posició/rotació d'un `RoomSpawnPoint` de prova. Pendent: backtracking amb `Single` no preserva l'estat de la sala anterior en tornar-hi (es recarrega tal com estava autoritzada, no tal com el jugador la va deixar) — `Additive` és l'alternativa documentada per qui el necessiti amb poques sales; una cau de snapshots per sala seria la solució completa i queda com a treball futur. `ER-001` (vista d'autor del graf) segueix pendent — avui `_targetScene`/`_targetSpawnId` s'escriuen a mà, igual que la resta d'IDs creuats del projecte. |

### P2 — VR funcional i publicable

| ID | Sistema | Implementació pendent | Criteri d'acceptació |
|---|---|---|---|
| VR-004 | UI Toolkit | Fet: `PanelRenderMode.WorldSpace` resultava tenir el picking trencat en aquesta versió d'Unity (confirmat provant `IPanel.Pick`/`SendEvent` directament contra el panell real de `GameplayUIController` — funcionava en mode natiu `ScreenSpaceOverlay` i deixava de funcionar en canviar a `WorldSpace`, independentment del codi de dispatch). `VRUIToolkitPresenter` ja no usa `WorldSpace`: deixa cada `PanelSettings` en el seu mode natiu i el renderitza a una `RenderTexture` mostrada sobre un `Quad` amb `MeshCollider` (necessari per `RaycastHit.textureCoord`) posicionat on abans hi havia el panell. `VRUIPointerBridge` (nou, `Player/VR/`) seguix usant `XRSimpleInteractable.hoverEntered/selectEntered` (el mateix mecanisme ja provat de `VRInteractionBridge`) per saber quan un controlador apunta al quad, però calcula la posició fent el seu propi `Collider.Raycast` contra el `MeshCollider` en lloc de confiar en el picking d'Unity — `textureCoord` escalat per la mida *viva* de `rootVisualElement.layout` (no la resolució de la textura, que no hi coincideix necessàriament). `VRUIPanelColliderController` generalitzat de `BoxCollider` a `Collider` (mateixa lògica de gating per `GameplayBlockState`, sense canvis de comportament). Verificat en viu (Play Mode) contra el panell real de `GameplayUIController` amb l'inventari obert: el raig contra el quad, la conversió a coordenades de panell, i el `SendEvent` despatxat van resoldre tots tres al mateix element real (`Image "icon"`), confirmant la cadena completa. Pendent: l'entrada real d'un controlador (físic o simulador XR) disparant `hoverEntered`/`selectEntered` no s'ha pogut provar sense maquinari/simulador — és el mateix mecanisme ja provat de `VRInteractionBridge`, només apuntant a un altre collider, així que no hi ha incertesa nova, però cal confirmar-ho jugant-hi. | Punter XR i clic verificats (fet, prova directa contra el panell real). Pendent verificar amb el simulador XRI o maquinari real: focus, scroll, teclat numèric i inventari amb ambdues mans simultànies. |
| VR-005c | Física | Permetre agafar un mateix objecte amb dues mans (dual-grab) i revisar Save/Load d'objectes equipats/agafats per duplicats (equipar per mà i paritat de socket ja fets). | Dual-grab opcional i test de Save/Load sense estats duplicats. |
| VR-006 | Feature gating | Separar la capa XR de les escenes PC. | En PC no s'executa cap bridge ni interactable XR; en VR s'activen sense duplicar la lògica de gameplay. |
| VR-007 | QA hardware | Crear matriu de dispositius i proves. | OpenXR validat almenys en un visor PCVR i un standalone objectiu; locomoció, UI, haptics, guardat i rendiment documentats. |

### P3 — Arquitectura, mantenibilitat i experiència de comprador

| ID | Àrea | Implementació pendent | Criteri d'acceptació |
|---|---|---|---|
| ARC-001 | Assemblies | `Core` ja separat (vegeu `P0-001`). Les citacions `Systems`/`Player`→`UI` ja estan desacoblades amb `EventBus` (fase 2b, més una que se li va escapar i s'ha corregit a la fase 5) i la majoria de citacions `Systems`→tipus concrets de `Player` ja passen per `IPlayerPlatformAdapter` (fase 3). `IInteractable`/`InteractionDispatcher` (fase 4) i `AudioManager`/`SurfaceAudioData`/`GameplayNoise` (fase 5) ja són a `Core`, resolent totes les dependències reals `Player`→`Systems`/`UI` detectades (`VRInteractionBridge`, `PlayerMovement`, `PlayerInputHandler`, `VRUIToolkitPresenter`) — via moviment a `Core` quan el tipus era autocontingut, via `EventBus` invertit quan era estat/comandament genuí, i via un marcador `IMenuPanel` quan era una classificació estructural puntual, no un esdeveniment. Queden 4 dependències `Player.PC`/`Player.VR` explícitament acceptades dins `Systems` (`TraversalController`, `HidingSpot`, `EvasionController`, `PlayerVisibility`) i `PhysicsSocket`→`VRInteractionBridge` (estat per-objecte, no per-jugador) — totes documentades a `P0-001`, cap bloqueja un `asmdef` (la direcció que hi queda és `Systems`→`Player`, vàlida). **Fet**: creats `EscapeRoomRevolt.Player.asmdef`, `EscapeRoomRevolt.Systems.asmdef` i `EscapeRoomRevolt.UI.asmdef` (cadena unidireccional `Core`←`Player`←`Systems`←`UI`, cadascun referenciant només els que té a sota). El compilador de Unity ha confirmat el que l'auditoria manual ja apuntava: cap cicle amagat, compilació neta a la primera passada un cop resolts els paquets que faltaven (`Unity.XR.Interaction.Toolkit`, `Unity.XR.CoreUtils` a `Player`; els mateixos més `Unity.RenderPipelines.Universal.Runtime`, `Unity.RenderPipelines.Core.Runtime` i `Unity.TextMeshPro` a `Systems`, per `SelectionMaskOutlineFeature`/`Volume`/`Vignette`/`TMPro`; `Unity.InputSystem` a `UI` pels rebinds d'Ajustos). Verificat: compila net, 12/12 tests EditMode, `manage_scene validate` net a `LockedOffice` i `SurvivalHorrorDemo`, i Play Mode complet a `SurvivalHorrorDemo` confirmant que `InventoryManager`(Systems)/`GameplayUIController`(UI)/`PlayerMovement`(Player)/`GameFlowManager`(Core) es troben i interoperen correctament entre les quatre assemblies noves, incloent un cicle `RequestToggleInventory`→`RequestCloseTopPanel` complet via `EventBus`. Pendent: `Core/Editor/*.cs` i la resta d'scripts d'Editor continuen dins `Assembly-CSharp-Editor` (fora d'abast d'aquesta passada); separar-los en un `EscapeRoomRevolt.Editor.asmdef` seria la següent peça, però no bloqueja res del que ja funciona. `VR-006` (separar la capa XR de les escenes PC) segueix pendent per separat. | Dependències unidireccionals, XR opcional i temps de recompilació reduït; tests poden referenciar runtime sense internals accidentals. **Fet**: 4 `asmdef` en cadena unidireccional, confirmat pel compilador. |
| ARC-002 | UI | Dividir els dos controladors UI Toolkit grans. | Presenters per HUD, inventari, examen, documents, keypad, menú i saves; callbacks registrats/desregistrats simètricament. |
| ARC-003 | Legacy | Fet en part: eliminats els sis scripts legacy de `UI/PC` (`UIManager`, `InteractionPromptUI`, `InventoryUI`, `ItemExaminerUI`, `KeypadUI`, `NoteReaderUI`) — cap tenia ja cap cita real des de codi de gameplay (confirmat abans d'esborrar), els cinc wrappers ja portaven `[Obsolete(...)]` de l'autor original, i `UIManager` era l'únic amb un component real al `GameManager.prefab` (retirat via `manage_prefabs` abans d'esborrar el fitxer, sense deixar cap "missing script"). Verificat: compila net, 12/12 tests, `manage_scene validate` net, i Play Mode confirmant que `GameplayUIController` (l'únic propietari real de la UI des de fa diverses fases) segueix obrint/tancant modals amb normalitat sense `UIManager` enmig. Pendent: `TemplateSceneBuilder.cs` (1103 línies, sense cap `[MenuItem]` — inabastable des de cap menú avui) encara no s'ha eliminat ni mogut a Samples/Legacy; només se li ha tret un `using` que apuntava a l'`UI/PC` ja eliminat perquè bloquejava la compilació. | Cap script runtime depèn de `UI/PC` legacy (fet). Pendent: `TemplateSceneBuilder` eliminat o traslladat a Samples/Legacy. |
| ARC-004 | Serveis | Reduir singletons i cerques globals als límits del framework. | Context o registry injectable, lifecycle explícit, càrrega additiva segura i tests sense escenes completes. |
| ARC-005 | Inventari | Separar storage, quick access, recipes, use/drop i equipment. | APIs petites, tests per mòdul, migració de saves i mateixa UX actual sense regressions. |
| ARC-006 | IDs | Crear IDs persistents editor-only i immutables. | GUID generat una vegada, duplicats bloquejats abans de build i migracions documentades quan canvia un ID publicat. |
| ARC-007 | Authoring | Crear inspectors i wizards professionals. | Previews, help boxes, validació inline, botons de prova i creació segura per puzles, items, esdeveniments, IA i perfils. |
| ARC-008 | Rendiment | Definir pressupostos i escenes de benchmark. | CPU, GC, draw calls i memòria mesurats en PC i VR; zero allocations sostingudes als loops principals. |
| ARC-009 | CI | Afegir validació automàtica del repositori. | Compilació, tests, IDs, meta files, YAML, documentació i llicències verificats en cada canvi. |
| ARC-010 | Samples | Separar codi distribuïble i contingut de mostra. | Runtime/Editor nets, samples importables, dependències opcionals clares i export de package repetible. |

## 8. Ordre recomanat d'implementació

1. `P0-001`, `P0-003b`, `P0-005` a `P0-008` (aquests dos últims amb progrés parcial real): impedir que el deute creixi mentre s'afegeixen mecàniques (`P0-002` i `P0-004` ja fets).
2. `SH-016`: tancar QA de hardware (`SH-015`, presentació de visió nocturna, i `SH-020b`, head bob, ja fets).
3. `ER-001`, `ER-004` i `ER-007`: ampliar varietat i qualitat d'autor per Escape Room (`ER-002c`, `ER-003`, `ER-005`, `ER-006` i `ER-008b` ja fets).
4. `VR-004`, `VR-005c`, `VR-006` i `VR-007`: completar dual-grab, feature gating i QA de hardware sobre el rig funcional (mà, vinyeta, hàptics, paritat de socket i equipament per mà ja resolts).
5. `ARC-001` a `ARC-010`: es poden intercalar, però han d'estar resolts abans de publicar la versió comercial final.

`SH-018` (portes per fases), `SH-019` (director de tensió), `SH-006` (amagatalls genèrics), `SH-012` (vertical slice) i `SH-015` (presentació de visió nocturna) ja estan fets i verificats tècnicament; només queda que el propietari cronometri `SH-012` amb un jugador real per confirmar que el recorregut cau dins dels 10–15 minuts objectiu. Amb tot el backlog P1 tancat, la propera fita és decidir entre `P0-*` (bloquejadors comercials), `SH-016` (QA de hardware) o algun `ER-*` de profunditat d'Escape Room. `SH-016` continua requerint hardware real.

## 9. Fora d'abast intencionat

Seguint la decisió del projecte, l'autoria final d'àudio, mescla, il·luminació, postprocessat, materials i qualitat gràfica correspon al propietari de la plantilla. El roadmap sí que inclou les APIs, events, opcions i punts d'integració necessaris perquè aquests continguts es puguin connectar sense modificar el codi base.
