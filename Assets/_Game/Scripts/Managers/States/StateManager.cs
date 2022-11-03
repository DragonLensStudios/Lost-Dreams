using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager
{
    private Stack<BaseState> _states = new Stack<BaseState>();
    private GameSystems _systems;

    public StateManager(GameSystems systems)
    {
        _systems = systems;
    }

    /// <summary>
    /// Navigates to a given state
    /// </summary>
    /// <param name="stateType">State to push</param>
    /// <param name="popPreviousState">Option to pop current state off the stack</param>
    /// <param name="options">Optional data to be sent to the state</param>
    public void NavigateToState(Type stateType, bool popCurrentState = false, bool fadeTransition = false, Dictionary<string, object> options = null)
    {
        _systems.StartCoroutine(NavigateToState_Implementation(stateType, popCurrentState, fadeTransition, options));
    }
    private IEnumerator NavigateToState_Implementation(Type stateType, bool popCurrentState = false, bool fadeTransition = false, Dictionary<string, object> options = null) { 
        if (fadeTransition)
        {
            EventManager.LoadingTransition(true);
            yield return new WaitForSeconds(UILoadingTransition.TRANSITION_TIME);
        }

        // If true, pop the current state
        if (popCurrentState)
        {
            PopState();
        }

        // Setup and push new state based on type
        if(stateType == null)
        {
            // No-OP, popCurrentState should be true in this case
        }
        else if(stateType.Equals(typeof(MainMenuState)))
        {
            PushState(new MainMenuState(_systems), options);
        }
        else if(stateType.Equals(typeof(Forest1State)))
        {
            PushState(new Forest1State(_systems), options);
        }
        else if(stateType.Equals(typeof(Forest2State)))
        {
            PushState(new Forest2State(_systems), options);
        }
        else if(stateType.Equals(typeof(GhostForest1State)))
        {
            PushState(new GhostForest1State(_systems), options);
        }
        else if (stateType.Equals(typeof(GhostForest2State)))
        {
            PushState(new GhostForest2State(_systems), options);
        }
        else if (stateType.Equals(typeof(Dungeon1State)))
        {
            PushState(new Dungeon1State(_systems), options);
        }
        else if (stateType.Equals(typeof(Dungeon2State)))
        {
            PushState(new Dungeon2State(_systems), options);
        }
        else if(stateType.Equals(typeof(TavernState)))
        {
            PushState(new TavernState(_systems), options);
        }
        else if (stateType.Equals(typeof(Apartment1State)))
        {
            PushState(new Apartment1State(_systems), options);
        }
        else if (stateType.Equals(typeof(Apartment2State)))
        {
            PushState(new Apartment2State(_systems), options);
        }
        else if (stateType.Equals(typeof(Apartment3State)))
        {
            PushState(new Apartment3State(_systems), options);
        }
        else if(stateType.Equals(typeof(SettingsState)))
        {
            PushState(new SettingsState(_systems), options);
        }
        else if (stateType.Equals(typeof(SettingsAndQuitState)))
        {
            PushState(new SettingsAndQuitState(_systems), options);
        }
        else if(stateType.Equals(typeof(CreditsState)))
        {
            PushState(new CreditsState(_systems), options);
        }
        else if(stateType.Equals(typeof(GameOverState)))
        {
            PushState(new GameOverState(_systems), options);
        }
        else
        {
            Debug.LogWarningFormat("Unsupported State Type: {0}", stateType.ToString());
        }
    }

    /// <summary>
    /// Pushes a new state onto the stack
    /// </summary>
    /// <param name="state">State to push</param>
    /// <param name="options">Optional data for the state</param>
    private void PushState(BaseState state, Dictionary<string, object> options)
    {
        // Grab previous state if one exists
        BaseState prevState = _states.Count == 0 ? null : _states.Peek();

        // Add new state to the stack
        _states.Push(state);

        // Transition previous state to background if one exists
        if(prevState != null)
        {
            prevState.Transition(State.Background);
        }

        // Transition new state to active
        state.Transition(State.Active, prevState, options);
    }

    /// <summary>
    /// Pops the latest state off of the stack
    /// </summary>
    private void PopState()
    {
        BaseState popState = _states.Count == 0 ? null : _states.Pop();

        if (popState != null)
        {
            // Transition state to inactive for cleanup
            popState.Transition(State.Inactive);

            // Grab next state on the stack if one exists
            BaseState topState = _states.Count == 0 ? null : _states.Peek();

            if (topState != null)
            {
                topState.Transition(State.Active, popState);
            }
        }
    }
}
