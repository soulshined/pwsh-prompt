Import-Module "$PSScriptRoot/../bin/Debug/pwsh-prompt/pwsh-prompt.psd1" -Force -DisableNameChecking -ErrorAction SilentlyContinue

$Languages = @(
	@{ Value = "C#";         ForegroundColor = @("Magenta",    "138;43;226");  Style = "Bold" }
	@{ Value = "Rust";       ForegroundColor = @("DarkRed",    "183;65;14");   Style = "Bold" }
	@{ Value = "Go";         ForegroundColor = @("Cyan",       "0;173;216");   Style = "Bold" }
	@{ Value = "Python";     ForegroundColor = @("Yellow",     "255;212;59");  Style = "Bold" }
	@{ Value = "TypeScript"; ForegroundColor = @("Blue",       "49;120;198");  Style = "Bold" }
	@{ Value = "Zig";        ForegroundColor = @("DarkYellow", "236;154;41");  Style = "Bold" }
	@{ Value = "Haskell";    ForegroundColor = @("DarkMagenta","94;80;134");   Style = "Italic" }
	@{ Value = "Elixir";     ForegroundColor = @("Magenta",    "110;68;129");  Style = "Bold" }
	@{ Value = "Lua";        ForegroundColor = @("Blue",       "0;0;128");     Style = "Bold" }
	@{ Value = "OCaml";      ForegroundColor = @("DarkYellow", "238;119;0");   Style = "Italic" }
	@{ Value = "Swift";      ForegroundColor = @("Red",        "240;81;56");   Style = "Bold" }
	@{ Value = "Kotlin";     ForegroundColor = @("Magenta",    "169;123;255"); Style = "Bold" }
	@{ Value = "Clojure";    ForegroundColor = @("Green",      "99;171;63");   Style = "Italic" }
	@{ Value = "Scala";      ForegroundColor = @("Red",        "220;40;40");   Style = "Bold" }
	@{ Value = "F#";         ForegroundColor = @("Cyan",       "55;139;186");  Style = "Bold" }
	@{ Value = "Erlang";     ForegroundColor = @("DarkRed",    "163;31;52");   Style = "Bold" }
	@{ Value = "Julia";      ForegroundColor = @("Green",      "64;99;216");   Style = "Bold" }
	@{ Value = "Nim";        ForegroundColor = @("DarkYellow", "255;233;83");  Style = "Bold" }
	@{ Value = "Crystal";    ForegroundColor = @("White",      "200;200;200"); Style = "Italic" }
	@{ Value = "Dart";       ForegroundColor = @("Cyan",       "1;210;255");   Style = "Bold" }
	@{ Value = "Ruby";       ForegroundColor = @("Red",        "204;52;45");   Style = "Bold" }
	@{ Value = "PHP";        ForegroundColor = @("DarkMagenta","119;123;180"); Style = "Bold" }
	@{ Value = "Perl";       ForegroundColor = @("DarkCyan",   "57;106;137");  Style = "Italic" }
	@{ Value = "R";          ForegroundColor = @("Blue",       "39;108;194");  Style = "Bold" }
	@{ Value = "MATLAB";     ForegroundColor = @("DarkYellow", "227;108;9");   Style = "Bold" }
	@{ Value = "Fortran";    ForegroundColor = @("DarkMagenta","115;60;141");  Style = "Italic" }
	@{ Value = "COBOL";      ForegroundColor = @("DarkBlue",   "0;0;102");     Style = "Bold" }
	@{ Value = "Ada";        ForegroundColor = @("Green",      "0;128;64");    Style = "Bold" }
	@{ Value = "Prolog";     ForegroundColor = @("DarkYellow", "200;150;50");  Style = "Italic" }
	@{ Value = "Lisp";       ForegroundColor = @("DarkGreen",  "85;107;47");   Style = "Italic" }
	@{ Value = "Racket";     ForegroundColor = @("Red",        "157;17;28");   Style = "Bold" }
	@{ Value = "D";          ForegroundColor = @("DarkRed",    "176;48;51");   Style = "Bold" }
	@{ Value = "V";          ForegroundColor = @("Blue",       "82;130;196");  Style = "Bold" }
	@{ Value = "Odin";       ForegroundColor = @("Blue",       "56;102;163");  Style = "Bold" }
	@{ Value = "Gleam";      ForegroundColor = @("Magenta",    "255;175;243"); Style = "Bold" }
	@{ Value = "Roc";        ForegroundColor = @("DarkMagenta","120;60;180");  Style = "Bold" }
	@{ Value = "Unison";     ForegroundColor = @("DarkCyan",   "80;160;160");  Style = "Italic" }
	@{ Value = "Idris";      ForegroundColor = @("Red",        "163;32;32");   Style = "Italic" }
	@{ Value = "Lean";       ForegroundColor = @("DarkYellow", "160;120;40");  Style = "Bold" }
	@{ Value = "Agda";       ForegroundColor = @("Blue",       "0;70;140");    Style = "Italic" }
	@{ Value = "Coq";        ForegroundColor = @("DarkYellow", "192;160;32");  Style = "Bold" }
	@{ Value = "Elm";        ForegroundColor = @("Cyan",       "96;181;204");  Style = "Bold" }
	@{ Value = "PureScript"; ForegroundColor = @("DarkGray",   "40;40;40");    Style = "Bold" }
	@{ Value = "ReScript";   ForegroundColor = @("Red",        "230;72;79");   Style = "Bold" }
	@{ Value = "Mojo";       ForegroundColor = @("DarkYellow", "255;128;0");   Style = "Bold" }
	@{ Value = "Carbon";     ForegroundColor = @("DarkGray",   "100;100;100"); Style = "Bold" }
	@{ Value = "Vale";       ForegroundColor = @("Green",      "50;180;100");  Style = "Italic" }
	@{ Value = "Janet";      ForegroundColor = @("Magenta",    "120;80;200");  Style = "Italic" }
	@{ Value = "Fennel";     ForegroundColor = @("Yellow",     "255;230;100"); Style = "Bold" }
	@{ Value = "Wren";       ForegroundColor = @("DarkYellow", "140;110;60");  Style = "Italic" }
	@{ Value = "Tcl";        ForegroundColor = @("DarkCyan",   "0;110;130");   Style = "Bold" }
	@{ Value = "Smalltalk";  ForegroundColor = @("Blue",       "70;130;180");  Style = "Italic" }
	@{ Value = "APL";        ForegroundColor = @("DarkGreen",  "0;100;0");     Style = "Bold" }
	@{ Value = "J";          ForegroundColor = @("Blue",       "30;80;160");   Style = "Bold" }
	@{ Value = "K";          ForegroundColor = @("DarkGray",   "80;80;80");    Style = "Bold" }
	@{ Value = "BQN";        ForegroundColor = @("Magenta",    "160;90;210");  Style = "Bold" }
	@{ Value = "Uiua";       ForegroundColor = @("Yellow",     "255;200;50");  Style = "Bold" }
	@{ Value = "Forth";      ForegroundColor = @("DarkRed",    "120;40;40");   Style = "Italic" }
	@{ Value = "Factor";     ForegroundColor = @("Green",      "88;166;24");   Style = "Bold" }
	@{ Value = "Scheme";     ForegroundColor = @("DarkRed",    "139;69;19");   Style = "Italic" }
	@{ Value = "Groovy";     ForegroundColor = @("Blue",       "66;133;244");  Style = "Bold" }
	@{ Value = "Ceylon";     ForegroundColor = @("DarkYellow", "180;130;40");  Style = "Italic" }
	@{ Value = "Hack";       ForegroundColor = @("DarkGray",   "90;90;90");    Style = "Bold" }
	@{ Value = "Chapel";     ForegroundColor = @("Cyan",       "0;160;176");   Style = "Bold" }
	@{ Value = "Pony";       ForegroundColor = @("DarkMagenta","140;60;100");  Style = "Bold" }
	@{ Value = "Red";        ForegroundColor = @("Red",        "255;0;0");     Style = "Bold" }
	@{ Value = "Io";         ForegroundColor = @("DarkCyan",   "70;130;150");  Style = "Italic" }
	@{ Value = "Ballerina";  ForegroundColor = @("Blue",       "0;100;200");   Style = "Bold" }
	@{ Value = "Awk";        ForegroundColor = @("DarkGreen",  "60;100;40");   Style = "Italic" }
	@{ Value = "Sed";        ForegroundColor = @("DarkGray",   "110;110;110"); Style = "Italic" }
	@{ Value = "Bash";       ForegroundColor = @("Green",      "0;170;0");     Style = "Bold" }
	@{ Value = "Zsh";        ForegroundColor = @("Green",      "80;200;120");  Style = "Bold" }
	@{ Value = "Fish";       ForegroundColor = @("Cyan",       "0;180;180");   Style = "Bold" }
	@{ Value = "Nushell";    ForegroundColor = @("Green",      "70;200;70");   Style = "Bold" }
	@{ Value = "PowerShell"; ForegroundColor = @("Blue",       "0;120;215");   Style = "Bold" }
	@{ Value = "WASM";       ForegroundColor = @("Magenta",    "101;79;240");  Style = "Bold" }
	@{ Value = "SQL";        ForegroundColor = @("DarkYellow", "200;150;0");   Style = "Bold" }
	@{ Value = "GraphQL";    ForegroundColor = @("Magenta",    "229;53;171");  Style = "Bold" }
)

$BufferConfig = Get-Content "$PSScriptRoot/buffers/CompleteExample.json" -Raw | ConvertFrom-Json -AsHashtable
$BufferConfig.Pagination = @{
	SelectedItem = @{ Text = "▰"; ForegroundColor = @("Cyan", "0;255;255") }
	Item         = @{ Text = "▱"; ForegroundColor = @("DarkGray", "60;60;60") }
	PrevPage     = @{ Text = ""; ForegroundColor = @("DarkCyan", "0;139;139"); Style = "Bold" }
	NextPage     = @{ Text = ""; ForegroundColor = @("DarkCyan", "0;139;139"); Style = "Bold" }
	TotalPage    = @{ Text = ""; ForegroundColor = @("DarkGray", "80;80;80"); Style = "Dim" }
}

$Result = Prompt-Choice $Languages `
	-Message @{ Text = "Pick your favorites."; ForegroundColor = @("White", "200;200;200") } `
	-Title @{ Text = "⌨ Languages"; ForegroundColor = @("Cyan", "0;255;255"); Style = "Bold,Underline" } `
	-Multiple `
	-AlternateBuffer $BufferConfig

if ($null -eq $Result) {
	Write-Host "`n  Cancelled!" -ForegroundColor DarkGray
	return
}

Write-Host ""
Write-Host "  ⌨ Your picks:" -ForegroundColor Cyan
foreach ($i in $Result) {
	$Lang = $Languages[$i].Value
	Write-Host "     $Lang" -ForegroundColor Green
}
Write-Host ""
