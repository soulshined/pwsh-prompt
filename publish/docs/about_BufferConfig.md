# Buffer Configuration
## about_BufferConfig

# SHORT DESCRIPTION
Configures the alternate screen buffer appearance when rendering in
fullscreen mode.

# LONG DESCRIPTION
Pass an empty hashtable for default styling, or provide keys to customize.

```
# Defaults
Prompt-Choice @("A","B") "Pick" -AlternateBuffer @{}

# Custom colors and border
Prompt-Choice @("A","B") "Pick" -AlternateBuffer @{
    fg     = [Colors]::WHITE
    bg     = @("DarkBlue", "0;0;139")
    Border = @{ hs = "═"; vs = "║"; Color = [Colors]::CYAN }
}
```

## Keys

All keys are case-insensitive.

```
ForegroundColor   [string, string]
fg
    Base foreground color for the buffer.
    See `about_Color`.

BackgroundColor   [string, string]
bg
    Base background color for the buffer.
    See `about_Color`.

Border            [hashtable]
    Border characters and color.
    See `about_BorderConfig`.

Item              [hashtable]
    Item appearance and multi-select indicators.
    See `about_ItemConfig`.

Pagination        [hashtable]
    Pagination indicator labels.
    See `about_PaginationConfig`.
```

An unknown key throws a terminating `ParameterDefinitionError`.

# SEE ALSO
[about_BorderConfig](https://github.com/soulshined/pwsh-prompt/wiki/about_BorderConfig)

[about_ItemConfig](https://github.com/soulshined/pwsh-prompt/wiki/about_ItemConfig)

[about_PaginationConfig](https://github.com/soulshined/pwsh-prompt/wiki/about_PaginationConfig)

[about_Label](https://github.com/soulshined/pwsh-prompt/wiki/about_Label)

[about_Color](https://github.com/soulshined/pwsh-prompt/wiki/about_Color)
