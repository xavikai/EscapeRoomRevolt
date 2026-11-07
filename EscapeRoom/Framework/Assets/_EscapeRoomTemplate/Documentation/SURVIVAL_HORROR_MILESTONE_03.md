# Fita 03 — Dany, mort, amagatalls baixos i dificultat

## Resultat

La Fita 03 completa els criteris de programació de `SH-008` i `SH-010`. La plantilla disposa ara de dany reutilitzable, mort amb temps de feedback, respawn o derrota, tres presets de dificultat i selecció des del menú UI Toolkit.

`SH-006` continua al roadmap fins a validar els amagatalls en Meta Quest real. La restauració transaccional d'entitats destruïdes de `SH-009` s'ha completat posteriorment a la Fita 04.

## Presets de dificultat

El sistema consta de:

- `SurvivalDifficultyProfile`: valors d'un preset;
- `SurvivalDifficultySettings`: catàleg i perfil per defecte;
- `SurvivalDifficultyService`: selecció runtime, persistència i API de consulta.

Assets de mostra:

- `Difficulty_Easy.asset`, mostrat com **Accessible**;
- `Difficulty_Standard.asset`;
- `Difficulty_Nightmare.asset`.

Els presets modifiquen:

| Àrea | Paràmetres |
|---|---|
| Jugador | Dany rebut, consum i recuperació de stamina, salut recuperada al checkpoint |
| Enemic | Velocitat, vista, oïda, dany, freqüència d'atac i temps d'inspecció |
| Recursos | Consum de llanterna i visió nocturna |
| Flux | Checkpoints i permís de guardat manual |

Nightmare desactiva checkpoints i guardat manual. En morir, el joc mostra el resultat de derrota. Accessible redueix consum i agressivitat; Standard conserva els valors base.

La dificultat seleccionada es conserva a `PlayerPrefs` per a partides noves i també forma part del Save/Load mitjançant `ISaveable`.

API:

```csharp
SurvivalDifficultyService.Instance.SetDifficulty("nightmare");

float sightMultiplier = SurvivalDifficultyService.EnemySight;
bool canRespawn = SurvivalDifficultyService.AllowsCheckpoints;
bool canSave = SurvivalDifficultyService.AllowsManualSaving;
```

## Integració al menú UI Toolkit

El panell d'ajustos mostra un `DropdownField` de dificultat només quan `PlayerVitals` està actiu. Conté els tres perfils disponibles.

Quan la dificultat prohibeix el guardat manual:

- el botó `Guardar partida` de pausa queda desactivat;
- la pantalla de guardat explica la política;
- `SaveManager` rebutja també crides directes i publica `OperationFailed`.

Aquesta doble comprovació impedeix saltar-se la política des d'una UI personalitzada.

## Dany tipificat

`DamageInfo` transporta:

- quantitat final;
- tipus (`Enemy`, `Environment`, `Trap`, `Fall` o genèric);
- objecte emissor;
- punt d'impacte.

`PlayerVitals.ApplyDamage(float)` continua disponible per compatibilitat. Els sistemes nous poden utilitzar el context complet:

```csharp
vitals.ApplyDamage(new DamageInfo(
    25f,
    DamageType.Trap,
    gameObject,
    transform.position));
```

`DamageVolume` és una font reutilitzable configurada com a trigger. Pot aplicar dany en entrar o de manera contínua, té cooldown propi i es pot desactivar després del primer impacte.

## Feedback sense dependència artística

`PlayerDamageFeedbackRelay` exposa `UnityEvent` per:

- rebre dany;
- morir;
- reaparèixer;
- acabar en derrota.

La plantilla no imposa postprocessat, animació, haptics ni àudio. El comprador pot connectar aquestes capes a l'Inspector sense modificar `PlayerVitals`.

També es mantenen els events C# `DamageReceived`, `Damaged`, `Died` i `DeathResolved`.

## Mort i resolució

El dany letal ja no teletransporta el jugador en el mateix frame:

1. `PlayerVitals` marca `IsDead`;
2. bloqueja moviment i mirada PC, o locomoció VR;
3. publica `Died`;
4. espera el retard de feedback amb temps no escalat;
5. intenta restaurar el checkpoint;
6. si no està permès, crida `FailGame()` i produeix derrota.

El respawn restaura controls, salut segons dificultat, stamina i estat d'ocultació.

## Amagatalls baixos

`HidingSpot` diferencia `Locker`, `UnderBed`, `Container` i `Custom`. També incorpora:

- temps mínim abans de poder sortir;
- postura ajupida forçada per PC;
- senyal de respiració entre 0 i 1;
- events C# d'entrada, sortida i exposició;
- els `UnityEvent` ja existents per autoria visual o sonora.

`PlayerMovement.SetForcedCrouch()` ajusta immediatament CharacterController i càmera, fins i tot quan el moviment està congelat.

La demo inclou `HidingBed_Modular.prefab`, amb `ModelSocket`, anchors independents i primitive placeholder substituïble.

## Contingut nou de la demo

- `HidingBed_Modular` com a segon tipus d'amagatall;
- `DamageHazard_Demo` per provar dany ambiental;
- catàleg de dificultat a Resources;
- selector de dificultat dins del menú d'ajustos.

## Validació realitzada a Unity 6000.4.9f1

- compilació: 0 errors i 0 warnings del projecte;
- Play Mode final: 0 errors i 0 warnings;
- scripts perduts: 0;
- tres perfils carregats i selector amb tres opcions;
- Standard: checkpoints i guardat manual actius;
- Nightmare: checkpoints i guardat manual desactivats;
- Nightmare: multiplicador de dany verificat, 10 punts base produeixen 12 punts;
- volum de dany: 100 → 65 i segon impacte bloquejat pel cooldown;
- mort Standard: estat mort immediat, controls bloquejats i respawn posterior al `Checkpoint_Start`;
- mort Nightmare: `GameFlowState.Failed` i resultat `Defeat`;
- llit: `UnderBed`, ocultació activa, moviment bloquejat i postura ajupida;
- sortida del llit: ocultació, bloqueig i postura restaurats;
- senyal de respiració en calma verificat a `0,20`;
- botó de guardat de pausa desactivat en Nightmare.

Captura:

`Assets/_EscapeRoomTemplate/Documentation/Captures/SurvivalHorrorDemo_Fita03_Difficulty.png`

## Següent fita

1. fer QA real d'amagatalls, UI i locomoció en Meta Quest;
2. definir com es recreen entitats destruïdes en restaurar checkpoints;
3. ampliar la demo i mesurar-ne la durada fins als 10–15 minuts;
4. començar la capa de traversal: vault, climb, ladder i squeeze-through.
