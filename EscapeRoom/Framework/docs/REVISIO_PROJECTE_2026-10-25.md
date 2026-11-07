# Revisió del projecte i proposta de separació

Data: 25 d'octubre de 2026. Base revisada: commit `2b2625e`.

> Aquest informe conserva la revisió inicial. La preparació autoritzada posteriorment i el seu estat actual es documenten a [SEPARACIO_PROJECTES.md](SEPARACIO_PROJECTES.md).

Revisió estàtica del codi, configuració, escenes serialitzades, tests i documentació. No s'han executat Unity, tests, builds ni proves en visor en aquesta revisió. Les verificacions de setembre continuen sent evidència històrica, no resultats nous.

## Recomanació

Separar Escape Room i Survival en dos projectes Unity de consum, amb una base comuna versionada. Primer cal preparar els límits dels mòduls al repositori actual; després crear els dos projectes. Aquesta és una proposta pendent de decisió, no una migració ja autoritzada o implementada.

La separació de producte ja ha començat: `ReleaseBuilder.WindowsScenes` inclou Intro, MainMenu, ShowcaseMuseum i LockedOffice, i `QuestScenes` només ShowcaseMuseumVR. Survival queda fora de les escenes d'aquestes releases, però no s'ha separat el codi ni tot el contingut distribuïble. En particular, seleccionar escenes no elimina automàticament els assets de `Resources`.

| Part | Contingut proposat |
|---|---|
| Base comuna | Interacció, inventari, combinació, examen, guardat, localització, àudio, objectius, finals i contractes d'entrada/jugador. |
| Escape Room | Puzles, pistes, autoria de sales, museu, oficina, perills i temporitzador opcionals. |
| Opcions reutilitzables | Llanterna, portes avançades i altres components que tinguin ús propi fora del survival; dependències explícites. |
| Survival | Vida/estamina, dany, IA, persecució, amagatalls, cordura, càmera nocturna, evidències, checkpoints i director de tensió. |
| Integració XR | Jugador, interacció i presentació VR separats del jugador PC; usable en qualsevol dels dos productes. |

El Survival pot consumir puzles quan els necessiti. L'Escape Room ha de poder compilar sense el mòdul Survival. Separar PC/VR és una altra dimensió: no cal crear quatre productes per separar els dos gèneres.

Dos repositoris copiats i mantinguts manualment serien una separació ràpida, però cada correcció comuna s'hauria de traslladar dues vegades. Recomano començar amb mòduls dins del repositori actual i acabar amb paquets locals o Git versionats, sense imposar encara nous repositoris. Els projectes de consum haurien de fixar una versió concreta de la base comuna.

## Dependències que bloquegen una separació per carpetes

| Evidència actual | Conseqüència i actuació proposada |
|---|---|
| `Core/Bootstrap/Bootstrapper.cs` crea serveis Survival i afegeix `CameraShakeController` i `SurvivalHUDController` sense comprovar el perfil. | Extreure la instal·lació Survival i el seu HUD a una composició pròpia. Decidir si camera shake pertany a la base o a un complement. Bootstrap és fora de `Core/Runtime`: el nom de la carpeta no el converteix en part de l'assembly Core. |
| `GameplayUIController` consulta `SanityController` i `FlashlightController`; `UIToolkitMenuController` consulta i crea `SurvivalDifficultyService`. | Separar el HUD específic i aportar opcions de dificultat/guardat mitjançant contractes o extensions. Ocultar controls amb un perfil no elimina dependències de compilació. |
| `PuzzleController` crida `SanityController.ApplyStress`. | Convertir la penalització en un esdeveniment o extensió que Survival pugui escoltar. |
| `PickableItem` afegeix i consulta `CheckpointEntity`/`CheckpointManager`. | Desacoblar la recollida d'objectes de la persistència específica de checkpoints. |
| `PhysicsGrabbable` exigeix `GameplayImpactNoiseEmitter`, físicament dins de Survival. | Moure l'emissor a una capa compartida o connectar-lo com a complement. |
| `GameplayNoise` ja viu a `Core/Runtime`, però conserva el namespace `Systems.Survival`. | Distingir namespace i ubicació real abans de moure o eliminar fitxers: `Door` usa aquest contracte compartit i això no demostra una dependència del mòdul Survival. |
| Un sol `EscapeRoomRevolt.Systems.asmdef` conté els dos gèneres. | Crear assemblies de gameplay compartit i Survival amb dependències en una sola direcció. Separar també els scripts d'Editor que els referencien. |
| `Player.asmdef` i `Systems.asmdef` referencien XRI; el smoke validator exigeix recursos VR. | Fer XR opcional requereix assemblies, eines i validacions per plataforma; eliminar els paquets XR avui trencaria la compilació. |
| Builders i validadors usen rutes literals `Assets/_EscapeRoomTemplate/...`. | Adaptar les rutes i la descoberta d'assets abans d'una extracció a `Packages`. |

En moure assets, cal conservar els `.meta` i GUIDs. No convé canviar simultàniament estructura, namespaces i dades serialitzades.

## Configuració que convé revisar

- El perfil global és `EscapeRoom` i l'escena inicial és `ShowcaseMuseum`. `SurvivalHorrorDemo` continua habilitada als Build Settings de desenvolupament, que tenen **vuit** escenes. El builder de Survival modifica el perfil i l'escena inicial globals. Cada producte necessita configuració pròpia perquè generar una demo no reconfiguri l'altre.
- `GameFeatures` combina amb OR tots els `SceneFeatureOverride` registrats. En càrrega additiva, una excepció d'una escena afecta el conjunt de la sessió, no exclusivament aquella escena. Cal definir si aquest és el comportament desitjat i documentar-lo.
- Els museus PC i VR habiliten la llanterna amb `_enabledFeatures: 1`. Per tant, separar gèneres no hauria d'eliminar aquesta opció de l'Escape Room.
- `SaveManager` desa sota `Application.persistentDataPath/SaveSlots` i els ajustos també usen `persistentDataPath`. El builder fixa el nom i identificador d'Escape Room. Els dos productes necessiten identitats i ubicacions de dades diferenciades; la compatibilitat de partides entre productes no està acreditada.

## Documentació que cal actualitzar

1. **ROADMAP:** encara presenta sis escenes, inventaris d'agost i una fila P0-001 amb «12 EditMode / 0 PlayMode». Contradiu les revisions posteriors i la suite actual. Separar resultats històrics del backlog vigent i resumir les llargues narracions de tasques acabades.
2. **Tests:** el recompte estàtic actual és **22 casos EditMode** (7 EventBus + 7 localització + 8 recuperació, comptant els dos TestCase) i **33 PlayMode** (14 + 9 + 5 + 5). README cita 20/20 EditMode com a resultat del 29 de setembre; no es pot substituir per 22/22 sense executar la suite. CHANGELOG i README diuen sis regressions a `SaveRestoreRegressionTests`, però el fitxer declara cinc tests. Cal reconciliar les afirmacions amb un nou informe del Test Runner.
3. **Preparació comercial:** `COMMERCIAL_READINESS.md` està datat el 9 de setembre i encara descriu només ES/EN i paritat dels museus anterior a Linked Lights. Actualitzar-lo amb els canvis posteriors, mantenint pendents la revisió humana dels idiomes i el QA real.
4. **Guardat:** P0-003b parla d'afegir un migrador quan aparegui una segona versió, però `SaveGameData.version` ja és 3 i el lector accepta 1–3. Acceptar un número antic no acredita una migració: documentar compatibilitat i provar-la amb fitxers representatius.
5. **Abast del producte:** README, guia completa, guia de programació i guia interactiva encara presenten un framework conjunt. Quan es decideixi la separació, donar a cada producte un inici ràpid, matriu de funcions, backlog i notes de versió; conservar una referència comuna d'API.
6. **Museu VR:** el README manté pendent Room 14/Linked Lights a VR. Confirmar aquesta paritat amb inspecció funcional i reflectir el resultat en totes les guies.

Les auditories d'agost i setembre s'han de conservar com a històriques, sense reescriure els seus resultats amb dades d'avui.

## Ordre d'execució proposat

1. Executar i guardar una línia base de tests i validacions de les demos actuals; fer el recorregut de referència amb Save/Load.
2. Tancar l'abast de cada producte i actualitzar la documentació viva, incloent la decisió sobre opcions compartides.
3. Desacoblar Bootstrap, HUD/menú, penalització de puzles, checkpoints de pickables i emissió de soroll. Crear límits d'assembly i extensions petites segons cada dependència.
4. Verificar que l'Escape Room compila i funciona sense el mòdul Survival en una còpia de prova. Comprovar llanterna, puzles, inventari, final, reintentar i guardat/càrrega; després comprovar el Survival consumint la base comuna.
5. Extreure els paquets i preparar els dos projectes Unity amb configuració, recursos, demos, builds i dades de jugador propis. Adaptar builders i validadors a les noves ubicacions.
6. Provar importació neta i builds de cada producte/plataforma. Retirar MCP només de les distribucions i acabar QA en dispositiu.

La separació es considerarà completada quan una còpia neta de cada producte pugui importar, compilar, passar les seves proves i generar la demo corresponent amb només les dependències declarades.

## Pendents que continuen oberts

La separació no resol per si sola el recorregut humà complet dels executables, QA i rendiment en visor, procedència dels cinc àudios marcats pendents a `ThirdPartyNotices.md`, ni els límits documentats de persistència: `PhysicsSocket`, recompenses després de reset i retorn a sales amb càrrega Single. S'han de prioritzar segons l'abast anunciat de cada entrega.

## Canvis d'aquesta revisió

Només s'ha afegit aquest informe i un enllaç al README. No s'ha separat el projecte ni modificat codi, escenes, paquets o configuració.
