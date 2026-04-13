# Changelog

## v0.0.4

- Prompt-Input `-Validation`
    - Fixed documentation examples using incorrect PowerShell syntax (comma operator precedence caused the message string to be consumed by the comparison instead of being a separate tuple element)
    - Updated fallback error message when validation fails without a custom message - previously said "cannot be converted to {type}" (identical to type conversion errors), now distinguishes validation failures
    - Added integration tests covering `-Validation` with `-ExpectedType`, custom messages, and the `$_` pipeline variable receiving correctly typed values

## v0.0.3

- Prompt-Input
    - New -ExpectedType variants
        - `sbyte`
        - `hex` | input hex value returns a `string` without the `0x` prefix
        - `timezone` | input timezone id returns a `TimeZoneInfo`

Minor updates to documentation verbiage and formatting and added more examples to cmdlets

## v0.0.1

Hello World
