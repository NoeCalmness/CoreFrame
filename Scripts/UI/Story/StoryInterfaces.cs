using System;

/// <summary>
/// some windows that be opened in story need close action
/// </summary>
public interface IBackToStory
{
    Action onBackToStory{ get; set; }
}

public interface IUIAnimationComplete
{
    Action onComplete { get; set; }
}
