[CmdletBinding()]
param(
    [ValidateSet('Debug_211', 'Debug_220', 'Debug_230', 'Debug_240', 'Release_211', 'Release_220', 'Release_230', 'Release_240')]
    [string]$Configuration = 'Release_240',

    [ValidateSet('Win32', 'x64')]
    [string]$Platform = 'x64'
)

$ErrorActionPreference = 'Stop'

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path -LiteralPath $vswhere)) {
    throw 'Visual Studio Installer was not found.'
}

$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (-not $msbuild) {
    throw 'MSBuild was not found.'
}

$solution = Join-Path $PSScriptRoot 'src\LusasContourColors\LusasContourColors.sln'
$solutionPlatform = if ($Platform -eq 'Win32') { 'x86' } else { $Platform }

& $msbuild $solution /t:Rebuild /p:Configuration=$Configuration /p:Platform=$solutionPlatform /m /v:minimal
if ($LASTEXITCODE -ne 0) {
    throw "Build failed with exit code $LASTEXITCODE."
}
