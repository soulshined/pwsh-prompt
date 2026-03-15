# Color Format
## about_Color

# SHORT DESCRIPTION
Colors in pwsh-prompt are specified as a string tuple that targets both
256-color and 24-bit (truecolor) terminals.

# LONG DESCRIPTION
The color format is a 2-element string tuple:

```
@("<256-color index or ConsoleColor name>",
  "<r;g;b or ConsoleColor name>")
```

## Elements

Element [0] is used on 256-color terminals. It accepts a ConsoleColor name
or a 256-color palette index (0-255).

Element [1] is used on 24-bit terminals. It accepts a ConsoleColor name or
an RGB string in "r;g;b" format (each component 0-255).

The runtime selects element [1] when the COLORTERM environment variable is
"truecolor" or "24bit", and element [0] otherwise.

## ConsoleColor names

See https://learn.microsoft.com/en-us/dotnet/api/system.consolecolor

Black, DarkBlue, DarkGreen, DarkCyan, DarkRed, DarkMagenta, DarkYellow,
Gray, DarkGray, Blue, Green, Cyan, Red, Magenta, Yellow, White.

## Predefined Colors

The `[Colors]` class provides predefined tuples for all
[System.ConsoleColor](https://learn.microsoft.com/en-us/dotnet/api/system.consolecolor)
members. Only ConsoleColor values are included; for custom 256-color indices
or arbitrary RGB values, use the manual tuple format described above.

```
[Colors]::BLACK         # @("0",  "0;0;0")
[Colors]::DARKRED       # @("1",  "128;0;0")
[Colors]::DARKGREEN     # @("2",  "0;128;0")
[Colors]::DARKYELLOW    # @("3",  "128;128;0")
[Colors]::DARKBLUE      # @("4",  "0;0;128")
[Colors]::DARKMAGENTA   # @("5",  "128;0;128")
[Colors]::DARKCYAN      # @("6",  "0;128;128")
[Colors]::GRAY          # @("7",  "192;192;192")
[Colors]::DARKGRAY      # @("8",  "128;128;128")
[Colors]::RED           # @("9",  "255;0;0")
[Colors]::GREEN         # @("10", "0;255;0")
[Colors]::YELLOW        # @("11", "255;255;0")
[Colors]::BLUE          # @("12", "0;0;255")
[Colors]::MAGENTA       # @("13", "255;0;255")
[Colors]::CYAN          # @("14", "0;255;255")
[Colors]::WHITE         # @("15", "255;255;255")
```

These can be used anywhere a color tuple is accepted:

```
Prompt-Input @{ Text = "Name"; fg = [Colors]::CYAN }
```

### Naming conflicts

The short name `[Colors]` is registered as a
[type accelerator](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_type_accelerators)
when the module is imported. If another loaded assembly also defines a
`Colors` type and you encounter ambiguity, you may use the fully qualified
name instead:

```
[PwshPrompt.Consts.Colors]::RED
```

## Examples

```
# Predefined color
[Colors]::CYAN

# ConsoleColor name for both capabilities
@("Cyan", "Cyan")

# 256-color index + explicit RGB
@("196", "255;0;0")

# Mix: ConsoleColor name for 256, RGB for 24-bit
@("White", "250;250;250")
```

# SEE ALSO
about_Label
about_TextStyle
