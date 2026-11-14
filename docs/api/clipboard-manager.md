# `ClipboardManager`

`HandyUI.Core.Helpers.ClipboardManager`

A process-wide singleton that holds the most recent clipboard text and image.
Core knows nothing about the OS clipboard - that's the host adapter's job.
On the Silk.NET side, [`ClipboardHelper`](silknet-helpers.md#clipboardhelper)
bridges `ClipboardManager` to the OS clipboard.

```csharp
public sealed class ClipboardManager : IDisposable
```

## Singleton

```csharp
public static ClipboardManager Instance { get; }
```

## Properties

### `ClipboardContentsText`

```csharp
public string? ClipboardContentsText { get; set; }
```

The current clipboard text. Setting this raises `ClipboardUpdated` with
`ClipboardItemType.Text`. Read/write, thread-safe.

```csharp
var current = ClipboardManager.Instance.ClipboardContentsText;
```

### `ClipboardContentsImage`

```csharp
public SKImage? ClipboardContentsImage { get; set; }
```

The current clipboard image. Setting this disposes the previous image (if
any) and raises `ClipboardUpdated` with `ClipboardItemType.Image`.

!!! warning "Ownership of the `SKImage`"
    `ClipboardManager` disposes the previous image when a new image is
    assigned. If you read `ClipboardContentsImage` and keep the reference,
    do not dispose it yourself - you'd invalidate the manager's copy.

## Events

### `ClipboardUpdated`

```csharp
public event Action<ClipboardItemType>? ClipboardUpdated;
```

Raised when `ClipboardContentsText` or `ClipboardContentsImage` is written.
The argument tells you which side changed.

### `OnClipboardSet`

```csharp
public event Action<string>? OnClipboardSet;
```

Raised by `SetClipboard(string)`. This is the *outbound* direction: your
app wants to write text to the OS clipboard, and the host adapter picks
the event up and performs the actual write.

## Methods

### `SetClipboard(string)`

```csharp
public void SetClipboard(string text);
```

Requests that `text` be written to the OS clipboard. Internally just raises
`OnClipboardSet` - the host adapter is responsible for actually touching the
OS clipboard.

```csharp
ClipboardManager.Instance.SetClipboard("copied from my app");
```

!!! info "Round-trip"
    `SetClipboard` does **not** update `ClipboardContentsText` directly. The
    host adapter writes to the OS clipboard, then its clipboard monitor
    fires and feeds the new contents back through
    `ClipboardManager.ClipboardContentsText`, which raises
    `ClipboardUpdated`. This keeps the manager as a single source of truth
    for "what's currently in the clipboard", even across other apps.

### `Dispose()`

```csharp
public void Dispose();
```

Disposes the held image (if any). Normally you don't dispose the singleton
yourself - it lives for the process lifetime.

## Typical flow

```text
[App]         -> ClipboardManager.SetClipboard(text)
                        │
                        ▼ (OnClipboardSet event)
[Host adapter] -> writes to OS clipboard
                        │
                        ▼ (OS-level clipboard monitor fires)
[Host adapter] -> ClipboardManager.ClipboardContentsText = text
                        │
                        ▼ (ClipboardUpdated event)
[App]         <- observes clipboard contents
```

## Example

```csharp
// Write to the OS clipboard (via any registered adapter).
ClipboardManager.Instance.SetClipboard("Hello, world!");

// Observe changes (whether from your app or another one).
ClipboardManager.Instance.ClipboardUpdated += kind =>
{
    if (kind == ClipboardItemType.Text)
        Console.WriteLine($"Text: {ClipboardManager.Instance.ClipboardContentsText}");
    else if (kind == ClipboardItemType.Image)
        Console.WriteLine($"Image: {ClipboardManager.Instance.ClipboardContentsImage?.Width}px");
};
```

## `ClipboardItemType`

`HandyUI.Core.Data.Enums.ClipboardItemType`

```csharp
public enum ClipboardItemType
{
    None = 0,
    Text = 1,
    Image = 2
}
```