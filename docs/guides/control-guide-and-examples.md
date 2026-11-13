# Control Guide & Examples

In this guide, i will be **teaching** how to make a **custom HandyUI control** and will be giving some custom control **examples**. 

!!! warning "Prerequisites"
    First of all, **you will need** a .NET 10 project preferably using C#.<br>
    Second of all, **you will need** to have HandyUI installed on your .NET project. *[(How you can do that)](../../getting-started/installation)*

## Anatomy of a HandyUI Control

Since HandyUI operates as a retained UI Framework without an layout engine, every custom control you build is responsible for three things:

* **1:** **State:** Holding its own properties and turning unnecessary public properties into fields.

* **2:** **Rendering:** Drawing itself, relative to its current state.

* **3:** **Input:** Handling input events and consuming a input event when required.

## Step-by-Step: Building a custom Toggle Switch

Let's build a simple and interactive ToggleSwitch control from scratch.

### **Step 1:** Define the control and it's properties.

We will start by **creating a new C# file** called `ToggleSwitch.cs`. You do not particularly need to name it `ToggleSwitch.cs` but we will use this as a placeholder for this guide.

Then, you will **define a class** inside the new `ToggleSwitch.cs` file we just created.

Here's what i have created for this guide:

```csharp linenums="1"
public class ToggleSwitch
{
    // Empty.
}
```

After that, you can define the control class's properties.

Here's what we will add:

```csharp linenums="1"
public class ToggleSwitch
{
    public bool Checked { get; private set; }
    public event Action<bool>? WhenToggled;
}
```
Now, here comes an **important part**.

We will make our newly created class **inherit from** `BaseControl`.

Here's how you can do that:

```csharp linenums="1"
public class ToggleSwitch : BaseControl<ToggleSwitch>
{
    public bool Checked { get; private set; }
    public event Action<bool>? WhenToggled;

    public override void OnDraw(SKCanvas canvas)
    {
        // No drawing code yet.
    }

    public override bool DoesIntersect(SKPoint point)
    {
        return Bounds.Contains(point);
    }
}
```
Well, you might now be asking *"What does `DoesIntersect` do and why did you inherit from `BaseControl<ToggleSwitch>` and not just `BaseControl` directly?"*

!!! info "About the `BaseControl<ToggleSwitch>` one:"
    Well, you can always just use normal BaseControl **BUT** keep in mind that you **can't** use the **fluent APIs** on your control if you dont inherit like that.

    It's called **CRTP** *(Curiously Recurring Template Pattern)* in programming terms and it basically lets the fluent APIs return your control as your control's type instead of converting it into a generic BaseControl.