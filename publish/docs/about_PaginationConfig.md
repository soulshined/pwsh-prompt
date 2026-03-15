# Pagination Configuration
## about_PaginationConfig

# SHORT DESCRIPTION
Configures the pagination indicator labels shown when items span multiple
pages.

# LONG DESCRIPTION
```
Pagination = @{
    Item         = "○"
    SelectedItem = "●"
    PrevPage     = ""
    NextPage     = ""
    TotalPage    = @{ Text = ""; Style = "Dim" }
}
```

## Keys

All keys are case-insensitive. Each value accepts a label configuration.
See `about_Label`.

```
Item           [label]    Default: "○"
    Indicator for an unselected page.

SelectedItem   [label]    Default: "●"
selected
    Indicator for the current page.

PrevPage       [label]    Default: ""
prev
    Label shown before the page indicators.

NextPage       [label]    Default: ""
next
    Label shown after the page indicators.

TotalPage      [label]    Default: "" (Dim)
total
    Total page count label.
```

An unknown key throws a terminating ParameterDefinitionError.

# SEE ALSO
about_BufferConfig
about_Label
