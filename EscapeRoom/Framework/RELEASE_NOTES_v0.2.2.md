# Escape Room Revolt 0.2.2

La versió 0.2.2 reuneix les correccions dels puzles, el menú per crear circuits de llums i la guia interactiva actualitzada.

| Descàrrega | Com s'utilitza |
| --- | --- |
| `EscapeRoom-0.2.2-source.zip` | Extraieu el ZIP i obriu la carpeta que conté `Assets`, `Packages` i `ProjectSettings` amb Unity Hub. El paquet del framework ja hi és inclòs. |
| `EscapeRoom-0.2.2-windows.zip` | Extraieu tot el ZIP i executeu `Windows/escaperoom-revolt.exe`. |
| `guia-escape-room-0.2.2.html` | Obriu el fitxer al navegador. La guia manté les captures incrustades i carrega Three.js per Internet. |
| `UPDATE_FROM_0.2.1.ca.md` | Passos per actualitzar el projecte del grup i conservar les escenes, models i configuració. |

## Canvis

- Els impactes vàlids de `Throw Puzzle` i els estats d'encaix de `Placement Puzzle` canvien el color del model encara que sigui en un objecte fill.
- En la vista de prop de les rodetes, el cos del jugador ja no intercepta els clics. Les parets i altres obstacles sòlids continuen bloquejant-los.
- `Escape Room Framework > Create > Puzzles > Linked Lights Puzzle` crea un circuit de cinc llums a l'escena actual. La combinació de mostra és 1, 3 i 5.
- La guia incorpora `Definition` i pistes, cinemàtiques, ampliació de puzles i instruccions per a les còpies 0.2.1. La recepta de palanques obre el laboratori de palanques. Els rodets tenen una simulació pròpia.
- El francès i el rus s'afegeixen al català, castellà i anglès. Les traduccions noves són automàtiques amb revisió dels termes principals. Les captures conserven l'idioma original.

## Si el grup ja té un projecte

Feu una còpia amb Unity tancat i apliqueu els cinc scripts indicats a la guia de migració. Conserveu les escenes, els prefabs i el `Persistent Id` dels puzles. Si heu modificat algun dels mateixos scripts, fusioneu les diferències. La guia de migració també és dins del ZIP portable, a `docs/release/`.

[Guia per actualitzar un projecte 0.2.1](https://github.com/xavikai/EscapeRoomRevolt/releases/download/escaperoom-v0.2.2/UPDATE_FROM_0.2.1.ca.md)
