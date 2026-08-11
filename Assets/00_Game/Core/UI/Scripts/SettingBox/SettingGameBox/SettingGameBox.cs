using UnityEngine.UI;

public class SettingGameBox : SettingBaseBox
{
    public Button btnReturnHome;
    public Button btnRestart;
    public Button btnCheat;

    protected override void OnInit()
    {
        btnReturnHome.OnClicked(() =>
            PopupManager.Show<QuitLevelBox>(box => box.SetMode(QuitLevelBox.Mode.Leave), cover: true).Forget());

        btnRestart.OnClicked(() =>
            PopupManager.Show<QuitLevelBox>(box => box.SetMode(QuitLevelBox.Mode.Restart), cover: true).Forget());

        btnCheat.OnClicked(() =>
            PopupManager.Show<CheatBox>().Forget());
    }
}
