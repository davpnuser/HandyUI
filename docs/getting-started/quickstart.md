# Getting started with HandyUI

## Quickstart

!!! note "A quick heads-up"
    All code examples in this page use the C# 9 feature **Top-level Statements**.

Getting started with HandyUI also includes creating a testing window.

You have two options:

* Create **one** window

    _or_

* Create **multiple** windows

=== "Creating a single window"

    ### You can create a window just like this:

    ```csharp linenums="1"
    using HandyUI.Controls.Controls;
    using HandyUI.SilkNet.Data;
    using HandyUI.SilkNet.Helper;

    var (window, renderer) = WindowHelper.CreateWindowAndGetRenderer(WindowConfiguration.Default);

    new Label()
        .WithText("Hello, World!")
        .WithFontSize(24)
        .WithLocation(15, 15)
        .WithAddToRenderer(renderer);

    await window.RunAsync();
    ```

    Here's the result of running the piece of code given above:

    ![A single window running](assets/quickstart/single-window.png)

=== "Creating multiple windows"

    ### You can also create multiple windows at the same time.

    ```csharp linenums="1"
    using HandyUI.Controls.Controls;
    using HandyUI.SilkNet.Data;
    using HandyUI.SilkNet.Helper;

    var (window1, renderer1) = WindowHelper.CreateWindowAndGetRenderer(WindowConfiguration.Default with { Title = "Window 1" });
    var (window2, renderer2) = WindowHelper.CreateWindowAndGetRenderer(WindowConfiguration.Default with { Title = "Window 2" });

    new Label()
        .WithText("Hello, World!")
        .WithFontSize(24)
        .WithLocation(15, 15)
        .WithAddToRenderer(renderer1);

    new Label()
        .WithText("Hello World, Once Again!")
        .WithFontSize(24)
        .WithLocation(15, 15)
        .WithAddToRenderer(renderer2);

    await Task.WhenAll(window1.RunAsync(), window2.RunAsync());
    ```

    Here's the result of running the piece of code given above:

    ![Multiple Windows Running](assets/quickstart/multiple-windows.png)

## Creating a window with a custom configuration

You can do this by passing in a configuration object with parameters into the window creation function.

The example i will give right here is changing the title of the window.

To do this, you can create a WindowConfiguration object with the Title property set to the title you want.

```csharp linenums="1"
using HandyUI.Controls.Controls;
using HandyUI.SilkNet.Data;
using HandyUI.SilkNet.Helper;

var configuration = WindowConfiguration.Default with { Title = "Your title comes here!" };

var (window, renderer) = WindowHelper.CreateWindowAndGetRenderer(configuration);

new Label()
    .WithText("Hello, World!")
    .WithFontSize(24)
    .WithLocation(15, 15)
    .WithAddToRenderer(renderer);

await window.RunAsync();
```

Here's the result of this example:

![A single window running](assets/quickstart/newtitle.png)

For more info on custom configurations, you can check out the [WindowConfiguration](../api/window-configuration.md) page in the API Reference.