# Auditoria de tancament — plantilla Escape Room

Data: 9 d'agost de 2026  
Actualització de tancament: 10 d'agost de 2026  
Actualització PC/VR i release beta: 13 d'agost de 2026
Projecte: Unity 6000.4.9f1, URP 17.4.0  
Abast: eines d'autoria, menú principal, flux de build, persistència, nomenclatura, documentació i totes les sales de les escenes incloses.

## Veredicte executiu

La base d'Escape Room és funcional: els vuit puzles originals instal·lats al museu es poden resoldre i l'ampliació incorpora dos exemples avançats (grup de puzles simultanis i rodets numèrics amb botons físics), un perill mòbil multidireccional i un temporitzador HUD independent. `ShowcaseMuseum` i `ShowcaseMuseumVR` contenen la mateixa lògica a les sales 11 i 13. Totes les escenes estan lliures de scripts perduts i els IDs persistents comprovats són únics. El menú principal cobreix nova partida, continuar, tres ranures manuals, ajustos, controls, crèdits, pausa i sortida.

Els tres bloquejadors específics detectats a la mostra Escape Room ja estan resolts:

1. `ThrowPuzzleController` i `MelodyPuzzleController` tenen `PuzzleDefinition` i `HintData`; els fills coordinats per `MultiStagePuzzle` continuen sent `PuzzleController` independents amb identitat pròpia.
2. El puzle de canonades de la sala 10 té tres respostes persistents a `OnSolved`: desbloqueja i obre `PipeExitDoor_Logic` i activa `PipeSolvedBeacon`.
3. La cobertura PlayMode arriba a 14 tests reals, tots passant, amb puzles originals, grups encadenats visibles, rodets —inclòs el bloqueig per enfocament, el pas bidireccional compartit pels botons PC/VR i la normalització dinàmica de 2–8 rodes—, perill mòbil en qualsevol direcció, temporitzador HUD independent, menú principal i round-trip Save/Load en memòria.

També continua pendent el bloquejador transversal de llicències: cal confirmar l'origen dels àudios i completar `ThirdPartyNotices.md` abans de distribuir l'asset.

## Evidència i metodologia

- MCP de Unity connectat a l'Editor real, no només lectura de YAML.
- Validació estructural de les sis escenes del Build Profile.
- Inspecció de jerarquia, components, referències serialitzades, `UnityEvent`, IDs i assets de configuració.
- Execució manual en Play Mode dels vuit puzles del museu.
- Prova de les 27 accions de creació principals del menú d'autoria.
- Execució del validador comercial, smoke tests i Unity Test Runner.
- Revisió del flux `Intro → MainMenu → Nova partida` i de totes les pantalles del menú.

Resultats mesurats:

| Prova | Resultat |
|---|---|
| Smoke tests del framework | PASS, 0 avisos |
| EditMode | 12/12 PASS (`EventBus` i `LocalizationCatalog`) |
| PlayMode automatitzat | 14/14 PASS (puzles/mecàniques, menú principal i Save/Load) |
| Escenes amb missing scripts o prefabs trencats | 0/6 |
| Puzles del museu resolts en Play Mode | 8/8 |
| Accions de creació provades | 27/27 després de corregir `Keypad Panel` |
| IDs de desat duplicats al museu | 0 |

L'únic soroll extern observat és OpenXR quan l'ordinador no té un runtime XR actiu. No és un error de lògica de la plantilla, però la promesa VR continua requerint QA amb visor real.

## Flux de build i menú principal

Ordre verificat del Build Profile:

1. `Intro`
2. `MainMenu`
3. `ShowcaseMuseum`
4. `LockedOffice`
5. `SurvivalHorrorDemo`
6. `VRTemplate`

Pantalles i opcions verificades:

- principal: continuar, nova partida, carregar, ajustos, crèdits i sortir;
- pausa: reprendre, guardar, carregar, ajustos, tornar al menú i sortir;
- guardat/càrrega: tres ranures manuals, previsualització, eliminació i quick save/load;
- ajustos: volums, sensibilitat, qualitat, pantalla completa, accessibilitat i reassignació de controls;
- localització: selector `Idioma` amb castellà i anglès quan el servei està actiu;
- dificultat i controls de Survival: només apareixen quan el perfil habilita les funcions corresponents.

Incidències corregides durant l'auditoria:

- `GameFlowSettings` apuntava `Nova partida` a `SurvivalHorrorDemo` tot i tenir el perfil `EscapeRoom`; ara apunta a `ShowcaseMuseum`.
- `MainMenu` no creava `LocalizationService` quan s'executava directament des de la build; ara el crea després de `GameSettingsService` i el selector d'idioma apareix correctament.
- regenerar `MainMenu` la col·locava sempre a l'índex 0 i podia saltar-se una `Intro` ja habilitada; ara conserva `Intro` al davant i col·loca `MainMenu` a continuació.
- `Create > Puzzles > Keypad Panel` llançava una excepció per `RectTransform` i intentava escriure un camp d'outline obsolet; ara crea 11 botons, 12 textos, 12 targets d'outline i cap error.
- Les eines `IntroSceneSetup` i `LogicVisualsMigrator` utilitzaven overloads marcats obsolets a Unity 6.4; s'han substituït pels equivalents actuals i la recompilació queda a zero errors i zero avisos.

## Auditoria escena per escena

| Escena | Estructura | Resultat | Pendent |
|---|---:|---|---|
| `Intro` | 3 objectes | Seqüència i càrrega de menú cablejades | Substituir el pas d'imatge buit pel logo o contingut comercial final |
| `MainMenu` | 3 objectes, 1 `UIDocument` | Flux i serveis correctes després de les correccions | Assignar tema/crèdits finals si es publica |
| `ShowcaseMuseum` | 321+ objectes, 2 `UIDocument`, 82+ saveables | Definicions, payoff i nomenclatura corregits; 14/14 PlayMode | Homogeneïtzar la segona peça de Placement i fer playthrough de build |
| `LockedOffice` | 93 objectes, 2 `UIDocument` | 2 teclats amb definició i noms semàntics | Cal un playthrough humà complet de principi a fi |
| `SurvivalHorrorDemo` | 96 objectes, 2 `UIDocument` | Vertical slice estructuralment vàlida | Cinc `Placeholder_ReplaceMe` i QA de durada; fora del tancament Escape Room |
| `VRTemplate` | 76 objectes, 2 `UIDocument` | Rig i adaptadors assignats | QA OpenXR en hardware real |

Cap de les sis escenes té scripts perduts ni referències de prefab trencades segons `manage_scene validate`.

## ShowcaseMuseum, sala per sala

### Sala 1 — Portes i contenidors

- Calaix, armari i porta amb clau utilitzen el mateix component `Door` amb moviments i bloqueig diferents.
- La consolidació és bona: no calen controladors separats per cada moble.
- La clau `Key_Office` i el pany estan cablejats.
- Els noms `New*` i `Cube` s'han substituït per noms semàntics; reagrupar alguns roots antics sota un únic node de sala continua sent una millora d'ordre, no un bloquejador funcional.

### Sala 2 — Nota i codi

- Nota, teclat i caixa forta estan presents.
- `CodePanelPuzzle` resol correctament el codi de quatre símbols/dígits i obre la caixa forta amb feedback de càmera.
- La definició és `DemoCodePanelPuzzle`.
- En contingut final, la pista de la nota ha de ser l'única font fiable del codi; no convé imprimir la solució al costat del teclat.

### Sala 3 — Receptors i combinació

- `ItemReceiver` exigeix `Key_SecretRoom` i activa la porta secreta.
- La combinació llanterna buida + bateries usa `PhysicsSocket` amb `batteries` i té tres respostes connectades.
- Recomanació: mantenir separats el flux d'inventari i el físic, però fer-los compartir una regla d'acceptació d'ítems per evitar duplicar IDs i missatges.

### Sala 4 — Seqüència

- Botons vermell, verd i blau cablejats a `SequencePuzzle`.
- Ordre correcte verificat: vermell → verd → blau.
- En resoldre, desbloqueja i obre la porta.
- Les tres zones de pistes estan agrupades i reanomenades com `Room04_SequenceHints`; ja no arrosseguen numeració antiga.

### Sala 5 — Interruptors

- `StatePuzzle` observa sis condicions entre palanques i interruptors.
- Es resol amb els sis estats correctes i té porta i so de resposta.
- És un bon exemple de reutilització de `SteppedPositioner`; un dial, una vàlvula o un selector analògic haurien de ser presets visuals d'aquest sistema, no nous controladors.

### Sala 6 — Física

- Tres `ThrowTarget` i diversos objectes llançables.
- Els IDs `target1`, `target2` i `target3` coincideixen i el puzle es resol en encertar-los.
- Obre `PuzzleDoor_Throw` i reprodueix so.
- Té `Def_demo_throw_puzzle` amb tres pistes progressives.

### Sala 7 — Col·locació

- Dues peces, dos sockets vàlids i un socket esquer.
- La variant aleatòria es manté determinista durant la partida; resol en qualsevol dels mapatges vàlids generats.
- Obre porta i reprodueix so.
- Inconsistència d'autoria: una peça té `ReplaceableModelSlot` i l'altra no. Cal homogeneïtzar-les abans d'oferir l'escena com a referència.

### Sala 8 — Lliscant

- Graella 3×3, vuit fitxes, forat i `SlidingBoardView` cablejats.
- Es resol amb moviments legals i restaura l'estat complet.
- Els vuit prompts s'han unificat a `Moure peça`; el constructor també genera aquest text per no reintroduir la inconsistència.
- Els nous inspectors i el botó `Rebuild board` milloren correctament l'autoria, però aquests canvis encara són modificacions locals pendents d'integrar.

### Sala 9 — Melodia

- La pista reprodueix C, A, D, B i els quatre botons introdueixen exactament aquesta seqüència.
- Reutilitza `SequencePuzzle`, que és la decisió correcta: la melodia és una presentació diferent del mateix verb d'ordre.
- Obre la porta i reprodueix so.
- Té `Def_demo_melody_puzzle` amb tres pistes progressives.

### Sala 10 — Canonades

- Dos segments rotables, definició `Def_demo_pipe_puzzle` i comprovació de connectivitat funcional.
- La cerca de camí resol el puzle correctament.
- `OnSolved` té tres listeners persistents: desbloqueja i obre `PipeExitDoor_Logic` i activa `PipeSolvedBeacon`.

### Sala 11 — Puzles encadenats

- `MultiStagePuzzle` coordina una llista ampliable de puzles fills independents; tots es mantenen actius i visibles a l'habitació.
- El preset mostra la seqüència a l'esquerra i les palanques a la dreta. La sala està configurada amb ordre obligatori: el segon conjunt es veu des del principi, però no accepta interacció fins que es resol el primer.
- El component permet desactivar `_requireOrder` perquè els fills es resolguin lliurement, o afegir tantes entrades `ChainedPuzzle` com calgui.
- La porta i la balisa verda escolten només l'`OnSolved` del coordinador, que no s'emet fins que tots els fills estan resolts.

### Sala 12 — Perill mòbil i temporitzador independents

- `MovingHazard` configurat com a sostre descendent entre marcadors verticals, amb cos cinemàtic i trigger letal. El mateix component admet qualsevol direcció 3D i, per tant, també parets, terres, plataformes o aigua.
- `GameOverTimer` separat, configurat a 25 segons, desable i visible al HUD compartit; perd la partida en arribar a zero sense dependre del moviment.
- Cada mecànica té un botó d'inici propi. No hi ha `TimedGameOverHazard` ni display de compte enrere al món dins la sala.
- El menú d'autoria ofereix dos configuradors diferents: `Moving Hazard (Any Direction)` i `Game Over Timer (HUD)`.

### Sala 13 — Rodets numèrics

- El preset del museu usa quatre `SteppedPositioner` de deu posicions, amb `NumberWheelView` per mostrar les xifres, però l'autoria ja és dinàmica entre 2 i 8 rodes.
- `Create > Puzzles > Number Wheels Puzzle` obre un configurador de nombre de rodes i combinació. `NumberWheelsPuzzleAuthoring` permet canviar-los després i reconstruir carcassa, separació, títol, condicions i càmera sense perdre `PuzzleDefinition`, pistes ni conseqüències `OnSolved`.
- El candau està reduït al 48% i muntat a la paret al costat de la porta: a la sala té escala de maleta/candau, però la càmera enfocada continua mostrant-lo gran i centrat.
- En PC les rodetes no accepten interacció des de la vista general. Cal entrar amb `Examinar combinació` i clicar ▲/▼ damunt o sota de cada rodet, amb retorn circular entre 0 i 9. No s'utilitzen W/S ni les fletxes del teclat.
- `PuzzleFocusPoint` obre una vista centrada del model 3D real. En VR no es força aquesta càmera: el panell genera controls ▲/▼ equivalents. PC i VR acaben cridant el mateix `TryStep(+1/-1)`. La carcassa i cada rodet tenen `ReplaceableModelSlot`, de manera que es poden adaptar a una maleta, caixa forta o candau sense duplicar la lògica.
- `StatePuzzle` comprova la combinació 3142; no duplica la lògica de solver.
- Té `PuzzleDefinition`, tres pistes, porta i balisa verda connectades a `OnSolved`.

## Matriu de puzles del museu

| Puzle | Definició | Prova en viu | Resposta `OnSolved` | Estat |
|---|---|---|---|---|
| Codi | `DemoCodePanelPuzzle` | PASS | caixa forta + càmera | Tancat |
| Seqüència | `DemoSequencePuzzle` | PASS | porta | Tancat |
| Estat | `DemoStatePuzzle` | PASS | porta + so | Tancat |
| Llançament | `Def_demo_throw_puzzle` | PASS automatitzat | porta + so | Tancat |
| Col·locació | `Def_demo_placement_puzzle` | PASS | porta + so | Homogeneïtzar slots visuals |
| Lliscant | `Def_demo_sliding_puzzle` | PASS automatitzat | llum + porta | Tancat |
| Melodia | `Def_demo_melody_puzzle` | PASS automatitzat (`SequencePuzzle`) | porta + so | Tancat |
| Canonades | `Def_demo_pipe_puzzle` | PASS automatitzat | porta + balisa | Tancat |
| Puzles encadenats | `Def_demo_multistage_puzzle` | PASS automatitzat (ordre lliure i obligatori) | porta + llum | Tancat |
| Rodets numèrics | `Def_demo_number_wheels_puzzle` | PASS automatitzat | porta + llum | Tancat |

## Eines d'autoria i menú superior

Les 27 accions principals provades creen objectes sense missing scripts ni referències d'objecte trencades després de la correcció del teclat. Hi ha, però, dos límits de producte que la documentació ha d'explicar sense ambigüitat:

- `Multi-Stage Puzzle` crea un grup jugable de fills simultàniament visibles, amb llista ampliable i ordre opcional. `Number Wheels Puzzle` crea un candau decimal amb botons ▲/▼ compartits entre PC i VR reutilitzant `StatePuzzle`. `Pipe Puzzle` continua creant principalment controlador i dades d'exemple.
- Els creadors genèrics de puzles deixen `PuzzleDefinition` buit perquè no poden inventar el contingut final. El fallback de nom permet jugar, però cal assignar definició i pistes abans de publicar. Les sales 11 i 13 sí que les generen perquè són mostres tancades.

Millora d'autoria recomanada: afegir a cada creador un diàleg opcional «Crear definició i feedback» que generi un `PuzzleDefinition`, un `HintData` inicial i un preset de resposta. Això tanca el forat entre «es pot crear» i «passa el validador comercial».

## Nomenclatura i organització

Convenció recomanada:

- arrel funcional: `<Sala>_<Funcio>_Logic`;
- representació substituïble: `<Funcio>_Visuals` o `ModelSocket`;
- agrupació: `Room_01_Doors`, `Room_02_Code`, etc.;
- IDs persistents: minúscules amb guió baix, estables i independents del nom del GameObject;
- assets: `Def_<nivell>_<puzle>`, `Hint_<nivell>_<puzle>`, `Item_<id>`.

Migració executada:

- `LockedOffice` no conté cap nom `New*`; portes, mobles, ítems, receptors, notes i triggers tenen prefix `Office` i numeració estable.
- `ShowcaseMuseum` no conté arrels `Cube` ni noms `New*`; l'arquitectura blockout, portes, palanques, interruptors i visuals tenen noms semàntics.
- Les sales visibles 2–10 s'han renumerat internament (`Room02_…` fins a `Room10_…`) sense modificar `SaveId` ni `PersistentId`.
- `PlaceholderVisual` es conserva únicament quan identifica deliberadament un slot d'art substituïble, no com a nom accidental d'escena.

## Solapaments i millores de mecàniques

### Consolidacions que ja estan ben resoltes

- `Door` cobreix porta, armari i calaix; mantenir un sol component.
- Melodia reutilitza `SequencePuzzle`; mantenir la separació entre solver i presentació.
- Palanques, interruptors, vàlvules i tubs reutilitzen `SteppedPositioner`; crear presets, no subclasses noves.
- `PuzzleController` centralitza estat, save, hints i events; qualsevol mecànica nova ha d'heretar-lo o compondre'n un de ja existent.

### Solapaments que convé aclarir, no fusionar a cegues

- `SocketPuzzle` i `ItemReceiver`: tots dos accepten un item d'inventari. `ItemReceiver` és millor per autoria contextual; `SocketPuzzle` només aporta el cicle de vida formal de puzle. Recomanació: marcar `SocketPuzzle` com a adaptador avançat/legacy o fer que `ItemReceiver` pugui notificar un `PuzzleController`, en lloc de mantenir dos fluxos gairebé idèntics.
- `PhysicsSocket` i `ItemReceiver`: han de compartir regles d'acceptació i missatges, però no semàntica. Un treballa amb un Rigidbody físic i l'altre amb inventari.
- `MultiStagePuzzle` i objectius: el primer és estat intern d'un puzle; els objectius governen progressió de nivell. Mantenir-los separats.

### Millores de màxima rendibilitat

1. **Condicions compostes per `StatePuzzle`**: afegir adaptadors `WeightZoneCondition`, `LightBeamCondition` i `ProximityCondition`. Permetrien plaques de pressió, balances, llum i sensors sense crear tres nous solvers.
2. **Presentació genèrica de xarxes**: extreure la connectivitat de `PipePuzzle` a un model visual configurable per canonades, cables o circuits. Mateix BFS, diversos skins.
3. **Preset de feedback**: porta, llum, so, càmera i missatge configurables des d'un sol asset reutilitzable. Evita que una sala, com la 10, quedi resolta però visualment penjada.
4. **Validador de sala**: comprovar definició, hints, almenys un feedback, prompts en un sol idioma, noms `New*`/`Cube` i roots fora del grup de sala.
5. **PlayMode tests de contracte**: un test per cada tipus de puzle, un per nova partida/retorn al menú i un per save/load d'un puzle a mig resoldre.

## Criteri de tancament proposat

Abans de congelar la part Escape Room:

- [x] Crear i assignar definicions per Throw i Melody, amb `HintData` si la mostra promet pistes.
- [x] Afegir payoff persistent al Pipe Puzzle.
- [ ] Homogeneïtzar `ReplaceableModelSlot` de les dues peces de Placement.
- [x] Unificar prompts del Sliding Puzzle.
- [x] Renumerar les tretze sales sense canviar IDs persistents.
- [x] Reanomenar `New*` i `Cube` després d'eliminar fallbacks per nom.
- [x] Ampliar els PlayMode tests reals fins a un mínim de 10, prioritzant puzles, menú i Save/Load.
- [ ] Acabar d'agrupar alguns roots antics de les sales 1–5 sota nodes semàntics de sala.
- [ ] Fer un playthrough humà complet de `LockedOffice` i `ShowcaseMuseum` des d'una build neta.
- [ ] Confirmar llicències i completar `ThirdPartyNotices.md`.

Amb aquests punts resolts, la plantilla Escape Room es pot congelar amb risc baix i el desenvolupament pot concentrar-se en Survival Horror sense arrossegar deute d'autoria evident.
