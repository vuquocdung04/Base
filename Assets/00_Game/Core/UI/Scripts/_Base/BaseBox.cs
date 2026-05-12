using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(GraphicRaycaster))]
public abstract class BaseBox<T> : MonoBehaviour where T : BaseBox<T>
{
    // ==========================================
    // 1. SINGLETON & ADDRESSABLES LOGIC
    // ==========================================
    public static T Instance { get; private set; }
    private static AsyncOperationHandle<GameObject> handle;
    private static bool isInstantiating;

    public static async UniTaskVoid Setup(Transform parent, System.Action<T> callback)
    {
        string addressableKey = typeof(T).Name;
        var instance = await GetInstanceAsync(addressableKey, parent);
        callback?.Invoke(instance);
    }

    private static async UniTask<T> GetInstanceAsync(string addressableKey, Transform parent)
    {
        if (Instance != null)
        {
            return Instance;
        }

        if (isInstantiating)
        {
            await UniTask.WaitUntil(() => Instance != null || !isInstantiating);
            return Instance;
        }

        isInstantiating = true;
        handle = Addressables.InstantiateAsync(addressableKey, parent);
        GameObject obj = await handle.Task;
        
        if (obj == null)
        {
            Debug.LogError($"[BaseBox] Không tìm thấy key: {addressableKey}");
            isInstantiating = false;
            return null;
        }
        Instance = obj.GetComponent<T>();

        if (Instance == null)
        {
            Addressables.ReleaseInstance(obj);
            isInstantiating = false;
            return null;
        }
        Instance.ForceHide();
        
        Instance.Init();

        isInstantiating = false;
        return Instance;
    }

    protected abstract void Init();
    protected abstract void InitState();

    protected virtual void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            isInstantiating = false;
        }

        if (handle.IsValid())
        {
            Addressables.ReleaseInstance(gameObject);
        }
    }

    // ==========================================
    // 2. UI ANIMATION & CANVAS GROUP LOGIC
    // ==========================================
    [Header("UI Animation Settings")] [SerializeField]
    protected RectTransform mainPanel;

    [SerializeField] protected CanvasGroup canvasGroup;
    [SerializeField] protected bool isAnim = true;
    [SerializeField] protected float durationAppeared = 0.3f;
    [SerializeField] protected float durationSlide = 0.2f;

    private Tween currentTween;
    private Tween fadeTween;

    public void Show()
    {
        InitState();
        
        KillCurrentTweens();
        transform.SetAsLastSibling();
        if (isAnim)
        {
            mainPanel.localScale = Vector3.zero;
            canvasGroup.SetCanvasState(true,0);
            DoAppearAnimation();
        }
        else
        {
            mainPanel.localScale = Vector3.one;
            canvasGroup.SetCanvasState(true,1);
        }
    }

    private void DoAppearAnimation()
    {
        currentTween = mainPanel.DOScale(Vector3.one, durationAppeared).SetEase(Ease.OutBack);
        fadeTween = canvasGroup.DOFade(1f, durationAppeared * 0.8f).SetEase(Ease.OutQuad);
    }


    protected void Close()
    {
        KillCurrentTweens();

        canvasGroup.SetCanvasState(false);

        if (isAnim)
        {
            currentTween = mainPanel.DOScale(Vector3.zero, durationAppeared * 0.8f).SetEase(Ease.InBack);
            fadeTween = canvasGroup.DOFade(0f, durationAppeared * 0.8f)
                .SetEase(Ease.InQuad)
                .OnComplete(ForceHide);
        }
        else
        {
            ForceHide();
        }
    }

    // ==========================================
    // 3. SLIDING LOGIC (Cũng áp dụng Canvas Group)
    // ==========================================
    public Tween ShowSliding(bool slideInFromLeft)
    {
        InitState();

        KillCurrentTweens();
        transform.SetAsLastSibling();

        canvasGroup.SetCanvasState(true, 1f);

        RectTransform self = (RectTransform)transform;
        float slideWidth = mainPanel.rect.width > 0 ? mainPanel.rect.width : Screen.width;
        float startX = slideInFromLeft ? -slideWidth : slideWidth;
        self.anchoredPosition = new Vector2(startX, 0);

        currentTween = self.SlideTo(Vector2.zero, durationSlide);
        return currentTween;
    }

    public Tween CloseSliding(bool slideOutToLeft)
    {
        KillCurrentTweens();

        RectTransform self = (RectTransform)transform;
        float slideWidth = mainPanel.rect.width > 0 ? mainPanel.rect.width : Screen.width;
        float endX = slideOutToLeft ? -slideWidth : slideWidth;

        var seq = DOTween.Sequence();
        seq.Append(self.SlideTo(new Vector2(endX, 0), durationSlide, Ease.InCubic));
        seq.AppendCallback(() => canvasGroup.SetCanvasState(false, 0f));

        currentTween = seq;
        return seq;
    }

    private void ForceHide()
    {
        canvasGroup.SetCanvasState(false, 0f);
    }

    private void KillCurrentTweens()
    {
        currentTween?.Kill();
        fadeTween?.Kill();
        currentTween = null;
        fadeTween = null;
    }
}