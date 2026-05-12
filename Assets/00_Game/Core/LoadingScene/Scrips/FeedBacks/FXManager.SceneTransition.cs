using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public partial class FXManager
{
    public Canvas wipeCanvas;
    public float transitionDurationOut = 1f;
    public float transitionDurationIn = 1f;
    [HideInInspector] public bool isNextSceneReady;

    private Material cachedWipeMat;

    private Material WipeMat
    {
        get
        {
            if (cachedWipeMat == null)
                cachedWipeMat = wipeCanvas.GetComponentInChildren<RawImage>().material;
            return cachedWipeMat;
        }
    }

    public void LoadSceneWithIrisWipe(string sceneName, bool skipOutPhase = false)
    {
        IrisWipeAsync(sceneName, skipOutPhase).Forget();
    }

    private async UniTaskVoid IrisWipeAsync(string sceneName, bool skipOutPhase)
    {
        isNextSceneReady = false;

        SetupCanvasCamera();

        if (skipOutPhase)
        {
            SetWipeState(1f, 0f);
        }
        else
        {
            SetWipeState(0f, 0f);
            await WipeMat.DOFloat(1.2f, "_Radius", transitionDurationOut).ToUniTask();
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            await UniTask.Yield();
        }

        // Scene mới đã load xong -> Cập nhật lại camera mới cho Canvas
        SetupCanvasCamera();
        SetWipeState(1f, 0f);

        await UniTask.WaitUntil(() => isNextSceneReady);
        // Chạy hiệu ứng mở ra
        await WipeMat.DOFloat(1.2f, "_Radius", transitionDurationIn).ToUniTask();

        Debug.Log("Completed Transition");
        wipeCanvas.gameObject.SetActive(false);
    }

    public void PrepareWipeClosed()
    {
        SetupCanvasCamera();
        SetWipeState(1f, 0f);
    }

    private void SetupCanvasCamera()
    {
        if (!wipeCanvas.gameObject.activeSelf)
        {
            wipeCanvas.gameObject.SetActive(true);
        }
        GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");
        if (camObj != null)
        {
            wipeCanvas.worldCamera = camObj.GetComponent<Camera>();
        }
    }

    private void SetWipeState(float isInvert, float radius)
    {
        WipeMat.SetFloat("_IsInvert", isInvert);
        WipeMat.SetFloat("_Radius", radius);
    }
}