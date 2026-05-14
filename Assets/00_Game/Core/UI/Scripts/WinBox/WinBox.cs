using UnityEngine;
using UnityEngine.UI;

public class WinBox : BaseBox<WinBox>
{

    public Button btnReward;
    public Button btnDoubleReward;


    protected override void Init()
    {
        btnReward.OnClicked(delegate
        {
            FXManager.Instance.LoadSceneWithIrisWipe(SceneName.GAME_PLAY);
        });

        
    }

    protected override void InitState()
    {
        
    }
}
