using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class IrisTransition : SceneTransition
{
    private static readonly int RadiusId = Shader.PropertyToID("_Radius");
    private static readonly int IsInvertId = Shader.PropertyToID("_IsInvert");

    public override SceneTransitionType Type => SceneTransitionType.Iris;

    [SerializeField] private RawImage rawImage;

    [Header("Iris")]
    [SerializeField] private Ease coverEase = Ease.InOutQuad;
    [SerializeField] private Ease revealEase = Ease.InOutQuad;

    [Header("Icon")]
    [SerializeField] private RectTransform icon;
    [SerializeField] private float iconDuration = 0.25f;
    [SerializeField] private Ease iconInEase = Ease.OutBack;
    [SerializeField] private Ease iconOutEase = Ease.InBack;

    private Material instance;
    private float radius;

    private Material Mat
    {
        get
        {
            if (instance == null)
            {
                instance = new Material(rawImage.material);
                rawImage.material = instance;
            }

            return instance;
        }
    }

    private void OnDestroy()
    {
        DOTween.Kill(this);

        if (instance != null) Destroy(instance);
    }

    public override void Prepare()
    {
        base.Prepare();

        DOTween.Kill(this);

        rawImage.enabled = true;
        rawImage.raycastTarget = true;
    }

    public override void Hide()
    {
        base.Hide();

        DOTween.Kill(this);

        rawImage.enabled = false;
        rawImage.raycastTarget = false;

        SetIcon(0f);
    }

    public override void SetCovered()
    {
        SetIris(0f, true);
        SetIcon(1f);
    }

    public override async Awaitable Cover(float duration)
    {
        SetIcon(0f);

        await TweenIris(false, duration, coverEase);
        await TweenIcon(1f, iconInEase);
    }

    public override async Awaitable Reveal(float duration)
    {
        await TweenIcon(0f, iconOutEase);
        await TweenIris(true, duration, revealEase);
    }

    private Awaitable TweenIris(bool invert, float duration, Ease ease)
    {
        SetIris(0f, invert);

        if (duration <= 0f)
        {
            SetIris(1f, invert);
            return AwaitableEx.CompletedAwaitable();
        }

        return DOTween.To(() => radius, SetRadius, 1f, duration)
            .SetEase(ease)
            .SetUpdate(true)
            .SetTarget(this)
            .ToAwaitable(destroyCancellationToken);
    }

    private Awaitable TweenIcon(float scale, Ease ease)
    {
        if (icon == null || iconDuration <= 0f)
        {
            SetIcon(scale);
            return AwaitableEx.CompletedAwaitable();
        }

        return icon.DOScale(scale, iconDuration)
            .SetEase(ease)
            .SetUpdate(true)
            .SetTarget(this)
            .ToAwaitable(destroyCancellationToken);
    }

    private void SetIris(float value, bool invert)
    {
        Mat.SetFloat(IsInvertId, invert ? 1f : 0f);
        SetRadius(value);
    }

    private void SetRadius(float value)
    {
        radius = value;
        Mat.SetFloat(RadiusId, value);
    }

    private void SetIcon(float scale)
    {
        if (icon != null) icon.localScale = Vector3.one * scale;
    }
}
