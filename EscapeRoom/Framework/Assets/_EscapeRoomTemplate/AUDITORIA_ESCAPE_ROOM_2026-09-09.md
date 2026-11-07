# Auditoria Escape Room PC / VR — 9 de setembre de 2026

## Dictamen

S'han corregit errors de lògica i persistència, ampliat les proves i preparat una guia pràctica per al comprador. **La versió continua en preparació comercial, no certificada per publicar com a estable.** Falten la prova completa en dispositius, la validació de la distribució neta i el tancament de la procedència del contingut. Survival Horror queda fora de l'abast funcional d'aquesta auditoria; es conserva el seu codi perquè encara té dependències compartides.

Entorn: Windows, Unity **6000.4.9f1**, URP **17.4.0**, Input System **1.20.0**, XRI **3.3.0**, XR Management **4.5.3**, OpenXR **1.16.1**. Comprovacions executades contra l'editor obert mitjançant MCP. No s'ha instal·lat ni provat l'APK en un visor físic.

## Resultats verificats

| Comprovació | Resultat | Límit de l'evidència |
|---|---|---|
| Compilació de scripts després de les correccions | Sense errors de compilació | No acredita el funcionament en dispositiu. |
| Proves Edit Mode | **20/20**, 0 fallades, 0 omeses | EventBus, localització i recuperació/validació de guardats. |
| Proves Play Mode | **23/23**, 0 fallades, 0 omeses | 14 existents i 9 regressions noves; no és un recorregut integral de totes les sales. |
| Framework Smoke Test | PASS, 0 avisos | Assets requerits, input, IDs i configuració OpenXR. |
| Validador comercial, museu PC | OK: 2 UIDocuments, 12 puzles, 112 estats, 16 ítems | Validació de configuració; no és certificació comercial. |
| Arrencada PC en editor | `GameContext` inicialitzat, estat Playing, plataforma Desktop, 12 puzles | Inspecció d'arrencada i captura visual. Es van observar missatges d'àudio XR amb fallback al dispositiu predeterminat. |
| Arrencada VR en editor | `GameContext` inicialitzat, estat Playing, plataforma VirtualReality, 12 puzles | Sense visor físic; no valida tracking, confort ni inputs reals. |
| Build Windows | **Èxit, 0 errors, 0 avisos** | Arrencada addicional en mode sense gràfics; no és un recorregut visual complet. |
| Build Android | **Èxit, 0 errors, 426 avisos de shaders de Unity** | Compilar no substitueix instal·lar i jugar al visor. |

### Inspecció de les escenes

S'han examinat components, scripts absents, referències serialitzades d'objectes, IDs de guardat i assignació de definicions/pistes. La detecció de referències comprova les referències serialitzades que Unity identifica com a trencades; no detecta tota possible configuració semànticament incorrecta ni tota ruta escrita com a text.

| Escena | Puzles | IDs persistents | Scripts absents | Referències trencades |
|---|---:|---:|---:|---:|
| ShowcaseMuseum | 12 | 112 | 0 | 0 |
| ShowcaseMuseumVR | 12 | 110 | 0 | 0 |
| LockedOffice | 2 | 32 | 0 | 0 |
| LockedOfficeVR | 2 | 31 | 0 | 0 |
| VRTemplate | 0 | 4 | 0 | 0 |
| MainMenu | 0 | 0 | 0 | 0 |
| Intro | 0 | 0 | 0 | 0 |

Cap ID duplicat dins de cadascuna de les escenes revisades. Tots els puzles detectats tenen `PuzzleDefinition` i pistes. Els museus tenen la mateixa distribució de solvers: 1 codi, 3 seqüències, 3 estats, 1 llançament, 1 col·locació, 1 deslliscant, 1 tuberies i 1 grup. La diferència d'IDs entre PC i VR correspon a components de plataforma; no s'afirma compatibilitat entre els seus fitxers de partida.

## Correccions aplicades

| Problema detectat | Correcció |
|---|---|
| Un socket podia capturar una peça encara agafada per `VRHardwareInteractor`. La recuperació de peces tampoc reconeixia aquesta agafada. | Consulta explícita de possessió en sockets físics, receptors de peces i recuperació de peces. |
| Les dues mans podien intentar posseir el mateix objecte físic. | El controlador de hardware rebutja una segona agafada mentre ja el té una mà. |
| El hardware VR podia activar objectes darrere d'un menú/pausa. | Bloqueig d'interacció de món, record de l'estat del gatell i neteja del focus; també es bloqueja la selecció del pont XRI. |
| Desactivar un component interactuable no impedia cridar-lo. | `CanInteract` respecta `isActiveAndEnabled`. |
| La càrrega fallava si només quedava el `.bak`; els slots disponibles també l'ignoraven. | Recuperació i enumeració de còpies amb principal absent; validació de versió, llistes, IDs i estat abans de restaurar. |
| Una excepció en serialitzar una entitat podia desalinear claus i valors. | Només s'afegeix el parell després d'obtenir el seu estat. |
| Una càrrega rebutjada podia deixar el flux a Loading. | Es recupera l'estat anterior quan el servei informa de fallada. |
| Una escena no inclosa en la build podia iniciar una transició invàlida. | Es valida el destí abans de tocar la sessió; els portals additius reconeixen també rutes completes. |
| Reintentar conservava entitats marcades com a recollides. | Reinici explícit de sessió abans de recarregar la sala. |
| Quest oferia tornar a MainMenu, que la build d'una sola escena no inclou. | L'opció es mostra només quan el destí està disponible. |
| `SequencePuzzle` no desava el prefix introduït. Una seqüència buida podia provocar un índex invàlid. | Desat/restauració del prefix i protecció de configuració buida. |
| `StatePuzzle` ometia condicions sense positioner i podia donar-les per complertes. | Les condicions incompletes impedeixen resoldre; la llista nova s'inicialitza. |
| `SocketPuzzle` no reconstruïa el model col·locat en carregar. | Restauració visual idempotent i retirada del model en reiniciar/carregar no resolt. |
| Nou referències d'accions del simulador XRI apuntaven a un GUID absent. | Enllaç al GUID existent de `XR Interaction Hand Controls.inputactions`; comprovació posterior amb zero referències trencades. |
| El validador no cercava scripts/referències absents i considerava duplicat un mateix ítem compartit per catàlegs. | Inspecció de referències i diferenciació entre reutilitzar el mateix asset i repetir un ID en assets diferents. |
| La release PC incloïa SurvivalHorrorDemo i el README afirmava que es podia treure XRI en PC. | Llista de release limitada a Escape Room; dependències reals documentades. |
| La release d'escriptori inicialitzava XR sense necessitar un visor. | El constructor Windows desactiva temporalment la inicialització XR i restaura la configuració d'autoria en acabar, també en cas de fallada. |
| La build assenyalava API obsoletes del nostre codi i un ajust de latència Quest. | Eliminades les crides obsoletes de la cerca de peces i el registre de càmeres; Android configurat amb `PrioritizeInputPolling` segons la validació del paquet OpenXR. |

## Documentació preparada

- [Guia pràctica de creació i entrega](Documentation/ESCAPE_ROOM_QUICKSTART.md): passos d'instal·lació, exemple codi-porta, taula de mecàniques i camps, inventari, guardat, VR i problemes freqüents.
- Accés directe des de `Escape Room Framework > Documentation > Open Escape Room Quick Start`.
- Enllaços des de README, manual i documentació completa; els informes d'agost es mantenen com a històrics.
- [Preparació comercial](COMMERCIAL_READINESS.md) reescrita amb proves actuals i requisits pendents, sense donar per comprovades prestacions de hardware.
- [ThirdPartyNotices](ThirdPartyNotices.md) aclareix que fonts, mostres i paquets també necessiten revisar els seus avisos; els cinc audios continuen sense procedència acreditada al repositori.

## Límits i pendents prioritaris

1. **VR físic:** completar la matriu de la guia amb dispositiu, mandos, runtime i build. Especial atenció a doble entrada XRI/hardware, llançament, inventari/examen, suspensió i pèrdua de tracking.
2. **Distribució neta:** provar importació i compilació fora de la carpeta de desenvolupament; retirar MCP només de la còpia d'entrega i mantenir les dependències necessàries.
3. **Recorregut complet dels executables:** errors/acerts de tots els mecanismes, mort/final, reintentar, guardar, tancar i tornar a carregar. No s'ha completat aquesta matriu en aquesta sessió.
4. **Persistència avançada:** `PhysicsSocket` no implementa `ISaveable`; cal una lògica persistent explícita o usar els sistemes de puzle guardables. `ResetPuzzle` no desfà automàticament premis ni ítems consumits. Les transicions Single no mantenen una caché completa de totes les sales anteriors.
5. **Contingut i presentació:** procedència d'audios, revisió dels idiomes anunciats i acabat de la demo, que conserva geometria provisional. Cap d'aquests punts es considera resolt només per passar les proves.

Els canvis previs a les escenes de museu, al perfil/excepció de linterna i als manuals s'han conservat. No s'ha fet cap publicació, commit ni pujada remota.

## Nota de compilació

S'han generat els binaris de comprovació, sense publicar-los:

- **Windows:** `Builds/Audit-2026-09-09/Windows/EscapeRoomRevolt.exe`, amb Intro, MainMenu, ShowcaseMuseum i LockedOffice. Build de 76,9 segons, **0 errors i 0 avisos**. S'ha iniciat el procés fora de Unity, en mode sense gràfics, s'ha inspeccionat el registre sense excepcions i s'ha tancat el procés de prova. No s'ha completat una partida en aquest executable.
- **Android/VR:** `Builds/Audit-2026-09-09/EscapeRoom-VR.apk`, amb ShowcaseMuseumVR. Build final de 128,8 segons, **0 errors i 426 avisos**, tots identificats com a avisos de shaders de `com.unity.render-pipelines.core` (principalment conversions de precisió). L'APK ocupa **55.310.899 bytes**. No s'han silenciat els avisos ni modificat els shaders del paquet. La validació visual i de rendiment en visor continua pendent.

La primera build Android tenia quatre avisos (tres d'API C# obsoletes i un de configuració OpenXR), corregits abans de les builds finals. La recompilació posterior va registrar els avisos de shaders del paquet; no s'afirma que sigui una build sense avisos.

[Resum de resultats exportat](Documentation/AUDIT_RESULTS_2026-09-09.json). Captures d'arrencada a l'editor: [PC](Documentation/Audit-PC-2026-09-09.png) i [VR sense visor físic](Documentation/Audit-VR-Editor-2026-09-09.png).

En acabar s'ha deixat l'editor fora de Play, a `ShowcaseMuseum`, amb la configuració XR d'escriptori restaurada i retirats els canvis transitoris de preloads i filtres de shaders generats per la compilació.
