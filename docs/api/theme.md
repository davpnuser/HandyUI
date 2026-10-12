# `Theme`

`HandyUI.Core.Classes.Records.Theme`

An immutable-looking palette: a plain C# `record` with public color **fields**
and a static `Default` factory. Controls read their initial colors from a
`Theme` and expose per-channel overrides so you can tweak a single color
without cloning the whole palette.

```csharp
public record Theme
{
    public SKColor PrimaryColor          = SKColor.Parse("#2563EB");
    public SKColor SecondaryColor        = SKColor.Parse("#3B82F6");
    public SKColor DisabledColor         = SKColor.Parse("#E2E8F0");
    public SKColor TextColor             = SKColor.Parse("#0F172A");
    public SKColor LightTextColor        = SKColor.Parse("#FFFFFF");
    public SKColor DisabledTextColor     = SKColor.Parse("#94A3B8");
    public SKColor BackgroundColor       = SKColor.Parse("#F8FAFC");
    public SKColor DarkerBackgroundColor = SKColor.Parse("#FFFFFF");
    public SKColor ActiveColor           = SKColor.Parse("#2563EB");
    public SKColor SemiActiveColor       = SKColor.Parse("#DBEAFE");
    public SKColor BorderColor           = SKColor.Parse("#CBD5E1");

    public static Theme Default => new();
}
```

!!! info "Fields, not properties"
    `Theme` uses **public fields** rather than auto-properties. That means
    you can mutate an instance in place (`theme.PrimaryColor = SKColors.Red;`)
    - and `with` expressions still work because the compiler's synthesized
    copy constructor copies fields as well.

## Static members

### `Default`

```csharp
public static Theme Default => new();
```

Returns a fresh `Theme` instance with the built-in light palette. Because it
allocates on every access, cache it in a field if you call it in a hot loop:

```csharp
private static readonly Theme s_theme = Theme.Default;
```

## Fields

| Field | Default | Typical use |
| --- | --- | --- |
| `PrimaryColor` | `#2563EB` | Accent / brand. |
| `SecondaryColor` | `#3B82F6` | Less-emphasized accent. |
| `DisabledColor` | `#E2E8F0` | Fill for disabled controls. |
| `TextColor` | `#0F172A` | Primary text. |
| `LightTextColor` | `#FFFFFF` | Text on dark / colored backgrounds. |
| `DisabledTextColor` | `#94A3B8` | Text on disabled controls. |
| `BackgroundColor` | `#F8FAFC` | Panel / control background. |
| `DarkerBackgroundColor` | `#FFFFFF` | Window background (default canvas clear). |
| `ActiveColor` | `#2563EB` | Highlight for the active / selected item. |
| `SemiActiveColor` | `#DBEAFE` | Hover / soft highlight. |
| `BorderColor` | `#CBD5E1` | Outline strokes. |

## Custom palettes

Because it's a record, `with` gives you a new instance without mutating the
original:

```csharp
var dark = Theme.Default with
{
    BackgroundColor       = SKColor.Parse("#131E37"),
    DarkerBackgroundColor = SKColor.Parse("#090C13"),
    TextColor             = SKColor.Parse("#F8FAFC"),
    BorderColor           = SKColor.Parse("#3F719A")
};

var accent = Theme.Default with { PrimaryColor = SKColors.Orange };
```

Every built-in control exposes `Theme` as a `Theme` property and a
`WithTheme(Theme)` fluent method, so a palette swap is a one-liner:

```csharp
new TextButton()
    .WithText("Save")
    .WithTheme(dark)
    .WithAddToRenderer(renderer);
```

!!! tip "Custom colors survive a theme swap"
    When you assign an individual color on a control (e.g.
    `button.NormalBackgroundColor = SKColors.Black;`), the control marks
    that channel as customized. A later `WithTheme(...)` will only overwrite
    the channels you haven't touched. This is implemented with private
    `_customXxxSet` flags inside each control.

!!! warning "Default is a factory, not a singleton"
    `Theme.Default` allocates a new `Theme` each call. Do **not** compare
    references (`Theme.Default == Theme.Default` is `false`). The record's
    value-based `Equals` compares fields, so `Theme.Default.Equals(other)`
    behaves as you'd expect.