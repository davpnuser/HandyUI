using HandyUI.Core.Classes.Records;
using HandyUI.Core.Components;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SkiaSharp;

namespace HandyUI.SilkNet.Classes.Helper;

public static class SilkRendererHelper
{
    public static UIRenderer Attach(IWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);

        var renderer = new UIRenderer();
        var currentMousePos = new SKPoint(-1, -1);

        GL? gl = null;
        GRContext? grContext = null;
        GRBackendRenderTarget? backendRenderTarget = null;
        SKSurface? skSurface = null;
        IInputContext? inputContext = null;

        void CreateRenderTarget(int width, int height)
        {
            if (gl == null || grContext == null) return;

            skSurface?.Dispose();
            backendRenderTarget?.Dispose();

            gl.GetInteger((GetPName)0x8CA6, out var framebuffer);
            gl.GetInteger((GetPName)0x0D57, out var stencilBits);
            gl.GetInteger((GetPName)0x80A9, out var samples);

            if (stencilBits == 0) stencilBits = 8;

            var maxSamples = grContext.GetMaxSurfaceSampleCount(SKColorType.Rgba8888);
            var sampleCount = Math.Min(samples, maxSamples);
            var fbInfo = new GRGlFramebufferInfo((uint)framebuffer, 0x8058);

            backendRenderTarget = new GRBackendRenderTarget(width, height, sampleCount, stencilBits, fbInfo);
            skSurface = SKSurface.Create(grContext, backendRenderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);

            renderer.Invalidate();
        }

        window.Load += () =>
        {
            gl = GL.GetApi(window);
            var skiaGlInterface = GRGlInterface.Create();
            grContext = GRContext.CreateGl(skiaGlInterface);

            inputContext = window.CreateInput();

            foreach (var mouse in inputContext.Mice)
            {
                mouse.MouseMove += (m, position) =>
                {
                    currentMousePos = new SKPoint(position.X, position.Y);
                    renderer.ProcessMouseEvent(new MouseEventContext(
                        ClientPosition: currentMousePos,
                        Type: MouseEventType.Move
                    ));
                };

                mouse.MouseDown += (m, button) =>
                {
                    renderer.ProcessMouseEvent(new MouseEventContext(
                        ClientPosition: currentMousePos,
                        Type: MouseEventType.MouseDown,
                        Button: MapButton(button)
                    ));
                };

                mouse.MouseUp += (m, button) =>
                {
                    renderer.ProcessMouseEvent(new MouseEventContext(
                        ClientPosition: currentMousePos,
                        Type: MouseEventType.MouseUp,
                        Button: MapButton(button)
                    ));
                };

                mouse.Scroll += (m, scroll) =>
                {
                    renderer.ProcessMouseEvent(new MouseEventContext(
                        ClientPosition: currentMousePos,
                        Type: MouseEventType.Wheel,
                        Button: HandyUI.Core.Classes.Records.MouseButton.Middle,
                        WheelDelta: (int)(scroll.Y * 120)
                    ));
                };
            }

            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += (k, key, keyCode) =>
                {
                    var mappedKey = MapKey(key);
                    var isControl = k.IsKeyPressed(Key.ControlLeft) || k.IsKeyPressed(Key.ControlRight);
                    var isShift = k.IsKeyPressed(Key.ShiftLeft) || k.IsKeyPressed(Key.ShiftRight);
                    var isAlt = k.IsKeyPressed(Key.AltLeft) || k.IsKeyPressed(Key.AltRight);

                    renderer.ProcessKeyEvent(new KeyEventContext(
                        Type: KeyEventType.KeyDown,
                        KeyCode: mappedKey,
                        Character: '\0',
                        IsControlPressed: isControl,
                        IsShiftPressed: isShift,
                        IsAltPressed: isAlt
                    ));

                    char? synthesizedChar = key switch
                    {
                        Key.Tab => '\t',
                        Key.Enter => '\r',
                        Key.Backspace => '\b',
                        _ => null
                    };

                    if (synthesizedChar.HasValue)
                    {
                        renderer.ProcessKeyEvent(new KeyEventContext(
                            Type: KeyEventType.CharInput,
                            KeyCode: (byte)synthesizedChar.Value,
                            Character: synthesizedChar.Value,
                            IsControlPressed: isControl,
                            IsShiftPressed: isShift,
                            IsAltPressed: isAlt
                        ));
                    }
                };

                keyboard.KeyUp += (k, key, keyCode) =>
                {
                    renderer.ProcessKeyEvent(new KeyEventContext(
                        Type: KeyEventType.KeyUp,
                        KeyCode: MapKey(key),
                        Character: '\0',
                        IsControlPressed: k.IsKeyPressed(Key.ControlLeft) || k.IsKeyPressed(Key.ControlRight),
                        IsShiftPressed: k.IsKeyPressed(Key.ShiftLeft) || k.IsKeyPressed(Key.ShiftRight),
                        IsAltPressed: k.IsKeyPressed(Key.AltLeft) || k.IsKeyPressed(Key.AltRight)
                    ));
                };

                keyboard.KeyChar += (k, character) =>
                {
                    renderer.ProcessKeyEvent(new KeyEventContext(
                        Type: KeyEventType.CharInput,
                        KeyCode: (byte)character,
                        Character: character,
                        IsControlPressed: false,
                        IsShiftPressed: false,
                        IsAltPressed: false
                    ));
                };
            }

            CreateRenderTarget(window.Size.X, window.Size.Y);
        };

        window.Resize += (size) =>
        {
            if (gl == null) return;
            gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
            CreateRenderTarget(size.X, size.Y);
        };

        window.Render += (delta) =>
        {
            if (skSurface?.Canvas == null) return;

            if (renderer.RenderControls(skSurface.Canvas, currentMousePos))
            {
                skSurface.Canvas.Flush();
                grContext?.Flush();
            }
        };

        window.Closing += () =>
        {
            skSurface?.Dispose();
            backendRenderTarget?.Dispose();
            grContext?.Dispose();
            renderer.Dispose();
            inputContext?.Dispose();
            gl?.Dispose();
        };

        return renderer;
    }

    private static int MapKey(Key key)
    {
        return key switch
        {
            Key.Tab => 9,
            Key.Enter => 13,
            Key.Backspace => 8,
            Key.Escape => 27,
            Key.Delete => 46,
            Key.Space => 32,
            Key.Left => 37,
            Key.Up => 38,
            Key.Right => 39,
            Key.Down => 40,
            Key.Home => 36,
            Key.End => 35,
            _ => (int)key
        };
    }

    private static Core.Classes.Records.MouseButton MapButton(Silk.NET.Input.MouseButton button)
    {
        return button switch
        {
            Silk.NET.Input.MouseButton.Left => Core.Classes.Records.MouseButton.Left,
            Silk.NET.Input.MouseButton.Right => Core.Classes.Records.MouseButton.Right,
            Silk.NET.Input.MouseButton.Middle => Core.Classes.Records.MouseButton.Middle,
            _ => Core.Classes.Records.MouseButton.None
        };
    }
}