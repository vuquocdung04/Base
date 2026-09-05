using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class FXManager
{
    [SerializeField] private List<SceneTransition> transitions;
    [SerializeField] private SceneTransitionType defaultTransition = SceneTransitionType.Iris;
    public float transitionDurationOut = 1f;
    public float transitionDurationIn = 1f;

    private SceneTransition current;
    private bool isNextSceneReady;
    private bool isTransitioning;

    public bool IsTransitioning => isTransitioning;

    public void NotifySceneReady()
    {
        if (!isTransitioning) return;

        isNextSceneReady = true;
    }

    public async Awaitable NotifySceneReadyAsync()
    {
        NotifySceneReady();

        if (!isTransitioning) return;

        await AwaitableEx.WaitUntil(() => !isTransitioning, destroyCancellationToken);
    }

    public void LoadSceneWithIrisWipe(string sceneName, bool skipOutPhase = false)
        => LoadScene(sceneName, defaultTransition, skipOutPhase);

    public void LoadScene(string sceneName, SceneTransitionType type, bool skipOutPhase = false)
    {
        current = Resolve(type);
        TransitionAsync(sceneName, skipOutPhase).Forget();
    }

    private async Awaitable TransitionAsync(string sceneName, bool skipOutPhase)
    {
        isNextSceneReady = false;
        isTransitioning = true;

        current.Prepare();

        if (skipOutPhase)
            current.SetCovered();
        else
            await current.Cover(transitionDurationOut);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
            await Awaitable.NextFrameAsync();

        current.SetCovered();

        await AwaitableEx.WaitUntil(() => isNextSceneReady);

        await current.Reveal(transitionDurationIn);

        current.Hide();

        isTransitioning = false;
    }

    public void PrepareWipeClosed()
    {
        current = Resolve(defaultTransition);
        current.Prepare();
        current.SetCovered();
    }

    private void HideAllTransitions()
    {
        foreach (SceneTransition t in transitions) t.Hide();
    }

    private SceneTransition Resolve(SceneTransitionType type)
    {
        foreach (SceneTransition t in transitions)
            if (t.Type == type) return t;
        return transitions[0];
    }
}
