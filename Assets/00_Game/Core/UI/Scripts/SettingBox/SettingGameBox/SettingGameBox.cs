using UnityEngine.UI;

public class SettingGameBox : SettingBaseBox
{
    public Button btnReturnHome;
    public Button btnRestart;
    public Button btnCheat;

    protected override void OnInit()
    {
        btnReturnHome.onClick.AddListener(() =>
            PopupManager.Show<QuitLevelBox>(box => box.SetMode(QuitLevelBox.Mode.Leave), cover: true).Forget());

        btnRestart.onClick.AddListener(() =>
            PopupManager.Show<QuitLevelBox>(box => box.SetMode(QuitLevelBox.Mode.Restart), cover: true).Forget());

        btnCheat.onClick.AddListener(() =>
            PopupManager.Show<CheatBox>().Forget());
    }
}
