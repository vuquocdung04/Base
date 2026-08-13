
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameScene : InitSingleton<GameScene>
{
    [Header("Panel")]
    public Image darkPanel;
    public Image whitePanel;

    public TextMeshProUGUI txtLevelDisplay;

    [Header("Button")]
    public Button btnSetting;
    public Button btnCoin;

    [Header("Controller")]
    public BoosterController boosterController;

    public override void Init()
    {
        boosterController.InitInstance();

        // btnSetting.OnClicked(delegate
        // {
        //     PopupManager.Show<SettingGameBox>().Forget();
        // });

        // btnCoin.OnClicked(delegate
        // {
        //     PopupManager.Show<ShopBox>().Forget();
        // });
        txtLevelDisplay.text = $"Level {GamePlayController.Instance.CurrentLevel}";

        boosterController.Init();
    }

    public static void EnableDarkPanel(bool state) => SetPanel(Instance.darkPanel, state);

    public static void EnableWhitePanel(bool state) => SetPanel(Instance.whitePanel, state);

    private static void SetPanel(Image panel, bool state)
    {
        if (panel == null) return;

        panel.gameObject.SetActive(state);
    }

    public static Transform GetCoinBar() => Instance.btnCoin.transform;
}
