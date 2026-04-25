# check-localization-consistency.ps1 - verifies that DE and EN have identical key lists.
# Usage: pwsh Tools/check-localization-consistency.ps1
# Exit-Code 0 = consistent, 1 = mismatch

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
# Story 1.8 Bug-Fix 2026-04-25: RimWorld 1.6 Vanilla-Konvention erfordert "German (Deutsch)"
# als Folder-Name (mit Klammern). "Deutsch" allein matcht die Vanilla-Sprache nicht und triggert
# den Fallback-zu-English-Bug bei aktivierter Mod.
$deDir = Join-Path $repoRoot 'Languages/German (Deutsch)/Keyed'
$enDir = Join-Path $repoRoot 'Languages/English/Keyed'

if (-not (Test-Path $deDir)) { Write-Error "German (Deutsch)/Keyed dir missing: $deDir"; exit 1 }
if (-not (Test-Path $enDir)) { Write-Error "English/Keyed dir missing: $enDir"; exit 1 }

function Get-KeyValuePairs($dir) {
    $kv = @{}
    foreach ($file in Get-ChildItem -Path $dir -Filter '*.xml' -File) {
        [xml]$xml = Get-Content $file.FullName -Raw -Encoding UTF8
        if ($xml.LanguageData) {
            foreach ($node in $xml.LanguageData.ChildNodes) {
                if ($node.NodeType -eq 'Element') {
                    $kv[$node.LocalName] = $node.InnerText
                }
            }
        }
    }
    return $kv
}

# Counts {0}, {1}, {2}... placeholders in a string.
function Get-PlaceholderSet($text) {
    $matches = [regex]::Matches($text, '\{(\d+)\}')
    $set = @{}
    foreach ($m in $matches) { $set[$m.Groups[1].Value] = $true }
    return ($set.Keys | Sort-Object)
}

$deKv = Get-KeyValuePairs $deDir
$enKv = Get-KeyValuePairs $enDir
$deKeys = $deKv.Keys | Sort-Object
$enKeys = $enKv.Keys | Sort-Object

$onlyDe = $deKeys | Where-Object { -not $enKv.ContainsKey($_) }
$onlyEn = $enKeys | Where-Object { -not $deKv.ContainsKey($_) }

# Placeholder-Parity-Check: same {0}/{1}/... set in DE vs EN per shared key.
$placeholderMismatches = @()
foreach ($key in $deKeys) {
    if ($enKv.ContainsKey($key)) {
        $dePh = (Get-PlaceholderSet $deKv[$key]) -join ','
        $enPh = (Get-PlaceholderSet $enKv[$key]) -join ','
        if ($dePh -ne $enPh) {
            $placeholderMismatches += "$key (DE has '$dePh', EN has '$enPh')"
        }
    }
}

if ($onlyDe.Count -gt 0) {
    Write-Host "Keys only in DE:" -ForegroundColor Red
    $onlyDe | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
}
if ($onlyEn.Count -gt 0) {
    Write-Host "Keys only in EN:" -ForegroundColor Red
    $onlyEn | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
}

if ($placeholderMismatches.Count -gt 0) {
    Write-Host "Placeholder-set mismatches between DE and EN:" -ForegroundColor Red
    $placeholderMismatches | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
}

if ($onlyDe.Count -eq 0 -and $onlyEn.Count -eq 0 -and $placeholderMismatches.Count -eq 0) {
    $count = $deKeys.Count
    Write-Host "Localization consistent: DE and EN have identical key lists ($count keys total) and matching placeholders." -ForegroundColor Green
    exit 0
}
exit 1
