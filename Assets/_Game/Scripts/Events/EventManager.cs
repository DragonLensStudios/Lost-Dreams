using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager
{
    public delegate void D_Void();
    public delegate void D_Bool(bool value);
    public delegate void D_Int(int value);
    public delegate void D_GameObject(GameObject _value);
    public delegate void D_GameObjecWithtInt(GameObject _value, int _value2);
    public delegate void D_TwoGameObjecWithtIntAndFloat(GameObject _value, GameObject _value1, int _value2, float _value3);
    public delegate void D_ItemWithInventoryWithInt(InventoryObject _inventory, ItemObject _item, int _amount);
    public delegate void D_ItemWithInventory(InventoryObject _inventory, ItemObject _item);
    public delegate void D_Inventory(InventoryObject _inventory);
    public delegate void D_ItemWithGameObject(ItemObject _item, GameObject _obj);
    public delegate void D_WeaponWithGameObject(WeaponObject _weapon, GameObject _obj);
    public delegate void D_FoodWithGameObject(FoodObject _food, GameObject _obj);
    public delegate void D_HeartContainerWithGameObject (HeartContainerObject _container, GameObject _obj);
    public delegate void D_HealthChanged(int newHealth, int maxHealth);

    public static event D_GameObject onObjectDied;
    public static event D_Bool onControlsEnabled;
    public static event D_TwoGameObjecWithtIntAndFloat onDamageActor;
    public static event D_Inventory onInventoryChanged;
    public static event D_ItemWithGameObject onItemUse;
    public static event D_WeaponWithGameObject onWeaponEquip;
    public static event D_FoodWithGameObject onFoodEat;
    public static event D_HeartContainerWithGameObject onHeartContainerIncrease;

    public static event D_HealthChanged onPlayerHealthChanged;
    public static event D_GameObjecWithtInt onHealthAdd;
    public static event D_Bool onCutsceneShow;
    public static event D_Void onSavePlayerInventory;
    public static event D_Void onLoadPlayerInventory;
    public static event D_Void onResetCurrentLevel;
    public static event D_Bool onLoadingTransition;
    public static event D_Void onAttack;
    public static event D_Bool onPause;

    public static void ControlsEnabled(bool value) { onControlsEnabled?.Invoke(value); }
    public static void ItemUse(ItemObject _item, GameObject _obj) { onItemUse?.Invoke(_item, _obj); }
    public static void WeaponEquip(WeaponObject _weapon, GameObject _obj) { onWeaponEquip?.Invoke(_weapon, _obj); }
    public static void FoodEat(FoodObject _food, GameObject _obj) { onFoodEat?.Invoke(_food, _obj); }
    public static void InventoryChanged(InventoryObject _inventory) { onInventoryChanged?.Invoke(_inventory); }
    public static void DamageActor(GameObject _target, GameObject _attacker, int _dmg, float _knockback) { onDamageActor?.Invoke(_target, _attacker, _dmg, _knockback); }
    public static void ObjectDied(GameObject _obj) { onObjectDied?.Invoke(_obj); }
    public static void PlayerHealthChanged(int _newHealth, int _maxHealth) { onPlayerHealthChanged?.Invoke(_newHealth, _maxHealth); }
    public static void HealthAdd(GameObject _obj, int _value) { onHealthAdd?.Invoke(_obj, _value); }
    public static void ShowCutscene(bool _showCutscene) { onCutsceneShow?.Invoke(_showCutscene); }
    public static void SavePlayerInventory() { onSavePlayerInventory?.Invoke(); }
    public static void LoadPlayerInventory() { onLoadPlayerInventory?.Invoke(); }
    public static void ResetCurrentLevel() { onResetCurrentLevel?.Invoke(); }
    public static void IncreaseHeartContainer(HeartContainerObject _heartContainer, GameObject _obj) { onHeartContainerIncrease?.Invoke(_heartContainer, _obj);}
    public static void LoadingTransition(bool showTransition) { onLoadingTransition?.Invoke(showTransition); }
    public static void Attack() { onAttack?.Invoke(); }
    public static void Pause(bool ispaused) { onPause?.Invoke(ispaused); }
}