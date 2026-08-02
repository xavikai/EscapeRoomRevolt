# Escape Room / Survival Horror Framework

## Documentació completa de la plantilla

Versió documentada: Unity 6.4 (`6000.4.9f1`)  
Pipeline: Universal Render Pipeline (URP) 17.4  
Input: Unity Input System 1.20  
VR: OpenXR 1.16, XR Plug-in Management 4.5 i XR Interaction Toolkit 3.3  
Interfície: UI Toolkit; no es necessita cap `Canvas` per al flux principal.

> Aquesta guia explica tant l'ús per a dissenyadors com l'arquitectura per a programadors. Les parts d'àudio, art i configuració gràfica final queden intencionadament obertes perquè l'autor del projecte les personalitzi.

---

## Índex

1. Què inclou la plantilla
2. Inici ràpid
3. Escenes i flux de joc
4. Menú principal i menú de pausa
5. Estructura de carpetes
6. Arquitectura global
7. Bootstrap i serveis persistents
8. Sistema d'entrada i controls
9. Jugador de PC
10. Interacció comuna PC/VR
11. Outline URP professional
12. Inventari i accés ràpid
13. Crear objectes d'inventari
14. Combinació, ús contextual i examen 3D
15. Objectes físics i equipament
16. Llanterna
17. Cordura i esdeveniments de terror
18. Puzles
19. Pistes progressives
20. Objectius i final de partida
21. Guardat i càrrega
22. UI Toolkit
23. Preparació per a VR
24. Substitució segura de models
25. Menú superior `Escape Room Framework`
26. Escenes de demostració
27. Crear una sala des de zero
28. Ampliar la plantilla amb C#
29. Validació i publicació
30. Resolució de problemes
31. Referència ràpida d'API

---

## 1. Què inclou la plantilla

La plantilla és una base modular per construir jocs d'Escape Room i Survival Horror en primera persona, amb una mateixa lògica reutilitzable a PC i VR.

### Mecàniques d'Escape Room

- Interacció per raycast amb portes, calaixos, notes, objectes, palanques i interruptors.
- Inventari persistent amb piles, categories, accions contextuals i accés ràpid.
- Combinació guiada d'objectes.
- Examen 3D amb rotació i zoom.
- Lectura de documents i notes.
- Portes bloquejades i receptors d'objectes.
- Panells de codi numèric.
- Puzles de seqüència.
- Puzles d'estat, com una combinació de palanques.
- Puzles de socket lògic o físic.
- Manipulació física, transport, rotació, llançament i encaix d'objectes.
- Pistes progressives associades al puzle actiu.
- Objectius amb prerequisits i final automàtic de sala.
- Finals de victòria o derrota configurables.
- Guardat de l'estat de puzles, portes, inventari, objectius i objectes recollits.

### Mecàniques de Survival Horror

- Llanterna equipable amb càrrega persistent.
- Consum de bateries des de l'inventari.
- HUD de bateria només quan la llanterna està equipada.
- Sistema de cordura amb quatre estats: estable, inquiet, angoixat i crític.
- Penalització de cordura en errors de puzle.
- Recuperació passiva configurable.
- Esdeveniments de terror condicionats per zona, cordura o activació manual.
- Esdeveniments persistents d'un sol ús o amb cooldown.
- Subtítols, so i `UnityEvent` per connectar animació, llums o altres efectes.
- Equipament visible a la mà i possibilitat de deixar-lo anar.
- Evasió avançada opcional: lean amb col·lisió de càmera, mirada enrere i slide amb postura segura.
- En VR, lean i mirada enrere físics; slide artificial de Quest desactivat per defecte.

### Sistemes transversals

- Menú principal, pausa, resultats, crèdits, ajustos i Save/Load amb UI Toolkit.
- Rebinding de teclat durant l'execució.
- Tres ranures manuals, guardat ràpid i càrrega ràpida.
- Escriptura JSON atòmica i miniatures.
- Arquitectura per esdeveniments i dades en `ScriptableObject`.
- Adaptadors de plataforma per evitar dependències directes de PC o d'un visor VR.
- Eines d'Editor per crear, preparar i validar contingut.

### Perfils de gènere: què s'activa i què s'amaga

La plantilla té un únic perfil actiu per projecte, desat a `Resources/GenreFeatureSettings.asset`. Es canvia des de `Escape Room Framework > Configuration` i s'aplica en iniciar la sessió de Play següent.

| Sistema | Escape Room | Survival Horror | Custom Hybrid |
|---|---:|---:|---:|
| Interacció, inventari, examen, notes i objectes físics | Sí | Sí | Sí |
| Portes, codis, puzles, pistes, objectius, Save/Load i finals | Sí | Sí | Sí |
| PC, comandament, VR, menús i UI Toolkit | Sí | Sí | Sí |
| Llanterna, bateries, HUD i controls `F`/`R` | Ocult i inactiu | Actiu | Configurable |
| Evasió avançada (`Alt + A/D`, `X`, `V`) | Oculta i inactiva | Activa | Configurable |
| Estabilitat/cordura i el seu HUD | Ocult i inactiu | Actiu | Configurable |
| Penalització de cordura per errors | Inactiva | Activa | Configurable |
| Esdeveniments de terror | Inactius | Actius | Configurable |

Per crear un Escape Room pur:

1. Selecciona `Escape Room Framework > Configuration > Use Escape Room Profile`.
2. Inicia Play de nou. El HUD d'`ESTABILIDAD`, la bateria de la llanterna i els controls de llanterna no apareixeran.
3. Pots conservar els components de Survival Horror als prefabs i escenes: s'autodesactiven, de manera que no cal duplicar contingut.

Per crear un Survival Horror, selecciona `Use Survival Horror Profile`. S'activen conjuntament llanterna, cordura i esdeveniments de terror.

`Use Custom Hybrid Profile` permet seleccionar individualment `Flashlight`, `Sanity` i `Horror Events` a l'Inspector de `GenreFeatureSettings`. Això permet, per exemple, un Escape Room fosc amb llanterna però sense cordura. `Select Genre Feature Settings` localitza l'asset central.

La separació afecta runtime i interfície, no elimina assets. Així es pot canviar de gènere sense perdre configuració. Els sistemes compartits continuen disponibles als tres perfils.

---

## 2. Inici ràpid

### Provar la plantilla

1. Obre `Assets/_EscapeRoomTemplate/Scenes/MainMenu.unity`.
2. Prem Play.
3. Selecciona `Nueva partida`.
4. Es carregarà `ShowcaseMuseum`, la primera escena jugable configurada.
5. Mou-te amb WASD i mira amb el ratolí.
6. Mira un objecte interactuable i prem `E`.
7. Prem `I` per obrir l'inventari.
8. Prem `Esc` per obrir la pausa.

### Provar directament una escena jugable

També pots obrir `ShowcaseMuseum.unity` o `LockedOffice.unity` i prémer Play. El `GameManager` de l'escena crea o localitza els serveis necessaris. No obstant això, una build comercial sempre ha de començar per `MainMenu`.

### Ordre actual del Build Profile

1. `MainMenu.unity`
2. `ShowcaseMuseum.unity`
3. `LockedOffice.unity`

Si una escena es canvia de nom o de carpeta, cal actualitzar el Build Profile i `GameFlowSettings`.

---

## 3. Escenes i flux de joc

`GameFlowManager` és l'autoritat de navegació. És persistent entre escenes i manté un únic estat:

| Estat | Significat |
|---|---|
| `Boot` | Encara no s'ha inicialitzat el flux. |
| `MainMenu` | L'escena activa coincideix amb el menú configurat. |
| `Loading` | Hi ha una càrrega asíncrona en curs. |
| `Playing` | La partida està activa. |
| `Paused` | `Time.timeScale` és 0 i el menú de pausa és visible. |
| `Completed` | La partida ha acabat amb victòria. |
| `Failed` | La partida ha acabat amb derrota. |

La configuració es carrega de:

`Assets/_EscapeRoomTemplate/Resources/GameFlowSettings.asset`

Aquest asset defineix almenys:

- l'escena del menú principal;
- la primera escena jugable;
- la ranura preferida per a `Continuar`.

### Exemple de navegació des de codi

```csharp
using EscapeRoomRevolt.Core.Flow;

public void ComencarPartida()
{
    GameFlowManager.EnsureInstance().StartNewGame();
}

public void TornarAlMenu()
{
    GameFlowManager.EnsureInstance().ReturnToMainMenu();
}

public void ReiniciarEscena()
{
    GameFlowManager.EnsureInstance().RestartCurrentScene();
}
```

`EnsureInstance()` retorna el gestor existent o en crea un si la escena s'ha executat aïlladament. Això facilita provar escenes sense passar sempre pel menú.

---

## 4. Menú principal i menú de pausa

### Sí: existeix un menú principal real

L'escena és:

`Assets/_EscapeRoomTemplate/Scenes/MainMenu.unity`

Conté un `UIDocument` amb:

- `EscapeRoomMenu.uxml`: estructura visual;
- `EscapeRoomMenu.uss`: estil;
- `UIToolkitMenuController`: navegació i accions.

No és un simple panell dins de l'escena jugable: és una escena independent i és la primera escena de la build.

### Opcions del menú principal

- `Continuar`: carrega la ranura preferida o la partida guardada més recent. Es desactiva si no hi ha cap partida.
- `Nueva partida`: reinicia l'estat temporal i carrega la primera escena jugable. No elimina les ranures existents.
- `Cargar partida`: mostra les tres ranures manuals.
- `Ajustes`: obre opcions i controls.
- `Créditos`: mostra el text de crèdits que el comprador ha de personalitzar.
- `Salir`: demana confirmació i tanca l'aplicació. A l'Editor atura Play Mode.

### Opcions del menú de pausa

- reprendre la partida;
- guardar;
- carregar;
- ajustos;
- tornar al menú principal;
- sortir del joc.

### Flux exacte per tornar al menú principal

1. Prem `Esc` durant una partida.
2. Prem `Menú principal…`.
3. Apareix una pantalla titulada `VOLVER AL MENÚ PRINCIPAL`.
4. Prem `VOLVER AL MENÚ`.
5. `GameFlowManager` restaura `Time.timeScale = 1` i carrega `MainMenu` asíncronament.

La confirmació evita perdre progrés per un clic accidental. `CANCELAR` retorna al menú de pausa.

### Si no es carrega

Comprova, en aquest ordre:

1. que has premut el segon botó de confirmació;
2. que `MainMenu.unity` està habilitada al Build Profile;
3. que `GameFlowSettings.MainMenuScene` és `MainMenu` o la seva ruta correcta;
4. que no hi ha un error a Console just després de confirmar;
5. que no s'ha iniciat una altra transició al mateix frame.

---

## 5. Estructura de carpetes

```text
Assets/_EscapeRoomTemplate/
├── Art/                         Materials i art temporal
├── Audio/                       Música, veus i passos de mostra
├── Core/
│   ├── Editor/                  Generadors, menú i validadors
│   └── Runtime/
│       ├── Flow/                Escenes, objectius i finals
│       ├── Input/               InputRouter
│       ├── Save/                SaveManager i ISaveable
│       └── Settings/            Ajustos persistents
├── Player/
│   ├── Common/                  Contractes compartits PC/VR
│   ├── PC/                      Moviment i adaptador desktop
│   └── VR/                      Adaptador, bridge XRI i UI 3D
├── Prefabs/                     GameManager, jugadors i equipament
├── Resources/                   Assets carregats per nom en runtime
├── Scenes/                      MainMenu i demos
├── ScriptableObjects/           Items, puzles, pistes i Survival
├── Settings/                    URP i RenderTexture d'examen
├── Systems/
│   ├── Equipment/
│   ├── Hint/
│   ├── Interaction/
│   ├── Inventory/
│   ├── Puzzle/
│   └── Survival/
└── UI/
    ├── PC/                      Façana/compatibilitat de UI
    └── Toolkit/                 UXML, USS i controladors
```

### Regla d'organització per a un joc comprador

No cal modificar el framework per crear contingut. És preferible crear una carpeta pròpia:

```text
Assets/MyGame/
├── Art/
├── Audio/
├── Prefabs/
├── Scenes/
├── ScriptableObjects/
│   ├── Items/
│   ├── Puzzles/
│   ├── Hints/
│   ├── Objectives/
│   └── Endings/
└── Scripts/
```

Això simplifica actualitzacions futures de l'asset.

---

## 6. Arquitectura global

```text
Input System
    ↓
InputRouter ───────────────┐
    ↓                      │
Jugador PC / Rig VR       │
    ↓                      │
InteractionDispatcher     │
    ↓                      │
IInteractable             │
    ├── portes             │
    ├── notes              │
    ├── objectes           │
    └── puzles             │
                           │
EventBus ←─────────────────┘
    ├── ObjectiveManager
    ├── HintManager
    ├── GameplayUIController
    └── lògica personalitzada

SaveManager ← ISaveable de cada sistema
GameFlowManager → escenes, pausa i finals
```

Principis aplicats:

- `ScriptableObject` per separar dades de lògica i de presentació.
- Interfaces per evitar dependències concretes.
- Esdeveniments C# i `EventBus` per comunicar sistemes desacoblats.
- `UnityEvent` per permetre connexions de dissenyador a l'Inspector.
- IDs persistents per desar dades sense dependre del nom visible.
- Una font única d'entrada: `InputRouter`.
- Una autoritat única de flux: `GameFlowManager`.
- Una autoritat única de persistència: `SaveManager`.

---

## 7. Bootstrap i serveis persistents

El prefab `GameManager.prefab` conté el `Bootstrapper`. Durant l'arrencada garanteix que existeixin els serveis globals necessaris, com:

- `GameFlowManager`;
- `SaveManager`;
- `GameSettingsService`;
- `InputRouter`;
- `SanityController`;
- `AudioManager`;
- `HintManager`.

Els serveis que han de sobreviure a canvis d'escena utilitzen `DontDestroyOnLoad` i protegeixen el singleton contra duplicats.

`GameContext` conserva l'estat general d'inicialització i neteja l'`EventBus` en una sessió nova. No és un contenidor de dependències extensiu: el codi actual utilitza registres o instàncies explícites per als sistemes principals.

### EventBus

Tots els esdeveniments són `struct`. Exemple d'escolta d'un puzle:

```csharp
using EscapeRoomRevolt.Core;
using UnityEngine;

public sealed class ObreCompartimentEnResoldre : MonoBehaviour
{
    private void OnEnable()
    {
        EventBus.Subscribe<OnPuzzleSolved>(QuanPuzleResol);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnPuzzleSolved>(QuanPuzleResol);
    }

    private void QuanPuzleResol(OnPuzzleSolved evt)
    {
        if (evt.puzzleId != "electric_panel") return;
        Debug.Log("Obrir compartiment secret");
    }
}
```

Cal donar-se de baixa a `OnDisable` o `OnDestroy`. En cas contrari, un objecte destruït podria deixar una subscripció morta.

---

## 8. Sistema d'entrada i controls

L'asset d'accions és:

`Assets/_EscapeRoomTemplate/Resources/Input/EscapeRoomInputActions.inputactions`

`InputRouter` en crea una còpia de runtime. Això evita modificar l'asset original quan el jugador reasigna controls.

### Controls predeterminats de PC

| Acció | Tecla / ratolí |
|---|---|
| Moure | WASD |
| Mirar | moviment del ratolí |
| Interactuar | E |
| Córrer | Shift esquerre |
| Ajupir-se | Ctrl esquerre |
| Saltar | Espai |
| Pausa / tancar modal | Esc |
| Inventari | I |
| Accés ràpid | 1, 2, 3, 4 |
| Navegar accés ràpid | roda del ratolí |
| Llanterna | F |
| Recarregar llanterna | R |
| Deixar equipament | G |
| Llançar objecte físic | clic esquerre |
| Rotar objecte físic | mantenir clic dret + ratolí |
| Deixar objecte físic | Q |
| Convertir objecte físic en inventari | E mentre el sostens |
| Demanar pista | H |
| Guardat ràpid | F5 |
| Càrrega ràpida | F9 |

També hi ha bindings de gamepad i XR dins del mateix asset.

### Llegir input en una mecànica nova

```csharp
using EscapeRoomRevolt.Core.Input;
using UnityEngine;

public sealed class ExempleInput : MonoBehaviour
{
    private void Update()
    {
        InputRouter input = InputRouter.Instance;
        if (input == null) return;

        Vector2 moviment = input.Move;
        if (input.HintPressed)
            Debug.Log("El jugador ha demanat una pista");
    }
}
```

No utilitzis `Input.GetKeyDown` en mecàniques noves. Passar per `InputRouter` manté rebinding, gamepad i VR en una única ruta.

### Rebinding

El menú `Ajustes > Controles` localitza el binding de teclat i inicia `StartInteractiveRebind`. `Esc` cancel·la l'operació. Les sobreescriptures es guarden a `GameSettingsData.bindingOverridesJson`, separades de les partides.

Si `E` deixa de funcionar:

1. obre `Ajustes`;
2. mira el valor d'`Interactuar`;
3. prem `Restablecer controles` si no indica `E`;
4. comprova que no hi hagi un modal obert;
5. comprova la capa `Interactable` i el LayerMask del jugador.

---

## 9. Jugador de PC

El prefab és `Assets/_EscapeRoomTemplate/Prefabs/Player_PC.prefab`.

Components principals:

- `PlayerMovement`: moviment, gravetat, salt, ajupir-se i càmera.
- `PlayerInputHandler`: accions de UI, accés ràpid i interaccions auxiliars.
- `PCPlayerPlatformAdapter`: publica cap i mà dreta al registre comú.
- `InteractionManager`: raycast d'interacció.
- `PhysicsGrabber`: objectes físics.
- `EquipmentController`: objectes equipables.

La mecànica no consulta directament `Camera.main` quan necessita la posició del jugador. Utilitza:

```csharp
Transform cap = PlayerPlatformRegistry.Current?.Head;
Transform maDreta = PlayerPlatformRegistry.Current?.GetHand(PlayerHand.Right);
```

Aquesta abstracció permet que la mateixa porta, equipament o puzle funcioni amb el rig VR.

---

## 10. Interacció comuna PC/VR

### Contracte `IInteractable`

Un interactuable exposa:

- text del prompt;
- tipus de cursor;
- si es pot usar ara;
- `Interact()`;
- entrada i sortida de focus.

`InteractableBase` ja implementa el contracte, la persistència base, l'outline i la generació d'un `SaveId` per instància d'escena.

### Exemple d'interactuable propi

```csharp
using EscapeRoomRevolt.Systems.Interaction;
using UnityEngine;
using UnityEngine.Events;

public sealed class BotóSecret : InteractableBase
{
    [SerializeField] private UnityEvent _onPressed;
    private bool _pressed;

    public override string InteractionPrompt =>
        _pressed ? "Ja està activat" : "Prémer botó";

    protected override void OnInteract()
    {
        if (_pressed) return;
        _pressed = true;
        SetInteractable(false);
        _onPressed?.Invoke();
    }

    [System.Serializable]
    private sealed class State { public bool pressed; }

    public override string SaveData()
    {
        return JsonUtility.ToJson(new State { pressed = _pressed });
    }

    public override void LoadData(string json)
    {
        State state = JsonUtility.FromJson<State>(json);
        _pressed = state != null && state.pressed;
        SetInteractable(!_pressed);
    }
}
```

### Flux PC

1. `InteractionManager` llança un raycast des del centre de càmera.
2. Cerca `IInteractable` al pare del collider.
3. En canviar l'objectiu, activa/desactiva focus i actualitza HUD.
4. Quan `InputRouter.InteractPressed` és cert, crida `InteractionDispatcher`.
5. El dispatcher executa la interacció, publica `OnInteractionPerformed` i envia haptic si la plataforma ho suporta.

### Seguretat amb objectes destruïts

Una referència guardada com a interface no conserva automàticament el comportament especial de `UnityEngine.Object == null`. Per això existeix `InteractableUtility.IsAlive()`. Qualsevol consumidor nou ha d'utilitzar-la abans d'accedir a un interactuable que es pugui destruir o desactivar.

---

## 11. Outline URP professional

La solució escollida és una `ScriptableRendererFeature` compatible amb Render Graph:

- `SelectionOutlineTarget` marca temporalment els renderers enfocats amb el bit 30 de `renderingLayerMask`.
- `SelectionMaskOutlineFeature` dibuixa aquests renderers en una màscara.
- el shader de composició detecta la vora de la màscara i la barreja amb la imatge final.

### Avantatges

- no modifica materials compartits;
- funciona amb models formats per diversos renderers;
- el gruix és estable en pantalla;
- és compatible amb prefabs i substitució de models;
- separa la selecció de les GameObject Layers;
- una sola passada gestiona tots els materials dels objectes.

### Configuració

1. Obre el Renderer de l'URP actiu, per exemple `Default_Forward_Renderer.asset`.
2. Comprova que conté `SelectionMaskOutlineFeature`.
3. Ajusta `thickness`, `color` i `injectionPoint`.
4. Assegura't que l'objecte deriva d'`InteractableBase` o incorpora `SelectionOutlineTarget`.
5. Els renderers visuals han d'estar sota el mateix arrel interactuable.

### Si no es veu

- confirma que la càmera usa el Renderer on hi ha la feature;
- comprova que els dos shaders `Hidden/EscapeRoom/...` existeixen;
- comprova que el renderer visual està inclòs a `_renderers`;
- evita substituir tot el prefab: substitueix només els fills de `ModelSocket`;
- comprova que el raycast realment enfoca l'objecte.

---

## 12. Inventari i accés ràpid

`InventoryManager` separa dos conceptes:

1. **Emmagatzematge**: fins a 20 slots per defecte, amb piles i quantitats.
2. **Accés ràpid**: fins a 4 referències per defecte; no duplica ni mou l'objecte.

Un accés ràpid guarda l'`ItemId`. Si s'esgota l'última unitat, la referència ràpida es neteja automàticament.

### Flux recomanat per al jugador

1. Recull una evidència amb `E`.
2. Obre l'inventari amb `I`.
3. Selecciona l'objecte.
4. La interfície només mostra accions vàlides.
5. Pot usar/equipar, llegir, examinar, combinar, assignar a accés ràpid o deixar-lo.

### Accions principals

`InventoryPrimaryAction` pot ser:

- `Automatic`: tria llegir, equipar/sostenir, consumir o cap acció segons dades.
- `Read`: obre la nota.
- `EquipOrHold`: instancia el `WorldPrefab` i l'equipa o sosté.
- `Consume`: elimina una unitat.
- `None`: no ofereix acció principal.

### API bàsica

```csharp
InventoryManager inventory = InventoryManager.Instance;

inventory.AddItem(itemData, 2);
bool teClau = inventory.HasItem("master_key");
inventory.UseItem("batteries");
inventory.SetActiveQuickSlot(1);
InventoryItemData actiu = inventory.GetActiveItem();
```

No modifiquis directament `Slots`. Utilitza l'API perquè es publiquin esdeveniments, s'actualitzi el HUD i es mantinguin els accessos ràpids.

---

## 13. Crear objectes d'inventari

### Crear les dades

1. Clic dret al Project.
2. `Create > Escape Room Framework > Inventory > Item`.
3. Configura un `ItemId` estable i únic.
4. Escriu nom i descripció.
5. Assigna icona i `WorldPrefab`.
6. Defineix categoria i acció principal.
7. Configura piles, consum, examen, lectura i combinacions.
8. Afegeix l'asset al `DefaultItemCatalog` o a un catàleg assignat al teu `InventoryManager`.

### Què ha de contenir el World Prefab

Per a un objecte recollible simple:

- collider;
- `PickableItem` amb el mateix `InventoryItemData`;
- visual sota un fill reemplaçable;
- opcionalment `Rigidbody` i `PhysicsGrabbable`.

Per a equipament:

- `Rigidbody`;
- collider;
- `EquippableItem`;
- el comportament d'equip, com `FlashlightController`;
- `ModelSocket` i model visual.

### IDs

`ItemId` és la clau de guardat, receptacles, bateries, combinacions i accés ràpid. No el canviïs després de publicar una versió sense una migració.

---

## 14. Combinació, ús contextual i examen 3D

### Combinació

Cada `InventoryItemData` pot declarar receptes:

- objecte compatible;
- resultat;
- si es consumeix aquest objecte;
- si es consumeix l'altre.

Exemple: `clau_trencada + cinta -> clau_reparada`.

El jugador selecciona `COMBINAR` i després un candidat compatible. La UI filtra les opcions. També es pot combinar des de l'examinador amb l'objecte actiu.

### Ús contextual en portes i receptors

`IInventoryItemTarget` defineix tres polítiques:

| Política | Comportament |
|---|---|
| `SelectedOnly` | Només prova l'objecte actiu de l'accés ràpid. |
| `OfferCompatible` | Obre un selector amb els objectes compatibles. Recomanat. |
| `AutoUseSingle` | Si només hi ha un candidat, l'utilitza automàticament. Opció d'accessibilitat. |

Exemple de receptor personalitzat:

```csharp
using EscapeRoomRevolt.Systems.Inventory;
using UnityEngine;

public sealed class ReceptorFusible : MonoBehaviour, IInventoryItemTarget
{
    public ItemUsePolicy UsePolicy => ItemUsePolicy.OfferCompatible;
    public bool ConsumeItemOnUse => true;

    public bool AcceptsItem(InventoryItemData item)
    {
        return item != null && item.ItemId == "fuse_15a";
    }

    public bool TryUseItem(InventoryItemData item)
    {
        if (!AcceptsItem(item)) return false;
        ActivarElectricitat();
        return true;
    }

    private void ActivarElectricitat() { }
}
```

### Examen 3D

`GameplayUIController` crea una càmera i una escena visual d'examen en runtime:

- arrossegar: rotar;
- roda: zoom;
- `Esc`: tancar;
- `COMBINAR`: intenta una combinació guiada.

L'objecte necessita `WorldPrefab` i `CanExamine = true`. El model d'examen és una còpia visual; no modifica l'objecte real ni el prefab.

---

## 15. Objectes físics i equipament

### Física

Un `PhysicsGrabbable` necessita `Rigidbody` i collider.

En PC:

- `E` sobre l'objecte: sostenir-lo;
- clic esquerre: llançar-lo;
- `Q`: deixar-lo;
- clic dret + ratolí: rotar-lo;
- `E` mentre el sostens i també té `PickableItem`: guardar-lo a l'inventari.

`PhysicsGrabber` restaura gravetat, damping, detecció de col·lisions i col·lisions amb el jugador en deixar-lo. També el deixa anar si s'allunya massa o s'obre una UI modal.

### Socket físic

`PhysicsSocket` és un trigger que:

1. espera que l'objecte compatible deixi d'estar sostingut;
2. compara l'`ItemId`;
3. desactiva física;
4. interpola fins al punt d'encaix;
5. llança `OnItemSnapped`;
6. opcionalment consumeix l'objecte i crea un resultat.

### Equipament

`EquipmentController` manté un únic objecte equipat. `EquippableItem` mou l'objecte al `EquipmentSocket`, desactiva colliders i notifica components `IEquipmentLifecycle`.

Quan es deixa anar amb `G`, es restaura al món amb física. Aquesta mateixa ruta serveix per a la llanterna i futurs objectes, com càmeres, detectors o armes defensives.

---

## 16. Llanterna

La llanterna modular és:

`Assets/_EscapeRoomTemplate/Prefabs/Survival/Flashlight_Modular.prefab`

### Ús del jugador

1. Equipa la llanterna interactuant-hi o usant-la des de l'inventari.
2. Prem `F` per encendre/apagar.
3. Prem `R` per consumir una unitat de l'item `batteries` i carregar-la al 100%.
4. Prem `G` per deixar-la al món.

Si no està equipada, `F` i `R` no fan res intencionadament.

### HUD

El HUD només apareix quan la llanterna està equipada. Mostra:

- percentatge;
- estat activa/en espera;
- estat de bateria baixa o crítica;
- recordatori de tecles.

### Configuració

`FlashlightController` permet ajustar:

- referència a `Light`;
- càrrega inicial;
- consum per segon;
- `ItemId` de bateria;
- si vol començar encesa quan s'equipa.

És `ISaveable`, per tant conserva càrrega i intenció d'encesa. `SaveId` actual: `Flashlight`. No hi ha d'haver dues llanternes persistents actives amb el mateix ID sense personalitzar el sistema.

---

## 17. Cordura i esdeveniments de terror

### SanityProfile

Defineix:

- valor màxim;
- recuperació passiva per segon;
- llindar `Uneasy`;
- llindar `Distressed`;
- llindar `Critical`.

Els llindars es normalitzen entre 0 i 1.

### API

```csharp
SanityController.Instance?.ApplyStress(12f);
SanityController.Instance?.Recover(8f);
SanityController.Instance?.SetNormalized(0.5f);
```

El HUD canvia text i classes USS quan canvia l'etapa. La plantilla no imposa efectes gràfics o d'àudio finals; el projecte pot escoltar `Changed` o `StageChanged` i aplicar postprocess, respiració o al·lucinacions pròpies.

### HorrorEvent

Un `HorrorEventDefinition` defineix:

- ID persistent;
- nom;
- subtítol;
- clip d'àudio;
- cordura màxima necessària per activar-se;
- estrès aplicat;
- si només passa una vegada;
- cooldown.

`HorrorEventTrigger` admet:

- entrada del jugador en un trigger;
- creuar un llindar de cordura;
- `TryTrigger()` manual.

Connecta `_onTriggered` a animacions, canvis de llum, aparicions o portes. El trigger desa si ja s'ha activat.

---

## 18. Puzles

### Separació dades/lògica

`PuzzleDefinition` conté dades reutilitzables:

- ID persistent;
- nom;
- categoria;
- objectiu textual;
- pistes;
- penalització de cordura per error.

`PuzzleController` conté estat runtime:

- `Unsolved`;
- `InProgress`;
- `Solved`.

També publica esdeveniments, desa l'estat i permet connectar `OnSolved` i `OnFailed` a l'Inspector.

### Panell de codi

`CodePanelPuzzle` admet:

- codi correcte;
- longitud màxima;
- comprovació automàtica;
- display TMP 3D;
- LED d'estat;
- sons de tecla, èxit i error.

Els botons 3D poden cridar `InputDigit("7")`; la UI Toolkit usa la mateixa API.

### Seqüència

`SequencePuzzle` rep IDs amb:

```csharp
sequencePuzzle.InputStep("red");
sequencePuzzle.InputStep("blue");
sequencePuzzle.InputStep("green");
```

Qualsevol desviació reinicia la seqüència i aplica el flux d'error.

### Estat

`StatePuzzle` observa diversos `InteractableToggle`. Es resol quan tots coincideixen amb el booleà requerit. L'ordre no importa.

### Socket

`SocketPuzzle` compara un `ItemId`, pot consumir-lo i crear un model col·locat. Per a un flux d'inventari més intuïtiu, una porta o `ItemReceiver` amb `OfferCompatible` acostuma a ser preferible. Per a interacció física, utilitza `PhysicsSocket`.

### Puzle personalitzat

```csharp
using EscapeRoomRevolt.Systems.Puzzle;

public sealed class PuzlePes : PuzzleController
{
    public void ActualitzarPes(float pes)
    {
        SetInProgress();
        if (pes >= 9.5f && pes <= 10.5f)
            Solve();
        else if (pes > 15f)
            Fail("Massa pes");
    }
}
```

`Solve`, `Fail` i `SetInProgress` ja gestionen persistència indirecta, EventBus, pistes, cordura i UnityEvents.

---

## 19. Pistes progressives

Cada `PuzzleDefinition` pot apuntar a un `HintData`.

Quan el puzle entra en progrés:

- es registra com a puzle actiu;
- `H` demana la següent pista;
- les pistes poden progressar de subtils a explícites;
- en resoldre el puzle es neteja el context actiu.

`HintZoneTrigger` permet activar pistes en entrar en una zona. És útil quan un puzle no s'inicia amb una interacció directa.

Recomanació de disseny:

1. pista 1: recorda l'element rellevant;
2. pista 2: relaciona dos elements;
3. pista 3: explica l'operació;
4. evita donar directament la solució fins a l'últim nivell.

---

## 20. Objectius i final de partida

### ObjectiveDefinition

Cada objectiu defineix:

- ID;
- títol i descripció;
- visibilitat;
- trigger;
- ID de l'element esperat;
- prerequisits.

Triggers disponibles:

- manual;
- puzle resolt;
- item recollit;
- nota llegida;
- interacció realitzada.

### ObjectiveSet

Agrupa els objectius d'una sala i un `EndingDefinition` de finalització.

### ObjectiveManager

Escolta l'EventBus i completa automàticament objectius. Quan tots estan complets:

1. publica `OnRoomEscaped`;
2. calcula el temps;
3. crida `CompleteGame`;
4. la UI mostra la pantalla de resultats.

Per completar un objectiu des de codi:

```csharp
ObjectiveManager.Instance?.CompleteObjective("restore_power");
```

### Final directe

`GameEndTrigger` pot activar-se per `UnityEvent`, per codi o quan el jugador entra al trigger.

```csharp
using EscapeRoomRevolt.Core.Flow;

public void MonstreAtrapaJugador(EndingDefinition derrota)
{
    GameFlowManager.EnsureInstance().FailGame(derrota);
}
```

La pantalla final ofereix reintentar, menú principal o sortir.

---

## 21. Guardat i càrrega

### Funcionalitats

- tres ranures manuals: `slot_1`, `slot_2`, `slot_3`;
- ranura ràpida interna `slot_0`;
- metadades d'escena, data, temps jugat i miniatura;
- JSON versionat;
- escriptura temporal i substitució final;
- restauració després de carregar l'escena adequada;
- registre d'entitats destruïdes o recollides;
- protecció contra `SaveId` duplicats;
- neteja de referències Unity destruïdes.

Els fitxers es desen sota:

`Application.persistentDataPath/SaveSlots`

La ubicació física depèn del sistema operatiu i del `Company Name`/`Product Name` del projecte.

### Contracte ISaveable

```csharp
public interface ISaveable
{
    string SaveId { get; }
    string SaveData();
    void LoadData(string json);
}
```

### Exemple complet

```csharp
using EscapeRoomRevolt.Core.Save;
using UnityEngine;

public sealed class GeneradorPersistent : MonoBehaviour, ISaveable
{
    [SerializeField] private string _saveId = "generator_room_a";
    [SerializeField] private GameObject _poweredVisual;
    private bool _powered;

    [System.Serializable]
    private sealed class State
    {
        public int version = 1;
        public bool powered;
    }

    public string SaveId => _saveId;

    private void Start()
    {
        SaveManager.Instance?.Register(this);
        ApplyVisual();
    }

    private void OnDestroy()
    {
        SaveManager.Instance?.Unregister(this);
    }

    public void Activate()
    {
        _powered = true;
        ApplyVisual();
    }

    public string SaveData()
    {
        return JsonUtility.ToJson(new State { powered = _powered });
    }

    public void LoadData(string json)
    {
        State state = JsonUtility.FromJson<State>(json);
        if (state == null) return;
        _powered = state.powered;
        ApplyVisual();
    }

    private void ApplyVisual()
    {
        if (_poweredVisual != null)
            _poweredVisual.SetActive(_powered);
    }
}
```

### Regles crítiques

- `SaveId` ha de ser únic dins de l'escena restaurada.
- No utilitzis un nom que pugui canviar com a ID publicat.
- Registra a `Awake` o `Start` i desregistra a `OnDestroy`.
- `LoadData` ha d'aplicar l'estat visual immediatament, sense reproduir recompenses o sons d'èxit.
- Inclou una versió dins de dades pròpies si preveus canvis d'esquema.
- Executa `Validation > Validate Save IDs` abans de publicar.

### Objectes recollits

`PickableItem` crida `MarkAsDestroyed(SaveId)` després d'afegir-se a l'inventari. En carregar, `SaveManager` elimina l'objecte mundial i evita que reaparegui.

---

## 22. UI Toolkit

### Assets principals

- `EscapeRoomMenu.uxml`: menú principal, pausa, ajustos, slots i resultats.
- `EscapeRoomMenu.uss`: tema visual dels menús.
- `GameplayHUD.uxml`: HUD, inventari, notes, keypad i examinador.
- `GameplayHUD.uss`: estil de gameplay.
- `EscapeRoomPanelSettings.asset`: configuració compartida de panell.

### Controladors

`UIToolkitMenuController` construeix dinàmicament les pantalles de menú a `screen-content`.

`GameplayUIController` controla:

- crosshair;
- prompt d'interacció;
- hotbar;
- inventari;
- selector d'objecte compatible;
- notes;
- keypad;
- examen 3D;
- subtítols;
- HUD de llanterna;
- HUD de cordura.

### Bloqueig de gameplay

Quan hi ha una modal oberta:

- s'allibera el cursor;
- es bloqueja moviment/interacció segons el controlador;
- `Esc` tanca la modal superior abans d'obrir pausa;
- els objectes físics sostinguts es deixen anar.

### Personalitzar estètica

Modifica USS per colors, marges, tipografia i estats. Mantén els `name` dels elements UXML perquè els controladors els busquen amb `Q<T>("nom")`.

Exemple segur:

```css
.menu-button {
    background-color: rgb(28, 31, 34);
    color: rgb(225, 225, 215);
    border-left-color: rgb(120, 20, 20);
}

.menu-button:hover {
    background-color: rgb(45, 48, 52);
}
```

Canviar una classe és segur. Canviar `name="screen-content"`, `name="inventory-grid"` o altres IDs exigeix actualitzar el C#.

---

## 23. Preparació per a VR

La plantilla inclou una base OpenXR funcional i independent del fabricant. Està configurada per a `Standalone` i `Android`, utilitza XRI 3.3 i manté la lògica d'Escape Room/Survival Horror compartida amb PC. La validació amb el visor final continua sent obligatòria abans de publicar un joc.

![VRTemplate executada amb XR Interaction Simulator](Documentation/Images/VRTemplate_Runtime.png)

### Inici ràpid

1. Obre `Assets/_EscapeRoomTemplate/Scenes/VRTemplate.unity`.
2. Connecta el visor i confirma que el runtime OpenXR del sistema és l'actiu.
3. Entra en Play. El rig ja conté càmera, controladors, mans visuals, haptics, interactors i locomoció.
4. Sense visor, activa temporalment `XR Interaction Simulator (enable for editor testing)` i utilitza els controls del simulador oficial d'XRI.
5. Per convertir una escena pròpia, elimina `Player_PC`, col·loca `Player_VR.prefab` i executa `Escape Room Framework > Setup > Prepare Current Scene Interactables for VR`.
6. Revisa `Project Settings > XR Plug-in Management > Project Validation` per a la plataforma de build.

No mantinguis `Player_PC` i `Player_VR` actius alhora: tots dos registren el jugador, l'equipament i l'entrada global.

### Generació i reparació automàtica

`Escape Room Framework > Setup > Build Complete VR Template`:

- assigna `OpenXRLoader` a Standalone i Android;
- crea la configuració XR persistent sota `Assets/XR`;
- reconstrueix `Player_VR.prefab` a partir del rig oficial dels Starter Assets;
- crea `VRComfortSettings.asset`;
- crea o actualitza `VRTemplate.unity` i l'afegeix al Build Profile;
- inclou dos objectes de prova: grab físic i interacció simple.

Les ordres individuals `Configure OpenXR (PC + Android)`, `Create or Update VR Player Prefab` i `Create VR Template Scene` permeten regenerar només una part.

### Contingut de Player_VR

- `XROrigin`, `CharacterController` i càmera amb tracking;
- controladors esquerre i dret amb poke, near/far i teleport ray;
- visuals de mans/controladors i feedback hàptic;
- moviment continu i teleport;
- gir snap i continu;
- providers de climb, gravity i jump aportats pels Starter Assets;
- `VRPlayerPlatformAdapter`, equipament compartit i input global;
- un `ModelSocket` sota cada controlador;
- presentador world-space per a UI Toolkit.

Per substituir les mans o afegir un objecte visual, fes-lo fill del `ModelSocket` corresponent. No eliminis el controlador, els interactors ni el socket.

### Confort i locomoció

`Assets/_EscapeRoomTemplate/Resources/VRComfortSettings.asset` exposa:

- `locomotionMode`: només teleport, només moviment continu o tots dos;
- `continuousMoveSpeed`: velocitat de desplaçament;
- `turnMode`: gir snap o continu;
- `snapTurnAmount`: graus per gir;
- `continuousTurnSpeed`: velocitat angular.

`VRComfortController` aplica el perfil als providers XRI. També és la ruta comuna perquè amagatalls, pausa o respawn bloquegin/restaurin la locomoció sense dependre de `PlayerMovement`.

### Preparació d'interactuables

L'eina afegeix:

- `XRSimpleInteractable` per a interaccions simples;
- `XRGrabInteractable` per a `PhysicsGrabbable`;
- `VRInteractionBridge` per redirigir focus i selecció al mateix `IInteractable`.

Només registra colliders propietat d'aquell interactuable. Això evita duplicar colliders de fills que pertanyen a un altre interactuable. El bridge escolta els esdeveniments `hoverEntered`, `hoverExited` i `selectEntered`; no utilitza reflexió ni polling per frame.

Exemple:

```text
Porta_Logica
├── Door                       mateixa mecànica a PC i VR
├── Collider
├── XRSimpleInteractable       selecció XRI
├── VRInteractionBridge        envia Select a Door.Interact()
└── ModelSocket
    └── ElTeuModel
```

Els `PhysicsGrabbable` reben `XRGrabInteractable`. En aquest cas XRI gestiona el moviment físic i el bridge només conserva focus/outline; no dispara una segona interacció en seleccionar.

### UI en VR

`VRUIToolkitPresenter` clona `PanelSettings` en runtime i canvia la còpia a `WorldSpace`. Els assets originals continuen sent screen-space en PC. El document se situa davant del cap i rep un collider compatible amb l'`XRUIInputModule`.

`VRUIPanelColliderController` activa el collider del menú o del document de gameplay només quan aquell document està bloquejant el joc. Això evita que un HUD transparent intercepti els rajos destinats al menú. El crosshair de PC s'oculta en VR.

La configuració final ha de validar:

- distància llegible;
- escala de text;
- interacció amb UI d'XRI;
- que el panell no segueixi el cap de manera incòmoda;
- mode assegut o dempeus;
- gir snap/smooth i locomoció del projecte.

### Controls de gameplay VR

| Acció | Control per defecte |
|---|---|
| Interactuar | botó primari esquerre o dret |
| Pausa | menú de la mà dreta |
| Inventari | menú de la mà esquerra |
| Llanterna | botó secundari dret |
| Camcorder | botó secundari esquerre |
| Visió nocturna | clic de l'stick dret |
| Recarregar camcorder | clic de l'stick esquerre |

La locomoció, teleport, gir, select i UI utilitzen `XRI Default Input Actions` dels Starter Assets. Les accions de gameplay anteriors utilitzen `EscapeRoomInputActions.inputactions`.

### Checkpoints i amagatalls

`VRPlayerPlatformAdapter.TeleportRig` mou l'origen de tracking mantenint el desplaçament físic del cap. `CheckpointManager` i `HidingSpot` detecten automàticament si el jugador és PC o VR:

- PC: congelen `PlayerMovement` i mouen el `CharacterController`;
- VR: bloquegen els providers de locomoció i mouen l'`XROrigin`;
- en sortir o fer respawn, restauren locomoció i vitals.

Els anchors d'amagatall i checkpoint representen la posició del terra del rig, no la posició exacta de la càmera.

### Limitacions que s'han de validar en hardware

- compatibilitat del perfil d'interacció amb el visor concret;
- haptics, escala i alçada real;
- inventari, keypad, scroll i focus amb les dues mans;
- llançament i Save/Load d'objectes agafats;
- rendiment URP en PCVR i standalone.

---

## 24. Substitució segura de models

Els prefabs modulars separen programació i visual:

```text
Flashlight_Modular (arrel lògica)
├── Rigidbody
├── Collider
├── EquippableItem
├── FlashlightController
├── ReplaceableModelSlot
└── ModelSocket
    ├── PlaceholderVisual
    └── ElTeuModel_Visual
```

### Mètode recomanat

1. Mantén l'arrel del prefab.
2. No eliminis scripts, IDs, collider principal ni Rigidbody.
3. Assigna el teu prefab de model a `_modelPrefab` de `ReplaceableModelSlot` o col·loca'l sota `ModelSocket` segons el prefab.
4. Ajusta posició, rotació i escala només al model/socket.
5. Desactiva o elimina únicament el placeholder visual.
6. Revisa els renderers de `SelectionOutlineTarget` si el canvi es fa en runtime.

`ReplaceableModelSlot.SetModel(modelPrefab)` també permet skins runtime. La instància rep posició i rotació local zero.

### Què no s'ha de fer

- substituir tot el prefab per un FBX;
- moure scripts al mesh;
- usar el collider detallat del model com a única lògica sense revisar física;
- canviar `SaveId`, `ItemId` o `EquipmentId` per adaptar-los al nom del model.

---

## 25. Menú superior `Escape Room Framework`

### Setup

- `Build Complete VR Template`: configura OpenXR, regenera el rig i crea l'escena de prova.
- `Configure OpenXR (PC + Android)`: assigna el loader i inicialització automàtica.
- `Instantiate Game Manager`: col·loca el prefab si no n'hi ha cap; si ja existeix, el selecciona.
- `Instantiate PC Player`: mateix comportament per al jugador PC.
- `Create or Update Main Menu Scene...`: crea o reconstrueix `MainMenu`, crea settings si falten i la posa primera al Build Profile. Reemplaçar una escena existent demana confirmació.
- `Create or Update VR Player Prefab`: regenera el rig complet a partir dels Starter Assets oficials.
- `Create VR Template Scene`: crea una escena mínima executable i un simulador opcional.
- `Prepare Current Scene Interactables for VR`: afegeix components XRI sense substituir scripts de gameplay.

### Create > Interactables

- `Door`: porta pivotant o lliscant.
- `Cabinet`: contenidor interactuable.
- `Drawer`: calaix.
- `Note`: document llegible.
- `Pickable Item`: base d'objecte d'inventari.
- `Generic Trigger`: interacció amb `UnityEvent`.
- `Item Receiver`: receptor amb selecció contextual.
- `Lever`: toggle visual per a puzles.
- `Switch`: interruptor.
- `Physics Grabbable`: objecte físic.

### Create > Puzzles

- `Keypad Panel`: panell de codi amb botons.
- `PuzzleDefinition`, `HintData` i altres dades es creen des del menú Create del Project.

### Create > Triggers

- `Narrative Trigger`: subtítol/veu o lògica narrativa.
- `Hint Zone`: activa context de pistes.

### Create > Flow

- `Objective Manager`: controlador d'objectius de l'escena.
- `Game End Trigger`: final de victòria/derrota invocable o per volum.

### Demo

- obre Main Menu;
- obre Showcase Museum;
- obre Locked Office.

Abans de canviar d'escena, Unity ofereix guardar canvis.

### Validation

- `Run Framework Smoke Tests`: integritat general d'assets i configuració.
- `Validate Current Scene`: UIDocuments, Canvas heretats, layers, Build Profile, puzles, pistes, IDs, catàleg i bateries.
- `Validate Save IDs`: duplicitats de persistència.
- `Check Render Pipeline Dependency`: dependència URP.

### Maintenance

- `Preview Missing Scripts`: selecciona problemes, no modifica res.
- `Repair Missing Scripts…`: demana confirmació, registra Undo i elimina només referències perdudes.

### Documentation

- manual curt per a dissenyadors;
- guia de programació;
- documentació completa;
- localització del HUD.

---

## 26. Escenes de demostració

### MainMenu

Demostra:

- entrada real de la build;
- nova partida i continuar;
- ranures de Save/Load;
- ajustos i rebinding;
- crèdits;
- sortida segura.

### ShowcaseMuseum

És l'aparador principal del framework. Inclou exemples de:

- interacció;
- inventari;
- combinació;
- puzles de codi, seqüència i estat;
- portes i objectes;
- llanterna i bateria;
- cordura i esdeveniment de terror;
- guardat/càrrega;
- preparació VR.

### LockedOffice

És una sala més compacta orientada a mostrar un flux d'Escape Room encadenat, amb portes, keypad, caixa forta, objectes i persistència.

Les demos són referència, no una obligació estructural. Un comprador pot crear escenes pròpies i mantenir únicament els prefabs/sistemes necessaris.

---

## 27. Crear una sala des de zero

### 1. Escena i serveis

1. Crea i desa una escena.
2. `Setup > Instantiate Game Manager`.
3. `Setup > Instantiate PC Player` o col·loca `Player_VR`.
4. Afegeix la escena al Build Profile.

### 2. Geometria

1. Crea terra, parets i llum temporal.
2. Assegura't que el jugador no apareix dins d'un collider.
3. Mantén els meshes visuals separats de les arrels lògiques.

### 3. Interacció

1. Crea una porta des del menú.
2. Configura moviment pivot/slide.
3. Si està bloquejada, assigna `requiredItemId` i `OfferCompatible`.
4. Crea notes, objectes i receptors.

### 4. Inventari

1. Crea els `InventoryItemData`.
2. Crea els prefabs mundials.
3. Afegeix els items a l'`ItemCatalog`.
4. Col·loca `PickableItem` a l'escena.

### 5. Puzles

1. Crea `PuzzleDefinition` amb ID únic.
2. Crea `HintData` i assigna'l.
3. Col·loca el controlador adequat.
4. Connecta `OnSolved` a la porta, animació o objectiu.

### 6. Final

Opció A: crea objectius i un `ObjectiveSet`; el final serà automàtic.  
Opció B: usa `GameEndTrigger` connectat a l'últim puzle.

### 7. Persistència

1. Revisa tots els `SaveId`.
2. Prova guardar abans i després de cada puzle.
3. Torna al menú principal.
4. Carrega la ranura i confirma visuals i lògica.

### 8. Validació

1. `Validate Current Scene`.
2. `Validate Save IDs`.
3. `Run Framework Smoke Tests`.
4. Prova des de `MainMenu`, no només des de l'escena directa.

---

## 28. Ampliar la plantilla amb C#

### Patró recomanat

- dades de disseny en `ScriptableObject`;
- estat runtime en `MonoBehaviour`;
- comunicació global per esdeveniments;
- connexions visuals simples per `UnityEvent`;
- implementació d'`ISaveable` només on hi ha estat persistent;
- input únicament des d'`InputRouter`;
- jugador únicament des de `PlayerPlatformRegistry`;
- UI a través dels controladors, no cercant GameObjects de Canvas.

### Exemple: objectiu personalitzat per esdeveniment propi

```csharp
public struct OnGeneratorPowered
{
    public string generatorId;
}

// Publicar
EventBus.Publish(new OnGeneratorPowered { generatorId = "basement" });

// Escoltar
private void OnEnable()
{
    EventBus.Subscribe<OnGeneratorPowered>(HandlePowered);
}

private void OnDisable()
{
    EventBus.Unsubscribe<OnGeneratorPowered>(HandlePowered);
}
```

Si vols integrar-lo al `ObjectiveManager` genèric, pots completar un objectiu manual des del handler o ampliar `ObjectiveTrigger` mantenint compatibilitat amb saves.

### Evitar acoblament

Evita:

```csharp
GameObject.Find("Player").transform...
Input.GetKeyDown(KeyCode.E)
GameObject.Find("Canvas/Inventory")...
```

Prefereix:

```csharp
PlayerPlatformRegistry.Current?.Head
InputRouter.Instance?.InteractPressed
GameplayUIController.Instance?.ToggleInventory()
```

---

## 29. Validació i publicació

### Checklist tècnic

- [ ] La build comença a `MainMenu`.
- [ ] Nova partida, continuar i càrrega manual funcionen.
- [ ] Pausa > Menú principal > confirmació funciona en una build.
- [ ] No hi ha `Canvas` heretats a les escenes finals.
- [ ] No hi ha errors ni warnings propis a Console.
- [ ] Cada escena jugable té serveis i jugador.
- [ ] `Interactable` i `Examine` existeixen com a layers.
- [ ] Tots els `SaveId`, `ItemId`, `PersistentId` i `EquipmentId` necessaris són estables.
- [ ] Cada item referenciat és a un `ItemCatalog`.
- [ ] Les bateries utilitzen l'ID que espera la llanterna.
- [ ] Cada puzle té definició i pistes si correspon.
- [ ] Guardar/carregar restaura estat i visuals.
- [ ] Els objectes recollits no reapareixen.
- [ ] PC, gamepad i VR s'han provat per separat.
- [ ] OpenXR Project Validation passa per cada plataforma.
- [ ] Els models substituïts mantenen colliders i scripts.

### Checklist comercial

- [ ] Art temporal substituït o identificat clarament.
- [ ] Fonts, icones, models i àudio tenen llicència redistribuïble.
- [ ] Crèdits actualitzats.
- [ ] Company Name, Product Name i icones de build actualitzats.
- [ ] Manual i exemples coincideixen amb la versió publicada.
- [ ] S'ha provat una instal·lació neta del package.
- [ ] Els canvis d'esquema tenen migració.

---

## 30. Resolució de problemes

### WASD no mou el jugador

- comprova que `InputRouter` existeix;
- comprova que l'asset Input Actions és a `Resources/Input`;
- comprova que el mapa `Gameplay` està habilitat;
- tanca pausa, inventari, nota, keypad o examinador;
- revisa `CharacterController` i que el jugador no estigui bloquejat per geometria;
- revisa si un rebind ha modificat Move i restableix controls.

### E no interactua

- mira si `Ajustes > Interactuar` mostra E;
- restableix controls;
- comprova la capa `Interactable`;
- comprova el LayerMask d'`InteractionManager`;
- comprova que l'objecte té collider i `IInteractable` en ell o en un pare;
- comprova que està a menys de 2,5 m per defecte;
- comprova que no tens un objecte físic sostingut o una UI bloquejant.

### “Menú principal” no canvia d'escena

- el primer clic obre confirmació;
- prem `VOLVER AL MENÚ` a la segona pantalla;
- comprova que `MainMenu` és al Build Profile;
- revisa Console per `Unity could not load scene`;
- comprova `GameFlowSettings`;
- evita llançar dues càrregues simultànies des d'altres scripts.

### El menú principal no apareix en iniciar build

- posa `MainMenu.unity` a l'índex 0 del Build Profile;
- comprova que està habilitada;
- executa `Setup > Create or Update Main Menu Scene...` només si l'escena falta o està corrupta; reconstruir reemplaça el fitxer després de confirmar.

### Apareix “Display 1 — No cameras rendering” sobre el menú

- significa que UI Toolkit funciona, però l'escena no té cap càmera activa per netejar el Display 1;
- `UIToolkitMenuController` crea automàticament una càmera mínima de fallback si no en detecta cap;
- les escenes `MainMenu` generades de nou també inclouen una `MainMenuCamera` explícita;
- no desactivis aquesta càmera tret que una altra càmera activa renderitzi el fons del menú;
- si vols un fons 3D, conserva la càmera i canvia el seu `Culling Mask`, posició i entorn en lloc d'eliminar-la.

### L'inventari no s'obre

- comprova binding d'`Inventory` (`I`);
- comprova que existeix `GameplayUIController` i el seu `UIDocument`;
- comprova que `GameplayHUD.uxml` està assignat;
- tanca altres modals o pausa;
- comprova que `InventoryManager` existeix.

### Un item no apareix després de carregar

- afegeix l'item al `ItemCatalog`;
- no canviïs l'`ItemId` després de guardar;
- comprova que `WorldPrefab` i dades no tenen Missing Reference;
- revisa si l'objecte ja estava marcat com a destruït en aquella ranura.

### Error MissingReferenceException després de recollir

- qualsevol sistema que guardi un `IInteractable` ha de comprovar `IsAlive()`;
- després d'interactuar, allibera focus si l'objecte es pot destruir;
- desregistra `ISaveable` i esdeveniments en destruir;
- no accedeixis a `gameObject` d'una interface sense la comprovació Unity-null.

### La llanterna no respon a F

- primer s'ha d'equipar;
- necessita càrrega superior a 0;
- comprova el binding `ToggleFlashlight`;
- comprova que `FlashlightController` troba el `Light`;
- per recarregar, l'inventari necessita una unitat amb `ItemId = batteries` o l'ID configurat.

### El HUD de llanterna no apareix

- el HUD és intencionadament invisible quan no està equipada;
- comprova `EquipmentController.CurrentItem`;
- comprova que `FlashlightController` està al mateix prefab equipable;
- comprova els noms `flashlight-hud`, `flashlight-fill`, `flashlight-percent` i `flashlight-state` a UXML.

### No es veu l'outline

- revisa la Renderer Feature activa;
- revisa els shaders ocults;
- comprova focus i layer de raycast;
- actualitza renderers de `SelectionOutlineTarget` després de canviar un model runtime;
- comprova que la càmera no usa un altre Renderer URP.

### Un puzle es resol però no obre la porta

- comprova el `UnityEvent OnSolved`;
- comprova que apunta a l'objecte de l'escena, no a un prefab asset;
- comprova l'ID si la reacció usa EventBus;
- en càrrega, restaura visual sense esperar que `OnSolved` torni a disparar-se.

### Save/Load no restaura un component

- implementa `ISaveable`;
- registra'l al `SaveManager`;
- `SaveId` únic i no buit;
- serialitza dades compatibles amb `JsonUtility`;
- aplica estat visual a `LoadData`;
- comprova que l'escena és al Build Profile.

### VR no pot seleccionar un objecte

- executa `Prepare Current Scene Interactables for VR`;
- comprova collider;
- comprova `XRSimpleInteractable` o `XRGrabInteractable`;
- comprova `VRInteractionBridge` i `_interactableSource`;
- revisa Interaction Layer Masks d'XRI;
- comprova que el Ray/Direct Interactor del rig està habilitat.

---

## 31. Referència ràpida d'API

### Flux

```csharp
GameFlowManager.EnsureInstance().StartNewGame();
GameFlowManager.EnsureInstance().ContinueGame();
GameFlowManager.EnsureInstance().LoadSlot("slot_1");
GameFlowManager.EnsureInstance().SetPaused(true);
GameFlowManager.EnsureInstance().ReturnToMainMenu();
GameFlowManager.EnsureInstance().CompleteGame(ending);
GameFlowManager.EnsureInstance().FailGame(ending);
```

### Guardat

```csharp
SaveManager.Instance?.SaveGame("slot_1");
SaveManager.Instance?.LoadGame("slot_1");
bool existeix = SaveManager.Instance != null && SaveManager.Instance.HasSave("slot_1");
SaveManager.Instance?.DeleteSlot("slot_1");
```

### Inventari

```csharp
InventoryManager.Instance?.AddItem(item, 1);
InventoryManager.Instance?.HasItem("item_id");
InventoryManager.Instance?.UseItem("item_id");
InventoryManager.Instance?.SetActiveQuickSlot(0);
InventoryManager.Instance?.TryCombine(0, 1);
```

### UI

```csharp
GameplayUIController.Instance?.ToggleInventory();
GameplayUIController.Instance?.ShowNote("Text del document");
GameplayUIController.Instance?.ShowKeypad(codePanelPuzzle);
GameplayUIController.Instance?.ShowItemExaminer(itemData);
GameplayUIController.Instance?.ShowSubtitle("Alguna cosa es mou darrere teu...");
UIToolkitMenuController.Instance?.ShowPause();
```

### Supervivència

```csharp
SanityController.Instance?.ApplyStress(10f);
SanityController.Instance?.Recover(5f);
horrorEventTrigger.TryTrigger();
flashlightController.Toggle();
flashlightController.TryReload();

camcorder.SetCamcorderRaised(true);
camcorder.SetNightVisionEnabled(true);
camcorder.SetZoomed(true);
bool evidenceRecorded = EvidenceJournal.Instance != null
    && EvidenceJournal.Instance.IsRecorded("anomaly_subject");
```

La base jugable de Survival Horror es divideix en documents incrementals. La càmera equipable, les bateries independents, la gravació d'evidències, els controls PC/Quest i el procediment de substitució del model estan documentats amb exemples a `Documentation/SURVIVAL_HORROR_MILESTONE_06.md`.

### Objectius

```csharp
ObjectiveManager.Instance?.CompleteObjective("objective_id");
bool complet = ObjectiveManager.Instance != null
    && ObjectiveManager.Instance.IsComplete("objective_id");
```

### Plataforma

```csharp
IPlayerPlatformAdapter player = PlayerPlatformRegistry.Current;
Transform head = player?.Head;
Transform rightHand = player?.GetHand(PlayerHand.Right);
player?.SendHaptic(PlayerHand.Right, 0.25f, 0.06f);
```

---

## Notes finals

La plantilla està dissenyada perquè el comprador construeixi contingut nou sense editar els sistemes centrals. La ruta més segura és:

1. crear dades pròpies en `ScriptableObject`;
2. instanciar prefabs lògics;
3. substituir només models visuals;
4. connectar resultats amb `UnityEvent` o EventBus;
5. donar IDs estables;
6. validar;
7. provar sempre el recorregut complet `MainMenu → Partida → Pausa → MainMenu → Càrrega → Final`.

Per a una consulta breu orientada a dissenyadors, consulta `UserManual.md`. Per a una introducció compacta a les APIs, consulta `PROGRAMMING_GUIDE.md`.
