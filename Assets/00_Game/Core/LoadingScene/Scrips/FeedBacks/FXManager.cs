using UnityEngine;
public partial class FXManager : MonoBehaviour
{
    public static FXManager Instance {get; private set;}
    public void Init()
    {
        Instance = this;
        
        if(wipeCanvas.gameObject.activeInHierarchy) wipeCanvas.gameObject.SetActive(false);
    }
    
}