# Manual d'usuari - Escape Room Framework

**Idioma:** [castellano](UserManual.md) · català · [English](UserManual.en.md)

**Comença aquí:** [guia pràctica d'Escape Room PC/VR](Documentation/ESCAPE_ROOM_QUICKSTART.ca.md), les [receptes pas a pas de cada mecànica](Documentation/RECEPTES.md), desament, preparació VR i matriu d'acceptació. Consulta [l'auditoria del 9 de setembre de 2026](AUDITORIA_ESCAPE_ROOM_2026-09-09.md) per als resultats actuals; l'informe d'agost és històric.

Aquesta guia cobreix el flux de treball per a dissenyadors. L'arquitectura i les API estan documentades a [PROGRAMMING_GUIDE.md](PROGRAMMING_GUIDE.md). La referència exhaustiva, amb tutorials, exemples i resolució de problemes, és a [DOCUMENTACIO_COMPLETA.md](DOCUMENTACIO_COMPLETA.md). El recorregut històric sala per sala és a [AUDITORIA_ESCAPE_ROOM_2026-08-09.md](AUDITORIA_ESCAPE_ROOM_2026-08-09.md). Per aprendre la plantilla de manera pràctica, obre la guia interactiva [`docs/guia/index.html`](../../docs/guia/index.html) (català, castellà i anglès).

## 1. Menú del framework

Totes les eines compatibles són a `Escape Room Framework`:

- `Configuration`: selecciona el perfil Escape Room, Survival Horror o una combinació personalitzada.
- `Setup`: instal·la instàncies segures del Game Manager o del jugador i genera les escenes/prefabs de plataforma.
- `Create`: crea interactuables, puzles, hotspots d'examen, triggers i components de flux sense modificar altres objectes. `Multi-Stage Puzzle` crea un grup de dos puzles físicament separats i visibles; la llista admet tants puzles com calgui i permet acabar-los en ordre lliure o obligatori abans d'activar una única porta o mecanisme. `Number Wheels Puzzle` obre un configurador per triar entre 2 i 8 rodes i definir la combinació. Després es pot redimensionar des de `NumberWheelsPuzzleAuthoring > Rebuild wheels and layout`; la carcassa, el títol, les condicions, els botons i la càmera s'adapten sense perdre la definició ni els esdeveniments del puzle. A PC s'obre la vista enfocada i es fa clic amb el botó esquerre sobre ▲/▼; `E` només obre el panell o actua com a alternativa d'interacció. A VR s'utilitzen els controls ▲/▼ equivalents amb el gallet. A `Create > Flow`, `Moving Hazard (Any Direction)` crea per separat una paret, un sostre, un terra, una plataforma o un volum mòbil entre dos marcadors 3D, mentre que `Game Over Timer (HUD)` crea un límit de temps opcional visible a la interfície. `Pipe Puzzle` encara requereix completar-ne la presentació interactiva.
- Tant `MovingHazard.StartHazard` com `GameOverTimer.StartTimer` es poden connectar des de l'Inspector a un interruptor (`InteractableTrigger`) o a una zona de pas (`EventTriggerZone`). La zona admet filtre per tag, mode d'un sol ús, esdeveniments d'entrada/sortida i rearmament mitjançant `ResetZone`.
- `Demo`: obre les escenes d'exemple després d'oferir desar els canvis actuals. `Apply Escape Room Closure Fixes` torna a aplicar de manera idempotent definicions, payoff de Pipe, prompts i nomenclatura semàntica a `ShowcaseMuseum` i `LockedOffice`.
- `Validation`: comprova IDs, dependències, escena activa i preparació comercial.
- `Maintenance`: previsualitza problemes abans de permetre una reparació amb Undo.
- `Documentation`: obre aquest manual, la guia de programació, la documentació completa o localitza el HUD d'UI Toolkit.

Els antics generadors destructius i la instal·lació automàtica de paquets ja no formen part del menú.

### Triar el gènere del projecte

- `Configuration/Use Escape Room Profile`: manté interacció, inventari, puzles, pistes, objectius, finals, Save/Load, PC i VR. Desactiva i amaga la llanterna, la bateria, l'estabilitat/seny i els esdeveniments de terror.
- `Configuration/Use Survival Horror Profile`: activa totes les mecàniques comunes i també la llanterna, el seny i els esdeveniments de terror.
- `Configuration/Use Custom Hybrid Profile`: permet escollir per separat `Flashlight`, `Sanity` i `Horror Events` a `GenreFeatureSettings.asset`.

Les escenes de demostració `ShowcaseMuseum` i `ShowcaseMuseumVR` inclouen una excepció local per a la llanterna, perquè la sala 3 demostra la combinació d'una llanterna buida amb bateries. Per això la llanterna funciona en aquestes escenes fins i tot amb el perfil `Escape Room`; les escenes noves continuen respectant el perfil i no activen la llanterna tret que s'utilitzi `Custom Hybrid`.

El canvi s'aplica en tornar a iniciar Play. Els components opcionals poden continuar presents a escenes i prefabs: el perfil evita que s'executin o apareguin a la UI quan no correspon.

## 2. Escena jugable

1. Obre o crea una escena.
2. Utilitza `Setup/Instantiate Game Manager`.
3. Utilitza `Setup/Instantiate PC Player` o col·loca `Player_VR`.
4. Crea interactuables des de `Create/Interactables` i configura'n els camps a l'Inspector.
5. Executa `Validation/Validate Current Scene`.

El model visual dels prefabs reemplaçables es troba sota un `ModelSocket`. Substitueix únicament els seus fills visuals per conservar colliders, IDs, esdeveniments i programació.

## 3. Menú inicial i final del joc

Utilitza `Setup/Create or Update Main Menu Scene` per generar el menú inicial. Queda en primer lloc a Build Settings, tret que ja existeixi una `Intro` habilitada: aleshores es conserva l'ordre `Intro → MainMenu`.

`Nueva partida` (Nova partida) carrega l'escena indicada per `Resources/GameFlowSettings.asset`. En el perfil de mostra Escape Room ha d'apuntar a `ShowcaseMuseum`; canvia-la explícitament quan comencis el joc definitiu.

Per acabar una partida pots:

- crear un `Objective Set` i assignar-lo a un `ObjectiveManager`;
- crear `Create/Flow/Game End Trigger` i connectar-lo a un puzle o volum;
- cridar `GameFlowManager.CompleteGame` o `FailGame` des del codi.

La pantalla final permet tornar-ho a intentar, tornar al menú principal o sortir.

## 4. Personalitzar l'aspecte i els textos del menú

### Canviar colors, fonts i logotip

1. Al panell de Projecte, botó dret → `Create > Escape Room Framework > Menu Theme Settings`. Posa-li un nom, per exemple `MiTemaDeMenu`.
2. A l'Inspector del nou asset, ajusta els colors (fons del panell, accent, títol, botons) i, si vols, arrossega una font ja importada (`.ttf`/`.otf`) a `Title Font`/`Body Font` i una imatge a `Logo`.
3. A `MainMenu.unity`, selecciona `MainMenuUI`. En una escena jugable, selecciona `MenuUI`, dins del `GameManager`. Tots dos tenen el component `UI Toolkit Menu Controller`; arrossega el teu asset al seu camp `_theme`.
4. Si tens diverses escenes jugables amb instàncies independents del `GameManager`, assigna el mateix asset a cadascuna o al prefab compartit.
5. Opcional: escriu a `Main Menu Title` el nom del teu joc i a `Credits Text` els crèdits (autors, assignatura, llicències). Si els deixes buits, es mantenen els textos de la plantilla.
6. Entra a Play — el menú ja utilitza la teva paleta, tipografies, logotip i textos. Si no assignes res, el menú conserva el disseny original de la plantilla.

Si prefereixes editar directament el fitxer d'estils en lloc de crear un asset, `EscapeRoomMenu.uss` té els colors més repetits com a variables al principi del fitxer (`--color-accent`, `--color-text`...), de manera que canviar la paleta base és editar unes quantes línies en lloc de buscar cada color solt.

El mateix jugador pot activar un mode d'alt contrast des de la Configuració; aquest mode sempre té prioritat sobre el teu tema, perquè l'accessibilitat no depengui mai de la personalització visual.

### Dissenyar els botons a partir d'imatges

Pots utilitzar imatges pròpies per al fons dels botons. En la versió actual, `Menu Theme Settings` controla els colors, les fonts i el logotip, però les imatges dels botons s'assignen des d'`EscapeRoomMenu.uss`.

Prepara en una carpeta pròpia, per exemple `Assets/UI/Menu/`, una imatge per a cada estat:

- `ButtonNormal.png`: estat normal;
- `ButtonHover.png`: en passar el ratolí per sobre;
- `ButtonPressed.png`: mentre es prem;
- opcionalment, `ButtonDisabled.png`: botó desactivat.

Recomanacions:

- utilitza PNG amb transparència quan calgui;
- conserva la mateixa proporció en totes les variants;
- no dibuixis el text dins de la imatge: el text el genera el menú i es pot canviar mitjançant el catàleg de localització;
- importa les imatges com a `Sprite (2D and UI)`;
- si la imatge té un marc que ha de conservar les cantonades en canviar de mida, prepara-la per a 9-slice.

Per assignar-les:

1. Obre `Assets/_EscapeRoomTemplate/UI/Toolkit/EscapeRoomMenu.uss` amb UI Builder.
2. Selecciona el selector `.menu-button` i assigna la imatge normal a **Background > Image**.
3. Selecciona o crea `.menu-button:hover` i assigna la imatge hover.
4. Selecciona o crea `.menu-button:active` i assigna la imatge premuda.
5. Si tens botons que es poden desactivar, configura també `.menu-button:disabled`.

És preferible assignar les imatges des d'UI Builder perquè Unity escrigui correctament les referències dels assets. Si el botó conserva un color per sota de la imatge, posa el color de fons del `Menu Theme Settings` amb alfa 0 o utilitza un fons opac a la mateixa imatge.

Mantén el text separat de la imatge. Així continuaran funcionant les traduccions i el mode d'alt contrast.

### Canviar els textos que apareixen i els idiomes

Tots els textos que veu el jugador passen per un únic catàleg editable sense tocar codi: menús, HUD, prompts d'interacció (`[E] Abrir armario`), panell VR, subtítols, pistes, objectius, noms i descripcions d'objectes, notes i els cartells 3D de les escenes. El catàleg inclou **castellà (`es`), anglès (`en`) i català (`ca`)**; el jugador tria l'idioma a la Configuració.

La regla és senzilla: **escriu els textos en castellà** a l'Inspector (prompts, noms d'objectes, pistes, cartells) i afegeix-ne la traducció al catàleg.

1. Selecciona `Assets/_EscapeRoomTemplate/Resources/DefaultLocalizationCatalog.asset`.
2. Cada entrada té una clau (el text castellà exacte, per exemple `"Abrir armario"`) i una fila per idioma.
3. Per a un text nou, afegeix una entrada amb la clau idèntica al text de l'Inspector (majúscules, accents i salts de línia inclosos) i les files `es`, `en` i `ca`.
4. Per afegir un idioma, afegeix files amb el seu codi (`fr`, `it`...). Apareix automàticament al desplegable d'idioma de la Configuració.

Si una clau no existeix, es mostra el text tal qual, de manera que un text sense traduir no desapareix mai: només no canvia d'idioma. Els cartells 3D (TextMeshPro o TextMesh) es tradueixen sols si el seu text és una clau del catàleg (`SceneTextLocalizer`); els textos que un script canvia en temps d'execució no es toquen.

## 5. Inventari

L'inventari s'obre amb `I` a PC. L'emmagatzematge ja no està limitat per la barra ràpida.

- Selecciona un objecte per veure només les accions vàlides: llegir, sostenir/equipar, consumir, examinar, combinar o deixar anar.
- `ACCESO RÁPIDO N` (ACCÉS RÀPID N) assigna l'objecte a la posició ràpida activa.
- Les tecles `1-4`, la roda del ratolí o els botons d'espatlla del comandament canvien l'accés ràpid.
- En interactuar amb un pany en mode `Offer Compatible`, la interfície mostra únicament objectes vàlids. No n'utilitza cap sense confirmació.

Cada porta o receptor pot canviar la seva política a `Selected Only` o `Auto Use Single` des de l'Inspector.

### Examinar un objecte en 3D

La plantilla permet inspeccionar en 3D un objecte que ja és a l'inventari:

1. Obre l'inventari amb `I`.
2. Selecciona l'objecte.
3. Prem `EXAMINAR`.
4. Arrossega sobre la imatge de l'objecte per fer-lo girar.
5. Utilitza la roda del ratolí per apropar o allunyar la vista.
6. Prem `ESC` o `CERRAR` (TANCAR) per tornar a l'inventari.

Perquè el botó aparegui actiu, l'`InventoryItemData` ha de tenir:

- un `WorldPrefab` assignat;
- `Can Examine` activat.

El model que apareix és una còpia visual temporal: examinar-lo no elimina ni modifica l'objecte real de l'inventari. Si l'objecte té `ExamineHotspot`, el jugador pot passar el cursor per sobre d'aquesta zona per veure una pista i fer clic per revelar-la. Els hotspots poden concedir un altre objecte, llançar un esdeveniment o mostrar una descripció, i el seu estat es conserva amb les partides desades.

Les notes llegibles (`Is Readable`) utilitzen el lector de text de l'inventari i no necessiten un model 3D. A VR, el mateix panell es presenta com a UI 3D i utilitza els esdeveniments de punter del controlador; tot i així, el suport VR continua sent experimental i s'ha de provar amb el visor final.

## 6. Controls PC predeterminats

- WASD: moviment.
- Ratolí: mirar.
- Shift esquerre: córrer.
- Ctrl esquerre: ajupir-se.
- E: interactuar o guardar un objecte físic que se sosté.
- I: inventari.
- F: encendre/apagar la llanterna equipada.
- R: recarregar la llanterna.
- Q: deixar anar un objecte físic que se sosté.
- G: deixar anar l'equipament.
- H: demanar una pista.
- Alt esquerre + A/D: inclinar-se a Survival Horror.
- X: mirar enrere a Survival Horror.
- V mentre corres cap endavant: slide a Survival Horror.
- Esc: tancar el panell actual o fer pausa.
- F5/F9: desament/càrrega ràpids.

Els controls principals es poden reassignar durant el joc des de `Ajustes > Controles` (Configuració > Controls); els canvis es desen fora de les partides. Per modificar els bindings del comandament o XR, edita `Resources/Input/EscapeRoomInputActions.inputactions`.

## 7. Preparació VR

**Experimental**: el suport VR és funcionalment complet (rig, mans, hàptics, UI 3D, confort) però encara no ha passat QA en un visor físic real — només al simulador d'XRI. No assumeixis una paritat total amb PC fins que no s'hagi validat en maquinari.

`ShowcaseMuseumVR` conté la versió VR del museu i conserva els mateixos puzles de les sales 11 i 13. `VRTemplate` és una escena mínima d'arrencada i no conté les habitacions del museu.

1. Espera que el Package Manager acabi d'importar OpenXR, XR Plug-in Management i XRI.
2. Configura OpenXR per a les plataformes de destinació desitjades a Project Settings.
3. Executa `Setup/Create or Update VR Player Prefab`.
4. Executa `Setup/Prepare Current Scene Interactables for VR` a cada escena.
5. Executa les comprovacions de Project Validation d'OpenXR/XRI.

El prefab VR el genera la versió instal·lada d'XRI i incorpora adaptadors de mans, hàptics i UI Toolkit 3D. Els models de comandament/mà se substitueixen sota els seus `ModelSocket`.

## 8. Accessibilitat i ritme de terror

Des del menú de configuració del mateix joc (no de l'Editor), el jugador pot activar:

- reduir centelleigs, tremolor de càmera i sons forts;
- assistència en persecucions (l'enemic va una mica més lent i oblida abans);
- reducció de gore, disponible com a opció encara que la plantilla base encara no inclogui contingut de gore.

Cap d'aquestes opcions substitueix la dificultat: són independents, de manera que un jugador pot combinar `Nightmare` amb `chaseAssistance` si ho necessita.

Si afegeixes un `TensionDirector` a l'escena, limita quants esdeveniments de terror es poden disparar seguits (cooldown global i pressupost per finestra de temps), per sobre del cooldown propi de cada esdeveniment. És opcional: sense ell, tot funciona igual que abans.

## 9. Publicació

Abans de distribuir l'asset:

1. Executa `Validation/Run Framework Smoke Tests`.
2. Executa `Validation/Validate Save IDs` a cada escena.
3. Comprova PC i VR per separat.
4. No canviïs `SaveId` ni `ItemId` en una actualització publicada sense afegir-hi una migració.
5. Executa `Validation/Validate Current Scene` i resol tots els puzles de cada escena des d'una build neta.
