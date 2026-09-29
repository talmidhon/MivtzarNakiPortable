param([string]$Folder = 'artifacts/MivtzarNaki-delivery', [switch]$VisualOnly, [switch]$UpdateShutdown)
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$root = [IO.Path]::GetFullPath((Join-Path $repo $Folder))
$exe = Join-Path $root 'App/MivtzarNaki.exe'
if ($VisualOnly) {
    $visual = Start-Process -FilePath $exe -ArgumentList '--ui-fixture=success','--theme=dark','--small-window' -WorkingDirectory $env:WINDIR -WindowStyle Normal -PassThru
    $visual.WaitForExit()
    return
}
$report = Join-Path $root 'Data/smoke-test.txt'
$results = @()
foreach ($theme in @('dark','light')) {
    foreach ($state in @('loading','missing','success','offline','failure','progress')) {
        $arguments = @('--smoke-test', "--ui-fixture=$state", "--theme=$theme")
        if ($UpdateShutdown) { $arguments += '--update-shutdown-fixture' }
        if ($state -eq 'progress') { $arguments += '--small-window' }
        $started = [DateTime]::UtcNow
        $process = Start-Process -FilePath $exe -ArgumentList $arguments -WorkingDirectory $env:WINDIR -WindowStyle Hidden -PassThru
        if (-not $process.WaitForExit(30000)) { throw "Smoke timed out: PID $($process.Id)" }
        $process.Refresh()
        if ($process.ExitCode -ne 0) { throw "Smoke exit code $($process.ExitCode): $state/$theme" }
        if (-not (Test-Path -LiteralPath $report) -or (Get-Item -LiteralPath $report).LastWriteTimeUtc -lt $started) { throw 'No fresh XAML-load report.' }
        $data = [string](Get-Content -Encoding UTF8 -LiteralPath $report -Raw)
        foreach ($expected in @('UI loaded', 'Elevated=False', 'RTL=RightToLeft', "Fixture=$state", "Theme=$theme", "ViewportTheme=$theme", "Root=$root")) {
            if ($data.IndexOf($expected, [StringComparison]::OrdinalIgnoreCase) -lt 0) { throw "Missing smoke assertion: $expected" }
        }
        $results += [pscustomobject]@{ Fixture=$state; Theme=$theme; ExitCode=$process.ExitCode; Report=$data }
        Write-Output "PASS $state/$theme"
    }
}
$suffix = if ($UpdateShutdown) { '-update-shutdown' } else { '' }
$evidence = Join-Path $repo ('artifacts/smoke-' + (Split-Path $root -Leaf) + $suffix + '.json')
$results | ConvertTo-Json -Depth 4 | Set-Content -Encoding UTF8 -LiteralPath $evidence
Write-Output "Evidence: $evidence"
