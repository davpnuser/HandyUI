# `ThemeRecord`

`HandyUI.Core.Classes.Records.ThemeRecord`

An immutable palette. Controls are expected to read from a `ThemeRecord`
rather than hard-coding colors.

```csharp
public record ThemeRecord(
    SKColor PrimaryColor,
    SKColor SecondaryColor,
    SKColor DisabledColor,
    SKColor TextColor,
    SKColor LightTextColor,
    SKColor DisabledTextColor,
    SKColor BackgroundColor,
    SKColor DarkerBackgroundColor,
    SKColor ActiveColor,
    SKColor SemiActiveColor,
    SKColor BorderColor);
```

| Field | Typical use |
| --- | --- |
| `PrimaryColor` | Accent / brand. |
| `SecondaryColor` | Less-emphasized accent. |
| `DisabledColor` | Fill for disabled controls. |
| `TextColor` | Primary text. |
| `LightTextColor` | Text on dark / colored backgrounds. |
| `DisabledTextColor` | Text on disabled controls. |
| `BackgroundColor` | Panel / control background. |
| `DarkerBackgroundColor` | Window background (default canvas clear). |
| `ActiveColor` | Highlight for the active/selected item. |
| `SemiActiveColor` | Hover / soft highlight. |
| `BorderColor` | Outline strokes. |

!!! tip "Swapping themes at runtime"
    Because it's a record, tweaking is a one-liner with `with`:

    ```csharp
    var dark = DefaultTheme.GetTheme() with
    {
        BackgroundColor = SKColor.Parse("#131E37"),
        DarkerBackgroundColor = SKColor.Parse("#090C13"),
        TextColor = SKColor.Parse("#F8FAFC")
    };
    ```