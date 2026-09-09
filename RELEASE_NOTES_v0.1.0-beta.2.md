# Escape Room Revolt v0.1.0-beta.2

Segona release beta del framework Escape Room per a PC i VR (Meta Quest).

Aquesta versió incorpora la nova guia ràpida per a creadors/compradors, les millores detectades a l'auditoria de setembre de 2026, la recuperació robusta de partides guardades (.bak), i correccions crítiques d'interacció física en VR.

## Punts principals

- **Guia d'inici ràpid per a compradors**: Nova documentació detallada a `Assets/_EscapeRoomTemplate/Documentation/ESCAPE_ROOM_QUICKSTART.md`, amb accés directe des del menú `Escape Room Framework > Documentation > Open Escape Room Quick Start`.
- **Correccions en la interacció física VR**:
  - Evitat que un socket o receptor físic arrabassi una peça que el jugador té subjecta amb la mà (`VRHardwareInteractor`).
  - Bloqueig de doble agafada simultània del mateix objecte físic amb les dues mans.
  - Bloqueig d'interaccions VR darrere de menús o pausa per evitar accions no desitjades al món de joc.
- **Robustesa del sistema de guardat (`SaveManager`)**:
  - Recuperació automàtica des de la còpia de seguretat (`.bak`) si el fitxer `.json` principal no existeix o està corrupte.
  - Validació estricta d'integritat, versió i alineació claus-valors per evitar desincronització d'estats.
  - Protecció del flux de joc davant transicions d'escena fallides.
- **Lògica de puzles polida**:
  - `SequencePuzzle`: persistència del prefix introduït i protecció davant seqüències buides.
  - `StatePuzzle`: comprovació rigorosa de condicions (les condicions incompletes ja no es donen per resoltes).
  - `SocketPuzzle`: reconstrucció visual idempotent del model col·locat en carregar la partida.
- **Validació i proves**:
  - 20/20 proves EditMode superades (incloses 8 noves proves de recuperació de guardats a `SaveRecoveryTests`).
  - 23/23 proves PlayMode superades (incloses 9 noves proves de regressió comercial a `CommercialRegressionTests`).
- **Aïllament de la build Desktop**:
  - El constructor de Windows desactiva temporalment la inicialització XR en l'arrencada per evitar carregar runtimes de realitat virtual en PC d'escriptori.

## Descàrregues

- `EscapeRoomRevolt-PC-Windows-v0.1.0-beta.2.zip`: build de demostració per a Windows (Intro, MainMenu, ShowcaseMuseum, LockedOffice).
- `EscapeRoomRevolt-VR-Quest-v0.1.0-beta.2.apk`: build OpenXR Android de `ShowcaseMuseumVR` per a Meta Quest.
- `Source code`: codi font complet generat automàticament per GitHub.

## Integritat de les descàrregues

- Windows ZIP — SHA-256: `D25265348BD4C9226C3C9A9D5D74B773BA4963DB1F8821EFDD451ED77E95D1EB`
- Quest APK — SHA-256: `BC7D9C2196EF09981679A3B46C1C2BE2CB5B9DAFACA73A038BFC4211D1B25DA0`

## Estat comercial i limitacions conegudes

Aquesta és una versió beta en preparació comercial. Consulta `COMMERCIAL_READINESS.md` i l'auditoria `AUDITORIA_ESCAPE_ROOM_2026-09-09.md` per als detalls tècnics i la matriu de proves de hardware VR pendents.
