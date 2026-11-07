# Revolt Common — 0.2.0

Canonical source package consumed by `Projects/EscapeRoom`, `Projects/Survival` and the legacy authoring project. Runtime, Editor, UI templates, shaders, shared input/localization defaults and tests are maintained here once. No Survival assembly dependency.

Each consumer owns its scene list, item catalog, Resources configuration, application identity and gameplay assets. Shared templates are resolved by `FrameworkAssetPaths` after checking project overrides. Editor writes remain inside consumer `Assets`, never inside this package.

`ProductConfiguration` configures release scenes, title and data namespace. Without it, the legacy authoring project keeps its existing behavior. Do not ship product-specific configuration assets in this package's Resources.

Set this package in the consumer manifest's `testables` array to enable the EditMode and PlayMode suites. `tools/validate-product.ps1` runs tests and product reference checks. XR remains required by Player/Systems; an installation without XR is outside this extraction.
