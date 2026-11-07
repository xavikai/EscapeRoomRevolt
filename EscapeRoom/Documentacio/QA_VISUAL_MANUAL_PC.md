# Guia de QA visual manual — demos PC

Preparada el **26/10/2026** per Unity **6000.4.9f1**, revisió portable **0.2.0-pcfix-2026-10-26**. **Estat: guia preparada; casos manuals pendents d'executar.** No s'han obert ni controlat aplicacions per elaborar-la. Els resultats esperats deriven del codi i dels recursos actuals; no són resultats observats.

## Preparació i evidències

1. Escull un origen i registra'l: consumidor `Projects/EscapeRoom` o `Projects/Survival`, o exportació `Distributions/<producte>-0.2.0-pcfix-2026-10-26`. No barregis un executable antic amb recursos nous. Els snapshots són projectes font: no inclouen executables.
2. Usa la carpeta Windows completa de la build, amb l'executable, `_Data`, DLL i altres fitxers adjunts. Als consumidors és `Builds/Release/v0.1.0-beta.5/Windows/`; aquest nom és històric. Executables: `escaperoom-revolt.exe` i `survival-revolt.exe`. Les builds fetes en la verificació portable són a les còpies temporals indicades als registres locals `.utmp/portable-<producte>.json`.
3. Registra origen, hash SHA-256 de l'executable, data, Windows, CPU, GPU/controlador, RAM, pantalla, resolució, escala de Windows, qualitat gràfica, idioma, dificultat i possibles controls reassignats. L'informe batch [de portabilitat](validation/portable-exports-2026-10-26.json) identifica els executables de les còpies aïllades; [l'informe de correccions](validation/qa-corrections-2026-10-26.json) identifica els dels consumidors.
4. Fes servir una partida i ranura de prova. Si hi ha dades personals existents, fes-ne còpia abans d'esborrar o reinicialitzar res. Per provar un inici net es pot usar un compte Windows de prova. Registra si la sessió comença neta o amb dades prèvies; no pressuposis que les preferències estan reiniciades.
5. Primera passada: 1920×1080 si la pantalla ho permet, idioma CA, teclat/ratolí i dificultat **Standard** a Survival. Segona passada curta: 1280×720, finestra/pantalla completa i ES/EN. Si el maquinari no admet una configuració, deixa-la com a pendent amb el motiu i registra la real.
6. Desa una captura del menú, HUD, inventari, textos 3D i cada incidència. Per errors de moviment, navegació o animació, grava un clip curt. Conserva el `Player.log` de la sessió: habitualment sota `%USERPROFILE%\AppData\LocalLow\<CompanyName>\<ProductName>\Player.log`; consulta els noms del projecte si cal. Copia'l després de sortir i abans de tornar a executar el joc, perquè es pot substituir.

Estats per cas: **PENDENT**, **PASSA**, **FALLA**, **BLOQUEJAT**, **NO APLICA**. Un bloqueig necessita causa i incidència relacionada. NO APLICA necessita una justificació de composició o abast; no serveix per ometre una funcionalitat trencada. Registra resultats per producte i configuració.

## Controls de referència

Són els bindings inicials de `EscapeRoomInputActions.inputactions`. Les preferències desades poden sobreescriure'ls. Segueix també el prompt que mostra el joc; una discrepància és una incidència.

| Acció | Control inicial |
|---|---|
| Moure / mirar | WASD / ratolí |
| Interactuar / córrer / ajupir / saltar | E / Shift esquerre / Ctrl esquerre / Espai |
| Inventari / pausa o tancar el panell superior | I / Esc |
| Accés ràpid | 1–4 o roda del ratolí |
| Llanterna / recàrrega / deixar equipament | F / R / G; requereix equipament i recursos |
| Acció principal / secundària / deixar objecte sostingut | Botó esquerre / dret / Q; depèn del context |
| Pista / desament ràpid / càrrega ràpida | H / F5 / F9; depèn de la funcionalitat i dificultat |
| Survival: càmera / visió nocturna / bateria de càmera | C / N / B |
| Survival: gravar / zoom amb càmera equipada | Botó esquerre mantingut / botó dret |
| Survival: mirar enrere / lliscar | X / V; segons l'estat del jugador |
| Survival: modificador d'inclinació o interacció curosa | Alt esquerre; segons el context |

## Casos comuns — repetir als dos productes

| ID | Passos manuals | Resultat esperat i evidència |
|---|---|---|
| C01 | Executar l'EXE sense visor ni runtime XR obert expressament. Esperar l'entrada al menú. | Arrencada fins al menú, sense diàlegs XR, bloqueig, pantalla negra permanent ni crash. Captura del menú i temps aproximat d'arrencada. |
| C02 | Obrir ajustos, crèdits i tornar. Cancel·lar una confirmació de sortida. Iniciar partida nova. | Botons llegibles i accionables; cancel·lar conserva la pantalla; partida nova entra a ShowcaseMuseum o SurvivalHorrorDemo segons el producte. |
| C03 | Mirar a terra, parets, rètols i objectes de prop i lluny; caminar fins a zones amb llum i ombra. | Sense materials magenta, superfícies absents, parpelleig persistent, geometria que tapa la càmera o col·lisions que deixen caure fora del nivell. La geometria provisional de la demo Survival és intencionada. |
| C04 | Llegir rètols 3D, accents, instruccions, subtítols activats i prompts d'interacció. | Text visible, sense quadrats per glifs absents, solapaments que impedeixin llegir ni desaparició global dels rètols. Un subtítol buit fora d'un diàleg actiu no és per si sol un error. Capturar un rètol i un prompt. |
| C05 | Moure's, girar, interactuar amb E; obrir inventari i tancar-lo amb Esc; després obrir pausa i reprendre. | Cursor disponible als panells i captura coherent en tornar; sense accions de joc mentre un modal les bloqueja. Esc tanca primer el panell superior. No queden overlays duplicats. |
| C06 | Recollir un objecte disponible; obrir inventari, examinar-lo si ho permet i provar accés ràpid. | Nom, icona i quantitat coherents; cap duplicació per una recollida; examinar tanca correctament; slots sense seleccions fantasma. Una acció no admesa no consumeix l'objecte. |
| C07 | Canviar CA→ES→EN i tornar a CA. Revisar menú, HUD, inventari i prompts en cada idioma. | Sense claus de localització exposades ni text retallat; el significat i el context es conserven. Registrar textos d'autoria que no es tradueixin i valorar-los com a incidència de localització. |
| C08 | Canviar volum mestre, música i efectes; sensibilitat, qualitat, pantalla completa i opcions de reducció de moviment/destells. Tornar als ajustos. | Valors conservats i efecte observable quan hi ha l'estímul corresponent; el menú continua usable. No donar per validada la reducció d'un efecte que no s'ha provocat. |
| C09 | Reassignar Interactuar a una tecla lliure des dels ajustos, provar-la i restaurar E. | La nova tecla funciona i els prompts són coherents; es pot tornar al control inicial sense quedar bloquejat. |
| C10 | Revisar menú, inventari i HUD a 1080p i 720p, finestra i pantalla completa; perdre/recuperar focus amb Alt+Tab. | Controls essencials accessibles, sense retalls ni superposicions; focus i ratolí es recuperen. Registrar configuracions no provades. |
| C11 | Fer F5 o guardar en una ranura de prova; canviar posició i inventari; carregar aquella ranura. | La càrrega recupera un estat coherent amb el desament, sense duplicar inventari, HUD ni jugador. Survival Standard permet el desament manual; comprovar que la ranura pertany al producte correcte. |
| C12 | Guardar a Escape Room, obrir després Survival i consultar ranures; repetir en sentit invers. Sortir amb confirmació i tornar a iniciar. | Les partides dels dos productes no es barregen. Les preferències pròpies persisteixen segons la configuració; sortida neta. Revisar logs: cap excepció repetida de TMP ni error que trenqui la sessió. |

## Escape Room — recorregut de ShowcaseMuseum

Comença una partida nova i segueix la numeració visible. El museu és una mostra de mecàniques: l'acceptació és completar cada demostració i el seu efecte associat, sense pressupostar un final global de campanya. Les sales 11–13 actuals són Multi-Stage, Hazards i Number Wheels; no usar l'antic itinerari Audio/Hints/Physics del constructor base. Desa abans de la sala 12, que pot acabar la partida.

| ID | Passos manuals | Resultat esperat |
|---|---|---|
| E01 | En entrar, revisar HUD, obrir ajustos i inventari. | HUD i controls d'Escape Room presents; sense overlays de vida/estamina, cordura o càmera de Survival ni selector de dificultat Survival. |
| E02 | Sala 1: activar i tornar a activar el botó de la demostració. | Canvi d'estat visible del mecanisme/llum i resposta repetible, sense animació o so duplicats per una pulsació. |
| E03 | Sala 2, NoteAndCode: llegir la nota i tancar-la; entrar un codi erroni i després **1234** al teclat. | Nota llegible i control retornat al jugador. L'error no resol el pany; el codi correcte dona el feedback de resolució i l'accés associat. |
| E04 | Sala 3, Receivers: provar un objecte no admès i després el corresponent al receptor, segons la indicació local. | El receptor rebutja l'objecte incorrecte sense perdre'l; l'objecte correcte activa el mecanisme. Prompt, slot i objecte físic/inventari concorden. |
| E05 | Sala 4, Sequence: provar un ordre incorrecte; tornar a començar i seguir les pistes de colors. Provar H abans de resoldre. | Feedback d'error coherent, es pot reintentar; l'ordre correcte resol el puzle. Pista llegible i pertinent, sense tapar els botons. |
| E06 | Sala 5, State: canviar palanques fins a la configuració indicada; provar també una configuració incorrecta. | Només la combinació correcta resol; les posicions visuals concorden amb el resultat i el mecanisme associat respon. |
| E07 | Sala 6, Physics/Throw: agafar un objecte, deixar-lo amb Q i provar un llançament als objectius segons els prompts. | Objectes sòlids i manipulables, impactes/encerts visibles; sense teletransport incontrolat, travessar el terra ni incapacitat de recuperar el control. |
| E08 | Sala 7, Placement: provar un encaix incorrecte i després col·locar cada peça on correspon. | Posició rebutjada o acceptada de manera entenedora; peces acceptades alineades amb els encaixos; resolució sense peces fantasma. |
| E09 | Sala 8, Sliding: moure peces a l'espai lliure, intentar un moviment no vàlid i completar la imatge/ordre indicat. | Només es mouen peces permeses, sense solapaments; resolució visible quan s'assoleix l'ordre correcte. |
| E10 | Sala 9, Melody: escoltar la referència, provar una seqüència errònia i després reproduir la correcta. | Notes distingibles amb volum normal, error recuperable i confirmació de resolució sense pistes sonores tallades. |
| E11 | Sala 10, Pipe: girar/connectar trams des de l'entrada fins a la sortida; provar una connexió incompleta i completar-la. | Incomplet no resol. La connexió correcta dona feedback i activa l'efecte de sortida associat; les canonades són llegibles i el focus no tapa la solució. |
| E12 | Sala 11, Multi-Stage: completar la seqüència i després les palanques; intentar la segona fase abans d'acabar la primera. | Progressió ordenada, estat de fase entenedor i porta/indicador de resolució al completar la cadena. No es dona per resolta amb una sola fase. |
| E13 | Sala 13, Number Wheels: enfocar el candau, girar els quatre rodets fins a **3142** i sortir del focus. | Dígits grans i llegibles; la combinació correcta obre la porta associada i activa l'indicador; el control i la càmera es recuperen en sortir. |
| E14 | Sala 14, Linked Lights: activar interruptors, observar quines llums afecten, usar REINICIAR i assolir la configuració objectiu indicada. | Interruptors i llums concorden; reinici retorna a un estat que permet reintentar; resolució obre la porta i mostra l'indicador. Text i colors llegibles. |
| E15 | Amb una partida desada, entrar finalment a la sala 12. Observar el temporitzador d'entrada; provar separadament el botó del sostre i el temporitzador. Deixar esgotar el compte i recuperar la partida. | Compte visible i final de partida coherent al termini. El sostre és una amenaça independent: no pressupostar que el seu recorregut i el compte són el mateix sistema. Es pot tornar a jugar/carregar sense overlays residuals. |
| E16 | Repetir una sala resolta i carregar un desament anterior/posterior a un puzle. | Estat de portes, peces, inventari i indicadors coherent amb la ranura, sense disparar efectes duplicats. Completar la matriu de sales; una sala inaccessible és BLOQUEJAT amb incidència. |

`LockedOffice` també s'inclou a la build Escape Room, però no és l'escena inicial configurada. Si la sessió ofereix un accés jugable, afegir una passada separada de notes, panys i retorn; registrar com s'hi ha arribat. La seva presència a la build no prova que el menú ofereixi un selector. No modificar l'escena ni usar trucs per donar per completat el recorregut principal.

## Survival — recorregut de SurvivalHorrorDemo

Fer la primera passada en **Standard**. La cadena d'objectius és: piles de recanvi → generador d'emergència → gravar anomalia → sortida. Les bateries de càmera són un recurs diferent de les piles de llanterna/objectiu inicial. Els noms tècnics entre parèntesis ajuden a registrar incidències; no cal que apareguin a la pantalla.

| ID | Passos manuals | Resultat esperat |
|---|---|---|
| S01 | Iniciar partida nova, observar HUD i caminar pel corredor fins a veure la patrulla. | Vida/estamina i elements Survival disponibles quan pertoquen; jugador recolzat al terra. Enemic patrullant sobre el nivell, sense quedar flotant, travessar murs o estar immòbil per falta de NavMesh. Clip de la patrulla. |
| S02 | Recollir les piles de recanvi (`BatteryPickup_Demo`) i les bateries de càmera de l'inici; obrir inventari. | Recollides registrades una sola vegada. Es completa «Recupera piles de recanvi» i s'habilita l'objectiu del generador; les bateries de càmera no substitueixen les piles de l'objectiu. |
| S03 | Provar F/R amb llanterna equipada i piles disponibles; comparar llum apagada/encesa. | Feix visible i il·luminació útil, feedback de bateria coherent; recàrrega consumeix el recurs correcte i no produeix una llanterna duplicada. |
| S04 | Apropar-se a la sortida sense completar la cadena; després anar al generador de la ruta lateral (`EmergencyGenerator`) i activar-lo amb E. | No hi ha victòria anticipada. Després del generador es completa l'objectiu d'energia, s'activa el registre de l'anomalia i la porta de seguretat deixa avançar. |
| S05 | Córrer en una zona oberta, parar per recuperar estamina, ajupir-se i comparar desplaçament/visibilitat. | Estamina baixa i es recupera de manera coherent; càmera i càpsula no travessen el terra ni sostres. Els indicadors no tapen el camp de visió. |
| S06 | Fer-se detectar en una zona oberta, fugir al voltant d'un obstacle i trencar la línia de visió. | Transició entenedora de patrulla a persecució i cerca; enemic segueix camins transitables i no travessa parets. No exigir evasió instantània només per girar una cantonada. |
| S07 | Provar porta operable i amagatall (armari/llit) amb E; sortir-ne i recuperar moviment. Repetir amb l'enemic a prop. | Entrada/sortida sense bloquejar càmera ni jugador; l'enemic pot inspeccionar amagatalls, per tant amagar-se no garanteix seguretat. Porta i enemic no queden encallats permanentment. |
| S08 | Provar obstacles senyalitzats de salt, escalada/escala i pas estret amb el prompt contextual; provar mirar enrere i lliscar on pertoqui. | Transicions acabades en una posició transitable, sense quedar dins del mur ni perdre controls. Registrar qualsevol acció sense prompt o sense resposta; no pressupostar que Espai resol tots els obstacles. |
| S09 | Equipar càmera amb C, activar/desactivar N, provar zoom i B amb bateria disponible. | Overlay llegible, visió nocturna canvia la imatge, càrrega coherent i recurs de càmera correcte. HUD no es duplica en alternar càmera, inventari i pausa. |
| S10 | Després del generador, enquadrar el subjecte anòmal sense obstacles i mantenir botó esquerre aproximadament **2,5 s**, dins de **14 m**. Provar abans interrompre l'enquadrament. | Progrés de gravació visible i coherent; gravació completada registra l'evidència una sola vegada i habilita «Arriba a la sortida». Fora de rang o sense enquadrament no es completa. |
| S11 | Travessar el punt de control intermedi, recollir o moure un recurs i provocar una mort controlada per enemic/perill. | En Standard es retorna al punt de control amb vida restaurada segons el perfil i estat coherent amb el checkpoint; no hi ha HUD duplicat, pantalla negra permanent ni recollides infinites. El checkpoint és independent de la ranura manual. |
| S12 | En una partida amb l'evidència completada, avançar pel pas estret fins a la sortida final. | Es completa l'objectiu de sortida i apareix el final «Has escapat de la instal·lació» o la traducció equivalent; controls del final utilitzables, sense combat que impedeixi llegir-lo. Captura del final. |
| S13 | En una sessió separada, seleccionar Accessible i després Nightmare als ajustos; revisar desament manual/checkpoints i reprendre. | La dificultat seleccionada queda reflectida. Nightmare desactiva desament manual i checkpoints; la restricció es comunica i no s'interpreta com una avaria. No sobreescriure la ranura de referència Standard. |

## Registre i criteris de tancament

Copiar [la plantilla de sessió](QA_VISUAL_MANUAL_PC_REGISTRE.md) abans de començar. Conservar la guia i els informes batch com a referència; els resultats manuals van a una còpia nova amb data, producte i identificador de build.

- **Bloquejant:** crash, no arrenca, partida o objectiu principal impossible, pèrdua/corrupció de dades, control irrecuperable. Aturar el recorregut afectat i marcar dependents BLOQUEJAT.
- **Major:** text essencial il·legible, enemic sense navegació útil, UI essencial inaccessible, càrrega incorrecta, efecte de puzle que no correspon. Obrir incidència i repetir el cas després de corregir.
- **Menor:** defecte visual o sonor local que no impedeix continuar. Adjuntar captura, ubicació i configuració.

Per acceptar la passada PC, tots els casos aplicables han de tenir resultat i evidència; cap bloquejant o major oberta; les dues demos han de completar el recorregut principal, càrrega i retorn als menús. Les incidències menors pendents requereixen decisió explícita de qui accepta el QA. Els casos no executats es mantenen PENDENT: les proves batch no els converteixen en PASSA.

Aquesta guia no tanca QA de visor, rendiment sostingut ni preparació comercial. No cal reconstruir ni tornar a executar Unity per llegir-la o omplir el registre. Les exportacions portables ja validades es conserven intactes; aquesta guia es distribueix al costat dels snapshots com a documentació del repositori.

## Fonts de contrast

- Controls: `SharedPackages/com.xavikai.revolt.common/Resources/Input/EscapeRoomInputActions.inputactions` i `Player/PC/PlayerInputHandler.cs`.
- Menús i modalitat: `SharedPackages/com.xavikai.revolt.common/UI/Toolkit/UIToolkitMenuController.cs`.
- Museu actual: `Projects/EscapeRoom/Assets/_EscapeRoomTemplate/Scenes/ShowcaseMuseum.unity`; ampliació a `SharedPackages/com.xavikai.revolt.common/Core/Editor/EscapeRoomShowcaseExpansionBuilder.cs`.
- Objectius i recorregut Survival: `Projects/Survival/Assets/_EscapeRoomTemplate/Scenes/SurvivalHorrorDemo.unity`, assets de `ScriptableObjects/Survival` i `SharedPackages/com.xavikai.revolt.survival/Editor/SurvivalHorrorDemoBuilder.cs`.
- Gravació i checkpoints: `CamcorderEvidenceRecorder.cs`, `CheckpointManager.cs` i `SurvivalObjectiveZone.cs` del paquet Survival. Si una build difereix d'aquesta revisió, registrar-ho abans d'aplicar els resultats esperats.
