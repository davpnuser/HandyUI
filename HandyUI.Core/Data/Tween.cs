using HandyUI.Core.Data.Enums;

namespace HandyUI.Core.Data;

public enum TweenStatus
{
    None = 0,
    Playing = 1,
    Stopped = 2,
    Paused = 3,
}

internal class TweenJob
{
    public float StartValue;
    public float EndValue;
    public float Duration;
    public float Elapsed;
    public Action<float>? OnUpdateAction;
    public Action? OnCompleteAction;
    public EaseType Ease;
    public bool IsPaused;
    public bool IsReversed;
    public bool IsCompleted;
    public bool IsCancelled;

    public void Reset()
    {
        Elapsed = 0f;
        IsCompleted = false;
        IsCancelled = false;
        IsPaused = false;
    }
}

public class Tween
{
    private readonly TweenJob _job;

    internal Tween(TweenJob job)
    {
        _job = job;
    }

    public bool IsValid
        => _job != null
            && !_job.IsCompleted
            && !_job.IsCancelled;

    public void Pause()
    {
        _job?.IsPaused = true;
    }

    public void Play()
    {
        _job?.IsPaused = false;
    }

    public void Cancel()
    {
        if (_job != null)
        {
            _job.IsCancelled = true;
            var resetValue = _job.IsReversed ? _job.EndValue : _job.StartValue;
            _job.OnUpdateAction?.Invoke(resetValue);
        }
    }

    public void Reverse()
    {
        _job?.IsReversed = !_job.IsReversed;
    }
}