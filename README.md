# Escape Room i Survival

El repositori està organitzat en **dues carpetes principals**, una per projecte:

**Darrera versió del joc: [Escape Room 0.2.2](https://github.com/xavikai/EscapeRoomRevolt/releases/tag/escaperoom-v0.2.2)** · [Notes de la versió](EscapeRoom/Framework/RELEASE_NOTES_v0.2.2.md) ([0.2.0](EscapeRoom/Framework/RELEASE_NOTES_v0.2.0.md)). Inclou demo Windows, projecte portable, guia revisada i sumes SHA-256.

**[Guies interactives amb cerca](https://xavikai.github.io/EscapeRoomRevolt/)** · [Escape Room (CA/ES/EN/FR/RU)](https://xavikai.github.io/EscapeRoomRevolt/escape-room/) · [Survival (CA)](https://xavikai.github.io/EscapeRoomRevolt/survival/) · [Descàrregues HTML](https://github.com/xavikai/EscapeRoomRevolt/releases/tag/docs-search-2026-11-19).

| Carpeta | Projecte Unity per editar | Demo Windows per provar sense Unity |
|---|---|---|
| [EscapeRoom](EscapeRoom/README.md) | [EscapeRoom/Unity](EscapeRoom/Unity) | [ZIP Escape Room](https://github.com/xavikai/EscapeRoomRevolt/releases/download/escaperoom-v0.2.2/EscapeRoom-0.2.2-windows.zip) |
| [Survival](Survival/README.md) | [Survival/Unity](Survival/Unity) | Paquet local; encara sense release pròpia a GitHub |

Cada carpeta conté `Unity/`, `Documentacio/`, `Exportacions/`, `PaquetsFonts/` i `Windows/`. Els ZIP i les exportacions són artefactes locals exclosos de Git. Extreu tot el ZIP Windows abans d'executar la demo.

La **base comuna canònica**, les eines, la documentació tècnica i l'autoria Unity històrica es conserven a [EscapeRoom/Framework](EscapeRoom/Framework/README.md). Survival referencia els mateixos paquets; no se n'ha creat una segona còpia. Per treballar en les demos actuals, obre `EscapeRoom/Unity` o `Survival/Unity`, amb Unity 6000.4.9f1. La carpeta arrel ja no és un projecte Unity.

[Detall de l'organització i mapa de rutes](EscapeRoom/Framework/docs/ORGANITZACIO_CARPETES_2026-10-26.md). [Validació d'Escape Room 0.2.1 del 9/11/2026](EscapeRoom/Framework/docs/validation/release-0.2.1.json): 26/26 tests EditMode, 34/34 PlayMode i build Windows correcta. El QA visual manual de PC i la comprovació amb maquinari VR continuen pendents.

El [ZIP font 0.2.1 corregit](EscapeRoom/Framework/docs/validation/portable-release-0.2.1.json) també ha passat una importació neta fora del repositori, l'auditoria de recursos i shaders, els 60 tests i una build Windows. L'informe registra el SHA-256 del paquet comprovat.

Eines, executades des d'aquesta arrel:

```powershell
node EscapeRoom/Framework/tools/check-products.cjs
pwsh -File EscapeRoom/Framework/tools/validate-product.ps1 -Product EscapeRoom -BuildWindows
pwsh -File EscapeRoom/Framework/tools/validate-product.ps1 -Product Survival -BuildWindows
```

La primera ordre fa comprovacions de fitxers. Les dues següents importen, proven i construeixen en batch. Escape Room s'ha validat per a la 0.2.1; les darreres proves completes de Survival corresponen a la [revisió del 26/10/2026](EscapeRoom/Framework/docs/QA_PC_2026-10-26.md), sense una nova validació de Survival per a la 0.2.1.

La guia pròpia de Survival inclou arquitectura, controls, recorregut i laboratori de detecció, càmera i checkpoints. La guia comuna conserva les demos originals i incorpora la separació dels dos projectes.
