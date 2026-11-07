# Fita 01 — Base jugable de Survival Horror

## Estat

La primera base jugable està implementada a `SurvivalHorrorDemo.unity`. No és art final ni una escena comercial acabada: és una vertical slice tècnica creada amb primitives que demostra el bucle de programació i permet substituir els visuals sense perdre comportament.

S’han retirat del roadmap les tasques `SH-001`, `SH-002` i `SH-005` perquè els seus criteris de programació ja estan coberts:

- perfil Survival Horror modular i separació total respecte d’Escape Room;
- salut, resistència, esgotament, dany, mort i recuperació;
- state machine d’enemic data-driven amb NavMesh.

La resta de tasques continuen al roadmap perquè encara tenen criteris pendents, encara que ja disposin d’una primera implementació funcional.

## Com obrir i provar la demo

1. A Unity, selecciona `Escape Room Framework > Configuration > Use Survival Horror Profile`.
2. Obre `Assets/_EscapeRoomTemplate/Scenes/SurvivalHorrorDemo.unity`.
3. Prem Play.
4. Recull les piles del corredor.
5. Entra a la ruta lateral i activa `EmergencyGenerator` amb `E`.
6. Decideix si tornes per la porta de seguretat o continues per la ruta lateral.
7. Evita l’enemic, utilitza l’armari si et detecta i arriba a la sortida.

El botó `Nova partida` del menú principal apunta actualment a `SurvivalHorrorDemo` mitjançant `GameFlowSettings.asset`.

## Controls PC

| Acció | Tecla |
|---|---|
| Moure | WASD |
| Mirar | Ratolí |
| Interactuar / entrar o sortir d’un amagatall | E |
| Córrer | Shift esquerre |
| Ajupir-se | Ctrl esquerre |
| Inventari | I |
| Llanterna | F |
| Recarregar llanterna | R |
| Pujar o baixar la càmera | C |
| Activar o desactivar visió nocturna | N |
| Recarregar la càmera | B |
| Pausa | Esc |

`R` i `B` són independents. Això evita que una sola pulsació consumeixi simultàniament una pila de la llanterna i una de la càmera.

## Separació entre Escape Room i Survival Horror

El recurs central és:

`Assets/_EscapeRoomTemplate/Resources/GenreFeatureSettings.asset`

El perfil `SurvivalHorror` activa:

- `Flashlight`;
- `Sanity`;
- `HorrorEvents`;
- `PlayerVitals`;
- `EnemyAI`;
- `Hiding`;
- `NightVision`;
- `Checkpoints`.

El perfil `EscapeRoom` no activa cap mòdul específic de Survival Horror. El `Bootstrapper` no crea salut, càmera, checkpoints o HUD de supervivència; els enemics, amagatalls i zones específiques es desactiven abans de començar a jugar.

Per canviar de perfil:

- `Escape Room Framework > Configuration > Use Escape Room Profile`;
- `Escape Room Framework > Configuration > Use Survival Horror Profile`;
- `Escape Room Framework > Configuration > Use Custom Hybrid Profile`.

El perfil híbrid permet triar exactament quines banderes s’utilitzen.

## Arquitectura implementada

### PlayerVitals

`PlayerVitals` és l’autoritat de salut i resistència del jugador.

Responsabilitats:

- drenar resistència mentre el jugador corre;
- aplicar retard abans de recuperar-la;
- impedir l’sprint quan el jugador està esgotat;
- permetre reprendre l’sprint quan s’ha recuperat el llindar configurat;
- rebre dany;
- avisar de mort;
- restaurar el jugador al checkpoint o provocar derrota si no n’hi ha cap;
- desar i carregar salut i resistència.

Events disponibles:

```csharp
PlayerVitals.Instance.HealthChanged += OnHealthChanged;
PlayerVitals.Instance.StaminaChanged += OnStaminaChanged;
PlayerVitals.Instance.ExhaustionChanged += OnExhaustionChanged;
PlayerVitals.Instance.HiddenChanged += OnHiddenChanged;
PlayerVitals.Instance.Died += OnPlayerDied;
```

`ExhaustionChanged` és el punt d’integració previst per connectar respiració, animació o àudio sense acoblar-los al moviment.

### GameplayNoise

`GameplayNoise` és un bus d’estímuls de gameplay. No depèn del volum real d’un `AudioSource`.

```csharp
GameplayNoise.Emit(
    transform.position,
    10f,
    GameplayNoiseType.PlayerAction,
    gameObject);
```

Actualment publiquen soroll:

- passes caminant;
- passes corrent;
- portes en obrir-se o tancar-se;
- consola d’emergència;
- qualsevol objecte amb `GameplayNoiseEmitter`.

La integració d'impactes, objectes llançats i gizmos s'ha completat a la Fita 02; consulta `SURVIVAL_HORROR_MILESTONE_02.md`.

### HorrorEnemyProfile

És un `ScriptableObject` que permet configurar sense codi:

- velocitat de patrulla;
- velocitat d’investigació;
- velocitat de persecució;
- distància i angle de visió;
- multiplicador d’oïda;
- freqüència de percepció;
- memòria de persecució;
- durada de cerca;
- distància, dany i cooldown d’atac.

La demo utilitza `DemoStalkerProfile.asset`.

### HorrorEnemyController

Estats disponibles:

```text
Idle → Patrol → Suspicious → Investigate → Search → Return
                                  ↘ Chase ↗
```

La visió comprova distància, angle i oclusió amb raycast. L’oïda rep `GameplayNoiseStimulus`. L’enemic conserva l’última posició coneguda, investiga, busca durant un temps i torna a la ruta.

Quan arriba al jugador, aplica dany a `PlayerVitals`. Si sospita que el jugador és dins l’amagatall i arriba prou a prop, pot forçar-ne la sortida.

### HidingSpot

`HidingSpot` hereta d’`InteractableBase` i utilitza dos anchors:

- `InsideAnchor`: posició del jugador mentre està ocult;
- `ExitAnchor`: posició segura de sortida.

En entrar:

- mou el `CharacterController` de manera segura;
- bloqueja el moviment però conserva la mirada;
- marca `PlayerVitals.IsHidden`;
- impedeix ocupar un segon amagatall.

En prémer `E` es surt. `ForceExpose()` permet que la IA expulsi el jugador.

### CheckpointManager

El checkpoint de mort és independent de les ranures manuals de Save/Load.

En morir:

1. es força la sortida d’un possible amagatall;
2. es desactiva temporalment el `CharacterController`;
3. es restaura posició i rotació;
4. es recuperen salut i resistència;
5. es publica `Respawned`;
6. els enemics tornen a la seva posició inicial.

La tasca `SH-009` continua pendent perquè encara falta restaurar objectius i polítiques de dificultat de manera transaccional.

### NightVisionController

La càmera és independent de la llanterna. Té estat propi, consum només quan la visió nocturna està activa i recàrrega mitjançant l’item `batteries` de l’inventari.

API principal:

```csharp
nightVision.SetCamcorderRaised(true);
nightVision.SetNightVisionEnabled(true);
bool reloaded = nightVision.ReloadBattery();
```

El focus verd creat en runtime és un fallback funcional de programació. La capa visual final —postprocessat URP, gra, aberració, bloom, materials i il·luminació— queda preparada per connectar-se als events `StateChanged` i `ChargeChanged`, però correspon a l’autoria gràfica del projecte.

### Objectius de la demo

La demo reutilitza `ObjectiveManager` i tres `ObjectiveDefinition`:

1. `recover_batteries`: es completa amb `OnItemPickedUp` per l’ID `batteries`;
2. `restore_power`: requereix l’anterior i es completa interactuant amb `EmergencyGenerator`;
3. `escape_facility`: requereix l’anterior i es completa entrant a `DemoExit_Victory`.

Quan es completen tots, `GameFlowManager` acaba la partida amb l’ending `survival_demo_escape`.

### Ruta alternativa i porta

`SecurityGate` és una porta lliscant bloquejada. `EmergencyGenerator` la desbloqueja. La ruta lateral connecta els dos costats de la porta, així el nivell sempre conserva una ruta navegable.

La porta conté un `NavMeshObstacle` amb carving. Quan és tancada, l’enemic ha de recalcular la ruta i utilitzar el pas lateral; quan s’obre, recupera el camí curt. Això retarda la persecució sense teletransportar l’enemic.

## HUD UI Toolkit

`SurvivalHUDController` és un presenter separat de `GameplayUIController`.

Mostra només en el perfil adequat:

- salut;
- resistència;
- estat de càmera;
- càrrega nocturna;
- objectiu disponible;
- estabilitat, mitjançant el sistema de cordura existent.

Els elements estan definits a `GameplayHUD.uxml` i `GameplayHUD.uss`. No s’ha afegit cap Canvas legacy.

## Substituir les primitives per models

Prefabs preparats:

- `HorrorEnemy_Modular.prefab`;
- `HidingLocker_Modular.prefab`.

La programació, NavMesh, colliders i interacció viuen al root. Els visuals viuen sota `ModelSocket` i són gestionats per `ReplaceableModelSlot`.

Procediment segur:

1. selecciona el prefab;
2. localitza `ReplaceableModelSlot`;
3. assigna un prefab només visual a `Model Prefab`;
4. ajusta escala i orientació dins del prefab visual;
5. no moguis ni eliminis `Eye`, `InsideAnchor`, `ExitAnchor`, colliders, `NavMeshAgent` o `NavMeshObstacle`;
6. entra en Play Mode i comprova focus, col·lisió i navegació.

El placeholder s’oculta automàticament quan hi ha un model assignat.

## Regenerar la demo

Menú:

`Escape Room Framework > Demo > Create or Update Survival Horror Demo`

El builder:

- crea o actualitza prefabs modulars;
- crea perfils i objectius;
- construeix l’escena amb primitives;
- genera el NavMesh;
- afegeix l’escena a Build Settings;
- activa el perfil Survival Horror;
- configura `Nova partida` perquè obri la demo.

La regeneració substitueix `SurvivalHorrorDemo.unity`. No s’ha d’utilitzar sobre una versió de la demo que contingui art manual no desat en prefabs.

## Validació realitzada

Validació dins Unity 6000.4.9f1:

- compilació: 0 errors;
- escena: 0 scripts perduts i 0 prefabs trencats;
- `NavMeshAgent.isOnNavMesh`: `true`;
- estat inicial de l’enemic: `Patrol`;
- estímul de soroll: `Patrol → Suspicious`;
- mort: reapareix al `Checkpoint_Start` amb salut completa;
- amagatall: entrada i sortida actualitzen `IsHidden`;
- objectius: piles → generador → sortida;
- porta: el generador la desbloqueja;
- resultat final: `GameFlowState.Completed`, `Victory`, ending `survival_demo_escape`;
- perfil Escape Room: no es creen vitals, càmera, enemic, amagatall ni checkpoint;
- consola durant la prova final: 0 errors.

## Pendent abans de considerar SH-012 complet

`SH-012` es manté al roadmap. La demo actual demostra el bucle, però encara no compleix tota la profunditat comercial de 10–15 minuts. Falta:

- més espai d’exploració i una durada mesurada amb usuaris;
- més punts de cobertura i amagatalls;
- tractament final de càmera nocturna;
- feedback de dany i mort;
- QA amb joc complet, guardat, càrrega i dificultats;
- paritat VR.

Aquestes mancances continuen representades per les files no eliminades de `ROADMAP.md`.
