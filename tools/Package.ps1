param([string]$Output = 'artifacts/MivtzarNaki-delivery')
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$delivery = [IO.Path]::GetFullPath((Join-Path $repo $Output))
$artifactsRoot = [IO.Path]::GetFullPath((Join-Path $repo 'artifacts')) + [IO.Path]::DirectorySeparatorChar
if (-not $delivery.StartsWith($artifactsRoot, [StringComparison]::OrdinalIgnoreCase)) { throw 'Output must be inside repository artifacts.' }
if (Test-Path -LiteralPath $delivery) { throw "Output already exists; choose a fresh Output: $delivery" }
$app = Join-Path $delivery 'App'
& dotnet publish (Join-Path $repo 'src/MivtzarNaki.App/MivtzarNaki.App.csproj') -c Release -o $app --nologo
if ($LASTEXITCODE -ne 0) { throw 'Publish failed.' }
# Keep SDK-generated metadata and all native/runtime dependencies; remove development symbols only.
Get-ChildItem -LiteralPath $app -Recurse -File -Filter '*.pdb' | ForEach-Object { Remove-Item -LiteralPath $_.FullName }
$files = [ordered]@{}
Get-ChildItem -LiteralPath $app -Recurse -File | Sort-Object FullName | ForEach-Object {
    $name = $_.FullName.Substring($app.Length + 1).Replace('\','/')
    $files[$name] = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
}
$version = (Get-Item -LiteralPath (Join-Path $app 'MivtzarNaki.exe')).VersionInfo.ProductVersion.Split('+')[0]
if ($version -notmatch '^\d+\.\d+\.\d+$') { throw 'Invalid application version.' }
$manifest = [ordered]@{ Version = $version; Files = $files }
$manifest | ConvertTo-Json -Depth 4 | Set-Content -Encoding utf8 -LiteralPath (Join-Path $app 'portable.manifest.json')
Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.IO.Compression
$zip = Join-Path (Split-Path $delivery -Parent) ((Split-Path $delivery -Leaf) + '-win-x64.zip')
if (Test-Path -LiteralPath $zip) { throw 'ZIP already exists.' }
$archive = [IO.Compression.ZipFile]::Open($zip, [IO.Compression.ZipArchiveMode]::Create)
try {
    Get-ChildItem -LiteralPath $app -Recurse -File | ForEach-Object {
        $entry = $_.FullName.Substring($delivery.Length + 1).Replace('\','/')
        [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $_.FullName, $entry, [IO.Compression.CompressionLevel]::Optimal) | Out-Null
    }
} finally { $archive.Dispose() }
[pscustomobject]@{ Folder=$delivery; Zip=$zip; FolderBytes=(Get-ChildItem -LiteralPath $app -Recurse -File | Measure-Object Length -Sum).Sum; ZipBytes=(Get-Item -LiteralPath $zip).Length; ZipSha256=(Get-FileHash -LiteralPath $zip).Hash; Files=$files.Count+1 } | ConvertTo-Json | Tee-Object -FilePath (Join-Path (Split-Path $delivery -Parent) ((Split-Path $delivery -Leaf) + '-measurements.json'))
