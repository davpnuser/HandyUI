using HandyUI.Core.Components;

namespace HandyUI.Core.Data;

public readonly struct TweenHandle
{
    private readonly TweenJob _job;

    internal TweenHandle(TweenJob job)
    {
        _job = job;
    }

    public bool IsValid => _job != null && !_job.IsCompleted && !_job.IsCancelled;

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
