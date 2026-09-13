param(
    [switch]$NoArchive
)

$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath($PSScriptRoot)
$artifactsRoot = [IO.Path]::GetFullPath((Join-Path $projectRoot 'artifacts'))
$publishDirectory = [IO.Path]::GetFullPath((Join-Path $artifactsRoot 'ERPContable-Desktop-win-x64'))
$archivePath = [IO.Path]::GetFullPath((Join-Path $artifactsRoot 'ERPContable-Desktop-win-x64.zip'))

if (-not $publishDirectory.StartsWith($artifactsRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'La carpeta de publicación quedó fuera de artifacts.'
}

Push-Location (Join-Path $projectRoot 'ERPContable.Web')
try {
    npm install
    if ($LASTEXITCODE -ne 0) { throw 'npm install terminó con errores.' }
    npm run build
    if ($LASTEXITCODE -ne 0) { throw 'La compilación del frontend terminó con errores.' }
}
finally {
    Pop-Location
}

New-Item -ItemType Directory -Path $artifactsRoot -Force | Out-Null
if (Test-Path -LiteralPath $publishDirectory) {
    Remove-Item -LiteralPath $publishDirectory -Recurse -Force
}

dotnet publish (Join-Path $projectRoot 'ERPContable.Desktop\ERPContable.Desktop.csproj') `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -o $publishDirectory
if ($LASTEXITCODE -ne 0) { throw 'La publicación de escritorio terminó con errores.' }

if (-not $NoArchive) {
    Compress-Archive -Path (Join-Path $publishDirectory '*') -DestinationPath $archivePath -CompressionLevel Optimal -Force
    Write-Host "Paquete creado: $archivePath"
}

Write-Host "Aplicación creada: $(Join-Path $publishDirectory 'ERPContable.exe')"
