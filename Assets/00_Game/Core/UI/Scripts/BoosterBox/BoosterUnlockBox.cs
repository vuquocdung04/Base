using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BoosterUnlockBox : BaseBox
{
    public Image imgBooster;
    public Button btnClaim;

    public Transform txtHolder;

    [Header("Jump")]
    [SerializeField] private float jumpPower = 120f;
    [SerializeField] private int jumpCount = 1;
    [SerializeField] private float jumpDuration = 0.6f;
    [SerializeField] private Ease sizeEase = Ease.OutQuad;

    [Header("Gizmos")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private RectTransform targetGizmos;
    [SerializeField] private Color gizmosColor = Color.yellow;
    [SerializeField] private int gizmosSample = 64;
    [SerializeField] private float gizmosRadius = 12f;

    private RectTransform _iconRect;
    private Vector3 _iconBasePosition;
    private Vector2 _iconBaseSize;
    private RectTransform _target;
    private Action _onClaim;
    private Action _onJumpDone;

    protected override void Init()
    {
        _iconRect = imgBooster.rectTransform;
        _iconBasePosition = _iconRect.localPosition;
        _iconBaseSize = _iconRect.sizeDelta;

        btnClaim.OnClicked(Claim);
    }

    protected override void InitState()
    {
        btnClaim.interactable = true;

        _iconRect.localPosition = _iconBasePosition;
        _iconRect.sizeDelta = _iconBaseSize;

        if (txtHolder != null) txtHolder.gameObject.SetActive(true);
    }

    public void Setup(Sprite sprite, RectTransform target, Action onClaim, Action onJumpDone)
    {
        if (sprite != null) imgBooster.sprite = sprite;

        _target = target;
        _onClaim = onClaim;
        _onJumpDone = onJumpDone;
    }

    private void Claim()
    {
        btnClaim.interactable = false;

        PopupManager.HideBackdrop();

        Action claim = _onClaim;
        _onClaim = null;
        claim?.Invoke();

        JumpToTarget().Forget();
    }

    private async Awaitable JumpToTarget()
    {
        if (txtHolder != null) txtHolder.gameObject.SetActive(false);

        if (_target != null)
        {
            Sequence sequence = DOTween.Sequence().SetUpdate(true);

            sequence.Append(_iconRect.DOLocalJump(LocalPositionOf(_target), jumpPower, jumpCount, jumpDuration));
            sequence.Join(_iconRect.DOSizeDelta(_target.sizeDelta, jumpDuration).SetEase(sizeEase));

            await sequence.ToAwaitable(destroyCancellationToken);
        }

        Action done = _onJumpDone;
        _onJumpDone = null;
        done?.Invoke();

        Close();
    }

    private Vector3 LocalPositionOf(RectTransform target)
    {
        RectTransform parent = _iconRect.parent as RectTransform;

        return parent == null ? _iconRect.localPosition : parent.InverseTransformPoint(target.position);
    }

    private Vector3 JumpPositionAt(Vector3 from, Vector3 to, float percent)
    {
        int count = Mathf.Max(1, jumpCount);
        float length = 1f / (count * 2);
        int index = Mathf.Clamp(Mathf.FloorToInt(percent / length), 0, count * 2 - 1);
        float local = (percent - index * length) / length;
        float hop = index % 2 == 0 ? OutQuad(local) : OutQuad(1f - local);

        Vector3 position = Vector3.Lerp(from, to, percent);
        position.y = from.y + jumpPower * hop + (to.y - from.y) * OutQuad(percent);

        return position;
    }

    private static float OutQuad(float value) => value * (2f - value);

    private void OnDrawGizmos()
    {
        if (!drawGizmos || imgBooster == null) return;

        RectTransform rect = imgBooster.rectTransform;
        RectTransform parent = rect.parent as RectTransform;
        RectTransform target = _target != null ? _target : targetGizmos;

        if (parent == null || target == null) return;

        Vector3 from = Application.isPlaying ? _iconBasePosition : rect.localPosition;
        Vector3 to = parent.InverseTransformPoint(target.position);
        int sample = Mathf.Max(2, gizmosSample);

        Gizmos.matrix = parent.localToWorldMatrix;
        Gizmos.color = gizmosColor;

        Vector3 previous = JumpPositionAt(from, to, 0f);

        for (int i = 1; i <= sample; i++)
        {
            Vector3 point = JumpPositionAt(from, to, (float)i / sample);

            Gizmos.DrawLine(previous, point);
            previous = point;
        }

        Gizmos.DrawWireSphere(from, gizmosRadius);
        Gizmos.DrawWireSphere(to, gizmosRadius);
        Gizmos.matrix = Matrix4x4.identity;
    }
}
