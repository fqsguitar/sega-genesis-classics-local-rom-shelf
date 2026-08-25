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

if (-not (Test-Path -LiteralPath $backup)) { throw "Backup not found: $backup" }
$backupHash = (Get-FileHash -LiteralPath $backup -Algorithm SHA256).Hash
if ($backupHash -ne $originalHash) { throw "Backup hash is not recognized: $backupHash" }

Copy-Item -LiteralPath $backup -Destination $target -Force
$restoredHash = (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash
if ($restoredHash -ne $originalHash) { throw 'Restoration verification failed.' }

Write-Host 'Original DLL restored and verified.'
