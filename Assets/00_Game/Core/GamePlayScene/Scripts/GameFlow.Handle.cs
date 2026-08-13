using UnityEngine;

public partial class GameFlow
{
    private void HandleStateEntered(GameState state)
    {
        switch (state)
        {
            case GameState.Intro:

                break;
            case GameState.Playing:

                break;
            case GameState.Paused:

                break;
            case GameState.Win:
                PopupManager.Show<WinBox>().Forget();
                AudioManager.Instance.PlaySfx("sfx-Win");
                break;
            case GameState.Lose:
                PopupManager.Show<LoseBox>().Forget();
                AudioManager.Instance.PlaySfx("sfx-Lose");
                break;
            case GameState.BoosterActive:
                // open booster UI
                break;
            case GameState.Tutorial:
                // show tutorial overlay
                break;
        }
    }

    private void HandleStateExited(GameState state)
    {
        switch (state)
        {
            case GameState.Intro:
                break;
            case GameState.Playing:
                break;
            case GameState.Paused:
                // hide pause UI
                break;
            case GameState.BoosterActive:
                // close booster UI
                break;
            case GameState.Tutorial:
                // hide tutorial overlay
                break;
        }
    }
}