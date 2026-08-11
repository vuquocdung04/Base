using System.Threading;
using DG.Tweening;
using UnityEngine;

public enum SlideSide { Left, Right }

public interface IPopupScale { }

public interface IPopupSlide { }

public static class PopupAnims
{
    private const Ease ScaleShowEase = Ease.OutBack;
    private const Ease ScaleHideEase = Ease.InBack;
    private const Ease SlideEase = Ease.OutCubic;
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

    public static Awaitable SlideShow(RectTransform panel, CanvasGroup group, SlideSide from, float duration, CancellationToken token)
    {
        panel.DOKill();
        group.SetCanvasState(true, 1f);
        panel.anchoredPosition = new Vector2(OffscreenX(panel, from), 0f);

        return panel.DOAnchorPos(Vector2.zero, duration)
            .SetEase(SlideEase)
            .SetUpdate(true)
            .ToAwaitable(token);
    }

    public static Awaitable SlideHide(RectTransform panel, CanvasGroup group, SlideSide to, float duration, CancellationToken token)
    {
        panel.DOKill();
        group.SetCanvasState(false);

        return panel.DOAnchorPos(new Vector2(OffscreenX(panel, to), 0f), duration)
            .SetEase(SlideEase)
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

    private static float OffscreenX(RectTransform panel, SlideSide side)
    {
        float width = panel.rect.width > 0f ? panel.rect.width : Screen.width;
        return side == SlideSide.Left ? -width : width;
    }
}
