# Label Configuration
## about_Label

# SHORT DESCRIPTION
A label is the fundamental styled-text unit in pwsh-prompt. Any parameter that
accepts a label can receive either a plain [string] or a [hashtable].

# LONG DESCRIPTION
## String form

When a plain string is passed, the text inherits foreground/background colors
from the terminal (or from the alternate-buffer config when applicable) and
uses the parameter's default style.

```
Prompt-Input "Enter your name"
```

## Hashtable form

A hashtable gives explicit control over text, colors, and style.

```
Prompt-Input @{
    Text            = "Enter your name"
    ForegroundColor = [Colors]::WHITE
    BackgroundColor = @("DarkBlue", "0;0;139")
    Style           = "Bold,Italic"
}
```

## Keys

All keys are case-insensitive.

```
Text              [string]           (required)
    The display text.

ForegroundColor   [string, string]
fg
    Foreground color. See `about_Color`.

BackgroundColor   [string, string]
bg
    Background color. See `about_Color`.

Style             [string]
    Text decoration. See `about_TextStyle`.
```

A hashtable missing the Text key throws a terminating
ParameterDefinitionError.

# SEE ALSO
about_Color
about_TextStyle
