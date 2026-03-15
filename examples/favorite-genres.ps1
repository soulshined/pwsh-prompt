Import-Module "$PSScriptRoot/../bin/Debug/pwsh-prompt/pwsh-prompt.psd1" -Force -DisableNameChecking -ErrorAction SilentlyContinue

$Genres = @(
	@{ Value = "Rock";        Description = "Classic & alternative rock";  ForegroundColor = @("Red",        "255;69;0");    Style = "Bold" }
	@{ Value = "Jazz";        Description = "Smooth & contemporary jazz";  ForegroundColor = @("DarkYellow", "218;165;32"); Style = "Italic" }
	@{ Value = "Classical";   Description = "Orchestral & chamber music";  ForegroundColor = @("Cyan",       "175;238;238"); Style = "Italic" }
	@{ Value = "Hip-Hop";     Description = "Rap & beats";                 ForegroundColor = @("Magenta",    "255;0;255");   Style = "Bold" }
	@{ Value = "Latin";       Description = "Salsa, reggaeton & more"; }
	@{ Value = "Country";     Description = "Nashville & folk country";    ForegroundColor = @("DarkYellow", "210;180;140") }
	@{ Value = "Lo-Fi";       Description = "Chill beats to relax to";     ForegroundColor = @("DarkCyan",   "100;149;237"); Style = "Dim" }
	@{ Value = "Electronic";  Description = "EDM, house & techno";         ForegroundColor = @("Green",      "0;255;127");   Style = "Bold" }
	@{ Value = "R&B / Soul";  Description = "Rhythm, blues & neo-soul";   ForegroundColor = @("DarkMagenta","148;103;189"); Style = "Italic" }
	@{ Value = "Metal";       Description = "Heavy, thrash & doom";        ForegroundColor = @("DarkRed",    "139;0;0");     Style = "Bold,Underline" }
	@{ Value = "World";       Description = "Afrobeat, bossa nova & more"; ForegroundColor = @("DarkGreen",  "34;139;34") }
	@{ Value = "Pop";         Description = "Top 40 & chart hits";         ForegroundColor = @("Blue",       "30;144;255");  Style = "Bold" }
	@{ Value = "Reggae";      Description = "Roots, dub & dancehall";      ForegroundColor = @("Green",      "0;200;0");     Style = "Bold"; BackgroundColor = @("DarkYellow", "50;40;0") }
	@{ Value = "Blues";       Description = "Delta, Chicago & electric";   ForegroundColor = @("DarkBlue",   "65;105;225");  Style = "Italic" }
	@{ Value = "Punk";        Description = "Fast, loud & raw";            ForegroundColor = @("DarkGray",   "169;169;169"); Style = "Bold,Strikethrough" }
	@{ Value = "Funk";        Description = "Groovy bass & rhythm";       ForegroundColor = @("Yellow",     "255;165;0");   Style = "Bold" }
	@{ Value = "Disco";       Description = "Dance floor classics";       ForegroundColor = @("Magenta",    "255;105;180"); Style = "Bold" }
	@{ Value = "Gospel";      Description = "Spiritual & choir";          ForegroundColor = @("White",      "255;248;220"); Style = "Italic" }
	@{ Value = "Ska";         Description = "Upbeat & brass-driven";      ForegroundColor = @("DarkYellow", "189;183;107") }
	@{ Value = "Grunge";      Description = "Seattle sound & distortion"; ForegroundColor = @("DarkGray",   "105;105;105"); Style = "Bold" }
	@{ Value = "Ambient";     Description = "Atmospheric & textural";     ForegroundColor = @("DarkCyan",   "72;61;139");   Style = "Dim,Italic" }
	@{ Value = "K-Pop";       Description = "Korean pop & idol groups";   ForegroundColor = @("Magenta",    "255;20;147");  Style = "Bold" }
	@{ Value = "Trap";        Description = "808s & hi-hats";             ForegroundColor = @("Red",        "220;20;60");   Style = "Bold" }
	@{ Value = "Indie";       Description = "Independent & alternative";  ForegroundColor = @("DarkGreen",  "107;142;35");  Style = "Italic" }
	@{ Value = "Synthwave";   Description = "Retro-futuristic & neon";    ForegroundColor = @("Cyan",       "0;255;255");   Style = "Bold"; BackgroundColor = @("DarkMagenta", "40;0;50") }
)

$BufferConfig = Get-Content "$PSScriptRoot/buffers/CompleteExample.json" -Raw | ConvertFrom-Json -AsHashtable

$Result = Prompt-Choice $Genres `
	-Message @{ Text = "What are your favorite genres?"; ForegroundColor = @("White", "255;255;255"); Style = "Bold" } `
	-Title @{ Text = "🎧 Music Preference"; ForegroundColor = @("Cyan", "0;255;255"); Style = "Bold,Underline" } `
	-Multiple `
	-AlternateBuffer $BufferConfig

if ($null -eq $Result) {
	Write-Host "`n  Cancelled!" -ForegroundColor DarkGray
	return
}

Write-Host ""
Write-Host "  🎧 Your picks:" -ForegroundColor Cyan
foreach ($i in $Result) {
	$genre = $Genres[$i].Value
	Write-Host "     $genre" -ForegroundColor Green
}
Write-Host ""
