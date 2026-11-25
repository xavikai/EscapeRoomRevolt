# Escape Room Revolt 0.2.3 · 25/11/2026

La versió 0.2.3 corregeix les portes, calaixos i tapes que saltaven al centre de l'escena en carregar una partida. També amplia la guia dels menús perquè es pugui seguir pas a pas sense conèixer Unity.

| Descàrrega | Com s'utilitza |
| --- | --- |
| `EscapeRoom-0.2.3-source.zip` | Extraieu el ZIP i obriu amb Unity Hub la carpeta que conté `Assets`, `Packages` i `ProjectSettings`. El paquet del framework ja hi és inclòs. |
| `EscapeRoom-0.2.3-windows.zip` | Extraieu tot el ZIP i executeu `Windows/escaperoom-revolt.exe`. |
| `guia-escape-room-0.2.3.html` | Obriu el fitxer al navegador. Té les captures incrustades i carrega Three.js per Internet. |
| `UPDATE_FROM_0.2.2.ca.md` | Com actualitzar el projecte del grup sense perdre escenes, models ni configuració. Només canvia un fitxer. |

## Correcció

- **Portes, calaixos i tapes que saltaven al centre en carregar una partida.** Afecta qualsevol objecte amb `Door` que no tingui Animator. En mode Pivot, l'objecte anava al centre de l'escena; en mode Slide, al centre del seu pare. Passava perquè la partida es restaura abans que la porta hagués calculat la frontissa. Ara la porta la calcula abans, i també es conserven les portes entreobertes. Hi ha dos tests nous que ho comproven.

## Desar i carregar partides

S'ha revisat tot el sistema de desar i carregar:

- **Partida nova i «Tornar-ho a provar»** ja no hereten els punts d'examen revelats a la partida anterior, que impedien tornar a trobar l'objecte.
- **F5 només desa mentre es juga.** Abans es podia desar al menú principal o a la pantalla final, i llavors «Continuar» obria el menú.
- **Els comptes enrere i els perills** que s'engeguen sols ja no tornen a començar després de carregar.
- **Les zones `Event Trigger Zone` d'un sol ús** recorden que ja s'han disparat.
- **Les notes que desapareixen en llegir-les** no tornen a aparèixer.
- **Els objectes de l'inventari** que no eren al catàleg ja no es perden en carregar.
- **Opció nova als puzles: `Invoke On Solved On Load`.** Serveix per restaurar els llums o els objectes que activa `On Solved ()`.
- **Els Save ID duplicats** (en copiar objectes amb Ctrl+D) es corregeixen sols en desar l'escena, i la Console n'avisa en fer Play.

## Menú sense Survival Horror

El menú de l'Escape Room ja no té textos ni opcions del Survival Horror:
- títols com `PAUSA`, `AJUSTES`, `GUARDAR PARTIDA` i `CARGAR PARTIDA`, en lloc dels textos d'investigació i de «cintas de seguridad»;
- sense les opcions de gore, destellos, sonidos fuertes ni temblor de cámara.

## Guia interactiva

- **Menús · Per on començar:** abans la guia deia que es fes servir `Create or Update Main Menu Scene`, que reconstrueix l'escena i esborra el tema. Ara explica que el menú ja existeix i que només cal connectar-lo a la sala, provar-lo i canviar-ne l'aspecte.
- **Disseny del menú:**
  - taula «Què vols canviar? On es fa» (15 casos);
  - nou **Nivell 0 · Abans de començar**: qui fa el menú, les finestres de Unity, on és cada peça i com desfer un canvi;
  - ajuts nous als passos T2-T6 i U1;
  - el pas U4 ara diu correctament `Margin & Padding`;
  - nou apartat de problemes freqüents amb el menú.
- **Nova secció «Missatges, notes i final»**, pas a pas:
  - veus i subtítols en passar per un lloc;
  - notes amb text: fixes o que el jugador s'emporta, i com editar-les;
  - notes que són una imatge, sense programar: un cartell a l'escenari o un document que es recull i s'examina;
  - el final del joc: sortint per la porta o resolent l'últim puzle, amb el text de la pantalla final.
- **Preguntes freqüents:** portes que salten al centre en carregar, amb la solució per als projectes 0.2.2.
- Tot en català, castellà, anglès, francès i rus.

## Si el grup ja té un projecte

Seguiu [UPDATE_FROM_0.2.2.ca.md](https://github.com/xavikai/EscapeRoomRevolt/releases/download/escaperoom-v0.2.3/UPDATE_FROM_0.2.2.ca.md). Cal tancar Unity, fer una còpia de seguretat i substituir la carpeta del paquet `com.xavikai.revolt.common`. No cal tocar cap escena ni cap prefab. Després, deseu cada escena (Ctrl+S): així es corregeixen sols els Save ID repetits.
