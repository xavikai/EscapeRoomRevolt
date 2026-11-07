# Primera fase: desacoblament Escape Room / Survival

> Document històric de la primera fase. La segona fase ja ha creat els dos projectes i els paquets compartits: consulta [Projectes independents](PROJECTES_INDEPENDENTS.md) per a l'estat actual, les rutes i la validació.

Actualització: 25 d'octubre de 2026. Primera fase: desacoblament dins del repositori existent.

## Estat i abast

La base comuna, l'Escape Room i les opcions reutilitzables ja no referencien l'assembly Survival. Survival i la seva presentació tenen assemblies pròpies que depenen de la base. Les escenes i les configuracions de producte continuen al projecte Unity actual: encara no hi ha dos projectes de consum ni paquets UPM distribuïbles.

S'han conservat els noms públics, namespaces i GUID dels scripts moguts per mantenir els enllaços a escenes, prefabs i assets. Per compatibilitat, alguns tipus compartits encara tenen el namespace `EscapeRoomRevolt.Systems.Survival`: això no significa que depenguin de l'assembly `EscapeRoomRevolt.Survival`.

## Propietat dels mòduls

| Assembly / ubicació | Responsabilitat i dependències |
|---|---|
| `EscapeRoomRevolt.Core` / `Core/Runtime` | Serveis i contractes comuns, entrada, guardat, localització, flux i punts d'extensió `GameModules`. No referencia Survival. |
| `EscapeRoomRevolt.Player` / `Player` | Jugadors PC/VR i adaptadors. Depèn de Core i dels paquets d'entrada/XR actuals. |
| `EscapeRoomRevolt.Systems` / `Systems`, excepte subassemblies | Inventari, interacció, puzles, pistes, equipament i opcions compartides. Depèn de Core/Player, no de Survival. |
| `EscapeRoomRevolt.UI` / `UI` | Menú i HUD compartit, inventari, examen, notes i llanterna. No referencia Survival. |
| `EscapeRoomRevolt.Bootstrap` / `Core/Bootstrap` | Composició de serveis compartits i crida als mòduls opcionals. Depèn de Core/Player/Systems. |
| `EscapeRoomRevolt.Survival` / `Systems/Survival` | Vida, estamina, dany, IA, amagatalls, cordura, checkpoints, evidències, càmera nocturna i tensió. Depèn de Core/Player/Systems. |
| `EscapeRoomRevolt.Survival.UI` / `Systems/Survival/UI` | HUD Survival, cordura i instal·lació opcional de la presentació. Depèn de Core/Survival/UI. |
| `EscapeRoomRevolt.Survival.Editor` / `Systems/Survival/Editor` | Constructor de demo i creador d'amagatalls. Només Editor; depèn de Core/Player/Systems/Survival i AI Navigation. |

Els tests comuns i els de Survival també estan separats. Les eines comunes d'Editor continuen a `Assembly-CSharp-Editor`; l'extracció a paquets haurà de donar-los una assembly pròpia.

## Contractes i inicialització

`GameModules` és un registre de mòduls opcionals; la base no cerca classes Survival per nom ni per reflexió. El registre es reinicia a `SubsystemRegistration`, incloent sessions amb domain reload desactivat. Els mòduls inclosos es registren a `BeforeSceneLoad` i la mateixa instància no es registra dues vegades.

- `EnsureServices()`: el menú i Bootstrap demanen els serveis opcionals. Survival crea dificultat només si el perfil habilita vida/estamina.
- `InitializeScene(playerRoot)`: Bootstrap demana als mòduls instal·lar els components específics després dels serveis comuns. Cada mòdul ha de respectar els flags i evitar duplicats; no ha de dependre de l'ordre de registre d'altres mòduls. Els HUD vinculen els serveis després de la inicialització de l'escena.
- `PreparePickup(pickup)`: Survival afegeix `CheckpointEntity` quan correspon. `PickableItem` només coneix el contracte `IPickupRemovalHandler`; si cap component assumeix la retirada, aplica la seva opció habitual de destruir/desactivar.
- `ApplyPuzzleStress(amount)`: els puzles comuniquen la penalització configurada. Sense mòdul receptor no passa res; Survival l'aplica a cordura només si la funció està habilitada.
- `IGameDifficultyProvider`: el menú consulta opcions amb ID/nom i selecciona dificultat sense conèixer `SurvivalDifficultyProfile`. La llanterna consulta el multiplicador de consum, amb valor neutre `1` quan no hi ha proveïdor. El servei elimina el seu registre i allibera el bloqueig de guardat quan es destrueix.

El perfil Escape Room no instal·la serveis Survival a través d'aquests hooks. Els components Survival ja col·locats explícitament en una escena continuen sent responsabilitat de l'escena i dels seus flags.

## Fitxers moguts

A `Systems/Options`, dins de l'assembly compartida:

- `FlashlightController`, `FlashlightBeamAttenuator`, `FlashlightVolumetricBeam`.
- `CameraShakeController` i `ReplaceableModelSlot`.
- `GameplayImpactNoiseEmitter`, `GameplayNoiseEmitter`, `GameplayNoiseDebugVisualizer`.

`GameplayNoise` ja era a Core i no s'ha mogut. Les portes i els objectes físics poden continuar emetent estímuls sense carregar un mòdul d'IA.

`SurvivalHUDController` s'ha mogut de `UI/Toolkit` a `Systems/Survival/UI`; la cordura s'ha extret de `GameplayUIController` a `SanityHUDController`. La llanterna continua al HUD compartit. L'UXML/USS encara conté els elements de Survival, ocults per defecte: separar-ne els assets visuals queda per a l'empaquetat.

`SurvivalHorrorDemoBuilder` ara és a `Systems/Survival/Editor`. La comanda d'amagatalls conserva la ruta del menú, amb implementació a `SurvivalInteractableCreator.CreateHidingSpot()` en lloc d'`InteractableCreator.CreateHidingSpot()`.

## Validació

Compilació batch amb Unity `6000.4.9f1`: correcta. EditMode: **25/25**, sense fallades ni omissions. Inclou tres tests nous del registre opcional, desregistre i consum neutre de la llanterna.

La suite PlayMode afegeix quatre regressions Survival: perfil Escape Room sense serveis específics, retirada/restauració d'un objecte via contracte compartit, fallback sense gestor de checkpoints i penalització de cordura a través del registre. Resultat: **37/37 PlayMode**, sense fallades ni omissions, el 25/10/2026 en batch sense gràfics.

Compilar les assemblies comunes sense cap referència a Survival impedeix dependències de tipus en aquesta direcció. Això no substitueix provar una distribució neta sense els assets de Survival, ni una partida completa, ni el QA de VR físic.

El nou validador `Escape Room Framework > Validation > Validate Product Module Boundaries` ha comprovat **5 assemblies compartides, 8 escenes i 13 prefabs** sense dependències de codi cap a Survival, scripts absents ni referències serialitzades trencades detectades. També s'han comprovat els GUID dels **10 scripts moguts**, conservats, i l'absència de GUID duplicats dins de la plantilla. La inspecció de referències no valida tota configuració semàntica ni les rutes escrites com a text.

[Resultats exportats d'aquesta fase](validation/separation-2026-10-25.json). Els XML complets i logs locals són a `.utmp/separation-editmode.*`, `.utmp/separation-playmode.*` i `.utmp/separation-validation.log`; no formen part de la distribució. El validador de mòduls es pot executar també amb `-executeMethod EscapeRoomRevolt.EditorTools.ProductModuleValidator.ValidateBatch`.

## Fases següents

1. Crear les dues composicions de producte: perfils, escena inicial, catàlegs, identitats d'aplicació i ubicacions de guardat pròpies. El builder Survival encara modifica configuració global i el release builder continua sent d'Escape Room.
2. Fer inventari de referències serialitzades i de `Resources`, incloent dades Survival, prefabs, àudios, entrada i localització. Seleccionar escenes de build no és suficient per excloure tots els recursos.
3. Separar Runtime, Editor, Tests i Samples en paquets. Resoldre les rutes literals `Assets/_EscapeRoomTemplate/...` dels constructors, validadors i fulls d'estil abans de moure'ls a `Packages`.
4. Preparar els dos projectes Unity que consumeixin versions fixades de la base comuna i només els mòduls necessaris. Verificar importació i compilació d'Escape Room sense Survival i de Survival amb la base comuna.
5. Separar la integració XR de Player/Systems si es vol oferir una instal·lació PC sense XRI. En aquesta fase XR continua sent una dependència necessària.
6. Validar builds i recorreguts complets de cada producte/plataforma; tancar els pendents comercials existents, com procedència d'àudio, importació neta i hardware real.

No eliminar encara tots els assets Survival de la còpia de desenvolupament: les demos, Resources i configuracions de producte encara no s'han extret.
