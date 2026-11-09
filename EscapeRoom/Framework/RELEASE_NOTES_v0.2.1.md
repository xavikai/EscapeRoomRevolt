# Escape Room Revolt 0.2.1

Versió per a les primeres sessions de classe. Manté l'estructura i la base Unity `6000.4.9f1` de la 0.2.0; el ZIP font corregit també incorpora la migració d'APIs per a Unity `6000.6.8f1`.

## Descàrregues

| Fitxer | Què és |
|---|---|
| `EscapeRoom-0.2.1-windows.zip` | La demo per jugar sense Unity. Extreu tot el ZIP i obre `Windows/escaperoom-revolt.exe`. |
| `EscapeRoom-0.2.1-source.zip` | El projecte Unity portable, amb els paquets ja inclosos. Extreu-lo i obre la carpeta amb Unity 6000.4.9f1. |
| `guia-escape-room-0.2.1.html` | La guia interactiva (CA/ES/EN). Es pot obrir amb doble clic. |

## Correccions importants

- **Projecte portable completat després de la revisió de recursos.** S'han recuperat les set icones de mans originals de les mostres XR i la icona de canvi de dispositiu. Les referències a sprites antics del simulador i del desplegable ara usen recursos disponibles. S'han retirat dues referències opcionals obsoletes: les dades d'il·luminació precalculada de la `DemoScene` d'XR i el recurs EmojiOne per defecte de TextMesh Pro, que no es distribuïa amb el projecte. La demo XR es pot tornar a il·luminar des de Unity; per fer servir etiquetes de sprites TMP cal assignar un `TMP Sprite Asset` propi.
- **Compatibilitat amb Unity 6000.6.8f1.** Els validadors ja no fan servir `objectReferenceInstanceIDValue`, que impedia compilar les eines i mostrar el menú `Escape Room Framework`. Les comprovacions de referències i identitat fan servir `EntityId`; les cerques d'objectes i de les mostres XR utilitzen les APIs actuals sense ordenar per InstanceID. A Unity 6.6, la consulta d'assemblies usa la llista gestionada per Unity.
- **Textos 3D trencats.** A la 0.2.0, la pantalla dels teclats, les rodes de números i els cartells de les sales sortien malament. El projecte només portava un dels shaders de TextMesh Pro i li faltaven els fitxers que necessita per funcionar. Ara hi és la carpeta `TextMesh Pro/Shaders` completa.
- **Sala 11, puzles encadenats.** Les palanques s'inclinaven cap al jugador i quasi no es veia que es movien; ara s'inclinen de costat. El cartell deia «ordre opcional», però les palanques estan bloquejades fins que es resol la seqüència de colors. Ara diu «Primer els colors · després les palanques». Les palanques que es creen des del menú `Create` també s'inclinen de costat.
- L'Inspector de les notes ja no dona l'error que amagava el camp `Note Content`.
- El text per defecte del compte enrere es tradueix als tres idiomes.
- La llanterna té bateria il·limitada per defecte al perfil Escape Room. Es pot activar el consum amb `Battery Mode` = `Drains`; el perfil Survival conserva el consum automàtic. L'indicador de bateria només es mostra quan hi ha consum.
- Els objectes equipables, com la llanterna, cauen amb gravetat quan es deixen anar. Es pot desactivar amb `Fall When Dropped`.
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
- Secció **Música i efectes sonors** (CA/ES/EN): música de fons, ambientació 3D, sons d'interacció i connexió amb els volums de la plantilla, amb les limitacions de les fonts independents.

## Si ja tens la 0.2.0

No has de canviar res a les teves escenes. Els temes que ja tinguis continuen funcionant, i els camps d'imatge nous comencen buits. Si el teu projecte és una còpia de la 0.2.0 i els textos 3D surten malament, fes `Window` > `TextMeshPro` > `Import TMP Essential Resources`.

## Comprovacions

Validació del 9 de novembre de 2026 amb Unity 6000.4.9f1: mòduls correctes, 26/26 tests EditMode i 34/34 PlayMode, cap test omès, i build Windows completada. S'ha comprovat que la configuració d'arrencada no activa la preinicialització XR nativa. La sintaxi JavaScript de la guia és correcta i inclou 24 captures incrustades.

La revisió visual manual de PC i la comprovació amb maquinari VR continuen pendents. La guia HTML necessita Internet per carregar Three.js i les tipografies; les captures de Unity van incrustades.
