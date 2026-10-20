$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$manifest = Get-Content -LiteralPath (Join-Path $repo 'Docs/external-assets.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$failures = 0
foreach ($asset in $manifest) {
    $path = Join-Path $repo $asset.path
    if (!(Test-Path -LiteralPath $path -PathType Leaf)) { Write-Output "MISSING: $($asset.path)"; $failures++; continue }
    if ((Get-Item -LiteralPath $path).Length -ne $asset.bytes) { Write-Output "SIZE MISMATCH: $($asset.path)"; $failures++ }
}
Write-Output "Checked $($manifest.Count) external assets; problems: $failures"
if ($failures -gt 0) { exit 1 }
exit 0
