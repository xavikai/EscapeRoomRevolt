param(
    [ValidateSet('EscapeRoom','Survival')][string]$Product = 'EscapeRoom',
    [string]$UnityPath = 'C:/Program Files/Unity/Hub/Editor/6000.4.9f1/Editor/Unity.exe',
    [string]$ExportPath,
    [switch]$BuildWindows
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'WorkspaceLayout.ps1')
$taskExport = Get-PortableRoot $Product
if ($ExportPath) { $taskExport = [IO.Path]::GetFullPath($ExportPath, $taskWorkspace) }
$taskRunId = [Guid]::NewGuid().ToString('N')
$taskProject = Join-Path ([IO.Path]::GetTempPath()) "RevoltPortable-$Product-$taskRunId"
$taskRecordFolder = Join-Path $taskRepo '.utmp'
New-Item -ItemType Directory -Path $taskRecordFolder -Force | Out-Null
if (!(Test-Path -LiteralPath (Join-Path $taskExport 'EXPORT.json'))) { throw "Missing portable export: $taskExport" }
foreach ($taskCache in @('Library','Temp','Logs','obj')) {
    if (Test-Path -LiteralPath (Join-Path $taskExport $taskCache)) { throw "Export is not clean: $taskCache is present." }
}
if (Test-Path -LiteralPath $taskProject) { throw "Existing validation copy preserved: $taskProject" }
function Get-ExportDigest {
    $taskEntries = Get-ChildItem -LiteralPath $taskExport -Recurse -File | Sort-Object FullName | ForEach-Object {
        $taskRelative = [IO.Path]::GetRelativePath($taskExport, $_.FullName).Replace('\','/')
        $taskHash = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
        "$taskRelative $taskHash"
    }
    $taskBytes = [Text.Encoding]::UTF8.GetBytes(($taskEntries -join "`n"))
    [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($taskBytes)).ToLowerInvariant()
}
$taskBeforeDigest = Get-ExportDigest
Copy-Item -LiteralPath $taskExport -Destination $taskProject -Recurse
$taskFresh = !(Test-Path -LiteralPath (Join-Path $taskProject 'Library'))
$taskOutput = Join-Path $taskProject '.utmp'
New-Item -ItemType Directory -Path $taskOutput -Force | Out-Null
$taskEditorFolder = Join-Path $taskProject 'Assets/PortableVerification/Editor'
New-Item -ItemType Directory -Path $taskEditorFolder -Force | Out-Null
# This audit exists only in the temporary test copy; the exported package snapshot stays untouched.
$taskAuditSource = @'
#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using UnityEngine;

public static class PortableIsolationAudit
{
    [Serializable] private sealed class PackageRecord { public string name; public string version; public string source; public string resolvedPath; }
    [Serializable] private sealed class Report { public string result; public string projectRoot; public bool noOriginalRepositoryPaths; public List<PackageRecord> packages = new(); public List<string> assemblyLocations = new(); }
    public static void Run()
    {
        string project = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string[] args = Environment.GetCommandLineArgs();
        int originalIndex = Array.IndexOf(args, "-originalRepositoryRoot");
        if (originalIndex < 0) throw new InvalidOperationException("Original repository path was not supplied.");
        string original = Path.GetFullPath(args[originalIndex + 1]);
        if (Within(project, original)) throw new InvalidOperationException("Portable validation must run outside the source repository.");
        var report = new Report { projectRoot = project, noOriginalRepositoryPaths = true };
        foreach (PackageInfo package in PackageInfo.GetAllRegisteredPackages())
        {
            if (Within(package.resolvedPath, original)) throw new InvalidOperationException("Package resolves inside original repository: " + package.name);
            if (package.name == "com.coplaydev.unity-mcp") throw new InvalidOperationException("Private development package was imported.");
            if (!package.name.StartsWith("com.xavikai.revolt.", StringComparison.Ordinal)) continue;
            string expected = Path.Combine(project, "Packages", package.name);
            if (!Within(package.resolvedPath, expected)) throw new InvalidOperationException("Framework package is not embedded: " + package.name);
            report.packages.Add(new PackageRecord { name = package.name, version = package.version, source = package.source.ToString(), resolvedPath = package.resolvedPath });
        }
        if (!report.packages.Any(package => package.name == "com.xavikai.revolt.common")) throw new InvalidOperationException("Common package was not imported.");
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(assembly => assembly.GetName().Name.StartsWith("EscapeRoomRevolt.", StringComparison.Ordinal)))
        {
            if (string.IsNullOrEmpty(assembly.Location) || !Within(assembly.Location, Path.Combine(project, "Library")))
                throw new InvalidOperationException("Framework assembly was not compiled inside this project: " + assembly.GetName().Name);
            report.assemblyLocations.Add(assembly.Location);
        }
        EscapeRoomRevolt.EditorTools.ProductModuleValidator.ValidateBatch();
        report.result = "Passed";
        File.WriteAllText(".utmp/portable-isolation.json", JsonUtility.ToJson(report, true));
        Debug.Log("[Portable Isolation] PASS: embedded framework packages and assemblies resolve only inside the relocated project.");
    }
    private static bool Within(string candidate, string parent)
    {
        if (string.IsNullOrEmpty(candidate)) return false;
        string normalized = Path.GetFullPath(candidate).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string root = Path.GetFullPath(parent).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return normalized.Equals(root, StringComparison.OrdinalIgnoreCase) || normalized.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }
}
#endif
'@
[IO.File]::WriteAllText((Join-Path $taskEditorFolder 'PortableIsolationAudit.cs'), $taskAuditSource, [Text.UTF8Encoding]::new($false))
function Invoke-PortableUnity([string]$Label, [string[]]$ExtraArguments) {
    $taskLog = Join-Path $taskOutput "$Label.log"
    $taskArguments = @('-batchmode','-nographics','-projectPath', ('"{0}"' -f $taskProject), '-logFile', ('"{0}"' -f $taskLog)) + $ExtraArguments
    $taskRun = Start-Process -FilePath $UnityPath -ArgumentList $taskArguments -WorkingDirectory $taskProject -WindowStyle Hidden -Wait -PassThru
    if ($taskRun.ExitCode -ne 0) { throw "$Product portable $Label failed (exit $($taskRun.ExitCode)); see $taskLog" }
    Write-Output "$Product portable $Label PASS"
}
$taskPending = @{ product = $Product; project = $taskProject; export = $taskExport; cleanLibraryAtStart = $taskFresh; originalExportSha256 = $taskBeforeDigest; result = 'Running' }
$taskRecord = Join-Path $taskRecordFolder "portable-$Product.json"
$taskPending | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $taskRecord -Encoding utf8
try {
Invoke-PortableUnity 'import-isolation' @('-quit','-executeMethod','PortableIsolationAudit.Run','-originalRepositoryRoot',('"{0}"' -f $taskWorkspace))
$taskSuites = @{}
foreach ($taskMode in @('EditMode','PlayMode')) {
    $taskResults = Join-Path $taskOutput "$taskMode.xml"
    Invoke-PortableUnity $taskMode @('-runTests','-testPlatform',$taskMode,'-testResults',('"{0}"' -f $taskResults))
    [xml]$taskXml = Get-Content -Raw -LiteralPath $taskResults
    $taskTestRun = $taskXml.'test-run'
    if ($taskTestRun.result -ne 'Passed' -or [int]$taskTestRun.skipped -ne 0) { throw "$Product portable $taskMode has failures or skipped tests." }
    $taskSuites[$taskMode] = @{ result = $taskTestRun.result; total = [int]$taskTestRun.total; passed = [int]$taskTestRun.passed; failed = [int]$taskTestRun.failed; skipped = [int]$taskTestRun.skipped; startedAt = $taskTestRun.'start-time'; finishedAt = $taskTestRun.'end-time' }
}
if ($BuildWindows) {
    Invoke-PortableUnity 'windows' @('-quit','-executeMethod','EscapeRoomRevolt.EditorTools.ReleaseBuilder.BuildWindows')
    $taskInventory = Get-Content -Raw -LiteralPath (Join-Path $taskProject 'product-inventory.json') | ConvertFrom-Json
    $taskBuildFolder = Join-Path $taskProject 'Builds/Release/v0.1.0-beta.5/Windows'
    $taskBinary = Join-Path $taskBuildFolder ($taskInventory.productId + '.exe')
    $taskBoot = Join-Path $taskBuildFolder ($taskInventory.productId + '_Data/boot.config')
    if (!(Test-Path -LiteralPath $taskBinary) -or !(Test-Path -LiteralPath $taskBoot)) { throw 'Portable Windows artifacts are missing.' }
    if ((Get-Content -Raw -LiteralPath $taskBoot) -match '(?m)^xrsdk-pre-init-library=\S+') { throw 'Portable Windows build requests native XR pre-initialization.' }
    $taskPending.windows = @{
        result = 'Succeeded'
        binary = [IO.Path]::GetRelativePath($taskProject, $taskBinary).Replace('\','/')
        binarySha256 = (Get-FileHash -LiteralPath $taskBinary -Algorithm SHA256).Hash.ToLowerInvariant()
        bootConfigSha256 = (Get-FileHash -LiteralPath $taskBoot -Algorithm SHA256).Hash.ToLowerInvariant()
        nativeXRPreInitialization = $false
    }
    Write-Output "$Product portable Windows boot configuration PASS"
}
$taskAfterDigest = Get-ExportDigest
if ($taskAfterDigest -ne $taskBeforeDigest) { throw 'The original portable export changed during validation.' }
$taskPending.result = 'Passed'
$taskPending.exportUnchanged = $true
$taskPending.suites = $taskSuites
$taskPending.isolation = Get-Content -Raw -LiteralPath (Join-Path $taskOutput 'portable-isolation.json') | ConvertFrom-Json
$taskPending.modules = Get-Content -Raw -LiteralPath (Join-Path $taskOutput 'product-module-validation.json') | ConvertFrom-Json
$taskPending | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $taskRecord -Encoding utf8
Write-Output "$Product portable validation complete: $taskRecord"
} catch {
    $taskPending.result = 'Failed'
    $taskPending.error = $_.Exception.Message
    $taskPending | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $taskRecord -Encoding utf8
    throw
}
