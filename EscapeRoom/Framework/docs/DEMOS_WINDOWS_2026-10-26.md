# Demos executables Windows — 26/10/2026

**Carpetes reorganitzades el 26/10/2026:** consulta [l’índex actual del repositori](../../../README.md). Les rutes antigues en informes JSON i exemples històrics identifiquen l’estat de la validació; el mapa de trasllat en documenta la ubicació actual.

Paquets **0.2.0-pcfix-2026-10-26** per provar les demos sense instal·lar Unity Editor ni Unity Hub. Contenen la build Windows de 64 bits completa, la guia de QA, el registre propi i els informes previs. No s'han executat les demos durant l'empaquetatge; el QA visual continua pendent.

| Demo | ZIP executable | Executable dins del paquet | Casos manuals pendents |
|---|---|---|---:|
| Escape Room | [EscapeRoom Windows.zip](../../Windows/EscapeRoom-0.2.0-pcfix-2026-10-26-windows.zip) | `Windows/escaperoom-revolt.exe` | 28 |
| Survival | [Survival Windows.zip](../../../Survival/Windows/Survival-0.2.0-pcfix-2026-10-26-windows.zip) | `Windows/survival-revolt.exe` | 25 |

Els ZIP, les carpetes sense comprimir i els fitxers `.zip.sha256` són a `Distributions/WindowsDemos/`, com a artefactes locals exclosos de Git. Per lliurar, enviar el ZIP i el seu `.zip.sha256`. [L'informe d'integritat](validation/windows-demos-2026-10-26.json) registra hashes, mides, inventari verificat i origen de cada build.

## Inici ràpid per a la persona que prova

1. Comparar el SHA-256 del ZIP amb el `.zip.sha256` adjacent o l'informe. En PowerShell: `Get-FileHash -LiteralPath '<paquet>.zip' -Algorithm SHA256`.
2. Extreure **tot** el ZIP a una carpeta amb permisos d'escriptura en un PC Windows de 64 bits. No executar-lo des del visor del ZIP.
3. Llegir `README.md` i obrir l'EXE de `Windows/`. La carpeta `_Data`, `UnityPlayer.dll`, `MonoBleedingEdge`, `D3D12` i la resta de fitxers han de continuar al seu costat. No separar ni intercanviar dades entre demos. No cal Unity ni visor VR.
4. Copiar `docs/QA_VISUAL_MANUAL_PC_REGISTRE.md` per a la sessió. Ja identifica producte, revisió, executable i hash. Completar maquinari, configuració, dades inicials i evidències.
5. Seguir `docs/QA_VISUAL_MANUAL_PC.md`, aplicant els casos comuns i els del producte. En aquest paquet no s'han de fer els passos d'importació ni reconstrucció de projectes font de la guia. El cas C12 requereix tots dos paquets.
6. Després de sortir, conservar el `Player.log` abans de tornar a executar el joc. Rutes segons el projecte: `%USERPROFILE%/AppData/LocalLow/XaviKai/Escape Room Revolt/Player.log` i `%USERPROFILE%/AppData/LocalLow/XaviKai/Survival Revolt/Player.log`. Extreure de nou el ZIP no reinicialitza les dades externes de jugador; fer còpia de les partides personals abans de qualsevol reinicialització.

Controls inicials: WASD i ratolí, E interactuar, I inventari, Esc tancar panell/pausa. La guia inclou controls d'equipament i Survival. Si hi ha preferències prèvies, poden canviar els bindings inicials.

## Contingut i comprovacions

Cada carpeta de lliurament té `README.md`, `FILES.sha256.json`, `Windows/` amb la build completa i `docs/` amb guia, registre i informes de validació batch anteriors. El registre conté únicament els casos comuns i els del producte, tots PENDENT.

Abans d'empaquetar s'han comparat EXE, `boot.config` i arbre complet `_Data` amb [l'informe de les reconstruccions correctes](validation/qa-corrections-2026-10-26.json). S'ha comprovat també que no es demana preinicialització nativa XR. S'han copiat tots els fitxers del runtime, excloent només les carpetes `*_BurstDebugInformation_DoNotShip`. Les builds originals es conserven intactes.

Després de comprimir s'ha rellegit cada fitxer del ZIP, comparat el hash SHA-256 i la llista exacta d'entrades, i rebutjat rutes insegures o duplicades. `FILES.sha256.json` inventaria el contingut i exclou el mateix inventari; el hash extern cobreix tot el ZIP. Els dos EXE poden compartir hash perquè són el mateix llançador Unity: el paquet i les dades adjuntes identifiquen la demo completa.

Aquestes comprovacions acrediten la integritat del lliurament i la procedència de les builds. **No s'ha rellançat Unity, executat cap demo ni reprès el control de les aplicacions.** La primera arrencada gràfica en el PC receptor, el recorregut complet, la compatibilitat del maquinari i l'acceptació visual continuen pendents de la sessió manual.

## Eina de preparació

`pwsh -NoProfile -File tools/package-windows-demos.ps1`

L'eina usa les builds identificades a l'informe de correccions, rebutja divergències de hashes i preserva paquets i informes existents. La revisió actual ja està generada. Una entrega posterior ha de tenir builds, revisió i evidència corresponents.

Els [paquets de projectes font](LLIURAMENT_PC_2026-10-26.md) es mantenen disponibles per editar o reconstruir amb Unity. Aquests ZIP Windows permeten fer la sessió de joc directament.
