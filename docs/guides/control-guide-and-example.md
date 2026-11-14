# Control Guide & Example

In this guide, i will be **teaching** how to make a **custom HandyUI control** and will be giving an custom control **example**. 

!!! warning "Prerequisites"
    First of all, **you will need** a .NET 10 project preferably using C#.<br>
    Second of all, **you will need** to have HandyUI installed on your .NET project. *[(How you can do that)](../../getting-started/installation)*

## Anatomy of a HandyUI Control

Since HandyUI operates as a retained UI Framework, every custom control you build is responsible for three things:

* **1:** **State:** Holding its own properties and turning unnecessary public properties into fields.

* **2:** **Rendering:** Drawing itself, relative to its current state.

* **3:** **Input:** Handling input events and consuming a input event when required.

## Step-by-Step: Building a custom Toggle Switch

Let's build a simple and interactive ToggleSwitch control from scratch.

---

### Create the control.

We will start by **creating a new C# file** called `ToggleSwitch.cs`. You do not particularly need to name it `ToggleSwitch.cs` but we will use this as a placeholder for this guide.

Then, you will **define a class** inside the new `ToggleSwitch.cs` file we just created.

Here's what i have created for this guide:

```csharp linenums="1"
public class ToggleSwitch
{
    // Empty.
}
```

---

### Define the control's properties.

After that, you can define the control class's properties.

Here's what we will add:

```csharp linenums="1"
public class ToggleSwitch
{
    public bool IsToggled { get; private set; }
    public event Action<bool>? WhenToggled;
}
```

### Inherit from `BaseControl` and create the overrides.

Now, here comes an **important part**.

We will make our newly created class **inherit from** `BaseControl`.

Here's how you can do that:

```csharp linenums="1"
public class ToggleSwitch : BaseControl<ToggleSwitch>
{
    public bool IsToggled { get; private set; }
    public event Action<bool>? WhenToggled;

    public override bool DoesIntersect(SKPoint point)
    {
        throw new NotImplementedException();
    }

    public override void OnDraw(SKCanvas canvas)
    {
        // No drawing code yet.
    }
}
```

Well, you might now be asking;

> *"What does `DoesIntersect` do and why did you inherit from `BaseControl<ToggleSwitch>` and not just `BaseControl` directly?"*

!!! info "About the `BaseControl<ToggleSwitch>` one:"
    Well, you can always just use normal BaseControl **BUT** keep in mind that you **can't** use the **fluent APIs** on your control if you dont inherit like that.

    It's called **CRTP** *(Curiously Recurring Template Pattern)* in programming terms and it basically lets the fluent APIs return your control as your control's type instead of converting it into a generic BaseControl.

After that, things get simpler.

---

Anyways, we will now create a override to handle mouse events including clicks.

This is how you can do that:

```csharp linenums="1"
public class ToggleSwitch : BaseControl<ToggleSwitch>
{
    public bool IsToggled { get; private set; }
    public event Action<bool>? WhenToggled;

    public override bool DoesIntersect(SKPoint point)
    {
        throw new NotImplementedException();
    }

    public override void OnDraw(SKCanvas canvas)
    {
        // No drawing code yet.
    }

    protected override bool OnMouse(MouseEventContext eventContext)
    {
        // Nothing to handle yet.
        return false;
    }
}
```

You override the protected OnMouse function and then return a bool.

!!! info "What does that bool mean?"
    Well, it's basically whether your control **consumed the event or not**.<br>
    This can also be worded as whether your control used that event or not.

---

Now, here comes the fun part. **We will add functionality.**

!!! info "How do we do that?"
    We add our functionality to the empty overrides we added to our control.

---

First of all, we will make our control handle mouse events.

That starts with **toggling the switch state** and **respectfully consuming mouse events.**

Here's how we will do that:

!!! tip "Looking for the API Reference?"
    You can find the API Reference for the mouse/key event contexts on [this docs page](../../api/contexts).

```csharp linenums="1"
protected override bool OnMouse(MouseEventContext eventContext)
{
    // Handle clicks
    if (eventContext.Type == MouseEventType.ClickEvent && eventContext.Button == MouseButton.Left)
    {
        IsToggled = !IsToggled;
        WhenToggled?.Invoke(IsToggled);
        return true;
    }

    return false;
}
```

After that, you might be asking; 

> *"But, well, what about drawing the actual control?"*

Yep. We're right on that.

---

### Making the control draw

We will start with defining two `SKPaint`s so that we can actually draw the two toggled states.

```csharp linenums="1"
private readonly SKPaint _toggledPaint = new() { Color = ThemeManager.Instance.SurfacePrimary };
private readonly SKPaint _untoggledPaint = new() { Color = ThemeManager.Instance.DisabledSurface };
```

> *Well, that is a big mouthful, **isn't it?***

Yes, it is. And i will explain it to you.

First of all, we define the SKPaint with this part:<br>`private readonly SKPaint _toggledPaint`<br>
Then, we can continue by creating the paint with this part:<br>`_toggledPaint = new()`<br>
After that, we finally assign the color to the SKPaint:<br>
`{Color = ThemeManager.Instance.TextPrimary }`

!!! tip "You aren't required to use the ThemeManager."
    Yep, that's right. You **aren't forced** to use the ThemeManager.
    I just recommend using it because it gives OS-accent-color based theme colors.
    You can always just use a color like `SKColors.Black`.

---

Now, we will actually draw.

For real, i swear.

No more filler.

---

Here's how:

```csharp linenums="1"
public override void OnDraw(SKCanvas canvas)
{
    // Check the control state and branch depending on the current state.
    if (IsToggled)
    {
        // Draw the toggled state
        canvas.DrawRect(Bounds, _toggledPaint);
    }
    else
    {
        // Draw the untoggled state
        canvas.DrawRect(Bounds, _untoggledPaint);
    }
}
```

That's really all there is to drawing the control.

Oh, wait, we **forgot about something**, didn't we?

---

### Disposing the `SKPaint`s.

Yes, we forgot about **disposing the `SKPaint`s.**

Let's quickly do that.

---

Create the `OnDisposed` override like this:

```csharp linenums="1"
protected override void OnDispose()
{
    _toggledPaint.Dispose();
    _untoggledPaint.Dispose();
}
```
Yeah. I don't really think there's anything to explain here.

Anyways, about the full control code, here it is.

??? info "The full control"
    ```csharp linenums="1"
    using HandyUI.Core.Abstractions.Base;
    using HandyUI.Core.Data;
    using HandyUI.Core.Data.Enums;
    using HandyUI.Core.Helpers;
    using SkiaSharp;

    public class ToggleSwitch : BaseControl<ToggleSwitch>
    {
        public bool IsToggled { get; private set; }
        public event Action<bool>? WhenToggled;

        private readonly SKPaint _toggledPaint = new() { Color = ThemeManager.Instance.SurfacePrimary };
        private readonly SKPaint _untoggledPaint = new() { Color = ThemeManager.Instance.DisabledSurface };

        public override bool DoesIntersect(SKPoint point)
        {
            throw new NotImplementedException();
        }

        public override void OnDraw(SKCanvas canvas)
        {
            // Check the control state and branch depending on the current state.
            if (IsToggled)
            {
                // Draw the toggled state
                canvas.DrawRect(Bounds, _toggledPaint);
            }
            else
            {
                // Draw the untoggled state
                canvas.DrawRect(Bounds, _untoggledPaint);
            }
        }

        protected override bool OnMouse(MouseEventContext eventContext)
        {
            // Handle clicks
            if (eventContext.Type == MouseEventType.ClickEvent && eventContext.Button == MouseButton.Left)
            {
                IsToggled = !IsToggled;
                WhenToggled?.Invoke(IsToggled);
                return true;
            }

            return false;
        }

        protected override void OnDispose()
        {
            _toggledPaint.Dispose();
            _untoggledPaint.Dispose();
        }
    }
    ```