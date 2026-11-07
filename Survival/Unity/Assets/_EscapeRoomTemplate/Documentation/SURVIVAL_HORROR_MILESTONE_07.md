# Fita 07 — Evasió avançada i seguretat de càmera

Data: 2 d'agost de 2026  
Unity: `6000.4.9f1`

## Resultat

La plantilla incorpora `SH-017`: un mòdul opcional d'evasió per a Survival Horror amb inclinació lateral, mirada ràpida enrere i slide. El mòdul no existeix al perfil Escape Room i es pot activar individualment en un perfil `CustomHybrid` mitjançant `AdvancedEvasion`.

La implementació evita tres problemes habituals en controladors FPS comercials:

1. inclinar la càmera a través d'una paret;
2. recuperar l'alçada dempeus sota un sostre baix;
3. deixar el moviment bloquejat després d'una mort, traversal o cancel·lació.

## Controls per defecte

| Acció | Teclat | Comandament | VR Quest |
|---|---|---|---|
| Inclinar-se | mantenir `Alt esquerre` + `A/D` | sense binding dedicat | moviment físic del cap |
| Mirar enrere | `X` | clic de l'estic esquerre | gir físic del cap |
| Slide | `V` mentre corres endavant | `B/Cercle` mentre corres endavant | desactivat per defecte |

Els tres controls de PC apareixen a `Ajustes > Controles` només quan `AdvancedEvasion` està actiu. Els bindings es desen amb la resta de controls a `GameSettingsData.bindingOverridesJson`.

L'ús d'un modificador per inclinar-se és intencionat: evita reutilitzar `E`, que continua reservada per interactuar, i impedeix que el jugador es desplaci lateralment mentre està fent lean.

## Arquitectura

### `OptionalGameFeature.AdvancedEvasion`

`GenreFeatureSettings` aplica el mòdul així:

- `EscapeRoom`: desactivat;
- `SurvivalHorror`: activat;
- `CustomHybrid`: decisió de l'autor.

`Bootstrapper` afegeix `EvasionController` al root del jugador només quan el flag està actiu. No cal modificar `Player_PC.prefab` ni `Player_VR.prefab`, i una escena Escape Room no paga cap cost d'Update del sistema.

### `EvasionController`

Responsabilitats:

- llegir les accions `LeanModifier`, `LookBack` i `Slide`;
- aplicar la presentació de càmera després del moviment normal;
- limitar el lean amb un `SphereCastNonAlloc` que ignora els colliders del jugador;
- moure el `CharacterController` durant el slide;
- emetre un estímul `GameplayNoiseType.PlayerAction`;
- cancel·lar-se de forma segura per mort, respawn, UI o traversal;
- exposar events i una API petita per accessibilitat, tests o inputs personalitzats.

API pública principal:

```csharp
EvasionController evasion = GetComponent<EvasionController>();

bool started = evasion.TryStartSlide(transform.forward);
evasion.CancelSlide();

evasion.SetLeanOverride(-1f);   // -1 esquerra, +1 dreta
evasion.ClearLeanOverride();

evasion.SetLookBackOverride(true);
evasion.ClearLookBackOverride();

evasion.SlideStarted += HandleSlideStarted;
evasion.SlideCompleted += HandleSlideCompleted;
evasion.SlideCancelled += HandleSlideCancelled;
```

Els overrides són útils per a accessibilitat, controls tàctils o un sistema d'input alternatiu. S'anul·len visualment mentre una UI bloqueja el gameplay, el jugador està mort/amagar o hi ha un traversal actiu.

## Col·lisions i postura segura

### Lean

La càmera demana un desplaçament lateral màxim, però abans de moure's calcula la distància lliure amb una esfera. Si hi ha una paret, el valor final es redueix de forma contínua. Això evita clipping sense moure el capsule del jugador ni alterar la seva posició de gameplay.

Paràmetres configurables a `EvasionController`:

- `Lean Distance`: distància lateral màxima;
- `Lean Roll`: rotació visual màxima;
- `Lean Response`: velocitat d'entrada/sortida;
- `Lean Collision Radius`: volum de seguretat de la càmera.

### Slide

El slide només comença quan:

- el component i el feature són actius;
- el `CharacterController` està actiu i a terra;
- no hi ha mort, amagatall, traversal ni UI bloquejant;
- en l'input estàndard de PC, el jugador corre i manté una entrada endavant suficient.

Durant el moviment, `CharacterController.Move` conserva les col·lisions normals. En finalitzar, `PlayerMovement` intenta recuperar la postura dempeus. `CanStand()` comprova el capsule complet amb `Physics.OverlapCapsuleNonAlloc`; si hi ha un sostre, el jugador continua ajupit fins que hi hagi espai.

El crouch del slide té propietat independent del crouch forçat dels amagatalls. Això evita que acabar un slide tregui el jugador d'un llit, locker o conducte.

## Meta Quest i confort

En VR no s'aplica cap rotació artificial al cap per inclinar-se o mirar enrere: aquestes accions provenen del tracking físic. És la decisió més segura per Quest i evita conflictes amb l'`XROrigin`.

`VRComfortSettings` afegeix:

```csharp
public bool allowArtificialSlide;                 // false per defecte
public float artificialSlideSpeedMultiplier;     // 0.75 per defecte
```

Només quan `allowArtificialSlide` és `true`, una crida a `TryStartSlide` pot moure el rig VR. Durant el slide es bloquegen temporalment els providers de locomoció XRI i es restauren en acabar. Aquesta opció s'ha de validar en Quest real abans de publicar-la activa.

## Integració visual i d'àudio

La fita implementa lògica i hooks, no art final. Un comprador pot connectar animació, FOV, head bob, so o vibració als events de slide sense modificar el controlador. Per mantenir accessibilitat, els efectes visuals intensos haurien de consultar `GameSettingsData.reduceFlashes` i disposar d'intensitat configurable.

## Validació realitzada

Prova en Play Mode sobre `SurvivalHorrorDemo.unity`:

- perfil actiu `SurvivalHorror` i `AdvancedEvasion` actiu;
- `Bootstrapper` crea un únic `EvasionController` actiu;
- les tres accions existeixen al `InputRouter` runtime;
- lean lliure arriba a `0.32 m` i `8°` de roll;
- mirar enrere arriba a `165°`;
- un obstacle lateral limita el lean a `0.04 m` en la prova;
- el slide comença ajupit i amb moviment normal bloquejat;
- recorre aproximadament `2.12 m` amb els valors per defecte;
- finalitza recuperant moviment i postura;
- un obstacle superior manté el capsule a `1.0 m` fins que torna a haver-hi espai;
- `EscapeRoomFeatures` no conté `AdvancedEvasion`;
- el perfil Quest té slide artificial desactivat per defecte;
- zero errors o warnings a la consola durant la prova.

La validació de confort del slide artificial en visor continua dins `VR-007`; aquesta fita no afirma QA de hardware.

## Checklist per a una escena nova

1. Selecciona `SurvivalHorror` o activa `AdvancedEvasion` en `CustomHybrid`.
2. Assegura't que el jugador utilitza `CharacterController` i `PlayerMovement` o `VRPlayerPlatformAdapter`.
3. Mantén un `Bootstrapper` a l'escena.
4. Ajusta les distàncies del controlador si canvies radi o alçada del jugador.
5. Prova lean al costat de murs, cantonades i portes mòbils.
6. Prova slide sota obstacles i confirma que no pot aixecar-se dins de geometria.
7. Prova cancel·lació amb pausa, mort, amagatall i traversal.
8. En Quest, deixa `allowArtificialSlide` desactivat fins superar QA en hardware.
