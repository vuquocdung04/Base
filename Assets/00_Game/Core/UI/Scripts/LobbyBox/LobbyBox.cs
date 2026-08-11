using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyBox : BaseBox, IPopupSlide
{
    [Header("Level Nodes")]
    [SerializeField] private List<LevelNode> levelNodes;

    [Header("Sprites")]
    [SerializeField] private Sprite mainHardSprite;
    [SerializeField] private Sprite lightHardSprite;

    [Header("Buttons")]
    public Button btnSetting;
    public Button btnAvatar;
    public Button btnNoAds;
    public Button btnPlay;

    [Header("Avatar")]
    public Image iconAvatar;

    protected override void Init()
    {
        btnSetting.OnClicked(delegate { PopupManager.Show<SettingLobbyBox>().Forget(); });
        btnAvatar.OnClicked(delegate { PopupManager.Show<AvatarBox>().Forget(); });
        btnNoAds.OnClicked(delegate { PopupManager.Show<NoAdsBox>().Forget(); });
    }

    protected override void InitState()
    {
    }

    public void SetAvatar(Sprite sprite) => iconAvatar.sprite = sprite;

    public void RefreshLevels(int currentLevel)
    {
        for (int i = 0; i < levelNodes.Count; i++)
        {
            int level = currentLevel + i;
            bool isHard = level % 3 == 0;

            levelNodes[i].Setup(level, isHard, mainHardSprite, lightHardSprite);
        }
    }
}
