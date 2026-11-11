namespace HandyUI.Core.Data;

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
