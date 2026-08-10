# Anàlisi de mecàniques i proposta de reorganització de sales

Informe de disseny sobre la plantilla. Tres parts:

1. Separació real dels tres eixos del projecte i què pertany a cadascun.
2. Inventari de mecàniques disponibles, per eix.
3. Proposta de fusió de sales del museu de demostració.

Per al **funcionament i configuració** de cada mecànica, consulta `DOCUMENTACIO_COMPLETA.md`. Aquest document és l'anàlisi; aquell és el manual.

---

## 1. Els tres eixos no són del mateix tipus

Una precisió important abans de la llista, perquè condiciona tot el que ve després:

**Escape Room i Survival Horror són gèneres. VR no: és una plataforma.**

- El gènere el defineix `Resources/GenreFeatureSettings.asset` i té tres valors: `EscapeRoom`, `SurvivalHorror`, `CustomHybrid`. Determina **quines mecàniques existeixen** durant l'execució.
- VR és un eix **perpendicular**: qualsevol dels dos gèneres s'executa tant a PC com a VR, amb la mateixa lògica de joc. VR no aporta mecàniques de joc pròpies; aporta **maneres d'accionar** les mecàniques que ja hi ha, més algunes opcions de confort.

Dit d'una altra manera: no has de triar entre tres coses. Has de triar **un gènere** (o una barreja) i, per separat, **quines plataformes** vols suportar.

```
                 PC                VR
            +-------------+-------------+
Escape Room |   suportat  |   suportat  |
            +-------------+-------------+
Survival    |   suportat  |   suportat  |
            +-------------+-------------+
```

Això té una conseqüència pràctica: **no cal duplicar contingut**. Els components de survival poden quedar-se als prefabs encara que facis un Escape Room pur — s'autodesactiven sols segons el perfil.

---

## 2. Inventari de mecàniques

### 2.1 Base compartida (sempre activa, als tres perfils)

Aquestes no es poden desactivar perquè són l'esquelet del joc.

| Mecànica | Component principal | Verb del jugador |
|---|---|---|
| Interacció per raig | `InteractionManager` + `IInteractable` | Mirar i prémer E |
| Portes i calaixos | `Door` | Obrir / desbloquejar |
| Interruptors binaris | `InteractableToggle` | Encendre / apagar |
| Controls de N posicions | `SteppedPositioner` + `InteractableCycler` | Cicles entre posicions |
| Disparador genèric | `InteractableTrigger` | Accionar un `UnityEvent` |
| Notes i documents | `InteractableNote` | Llegir |
| Inventari | `InventoryManager` + `InventoryItemData` | Recollir, usar, combinar |
| Examen 3D | `ExamineChamber` + `ExamineHotspot` | Girar, fer zoom, inspeccionar punts |
| Receptor d'objectes | `ItemReceiver` | Aplicar un objecte a un lloc |
| Objectes físics | `PhysicsGrabbable` + `PhysicsGrabber` | Agafar, transportar, llançar |
| Encaix físic | `PhysicsSocket` | Deixar un objecte al seu lloc |
| Pistes progressives | `HintManager` + `HintData` | (passiu, per temps) |
| Objectius | `ObjectiveManager` + `ObjectiveDefinition` | Progressar |
| Guardat / càrrega | `SaveManager` + `ISaveable` | Desar partida |
| Àudio de superfície | `AudioManager` + `SurfaceAudioData` | (passiu, per material del terra) |
| Disparador narratiu | `NarrativeTrigger` | Entrar en una zona |
| Cinemàtica de feedback | `CinematicCamera` | (passiu, en resoldre) |

### 2.2 Puzles (base compartida)

Tots hereten de `PuzzleController`, i per tant tots tenen: identitat (`PuzzleDefinition`), esdeveniments `OnSolved`/`OnFailed`, càmera de feedback, guardat d'estat i `ResetPuzzle()`.

| Puzle | Verb | Variant aleatòria per partida |
|---|---|---|
| `CodePanelPuzzle` | Introduir un codi | Sí (`_randomizeCode`) |
| `SequencePuzzle` | Accionar **en ordre** | Sí (`_randomizeOrder`) |
| `StatePuzzle` | Deixar N controls en la posició correcta (ordre indiferent) | No |
| `PlacementPuzzle` | Transportar peces i col·locar-les al seu lloc | Sí (`_randomizeMapping`) |
| `ThrowPuzzle` | Encertar dianes llançant objectes | No |
| `SlidingPuzzle` | Reordenar una graella lliscant peces | Sempre barrejat i resoluble |
| `PipePuzzle` | Rotar peces fins formar un **camí connectat** | Sí (`_randomizeRotations`) |
| `SocketPuzzle` | Inserir un objecte d'inventari concret | No |
| `MultiStagePuzzle` | Encadenar fases (meta-puzle) | No |

### 2.3 Exclusives de Survival Horror

Dotze mòduls, activables individualment amb el perfil `CustomHybrid`. Aquesta és la llista real del codi (`OptionalGameFeature`):

| Flag | Sistema | Què aporta |
|---|---|---|
| `Flashlight` | `FlashlightController` | Llanterna amb bateria consumible i feix volumètric |
| `Sanity` | `SanityController` + `SanityProfile` | Cordura en 4 estats, amb penalització per errors de puzle |
| `HorrorEvents` | `HorrorEventTrigger` + `TensionDirector` | Ensurts condicionats per zona/cordura, amb pressupost global |
| `PlayerVitals` | `PlayerVitals` | Vida, dany, mort/respawn i estamina de sprint |
| `EnemyAI` | `HorrorEnemyController` + `HorrorEnemyProfile` | Enemic amb patrulla, visió, oïda i persecució |
| `Hiding` | `HidingSpot` | Amagatalls (armari, sota el llit) amb risc d'exposició |
| `NightVision` | `NightVisionController` | Visió nocturna amb càrrega i zoom |
| `Checkpoints` | `CheckpointManager` + `CheckpointEntity` | Punts de control que restauren món i jugador |
| `Traversal` | `TraversalObstacle` | Saltar, esquivar i passar per forats, amb política per a l'enemic |
| `EvidenceRecording` | `CamcorderEvidenceRecorder` + `EvidenceJournal` | Gravar proves amb càmera, estil found-footage |
| `AdvancedEvasion` | `EvasionController` | Lean amb col·lisió, mirada enrere i slide |
| `AdvancedDoors` | `Door` (mode avançat) | Portes que es poden aguantar, forçar o bloquejar |

Sistemes de suport que acompanyen els anteriors: `ChaseDirector` (ritme de persecució), `ChaseSafeZone`, `PlayerVisibility` + `VisibilityZone` (com de visible ets), `GameplayNoiseEmitter` (què sent l'enemic), `DamageVolume`, `SurvivalObjectiveZone`, `SurvivalPowerConsole`, `SurvivalDifficultyProfile` (dificultat), `CameraShakeController`.

### 2.4 Capa VR (plataforma, no gènere)

No hi ha cap mecànica de joc aquí. Hi ha **traducció d'entrada** i **confort**:

| Component | Funció |
|---|---|
| `VRInteractionBridge` | Connecta XRI amb `IInteractable`: la mateixa lògica respon a PC i a VR |
| `VRPlayerPlatformAdapter` | Abstreu cap/mans perquè el gameplay no depengui del visor |
| `VRComfortController` + `VRComfortSettings` | Vinyeta, gir per passos, teleport vs. locomoció contínua |
| `VRUIPointerBridge` / `VRUIToolkitPresenter` / `VRUIPanelColliderController` | Menús UI Toolkit utilitzables amb els comandaments |

Diferències reals de jugabilitat en VR: el lean i la mirada enrere passen a ser **físics** (mous el cos), el camera shake es desactiva sol, i el slide artificial ve desactivat per defecte.

---

## 3. Proposta de fusió de sales

### 3.1 Avís previ: els rètols del terra no són fiables

Abans d'analitzar res, cal dir-ho: **el rètol pintat al terra d'algunes sales no descriu el que hi ha a dins**. El cas comprovat:

| Sala | Rètol | Contingut real (verificat component a component) |
|---|---|---|
| Room 1 | "Button & Light: Click to toggle state" | Una taula amb un **calaix** (`Door` en mode lliscant) i un **armari** (`Door` en mode pivotant). Cap `InteractableTrigger`, cap llum commutable. |

Ve del generador `TemplateSceneBuilder`, on la crida que pinta el rètol i la que crea el contingut no coincideixen (`CreatePlatform(..., "Button & Light...")` seguit de `CreateDrawerPuzzle(...)`).

**Conseqüència metodològica:** qualsevol anàlisi de redundància feta llegint els rètols és inservible. La taula següent està feta inspeccionant els components reals de cada sala, no la retolació.

### 3.2 Diagnòstic històric (abans de la reorganització)

La proposta d'aquesta secció ja s'ha aplicat en gran part: el museu visible actual té 10 sales. La taula conserva la numeració antiga perquè explica l'origen de les fusions; per a l'estat real sala per sala consulta [l'auditoria de tancament de 2026-08-09](../AUDITORIA_ESCAPE_ROOM_2026-08-09.md).

El criteri útil no és "quines mecàniques sobren" sinó **quantes sales comparteixen el mateix verb del jugador**.

| Verb real | Sala | Component clau |
|---|---|---|
| Obrir contenidors sense pany | Room1 | `Door` (Slide + Pivot) |
| Obrir portes amb clau | Room2 | `Door` amb pany + item |
| Llegir informació | Room3 | `InteractableNote` |
| Introduir un codi | Room4 | `CodePanelPuzzle` |
| Entrar en una zona | Room5 | `NarrativeTrigger` |
| Aplicar un objecte a un lloc | Room6 | `ItemReceiver` |
| Alternar un estat binari | Room7 | `InteractableToggle` + `Light` |
| Combinar dos objectes | Room8 | `PhysicsSocket` amb prefab resultat |
| Accionar en ordre | Room9 | `SequencePuzzle` |
| Posicionar controls (N posicions) | Room10 | `StatePuzzle` + `SteppedPositioner` |
| *(prova de sistema)* | Room11 | Àudio de superfície |
| *(prova de sistema)* | Room12 | `HintManager` |
| Apuntar i llançar | Room13 | `ThrowPuzzle` |
| Transportar i col·locar | Room14 | `PlacementPuzzle` |
| Reordenar graella | Room15 | `SlidingPuzzle` |
| *(meta)* | Room16 | `MultiStagePuzzle` |
| Rotar per connectar | Room17 | `PipePuzzle` |

Les agrupacions que en surten: **portes/contenidors** (R1, R2), **interruptors** (R7, R10), **posar la cosa al lloc** (R6, R8), **proves de sistema sense verb propi** (R11, R12).

### 3.3 Les fusions que recomano

**A. Room1 + Room2 → "Portes i contenidors"** *(estalvi: 1 sala)*

Les dues fan servir el **mateix component `Door`** en estats complementaris: Room1 mostra contenidors sense pany (calaix lliscant, armari pivotant) i Room2 mostra portes amb pany que requereixen clau. Juntes expliquen el component sencer —modes de moviment i estat de bloqueig— en lloc de mostrar-ne la meitat cadascuna.

**B. Room3 + Room4 → "la nota dona el codi"** *(estalvi: 1 sala, i millora el disseny)*

Aquesta és la fusió més valuosa, i no és només per estalviar espai. Ara mateix la nota no diu res útil i el teclat té el codi escrit al costat: **dues demostracions aïllades que no ensenyen a dissenyar un escape room**. Fusionades, es converteixen en la unitat mínima real d'un escape room: *trobar informació → aplicar-la*. És l'exemple que un comprador de la plantilla necessita veure.

**C. Room7 + Room10 → "Interruptors"** *(estalvi: 1 sala)*

Room7 és un interruptor **binari** que encén un focus; Room10 són palanques de **N posicions** que obren una porta. Des que el sistema de moviment es va generalitzar a `SteppedPositioner`, són els dos extrems del mateix component: veure'ls junts explica quan n'hi ha prou amb un booleà i quan cal un índex de posició.

**C-bis. Room6 + Room8 → "Receptors i combinació"** *(estalvi: 1 sala)*

Les dues són "posa la cosa correcta al lloc correcte", amb dos mecanismes diferents: `ItemReceiver` consumeix un objecte de l'inventari i dispara un event; `PhysicsSocket` accepta un objecte físic i pot **substituir-lo per un de nou** (pila + llanterna buida = llanterna carregada). És la fusió menys evident de totes, però estalvia una sala i deixa la comparació a la vista.

**D. Room11 i Room12 → dissoldre-les** *(estalvi: 2 sales)*

No són mecàniques, són **proves de sistemes transversals**:
- Room11 prova l'àudio de superfície. Es demostra millor etiquetant el terra d'una sala que ja existeix.
- Room12 prova les pistes. Es demostren millor associant un `HintData` a un puzle real que veient-les en abstracte.

De fet, integrar-les fa que el museu sigui **més honest**: així es veu que aquests sistemes funcionen *dins* del joc, no en una vitrina a part.

**E. Room5 → moure-la al passadís** *(estalvi: 1 sala)*

Un disparador narratiu és invisible per definició. Dedicar-li una plataforma amb número és contradictori. Posa'l al passadís entre dues sales: es dispara mentre camines, que és exactament com s'usa en un joc real.

**F. Room16 (multi-fase) → opcional** *(estalvi: 1 sala si ho fas)*

`MultiStagePuzzle` és un **meta-puzle**: encadena fases. Demostrar-lo amb fases inventades dins d'una sala aïllada explica pitjor el concepte que fer-lo servir per encadenar dues sales que ja existeixen. Si t'atreveixes, és la millor demostració possible; si no, deixa-la.

### 3.4 Resultat

| | Ara | Proposta |
|---|---:|---:|
| Sales | 17 | **10** |
| Verbs coberts | 14 | 14 |
| Sales sense mecànica pròpia | 2 | 0 |

Sales resultants: portes i contenidors · nota+codi · narrativa (al passadís) · receptors i combinació · interruptors · seqüència · llançar · col·locar · lliscant · multi-fase · canonades.

**No es perd cap mecànica.** Es perden sales redundants i es guanya una sala (nota+codi) que ensenya **disseny d'escape room** en comptes de només ensenyar un component aïllat.

### 3.5 El que NO recomano fusionar

Aquestes semblen semblants però són verbs genuïnament diferents; fusionar-les empobriria la plantilla:

- **Room9 (seqüència) i Room10 (posicions)**: l'ordre importa vs. no importa. És una distinció de disseny fonamental.
- **Room13 (llançar) i Room14 (col·locar)**: punteria i força vs. transport i precisió. Ho vam separar expressament.
- **Room15 (lliscant) i Room17 (canonades)**: reordenar peces vs. trobar un camí connectat. La segona té cerca de camins (BFS); la primera, no.

---

## 4. Nota sobre la numeració interna encara pendent

Hi ha un desfasament heretat entre el número pintat al terra i el nom dels objectes de dins:

| Plataforma (terra) | Objectes de contingut |
|---|---|
| `Room11_Audio` | `Room12_AudioTest` |
| `Room12_Hints` | `Room13_HintTest` |
| `Room06_Physics` | `Room06_ThrowPuzzle_Logic` |

La migració de tancament ha assignat definicions explícites abans de renomenar: ara les arrels són `Room07_PlacementPuzzle`, `Room08_SlidingPuzzle`, `Room09_MelodyPuzzle` i `Room10_PipePuzzle`, sense canviar els IDs persistents.
