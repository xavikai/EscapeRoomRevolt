# Publica Escape Room 0.2.3 a GitHub: empaqueta, fa commit i push, crea la release
# i actualitza la guia de la release 0.2.2.
#
# Abans d'executar-lo, a Unity (projecte EscapeRoom/Unity):
#   1. Window > General > Test Runner: passa els tests EditMode i PlayMode.
#   2. Edit > Project Settings > Player > Version ha de ser 0.2.3.
#   3. Escape Room Framework > Build > Release > Build Windows.
#
# Executa'l des de l'arrel del repositori:
#   powershell -ExecutionPolicy Bypass -File EscapeRoom/Framework/tools/release-0.2.3.ps1
$ErrorActionPreference = 'Stop'
$version = '0.2.3'
$previous = '0.2.2'
$tag = "escaperoom-v$version"
$previousTag = "escaperoom-v$previous"
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$product = Join-Path $repo 'EscapeRoom'
$framework = Join-Path $product 'Framework'
$build = Join-Path $product "Unity/Builds/Release/v$version/Windows"
$windowsZip = Join-Path $product "Windows/EscapeRoom-$version-windows.zip"
$export = Join-Path $product "Exportacions/EscapeRoom-$version"
$sourceZip = Join-Path $product "PaquetsFonts/EscapeRoom-$version-source.zip"
$guide = Join-Path $product 'Documentacio/GuiaInteractiva/index.html'
$guideCopy = Join-Path $product "Windows/guia-escape-room-$version.html"
$notes = Join-Path $framework "RELEASE_NOTES_v$version.md"
$update = Join-Path $framework "UPDATE_FROM_$previous.ca.md"

function Write-Sha([string]$File) {
    $hash = (Get-FileHash -LiteralPath $File -Algorithm SHA256).Hash.ToLowerInvariant()
    Set-Content -LiteralPath "$File.sha256" -Value "$hash  $(Split-Path -Leaf $File)" -Encoding ascii
    Write-Host "  $(Split-Path -Leaf $File)  sha256 $hash"
}
function Confirm-Step([string]$Question) {
    $answer = Read-Host "$Question (s/n)"
    return $answer -match '^(s|si|sí|y|yes)$'
}
function Invoke-Checked([string]$What) {
    if ($LASTEXITCODE -ne 0) { throw "$What ha fallat (codi $LASTEXITCODE)." }
}

Set-Location $repo

# 0. Comprovacions abans de començar.
Write-Host "== Escape Room $version ==" -ForegroundColor Cyan
foreach ($tool in @('git', 'gh', 'node')) {
    if (!(Get-Command $tool -ErrorAction SilentlyContinue)) { throw "No trobo '$tool'. Instal·la'l i torna-ho a provar." }
}
gh auth status | Out-Null; Invoke-Checked 'gh auth status (inicia sessió amb: gh auth login)'
$settings = Get-Content -LiteralPath (Join-Path $product 'Unity/ProjectSettings/ProjectSettings.asset') -Raw
if ($settings -notmatch "bundleVersion: $([regex]::Escape($version))") { throw "Player Settings > Version no és $version." }
foreach ($required in @($notes, $update, $guide)) {
    if (!(Test-Path -LiteralPath $required)) { throw "Falta $required" }
}
if (!(Test-Path (Join-Path $build 'escaperoom-revolt.exe'))) { throw "No hi ha build a $build. Fes primer Build Windows a Unity." }
foreach ($required in @('escaperoom-revolt_Data', 'UnityPlayer.dll')) {
    if (!(Test-Path (Join-Path $build $required))) { throw "Build incompleta: falta $required" }
}
git rev-parse -q --verify "refs/tags/$tag" | Out-Null
if ($LASTEXITCODE -eq 0) { throw "El tag $tag ja existeix." }
$branch = (git rev-parse --abbrev-ref HEAD).Trim()
if ($branch -ne 'main') {
    Write-Warning "Ets a la branca '$branch'. La guia en línia (GitHub Pages) només s'actualitza amb push a 'main'."
    if (!(Confirm-Step "Vols continuar igualment amb '$branch'?")) { exit 1 }
}

# 1. Paquets (no se sobreescriu res que ja existeixi).
foreach ($existing in @($windowsZip, $sourceZip, $export)) {
    if (Test-Path $existing) { throw "Ja existeix i no el sobreescric: $existing" }
}
Write-Host 'Empaquetant la demo de Windows...'
$stagingRoot = [IO.Path]::GetFullPath((Join-Path $product 'Windows'))
$staging = [IO.Path]::GetFullPath((Join-Path $stagingRoot (".staging-$version-" + [guid]::NewGuid().ToString('N'))))
if (!$staging.StartsWith($stagingRoot + [IO.Path]::DirectorySeparatorChar)) { throw 'Carpeta temporal fora de Windows/.' }
New-Item -ItemType Directory -Path (Join-Path $staging 'Windows') | Out-Null
Get-ChildItem -LiteralPath $build -Force | Where-Object { $_.Name -notlike '*_BurstDebugInformation_DoNotShip' } |
    ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $staging 'Windows') -Recurse }
Compress-Archive -Path (Join-Path $staging 'Windows') -DestinationPath $windowsZip
Remove-Item -LiteralPath $staging -Recurse -Force
Write-Sha $windowsZip

Write-Host 'Exportant el projecte portable...'
node (Join-Path $framework 'tools/export-product.cjs') EscapeRoom "EscapeRoom/Exportacions/EscapeRoom-$version"
Invoke-Checked 'export-product.cjs'
$exportDocs = Join-Path $export 'docs/release'
New-Item -ItemType Directory -Path $exportDocs -Force | Out-Null
Copy-Item -LiteralPath $notes -Destination $exportDocs
Copy-Item -LiteralPath $update -Destination $exportDocs
Copy-Item -LiteralPath (Join-Path $framework 'CHANGELOG.md') -Destination $exportDocs
Copy-Item -LiteralPath $guide -Destination (Join-Path $exportDocs "guia-escape-room-$version.html")
Compress-Archive -Path (Join-Path $export '*') -DestinationPath $sourceZip
Write-Sha $sourceZip

Copy-Item -LiteralPath $guide -Destination $guideCopy -Force
Write-Sha $guideCopy

# 2. Commit i push.
Write-Host ''
$pending = git status --porcelain
if ($pending) {
Write-Host 'Canvis que es publicaran:' -ForegroundColor Cyan
git status --short
if (!(Confirm-Step "Fer commit de TOTS aquests canvis i push a '$branch'?")) {
    Write-Host 'Aturat abans del commit. Els paquets ja són a Windows/ i PaquetsFonts/.'
    exit 1
}
git add -A; Invoke-Checked 'git add'
$message = @"
Escape Room $version: portes que es movien en carregar i guia dels menus

- Door: la posicio tancada es calcula a Awake (o a LoadData si estava inactiva),
  aixi una partida carregada ja no mou portes, calaixos i tapes al centre.
- Dos tests PlayMode nous a SaveRestoreRegressionTests.
- Guia: menus pas a pas (nivell 0, taula de canvis, problemes frequents),
  preguntes frequents i enllacos a la $version.
- Notes de la versio, guia d'actualitzacio des de $previous i versio $version.

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01NKv9BBH75U48C4XU4QhMeZ
"@
$messageFile = Join-Path ([IO.Path]::GetTempPath()) "escaperoom-$version-commit.txt"
[IO.File]::WriteAllText($messageFile, $message, (New-Object Text.UTF8Encoding $false))
git commit -F $messageFile; Invoke-Checked 'git commit'
Remove-Item -LiteralPath $messageFile
} else {
    Write-Host 'No hi ha canvis pendents: ja estaven pujats.'
}
git push origin $branch; Invoke-Checked 'git push'
git tag -a $tag -m "Escape Room Revolt $version"; Invoke-Checked 'git tag'
git push origin $tag; Invoke-Checked 'git push del tag'

# 3. Release nova amb tota la documentació.
Write-Host "Creant la release $tag..."
$assets = @(
    $windowsZip, "$windowsZip.sha256",
    $sourceZip, "$sourceZip.sha256",
    "$guideCopy#Guia interactiva (CA/ES/EN/FR/RU)", "$guideCopy.sha256",
    "$update#Actualitzar des de $previous",
    "$notes#Notes de la versio $version"
)
gh release create $tag @assets --title "Escape Room Revolt $version" --notes-file $notes --verify-tag
Invoke-Checked 'gh release create'

# 4. Guia actualitzada també a la release anterior.
if (Confirm-Step "Substituir la guia HTML de la release $previousTag per la nova?") {
    $oldGuide = Join-Path ([IO.Path]::GetTempPath()) "guia-escape-room-$previous.html"
    Copy-Item -LiteralPath $guide -Destination $oldGuide -Force
    gh release upload $previousTag $oldGuide --clobber
    Invoke-Checked 'gh release upload'
    Remove-Item -LiteralPath $oldGuide
}

Write-Host ''
Write-Host "Fet. Release: https://github.com/xavikai/EscapeRoomRevolt/releases/tag/$tag" -ForegroundColor Green
if ($branch -eq 'main') {
    Write-Host 'La guia en línia s''actualitzarà en uns minuts: https://xavikai.github.io/EscapeRoomRevolt/escape-room/'
} else {
    Write-Host "La guia en línia s'actualitzarà quan facis merge de '$branch' a 'main'."
}
