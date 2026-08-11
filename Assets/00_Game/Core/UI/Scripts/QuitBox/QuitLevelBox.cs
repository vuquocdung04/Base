using TMPro;
using UnityEngine.UI;

public class QuitLevelBox : BaseBox, IPopupScale
{
    public enum Mode { Leave, Restart }

    public Button btnLeave;
    public Button btnRestart;
    public TextMeshProUGUI txtTitle;

    private Mode _mode;

    protected override void Init()
    {
    }

    protected override void InitState()
    {
        Refresh();
    }

    public void SetMode(Mode mode) => _mode = mode;

    private void Refresh()
    {
        switch (_mode)
        {
            case Mode.Leave:
                txtTitle.text = "Quit";
                btnLeave.gameObject.SetActive(true);
                btnRestart.gameObject.SetActive(false);
                break;

            case Mode.Restart:
                txtTitle.text = "Restart";
                btnLeave.gameObject.SetActive(false);
                btnRestart.gameObject.SetActive(true);
                break;
        }
    }
}
