using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsState : BaseState
{
    public readonly string UI_PREFAB = "UICredits";
    private UIWidget _uiWidget;
    private GameSystems _systems;

    public CreditsState(GameSystems systems)
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
            SetupState();
        }
        else if (toState == State.Inactive && CurrentState == State.Active)
        {
            TeardownState();
        }
        else if (toState == State.Background && CurrentState == State.Active)
        {
            if (_uiWidget != null)
            {
                _uiWidget.UIObject.SetActive(false);
            }
        }
        else if (toState == State.Active && CurrentState == State.Background)
        {
            if (_uiWidget != null)
            {
                _uiWidget.UIObject.SetActive(true);
            }
        }

        CurrentState = toState;
    }

    public void SetupState()
    {
        _uiWidget = _systems.UIManager.LoadUI(UI_PREFAB);
        _uiWidget.UIObject.GetComponent<UICredits>()?.Setup(_systems);
        EventManager.Pause(true);
    }

    public void TeardownState()
    {
        if (_uiWidget != null)
        {
            _systems.UIManager.RemoveUIByGuid(_uiWidget.GUID);
        }
        EventManager.Pause(false);
    }
}
