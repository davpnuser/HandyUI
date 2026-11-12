using HandyUI.Core.Abstractions.Base;
using HandyUI.Core.Data;
using SkiaSharp;

namespace HandyUI.Core.Helpers.Controls;

internal class ProfilerV2Control : BaseControl<ProfilerV2Control>
{
    private float _targetFps = 0f;
    private float _fps = 0f;
    private readonly SKPaint _fgPaint = new() { Color = SKColors.White, Style = SKPaintStyle.Fill };
    private readonly SKPaint _bgPaint = new() { Color = SKColors.Black, Style = SKPaintStyle.Fill };

    public override bool DoesIntersect(SKPoint point)
        => Bounds.Contains(point);

    public override void OnDraw(SKCanvas canvas)
    {

    }

    public override void OnUpdate(UpdateContext updateContext)
    {
        var deltaTime = updateContext.DeltaTime;

        if (deltaTime <= 0f)
            return;

        var instantFps = 1f / deltaTime;

        if (_fps == 0f)
            _fps = instantFps;

        var smoothingFactor = Math.Clamp(deltaTime * 10f, 0f, 1f);

        _fps += (instantFps - _fps) * smoothingFactor;

        if (_targetFps == 0f || (_targetFps != updateContext.Host.FramesPerSecond))
            _targetFps = updateContext.Host.FramesPerSecond;
    }
}