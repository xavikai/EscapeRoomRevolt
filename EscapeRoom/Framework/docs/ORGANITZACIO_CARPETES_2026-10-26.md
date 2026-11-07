# Organització en dues carpetes — 26/10/2026

L'arrel del repositori té dues carpetes principals visibles, `EscapeRoom` i `Survival`, amb un índex README. `.git`, `.gitignore` i `.claude` continuen a l'arrel com a metadades del repositori i de les eines.

```text
EscapeRoom_Revolt/
  README.md
  EscapeRoom/
    README.md
    Unity/                 projecte consumidor Escape Room
    Documentacio/          guia i registre QA propis
    Exportacions/          snapshots portables, inclosos els històrics
    PaquetsFonts/          lliuraments font i ZIPs amb QA
    Windows/               demos executables i ZIPs amb QA
    Framework/             base comuna, eines i autoria històrica
      Assets/ Packages/ ProjectSettings/  autoria Unity històrica
      SharedPackages/      codi canònic Common i Survival
      tools/ docs/         eines i documentació tècnica compartides
  Survival/
    README.md
    Unity/                 projecte consumidor Survival
    Documentacio/
    Exportacions/
    PaquetsFonts/
    Windows/
```

La ubicació de Framework sota EscapeRoom permet mantenir les dues carpetes principals sense duplicar la base comuna. Framework continua sent l'espai tècnic d'autoria dels dos productes; per editar les demos actuals usar els dos directoris `Unity/`. La base compartida no s'ha convertit en una dependència de Survival cap al runtime Escape Room: segueix sent el paquet Common, i Survival instal·la a més el seu paquet específic.

## Rutes anteriors i actuals

| Abans | Ara |
|---|---|
| `Projects/EscapeRoom` | `EscapeRoom/Unity` |
| `Projects/Survival` | `Survival/Unity` |
| `SharedPackages` | `EscapeRoom/Framework/SharedPackages` |
| `tools`, `docs` | `EscapeRoom/Framework/tools`, `EscapeRoom/Framework/docs` |
| `Assets`, `Packages`, `ProjectSettings` de l'arrel | dins de `EscapeRoom/Framework` |
| `Distributions/EscapeRoom-*` | `EscapeRoom/Exportacions/EscapeRoom-*` |
| `Distributions/Survival-*` | `Survival/Exportacions/Survival-*` |
| `Distributions/Deliveries/<producte>-*` | `<producte>/PaquetsFonts/<producte>-*` |
| `Distributions/WindowsDemos/<producte>-*` | `<producte>/Windows/<producte>-*` |
| `Projects/<producte>/Builds/...` | `<producte>/Unity/Builds/...` |

Els informes JSON i els documents dins dels snapshots/ZIP mantenen les rutes originals com a evidència històrica. No s'han reescrit els arxius lliurats. [L'informe de reorganització](validation/workspace-layout-2026-10-26.json) relaciona les ubicacions antigues i actuals i registra les comprovacions d'integritat.

## Canvis tècnics i comprovacions

S'han traslladat els fitxers dins del mateix repositori, preservant assets i GUIDs. Només els manifests i locks dels projectes de treball han canviat les rutes locals UPM. Les eines comparteixen resolució de rutes a `workspace-layout.cjs` i `WorkspaceLayout.ps1`; els empaquetadors poden interpretar les rutes dels informes previs. Els enllaços locals de la documentació activa s'han adaptat.

Les caches Library s'han conservat amb els seus projectes; Unity pot actualitzar-les en la propera importació. No s'han rellançat importacions, tests ni builds després del trasllat: s'han fet comprovacions estàtiques de rutes, paquets, GUIDs, scripts i integritat dels artefactes. Els resultats batch anteriors no s'han presentat com a proves noves. No s'ha reprès el control de les aplicacions; el QA visual continua pendent.

Per usar les eines des de l'arrel:

```powershell
node EscapeRoom/Framework/tools/check-products.cjs
node EscapeRoom/Framework/tools/export-product.cjs EscapeRoom
node EscapeRoom/Framework/tools/export-product.cjs Survival
pwsh -File EscapeRoom/Framework/tools/validate-portable.ps1 -Product EscapeRoom -BuildWindows
pwsh -File EscapeRoom/Framework/tools/validate-portable.ps1 -Product Survival -BuildWindows
```

Les eines de creació/exportació preserven directoris existents i rebutgen sobreescriure'ls. Els exemples històrics `node tools/...` s'han d'executar des de `EscapeRoom/Framework`, o prefixar amb aquella carpeta si es parteix de l'arrel. Els empaquetadors actuals preserven igualment els informes i ZIPs existents.
