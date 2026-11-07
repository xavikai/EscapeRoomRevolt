# Fita 06 — Càmera, visió nocturna i evidències

Data: 2 d'agost de 2026  
Unity: `6000.4.9f1`

## Resultat

La càmera ha deixat de ser un component afegit automàticament al jugador. Ara és un equipament físic modular, diferent de la llanterna i preparat per substituir-ne les primitives per un model final.

El bucle disponible és:

1. trobar i equipar la càmera;
2. alçar-la o baixar-la;
3. aplicar zoom;
4. activar visió nocturna quan cal;
5. consumir una bateria exclusiva de càmera;
6. mantenir un subjecte enquadrat durant el temps requerit;
7. arxivar l'evidència de forma persistent;
8. completar un objectiu mitjançant `EvidenceRecorded`.

## Diferència entre llanterna i càmera

| Sistema | Llanterna | Càmera |
|---|---|---|
| Component | `FlashlightController` | `NightVisionController` |
| Equipament | `EquippableItem` | `EquippableItem` |
| Recurs | `batteries` | `camcorder_battery` |
| Acció principal | il·luminar | observar i gravar |
| Consum | mentre la llum està activa | només amb visió nocturna activa |
| Persistència | càrrega i estat | càrrega, posició alçada i NV |

La separació d'IDs evita que recarregar la càmera consumeixi per error les piles de la llanterna.

## Prefab modular

`Assets/_EscapeRoomTemplate/Prefabs/Survival/Camcorder_Modular.prefab` conté:

- `EquippableItem` al root;
- `NightVisionController`;
- `CamcorderEvidenceRecorder`;
- `Rigidbody` i collider de món;
- `ModelSocket`;
- `Placeholder_ReplaceMe` amb primitives;
- `NightVisionIlluminator` com a presentació mínima substituïble;
- `ReplaceableModelSlot`.

Per substituir el model:

1. assigna el prefab visual a `ReplaceableModelSlot.Model Prefab`;
2. mantén la lògica, colliders i scripts al root;
3. ajusta la pose equipada a `EquippableItem`;
4. assigna el teu Light o efecte al camp `Night Vision Illuminator`;
5. connecta Animator, postprocess o so als `UnityEvent` del controlador.

No cal modificar cap script per canviar el model.

## `NightVisionController`

Malgrat conservar el nom per compatibilitat amb projectes anteriors, ara representa el controlador complet de la càmera.

API principal:

```csharp
camcorder.SetCamcorderRaised(true);
camcorder.SetNightVisionEnabled(true);
camcorder.SetZoomed(true);

float remaining = camcorder.Charge01;
CamcorderBatteryState state = camcorder.BatteryState;
bool reloaded = camcorder.ReloadBattery();
```

Estats de bateria:

- `Normal`;
- `Low`;
- `Critical`;
- `Empty`.

El consum només avança quan:

- la càmera està equipada;
- està alçada;
- la visió nocturna està activa;
- el joc no està bloquejat per un menú.

Quan arriba a zero, la visió nocturna s'apaga. La gravació normal continua sent possible si el nivell encara és visible: aquest és el fallback jugable.

### Hooks de presentació

El component exposa events per:

- càmera alçada/baixada;
- NV activada/desactivada;
- recàrrega;
- bateria baixa, crítica o buida;
- càrrega normalitzada.

Aquests hooks permeten afegir el perfil URP, soroll, vinyeta, animacions i àudio sense acoblar-los a la lògica. El roadmap manté només aquesta integració artística com a `SH-015`.

## Zoom

El zoom modifica el FOV de la càmera principal de forma progressiva i restaura el valor original en baixar o deixar anar l'equipament. No crea una segona càmera, fet que evita renderitzat duplicat i funciona també amb el `Camera` del `XROrigin`.

Paràmetres:

- `Zoom Field Of View`;
- `Zoom Speed`.

## Evidències

### `EvidenceDefinition`

ScriptableObject que conté:

- ID estable;
- títol;
- descripció;
- segons necessaris de gravació;
- distància màxima.

### `RecordableEvidence`

Es col·loca al root funcional del subjecte i referencia un `EvidenceDefinition`. El model pot viure sota un `ModelSocket` independent.

```csharp
recordable.Configure(evidenceDefinition, optionalFocusPoint);
```

El gizmo magenta mostra el punt de focus i la distància màxima.

### `CamcorderEvidenceRecorder`

Llança un raycast des del centre de la càmera. La gravació només progressa mentre:

- la càmera està equipada i alçada;
- l'acció `RecordEvidence` està mantinguda;
- el mateix subjecte continua enquadrat;
- el subjecte és dins la distància autoritzada;
- l'evidència encara no ha estat gravada.

Canviar de subjecte o deixar anar el botó reinicia el progrés per defecte. El camp `Reset Progress When Released` permet canviar aquesta política.

### `EvidenceJournal`

Servei persistent que conserva els IDs gravats i implementa `ISaveable`.

```csharp
bool recorded = EvidenceJournal.Instance.IsRecorded("anomaly_subject");
IEnumerable<EvidenceDefinition> entries = EvidenceJournal.Instance.RecordedEvidence;
```

El format de save actual és:

```json
{"version":1,"recordedIds":["anomaly_subject"]}
```

També queda inclòs als snapshots de checkpoint perquè implementa la mateixa interfície de persistència.

## Objectius

`ObjectiveTrigger` incorpora `EvidenceRecorded`. Quan el diari afegeix una entrada publica `OnEvidenceRecorded`; `ObjectiveManager` completa qualsevol objectiu amb el mateix `targetId`.

Exemple de la demo:

```text
restore_power
    -> record_anomaly (target: anomaly_subject)
        -> escape_facility
```

## Controls

### PC

| Acció | Valor inicial |
|---|---|
| Alçar/baixar càmera | `C` |
| Visió nocturna | `N` |
| Recarregar bateria | `B` |
| Zoom | botó dret del ratolí |
| Gravar evidència | botó esquerre del ratolí |
| Deixar anar equipament | `G` |

### Meta Quest / OpenXR

| Acció | Binding inicial |
|---|---|
| Alçar/baixar | botó secundari de la mà esquerra |
| NV | clic de l'stick dret |
| Recarregar | clic de l'stick esquerre |
| Zoom | trigger esquerre |
| Gravar | trigger dret |

Totes les accions existeixen a l'Input System i els bindings de teclat apareixen al menú d'ajustos UI Toolkit.

## HUD UI Toolkit

`GameplayHUD.uxml` mostra el panell només quan la càmera està equipada. Inclou:

- estat `BAIXADA`, `PREPARADA`, `ZOOM`, `NV ACTIVA` o `REC`;
- càrrega i percentatge;
- feedback normal/baix/crític;
- títol del subjecte;
- barra de progrés de gravació;
- recordatori de controls.

No s'ha afegit cap Canvas.

## Demo

`SurvivalHorrorDemoBuilder` genera:

- `Camcorder_Modular` prop del punt inicial;
- `CamcorderBatteryPickup_Demo` amb dues bateries;
- `Evidence_AnomalySubject_Modular`;
- `Evidence_AnomalySubject.asset`;
- `Objective_RecordEvidence.asset`;
- bateria afegida al `DefaultItemCatalog`;
- cadena d'objectius de quatre passos.

## Validació realitzada

- compilació sense errors de projecte;
- càmera i subjecte presents una sola vegada;
- càmera equipada correctament i model connectat al socket;
- alçar, NV i zoom acceptats només amb equipament;
- accions `RecordEvidence` i `CamcorderZoom` presents a runtime;
- recàrrega de `10%` a `100%` consumint exactament una bateria de càmera;
- gravació de `anomaly_subject` completada;
- objectiu `record_anomaly` completat per event;
- diari buidat i restaurat des del JSON de save;
- contracte UI Toolkit verificat;
- deixar anar la càmera apaga NV, la marca com no equipada i baixa el HUD;
- Play Mode final sense errors ni warnings.

## Pendent

- connectar el perfil URP i tractament visual final de NV;
- decidir el nivell de soroll/degradació per dificultat;
- aplicar `reduceFlashes` a qualsevol efecte artístic que es connecti;
- validar bindings, ergonomia, rendiment i confort en Meta Quest físic;
- ampliar el diari amb una pantalla navegable quan s'abordi el casebook complet.
