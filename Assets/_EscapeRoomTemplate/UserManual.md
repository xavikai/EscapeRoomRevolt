# 📖 Manual d'Usuari: Escape Room Template

Benvingut/da al **Escape Room Framework**. Aquesta guia està pensada per a Artistes i Dissenyadors de Nivells. El sistema està dissenyat perquè **no hagis de programar ni una línia de codi**. Tot funciona connectant blocs des de la interfície de Unity!

---

## 1. 🏗️ Preparar l'Escena Base (Nivell Nou)

Quan obris una escena 3D completament buida, només necessites un parell de passos per convertir-la en un Escape Room funcional:

1. A la barra superior de Unity, vés a **`EscapeRoom > Build Player Prefab`**.
   - Això col·locarà el jugador automàticament a l'escena, amb la seva càmera, creueta, menú d'inventari i sistema de moviment.
   - *Nota: Si la teva escena ja tenia una `Main Camera` per defecte, esborra-la per evitar conflictes.*
2. Afegeix un `GameObject` buit anomenat **GameManager**, i posa-li l'script `Bootstrapper.cs`. Aquest script despertarà tots els sistemes (Inventari, Puzles, Audio, etc.) en prémer Play.
3. Per poder llegir subtítols, crea un **Subtitle Panel** al teu Canvas amb un **Text**, i assigna'ls a l'script `UIManager`.

> [!TIP]
> **Vols una habitació ja muntada per provar coses?** Vés a `EscapeRoom > Build Demo Scene (Locked Office)` i el motor construirà una habitació de parets grises amb taules, portes, una caixa forta i claus en un sol clic.

---

## 2. 🪄 Crear Objectes Interactuables (L'Eina d'Artistes)

Vés al menú superior **`EscapeRoom > Create`** per generar objectes a l'instant, just davant d'on estigui mirant la teva càmera d'editor:

### A. Portes, Calaixos i Armaris
- **`Door`:** Porta batent estàndard. Pots definir la velocitat de gir. Té l'opció `Is Locked` per demanar una clau (ex: `clau_or`).
- **`Drawer`:** Calaix que llisca cap endavant (`Slide`). Pots posar ítems a dins (com a fills visuals) i es mouran amb ell.
- **`Cabinet`:** Porta de moble petit, gira sobre un pivot costat.
- *Tip: A qualsevol d'ells pots moure l'objecte fill `CustomPivot` per decidir sobre quin eix gira o llisca.*

### B. Notes i Pistes
- **`Readable Note`:** Un tros de paper que el jugador pot llegir a pantalla completa. Pots escriure la teva pista directament al camp `Content` de l'Inspector.

### C. Teclats Numèrics i Caixes Fortes
- **`Keypad Panel`:** Genera un teclat 3D interactiu (botó a botó).
- Defineix el codi a `Correct Code` (ex: `1984`).
- Utilitza l'esdeveniment `On Solved ()` per obrir una porta o encendre un llum en encertar-ho, arrossegant-hi l'objecte destí. Sense programar!

### D. Botons i Interruptors Genèrics
- **`Generic Trigger (Button)`:** Genera un petit botó vermell. Funciona com un interruptor (On/Off). Als seus esdeveniments `On Turned On` i `On Turned Off` hi pots penjar canvis de color, llums, etc.

---

## 3. 🎒 Inventari i Recol·lecció d'Objectes

### A. Creació d'Ítems
Utilitza **`EscapeRoom > Create > Pickable Item`** per fer un objecte que es pugui guardar a la motxilla.
- Crea l'arxiu de dades (`Clic Dret > Create > EscapeRoom > Inventory Item`). Posa-li una icona i un `Id`. Arrossega'l a l'Ítem.

### B. Combinació d'Objectes (Drag & Drop)
El jugador pot arrossegar un objecte sobre un altre dins l'inventari per combinar-los (ex: Cinta + Clau Trencada = Clau Reparada).
1. Selecciona el teu fitxer de dades (ScriptableObject) de l'Ítem 1.
2. A sota de tot, a **Combinations**, afegeix una recepta:
   - **Combine With:** Ítem 2
   - **Result Item:** L'ítem que resultarà d'ajuntar-los
   - **Destroy This / Destroy Other:** Marca'ls si es consumeixen en combinar-se.

### C. Receptors d'Objectes
**`EscapeRoom > Create > Item Receiver`**: Crea una zona on el jugador ha d'usar un objecte de l'inventari en concret (ex: un panell elèctric que demana un fusible). Si l'encerta, disparem un esdeveniment `On Item Received()`.

---

## 4. 🔍 Visor 3D d'Objectes (Item Examiner)

El jugador pot fer **Clic Dret** sobre qualsevol objecte de l'inventari per veure'l en 3D flotant a la pantalla i girar-lo amb el ratolí per buscar pistes amagades per darrere.

**Instal·lació:**
1. Vés a **`EscapeRoom > Create Examine Chamber`**.
2. Això generarà una "Habitació Invisible" al cel amb una càmera dedicada que projecta a una RenderTexture.
3. El `UIManager` ja s'encarrega d'aparèixer l'objecte màgicament i girar-lo segons el moviment del ratolí!

---

## 5. 🔊 Narrativa: Subtítols i Veus (Triggers)

**`EscapeRoom > Create > Narrative Audio Trigger`**
Genera una zona invisible verda. Quan el jugador la travessi, s'activarà el diàleg.
- Pots triar si sona `Once` (un sol cop) o `ProgressiveHints` (cada cop que ho trepitgi dirà la següent línia del diàleg).
- S'hi poden afegir clips d'àudio.
- S'hi pot escriure text amb temps (ex: 3 segons). El text apareixerà com una màquina d'escriure de forma fluida a la part baixa de la pantalla i desapareixerà suaument en acabar.

---

## 6. 💾 Sistema de Guardat (Save / Load)

El joc recorda de forma persistent la posició del jugador, què hi ha a l'inventari, quines portes estan obertes, i quins puzles s'han resolt. Funciona sol de forma transparent, basat en el component `ISaveable`.

- En l'actual mode de proves: **Prem F5 per Guardar**, i **F9 per Carregar**.

---

## 7. ✨ Feedback Visual (Outline)

Tots els objectes hereten el sistema d'Interacció automàtic. En mirar-los de prop, veuràs que apareix una vora groga al seu voltant (Outline).
A l'Inspector, apartat **Visual Feedback (Outline)**, pots desactivar-ho, canviar-ne el color (vermell per errors, verd per lliure) o l'amplada segons les teves preferències estètiques.

> [!IMPORTANT]
> **Estructura d'Escales (Scale):** Recorda sempre aplicar l'escala dels teus models 3D als objectes FILLS (`_Visuals`). Mantingues sempre el Pare (`_Logic`) a Escala `(1,1,1)` perquè el sistema de física i les rotacions no es trenquin.
