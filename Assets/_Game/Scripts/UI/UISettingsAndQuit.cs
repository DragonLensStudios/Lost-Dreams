using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISettingsAndQuit : MonoBehaviour
{
    private GameSystems _systems;

    [SerializeField] private GameObject _quitButton;

    public void Setup(GameSystems systems)
    {
        _systems = systems;

#if UNITY_WEBGL
        _quitButton?.SetActive(false);
#endif
    }

    public void OnSettingsClicked()
    {
        _systems.StateManager.NavigateToState(typeof(SettingsState), true);
    }

    public void OnBackClicked()
    {
        _systems.StateManager.NavigateToState(null, true);
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
