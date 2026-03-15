[CmdletBinding(SupportsShouldProcess, DefaultParameterSetName = 'Bump')]
param(
	[Parameter(Mandatory, ParameterSetName = 'Bump')]
	[ValidateSet('Major', 'Minor', 'Patch')]
	[string] $BumpType,

	[Parameter(Mandatory, ParameterSetName = 'Exact')]
	[version] $Version,

	[switch] $Prerelease
)

$ErrorActionPreference = 'Stop'

$ProjectRoot = Split-Path $PSScriptRoot -Parent
$PSD1Path = Join-Path $ProjectRoot 'pwsh-prompt.psd1'
$CsprojPath = Join-Path $ProjectRoot 'pwsh-prompt.csproj'

$ManifestData = @{
	Author            = 'David Freer'
	CompanyName       = 'Unknown'
	Description       = 'The most Powerful PowerShell Prompt module for interactive, highly customizable prompts'
	Copyright         = '(c) 2026 David Freer. All rights reserved.'
	GUID              = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'
	PowerShellVersion = '7.0'
	HelpInfoURI       = 'https://github.com/soulshined/pwsh-prompt/wiki'

	Tags              = @('Prompt', 'Interactive', 'Input', 'Choice', 'CLI', 'TUI')
	ProjectUri        = 'https://github.com/soulshined/pwsh-prompt'
	LicenseUri        = 'https://github.com/soulshined/pwsh-prompt/blob/main/LICENSE'
	ReleaseNotes      = 'https://github.com/soulshined/pwsh-prompt/CHANGELOG.md'

	CmdletsToExport   = @('Prompt-Choice', 'Prompt-Input')

	PackageLicenseExpression = 'MIT'
}

#region Validate prerequisites

$RequiredModules = @('PSScriptAnalyzer', 'platyPS')
foreach ($Mod in $RequiredModules) {
	if (-not (Get-Module -ListAvailable -Name $Mod)) {
		throw "Required module '$Mod' is not installed. Install it with: Install-Module $Mod -Scope CurrentUser"
	}
}

#endregion

#region Compute version

$CurrentManifest = Import-PowerShellDataFile $PSD1Path
$CurrentVersion = [version] $CurrentManifest.ModuleVersion

if ($PSCmdlet.ParameterSetName -eq 'Exact') {
	$NewVersion = "$($Version.Major).$($Version.Minor).$([Math]::Max($Version.Build, 0))"
}
else {
	$Major = $CurrentVersion.Major
	$Minor = $CurrentVersion.Minor
	$Build = [Math]::Max($CurrentVersion.Build, 0)

	switch ($BumpType) {
		'Major' { $Major++; $Minor = 0; $Build = 0 }
		'Minor' { $Minor++; $Build = 0 }
		'Patch' { $Build++ }
	}

	$NewVersion = "$Major.$Minor.$Build"
}
Write-Host "Version: $($CurrentManifest.ModuleVersion) -> $NewVersion" -ForegroundColor Cyan

#endregion

#region Compute prerelease tag

$PrereleaseTag = $null
if ($Prerelease) {
	$Now = Get-Date
	$PrereleaseTag = 'update' + $Now.ToString('ddMMyyyy')
	Write-Host "Prerelease tag: $PrereleaseTag" -ForegroundColor Cyan
}

#endregion

#region Build Release

if ($PSCmdlet.ShouldProcess('dotnet build -c Release', 'Build')) {
	Write-Host "`nBuilding Release..." -ForegroundColor Yellow
	dotnet build $ProjectRoot -c Release -warnaserror
	if ($LASTEXITCODE -ne 0) { throw "dotnet build failed with exit code $LASTEXITCODE" }
}

#endregion

#region Run tests

if ($PSCmdlet.ShouldProcess('dotnet test', 'Test')) {
	Write-Host "`nRunning tests..." -ForegroundColor Yellow
	$TestProject = Join-Path $ProjectRoot 'PwshPrompt.Tests'
	$NullInput = if ($IsWindows) { 'NUL' } else { '/dev/null' }
	$Proc = Start-Process -FilePath 'dotnet' -ArgumentList @('test', $TestProject) `
		-NoNewWindow -Wait -PassThru -RedirectStandardInput $NullInput
	if ($Proc.ExitCode -ne 0) { throw "dotnet test failed with exit code $($Proc.ExitCode)" }
}

#endregion

#region Generate docs

if ($PSCmdlet.ShouldProcess('platyPS doc generation', 'Generate docs')) {
	& (Join-Path $PSScriptRoot 'generate-docs.ps1') -ProjectRoot $ProjectRoot -DocsRoot $PSScriptRoot
}

#endregion

#region Update psd1

if ($PSCmdlet.ShouldProcess($PSD1Path, 'Generate module manifest')) {
	Write-Host "`nGenerating psd1..." -ForegroundColor Yellow

	$ManifestParams = @{
		Path                   = $PSD1Path
		RootModule             = 'pwsh-prompt.dll'
		ModuleVersion          = $NewVersion
		GUID                   = $ManifestData.GUID
		Author                 = $ManifestData.Author
		CompanyName            = $ManifestData.CompanyName
		Description            = $ManifestData.Description
		Copyright              = $ManifestData.Copyright
		PowerShellVersion      = $ManifestData.PowerShellVersion
		CompatiblePSEditions   = @('Core', 'Desktop')
		HelpInfoUri            = $ManifestData.HelpInfoURI
		CmdletsToExport        = $ManifestData.CmdletsToExport
		FunctionsToExport      = @()
		AliasesToExport        = @()
		VariablesToExport      = @()
		Tags                   = $ManifestData.Tags
		ProjectUri             = $ManifestData.ProjectUri
		LicenseUri             = $ManifestData.LicenseUri
		ReleaseNotes           = $ManifestData.ReleaseNotes
	}

	if ($PrereleaseTag) {
		$ManifestParams.Prerelease = $PrereleaseTag
	}

	New-ModuleManifest @ManifestParams

	$RawPsd1 = Get-Content $PSD1Path -Raw
	$FormattedPsd1 = Invoke-Formatter -ScriptDefinition $RawPsd1
	Set-Content $PSD1Path $FormattedPsd1 -NoNewline
}

#endregion

#region Update csproj

if ($PSCmdlet.ShouldProcess($CsprojPath, 'Update csproj metadata')) {
	Write-Host "`nUpdating csproj..." -ForegroundColor Yellow

	[xml] $Csproj = Get-Content $CsprojPath -Raw
	$Props = $Csproj.SelectSingleNode('//PropertyGroup[Version]')

	$Props.Version          = $NewVersion
	$Props.Authors          = $ManifestData.Author
	$Props.Description      = $ManifestData.Description
	$Props.ProjectUrl       = $ManifestData.ProjectUri
	$Props.RepositoryUrl    = $ManifestData.ProjectUri
	$Props.PackageTags      = $ManifestData.Tags -join ';'
	$Props.PackageLicenseExpression = $ManifestData.PackageLicenseExpression

	$Csproj.Save($CsprojPath)
}

#endregion

Write-Host "`nPublish complete: v$NewVersion" -ForegroundColor Green
