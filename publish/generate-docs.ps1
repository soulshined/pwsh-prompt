param(
	[Parameter(Mandatory)]
	[string] $ProjectRoot,

	[Parameter(Mandatory)]
	[string] $DocsRoot
)

$ErrorActionPreference = 'Stop'

function ConvertFrom-XmlDoc([System.Xml.XmlNode]$Node) {
	$Text = ''
	foreach ($Child in $Node.ChildNodes) {
		switch ($Child.Name) {
			'para' { $Text += (ConvertFrom-XmlDoc $Child).Trim() + "`n`n" }
			'c' { $Text += '`' + $Child.InnerText + '`' }
			'b' { $Text += '**' + $Child.InnerText + '**' }
			'see' {
				$Cref = $Child.GetAttribute('cref')
				$Short = ($Cref -replace '^[A-Z]:', '' -replace '^.*\.', '')
				$Text += '`' + $Short + '`'
			}
			'list' {
				foreach ($Item in $Child.SelectNodes('item/description')) {
					$Text += "- " + (ConvertFrom-XmlDoc $Item).Trim() + "`n"
				}
				$Text += "`n"
			}
			'#text' {
				$Normalized = ($Child.Value -replace '\s+', ' ')
				$Text += $Normalized
			}
			default { $Text += $Child.InnerText }
		}
	}
	return $Text
}

function Get-XmlDocMember([xml]$XmlDoc, [string]$FullTypeName) {
	$MemberName = "T:$FullTypeName"
	return $XmlDoc.SelectSingleNode("//member[@name='$MemberName']")
}

function Get-XmlDocSynopsis([System.Xml.XmlNode]$Member) {
	$Summary = $Member.SelectSingleNode('summary')
	if (-not $Summary) { return '' }
	$FirstPara = $Summary.SelectSingleNode('para[1]')
	if ($FirstPara) { return (ConvertFrom-XmlDoc $FirstPara).Trim() }
	return (ConvertFrom-XmlDoc $Summary).Trim()
}

function Get-XmlDocDescription([System.Xml.XmlNode]$Member) {
	$Summary = $Member.SelectSingleNode('summary')
	if (-not $Summary) { return '' }
	$Children = $Summary.ChildNodes
	$SkippedFirst = $false
	$Text = ''
	foreach ($Child in $Children) {
		if ($Child.Name -eq 'para' -and -not $SkippedFirst) {
			$SkippedFirst = $true
			continue
		}
		if (-not $SkippedFirst) { continue }
		switch ($Child.Name) {
			'para' { $Text += (ConvertFrom-XmlDoc $Child).Trim() + "`n`n" }
			'list' {
				foreach ($Item in $Child.SelectNodes('item/description')) {
					$Text += "- " + (ConvertFrom-XmlDoc $Item).Trim() + "`n"
				}
				$Text += "`n"
			}
			'#text' {
				$Trimmed = $Child.Value.Trim()
				if ($Trimmed) { $Text += $Trimmed + "`n`n" }
			}
			default { $Text += (ConvertFrom-XmlDoc $Child).Trim() + "`n`n" }
		}
	}
	return $Text.Trim()
}

function Get-XmlDocExamples([System.Xml.XmlNode]$Member) {
	$Examples = $Member.SelectNodes('example')
	if (-not $Examples -or $Examples.Count -eq 0) { return @() }
	$Result = @()
	foreach ($Ex in $Examples) {
		$RawCode = ($Ex.SelectSingleNode('code')?.InnerText ?? '').Trim()
		$CodeLines = $RawCode -split "`n"
		$Indented = $CodeLines | Select-Object -Skip 1 | Where-Object { $_.Trim() }
		$MinIndent = if ($Indented) { ($Indented | ForEach-Object { ($_ -replace '^(\s*).*', '$1').Length } | Measure-Object -Minimum).Minimum } else { 0 }
		$Code = $CodeLines[0]
		if ($CodeLines.Count -gt 1) {
			$Code += "`n" + (($CodeLines | Select-Object -Skip 1 | ForEach-Object { if ($_.Length -ge $MinIndent) { $_.Substring($MinIndent) } else { $_ } }) -join "`n")
		}
		$Desc = ''
		$Para = $Ex.SelectSingleNode('para')
		if ($Para) { $Desc = (ConvertFrom-XmlDoc $Para).Trim() }
		$Result += @{ Code = $Code; Description = $Desc }
	}
	return $Result
}

function Remove-ContentSection([string]$Content, [string]$Header) {
	$Splits = $Content -split $Header

	$Level = $Header.Trim().Split(" ", 2)[0]

	$AfterLines = $Splits[1] -split "`n"
	$Length = 0

	$LevelLen = $Level.Length
	for ($i = 0; $i -lt $AfterLines.Count; $i++) {
		$Line = $AfterLines[$i]
		if ($Line.Trim() -match '^(#{1,})\s' -and $Matches[1].Length -le $LevelLen) {
			$Length = ($AfterLines[0..($i - 1)] -join "`n").ToString().Length
			break
		}
	}

	@{
		Before  = $Splits[0].Trim()
		Content = $Splits[1].Substring(0, $Length)
		After   = $Splits[1].Substring($Length).Trim()
	}
}

Write-Host "`nGenerating docs..." -ForegroundColor Yellow

$ReleaseDll = Join-Path $ProjectRoot 'bin/Release/pwsh-prompt/pwsh-prompt.dll'
$WikiDir = Join-Path $ProjectRoot 'wiki'
$HelpDir = Join-Path $ProjectRoot 'bin/Release/pwsh-prompt/en-US'

Remove-Item (Join-Path $HelpDir 'pwsh-prompt.dll-Help.xml') -Force -ErrorAction Ignore

Import-Module $ReleaseDll -Force
$Module = Get-Module 'pwsh-prompt'

$XmlDocPath = Join-Path $ProjectRoot 'bin/Release/pwsh-prompt/pwsh-prompt.xml'
if (-not (Test-Path $XmlDocPath)) {
	throw "XML documentation file not found at $XmlDocPath. Ensure <GenerateDocumentationFile>true</GenerateDocumentationFile> is in the csproj."
}
[xml] $XmlDoc = Get-Content $XmlDocPath -Raw

New-Item -Path (Join-Path $WikiDir 'commands') -ItemType Directory -Force | Out-Null

if (-not (Test-Path $HelpDir)) {
	New-Item -Path $HelpDir -ItemType Directory -Force | Out-Null
}

Copy-Item (Join-Path $DocsRoot 'docs/about_*.md') -Destination $WikiDir -Force
Copy-Item (Join-Path $ProjectRoot 'README.md') -Destination (Join-Path $WikiDir 'Home.md') -Force
Remove-Item (Join-Path $WikiDir 'commands/*.md') -Force -ErrorAction Ignore

$Module.ExportedCmdlets.Values | ForEach-Object {
	$CommandName = $_.Name
	$ClassName = $_.ImplementingType.FullName
	$XmlMember = Get-XmlDocMember $XmlDoc $ClassName

	$Config = @{
		Command               = $CommandName
		AlphabeticParamsOrder = $false
		ExcludeDontShow       = $true
		Encoding              = [System.Text.Encoding]::UTF8
		UseFullTypeName       = $true
		OutputFolder          = Join-Path $WikiDir 'commands'
		Force                 = $true
		NoMetadata            = $false
	}

	$File = New-MarkdownHelp @Config
	$Content = ($File | Get-Content -Raw).ReplaceLineEndings("`n")

	$Removed = Remove-ContentSection $Content "### -ProgressAction"
	$Content = $Removed.Before + "`n" + $Removed.After

	$Removed = Remove-ContentSection $Content "## EXAMPLES"
	$Content = $Removed.Before + "`n## EXAMPLES`n`n{{ Fill in the Examples }}`n`n" + $Removed.After

	$Removed = Remove-ContentSection $Content "## SYNTAX"
	$SyntaxParts = $Removed.Content -split "``````"
	$Syntax = ($SyntaxParts[1] -replace "`n", " ").Trim().Replace('[-ProgressAction <ActionPreference>] ', '')
	$Content = $Removed.Before + "`n## Syntax`n``````powershell`n$Syntax`n```````n" + $Removed.After

	$Removed = Remove-ContentSection $Content "## RELATED LINKS"
	$Content = $Removed.Before + "`n`n## RELATED LINKS`n`n"

	$local:Synopsis = if ($XmlMember) { Get-XmlDocSynopsis $XmlMember } else { '' }
	$local:Description = if ($XmlMember) { Get-XmlDocDescription $XmlMember } else { '' }
	$local:Examples = if ($XmlMember) { Get-XmlDocExamples $XmlMember } else { @() }
	$local:RelatedLinks = @()

	$AboutFile = Join-Path $DocsRoot "docs/cmdlets/about_${CommandName}.ps1"
	if (Test-Path $AboutFile) {
		. $AboutFile
	}

	$Removed = Remove-ContentSection $Content "## SYNOPSIS"
	$Content = $Removed.Before + "`n## SYNOPSIS`n" + ($local:Synopsis ?? '') + "`n"  + $Removed.After

	$Removed = Remove-ContentSection $Content "## DESCRIPTION"
	$Content = $Removed.Before + "`n## DESCRIPTION`n" + ($local:Description ?? '') + "`n`n" + $Removed.After

	$Content += "`n" + (($local:RelatedLinks | ForEach-Object {
			if ($_ -is [string]) {
				return "[{0}]({1})" -f $_, $_
			}
			else {
				return "[{0}]({1})" -f $_.Title, $_.Url
			}
		}) -join "`n`n")

	$Content = $Content.Replace("{{ Fill in the Examples }}", ($local:Examples | ForEach-Object -Begin { $i = 0 } -Process {
			@"
### Example {0}

``````powershell
{1}
``````

{2}
"@ -f (++$i), $_.Code, ($_.Description ?? '') }) -join "`n")

	$Content = $Content -replace '\\`(about_\w+)\\`', '`$1`'

	$Content | Out-File $File -Force
}

$null = New-ExternalHelp -Path $WikiDir, (Join-Path $WikiDir 'commands') -OutputPath $HelpDir -Force -ErrorAction Stop

Remove-Module 'pwsh-prompt' -Force
