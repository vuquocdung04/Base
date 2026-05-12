using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LobbyBox : BaseBox<LobbyBox>
{
    public Button btnSetting;
    public Button btnAvatar;

    public Button btnNoAds;
    public Button btnPlay;

    protected override void Init()
    {
        var holder = LobbyController.Instance.topCanvas;
        btnSetting.OnClicked(delegate { _ = SettingBox.Setup(holder, box => { box.Show(); }); });
        btnAvatar.OnClicked(delegate { });
        btnNoAds.OnClicked(delegate { _ = NoAdsBox.Setup(holder, box => { box.Show(); }); });
        btnPlay.OnClicked(delegate
        {
            FXManager.Instance.LoadSceneWithIrisWipe(SceneName.GAME_PLAY);
        });
    }

    protected override void InitState()
    {
    }
}