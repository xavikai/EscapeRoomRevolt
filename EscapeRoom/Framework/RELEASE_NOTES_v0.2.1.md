# Escape Room Revolt 0.2.1

Versió per a les primeres sessions de classe. Fa servir la mateixa versió de Unity (`6000.4.9f1`) i la mateixa estructura de projecte que la 0.2.0.

## Descàrregues

| Fitxer | Què és |
|---|---|
| `EscapeRoom-0.2.1-windows.zip` | La demo per jugar sense Unity. Extreu tot el ZIP i obre `Windows/escaperoom-revolt.exe`. |
| `EscapeRoom-0.2.1-source.zip` | El projecte Unity portable, amb els paquets ja inclosos. Extreu-lo i obre la carpeta amb Unity 6000.4.9f1. |
| `guia-escape-room-0.2.1.html` | La guia interactiva (CA/ES/EN). Es pot obrir amb doble clic. |

## Correccions importants

- **Textos 3D trencats.** A la 0.2.0, la pantalla dels teclats, les rodes de números i els cartells de les sales sortien malament. El projecte només portava un dels shaders de TextMesh Pro i li faltaven els fitxers que necessita per funcionar. Ara hi és la carpeta `TextMesh Pro/Shaders` completa.
- **Sala 11, puzles encadenats.** Les palanques s'inclinaven cap al jugador i quasi no es veia que es movien; ara s'inclinen de costat. El cartell deia «ordre opcional», però les palanques estan bloquejades fins que es resol la seqüència de colors. Ara diu «Primer els colors · després les palanques». Les palanques que es creen des del menú `Create` també s'inclinen de costat.
- L'Inspector de les notes ja no dona l'error que amagava el camp `Note Content`.
- El text per defecte del compte enrere es tradueix als tres idiomes.
- Les builds es generen a `Builds/Release/v0.2.1/` amb la versió 0.2.1.

## Novetats

**Imatges del menú sense UI Toolkit.** El tema del menú (`Menu Theme Settings`) té un apartat nou, **Imágenes**:

| Camp | Què fa |
|---|---|
| `Screen Background` | Imatge de fons de tota la pantalla del menú principal. Es retalla, mai es deforma. |
| `Screen Background In Pause` | La mostra també darrere del menú de pausa. |
| `Panel Image` + `Panel Image Slice` | Imatge del marc del menú. El *slice* evita que les vores decorades es deformin. |
| `Button Image` + `Button Hover Image` + `Button Image Slice` | Imatges dels botons, normal i amb el ratolí a sobre. |

Les imatges s'han d'importar com a `Sprite (2D and UI)`. En mode d'alt contrast s'amaguen perquè el menú es llegeixi bé.

**Guia interactiva** (`EscapeRoom/Framework/docs/guia/index.html`):

- Comença amb la plantilla ja oberta a Unity. Ja no explica com instal·lar Unity ni com moure's per l'editor.
- Té captures de Unity de cada menú i camp que es fa servir, i es poden ampliar.
- Fase nova **Veus, pistes, temps i finals**: zones amb veu i subtítols, pistes automàtiques, zones que disparen accions, compte enrere, perills, finals, objectius i altres peces.
- A la part de menús hi ha passos per a les imatges del tema.
- Preguntes freqüents noves sobre textos en rosa i sobre puzles encadenats.

## Si ja tens la 0.2.0

No has de canviar res a les teves escenes. Els temes que ja tinguis continuen funcionant, i els camps d'imatge nous comencen buits. Si el teu projecte és una còpia de la 0.2.0 i els textos 3D surten malament, fes `Window` > `TextMeshPro` > `Import TMP Essential Resources`.
