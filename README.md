# Escape Room i Survival

El repositori està organitzat en **dues carpetes principals**, una per projecte:

**Darrera release: Escape Room 0.2.1** — [notes de la versió](EscapeRoom/Framework/RELEASE_NOTES_v0.2.1.md) ([0.2.0](EscapeRoom/Framework/RELEASE_NOTES_v0.2.0.md)). Els ZIP (demo Windows i projecte portable) es descarreguen des de la pàgina *Releases* de GitHub.

**[Guia interactiva amb Three.js: museu 3D i laboratori de mecàniques](EscapeRoom/Framework/docs/guia/index.html)** · [Instruccions](EscapeRoom/Framework/docs/guia/README.md). Es conserva a la base comuna; obre `index.html` amb un navegador. Necessita Internet per carregar Three.js i les tipografies.

| Carpeta | Projecte Unity per editar | Demo Windows per provar sense Unity |
|---|---|---|
| [EscapeRoom](EscapeRoom/README.md) | [EscapeRoom/Unity](EscapeRoom/Unity) | [ZIP Escape Room](https://github.com/xavikai/EscapeRoomRevolt/releases/download/escaperoom-v0.2.1/EscapeRoom-0.2.1-windows.zip) |
| [Survival](Survival/README.md) | [Survival/Unity](Survival/Unity) | [ZIP Survival](Survival/Windows/Survival-0.2.0-pcfix-2026-10-26-windows.zip) |

Cada carpeta conté `Unity/`, `Documentacio/`, `Exportacions/`, `PaquetsFonts/` i `Windows/`. Els ZIP i les exportacions són artefactes locals exclosos de Git. Extreu tot el ZIP Windows abans d'executar la demo.

La **base comuna canònica**, les eines, la documentació tècnica i l'autoria Unity històrica es conserven a [EscapeRoom/Framework](EscapeRoom/Framework/README.md). Survival referencia els mateixos paquets; no se n'ha creat una segona còpia. Per treballar en les demos actuals, obre `EscapeRoom/Unity` o `Survival/Unity`, amb Unity 6000.4.9f1. La carpeta arrel ja no és un projecte Unity.

[Detall de l'organització, mapa de rutes i comprovacions](EscapeRoom/Framework/docs/ORGANITZACIO_CARPETES_2026-10-26.md). El QA visual manual continua pendent. La reorganització s'ha fet sense reprendre el control de les aplicacions ni rellançar Unity.

Eines, executades des d'aquesta arrel:

```powershell
node EscapeRoom/Framework/tools/check-products.cjs
pwsh -File EscapeRoom/Framework/tools/validate-product.ps1 -Product EscapeRoom -BuildWindows
pwsh -File EscapeRoom/Framework/tools/validate-product.ps1 -Product Survival -BuildWindows
```

La primera ordre fa comprovacions de fitxers. Les dues següents importen, proven i construeixen en batch quan es decideixi executar-les; no s'han rellançat durant la reorganització.

**[Guia pròpia de Survival amb Three.js](Survival/Documentacio/GuiaInteractiva/index.html)**: arquitectura, controls, recorregut i laboratori de detecció, càmera i checkpoints. La guia comuna conserva les demos originals i incorpora la separació dels dos projectes.
