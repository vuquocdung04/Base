using UnityEngine.UI;

public class SettingGameBox : SettingBaseBox
{
    public Button btnRestart;
    public Button btnCheat;

    protected override void OnInit()
    {
        btnRestart.onClick.AddListener(() =>
            PopupManager.Show<QuitLevelBox>(cover: true).Forget());

        btnCheat.onClick.AddListener(() =>
            PopupManager.Show<CheatBox>().Forget());
    }
}
