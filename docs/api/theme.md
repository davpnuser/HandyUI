# `Theme`

`HandyUI.Core.Data.Theme`

A plain palette record with **public `SKColor` fields**. Every built-in
control exposes a `Theme` property you can swap to customize its colors.

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
    `Theme` uses public fields rather than auto-properties. You can mutate a
    `Theme` instance in place (`theme.PrimaryColor = SKColors.Red;`) *and*
    use `with` expressions - the record's compiler-generated copy
    constructor copies fields, so both styles work.

!!! tip "`Theme` vs. `ThemeManager`"
    `Theme` is a **static palette** you attach to a control. The live,
    system-aware palette is [`ThemeManager`](theme-manager.md), which generates
    colors dynamically from the OS accent color. `Theme` does **not** react to
    `ThemeManager.ThemeChanged`.

## Static members

### `Default`

```csharp
public static Theme Default => new();
```

Returns a fresh `Theme` with the built-in light palette. Allocates on every
call - cache it if you read it in a hot loop:

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

## Building custom palettes

```csharp
var dark = Theme.Default with
{
    PrimaryColor          = SKColor.Parse("#F97316"),
    BackgroundColor       = SKColor.Parse("#131E37"),
    DarkerBackgroundColor = SKColor.Parse("#090C13"),
    TextColor             = SKColor.Parse("#F8FAFC"),
    BorderColor           = SKColor.Parse("#3F719A")
};

new Frame()
    .WithTheme(dark)
    .WithSize(320, 180)
    .WithAddToRenderer(renderer);
```

!!! warning "`Default` is not a singleton"
    `Theme.Default` allocates a new record on every call, and `Theme.Default
    == Theme.Default` is `false` (reference equality on record instances
    compares fields, so `Equals` works - just don't use `ReferenceEquals`).