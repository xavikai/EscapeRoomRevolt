# Fita 05 — Traversal complet, rutes d'IA i confort Quest

Data: 2 d'agost de 2026  
Unity: `6000.4.9f1`

## Resultat

La plantilla disposa ara de quatre tipus de recorregut d'escapada:

- `Vault`: saltar un obstacle baix.
- `Climb`: superar un obstacle alt.
- `Ladder`: pujar o baixar entre dues alçades.
- `Squeeze`: passar lateralment per una obertura estreta.

La lògica és compartida per PC i VR. Els models, animacions, efectes i càmera continuen desacoblats mitjançant anchors i events, de manera que es poden substituir les primitives sense perdre programació.

## Arquitectura

### `TraversalObstacle`

Cada obstacle defineix:

- tipus de recorregut;
- `EntryAnchor` i `ExitAnchor`;
- durada;
- alçada d'arc;
- `AnimationCurve`;
- prompt d'interacció;
- política de ruta de l'enemic;
- events de jugador i d'enemic per inici, final i cancel·lació.

`ResolvePath(origin, ...)` compara la distància als dos anchors. L'anchor més proper passa a ser l'entrada i l'altre la sortida. Per això la mateixa escala permet pujar i baixar i els altres obstacles funcionen des de tots dos costats.

`EvaluatePosition(...)` i `EvaluateRotation(...)` són l'única font de la trajectòria. Tant `TraversalController` com `HorrorEnemyController` les utilitzen; no hi ha dues implementacions de moviment que puguin divergir.

### Política de l'enemic

`EnemyTraversalPolicy` té tres valors:

| Valor | Comportament |
|---|---|
| `RouteAround` | L'obstacle conserva el `NavMeshObstacle`; la IA busca una ruta alternativa. |
| `UseTraversal` | El `NavMeshObstacle` es desactiva i la IA executa visualment la mateixa trajectòria. |
| `Blocked` | La IA no utilitza el link; és una ruta exclusiva del jugador. |

Exemple d'autor:

```csharp
TraversalObstacle obstacle = GetComponent<TraversalObstacle>();
Debug.Log(obstacle.EnemyPolicy);

obstacle.EnemyStarted += value => animator.SetTrigger("Traverse");
obstacle.EnemyCompleted += value => animator.ResetTrigger("Traverse");
```

Quan la IA detecta davant seu un obstacle `UseTraversal`, atura i desactiva temporalment el `NavMeshAgent`, recorre la corba de forma visible i el restaura sobre el NavMesh a la sortida. `CancelEnemyTraversal()` torna al punt segur i també s'executa en desactivar o reiniciar l'enemic.

### Jugador PC/VR

`TraversalController`:

1. desa posició i rotació segures;
2. bloqueja moviment i mirada;
3. desactiva temporalment el `CharacterController` de PC;
4. resol la direcció correcta;
5. mou el root de PC o el tracking origin de VR;
6. restaura collider i controls en completar o cancel·lar.

Si l'obstacle desapareix durant el recorregut, el controlador aborta sense conservar una referència destruïda i torna al punt segur.

## Confort per Meta Quest

`VRComfortSettings` afegeix:

```csharp
public VRTraversalMode traversalMode;              // Animated o Instant
public float traversalDurationMultiplier;          // 0.5–2.0
```

- `Animated`: reprodueix la corba de l'obstacle. El multiplicador permet fer-la més ràpida o lenta.
- `Instant`: espera un frame i mou el tracking origin directament a la sortida. És l'opció més conservadora per usuaris sensibles al moviment artificial.

La implementació és OpenXR/XRI i no depèn de Meta XR SDK. Això manté la plantilla compatible amb Quest mitjançant OpenXR i evita lligar la lògica de gameplay a un fabricant.

Configuració recomanada inicial per Quest:

1. `traversalMode = Instant` per al preset de màxim confort.
2. `traversalMode = Animated` i `traversalDurationMultiplier = 1.25` per a una opció immersiva més lenta.
3. Connectar fade, vinyeta o animació pròpia als events de l'obstacle; aquests efectes visuals no s'imposen des del framework.

## Mostres de la demo

`SurvivalHorrorDemoBuilder` crea primitives substituïbles:

| GameObject | Tipus | Política IA |
|---|---|---|
| `Traversal_Vault_Demo` | Vault | `UseTraversal` |
| `Traversal_Climb_Demo` | Climb | `RouteAround` |
| `Traversal_Ladder_Demo` | Ladder | `Blocked` |
| `Traversal_Squeeze_Demo` | Squeeze | `UseTraversal` |

La plataforma de l'escala forma part de `ENVIRONMENT_Primitives_Replaceable`. Les dues parets del pas estret són fills visuals substituïbles; el component i els anchors viuen al root estable.

## Substituir les primitives per art final

1. Mantén el GameObject root que conté `TraversalObstacle`.
2. Substitueix només els fills amb `MeshRenderer`/`MeshFilter` o afegeix el teu prefab visual com a fill.
3. Conserva o reposiciona `EntryAnchor` i `ExitAnchor` a punts segurs per als peus del jugador.
4. Ajusta collider, durada, arc i política de l'enemic.
5. Connecta els sis events a Animator, so o efectes propis.
6. Activa Gizmos i comprova les esferes cyan/verd i la trajectòria groga.

No posis el model final directament com a dependència del controlador: el root funcional ha de continuar estable perquè un canvi d'art no trenqui scripts, saves ni referències d'escena.

## Validació realitzada

- compilació: zero errors i zero warnings;
- quatre obstacles creats i serialitzats a la demo;
- resolució bidireccional verificada per als quatre tipus;
- escala: final a la plataforma, `CharacterController` i controls restaurats;
- squeeze: cancel·lació immediata, retorn amb distància `0.000` al punt segur;
- IA: detecció automàtica del squeeze, moviment visible amb agent desactivat durant el pas i restauració sobre NavMesh;
- polítiques serialitzades: dues `UseTraversal`, una `RouteAround` i una `Blocked`;
- mode Quest disponible a l'asset `Resources/VRComfortSettings.asset`.

## Pendent abans d'afirmar compatibilitat comercial Quest

No s'ha fet una prova física amb el visor. `SH-016` continua al roadmap només per QA de hardware:

- pujar i baixar l'escala amb Quest;
- provar `Animated` i `Instant` amb diferents alçades físiques;
- comprovar clipping, desplaçament del tracking origin i confort;
- provar cancel·lació, pausa i respawn dins de cada obstacle;
- verificar frame time en build Android standalone.

Fins completar aquesta matriu, la funcionalitat és tècnicament preparada per Quest/OpenXR però no s'ha de vendre com a validada en hardware.
