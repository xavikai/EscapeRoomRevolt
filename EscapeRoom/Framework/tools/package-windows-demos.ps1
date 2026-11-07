param([string]$OutputDirectory = '')
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'WorkspaceLayout.ps1')

$taskRevision = '0.2.0-pcfix-2026-10-26'
$taskEvidenceFile = Join-Path $taskRepo 'docs/validation/qa-corrections-2026-10-26.json'
$taskEvidence = Get-Content -Raw -LiteralPath $taskEvidenceFile | ConvertFrom-Json
$taskReportPath = Join-Path $taskRepo 'docs/validation/windows-demos-2026-10-26.json'
if (Test-Path -LiteralPath $taskReportPath) { throw "Existing report preserved: $taskReportPath" }
function Get-Hash([string]$File) { (Get-FileHash -LiteralPath $File -Algorithm SHA256).Hash.ToLowerInvariant() }
function Get-Manifest([string]$Root) {
    $taskPaths = [Collections.Generic.List[string]]::new()
    foreach ($taskFile in (Get-ChildItem -LiteralPath $Root -Recurse -File)) { $taskPaths.Add([IO.Path]::GetRelativePath($Root, $taskFile.FullName).Replace('\','/')) }
    $taskPaths.Sort([StringComparer]::Ordinal)
    foreach ($taskPath in $taskPaths) { [ordered]@{ path = $taskPath; bytes = (Get-Item -LiteralPath (Join-Path $Root $taskPath)).Length; sha256 = Get-Hash (Join-Path $Root $taskPath) } }
}
function Get-Digest($Manifest) {
    $taskLines = @($Manifest | ForEach-Object { "$($_.path) $($_.sha256)" })
    [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes(($taskLines -join "`n")))).ToLowerInvariant()
}
# Preflight both builds before writing any delivery files.
foreach ($taskProduct in @('EscapeRoom','Survival')) {
    $taskOutput = Get-DeliveryRoot $taskProduct 'Windows' $OutputDirectory
    $taskBuild = $taskEvidence.products.$taskProduct.windows
    $taskExe = Resolve-HistoricalPath $taskBuild.binary
    $taskBuildRoot = Split-Path -Parent $taskExe
    $taskData = Join-Path $taskBuildRoot (([IO.Path]::GetFileNameWithoutExtension($taskExe)) + '_Data')
    if ($taskBuild.result -ne 'Succeeded' -or (Get-Hash $taskExe) -ne $taskBuild.binarySha256 -or (Get-Hash (Resolve-HistoricalPath $taskBuild.bootConfig)) -ne $taskBuild.bootConfigSha256) { throw "$taskProduct does not match the verified Windows build." }
    if ((Get-Digest @(Get-Manifest $taskData)) -ne $taskBuild.dataTreeSha256) { throw "$taskProduct data differs from the verified build." }
    foreach ($taskRequired in @('UnityPlayer.dll','UnityCrashHandler64.exe','MonoBleedingEdge','D3D12')) { if (!(Test-Path -LiteralPath (Join-Path $taskBuildRoot $taskRequired))) { throw "Missing runtime: $taskRequired" } }
    if ((Get-Content -Raw -LiteralPath (Resolve-HistoricalPath $taskBuild.bootConfig)) -match '(?m)^xrsdk-pre-init-library=\S+') { throw 'Native XR pre-init is enabled.' }
    foreach ($taskSuffix in @('', '.zip', '.zip.sha256')) { if (Test-Path -LiteralPath (Join-Path $taskOutput "$taskProduct-$taskRevision-windows$taskSuffix")) { throw 'Existing Windows delivery preserved.' } }
}
$taskReport = [ordered]@{ date='2026-10-26'; revision=$taskRevision; applicationControlResumed=$false; unityRerun=$false; executableLaunched=$false; method='Verify EXE, boot.config and entire Data tree against prior successful build evidence. Copy all runtime files except Burst DoNotShip diagnostics. Read and hash every ZIP entry, verify exact paths and file list. Manual visual QA remains pending.'; products=[ordered]@{} }
foreach ($taskProduct in @('EscapeRoom','Survival')) {
    $taskOutput = Get-DeliveryRoot $taskProduct 'Windows' $OutputDirectory
    $taskBuild = $taskEvidence.products.$taskProduct.windows
    $taskSource = Split-Path -Parent (Resolve-HistoricalPath $taskBuild.binary)
    $taskExeName = [IO.Path]::GetFileName($taskBuild.binary)
    $taskDataName = [IO.Path]::GetFileNameWithoutExtension($taskExeName) + '_Data'
    $taskOriginalManifest = @(Get-Manifest $taskSource)
    $taskOriginalDigest = Get-Digest $taskOriginalManifest
    $taskName = "$taskProduct-$taskRevision-windows"
    New-Item -ItemType Directory -Path $taskOutput -Force | Out-Null
    $taskRoot = Join-Path $taskOutput $taskName
    $taskGame = Join-Path $taskRoot 'Windows'
    New-Item -ItemType Directory -Path $taskGame -Force | Out-Null
    $taskExcluded = @()
    foreach ($taskItem in (Get-ChildItem -LiteralPath $taskSource -Force)) {
        if ($taskItem.Name -like '*_BurstDebugInformation_DoNotShip') { $taskExcluded += $taskItem.Name; continue }
        Copy-Item -LiteralPath $taskItem.FullName -Destination $taskGame -Recurse
    }
    $taskRuntimeManifest = @(Get-Manifest $taskGame)
    $taskExpectedRuntime = @($taskOriginalManifest | Where-Object { $taskTop = $_.path.Split('/')[0]; $taskTop -notin $taskExcluded })
    if ((Get-Digest $taskRuntimeManifest) -ne (Get-Digest $taskExpectedRuntime)) { throw 'Runtime copy differs from build.' }
    $taskDocs = Join-Path $taskRoot 'docs'
    New-Item -ItemType Directory -Path (Join-Path $taskDocs 'validation') -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $taskRepo 'docs/QA_VISUAL_MANUAL_PC.md') -Destination $taskDocs
    Copy-Item -LiteralPath $taskEvidenceFile -Destination (Join-Path $taskDocs 'validation')
    Copy-Item -LiteralPath (Join-Path $taskRepo 'docs/validation/portable-exports-2026-10-26.json') -Destination (Join-Path $taskDocs 'validation')
    $taskTemplate = Get-Content -Raw -LiteralPath (Join-Path $taskRepo 'docs/QA_VISUAL_MANUAL_PC_REGISTRE.md')
    $taskExcludedPrefix = if ($taskProduct -eq 'EscapeRoom') { 'S' } else { 'E' }
    $taskTemplate = [regex]::Replace($taskTemplate, "(?m)^\| $taskExcludedPrefix\d{2} \|[^\r\n]*\r?\n", '')
    $taskTemplate = $taskTemplate.Replace('# Plantilla de registre — QA manual PC', "# Plantilla de registre — QA manual PC — $taskProduct (Windows)")
    $taskTemplate = $taskTemplate.Replace('| Producte / origen (consumidor o snapshot) | |', "| Producte / origen (consumidor o snapshot) | $taskProduct / Windows/$taskExeName |")
    $taskTemplate = $taskTemplate.Replace('| Revisió / ruta EXE / SHA-256 | |', "| Revisió / ruta EXE / SHA-256 | $taskRevision / Windows/$taskExeName / $($taskBuild.binarySha256) |")
    $taskTemplate = $taskTemplate.Replace('Duplicar les files comunes per al segon producte si es registra tot en un sol fitxer.', "Aquest registre inclou els casos comuns i els del producte lliurat. C12 necessita els dos paquets; si falta l'altre, registrar BLOQUEJAT.")
    Set-Content -LiteralPath (Join-Path $taskDocs 'QA_VISUAL_MANUAL_PC_REGISTRE.md') -Value $taskTemplate -Encoding utf8
    $taskReadme = @"
# Demo Windows — $taskProduct — $taskRevision

## Inici ràpid (sense instal·lar Unity)

1. Extreu TOT el ZIP a una carpeta d'escriptura del PC Windows de 64 bits. No executis el joc des del visor de fitxers comprimits.
2. Obre Windows/$taskExeName. Conserva al costat la carpeta $taskDataName, UnityPlayer.dll, MonoBleedingEdge, D3D12 i la resta de fitxers: l'EXE sol no és el joc complet.
3. Teclat i ratolí: WASD moure, ratolí mirar, E interactuar, I inventari, Esc tancar panell/pausa. La guia detalla la resta de controls. No cal Unity Editor, Hub ni visor VR per provar aquesta demo PC.
4. Copia [el registre propi](docs/QA_VISUAL_MANUAL_PC_REGISTRE.md) per a la sessió i segueix [la guia de QA](docs/QA_VISUAL_MANUAL_PC.md), aplicant els casos comuns i els d'aquest producte.

La preparació/importació de projectes de la guia s'aplica als lliuraments font: en aquest paquet executable es comença directament al pas d'identificar la build i executar-la. Les fonts i els scripts d'Editor no s'inclouen.

## Identificació i evidències

EXE SHA-256: $($taskBuild.binarySha256). El mateix llançador Unity pot compartir hash entre productes; identifica també el paquet, el seu manifest i totes les dades adjuntes.

Player.log després de la sessió: %USERPROFILE%/AppData/LocalLow/XaviKai/$(if ($taskProduct -eq 'EscapeRoom') { 'Escape Room Revolt' } else { 'Survival Revolt' })/Player.log. Copia'l abans de tornar a arrencar el joc. Les dades de jugador són externes al ZIP: extreure de nou el paquet no garanteix una partida neta. No esborris partides personals sense còpia.

FILES.sha256.json inventaria el contingut (exclou el mateix inventari). El fitxer .zip.sha256 adjacent al ZIP comprova l'arxiu complet. docs/validation conté les proves batch prèvies. No s'ha executat l'EXE ni reprès el control d'aplicacions durant l'empaquetatge; tots els casos manuals continuen pendents. No hi ha acceptació visual ni certificació de maquinari.
"@
    Set-Content -LiteralPath (Join-Path $taskRoot 'README.md') -Value $taskReadme -Encoding utf8
    @(Get-Manifest $taskRoot) | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $taskRoot 'FILES.sha256.json') -Encoding utf8
    $taskExpected = @{}
    foreach ($taskFile in @(Get-Manifest $taskRoot)) { $taskExpected[$taskFile.path] = $taskFile.sha256 }
    $taskZip = "$taskRoot.zip"
    [IO.Compression.ZipFile]::CreateFromDirectory($taskRoot, $taskZip, [IO.Compression.CompressionLevel]::Optimal, $true)
    $taskArchive = [IO.Compression.ZipFile]::OpenRead($taskZip)
    $taskSeen = [Collections.Generic.HashSet[string]]::new()
    try {
        foreach ($taskEntry in $taskArchive.Entries) {
            if ($taskEntry.FullName.EndsWith('/')) { continue }
            if (!$taskEntry.FullName.StartsWith("$taskName/") -or $taskEntry.FullName -match '(^|/)\.\.(/|$)|\\|:') { throw 'Unsafe ZIP entry.' }
            $taskRelative = $taskEntry.FullName.Substring($taskName.Length + 1)
            if (!$taskExpected.ContainsKey($taskRelative) -or !$taskSeen.Add($taskRelative)) { throw 'Unexpected or duplicate ZIP file.' }
            $taskStream = $taskEntry.Open()
            try { $taskHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($taskStream)).ToLowerInvariant() } finally { $taskStream.Dispose() }
            if ($taskHash -ne $taskExpected[$taskRelative]) { throw "ZIP hash mismatch: $taskRelative" }
        }
    } finally { $taskArchive.Dispose() }
    if ($taskSeen.Count -ne $taskExpected.Count) { throw 'Missing ZIP files.' }
    if ((Get-Digest @(Get-Manifest $taskSource)) -ne $taskOriginalDigest) { throw 'Original build changed.' }
    $taskZipHash = Get-Hash $taskZip
    "$taskZipHash  $taskName.zip" | Set-Content -LiteralPath "$taskZip.sha256" -Encoding ascii
    $taskReport.products[$taskProduct] = [ordered]@{
        result='Passed'; archive=[IO.Path]::GetRelativePath($taskWorkspace,$taskZip).Replace('\','/'); bytes=(Get-Item -LiteralPath $taskZip).Length; archiveSha256=$taskZipHash
        sourceBinary=$taskBuild.binary; binarySha256=$taskBuild.binarySha256; bootConfigSha256=$taskBuild.bootConfigSha256; dataTreeSha256=$taskBuild.dataTreeSha256
        originalBuildTreeSha256=$taskOriginalDigest; packagedRuntimeTreeSha256=(Get-Digest $taskRuntimeManifest); excludedDiagnostics=$taskExcluded
        runtimeFiles=$taskRuntimeManifest.Count; zipFilesVerified=$taskSeen.Count; originalBuildUnchanged=$true; nativeXRPreInitialization=$false
        manualCasesPending=$(if ($taskProduct -eq 'EscapeRoom') { 28 } else { 25 }); priorWindowsBuild=$taskBuild.result
    }
    Write-Output "$taskProduct Windows package PASS: $taskZip ($($taskSeen.Count) files read back and hashed)"
}
$taskReport | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $taskReportPath -Encoding utf8
Write-Output "Report: $taskReportPath"
