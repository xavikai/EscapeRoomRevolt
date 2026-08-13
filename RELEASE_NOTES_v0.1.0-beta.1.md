# Escape Room Revolt v0.1.0-beta.1

Primera release beta descarregable del framework PC/VR.

## Punts principals

- Room 11 redissenyada com un grup de puzles simultanis: tots romanen visibles i la porta espera que es resolguin tots.
- Ordre configurable: lliure o obligatori. En mode ordenat, els puzles futurs es veuen però els controls queden bloquejats fins que toca.
- Room 13 amb vuit botons físics ▲/▼. A PC funcionen amb clic esquerre; a VR, amb el gallet. Tots criden la mateixa operació de canvi de xifra.
- Paritat de contingut comprovada entre `ShowcaseMuseum` i `ShowcaseMuseumVR` per a les sales 11 i 13.
- Escenes VR, rig, panells d'interfície i configuració OpenXR inclosos al repositori.

## Descàrregues

- `EscapeRoomRevolt-PC-Windows-v0.1.0-beta.1.zip`: build de demostració per a Windows.
- `EscapeRoomRevolt-VR-Quest-v0.1.0-beta.1.apk`: build OpenXR Android de `ShowcaseMuseumVR` per a Meta Quest.
- `Source code`: codi font complet generat automàticament per GitHub.

## Controls rellevants

- PC: `E` obre el panell de combinació; amb el cursor lliure, clic esquerre sobre ▲/▼ canvia cada rodet; clic dret surt.
- VR: apunta al control ▲/▼ i prem el gallet.

## Estat beta

La compilació, el validador comercial de les escenes i les proves automatitzades són correctes: 12/12 EditMode i 14/14 PlayMode. La release es marca com a prerelease perquè encara falta completar la matriu de QA de hardware VR, confort i rendiment descrita a `ROADMAP.md`.

## Integritat de les descàrregues

- Windows ZIP — SHA-256: `4C549852F0610F45ED763F38D07ECB3BB8A3F0A5AEBC1F5B4EB721524F851753`
- Quest APK — SHA-256: `78B8970E48184DF1686765516D2190E005936C8DDDAAB071D76277C1CE2D8BBF`
