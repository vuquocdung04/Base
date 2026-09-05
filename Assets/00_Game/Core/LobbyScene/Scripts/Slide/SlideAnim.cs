using System.Threading;
using DG.Tweening;
using UnityEngine;

public enum SlideSide { Left, Right }

public class SlideAnim : IPopupAnim
{
    private const Ease SlideEase = Ease.OutCubic;

    private readonly SlideSide _side;
    private readonly float _duration;

    public SlideAnim(SlideSide side, float duration = 0f)
    {
        _side = side;
        _duration = duration;
    }

    public Awaitable Show(RectTransform panel, CanvasGroup group, float duration, CancellationToken token)
    {
        panel.DOKill();
        panel.localScale = Vector3.one;
        group.SetCanvasState(true, 1f);
        panel.anchoredPosition = new Vector2(OffscreenX(panel), 0f);

        return panel.DOAnchorPos(Vector2.zero, Resolve(duration))
            .SetEase(SlideEase)
            .SetUpdate(true)
            .ToAwaitable(token);
    }

    public Awaitable Hide(RectTransform panel, CanvasGroup group, float duration, CancellationToken token)
    {
        panel.DOKill();
        group.SetCanvasState(false);

        return panel.DOAnchorPos(new Vector2(OffscreenX(panel), 0f), Resolve(duration))
            .SetEase(SlideEase)
            .SetUpdate(true)
            .OnComplete(() => panel.anchoredPosition = Vector2.zero)
            .ToAwaitable(token);
    }

    private float Resolve(float duration) => _duration > 0f ? _duration : duration;

    private float OffscreenX(RectTransform panel)
    {
        float width = panel.rect.width > 0f ? panel.rect.width : Screen.width;
        return _side == SlideSide.Left ? -width : width;
    }
}
