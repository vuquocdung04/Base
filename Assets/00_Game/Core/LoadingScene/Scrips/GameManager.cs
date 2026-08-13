using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private DataRepo dataRepo;
    [SerializeField] private FXManager fxManager;
    [SerializeField] private AudioManager audioManager;
    public HeartManager heartManager;
    public CurrencyManager currencyManager;
    [SerializeField] private LoadingBox loadingBox;
    public ToastManager toastManager;

    public bool isSkipOutPhase;
    public float loadingStepDuration = 1f;
    public float loadingFadeOutDuration = 1f;

    protected override void OnAwake()
    {
        Init().Forget();
    }
    private async Awaitable Init()
    {
        Application.targetFrameRate = 60;
        loadingBox.Init();
        var load50Task = loadingBox.LoadingAsync(0.5f, loadingStepDuration);
        Test();
        
        dataRepo.Init();
        fxManager.Init();
        audioManager.Init(dataRepo);
        toastManager.Init(audioManager);
        heartManager.Init(toastManager);
        currencyManager.Init(toastManager);
        await load50Task;
        await loadingBox.LoadingAsync(1f, loadingStepDuration);
        if (isSkipOutPhase) fxManager.PrepareWipeClosed();
        await loadingBox.CloseAsync(loadingFadeOutDuration);

        //Init final
        fxManager.LoadSceneWithIrisWipe(SceneName.GAME_PLAY, isSkipOutPhase);
    }

    private void Test()
    {
        UseProfile.Heart = 4;
        UseProfile.TimeLastOverHeart = TimeManager.GetCurrentTime();
    }
}