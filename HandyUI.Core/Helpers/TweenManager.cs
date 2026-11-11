using HandyUI.Core.Abstractions.Base;
using HandyUI.Core.Data;

namespace HandyUI.Core.Helpers;

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