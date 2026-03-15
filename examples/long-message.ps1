Import-Module "$PSScriptRoot/../bin/Debug/pwsh-prompt/pwsh-prompt.psd1" -Force -DisableNameChecking -ErrorAction SilentlyContinue

$Items = @(
	@{ Value = "☑️ Accept"; ForegroundColor = [Colors]::GREEN; Style = "Bold" }
	@{ Value = "❌ Decline"; ForegroundColor = [Colors]::RED; Style = "Bold" }
	@{ Value = "⌛ Review Later"; ForegroundColor = [Colors]::DARKYELLOW; Style = "Italic" }
)

$BufferConfig = Get-Content "$PSScriptRoot/buffers/CompleteExample.json" -Raw | ConvertFrom-Json -AsHashtable

$Result = Prompt-Choice $Items `
	-Message @{
		Text = "By proceeding you acknowledge that all data will be processed per our privacy policy including collection, storage, & analysis of personal information.`n`nReview all terms carefully before selecting.`n`nThis action cannot be undone."
		ForegroundColor = [Colors]::WHITE
	} `
	-Title @{ Text = "📋 Terms & Conditions"; ForegroundColor = [Colors]::CYAN; Style = "Bold,Underline" } `
	-AlternateBuffer $BufferConfig

if ($null -eq $Result) {
	Write-Host "`n  Cancelled!" -ForegroundColor DarkGray
	return
}

$Selected = $Items[$Result].Value
Write-Host ""
Write-Host "  📋 You selected: $Selected" -ForegroundColor Cyan
Write-Host ""
