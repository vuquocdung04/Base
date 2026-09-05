using UnityEngine;

public class LobbyScene : MonoBehaviour
{
    public NavController navController;

    [Header("Pages")]
    [SerializeField] private Transform pagesRoot;
    [SerializeField] private BaseBox[] pages;
    [SerializeField] private int shopIndex = 0;

    [Header("Slide")]
    [SerializeField] private float slideDuration = 0.3f;

    private SlideAnim _slideLeft;
    private SlideAnim _slideRight;

    private void OnDestroy()
    {
        if (navController != null)
            navController.OnSelectedChanged -= OnNavChanged;
    }

    public async Awaitable InitAsync()
    {
        _slideLeft = new SlideAnim(SlideSide.Left, slideDuration);
        _slideRight = new SlideAnim(SlideSide.Right, slideDuration);

        navController.Init();

        await PreLoad();

        navController.OnSelectedChanged += OnNavChanged;

        await AwaitableEx.WaitUntil(() => navController.CurrentIndex >= 0, destroyCancellationToken);
        PageAt(navController.CurrentIndex)?.ShowDetached(true);
    }

    private async Awaitable PreLoad()
    {
        ShopBox shop = await PopupManager.GetAsync<ShopBox>();

        if (shop != null)
        {
            if (pagesRoot != null) shop.transform.SetParent(pagesRoot, false);

            if (pages != null && shopIndex >= 0 && shopIndex < pages.Length)
                pages[shopIndex] = shop;
        }

        FXManager.Instance.NotifySceneReady();
    }

    private void OnNavChanged(int previous, int current)
    {
        bool toRight = current > previous;

        PageAt(previous)?.CloseDetached(toRight ? _slideLeft : _slideRight);
        PageAt(current)?.ShowDetached(toRight ? _slideRight : _slideLeft);
    }

    private BaseBox PageAt(int index)
    {
        if (pages == null || index < 0 || index >= pages.Length)
            return null;

        return pages[index];
    }

    public void NavigateTo(int index) => navController.NavigateTo(index);
}
