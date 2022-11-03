using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Dungeon2State : BaseState
{
    public readonly string UI_PREFAB = "UIGame";
    private UIWidget _uiWidget;
    private GameSystems _systems;

    public Dungeon2State(GameSystems systems)
    {
        _systems = systems;
    }

    public override void Transition(State toState, BaseState prevStateClass = null, Dictionary<string, object> options = null)
    {
        // Guard against same state transition
        if (toState == CurrentState)
        {
            return;
        }

        if (toState == State.Active && CurrentState == State.Inactive)
        {
            EventManager.onResetCurrentLevel += ResetState;
            SetupState();
        }
        else if (toState == State.Inactive && CurrentState == State.Active)
        {
            EventManager.onResetCurrentLevel -= ResetState;
            TeardownState();
        }
        else if (toState == State.Background && CurrentState == State.Active)
        {
            EventManager.onResetCurrentLevel -= ResetState;
            if (_uiWidget != null)
            {
                _uiWidget.UIObject.SetActive(false);
            }
        }
        else if (toState == State.Active && CurrentState == State.Background)
        {
            EventManager.onResetCurrentLevel += ResetState;
            if (_uiWidget != null)
            {
                _uiWidget.UIObject.SetActive(true);
            }
        }

        CurrentState = toState;
    }

    public void SetupState()
    {
        _systems.LevelSceneManager.LoadLevel("Dungeon");
        _uiWidget = _systems.UIManager.LoadUI(UI_PREFAB);
        _uiWidget.UIObject.GetComponent<UiController>()?.Setup(_systems);

        // TODO: Grab Game UI script and inject data
        //_uiWidget.UIObject.GetComponent<>();
    }

    private void ResetState()
    {
        EventManager.LoadPlayerInventory();
        TeardownState();
        SetupState();
    }

    public void TeardownState()
    {
        if (_uiWidget != null)
        {
            _systems.UIManager.RemoveUIByGuid(_uiWidget.GUID);
        }
    }
}
