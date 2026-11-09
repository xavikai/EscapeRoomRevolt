param(
    [ValidateSet('EscapeRoom','Survival')][string]$Product = 'EscapeRoom',
    [string]$UnityPath = 'C:/Program Files/Unity/Hub/Editor/6000.4.9f1/Editor/Unity.exe',
    [switch]$BuildWindows
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'WorkspaceLayout.ps1')
$taskProject = Get-ProductRoot $Product
$taskOutput = Join-Path $taskProject '.utmp'
New-Item -ItemType Directory -Path $taskOutput -Force | Out-Null
function Invoke-ProductUnity([string]$Label, [string[]]$ExtraArguments) {
    $taskLog = Join-Path $taskOutput "$Label.log"
    $taskArguments = @('-batchmode','-nographics','-projectPath', ('"{0}"' -f $taskProject), '-logFile', ('"{0}"' -f $taskLog)) + $ExtraArguments
    $taskRun = Start-Process -FilePath $UnityPath -ArgumentList $taskArguments -WorkingDirectory $taskProject -WindowStyle Hidden -Wait -PassThru
    if ($taskRun.ExitCode -ne 0) { throw "$Product $Label failed (exit $($taskRun.ExitCode)); see $taskLog" }
    Write-Output "$Product $Label PASS"
}
if ($Product -eq 'Survival') { Invoke-ProductUnity 'navigation' @('-quit','-executeMethod','EscapeRoomRevolt.EditorTools.SurvivalNavigationTools.RepairDemoNavigation') }
Invoke-ProductUnity 'modules' @('-quit','-executeMethod','EscapeRoomRevolt.EditorTools.ProductModuleValidator.ValidateBatch')
foreach ($taskMode in @('EditMode','PlayMode')) {
    $taskResults = Join-Path $taskOutput "$taskMode.xml"
    Invoke-ProductUnity $taskMode @('-runTests','-testPlatform',$taskMode,'-testResults',('"{0}"' -f $taskResults))
    [xml]$taskXml = Get-Content -Raw -LiteralPath $taskResults
    if ($taskXml.'test-run'.result -ne 'Passed' -or [int]$taskXml.'test-run'.skipped -ne 0) { throw "$Product $taskMode has failures or skipped tests." }
}
if ($BuildWindows) {
    Invoke-ProductUnity 'windows' @('-quit','-executeMethod','EscapeRoomRevolt.EditorTools.ReleaseBuilder.BuildWindows')
    $taskInventory = Get-Content -Raw -LiteralPath (Join-Path $taskProject 'product-inventory.json') | ConvertFrom-Json
    $taskSettings = Get-Content -Raw -LiteralPath (Join-Path $taskProject 'ProjectSettings/ProjectSettings.asset')
    $taskVersion = [regex]::Match($taskSettings, '(?m)^\s*bundleVersion:\s*(\S+)').Groups[1].Value
    if (!$taskVersion) { throw 'Cannot determine product version from PlayerSettings.' }
    $taskBinaryFolder = Join-Path $taskProject "Builds/Release/v$taskVersion/Windows"
    $taskBinary = Join-Path $taskBinaryFolder ($taskInventory.productId + '.exe')
    $taskBoot = Join-Path $taskBinaryFolder ($taskInventory.productId + '_Data/boot.config')
    if (!(Test-Path -LiteralPath $taskBinary) -or !(Test-Path -LiteralPath $taskBoot)) { throw 'Expected Windows artifacts are missing.' }
    if ((Get-Content -Raw -LiteralPath $taskBoot) -match '(?m)^xrsdk-pre-init-library=\S+') { throw 'Desktop build still requests native XR pre-initialization.' }
    @{
        product = $Product
        result = 'Passed'
        binary = [IO.Path]::GetRelativePath($taskProject, $taskBinary).Replace('\','/')
        binarySha256 = (Get-FileHash -LiteralPath $taskBinary -Algorithm SHA256).Hash.ToLowerInvariant()
        bootConfigSha256 = (Get-FileHash -LiteralPath $taskBoot -Algorithm SHA256).Hash.ToLowerInvariant()
        nativeXRPreInitialization = $false
    } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $taskOutput 'windows-verification.json') -Encoding utf8
    Write-Output "$Product Windows boot configuration PASS (no native XR pre-initialization)"
}
