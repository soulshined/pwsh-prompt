# pwsh-prompt
## about_pwsh-prompt

# SHORT DESCRIPTION
Interactive, customizable prompt cmdlets for PowerShell — dependency free.

# LONG DESCRIPTION
pwsh-prompt provides two cmdlets for building interactive terminal prompts:

## Prompt-Input

Prompts the user for typed input with optional validation, type coercion,
and retry logic. Supports tab-completion for file and directory paths.

```
$name = Prompt-Input "Enter your name"
$age  = Prompt-Input "Enter your age" -ExpectedType int
```

## Prompt-Choice

Prompts the user to select one or more items from a navigable picker.
Supports hotkeys, descriptions, pagination, and alternate screen buffer
rendering.

```
$i = Prompt-Choice @("Red", "Green", "Blue") "Pick a color"
```

## Customization

Both cmdlets accept label configurations for the -Message and -Title
parameters. A label can be a plain string or a hashtable with Text,
ForegroundColor, BackgroundColor, and Style keys. See `about_Label` for
details.

Colors are specified as string tuples targeting both 256-color and
24-bit terminals. See `about_Color`.

Text decoration uses combinable TextStyle flags. See `about_TextStyle`.

# SEE ALSO
Prompt-Input
Prompt-Choice
about_Label
about_Color
about_TextStyle
about_BufferConfig
about_BorderConfig
about_ItemConfig
about_PaginationConfig
