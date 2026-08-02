# Roadmap viu — Escape Room / Survival Horror Framework

Data de l'auditoria: 2 d'agost de 2026  
Projecte auditat: Unity `6000.4.9f1`, URP `17.4.0`, Input System `1.20.0`, XRI `3.3.0`  
Perfil actiu actual: `SurvivalHorror`

## Com utilitzar aquest document

Aquest és un backlog viu, no un historial. Quan una tasca compleixi tots els seus criteris d'acceptació i hagi estat verificada a Unity, s'ha d'eliminar de la taula corresponent. Els sistemes ja consolidats es documenten a la secció «Base funcional verificada» i no s'han de tornar a afegir al backlog si no apareix una regressió.

Regles de manteniment:

1. No marcar una tasca com a acabada només perquè compila.
2. Verificar-la almenys en una escena mínima i en la demo corresponent.
3. Afegir tests EditMode o PlayMode quan hi hagi lògica determinista o flux crític.
4. Comprovar els perfils `EscapeRoom`, `SurvivalHorror` i `CustomHybrid` quan afecti una mecànica opcional.
5. Comprovar PC, comandament i VR quan afecti entrada, UI o interacció.
6. Eliminar la fila només després de superar els criteris d'acceptació.

## 1. Resum executiu

La plantilla ja és una base funcional i bastant completa per crear Escape Rooms en primera persona. Té interacció, inventari contextual, combinació, examen 3D, puzles, pistes, objectius, finals, Save/Load i UI Toolkit. La separació per perfils de gènere funciona i evita mostrar cordura o llanterna quan no corresponen.

Com a Survival Horror ja existeix una vertical slice tècnica inspirada en el tipus de tensió d'Outlast: patrulla, percepció visual i auditiva, persecució, amagatalls, dany, mort, checkpoints i recorregut d'escapada. El treball pendent és convertir aquesta base funcional en una experiència de durada comercial, ampliar la càmera nocturna/evidències i completar QA real de VR i accessibilitat.

Veredicte actual:

| Àrea | Estat | Valoració |
|---|---|---|
| Escape Room | Funcional | Base sòlida per construir jocs complets, amb mancances de varietat i eines d'autor. |
| Survival Horror ambiental | Funcional parcial | Llanterna, cordura i esdeveniments configurables funcionen. |
| Survival Horror tipus Outlast | Vertical slice tècnica funcional | El bucle enemic–detecció–persecució–amagatall–escapada ja funciona; falta profunditat, dificultats, feedback final i una durada comercial validada. |
| PC | Funcional | Moviment, interacció, UI i controls principals disponibles. |
| VR | Base funcional | Rig XRI complet, OpenXR, locomoció configurable, mans, interactors, simulador i escena de prova disponibles; queda QA amb visors reals i paritat completa d'UI/física. |
| Preparació comercial | Parcial | Bona documentació i validació manual, però sense tests reals, localització, inventari de llicències ni separació per assemblies. |

## 2. Evidència recollida durant l'auditoria

### Mida i estructura

- 384 fitxers dins de `_EscapeRoomTemplate`.
- 93 scripts C# i aproximadament 11.028 línies de codi.
- 47 scripts de sistemes, 30 de Core, 8 de Player i 8 d'UI.
- 5 escenes de build: `MainMenu`, `ShowcaseMuseum`, `LockedOffice`, `SurvivalHorrorDemo` i `VRTemplate`.
- Prefabs específics de PC i VR, més els prefabs modulars de Survival Horror i sistemes compartits.
- No hi ha cap `asmdef` propi; tot el runtime acaba principalment a `Assembly-CSharp`.

### Escenes verificades dins Unity

| Escena | GameObjects | Components | Missing scripts | Canvas | UIDocument | Saveables | IDs buits/duplicats |
|---|---:|---:|---:|---:|---:|---:|---:|
| MainMenu | 3 | 7 | 0 | 0 | 1 | — | — |
| ShowcaseMuseum | 254 | 929 | 0 | 0 | 2 | 57 | 0 / 0 |
| LockedOffice | 93 | 333 | 0 | 0 | 2 | 32 | 0 / 0 |

Build Settings verificat:

1. `MainMenu.unity`
2. `ShowcaseMuseum.unity`
3. `LockedOffice.unity`

La Renderer Feature `Focused Interaction Outline` està instal·lada i activa al renderer URP.

### Prefabs verificats

- `GameManager.prefab`: sense scripts perduts; conté Bootstrap, inventari i els dos documents UI Toolkit.
- `Player_PC.prefab`: CharacterController, moviment, càmera, InteractionManager i equipament.
- `Flashlight_Modular.prefab`: lògica separada del `ModelSocket`, llum, collider i model substituïble.
- `Player_VR.prefab`: derivat dels Starter Assets oficials d'XRI; inclou head tracking, controladors esquerra/dreta, poke, near/far i teleport interactors, haptics, CharacterController, locomoció i `ModelSocket` per mà.
- `VRTemplate.unity`: escena mínima amb rig, XR Interaction Manager, XR UI Input Module, terra teleportable, grab, interacció simple i simulador opcional.

### Tests

Unity Test Framework detecta les suites EditMode i PlayMode, però totes dues contenen `0` tests reals. El resultat «Passed» actual només indica que la suite buida no ha fallat; no valida cap mecànica.

## 3. Base funcional verificada

### 3.1 Sistemes compartits pels dos gèneres

- Bootstrap de serveis persistents i flux de canvi d'escena.
- Entrada centralitzada amb Input System i rebinding de teclat.
- Moviment FPS amb caminar, córrer, saltar, ajupir-se i passes segons superfície.
- Interacció per raycast amb focus, prompt i outline URP.
- Dispatcher compartit entre interacció PC i ponts VR.
- Portes rotatòries o lliscants, panys, calaixos, interruptors i triggers.
- Notes, documents, subtítols i seqüències narratives.
- Manipulació d'objectes físics, llançament i sockets.
- Inventari amb quantitats, categories, accés ràpid i accions contextuals.
- Combinació d'objectes amb resultat i consum configurable.
- Examen 3D amb rotació i zoom.
- Equipament i models substituïbles mitjançant `ModelSocket`.
- Puzles de codi, seqüència, estat i socket.
- `PuzzleDefinition`, pistes progressives i penalització opcional.
- Objectius amb prerequisits i esdeveniments de final de partida.
- Menú principal, pausa, resultats, crèdits, Save/Load i ajustos amb UI Toolkit.
- Tres ranures manuals, quick save/load, metadades, captures i persistència d'entitats destruïdes.
- Perfil central `EscapeRoom`, `SurvivalHorror` o `CustomHybrid`.

### 3.2 Escape Room

El perfil Escape Room és el més complet actualment. Permet construir un recorregut jugable amb exploració, recollida de pistes, combinació d'objectes, panys, puzles, pistes progressives i final de sala. Les dues escenes jugables no tenen Canvas ni scripts perduts i els IDs persistents són únics.

### 3.3 Survival Horror disponible ara

- Llanterna equipable amb consum i recàrrega mitjançant piles de l'inventari.
- HUD de bateria condicionat al perfil i a l'equipament.
- Cordura/estabilitat persistent amb quatre estats i recuperació configurable.
- Penalització de cordura per errors de puzle.
- Esdeveniments de terror d'un sol ús o amb cooldown.
- Activació d'esdeveniments per entrada a zona, llindar de cordura o crida manual.
- Hooks mitjançant `UnityEvent`, so i subtítols.
- Soroll de gameplay per passes, sprint, portes, accions i impactes físics, amb gizmos de depuració.
- Percepció enemiga amb FOV, distància, oclusió, visibilitat contextual, memòria i investigació.
- Director de persecució amb events d'inici/final, període de gràcia i zones segures.
- Amagatalls inspeccionables per la IA i anchors separats d'entrada, sortida i inspecció.
- Portes operables per la IA segons perfil, amb bloqueig NavMesh alliberat en obrir-se.
- Checkpoints amb snapshot independent de les ranures manuals i restauració de l'estat `ISaveable`.
- Dany tipificat, fonts ambientals reutilitzables, finestra d'invulnerabilitat, retard de mort i events per feedback extern.
- Derrota sense checkpoint o respawn fiable amb recuperació definida pel preset actiu.
- Tres dificultats data-driven que modifiquen recursos, stamina, IA, dany, amagatalls, checkpoints i guardat manual.
- Selector de dificultat integrat al menú d'ajustos UI Toolkit.
- Checkpoints que restauren també activació, transform, Rigidbody i IDs destruïts d'entitats de món.
- Traversal compartit PC/VR amb vault, climb, ladder i squeeze bidireccionals, anchors, corbes, gizmos, cancel·lació segura i polítiques de ruta per enemic.
- Recorregut enemic visible sobre obstacles autoritzats, amb restauració segura del `NavMeshAgent`; alternativa per NavMesh o bloqueig configurable per obstacle.
- Confort de traversal VR configurable entre moviment animat i canvi instantani, sense dependència d'un SDK propietari de Meta.
- Càmera modular equipable, independent de la llanterna, amb pujar/baixar, zoom, visió nocturna, bateria específica i controls PC/XR rebindables.
- Evidències gravables per temps d'enquadrament, diari persistent i integració directa amb objectius data-driven.
- Evasió opcional amb lean, mirada enrere i slide, col·lisions de càmera, postura segura i controls rebindables; en Quest el lean/look-back és físic i el slide artificial està desactivat per defecte.

Aquests sistemes ja constitueixen un bucle tècnic complet, però la demo encara necessita profunditat, dificultats, feedback i QA per assolir qualitat comercial.

## 4. Problemes i riscos detectats

### 4.1 Bloquejadors comercials

1. No hi ha tests automatitzats reals.
2. No hi ha inventari de llicències, `ThirdPartyNotices` ni crèdits verificables per a àudios, fonts, icones i altres assets.
3. El README descriu una arquitectura i un roadmap antics, inclou carpetes que no existeixen i diu que els sistemes actuals encara estan pendents.
4. La promesa de VR és superior al que ofereix el prefab actual.
5. No hi ha localització: molts textos d'UI i missatges estan escrits directament en castellà dins del codi.
6. L'escriptura de Save/Load utilitza `Delete + Move`; no és atòmica si hi ha una fallada entre les dues operacions i no conserva una còpia de seguretat.

### 4.2 Ajustos que existeixen però no estan connectats completament

- `mouseSensitivity` es desa, però `GameSettingsService` no l'aplica al `PlayerMovement`.
- `musicVolume` i `sfxVolume` existeixen a les dades, però no se sincronitzen amb `AudioManager`.
- `subtitles` es desa, però no impedeix mostrar subtítols.
- `reduceFlashes` es desa, però cap esdeveniment de terror consulta aquesta preferència.
- `qualityLevel` existeix, però la UI actual no l'exposa. La configuració artística final queda fora d'aquest roadmap, però el contracte de programació sí que s'ha de completar.
- No hi ha tractament d'errors en llegir o escriure `settings.json`.

### 4.3 Deute d'arquitectura

- `GameplayUIController` té 886 línies i concentra HUD, inventari, notes, keypad, examinador, subtítols i rig de render.
- `UIToolkitMenuController` té 509 línies i construeix totes les pantalles des de codi.
- `InventoryManager` té 500 línies i combina emmagatzematge, hotbar, ús, combinació, drop, migració i equipament.
- `TemplateSceneBuilder` té 1.103 línies de generació legacy i no forma part del flux comercial actual.
- Hi ha sis scripts legacy a `UI/PC`; cinc són wrappers obsolets i `UIManager` continua com a pont de compatibilitat al GameManager.
- El moviment del jugador depèn simultàniament del `UIManager` legacy i dels controladors UI Toolkit.
- Es fan servir nombrosos singletons i cerques globals. És acceptable per a una plantilla petita, però complica tests, múltiples jugadors i càrrega additiva.
- Sense `asmdef`, els mòduls no es poden compilar, provar o distribuir de manera independent.
- No existeix una capa de localització ni una interfície de serveis injectable per a tests.

### 4.4 Problemes del controlador de jugador

- El delta del ratolí es multiplica per `Time.deltaTime`; això pot fer que la sensibilitat depengui del framerate segons el dispositiu d'entrada.
- L'ajupiment no comprova si hi ha sostre abans de tornar a l'alçada normal.
- No hi ha stamina, cansament, dany per caiguda, inclinació, mirar enrere, head bob configurable ni feedback respiratori.
- Les passes només reprodueixen àudio: no publiquen un estímul de soroll reutilitzable per a IA.
- El `SaveId` del jugador és fix (`Player`), fet que limita multijugador o rigs simultanis.

### 4.5 Deute VR restant

`VRInteractionBridge` ja funciona amb callbacks XRI de hover/select i no fa polling ni reflexió a `Update`. La locomoció utilitza l'asset d'accions oficial d'XRI i l'entrada de gameplay compartida incorpora botons XR per interacció, pausa, inventari, llanterna i camcorder.

Encara queda separar físicament la capa XR de les escenes exclusivament PC, validar tots els controls de UI Toolkit amb ambdues mans, provar Save/Load d'objectes agafats i executar QA en almenys un visor PCVR i un visor standalone.

### 4.6 Robustesa i authoring

- Els validadores actuals comproven assets bàsics, IDs i algunes dependències, però no les referències obligatòries de cada component.
- No hi ha preflight complet de build, migracions generals de saves ni recuperació d'una ranura corrupta.
- Les captures de les ranures es generen de manera asíncrona i la UI pot intentar llegir-les abans que acabin d'escriure's.
- No hi ha validació de grafs d'objectius cíclics, receptes de combinació impossibles o puzles sense sortida.
- La majoria dels inspectors són els genèrics de Unity; falta una experiència d'autor professional guiada.

## 5. Referència de disseny: Outlast

El nucli que convé prendre com a inspiració no és l'estètica ni la propietat intel·lectual, sinó la relació entre vulnerabilitat, informació limitada i mobilitat.

Les fonts oficials descriuen Outlast com una experiència sense capacitat de lluita on el jugador ha de córrer o amagar-se, amb sigil, moviments inspirats en parkour i enemics imprevisibles. El blog oficial de PlayStation, amb explicacions de Red Barrels, també destaca una IA que busca activament el jugador i un mode de dificultat amb poca salut, poques piles i sense checkpoints ni guardat manual.

Patrons aplicables a la plantilla:

1. El jugador no domina l'enemic; sobreviu llegint l'espai.
2. La IA alterna patrulla, sospita, investigació, cerca i persecució.
3. El so i la línia de visió creen informació parcial i decisions de risc.
4. Els amagatalls no són invulnerabilitat automàtica; l'enemic pot inspeccionar-los.
5. Les portes, obstacles i dreceres són eines de fugida.
6. El moviment avançat converteix la persecució en gameplay: vault, climb, squeeze i mirar enrere.
7. Una càmera amb visió nocturna transforma la foscor en gestió de recurs.
8. Gravar evidències pot alimentar el diari i la narrativa sense convertir-se en un shooter.
9. Dificultat i checkpoints poden modificar salut, recursos, agressivitat i tolerància de detecció.

Fonts:

- [Outlast — pàgina oficial de Steam publicada per Red Barrels](https://store.steampowered.com/app/238320/Outlast/)
- [Outlast Will Scare the S*** Out of You on PS4 — PlayStation Blog, Philippe Morin/Red Barrels](https://blog.playstation.com/2013/06/11/outlast-will-scare-the-s-out-of-you-on-ps4/)
- [Outlast Out Today: Survival Horror on PS4 — PlayStation Blog](https://blog.playstation.com/2014/02/04/outlast-out-today-survival-horror-on-ps4/)
- [Camcorder — referència mecànica secundària d'Outlast Wiki](https://outlastwikia.fandom.com/wiki/Camcorder)

No s'han de copiar noms, interfícies, assets, personatges, sons ni presentació visual d'Outlast. La implementació ha de ser genèrica, configurable i comercialment original.

## 6. Arquitectura objectiu dels perfils

El sistema actual de flags és adequat per a tres mòduls, però quedarà curt quan s'afegeixin IA, amagatalls, persecució, stamina, càmera nocturna i moviment avançat.

Objectiu recomanat:

- Mantenir `GenreFeatureSettings` com a punt d'entrada per al dissenyador.
- Afegir packs de funcionalitat amb dependències validades.
- Permetre activar un pack complet o característiques individuals en `CustomHybrid`.
- Afegir un `FeatureGate` per activar/desactivar arrels d'escena, prefabs i UI.
- Fer que cada mòdul exposi una interfície petita i no depengui d'una escena concreta.

Matriu objectiu:

| Mòdul | Escape Room | Survival Horror | Custom Hybrid |
|---|---:|---:|---:|
| Interacció, inventari, examen, puzles, Save/Load i finals | Sí | Sí | Configurable però recomanat |
| Pistes progressives | Sí | Opcional | Opcional |
| Llanterna | Opcional | Opcional | Opcional |
| Cordura | No per defecte | Opcional | Opcional |
| Camcorder i visió nocturna | No | Sí en preset Outlast-like | Opcional |
| Stamina i vitals | No | Sí | Opcional |
| IA hostil i percepció | No | Sí | Opcional |
| Amagatalls i persecucions | No | Sí | Opcional |
| Moviment avançat | Opcional | Sí | Opcional |
| Gore o efectes intensos | No | Opcional | Opcional |

## 7. Roadmap pendent

### P0 — Bloquejadors abans de publicar l'asset

| ID | Àrea | Implementació pendent | Criteri d'acceptació |
|---|---|---|---|
| P0-001 | QA | Crear assemblies i suites de tests reals. | Mínim 20 EditMode i 10 PlayMode cobrint inventari, combinació, puzles, IDs, perfils, Save/Load, flux de menú i UI crítica; Test Runner amb tests reals i zero errors. |
| P0-002 | Ajustos | Connectar completament `GameSettingsData`. | Sensibilitat, master/music/SFX, subtítols, reducció de destells i bindings s'apliquen, es desen, es carreguen i tenen tests; errors de fitxer no trenquen l'arrencada. |
| P0-003 | Save/Load | Fer l'escriptura realment atòmica i versionada. | Fitxer temporal, backup recuperable, `File.Replace` o equivalent segur, migrador per versió, detecció de corrupció i test d'interrupció d'escriptura. |
| P0-004 | VR | Corregir la promesa comercial de VR. | Fins que el pack VR sigui complet, documentar-lo i etiquetar-lo com a experimental; no afirmar paritat completa PC/VR. |
| P0-005 | Llicències | Crear `ThirdPartyNotices.md` i inventari d'assets. | Cada font, icona, àudio, textura, model i package té origen, autor, llicència i permís de redistribució; qualsevol asset dubtós queda substituït. |
| P0-006 | Documentació | Actualitzar README i unificar l'estat del projecte. | README, UserManual, Programming Guide, documentació completa i roadmap descriuen la mateixa arquitectura, requisits i estat real. |
| P0-007 | Localització | Eliminar textos de runtime codificats directament. | Taules o catàleg localitzable, idioma de fallback i castellà/anglès de mostra; UXML, menús, prompts, objectius, pistes i errors no depenen de literals C#. |
| P0-008 | Validació | Ampliar el validador comercial. | Detecta referències obligatòries null, IDs, cicles d'objectius, receptes trencades, scenes de build, UXML contract, perfils, XR incomplet i assets sense llicència registrada. |

### P1 — Vertical slice Survival Horror tipus Outlast

| ID | Sistema | Implementació pendent | Criteri d'acceptació |
|---|---|---|---|
| SH-006 | Amagatalls | Crear lockers, llits i contenidors genèrics. | Entrada/sortida, càmera, bloqueig de moviment, respiració, ocupació, IA que pot inspeccionar i compatibilitat PC/VR. |
| SH-012 | Demo | Crear una escena vertical slice de Survival Horror original. | Inclou exploració, objectiu, recurs de visió, enemic, soroll, persecució, amagatall, checkpoint i final; recorregut de 10–15 minuts. |

### P2 — Càmera nocturna i moviment d'escapada

| ID | Sistema | Implementació pendent | Criteri d'acceptació |
|---|---|---|---|
| SH-015 | Presentació NV | Connectar l'art final de visió nocturna als hooks existents. | Perfil URP, soroll/degradació i feedback visual respecten reducció de destells; la lògica de consum, estats, fallback i events ja està implementada. |
| SH-016 | Traversal | Tancar QA de traversal en Meta Quest. | Les quatre variants, els dos modes de confort i la cancel·lació es verifiquen en visor; sense mareig greu, clipping ni pèrdua de tracking origin. |
| SH-018 | Portes | Afegir obertura lenta, peek i slam. | Interacció analògica o per fases, soroll diferent, compatibilitat amb portes actuals i ús per IA. |
| SH-019 | Director de tensió | Crear un director opcional de ritme. | Cooldowns, pressupost d'esdeveniments, zones segures i hooks; evita repetir ensurts i no substitueix l'autor de nivell. |
| SH-020 | Accessibilitat horror | Crear controls d'intensitat. | Reducció de flaixos, tremolor, sorolls forts, gore, head bob i chase assistance aplicats realment. |

### P2 — Profunditat específica d'Escape Room

| ID | Sistema | Implementació pendent | Criteri d'acceptació |
|---|---|---|---|
| ER-001 | Graf de puzles | Crear una vista d'autor de dependències i estat. | Mostra prerequisits, sortides, cicles, bloquejos i objectius; valida que una sala tingui almenys una ruta de solució. |
| ER-002 | Kits de puzle | Afegir dials/safe, cables, canonades, símbols i lliscant. | Cada kit és data-driven, té prefab de primitives, Save/Load, pistes, reset i exemple documentat. |
| ER-003 | Examen | Afegir hotspots i secrets a l'examen 3D. | Punts clicables, canvi de prompt, revelació d'informació o item i persistència; PC/VR. |
| ER-004 | Quadern | Crear un casebook amb notes, evidències, objectius i pistes. | Cerca/filtre, novetats, estat persistent i integració amb documents i evidències gravades. |
| ER-005 | Variants | Permetre solucions aleatòries amb seed persistent. | Codi, seqüència o distribució poden variar per partida sense crear estats impossibles; seed al save. |
| ER-006 | Multi-stage | Crear un controlador de puzle per fases. | Fases ordenades o ramificades, feedback per fase, rollback opcional i Save/Load en qualsevol punt. |
| ER-007 | Multi-room | Crear un graf de sales i transicions. | Portes/portals poden carregar additivament o canviar escena preservant estat, spawn point i objectius. |
| ER-008 | Accessibilitat | Afegir alternatives de puzle i ajuda contextual. | Modes de contrast, no dependència exclusiva del color/so, temps ampliable, pistes graduables i reset segur. |

### P2 — VR funcional i publicable

| ID | Sistema | Implementació pendent | Criteri d'acceptació |
|---|---|---|---|
| VR-004 | UI Toolkit | Fer la UI world-space realment interactiva. | Punter XR, focus, scroll, botons, teclat numèric i inventari verificats amb ambdues mans. |
| VR-005 | Física | Garantir paritat de grab, socket i equipament. | Agafar, deixar, llançar, sockets, dues mans opcionals i Save/Load sense duplicats. |
| VR-006 | Feature gating | Separar la capa XR de les escenes PC. | En PC no s'executa cap bridge ni interactable XR; en VR s'activen sense duplicar la lògica de gameplay. |
| VR-007 | QA hardware | Crear matriu de dispositius i proves. | OpenXR validat almenys en un visor PCVR i un standalone objectiu; locomoció, UI, haptics, guardat i rendiment documentats. |

### P3 — Arquitectura, mantenibilitat i experiència de comprador

| ID | Àrea | Implementació pendent | Criteri d'acceptació |
|---|---|---|---|
| ARC-001 | Assemblies | Separar Core, sistemes, UI, Editor, tests i XR amb `asmdef`. | Dependències unidireccionals, XR opcional i temps de recompilació reduït; tests poden referenciar runtime sense internals accidentals. |
| ARC-002 | UI | Dividir els dos controladors UI Toolkit grans. | Presenters per HUD, inventari, examen, documents, keypad, menú i saves; callbacks registrats/desregistrats simètricament. |
| ARC-003 | Legacy | Eliminar wrappers obsolets i generadors destructius. | Cap script runtime depèn de `UI/PC` legacy; migració documentada; `TemplateSceneBuilder` eliminat o traslladat a Samples/Legacy. |
| ARC-004 | Serveis | Reduir singletons i cerques globals als límits del framework. | Context o registry injectable, lifecycle explícit, càrrega additiva segura i tests sense escenes completes. |
| ARC-005 | Inventari | Separar storage, quick access, recipes, use/drop i equipment. | APIs petites, tests per mòdul, migració de saves i mateixa UX actual sense regressions. |
| ARC-006 | IDs | Crear IDs persistents editor-only i immutables. | GUID generat una vegada, duplicats bloquejats abans de build i migracions documentades quan canvia un ID publicat. |
| ARC-007 | Authoring | Crear inspectors i wizards professionals. | Previews, help boxes, validació inline, botons de prova i creació segura per puzles, items, esdeveniments, IA i perfils. |
| ARC-008 | Rendiment | Definir pressupostos i escenes de benchmark. | CPU, GC, draw calls i memòria mesurats en PC i VR; zero allocations sostingudes als loops principals. |
| ARC-009 | CI | Afegir validació automàtica del repositori. | Compilació, tests, IDs, meta files, YAML, documentació i llicències verificats en cada canvi. |
| ARC-010 | Samples | Separar codi distribuïble i contingut de mostra. | Runtime/Editor nets, samples importables, dependències opcionals clares i export de package repetible. |

## 8. Ordre recomanat d'implementació

1. `P0-001` a `P0-008`: impedir que el deute creixi mentre s'afegeixen mecàniques.
2. `SH-006` i `SH-012`: completar QA VR d'amagatalls i durada de la vertical slice.
3. `SH-015`, `SH-016` i `SH-018` a `SH-020`: tancar presentació/QA de hardware i ampliar portes, tensió i accessibilitat.
4. `ER-001` a `ER-008`: ampliar varietat i qualitat d'autor per Escape Room.
5. `VR-004` a `VR-007`: completar UI, física, feature gating i QA de hardware sobre el rig funcional.
6. `ARC-001` a `ARC-010`: es poden intercalar, però han d'estar resolts abans de publicar la versió comercial final.

La propera fita funcional recomanada és `SH-018`: obertura lenta, peek i slam de portes amb soroll diferenciat i ús per la IA. En paral·lel, `SH-006`, `SH-012` i `SH-016` continuen requerint validació de contingut o hardware. La base tècnica d'enemic, soroll, persecució, amagatall, stamina, checkpoint, càmera nocturna, traversal i evasió ja existeix.

## 9. Fora d'abast intencionat

Seguint la decisió del projecte, l'autoria final d'àudio, mescla, il·luminació, postprocessat, materials i qualitat gràfica correspon al propietari de la plantilla. El roadmap sí que inclou les APIs, events, opcions i punts d'integració necessaris perquè aquests continguts es puguin connectar sense modificar el codi base.
