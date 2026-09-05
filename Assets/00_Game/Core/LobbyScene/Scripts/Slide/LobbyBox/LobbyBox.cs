using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyBox : BaseBox
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
        btnSetting.onClick.AddListener(delegate { PopupManager.Show<SettingLobbyBox>().Forget(); });
        btnAvatar.onClick.AddListener(delegate { PopupManager.Show<AvatarBox>().Forget(); });
        //btnNoAds.onClick.AddListener(delegate { PopupManager.Show<NoAdsBox>().Forget(); });
        btnHeart.onClick.AddListener(delegate { HeartManager.Instance.TryShowHeartOffer(); });
        btnCoin.onClick.AddListener(delegate { NavController.Instance.NavigateTo(0); });

        this.RegisterListener(EventID.PROFILE_CHANGED, OnProfileChanged);
    }

    protected override void OnDestroy()
    {
        this.RemoveListener(EventID.PROFILE_CHANGED, OnProfileChanged);
        base.OnDestroy();
    }

    private void OnProfileChanged(object param) => RefreshProfile();

    protected override void InitState()
    {
        txtLevel.text = $"Level {UseProfile.Level}";
        RefreshProfile();
    }

    public void RefreshProfile()
    {
        AvatarCatalogSO data = DataRepo.Instance != null ? DataRepo.Instance.avatarData : null;
        if (data != null) SetAvatar(data.GetSpriteById(UseProfile.AvatarId));
    }

    public void SetAvatar(Sprite sprite) => iconAvatar.sprite = sprite;
}
