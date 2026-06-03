# 📖 Manual d'Usuari: Escape Room Template

Benvingut/da al **Escape Room Framework**. Aquesta guia està pensada per a Artistes i Dissenyadors de Nivells. El sistema està dissenyat perquè **no hagis de programar ni una línia de codi**. Tot funciona connectant blocs des de la interfície de Unity!

---

## 1. 🏗️ Preparar l'Escena Base (Nivell Nou)

Quan obris una escena 3D completament buida, només necessites dos passos per convertir-la en un Escape Room funcional:

1. A la barra superior de Unity, vés a **`EscapeRoom > Build Player Prefab`**.
   - Això col·locarà el jugador automàticament a l'escena, amb la seva càmera, creueta, menú d'inventari i sistema de moviment.
   - *Nota: Si la teva escena ja tenia una `Main Camera` per defecte, esborra-la per evitar conflictes.*
2. Afegeix un `GameObject` buit anomenat **GameManager**, i posa-li l'script `Bootstrapper.cs`. Aquest script despertarà tots els sistemes (Inventari, Puzles, Audio) en prémer Play.

> [!TIP]
> **Vols una habitació ja muntada per provar coses?** Vés a `EscapeRoom > Generate Demo Scene` i el motor construirà una habitació de parets grises amb una taula, una porta, una caixa forta i claus en un sol clic.

---

## 2. 🪄 Crear Objectes Ràpidament (La Eina d'Artistes)

Hem creat un menú superior màgic per estalviar-te hores de feina configurant `Colliders`, `Layers` i `Scripts`. 
Vés al menú **`EscapeRoom > Create`** per generar objectes a l'instant, just davant d'on estigui mirant la teva càmera d'editor:

### A. Crear una Porta (`EscapeRoom > Create > Door`)
Genera la jerarquia correcta de pare/fill per a una porta d'habitació.
- L'script `Door.cs` està a l'arrel (`_Logic`). Allà pots definir com es mou (Pivotant o Corredissa) i la velocitat.
- **Vols moure la frontissa?** Selecciona el fill anomenat `CustomPivot` i mou-lo lliurement fins a la vora del marc de la teva porta 3D. L'script farà orbitar la porta des d'aquest punt.
- **Portes amb clau:** A l'script, marca `Is Locked = True` i escriu quina "Id" de clau necessita (ex: `clau_or`).

### B. Crear un Calaix (`EscapeRoom > Create > Drawer`)
Genera un calaix preparat per lliscar cap endavant.
- L'script ja ve configurat en mode `Slide` amb un desplaçament a l'eix Z.
- Pots posar notes o claus a dins **fent que siguin Fills (Children)** de l'objecte visual del calaix. Es mouran físicament amb ell!

### C. Crear un Armariet (`EscapeRoom > Create > Cabinet`)
Genera la porta d'un moble petit o armari de paret.
- Ja ve configurat en mode `Pivot` amb el CustomPivot ben situat i les dimensions petites.

### D. Crear una Nota de Text (`EscapeRoom > Create > Readable Note`)
Genera un tros de paper llegible.
- A l'Inspector de la nota, busca el camp `Content` i escriu-hi la història o pista que vulguis. En clicar-la durant el joc, es llegirà a pantalla completa.

### E. Crear un Objecte Recol·lectable (`EscapeRoom > Create > Pickable Item`)
Genera un ítem que va a l'inventari (claus, llanternes, eines).
- **Pas vital:** A l'Inspector d'aquest objecte, has d'arrossegar un fitxer de dades (Scriptable Object) a la casella `Item Data`.
- *Com es fa un fitxer de dades?* A la teva carpeta del projecte, fes clic dret > `Create > EscapeRoom > Inventory Item`. Posa-li una icona i un **Id** (ex: `clau_or`). Arrossega'l a l'objecte 3D!

### F. Crear una Caixa Forta / Teclat Numèric (`EscapeRoom > Create > Safe`)
Genera un puzle numèric.
- A l'Inspector, a l'script `Code Panel Puzzle`, defineix quina és la `Correct Code` (ex: `1984`).
- **Com obrir una porta en encertar el codi?** Obre l'opció `On Solved ()` de l'Inspector, dóna-li al botó `+`, arrossega la teva Porta 3D, i selecciona `Door > Unlock`. Sense programar!

---

## 3. ✨ Feedback Visual (Outline)

Tots els objectes que generis hereten el sistema d'Interacció automàtic.
Quan facis Play i miris cap a la porta o la clau, **veuràs que apareix una vora groga perfecta al seu voltant (Outline)**.

> [!NOTE]
> Aquest Outline es pot personalitzar objecte per objecte! Selecciona qualsevol objecte interactuable, i a l'Inspector veuràs la secció **Visual Feedback (Outline)**. Allà pots canviar-ne el gruix o el color si prefereixes que una clau secreta no brilli tant, o fins i tot desactivar-ho.

---

> [!IMPORTANT]
> **Estructura d'Escales (Scale):** Recorda sempre aplicar l'escala dels teus models 3D als objectes FILLS (`_Visuals`). Si canvies l'escala de l'objecte PARE (`_Logic`), les matemàtiques de les rotacions de les portes es distorsionaran! Mantingues sempre el Pare a Escala `(1,1,1)`.
