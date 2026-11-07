# Fita 02 — Percepció, persecució i restauració fiable

## Resultat

La Fita 02 converteix la demo de Survival Horror en un bucle tècnic verificable:

```text
exploració → soroll o visió → sospita → investigació → persecució
                                      ↓                  ↓
                                  cerca i retorn ← amagatall / zona segura
```

S'han completat i retirat del backlog viu `SH-003`, `SH-004`, `SH-007` i `SH-011`. Les tasques d'amagatalls, dany, checkpoints i demo continuen al roadmap perquè encara els falten criteris comercials com respiració, feedback final, dificultats i durada mesurada.

## Project Validation d'OpenXR

S'han aplicat i persistit els dos ajustos opcionals mostrats per Unity:

- `USE_INPUT_SYSTEM_POSE_CONTROL`;
- `USE_STICK_CONTROL_THUMBSTICKS`.

Els defines s'han configurat per `Standalone`, `Android` i `Windows Store Apps`. Android conserva OpenXR, `MetaQuestFeature`, els perfils Touch de Meta i el perfil Oculus Touch; és la configuració base adequada per continuar la validació amb Oculus/Meta Quest.

Els ajustos no canvien cap binding del projecte: indiquen a OpenXR que utilitzi els controls moderns de l'Input System abans que les APIs antigues quedin obsoletes.

## Sistema de soroll de gameplay

`GameplayNoise` és un bus independent de l'àudio audible. Un estímul conté:

- posició;
- radi;
- tipus;
- objecte emissor;
- instant d'emissió.

Fonts integrades:

- passes i sprint;
- portes;
- accions de nivell;
- objectes físics deixats o llançats;
- impactes de rigidbodies.

`GameplayImpactNoiseEmitter` calcula el radi segons la velocitat relativa de la col·lisió. Un objecte marcat com a llançat augmenta temporalment el radi del pròxim impacte.

```csharp
GameplayNoise.Emit(
    transform.position,
    8f,
    GameplayNoiseType.PlayerAction,
    gameObject);
```

`GameplayNoiseDebugVisualizer` conserva un historial curt i dibuixa esferes de colors amb gizmos. És una ajuda d'autor: no crea UI ni depèn del renderer.

## Visibilitat i percepció

`PlayerVisibility` publica un multiplicador neutral respecte del pipeline gràfic. Té en compte:

- visibilitat base;
- jugador ajupit;
- jugador corrent;
- estat ocult;
- una o diverses `VisibilityZone` superposades.

Una `VisibilityZone` permet definir zones fosques o exposades sense obligar el comprador a utilitzar un shader concret. La demo incorpora `DarkRoute_VisibilityZone` a la ruta lateral.

`HorrorEnemyController` combina aquest multiplicador amb:

- distància màxima;
- angle de FOV amb producte escalar;
- oclusió amb raycast;
- detecció instantània a curta distància;
- temps configurable fins a detecció completa;
- pèrdua gradual de consciència;
- memòria de l'última posició;
- estímuls auditius.

Els valors d'autor són a `HorrorEnemyProfile`. La demo utilitza una detecció progressiva de 0,65 segons i una distància instantània d'1,75 metres.

## Director de persecució

`ChaseDirector` agrega l'estat de tots els enemics registrats. Exposa:

```csharp
ChaseDirector.Instance.ChaseStarted += OnChaseStarted;
ChaseDirector.Instance.ChaseEnded += OnChaseEnded;
```

També disposa de `UnityEvent` per connectar música, llums, portes o scripting de nivell sense editar codi. El final de persecució utilitza un període de gràcia per evitar canvis ràpids quan diversos enemics perden i recuperen el jugador.

`ChaseSafeZone` finalitza totes les persecucions actives i bloqueja temporalment una nova detecció. La sortida de la demo n'inclou una.

## Amagatalls inspeccionables

El prefab `HidingLocker_Modular.prefab` separa tres anchors:

- `InsideAnchor`: posició del jugador ocult;
- `ExitAnchor`: sortida segura;
- `InspectionAnchor`: punt al qual navega la IA.

Quan el jugador s'amaga durant una persecució, l'enemic investiga l'últim amagatall conegut. Després del retard definit al perfil pot executar `ForceExpose()`. Els events d'entrada, sortida i exposició permeten connectar animació, respiració i àudio posteriorment.

## Portes compatibles amb IA

`Door.TryOpenForAI(bool forceLocked)` és el contracte neutral per a enemics. Cada porta decideix si:

- permet operació enemiga;
- conserva el pany;
- pot ser forçada quan el perfil enemic també ho autoritza.

Els `NavMeshObstacle` de la porta es desactiven en obrir i es reactiven en tancar. Això evita que una porta visualment oberta continuï bloquejant la ruta.

La demo diferencia:

- `ChaseDoor_AICompatible`: porta normal que jugador i enemic poden obrir;
- `SecurityGate`: porta d'objectiu que la IA no pot saltar-se.

## Dany, mort i checkpoints

`PlayerVitals` incorpora una finestra d'invulnerabilitat configurable i l'event `Damaged`, preparat per connectar feedback visual, haptic o sonor.

`CheckpointManager` manté un snapshot en memòria separat de les ranures manuals. En arribar a un checkpoint captura tots els components actius o inactius que implementen `ISaveable`. En respawn:

1. restaura el snapshot;
2. mou el rig PC o VR al punt de respawn;
3. restaura salut i stamina;
4. allibera bloquejos de moviment;
5. publica `Respawned`.

Això restaura portes, inventari, objectius i altres entitats existents. La política per dificultat i la recreació transaccional d'entitats destruïdes continuen pendents a `SH-009`.

## Contingut de prova afegit a la demo

El builder crea automàticament:

- dos objectes físics llançables amb soroll d'impacte;
- una zona de baixa visibilitat;
- una porta compatible amb IA i parets de pas;
- un visualitzador de soroll;
- un anchor d'inspecció a l'armari;
- una zona segura a la sortida.

Tots els visuals continuen sent primitives substituïbles. La regeneració es fa amb:

`Escape Room Framework > Demo > Create or Update Survival Horror Demo`

## Validació realitzada a Unity 6000.4.9f1

- compilació: 0 errors i 0 warnings del codi del projecte;
- Play Mode: 0 errors i 0 warnings;
- scripts perduts a la demo: 0;
- soroll d'impacte: `Patrol → Suspicious`, consciència `0,55`;
- detecció a curta distància: `Chase`, consciència `1,00`;
- `ChaseDirector`: 1 perseguidor actiu i final robust després de supressió;
- porta d'IA: oberta correctament i `NavMeshObstacle.enabled == false`;
- checkpoint: respawn al `Checkpoint_Start` i porta restaurada al seu estat tancat;
- serveis runtime: `PlayerVisibility`, `ChaseDirector` i `CheckpointManager` presents;
- defines Android: `USE_INPUT_SYSTEM_POSE_CONTROL;USE_STICK_CONTROL_THUMBSTICKS`.

Captura de la prova:

`Assets/_EscapeRoomTemplate/Documentation/Captures/SurvivalHorrorDemo_Fita02.png`

## Següent fita

El següent bloc recomanat és:

1. completar `SH-006` amb respiració/events i més varietats d'amagatall;
2. completar `SH-008` amb fonts de dany, feedback i derrota verificats;
3. afegir presets de dificultat (`SH-010`);
4. ampliar i cronometrar la demo fins als 10–15 minuts (`SH-012`);
5. validar controls, UI i interaccions en Meta Quest real.

`SH-008` i `SH-010` s'han completat posteriorment a la Fita 03; consulta `SURVIVAL_HORROR_MILESTONE_03.md`.
