using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private DataRepo dataRepo;
    [SerializeField] private FXManager fxManager;
    [SerializeField] private AudioManager audioManager;
    public HeartManager heartManager;
    public RewardManager rewardManager;
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

        dataRepo.Init();
        fxManager.Init();
        audioManager.Init();
        toastManager.Init(audioManager);
        heartManager.Init(toastManager);
        rewardManager.Init();
        RewardBindings.Bind(rewardManager);
        await load50Task;
        await loadingBox.LoadingAsync(1f, loadingStepDuration);
        if (isSkipOutPhase) fxManager.PrepareWipeClosed();
        await loadingBox.CloseAsync(loadingFadeOutDuration);

        //Init final
        fxManager.LoadSceneWithIrisWipe(DevBoot.ResolveStartScene(SceneName.GAME_PLAY), isSkipOutPhase);
    }
}