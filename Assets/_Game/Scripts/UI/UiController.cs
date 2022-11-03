using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiController : MonoBehaviour
{
    public GameObject itemSlotPrefab;
    public TMP_Text inventoryFullText;
    public Image cutsceneImage;
    public GameObject mobileJoystick;

    [SerializeField] private float inventoryFullTextTime;
    [SerializeField] private ItemSlot[] _inventorySlots;
    [SerializeField] private UIHeartSlot[] _heartSlots;
    private GameSystems _systems;

    private void OnEnable()
    {
        EventManager.onInventoryChanged += UpdateInventoryUI;
        EventManager.onPlayerHealthChanged += OnPlayerHealthChanged;
        EventManager.onCutsceneShow += ShowCutSceneImage;
    }
    private void OnDisable()
    {
        EventManager.onInventoryChanged -= UpdateInventoryUI;
        EventManager.onPlayerHealthChanged -= OnPlayerHealthChanged;
        EventManager.onCutsceneShow -= ShowCutSceneImage;
    }

    public void Setup(GameSystems systems)
    {
        _systems = systems;
    }

    private void OnPlayerHealthChanged(int newHealth, int maxHealth)
    {
        int fullHearts = newHealth / 2;
        int maxHearts = maxHealth / 2;
        for (int i = 0; i < _heartSlots.Length; i++)
        {
            if(fullHearts > i)
            {
                _heartSlots[i].SetHeartPortion(2);
            }
            else if(fullHearts == i && fullHearts != maxHearts)
            {
                _heartSlots[i].SetHeartPortion(newHealth % 2);
            }
            else if (maxHearts > i)
            {
                _heartSlots[i].SetHeartPortion(0);
            }
            else
            {
                _heartSlots[i].SetVisibilty(false);
            }

        }
    }

    public void UpdateInventoryUI(InventoryObject _inventory)
    {
        for(int i = 0; i < _inventorySlots.Length; i++)
        {
            if (_inventory.container.Count > i)
            {
                InventorySlot invSlot = _inventory.container[i];
                _inventorySlots[i].SetItem(invSlot.item, invSlot.amount);
            }
            else
            {
                _inventorySlots[i].ClearItem();
            }
        }

        // Display full inventory message
        if(_inventory.container.Count >= _inventory.maxSize)
        {
            StartCoroutine(ShowInventoryFullText());
        }
    }

    IEnumerator ShowInventoryFullText()
    {
        inventoryFullText.gameObject.SetActive(true);
        yield return new WaitForSeconds(inventoryFullTextTime);
        inventoryFullText.gameObject.SetActive(false);
    }

    public void ShowCutSceneImage(bool value)
    {
        cutsceneImage.gameObject.SetActive(value);
    }

    private void Awake()
    {
#if UNITY_ANDROID || UNITY_IOS
        mobileJoystick.SetActive(true);
#else
        mobileJoystick.SetActive(false);
#endif
    }

    public void SettingsAndQuitClicked()
    {
        _systems.StateManager.NavigateToState(typeof(SettingsAndQuitState));
    }

    public void Attack()
    {
        EventManager.Attack();
    }
}
