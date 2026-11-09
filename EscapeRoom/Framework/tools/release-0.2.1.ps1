# Packages Escape Room 0.2.1 for a GitHub release.
# Run from the repository root AFTER building in Unity:
#   Escape Room Framework > Build > Release > Build Windows
# Usage: powershell -ExecutionPolicy Bypass -File EscapeRoom/Framework/tools/release-0.2.1.ps1
$ErrorActionPreference = 'Stop'
$version = '0.2.1'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$product = Join-Path $repo 'EscapeRoom'
$build = Join-Path $product "Unity/Builds/Release/v$version/Windows"
$windowsZip = Join-Path $product "Windows/EscapeRoom-$version-windows.zip"
$export = Join-Path $product "Exportacions/EscapeRoom-$version"
$sourceZip = Join-Path $product "PaquetsFonts/EscapeRoom-$version-source.zip"
$guide = Join-Path $product 'Framework/docs/guia/index.html'
$guideCopy = Join-Path $product "Windows/guia-escape-room-$version.html"

function Write-Sha([string]$File) {
    $hash = (Get-FileHash -LiteralPath $File -Algorithm SHA256).Hash.ToLowerInvariant()
    Set-Content -LiteralPath "$File.sha256" -Value "$hash  $(Split-Path -Leaf $File)" -Encoding ascii
    Write-Host "  $(Split-Path -Leaf $File)  sha256 $hash"
}

# 1. Check the Unity build exists and is complete.
if (!(Test-Path (Join-Path $build 'escaperoom-revolt.exe'))) { throw "No build at $build. Build Windows in Unity first." }
foreach ($required in @('escaperoom-revolt_Data', 'UnityPlayer.dll')) {
    if (!(Test-Path (Join-Path $build $required))) { throw "Build incomplete: missing $required" }
}
foreach ($existing in @($windowsZip, $sourceZip, $export)) {
    if (Test-Path $existing) { throw "Already exists, not overwriting: $existing" }
}

# 2. Windows demo ZIP (game inside a Windows/ folder, without Burst debug info).
Write-Host 'Packaging Windows demo...'
$staging = Join-Path ([IO.Path]::GetTempPath()) "EscapeRoom-$version-windows"
if (Test-Path $staging) { Remove-Item $staging -Recurse -Force }
New-Item -ItemType Directory -Path (Join-Path $staging 'Windows') | Out-Null
Get-ChildItem -LiteralPath $build -Force | Where-Object { $_.Name -notlike '*_BurstDebugInformation_DoNotShip' } |
    ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $staging 'Windows') -Recurse }
Compress-Archive -Path (Join-Path $staging 'Windows') -DestinationPath $windowsZip
Remove-Item $staging -Recurse -Force
Write-Sha $windowsZip

# 3. Portable source project (embedded packages, no Library) and its ZIP.
Write-Host 'Exporting portable project...'
node (Join-Path $repo 'EscapeRoom/Framework/tools/export-product.cjs') EscapeRoom "EscapeRoom/Exportacions/EscapeRoom-$version"
if ($LASTEXITCODE -ne 0) { throw 'export-product.cjs failed.' }
Compress-Archive -Path (Join-Path $export '*') -DestinationPath $sourceZip
Write-Sha $sourceZip

# 4. Interactive guide as a standalone asset.
Copy-Item -LiteralPath $guide -Destination $guideCopy -Force
Write-Sha $guideCopy

Write-Host ''
Write-Host 'Done. To publish (after commit + push):'
Write-Host "  gh release create escaperoom-v$version ``"
Write-Host "    `"EscapeRoom/Windows/EscapeRoom-$version-windows.zip`" ``"
Write-Host "    `"EscapeRoom/PaquetsFonts/EscapeRoom-$version-source.zip`" ``"
Write-Host "    `"EscapeRoom/Windows/guia-escape-room-$version.html#Guia interactiva (CA/ES/EN)`" ``"
Write-Host "    --title `"Escape Room Revolt $version`" --notes-file EscapeRoom/Framework/RELEASE_NOTES_v$version.md"
