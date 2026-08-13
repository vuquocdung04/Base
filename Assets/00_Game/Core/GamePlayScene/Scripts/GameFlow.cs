using System;
using EventDispatcher;
using Sirenix.OdinInspector;
using UnityEngine;

public enum GameState
{
    Intro = 0,
    Tutorial = 1,
    Playing = 2,
    Paused = 3,
    BoosterActive = 4,
    Win = 5,
    Lose = 6
}

public partial class GameFlow : InitSingleton<GameFlow>
{
    public GameState CurrentState { get; private set; }
    public event Action<GameState> OnStateEntered;
    public event Action<GameState> OnStateExited;

    private int pauseRequest;

    public override void Init()
    {
        OnStateEntered += HandleStateEntered;
        OnStateExited += HandleStateExited;

        this.RegisterListener(EventID.LEVEL_COMPLETE, OnLevelComplete);
        this.RegisterListener(EventID.POPUP_OPENED, OnPopupOpened);
        this.RegisterListener(EventID.POPUP_CLOSED, OnPopupClosed);
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        OnStateEntered -= HandleStateEntered;
        OnStateExited -= HandleStateExited;

        this.RemoveListener(EventID.LEVEL_COMPLETE, OnLevelComplete);
        this.RemoveListener(EventID.POPUP_OPENED, OnPopupOpened);
        this.RemoveListener(EventID.POPUP_CLOSED, OnPopupClosed);
    }

    public async Awaitable RunBootAsync()
    {
        EnterInitial(GameState.Intro);

        await Awaitable.EndOfFrameAsync(destroyCancellationToken);

        FXManager fxManager = FXManager.Instance;
        if (fxManager != null)
        {
            fxManager.isNextSceneReady = true;
            await Awaitable.WaitForSecondsAsync(fxManager.transitionDurationIn, destroyCancellationToken);
        }

        await GameIntro.Instance.PlayPrev();
        await GameIntro.Instance.PlayPlaying();

        await CheckTutorial();

        ChangeState(GameState.Playing);
    }

    private async Awaitable CheckTutorial()
    {
        //Note: hien moi co tutorial booster. Sau nay gom them tutorial level/obstacle/feature vao day,
        //Note: chay lan luot va cho xong het roi moi ChangeState(Playing).
        if (!BoosterController.Instance.HasTutorial) return;

        ChangeState(GameState.Tutorial);

        await BoosterController.Instance.ShowTutorialAsync();
    }

    private void EnterInitial(GameState state)
    {
        CurrentState = state;
        OnStateEntered?.Invoke(state);
    }

    public bool ChangeState(GameState next)
    {
        if (CurrentState == next) return false;
        if (!CanTransition(CurrentState, next)) return false;

        var prev = CurrentState;
        OnStateExited?.Invoke(prev);
        CurrentState = next;
        OnStateEntered?.Invoke(next);
        return true;
    }
    private bool CanTransition(GameState from, GameState to)
    {
        switch (from)
        {
            case GameState.Intro:
                return to == GameState.Tutorial || to == GameState.Playing;

            case GameState.Playing: return true;

            case GameState.Paused:
                return to == GameState.Playing ||
                       to == GameState.Win ||
                       to == GameState.Lose ||
                       to == GameState.BoosterActive ||
                       to == GameState.Tutorial;

            case GameState.BoosterActive:
                return to == GameState.Playing || to == GameState.Lose;

            case GameState.Tutorial:
                return to == GameState.Playing || to == GameState.Lose;

            case GameState.Win: return false;
            case GameState.Lose: return false;
        }
        return false;
    }
    private void OnPopupOpened(object _) => RequestPause();
    private void OnPopupClosed(object _) => RequestResume();
    public void RequestPause()
    {
        pauseRequest++;
        if (pauseRequest == 1 && CurrentState == GameState.Playing) ChangeState(GameState.Paused);
    }

    public void RequestResume()
    {
        pauseRequest = Mathf.Max(0, pauseRequest - 1);
        if (pauseRequest == 0 && CurrentState == GameState.Paused)
            ChangeState(GameState.Playing);
    }

    void OnLevelComplete(object _) => ChangeState(GameState.Win);
    public void TriggerLose() => ChangeState(GameState.Lose);
    public void EnterBooster() => ChangeState(GameState.BoosterActive);
    public void ExitBooster() => ChangeState(GameState.Playing);
    public void EnterTutorial() => ChangeState(GameState.Tutorial);
    public void ExitTutorial() => ChangeState(GameState.Playing);

}