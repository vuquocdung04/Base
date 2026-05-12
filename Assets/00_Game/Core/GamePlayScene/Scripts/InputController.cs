using System;
using System.Collections.Generic;
using UnityEngine;


public class InputController : StaffSingleton<InputController>
{
    private Camera cam;
    private BoosterType boosterTypeChoosing;
    private bool canInput = true;
    public override void Init()
    {
        GameFlow.Instance.OnStateEntered += HandleGameStateChanged;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GameFlow.Instance.OnStateEntered -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState newState)
    {
        canInput = !(newState == GameState.Win ||
                     newState == GameState.Lose ||
                     newState == GameState.Paused ||
                     newState == GameState.BoosterActive ||
                     newState == GameState.Tutorial);
    }

    private void Update()
    {
        

    }

}