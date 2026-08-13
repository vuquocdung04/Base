using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameIntro : InitSingleton<GameIntro>
{
    [SerializeField] private GameIntroConfig config;
    [SerializeField] private List<MonoBehaviour> introSteps;
    [SerializeField] private bool playIntro = true;

    public override void Init()
    {
        if (!playIntro) return;

        foreach (var step in introSteps)
            ((IIntroStep)step).Prepare(config);
    }

    public async Awaitable PlayPrev()
    {
        if (!playIntro) return;

        foreach (var step in introSteps)
            await ((IIntroStep)step).Play(config);
    }

    public Awaitable PlayPlaying()
    {
        //Note: doan intro chuyen vao trang thai choi (camera ve vi tri gameplay, UI bay vao...).
        //Note: chua xu ly, tra ve Awaitable da hoan tat de GameFlow chay tiep.
        return AwaitableEx.CompletedAwaitable();
    }

    [Button("Test All Intro")]
    private void TestAllIntro()
    {
        Init();
        PlayPrev().Forget();
    }

    [Button("Auto Collect Intro Steps")]
    private void AutoCollectIntroSteps()
    {
        introSteps = new List<MonoBehaviour>();
        foreach (var mono in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (mono is IIntroStep)
                introSteps.Add(mono);
    }
}
