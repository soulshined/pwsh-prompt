# TextStyle Format
## about_TextStyle

# SHORT DESCRIPTION
Text styles are specified as a comma-separated string of TextStyle flags.
Multiple styles combine via bitwise OR.

# LONG DESCRIPTION
## Available flags

```
None                          No styling
Bold              (SGR 1)     Bold / increased intensity
Dim               (SGR 2)     Dim / faint
Italic            (SGR 3)     Italic
Underline         (SGR 4)     Underline
SlowBlink         (SGR 5)     Slow blink
RapidBlink        (SGR 6)     Rapid blink
Reverse           (SGR 7)     Swap foreground and background
Hidden            (SGR 8)     Hidden / conceal
Strikethrough     (SGR 9)     Strikethrough
DoubleUnderline   (SGR 21)    Double underline
Overline          (SGR 53)    Overline
```

## Examples

```
# Single style
Style = "Bold"

# Combined styles
Style = "Bold,Italic,Underline"
```

# SEE ALSO
[about_Label](https://github.com/soulshined/pwsh-prompt/wiki/about_Label)

[about_Color](https://github.com/soulshined/pwsh-prompt/wiki/about_Color)
