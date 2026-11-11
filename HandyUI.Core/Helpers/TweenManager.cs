using HandyUI.Core.Abstractions.Base;
using HandyUI.Core.Data;

namespace HandyUI.Core.Helpers;

public static class Easing
{
    public static float Evaluate(float t, EaseType easeType)
    {
        t = Math.Clamp(t, 0f, 1f);
        return easeType switch
        {
            EaseType.Linear => t,
            EaseType.CubicIn => t * t * t,
            EaseType.CubicOut => 1f - MathF.Pow(1f - t, 3f),
            EaseType.CubicInOut => t < 0.5f ? 4f * t * t * t : 1f - (MathF.Pow((-2f * t) + 2f, 3f) / 2f),
            EaseType.SineIn => 1f - MathF.Cos(t * MathF.PI / 2f),
            EaseType.SineOut => MathF.Sin(t * MathF.PI / 2f),
            EaseType.SineInOut => -(MathF.Cos(MathF.PI * t) - 1f) / 2f,
            EaseType.QuartIn => t * t * t * t,
            EaseType.QuartOut => 1f - MathF.Pow(1f - t, 4f),
            EaseType.QuartInOut => t < 0.5f ? 8f * t * t * t * t : 1f - (MathF.Pow((-2f * t) + 2f, 4f) / 2f),
            _ => t
        };
    }
}

public class TweenManager : BaseModule<TweenManager>
{
    private readonly List<TweenJob> _tweens = [];
    private readonly List<TweenJob> _addQueue = [];
    private readonly Lock _lock = new();

    public TweenManager()
    {
        Start();
    }

    public Tween To(float start, float end, float duration, Action<float> onUpdate, EaseType ease = EaseType.Linear, Action? onComplete = null)
    {
        var job = new TweenJob
        {
            StartValue = start,
            EndValue = end,
            Duration = Math.Max(duration, 0.0001f),
            OnUpdateAction = onUpdate,
            OnCompleteAction = onComplete,
            Ease = ease
        };

        lock (_lock)
        {
            _addQueue.Add(job);
        }

        return new Tween(job);
    }

    public override void OnUpdate(UpdateContext updateContext)
    {
        if (!Enabled)
            return;

        lock (_lock)
        {
            if (_addQueue.Count > 0)
            {
                _tweens.AddRange(_addQueue);
                _addQueue.Clear();
            }

            if (_tweens.Count == 0) return;

            for (var i = _tweens.Count - 1; i >= 0; i--)
            {
                var tween = _tweens[i];

                if (tween.IsCancelled)
                {
                    _tweens.RemoveAt(i);
                    continue;
                }

                if (tween.IsPaused || tween.IsCompleted)
                {
                    continue;
                }

                tween.Elapsed += updateContext.DeltaTime;
                var rawProgress = Math.Clamp(tween.Elapsed / tween.Duration, 0f, 1f);
                if (tween.IsReversed)
                {
                    rawProgress = 1f - rawProgress;
                }

                var easedProgress = Easing.Evaluate(rawProgress, tween.Ease);
                var currentValue = tween.StartValue + ((tween.EndValue - tween.StartValue) * easedProgress);

                tween.OnUpdateAction?.Invoke(currentValue);

                if (tween.Elapsed >= tween.Duration)
                {
                    tween.IsCompleted = true;
                    tween.OnCompleteAction?.Invoke();
                    _tweens.RemoveAt(i);
                }
            }
        }
    }
}