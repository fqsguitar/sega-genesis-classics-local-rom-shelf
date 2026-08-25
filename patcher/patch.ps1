[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$GameDirectory
)

$ErrorActionPreference = 'Stop'
$originalHash = '2EBAEF80F9FCF1C0565B6E6120D6478804D72574A15E82931C5AE07D599D9A2B'
$managedDirectory = Join-Path $GameDirectory 'SEGAGameRoom_Data\Managed'
$target = Join-Path $managedDirectory 'Assembly-CSharp.dll'
$backup = Join-Path $managedDirectory 'Assembly-CSharp.local-rom-shelf.backup.dll'
$tools = Join-Path $PSScriptRoot '.tools'
$package = Join-Path $tools 'mono.cecil.0.11.6.nupkg'
$packageZip = Join-Path $tools 'mono.cecil.0.11.6.zip'
$packageDirectory = Join-Path $tools 'mono.cecil.0.11.6'
$cecil = Join-Path $packageDirectory 'lib\net40\Mono.Cecil.dll'
$patcher = Join-Path $tools 'ShelfPatcher.exe'
$source = Join-Path $PSScriptRoot 'src\ShelfPatcher.cs'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'

if (-not (Test-Path -LiteralPath $target)) { throw "Target DLL not found: $target" }
if (-not (Test-Path -LiteralPath $compiler)) { throw 'The Windows .NET Framework C# compiler was not found.' }

$currentHash = (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash
if ($currentHash -ne $originalHash) {
    throw "Unsupported or already modified DLL. Expected $originalHash but found $currentHash."
}

New-Item -ItemType Directory -Path $tools -Force | Out-Null
if (-not (Test-Path -LiteralPath $cecil)) {
    Invoke-WebRequest -Uri 'https://api.nuget.org/v3-flatcontainer/mono.cecil/0.11.6/mono.cecil.0.11.6.nupkg' -OutFile $package
    Copy-Item -LiteralPath $package -Destination $packageZip -Force
    Expand-Archive -LiteralPath $packageZip -DestinationPath $packageDirectory -Force
}

& $compiler /nologo /target:exe /out:$patcher /reference:$cecil $source
if ($LASTEXITCODE -ne 0) { throw 'Patcher compilation failed.' }
Copy-Item -LiteralPath $cecil -Destination (Join-Path $tools 'Mono.Cecil.dll') -Force

if (Test-Path -LiteralPath $backup) { throw "Backup already exists: $backup" }
Copy-Item -LiteralPath $target -Destination $backup
try {
    & $patcher patch $target
    if ($LASTEXITCODE -ne 0) { throw "Patcher exited with code $LASTEXITCODE." }
}
catch {
    Copy-Item -LiteralPath $backup -Destination $target -Force
    throw
}

Write-Host "Backup: $backup"
Write-Host 'Patch complete. Keep the backup until restoration is verified.'
