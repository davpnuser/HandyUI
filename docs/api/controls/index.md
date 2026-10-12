# Controls

Built-in controls shipped in `HandyUI.Core.Classes.Controls`. All of them
derive from [`UIControlBase<TSelf>`](../ui-control-base.md), so they inherit
the fluent `With…` pattern, invalidation, opacity, parent/child tree, and
input routing.

| Control | Purpose |
| --- | --- |
| [`CheckBox`](check-box.md) | Toggle a boolean with a box + optional label. |
| [`Frame`](frame.md) | A rectangle with a background fill and an optional border. |
| [`ImageButton`](image-button.md) | A clickable button with an image and/or text. |
| [`ImageLabel`](image-label.md) | A static image, optionally auto-sized. |
| [`TextBox`](text-box.md) | An editable single-line text field with caret & selection. |
| [`TextButton`](text-button.md) | A clickable button that renders a text label. |
| [`TextLabel`](text-label.md) | A static, non-interactive text label. |

!!! info "Common conventions"
    - All controls expose a `Theme` property of type
      [`ThemeRecord`](../theme-record.md).
    - Changing a themed color marks that channel as *custom*, so a later
      `Theme` swap won't overwrite your choice.
    - Every control overrides `IsEnabled` with a backing field that
      refreshes its paint brushes when toggled.
    - Every control disposes its `SKPaint` instances in `OnDispose`.