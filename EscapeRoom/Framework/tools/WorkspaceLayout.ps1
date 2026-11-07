$taskRepo = Split-Path -Parent $PSScriptRoot
$taskWorkspace = [IO.Path]::GetFullPath((Join-Path $taskRepo '../..'))
function Get-ProductRoot([string]$Name) { Join-Path $taskWorkspace "$Name/Unity" }
function Get-PortableRoot([string]$Name) { Join-Path $taskWorkspace "$Name/Exportacions/$Name-0.2.0-pcfix-2026-10-26" }
function Get-DeliveryRoot([string]$Name,[string]$Kind,[string]$Override) {
    if ($Override) { return [IO.Path]::GetFullPath($Override, $taskWorkspace) }
    Join-Path $taskWorkspace "$Name/$Kind"
}
function Resolve-HistoricalPath([string]$Value) {
    $taskNormalized = $Value.Replace('\','/')
    if ($taskNormalized -match '^Projects/(EscapeRoom|Survival)(?:/(.*))?$') { return Join-Path (Get-ProductRoot $Matches[1]) $Matches[2] }
    if ($taskNormalized -match '^Distributions/(?:(Deliveries|WindowsDemos)/)?((EscapeRoom|Survival)-.*)$') {
        $taskKind = switch ($Matches[1]) { 'Deliveries' { 'PaquetsFonts' }; 'WindowsDemos' { 'Windows' }; default { 'Exportacions' } }
        return Join-Path $taskWorkspace "$($Matches[3])/$taskKind/$($Matches[2])"
    }
    Join-Path $taskRepo $taskNormalized
}
