using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameIntro : InitSingleton<GameIntro>
{
    [SerializeField] private List<IntroStep> introSteps = new();
    [SerializeField] private bool playIntro = true;

    public override void Init()
    {
        if (!playIntro) return;

        for (int i = 0; i < introSteps.Count; i++)
        {
            if (introSteps[i] != null) introSteps[i].Prepare();
        }
    }

    public async Awaitable PlayPrev()
    {
        if (!playIntro) return;

        for (int i = 0; i < introSteps.Count; i++)
        {
            if (introSteps[i] != null) await introSteps[i].Play();
        }
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
        introSteps = new List<IntroStep>(
            FindObjectsByType<IntroStep>(FindObjectsInactive.Include, FindObjectsSortMode.None));
    }
}
