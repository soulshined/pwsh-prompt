# pwsh-prompt
## about_pwsh-prompt

# SHORT DESCRIPTION
Interactive, customizable prompt cmdlets for PowerShell — dependency free.

# LONG DESCRIPTION
pwsh-prompt provides cmdlets for building interactive terminal prompts:

## Prompt-Input

Prompts the user for typed input with type coercion,
retry logic, and optional extended validation. Supports tab-completion for file and directory paths.

```
$name = Prompt-Input "Enter your name"
$age  = Prompt-Input "Enter your age" -ExpectedType int
$port = Prompt-Input "Port number" -ExpectedType int -Validation {
    @($_ -ge 1024 -and $_ -le 65535, "Must be between 1024 and 65535")
}
```

## Prompt-Choice

Prompts the user to select one or more items from a navigable picker.
Supports hotkeys, descriptions, pagination, and alternate screen buffer
rendering.

```
$i = Prompt-Choice @("Red", "Green", "Blue") "Pick a color"
$selected = Prompt-Choice @("dev", "staging", "prod") "Deploy to:" -Multiple
$selected = Prompt-Choice @(
    @{ Value = "dev"; HotKey = "d"; Description = "Development" },
    @{ Value = "staging"; HotKey = "s" },
    @{ Value = "prod"; HotKey = "p"; Description = "Production" }
) "Deploy to:" -Multiple
```

## Customization

Both cmdlets accept label configurations for designated parameters and buffer configurations (they
will be marked as Label configuration compatible)
A label can be a plain string or a hashtable with Text,
ForegroundColor, BackgroundColor, and Style keys. See `about_Label` for
details.

Colors are specified as string tuples targeting both 256-color and
24-bit terminals. See `about_Color`.

Text decoration uses combinable TextStyle flags. See `about_TextStyle`.

# SEE ALSO
[Prompt-Input](https://github.com/soulshined/pwsh-prompt/wiki/Prompt-Input)

[Prompt-Choice](https://github.com/soulshined/pwsh-prompt/wiki/Prompt-Choice)

[about_Label](https://github.com/soulshined/pwsh-prompt/wiki/about_Label)

[about_Color](https://github.com/soulshined/pwsh-prompt/wiki/about_Color)

[about_TextStyle](https://github.com/soulshined/pwsh-prompt/wiki/about_TextStyle)

[about_BufferConfig](https://github.com/soulshined/pwsh-prompt/wiki/about_BufferConfig)

[about_BorderConfig](https://github.com/soulshined/pwsh-prompt/wiki/about_BorderConfig)

[about_ItemConfig](https://github.com/soulshined/pwsh-prompt/wiki/about_ItemConfig)

[about_PaginationConfig](https://github.com/soulshined/pwsh-prompt/wiki/about_PaginationConfig)
