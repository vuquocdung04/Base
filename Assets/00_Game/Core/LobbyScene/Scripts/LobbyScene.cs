using UnityEngine;
using UnityEngine.UI;

public class LobbyScene : MonoBehaviour
{
    public NavController navController;
    public Button btnHeart;

    public Button btnCoin;
    public async Awaitable InitAsync()
    {
        navController.Init();

        await PreLoad();

        btnHeart.OnClicked(delegate
        {
            HeartManager.Instance.TryShowHeartOffer();
        });
        btnCoin.OnClicked(delegate
        {
            navController.NavigateTo(ENavType.Shop);
        });
    }

    private static async Awaitable PreLoad()
    {
        await AwaitableEx.WhenAll(
            PopupManager.Preload<LobbyBox>(),
            PopupManager.Preload<ShopBox>(),
            PopupManager.Preload<RankBox>());

        PopupManager.Peek<LobbyBox>()?.ShowDetached();

        FXManager.Instance.isNextSceneReady = true;
    }

    public void NavigateTo(ENavType type) => navController.NavigateTo(type);
}