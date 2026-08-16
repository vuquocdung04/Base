using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyBox : BaseBox, IPopupSlide
{
    [Header("Level")]
    [SerializeField] private TextMeshProUGUI txtLevel;

    [Header("Buttons")]
    public Button btnSetting;
    public Button btnAvatar;
    public Button btnNoAds;
    public Button btnPlay;
    public Button btnHeart;
    public Button btnCoin;

    [Header("Avatar")]
    public Image iconAvatar;

    protected override void Init()
    {
        btnSetting.OnClicked(delegate { PopupManager.Show<SettingLobbyBox>().Forget(); });
        btnAvatar.OnClicked(delegate { PopupManager.Show<AvatarBox>().Forget(); });
        //btnNoAds.OnClicked(delegate { PopupManager.Show<NoAdsBox>().Forget(); });
        btnHeart.OnClicked(delegate { HeartManager.Instance.TryShowHeartOffer(); });
        btnCoin.OnClicked(delegate { NavController.Instance.NavigateTo(0); });
    }

    protected override void InitState()
    {
        txtLevel.text = $"Level {UseProfile.Level}";
        RefreshProfile();
    }

    public void RefreshProfile()
    {
        AvatarDataBase data = DataRepo.Instance != null ? DataRepo.Instance.avatarData : null;
        if (data != null) SetAvatar(data.GetSpriteById(UseProfile.AvatarId));
    }

    public void SetAvatar(Sprite sprite) => iconAvatar.sprite = sprite;
}
