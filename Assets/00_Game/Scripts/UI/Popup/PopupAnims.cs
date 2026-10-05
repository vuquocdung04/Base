using System.Threading;
using DG.Tweening;
using UnityEngine;

public interface IPopupScale { }

public interface IPopupAnim
{
    Awaitable Show(RectTransform panel, CanvasGroup group, float duration, CancellationToken token);
    Awaitable Hide(RectTransform panel, CanvasGroup group, float duration, CancellationToken token);
}

public static class PopupAnims
{
    private const Ease ScaleShowEase = Ease.OutBack;
    private const Ease ScaleHideEase = Ease.InBack;
    private const float HideRatio = 0.8f;

    public static Awaitable ScaleShow(RectTransform panel, CanvasGroup group, float duration, CancellationToken token)
    {
        panel.DOKill();
        panel.localScale = Vector3.zero;
        group.SetCanvasState(true, 1f);

        return panel.DOScale(Vector3.one, duration)
            .SetEase(ScaleShowEase)
            .SetUpdate(true)
            .ToAwaitable(token);
    }

    public static Awaitable ScaleHide(RectTransform panel, CanvasGroup group, float duration, CancellationToken token)
    {
        panel.DOKill();
        group.SetCanvasState(false);

        return panel.DOScale(Vector3.zero, duration * HideRatio)
            .SetEase(ScaleHideEase)
            .SetUpdate(true)
            .ToAwaitable(token);
    }

    public static Awaitable Instant(RectTransform panel, CanvasGroup group, bool visible)
    {
        panel.DOKill();

        if (visible)
        {
            panel.localScale = Vector3.one;
            group.SetCanvasState(true, 1f);
        }
        else
        {
            group.SetCanvasState(false, 0f);
        }

        return AwaitableEx.CompletedAwaitable();
    }
}
