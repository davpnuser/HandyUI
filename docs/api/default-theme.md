# `DefaultTheme`

`HandyUI.Core.Classes.Themes.DefaultTheme`

Supplies the built-in light theme.

```csharp
public static class DefaultTheme
{
    public static ThemeRecord GetTheme();
}
```

!!! warning "A new record every call"
    `GetTheme()` **allocates a new `ThemeRecord`** on each call. Cache it in
    a field if you call it inside a hot loop.

```csharp
var theme = DefaultTheme.GetTheme();
renderer.BackgroundColor = theme.DarkerBackgroundColor;
```

The default palette:

| Field | Hex |
| --- | --- |
| `PrimaryColor` | `#2563EB` |
| `SecondaryColor` | `#3B82F6` |
| `DisabledColor` | `#E2E8F0` |
| `TextColor` | `#0F172A` |
| `LightTextColor` | `#FFFFFF` |
| `DisabledTextColor` | `#94A3B8` |
| `BackgroundColor` | `#F8FAFC` |
| `DarkerBackgroundColor` | `#FFFFFF` |
| `ActiveColor` | `#2563EB` |
| `SemiActiveColor` | `#DBEAFE` |
| `BorderColor` | `#CBD5E1` |