# Sala 14: circuit de llums i zones de pistes

## Provar la demostració

Obre `ShowcaseMuseum` i entra a Play. Les zones vermella, verda i blava de la sala de pistes mostren el primer missatge després de 3 segons i el següent al cap de 3 segons més. Surt completament de la zona i torna-hi a entrar per repetir les pistes. Respecten l'opció de subtítols dels ajustos.

`AudioTriggerArea`, al passadís (z = 27,5), ara utilitza `Always`: reprodueix la narració i el text en entrar, amb 5 segons de cooldown. Per repetir-la, surt i torna a entrar després del cooldown. Els altres triggers narratius continuen admetent `Once`, `Always` i `ProgressiveHints`; `Once` conserva el seu estat al guardat.

La sala **14 · CIRCUIT DE LLUMS** és a la dreta, al final del passadís (x = 6, z = 70). Cada botó commuta la seva llum i les veïnes. Cal encendre les cinc. Des de l'estat inicial, prem **1, 3 i 5**, en qualsevol ordre. La porta s'obre i s'encén el marcador verd. El botó de reinici restaura el circuit, tanca/bloqueja la porta i apaga el marcador.

## Reutilitzar la mecànica

Arrossega `Prefabs/LinkedLightsPuzzleKit.prefab` a una escena. El kit inclou panell, cinc botons, etiquetes i reinici. Connecta `On Solved` de `LinkedLightsPuzzle` a la teva recompensa. Les connexions amb la porta de la sala 14 pertanyen a l'escena de demostració.

En `LinkedLightsPuzzle > Nodes`, cada element defineix:

- `Initially On`: estat inicial de la llum.
- `Target On`: estat que ha de tenir per resoldre el circuit.
- `Connections`: índexs de les llums que commuta el botó (comencen a 0). Inclou el seu propi índex si també s'ha de commutar a si mateix. Els duplicats s'ignoren.
- `Indicator`: Renderer que mostra l'estat. Els colors encès/apagat es poden editar; la interacció també indica l'estat amb text.

Cada `LinkedLightButton` apunta al circuit i al seu índex. Pots canviar el nombre de nodes, les connexions, l'objectiu i els models. Evita nodes nuls i comprova que la configuració escollida tingui solució. Una forma de garantir-ho és partir de l'objectiu i aplicar unes quantes pulsacions per obtenir l'estat inicial.

Duplica la `PuzzleDefinition` del circuit i assigna un `Persistent Id` únic a cada còpia independent. El prefab de demostració comparteix la definició `demo_linked_lights`: cal substituir-la quan s'utilitzi més d'una vegada. El circuit desa les llums i l'estat de resolució; el reinici descarta el progrés parcial. Les portes desen el seu propi estat.

Menú d'autoria: `Escape Room Framework > Demo > Add or Update Linked Lights Room`. Reconstrueix només la sala 14 i el seu prefab de demostració; substitueix els canvis manuals fets a aquesta sala/kit. Les sales anteriors es conserven.

## Zones reutilitzables

`HintZoneTrigger` i `NarrativeTrigger` preparen el collider com a trigger i un Rigidbody cinemàtic sense gravetat, també per a escenes antigues. El tag `Player` pot ser al collider o a qualsevol pare. La zona de pistes compta els colliders presents perquè sortir amb un sol collider no esborri el context dels altres. En sortir, només esborra les seves pròpies pistes i no les d'una altra zona acabada d'activar.

## Verificació

Verificació del 9 de setembre de 2026: **28/28 proves PlayMode aprovades**, sense errors a la consola al final de la comprovació. A `ShowcaseMuseum` s'han comprovat els missatges de les tres zones, el text narratiu, la resolució del circuit mitjançant els botons i el reinici de la porta i del marcador.

Captures: [sala 14](Screenshots/Room14_LinkedLights.png), [pista visible al HUD](Screenshots/HintZones_Subtitle_Verified.png) i [subtítol narratiu](Screenshots/AudioTrigger_Subtitle_Verified.png).

Suite `HintAndCircuitTests`: detecció física amb un collider fill sense tag, retard i sortida de pistes, preservació del context nou, narració repetible, solució única del circuit, reinici i guardat/restauració parcial i resolta.

La comprovació amb colliders fills és automatitzada; no substitueix la validació amb un visor VR real.
