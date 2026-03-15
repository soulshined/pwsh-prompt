# Item Configuration
## about_ItemConfig

# SHORT DESCRIPTION
Configures the appearance of list items and multi-select indicators.

# LONG DESCRIPTION
```
Item = @{
    Item     = @{ Text = ""; fg = [Colors]::WHITE }
    Selected = @{ Text = ""; Style = "Reverse" }
    multipleIndicator = @{
        Enabled  = "◉"
        Disabled = "○"
    }
}
```

## Keys

All keys are case-insensitive.

```
Item                         [label]
    Appearance for unselected items.
    See `about_Label`.

Selected                     [label]
selected
    Appearance for the currently highlighted item.
    Default style: Reverse. See `about_Label`.

multipleIndicator            [hashtable]
    Toggle indicator shown beside each item in
    multi-select mode. See Toggle Items below.

selectedMultipleIndicator    [hashtable]
    Toggle indicator for the currently highlighted
    item in multi-select mode. See Toggle Items below.
```

## Toggle Items

The multipleIndicator and selectedMultipleIndicator keys accept a
hashtable with two label values controlling the checked and unchecked
states.

```
Enabled    [label]    Default: "◉"
on, checked
    Label shown when the item is selected.

Disabled   [label]    Default: "○"
off, unchecked
    Label shown when the item is not selected.
```

Each value accepts a label configuration. See `about_Label`.

An unknown key throws a terminating ParameterDefinitionError.

# SEE ALSO
about_BufferConfig
about_Label
