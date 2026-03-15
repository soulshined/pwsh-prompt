Import-Module "$PSScriptRoot/../bin/Debug/pwsh-prompt/pwsh-prompt.psd1" -Force -DisableNameChecking -ErrorAction SilentlyContinue

$Destinations = @(
	@{ Value = "Tokyo";        ForegroundColor = @("Red",        "255;69;0");    Style = "Bold" }
	@{ Value = "Paris";        ForegroundColor = @("Blue",       "30;144;255");  Style = "Italic" }
	@{ Value = "New York";     ForegroundColor = @("Yellow",     "255;215;0");   Style = "Bold" }
	@{ Value = "Sydney";       ForegroundColor = @("Cyan",       "0;200;200");   Style = "Bold" }
	@{ Value = "Cairo";        ForegroundColor = @("DarkYellow", "210;180;100"); Style = "Bold" }
	@{ Value = "Rio";          ForegroundColor = @("Green",      "0;200;0");     Style = "Bold" }
	@{ Value = "Reykjavik";    ForegroundColor = @("Cyan",       "150;220;255"); Style = "Italic" }
	@{ Value = "Marrakech";    ForegroundColor = @("DarkRed",    "178;34;34");   Style = "Bold" }
	@{ Value = "Bangkok";      ForegroundColor = @("Magenta",    "255;105;180"); Style = "Bold" }
	@{ Value = "Rome";         ForegroundColor = @("DarkYellow", "184;134;11");  Style = "Italic" }
	@{ Value = "Cape Town";    ForegroundColor = @("Green",      "34;139;34");   Style = "Bold" }
	@{ Value = "Seoul";        ForegroundColor = @("Magenta",    "255;20;147");  Style = "Bold" }
	@{ Value = "Dubai";        ForegroundColor = @("Yellow",     "255;223;0");   Style = "Bold" }
	@{ Value = "Barcelona";    ForegroundColor = @("Red",        "220;20;60");   Style = "Bold" }
	@{ Value = "Kyoto";        ForegroundColor = @("DarkMagenta","148;103;189"); Style = "Italic" }
	@{ Value = "Istanbul";     ForegroundColor = @("DarkCyan",   "100;149;237"); Style = "Bold" }
	@{ Value = "Queenstown";   ForegroundColor = @("Green",      "0;128;0");     Style = "Bold" }
	@{ Value = "Havana";       ForegroundColor = @("DarkYellow", "210;105;30");  Style = "Bold" }
	@{ Value = "Amsterdam";    ForegroundColor = @("Blue",       "65;105;225");  Style = "Italic" }
	@{ Value = "Lisbon";         ForegroundColor = @("Yellow",     "255;200;0");   Style = "Bold" }
	@{ Value = "Prague";         ForegroundColor = @("DarkRed",    "139;0;0");     Style = "Bold" }
	@{ Value = "Buenos Aires";   ForegroundColor = @("Cyan",       "0;191;255");   Style = "Bold" }
	@{ Value = "Nairobi";        ForegroundColor = @("Green",      "50;205;50");   Style = "Bold" }
	@{ Value = "Hanoi";          ForegroundColor = @("Red",        "205;92;92");   Style = "Italic" }
	@{ Value = "Athens";         ForegroundColor = @("Blue",       "70;130;180");  Style = "Italic" }
	@{ Value = "Lima";           ForegroundColor = @("DarkYellow", "205;133;63");  Style = "Bold" }
	@{ Value = "Montreal";       ForegroundColor = @("Cyan",       "95;158;160");  Style = "Bold" }
	@{ Value = "Zanzibar";       ForegroundColor = @("Green",      "0;168;107");   Style = "Italic" }
	@{ Value = "Vienna";         ForegroundColor = @("DarkMagenta","128;0;128");   Style = "Italic" }
	@{ Value = "Santorini";      ForegroundColor = @("Blue",       "100;149;237"); Style = "Bold" }
	@{ Value = "Cusco";          ForegroundColor = @("DarkYellow", "160;82;45");   Style = "Bold" }
	@{ Value = "Dubrovnik";      ForegroundColor = @("Red",        "178;34;34");   Style = "Italic" }
	@{ Value = "Petra";          ForegroundColor = @("DarkYellow", "210;140;80");  Style = "Bold" }
	@{ Value = "Bali";           ForegroundColor = @("Green",      "46;139;87");   Style = "Bold" }
	@{ Value = "Bruges";         ForegroundColor = @("DarkYellow", "139;119;101"); Style = "Italic" }
	@{ Value = "Jaipur";         ForegroundColor = @("Magenta",    "219;112;147"); Style = "Bold" }
	@{ Value = "Chiang Mai";     ForegroundColor = @("Green",      "107;142;35");  Style = "Italic" }
	@{ Value = "Cartagena";      ForegroundColor = @("Yellow",     "255;165;0");   Style = "Bold" }
	@{ Value = "Edinburgh";      ForegroundColor = @("DarkCyan",   "47;79;79");    Style = "Bold" }
	@{ Value = "Medellin";       ForegroundColor = @("Green",      "60;179;113");  Style = "Bold" }
	@{ Value = "Tbilisi";        ForegroundColor = @("DarkRed",    "165;42;42");   Style = "Italic" }
	@{ Value = "Salzburg";       ForegroundColor = @("DarkMagenta","153;50;204");  Style = "Italic" }
	@{ Value = "Luang Prabang";  ForegroundColor = @("DarkYellow", "218;165;32");  Style = "Bold" }
	@{ Value = "Fez";            ForegroundColor = @("DarkRed",    "128;0;0");     Style = "Bold" }
	@{ Value = "Valparaiso";     ForegroundColor = @("Cyan",       "0;206;209");   Style = "Bold" }
	@{ Value = "Hoi An";         ForegroundColor = @("Yellow",     "255;255;0");   Style = "Italic" }
	@{ Value = "Tallinn";        ForegroundColor = @("Blue",       "0;0;139");     Style = "Bold" }
	@{ Value = "Oaxaca";         ForegroundColor = @("Red",        "255;99;71");   Style = "Bold" }
	@{ Value = "Split";          ForegroundColor = @("Blue",       "65;105;225");  Style = "Italic" }
	@{ Value = "Siem Reap";      ForegroundColor = @("DarkYellow", "205;155;29");  Style = "Bold" }
	@{ Value = "Kotor";          ForegroundColor = @("DarkCyan",   "0;128;128");   Style = "Italic" }
	@{ Value = "Lhasa";          ForegroundColor = @("Red",        "199;21;133");  Style = "Bold" }
	@{ Value = "Bhutan";         ForegroundColor = @("DarkYellow", "184;134;11");  Style = "Italic" }
	@{ Value = "Patagonia";      ForegroundColor = @("Cyan",       "176;224;230"); Style = "Bold" }
	@{ Value = "Madagascar";     ForegroundColor = @("Green",      "0;100;0");     Style = "Bold" }
	@{ Value = "Fiji";           ForegroundColor = @("Cyan",       "0;255;255");   Style = "Bold" }
	@{ Value = "Maldives";       ForegroundColor = @("Blue",       "0;191;255");   Style = "Bold" }
	@{ Value = "Svalbard";       ForegroundColor = @("White",      "200;200;220"); Style = "Italic" }
	@{ Value = "Antarctica";     ForegroundColor = @("White",      "240;248;255"); Style = "Bold" }
	@{ Value = "Greenland";      ForegroundColor = @("White",      "220;230;240"); Style = "Italic" }
	@{ Value = "Kathmandu";      ForegroundColor = @("DarkYellow", "139;90;43");   Style = "Bold" }
	@{ Value = "Bogota";         ForegroundColor = @("Yellow",     "255;200;50");  Style = "Bold" }
	@{ Value = "Taipei";         ForegroundColor = @("Red",        "230;0;38");    Style = "Bold" }
	@{ Value = "Helsinki";       ForegroundColor = @("Blue",       "0;47;108");    Style = "Italic" }
	@{ Value = "Antigua";        ForegroundColor = @("Cyan",       "0;200;180");   Style = "Bold" }
	@{ Value = "Saigon";         ForegroundColor = @("Red",        "218;37;29");   Style = "Bold" }
	@{ Value = "Riga";           ForegroundColor = @("DarkRed",    "155;30;32");   Style = "Italic" }
	@{ Value = "Seville";        ForegroundColor = @("DarkYellow", "200;140;0");   Style = "Bold" }
	@{ Value = "Osaka";          ForegroundColor = @("Magenta",    "200;80;140");  Style = "Bold" }
	@{ Value = "Florence";       ForegroundColor = @("DarkRed",    "150;50;30");   Style = "Italic" }
	@{ Value = "Marrakesh";      ForegroundColor = @("DarkYellow", "180;100;30");  Style = "Bold" }
	@{ Value = "Bergen";         ForegroundColor = @("Blue",       "50;80;140");   Style = "Italic" }
	@{ Value = "Montevideo";     ForegroundColor = @("Cyan",       "0;150;200");   Style = "Bold" }
	@{ Value = "Beirut";         ForegroundColor = @("Red",        "200;50;50");   Style = "Bold" }
	@{ Value = "Tangier";        ForegroundColor = @("Green",      "0;130;80");    Style = "Italic" }
	@{ Value = "Porto";          ForegroundColor = @("Blue",       "0;70;160");    Style = "Bold" }
	@{ Value = "Colombo";        ForegroundColor = @("DarkYellow", "170;120;0");   Style = "Bold" }
	@{ Value = "Muscat";         ForegroundColor = @("DarkRed",    "130;50;50");   Style = "Italic" }
	@{ Value = "Accra";          ForegroundColor = @("Green",      "0;100;0");     Style = "Bold" }
)

$BufferConfig = Get-Content "$PSScriptRoot/buffers/CompleteExample.json" -Raw | ConvertFrom-Json -AsHashtable
$BufferConfig.Pagination = @{
	SelectedItem = @{ Text = "📍"; ForegroundColor = @("Red", "255;60;60") }
	Item         = @{ Text = "🗺️"; ForegroundColor = @("DarkGray", "80;80;80") }
	PrevPage     = @{ Text = ""; ForegroundColor = @("Red", "Red"); Style = "Bold" }
	NextPage     = @{ Text = ""; ForegroundColor = @("Green", "Green"); Style = "Bold" }
	TotalPage    = @{ Text = ""; ForegroundColor = @("Yellow", "Yellow"); Style = "Bold,Italic" }
}

$Result = Prompt-Choice $Destinations `
	-Message @{ Text = "Where to next?"; ForegroundColor = @("White", "230;230;230") } `
	-Title @{ Text = "✈ Dream Destinations"; ForegroundColor = @("Cyan", "0;255;255"); Style = "Bold,Underline" } `
	-Multiple `
	-AlternateBuffer $BufferConfig

if ($null -eq $Result) {
	Write-Host "`n  Cancelled!" -ForegroundColor DarkGray
	return
}

Write-Host ""
Write-Host "  ✈ Your dream list:" -ForegroundColor Cyan
foreach ($i in $Result) {
	$City = $Destinations[$i].Value
	Write-Host "     $City" -ForegroundColor Green
}
Write-Host ""
