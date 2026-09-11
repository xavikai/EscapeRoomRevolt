# Escape Room Revolt v0.1.0-beta.3

Tercera release beta del framework Escape Room per a PC i VR (Meta Quest).

Aquesta versió introdueix la Sala 14 (Linked Lights Puzzle), millores d'estabilitat i robustesa en zones de pistes i triggers narratius, i noves proves PlayMode automatitzades.

## Punts principals

- **Sala 14: Puzle de circuits elèctrics (Linked Lights)**:
  - Sistema de circuits interconnectats amb botons d'estat i botó de reinici.
  - Prefab completament configurable i llest per a producció (`LinkedLightsPuzzleKit.prefab`).
  - Definició de dades amb ScriptableObjects (`Def_demo_linked_lights_puzzle.asset`, `Hint_demo_linked_lights_puzzle.asset`) amb suport per a pistes progressives i guardat d'estat parcial.
  - Porta de recompensa que s'obre automàticament en completar el circuit.
- **Robustesa de zones de pistes i triggers narratius**:
  - `HintZoneTrigger` i `NarrativeTrigger`: integració de `PlayerTriggerUtility` amb cossos cinemàtics per garantir la detecció física en tots els entorns.
  - Suport per a jerarquies complexes del jugador (detecció de tags a components arrel/pares).
  - Protecció en transicions de zones: sortir d'una zona anterior no desactiva la pista d'una zona nova en la qual s'acaba d'entrar.
  - Els triggers d'àudio del museu ara són reutilitzables després de sortir i reentrar amb període de refredament (cooldown), mantenint el comportament d'un sol ús (`Once`) quan s'especifiqui.
- **Validació i proves**:
  - 5 noves proves PlayMode automatitzades a `HintAndCircuitTests.cs` cobrint lògica de circuits, reset, canvis de context de pistes i reentrada de triggers narratius.
- **Documentació**:
  - Nova guia d'autorització i configuració pas a pas a `Assets/_EscapeRoomTemplate/Documentation/LINKED_LIGHTS_AND_HINT_ZONES.md`.

## Descàrregues

- `EscapeRoomRevolt-PC-Windows-v0.1.0-beta.3.zip`: build de demostració per a Windows (Intro, MainMenu, ShowcaseMuseum, LockedOffice).
- `EscapeRoomRevolt-VR-Quest-v0.1.0-beta.3.apk`: build OpenXR Android de `ShowcaseMuseumVR` per a Meta Quest.
- `Source code`: codi font complet generat automàticament per GitHub.

## Integritat de les descàrregues

- Windows ZIP — SHA-256: `162765146C68DF46FA4298DD024792E4D6841A4F22EE1CF508D3DE795D454C82`
- Quest APK — SHA-256: `FBB3C9F4D11978497CB797839A3E5D76BC788F6989A1DB6AC6A7D91F30018890`

## Estat comercial i limitacions conegudes

Aquesta és una versió beta en preparació comercial. Consulta `COMMERCIAL_READINESS.md` i l'auditoria `AUDITORIA_ESCAPE_ROOM_2026-09-09.md` per als detalls tècnics i la matriu de proves de hardware VR pendents.
