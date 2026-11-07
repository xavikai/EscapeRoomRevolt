# Escape Room Revolt

Projecte Unity consumidor de paquets Revolt 0.2.0. Obre la carpeta que conté Assets, Packages i ProjectSettings amb Unity 6000.4.9f1.

La configuració pròpia és a Assets/_EscapeRoomTemplate/Resources: ProductConfiguration, GenreFeatureSettings, GameFlowSettings i VRComfortSettings. El catàleg és a ScriptableObjects/DefaultItemCatalog.asset. Les dades del jugador usen un espai de noms de producte separat.

El codi no és als Assets: el manifest referencia la base comuna de SharedPackages en desenvolupament o paquets embeguts a Packages en una exportació. Consulta product-inventory.json per veure les escenes i les dades pròpies.

Escape Room no inclou Survival; la llanterna és compartida. Survival només té demo PC validada tècnicament; VRTemplate és una mostra d’autoria, sense demo Survival Quest acceptada. El recorregut humà i el QA de visor continuen pendents.

Verificació portable del 25/10/2026: snapshot importat des de zero en una carpeta temporal fora del repositori original, sense caches copiades. Paquets Revolt i assemblies resolts només dins de la còpia; exportació original intacta. EditMode 25/25 i PlayMode 34/34, sense fallades ni omissions.

Correccions del 26/10/2026: recursos TMP complets, build Windows sense preinicialització nativa XR. EditMode 26/26, PlayMode 34/34 i reconstrucció Windows correctes. QA visual final pendent; no s’ha reprès el control d’aplicacions. Les exportacions 0.2.0 són anteriors a aquesta revisió.

Revisió portable actual: `Distributions/EscapeRoom-0.2.0-pcfix-2026-10-26`. Inclou les correccions PC i ha passat importació neta i auditoria fora del repositori, 26/26 EditMode, 34/34 PlayMode i build Windows sense preinicialització XR. Les mencions anteriors a 0.2.0 són històriques. No s’ha reprès el control d’aplicacions ni el QA visual.

QA visual manual: [guia de les dues demos](../../Framework/docs/QA_VISUAL_MANUAL_PC.md) i [plantilla de registre](../../Framework/docs/QA_VISUAL_MANUAL_PC_REGISTRE.md). Execució pendent; guia preparada sense reprendre el control de les aplicacions. Els snapshots portables validats es conserven intactes.

Lliurament EscapeRoom: ZIP amb Project portable intacte, guia de QA i registre propi. [Instruccions i evidència d’integritat](../../Framework/docs/LLIURAMENT_PC_2026-10-26.md). Casos manuals pendents; no s’ha reprès el control d’aplicacions.

Demo EscapeRoom executable sense instal·lar Unity: [paquet Windows amb runtime, guia i registre](../../Framework/docs/DEMOS_WINDOWS_2026-10-26.md). Integritat comprovada; execució manual pendent. No s’ha reprès el control de les aplicacions.
