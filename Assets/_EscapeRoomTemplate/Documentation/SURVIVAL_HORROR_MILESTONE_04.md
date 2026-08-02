# Fita 04 — Entitats de checkpoint i traversal modular

## Resultat

La Fita 04 completa `SH-009`: els checkpoints ja restauren jugador, enemics, objectius, inventari i entitats recollides o eliminades del món sense modificar les ranures manuals.

També introdueix la base de `SH-016`, encara mantinguda al roadmap fins a completar més mostres, validació VR real i comportament específic per enemics.

## Restauració d'entitats

El checkpoint coordina tres snapshots:

1. dades JSON de tots els `ISaveable`;
2. estat de cada `CheckpointEntity`;
3. conjunt d'IDs destruïts mantingut per `SaveManager`.

`CheckpointEntityState` conserva:

- activació del GameObject;
- posició i rotació;
- escala local;
- velocitat lineal i angular del Rigidbody.

En restaurar, `CheckpointManager` aplica primer els IDs destruïts, després reactiva o desactiva les entitats i finalment carrega els `ISaveable`. Aquest ordre és important: permet que l'inventari torni a l'estat anterior alhora que el pickup reapareix.

## Pickups

En perfil Survival Horror, `PickableItem` garanteix que disposa d'un `CheckpointEntity`. Quan hi ha un sistema de checkpoints actiu, l'objecte es desactiva en lloc de destruir-se físicament.

Cas A — recollit després del checkpoint:

```text
checkpoint: pickup al món, inventari buit
jugador: recull pickup
respawn: pickup reapareix, inventari torna a estar buit
```

Cas B — recollit abans del checkpoint:

```text
checkpoint: pickup ja recollit, inventari conté l'objecte
jugador: consumeix o altera l'inventari
respawn: pickup continua absent, inventari recupera l'objecte
```

La càrrega manual continua utilitzant la persistència normal de `SaveManager`; el snapshot de mort no escriu ni elimina cap ranura.

## API de SaveManager

S'han afegit dues operacions de runtime:

```csharp
HashSet<string> snapshot = SaveManager.Instance.CaptureDestroyedEntities();
SaveManager.Instance.RestoreDestroyedEntities(snapshot);
```

Serveixen perquè un checkpoint pugui revertir el progrés temporal sense confondre'l amb una partida guardada.

## Traversal

`TraversalObstacle` defineix un enllaç autorable amb:

- tipus `Vault`, `Climb`, `Ladder` o `Squeeze`;
- `EntryAnchor` i `ExitAnchor`;
- durada;
- alçada d'arc;
- `AnimationCurve`;
- prompt i events d'inici, final i cancel·lació;
- gizmos de trajectòria.

`TraversalController` viu al root del jugador i és compartit per PC i VR. El `Bootstrapper` només el crea quan el flag `Traversal` està actiu.

Durant una travessa:

1. bloqueja moviment i mirada PC o locomoció VR;
2. desactiva temporalment el CharacterController PC;
3. interpola cap a l'anchor d'entrada;
4. recorre la corba fins a l'anchor de sortida;
5. restaura col·lisió i controls.

En VR utilitza `VRPlayerPlatformAdapter.TeleportRig`, preservant l'offset físic del visor.

## Cancel·lació segura

`CancelTraversal()`:

- atura la coroutine;
- torna a l'última posició segura;
- reactiva el CharacterController;
- restaura controls si el jugador continua viu;
- publica events de cancel·lació.

Una mort durant traversal cancel·la el moviment, però manté els controls bloquejats fins que la mort es resol. El respawn posterior els restaura normalment.

## Separació per gènere

S'ha afegit `OptionalGameFeature.Traversal`:

- actiu per defecte en `SurvivalHorror`;
- desactivat en `EscapeRoom`;
- configurable en `CustomHybrid`.

Els obstacles de traversal es desactiven automàticament quan el flag no està disponible.

## Demo

`SurvivalHorrorDemoBuilder` crea:

- `Traversal_Vault_Demo`;
- `Traversal_Climb_Demo`;
- anchors i valors de moviment;
- `NavMeshObstacle` perquè l'enemic busqui una ruta alternativa;
- `CheckpointEntity` explícit al pickup de piles.

## Validació realitzada a Unity 6000.4.9f1

- compilació del projecte sense errors ni warnings propis;
- pickup recollit després del checkpoint: reactivat en respawn;
- inventari posterior al checkpoint: revertit correctament;
- ID destruït posterior al checkpoint: eliminat en restaurar;
- pickup recollit abans del checkpoint: continua inactiu després del respawn;
- inventari capturat: recuperat;
- ID destruït capturat: recuperat;
- vault: inici, controls bloquejats, final exacte a l'anchor i controls restaurats;
- climb: cancel·lació segura i CharacterController reactivat;
- mort durant traversal: cancel·lació immediata sense desbloqueig prematur;
- respawn posterior: controls restaurats al `Checkpoint_Mid`.

Captura:

`Assets/_EscapeRoomTemplate/Documentation/Captures/SurvivalHorrorDemo_Fita04_Traversal.png`

## Pendent de SH-016

- mostres jugables específiques de ladder i squeeze-through;
- validació física i confort en Meta Quest;
- decisió per obstacle sobre si l'enemic l'utilitza o busca una alternativa;
- animacions o presentació connectades pels events, sense imposar art al framework.
