# Controls

Built-in controls shipped in the `HandyUI.Controls` package, under the
`HandyUI.Controls` namespace. All of them derive from
[`BaseControl<TSelf>`](../ui-control-base.md), so they inherit the fluent
`With…` pattern, invalidation, opacity, parent/child tree, and input routing.

| Control | Purpose |
| --- | --- |
| [`CheckBox`](check-box.md) | Toggle a boolean with a box + optional label. |
| [`Frame`](frame.md) | A rectangle with a background fill and an optional border. |
| [`ImageButton`](image-button.md) | A clickable button with an image and/or text. |
| [`ImageLabel`](image-label.md) | A static image, optionally auto-sized. |
| [`TextBox`](text-box.md) | An editable single-line text field with caret. |
| [`TextButton`](text-button.md) | A clickable button that renders a text label. |
| [`TextLabel`](text-label.md) | A static, non-interactive text label. |

!!! info "Package layout"
    Controls are **not** part of `HandyUI.Core` - they live in a separate
    NuGet package, `HandyUI.Controls`. Add it alongside `HandyUI.Core`:

    ```bash
    dotnet add package HandyUI.Core
    dotnet add package HandyUI.Controls
    ```

!!! info "Common conventions"
    - All controls expose a `Theme` property of type
      [`Theme`](../theme.md), defaulting to `Theme.Default`.
    - Changing a themed color marks that channel as *custom*, so a later
      `Theme` swap won't overwrite your choice.
    - Every control overrides `IsEnabled` with a backing field that
      refreshes its paint brushes when toggled.
    - Every control disposes its `SKPaint` instances in `OnDispose`.