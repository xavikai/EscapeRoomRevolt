param(
    [string]$Revision = '0.2.0-pcfix-2026-10-26',
    [string]$OutputDirectory = ''
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'WorkspaceLayout.ps1')

$taskEvidencePath = Join-Path $taskRepo 'docs/validation/portable-exports-2026-10-26.json'
$taskEvidence = Get-Content -Raw -LiteralPath $taskEvidencePath | ConvertFrom-Json
$taskReportPath = Join-Path $taskRepo 'docs/validation/deliveries-2026-10-26.json'
if (Test-Path -LiteralPath $taskReportPath) { throw "Existing delivery report preserved: $taskReportPath" }
if ($Revision -ne $taskEvidence.snapshotRevision.Insert(0, '0.2.0-')) { throw 'Revision does not match the verified portable snapshots.' }
function Get-TreeDigest([string]$Root) {
    $taskLines = Get-ChildItem -LiteralPath $Root -Recurse -File | Sort-Object FullName | ForEach-Object {
        $taskRelative = [IO.Path]::GetRelativePath($Root, $_.FullName).Replace('\','/')
        "$taskRelative $((Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash)"
    }
    [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes(($taskLines -join "`n")))).ToLowerInvariant()
}
# Check every input and output before creating any delivery.
foreach ($taskProduct in @('EscapeRoom','Survival')) {
    $taskDestination = Get-DeliveryRoot $taskProduct 'PaquetsFonts' $OutputDirectory
    $taskResult = $taskEvidence.products.$taskProduct
    if ($taskResult.result -ne 'Passed' -or !$taskResult.exportUnchanged -or $taskResult.windows.result -ne 'Succeeded') { throw "Missing validation for $taskProduct" }
    $taskSource = Resolve-HistoricalPath $taskResult.export
    if ((Get-TreeDigest $taskSource) -ne $taskResult.originalExportSha256) { throw "$taskProduct snapshot differs from the validated export." }
    foreach ($taskSuffix in @('', '.zip', '.zip.sha256')) {
        $taskCandidate = Join-Path $taskDestination "$taskProduct-$Revision-delivery$taskSuffix"
        if (Test-Path -LiteralPath $taskCandidate) { throw "Existing delivery preserved: $taskCandidate" }
    }
}
$taskReport = [ordered]@{ date = '2026-10-26'; revision = $Revision; applicationControlResumed = $false; unityRerun = $false; method = 'Package previously verified portable snapshots unchanged; read back every ZIP file entry and compare SHA-256, exact file list and safe relative paths. Manual QA remains pending.'; products = [ordered]@{} }
foreach ($taskProduct in @('EscapeRoom','Survival')) {
    $taskDestination = Get-DeliveryRoot $taskProduct 'PaquetsFonts' $OutputDirectory
    $taskResult = $taskEvidence.products.$taskProduct
    $taskSource = Resolve-HistoricalPath $taskResult.export
    $taskName = "$taskProduct-$Revision-delivery"
    New-Item -ItemType Directory -Path $taskDestination -Force | Out-Null
    $taskRoot = Join-Path $taskDestination $taskName
    New-Item -ItemType Directory -Path $taskRoot | Out-Null
    Copy-Item -LiteralPath $taskSource -Destination (Join-Path $taskRoot 'Project') -Recurse
    if ((Get-TreeDigest (Join-Path $taskRoot 'Project')) -ne $taskResult.originalExportSha256) { throw 'Packaged project differs from the validated source.' }
    $taskDocs = Join-Path $taskRoot 'docs'
    New-Item -ItemType Directory -Path (Join-Path $taskDocs 'validation') -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $taskRepo 'docs/QA_VISUAL_MANUAL_PC.md') -Destination $taskDocs
    Copy-Item -LiteralPath $taskEvidencePath -Destination (Join-Path $taskDocs 'validation')
    Copy-Item -LiteralPath (Join-Path $taskRepo 'docs/validation/qa-corrections-2026-10-26.json') -Destination (Join-Path $taskDocs 'validation')
    $taskTemplate = Get-Content -Raw -LiteralPath (Join-Path $taskRepo 'docs/QA_VISUAL_MANUAL_PC_REGISTRE.md')
    $taskExcludedPrefix = if ($taskProduct -eq 'EscapeRoom') { 'S' } else { 'E' }
    $taskTemplate = [regex]::Replace($taskTemplate, "(?m)^\| $taskExcludedPrefix\d{2} \|[^\r\n]*\r?\n", '')
    $taskTemplate = $taskTemplate.Replace('# Plantilla de registre — QA manual PC', "# Plantilla de registre — QA manual PC — $taskProduct")
    $taskTemplate = $taskTemplate.Replace('| Producte / origen (consumidor o snapshot) | |', "| Producte / origen (consumidor o snapshot) | $taskProduct / Project (snapshot $Revision) |")
    $taskTemplate = $taskTemplate.Replace('Duplicar les files comunes per al segon producte si es registra tot en un sol fitxer.', "Aquest registre conté els casos comuns i els del producte lliurat. La prova C12 requereix també el paquet de l'altre producte; si no està disponible, registrar-la com a BLOQUEJAT.")
    Set-Content -LiteralPath (Join-Path $taskDocs 'QA_VISUAL_MANUAL_PC_REGISTRE.md') -Value $taskTemplate -Encoding utf8
    $taskExe = if ($taskProduct -eq 'EscapeRoom') { 'escaperoom-revolt.exe' } else { 'survival-revolt.exe' }
    $taskReadme = @"
# Lliurament $taskProduct — $Revision

## Contingut

- Project/: projecte Unity portable complet, copiat sense canvis del snapshot validat. Paquets Revolt embeguts; no necessita el repositori original.
- [Guia de QA manual](docs/QA_VISUAL_MANUAL_PC.md): passos i resultats esperats per a les dues demos; aplicar els comuns i els de $taskProduct.
- [Registre propi](docs/QA_VISUAL_MANUAL_PC_REGISTRE.md): copiar abans d'omplir; tots els casos manuals continuen PENDENT.
- docs/validation/: informes batch previs. FILES.sha256.json: inventari i hashes del contingut, excloent el mateix inventari.

## Ús

Extreu el ZIP complet a una carpeta d'escriptura. Obre Project amb Unity 6000.4.9f1 quan vulguis importar-lo. Cal la instal·lació i llicència normals de Unity i les dependències externes del manifest/lock (registre o cache global); no s'inclouen Unity, Library, logs ni executables.

Per generar Windows en mode batch, amb el mòdul Windows Build Support instal·lat:

``````powershell
& 'C:/Program Files/Unity/Hub/Editor/6000.4.9f1/Editor/Unity.exe' -batchmode -nographics -quit -projectPath '<carpeta extreta>/Project' -executeMethod EscapeRoomRevolt.EditorTools.ReleaseBuilder.BuildWindows -logFile '<carpeta extreta>/windows-build.log'
``````

El resultat és Project/Builds/Release/v0.1.0-beta.5/Windows/$taskExe amb les seves dades adjuntes. El nom beta.5 és històric. Abans de fer QA, identifica la build i el seu hash al registre. Una reconstrucció local pot tenir un hash diferent dels executables dels informes previs.

La guia cita rutes del repositori per traçabilitat; dins del lliurament, les dades són a Project/Assets i el codi a Project/Packages. Els informes inclosos són evidència de les importacions/proves/builds ja executades sobre el snapshot, no una acceptació visual. Aquest empaquetatge només verifica integritat i contingut: no ha rellançat Unity ni reprès el control d'aplicacions. Els snapshots originals i el projecte dins del paquet es mantenen intactes.
"@
    Set-Content -LiteralPath (Join-Path $taskRoot 'README.md') -Value $taskReadme -Encoding utf8
    $taskFiles = @(Get-ChildItem -LiteralPath $taskRoot -Recurse -File | Sort-Object FullName | ForEach-Object {
        [ordered]@{ path = [IO.Path]::GetRelativePath($taskRoot, $_.FullName).Replace('\','/'); bytes = $_.Length; sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
    })
    $taskFiles | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $taskRoot 'FILES.sha256.json') -Encoding utf8
    $taskExpected = @{}
    foreach ($taskFile in (Get-ChildItem -LiteralPath $taskRoot -Recurse -File)) { $taskExpected[[IO.Path]::GetRelativePath($taskRoot, $taskFile.FullName).Replace('\','/')] = (Get-FileHash -LiteralPath $taskFile.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
    $taskZip = "$taskRoot.zip"
    [IO.Compression.ZipFile]::CreateFromDirectory($taskRoot, $taskZip, [IO.Compression.CompressionLevel]::Optimal, $true)
    $taskArchive = [IO.Compression.ZipFile]::OpenRead($taskZip)
    $taskVerified = 0
    $taskSeen = [Collections.Generic.HashSet[string]]::new()
    try {
        foreach ($taskEntry in $taskArchive.Entries) {
            if ($taskEntry.FullName.EndsWith('/')) { continue }
            if (!$taskEntry.FullName.StartsWith("$taskName/") -or $taskEntry.FullName -match '(^|/)\.\.(/|$)|\\|:') { throw 'Unsafe ZIP entry.' }
            $taskRelative = $taskEntry.FullName.Substring($taskName.Length + 1)
            if (!$taskExpected.ContainsKey($taskRelative) -or !$taskSeen.Add($taskRelative)) { throw "Unexpected or duplicate ZIP entry: $taskRelative" }
            $taskStream = $taskEntry.Open()
            try { $taskHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($taskStream)).ToLowerInvariant() } finally { $taskStream.Dispose() }
            if ($taskHash -ne $taskExpected[$taskRelative]) { throw "ZIP hash mismatch: $taskRelative" }
            $taskVerified++
        }
    } finally { $taskArchive.Dispose() }
    if ($taskVerified -ne $taskExpected.Count) { throw 'ZIP is missing files.' }
    if ((Get-TreeDigest $taskSource) -ne $taskResult.originalExportSha256) { throw 'Source snapshot changed during packaging.' }
    $taskZipHash = (Get-FileHash -LiteralPath $taskZip -Algorithm SHA256).Hash.ToLowerInvariant()
    "$taskZipHash  $taskName.zip" | Set-Content -LiteralPath "$taskZip.sha256" -Encoding ascii
    $taskReport.products[$taskProduct] = [ordered]@{
        result = 'Passed'; archive = [IO.Path]::GetRelativePath($taskWorkspace, $taskZip).Replace('\','/'); bytes = (Get-Item -LiteralPath $taskZip).Length; archiveSha256 = $taskZipHash
        sourceExport = $taskResult.export; sourceExportSha256 = $taskResult.originalExportSha256; sourceUnchanged = $true; packagedProjectMatchesValidatedExport = $true
        zipFilesVerified = $taskVerified; manualCasesPending = if ($taskProduct -eq 'EscapeRoom') { 28 } else { 25 }
        priorEditModePassed = $taskResult.editMode.passed; priorPlayModePassed = $taskResult.playMode.passed; priorWindowsBuild = $taskResult.windows.result
    }
    Write-Output "$taskProduct delivery PASS: $taskZip ($taskVerified files read back and hashed)"
}
$taskReport | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $taskReportPath -Encoding utf8
Write-Output "Delivery report: $taskReportPath"
