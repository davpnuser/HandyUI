using HandyUI.Core.Classes.Records;
using HandyUI.Core.Components;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace HandyUI.WinForms.Classes.Helper;

public static class RendererHelper
{
    public static UIRenderer Attach(SKGLControl glControl)
    {
        ArgumentNullException.ThrowIfNull(glControl);

        var renderer = new UIRenderer();
        var currentMousePos = Point.Empty;

        glControl.MouseMove += (s, e) =>
        {
            currentMousePos = e.Location;

            renderer.ProcessMouseEvent(new MouseEventContext(
                ClientPosition: e.Location.ToSKPoint(),
                Type: MouseEventType.Move
            ));
        };

        glControl.MouseDown += (s, e) =>
        {
            var btn = MapButton(e.Button);
            if (btn == MouseButton.None) return;

            if (!glControl.Focused)
            {
                glControl.Focus();
            }

            renderer.ProcessMouseEvent(new MouseEventContext(
                ClientPosition: e.Location.ToSKPoint(),
                Type: MouseEventType.MouseDown,
                Button: btn
            ));
        };

        glControl.MouseUp += (s, e) =>
        {
            var btn = MapButton(e.Button);
            if (btn == MouseButton.None) return;

            renderer.ProcessMouseEvent(new MouseEventContext(
                ClientPosition: e.Location.ToSKPoint(),
                Type: MouseEventType.MouseUp,
                Button: btn
            ));
        };

        glControl.MouseWheel += (s, e) =>
        {
            renderer.ProcessMouseEvent(new MouseEventContext(
                ClientPosition: e.Location.ToSKPoint(),
                Type: MouseEventType.Wheel,
                Button: MouseButton.Middle,
                WheelDelta: e.Delta
            ));
        };

        glControl.MouseLeave += (s, e) =>
        {
            renderer.ProcessMouseEvent(new MouseEventContext(
                ClientPosition: new SKPoint(-1, -1),
                Type: MouseEventType.Move
            ));
        };

        glControl.KeyDown += (s, e) =>
        {
            var context = CreateKeyEventContext(KeyEventType.KeyDown, e);
            e.Handled = renderer.ProcessKeyEvent(context);
        };

        glControl.KeyUp += (s, e) =>
        {
            var context = CreateKeyEventContext(KeyEventType.KeyUp, e);
            e.Handled = renderer.ProcessKeyEvent(context);
        };

        glControl.KeyPress += (s, e) =>
        {
            var context = new KeyEventContext(
                Type: KeyEventType.CharInput,
                KeyCode: (byte)e.KeyChar,
                Character: e.KeyChar,
                IsControlPressed: (Control.ModifierKeys & Keys.Control) != 0,
                IsShiftPressed: (Control.ModifierKeys & Keys.Shift) != 0,
                IsAltPressed: (Control.ModifierKeys & Keys.Alt) != 0
            );

            e.Handled = renderer.ProcessKeyEvent(context);
        };

        glControl.PaintSurface += (s, e) =>
        {
            renderer.RenderControls(e.Surface.Canvas, currentMousePos.ToSKPoint());
        };

        void OnIdle(object? sender, EventArgs e)
        {
            if (glControl.IsHandleCreated && !glControl.IsDisposed)
            {
                glControl.Invalidate();
            }
        }

        Application.Idle += OnIdle;

        glControl.Disposed += (s, e) =>
        {
            Application.Idle -= OnIdle;
            renderer.Dispose();
        };

        glControl.Show();
        glControl.Focus();
        glControl.Select();

        return renderer;
    }

    private static MouseButton MapButton(MouseButtons button)
    {
        return button.HasFlag(MouseButtons.Left)
            ? MouseButton.Left
            : button.HasFlag(MouseButtons.Right)
            ? MouseButton.Right
            : button.HasFlag(MouseButtons.Middle) ? MouseButton.Middle : MouseButton.None;
    }

    private static KeyEventContext CreateKeyEventContext(KeyEventType type, KeyEventArgs e)
    {
        return new KeyEventContext(
            Type: type,
            KeyCode: (int)e.KeyCode,
            Character: '\0',
            IsControlPressed: e.Control,
            IsShiftPressed: e.Shift,
            IsAltPressed: e.Alt
        );
    }
}