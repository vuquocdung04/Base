using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BoosterIntro : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CanvasGroup holder;
    [SerializeField] private GameObject content;
    [SerializeField] private Image icon;
    [SerializeField] private Button button;
    [SerializeField] private GameObject tapToContinue;

    [Header("Show")]
    [SerializeField] private float showDelay = 0.05f;
    [SerializeField] private float showDuration = 0.35f;
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float tapDelay = 0.3f;
    [SerializeField] private float contentScaleFrom = 0.85f;
    [SerializeField] private float iconDelayPercent = 0.25f;
    [SerializeField] private Ease showEase = Ease.OutBack;

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

    private BoosterCatalogSO _catalog;
    private RectTransform _iconRect;
    private Vector3 _iconBasePosition;
    private Vector3 _iconBaseScale;
    private Vector2 _iconBaseSize;
    private Vector3 _contentBaseScale;

    private RectTransform _target;
    private Action _onTap;
    private Action _onComplete;
    private Sequence _showSequence;
    private Sequence _jumpSequence;

    public bool IsShowing => holder != null && holder.gameObject.activeSelf;

    public void Init(BoosterCatalogSO catalog)
    {
        _catalog = catalog;

        if (icon != null)
        {
            _iconRect = icon.rectTransform;
            _iconBasePosition = _iconRect.localPosition;
            _iconBaseScale = _iconRect.localScale;
            _iconBaseSize = _iconRect.sizeDelta;
        }

        _contentBaseScale = content != null ? content.transform.localScale : Vector3.one;

        if (button != null) button.onClick.AddListener(HandleClick);

        SetHolder(false);
        SetReady(false);
    }

    private void OnDestroy() => Kill();

    public void Show(int levelUnlock, RectTransform target, Action onComplete, Action onTap = null)
    {
        Kill();

        _target = target;
        _onTap = onTap;
        _onComplete = onComplete;

        SetSprite(levelUnlock);
        SetHolder(true);
        SetReady(false);
        PlayShow();
    }

    public void SetSprite(Sprite sprite)
    {
        if (icon != null && sprite != null) icon.sprite = sprite;
    }

    public void SetSprite(int levelUnlock)
    {
        BoosterEntry entry = _catalog != null ? _catalog.FindByLevelUnlock(levelUnlock) : null;

        if (entry == null)
        {
            Debug.LogWarning($"[BoosterIntro] Khong co booster nao unlock o level {levelUnlock}.");
            return;
        }

        SetSprite(BoosterCatalogSO.IconOf(entry.id));
    }

    public void DoJump(RectTransform target, Action onComplete)
    {
        Kill();

        if (_iconRect == null || target == null)
        {
            SetHolder(false);
            onComplete?.Invoke();
            return;
        }

        SetReady(false);
        SetContent(false);

        if (holder != null) holder.alpha = 1f;

        _iconRect.localScale = _iconBaseScale;

        _jumpSequence = _iconRect.DOLocalJump(LocalPositionOf(target), jumpPower, jumpCount, jumpDuration);
        _jumpSequence.Join(_iconRect.DOSizeDelta(target.sizeDelta, jumpDuration).SetEase(sizeEase));
        _jumpSequence.SetTarget(this).SetUpdate(true);

        _jumpSequence.OnComplete(() =>
        {
            _jumpSequence = null;

            onComplete?.Invoke();
        });
    }

    public void Hide()
    {
        Debug.Log($"[BoosterIntro] Hide - holder={(holder == null ? "null" : holder.gameObject.activeSelf.ToString())}", this);

        Kill();
        SetHolder(false);
    }

    private void PlayShow()
    {
        float fade = Mathf.Max(0f, fadeDuration);

        if (_iconRect != null)
        {
            _iconRect.localPosition = _iconBasePosition;
            _iconRect.sizeDelta = _iconBaseSize;
            _iconRect.localScale = Vector3.zero;
        }

        if (content != null) content.transform.localScale = _contentBaseScale * contentScaleFrom;
        if (holder != null) holder.alpha = 0f;

        _showSequence = DOTween.Sequence().SetTarget(this).SetUpdate(true);
        _showSequence.AppendInterval(showDelay);

        if (holder != null) _showSequence.Append(holder.DOFade(1f, fade));

        if (content != null)
            _showSequence.Join(content.transform.DOScale(_contentBaseScale, showDuration).SetEase(showEase));

        if (_iconRect != null)
            _showSequence.Insert(showDelay + showDuration * iconDelayPercent,
                _iconRect.DOScale(_iconBaseScale, showDuration).SetEase(showEase));

        _showSequence.AppendInterval(Mathf.Max(0f, tapDelay));

        _showSequence.OnComplete(() =>
        {
            _showSequence = null;

            SetReady(true);
        });
    }

    private void HandleClick()
    {
        Action tap = _onTap;
        _onTap = null;
        tap?.Invoke();

        DoJump(_target, _onComplete);
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
        if (!drawGizmos || icon == null) return;

        RectTransform rect = icon.rectTransform;
        RectTransform parent = rect.parent as RectTransform;
        RectTransform to = _target != null ? _target : targetGizmos;

        if (parent == null || to == null) return;

        Vector3 from = Application.isPlaying ? _iconBasePosition : rect.localPosition;
        Vector3 end = parent.InverseTransformPoint(to.position);
        int sample = Mathf.Max(2, gizmosSample);

        Gizmos.matrix = parent.localToWorldMatrix;
        Gizmos.color = gizmosColor;

        Vector3 previous = JumpPositionAt(from, end, 0f);

        for (int i = 1; i <= sample; i++)
        {
            Vector3 point = JumpPositionAt(from, end, (float)i / sample);

            Gizmos.DrawLine(previous, point);
            previous = point;
        }

        Gizmos.DrawWireSphere(from, gizmosRadius);
        Gizmos.DrawWireSphere(end, gizmosRadius);
        Gizmos.matrix = Matrix4x4.identity;
    }

    private void SetHolder(bool value)
    {
        SetContent(value);

        if (holder == null) return;

        holder.alpha = value ? 1f : 0f;
        holder.blocksRaycasts = value;
        holder.interactable = value;
        holder.gameObject.SetActive(value);
    }

    private void SetContent(bool value)
    {
        if (content != null) content.SetActive(value);
    }

    private void SetReady(bool value)
    {
        if (button != null) button.interactable = value;
        if (tapToContinue != null) tapToContinue.SetActive(value);
    }

    private void Kill()
    {
        if (_showSequence != null)
        {
            _showSequence.Kill();
            _showSequence = null;
        }

        if (_jumpSequence == null) return;

        _jumpSequence.Kill();
        _jumpSequence = null;
    }
}
