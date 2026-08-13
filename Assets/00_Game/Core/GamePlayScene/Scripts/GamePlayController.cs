
using EventDispatcher;
using UnityEngine;

public class GamePlayController : Singleton<GamePlayController>
{
    [Header("Testing")]
    [SerializeField] private bool testing;
    [SerializeField] private int currentLevel = 1;

    [Header("Refs")]
    public Camera cameraUI;
    public Camera cameraGameplay;
    public GameScene gameScene;
    public GameFlow gameFlow;
    public GameIntro gameIntro;

    public bool Testing => testing;
    public int CurrentLevel => testing ? currentLevel : UseProfile.Level;

    protected override void OnAwake()
    {
        base.OnAwake();
        InitInstances();
        Init().Forget();
    }

    // Bind toàn bộ Instance trước, để các Init() bên dưới truy cập chéo lẫn nhau được.
    private void InitInstances()
    {
        gameScene.InitInstance();
        gameFlow.InitInstance();
        gameIntro.InitInstance();
    }

    private async Awaitable Init()
    {
        gameFlow.Init();
        gameScene.Init();
        gameIntro.Init();

        //AudioManager.Instance.PlayMusic("Normal Level Music (Cover) 1");

        await gameFlow.RunBootAsync();
    }
}
