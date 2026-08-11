using UnityEngine;
using UnityEngine.UI;

public class BoosterUnlockBox : BaseBox
{
    public Image imgBooster;
    public Button btnClaim;

    public Transform txtHolder;

    protected override void Init()
    {
    }

    protected override void InitState()
    {
    }

    public void SetBoosterIcon(Sprite sprite) => imgBooster.sprite = sprite;
}
