using System;
using UnityEngine;
using UnityEngine.UI;

public class LobbyScene : MonoBehaviour
{
    private static readonly Func<BaseBox>[] Boxes =
    {
        PopupManager.Peek<ShopBox>,
        PopupManager.Peek<RankBox>,
        PopupManager.Peek<LobbyBox>,
        PopupManager.Peek<TeamBox>,
        PopupManager.Peek<CollectBox>
    };

    public NavController navController;
    public Button btnHeart;

    public Button btnCoin;

    private void OnDestroy()
    {
        if (navController != null)
            navController.OnSelectedChanged -= OnNavChanged;
    }

    public async Awaitable InitAsync()
    {
        navController.OnSelectedChanged += OnNavChanged;
        navController.Init();

        await PreLoad();

        btnHeart.OnClicked(delegate
        {
            HeartManager.Instance.TryShowHeartOffer();
        });
        btnCoin.OnClicked(delegate
        {
            navController.NavigateTo(0);
        });
    }

    private static async Awaitable PreLoad()
    {
        await AwaitableEx.WhenAll(
            PopupManager.Preload<LobbyBox>(),
            PopupManager.Preload<ShopBox>(),
            PopupManager.Preload<RankBox>(),
            PopupManager.Preload<TeamBox>(),
            PopupManager.Preload<CollectBox>());

        PopupManager.Peek<LobbyBox>()?.ShowDetached();

        FXManager.Instance.isNextSceneReady = true;
    }

    private void OnNavChanged(int previous, int current)
    {
        bool toRight = current > previous;

        BoxAt(previous)?.CloseDetached(toRight ? SlideSide.Left : SlideSide.Right);
        BoxAt(current)?.ShowDetached(toRight ? SlideSide.Right : SlideSide.Left);
    }

    private static BaseBox BoxAt(int index)
    {
        if (index < 0 || index >= Boxes.Length)
            return null;

        return Boxes[index]();
    }

    public void NavigateTo(int index) => navController.NavigateTo(index);
}
