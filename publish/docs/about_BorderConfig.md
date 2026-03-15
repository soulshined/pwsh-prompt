# Border Configuration
## about_BorderConfig

# SHORT DESCRIPTION
Configures the border characters and color used for framing content.

# LONG DESCRIPTION
```
Border = @{
    hs    = "═"
    vs    = "║"
    tl    = "╔"
    tr    = "╗"
    bl    = "╚"
    br    = "╝"
    Color = [Colors]::CYAN
}
```

## Keys

All keys are case-insensitive.

```
HorizontalSide   [string]    Default: "─"
hs
    Character used for top and bottom edges.

VerticalSide     [string]    Default: " "
vs
    Character used for left and right edges.

TopLeft          [string]    Default: "┌"
tl
    Top-left corner character.

TopRight         [string]    Default: "┐"
tr
    Top-right corner character.

BottomLeft       [string]    Default: "└"
bl
    Bottom-left corner character.

BottomRight      [string]    Default: "┘"
br
    Bottom-right corner character.

Color            [string, string]
    Border foreground color. See `about_Color`.
```

An unknown key throws a terminating ParameterDefinitionError.

# SEE ALSO
about_BufferConfig
about_Color
