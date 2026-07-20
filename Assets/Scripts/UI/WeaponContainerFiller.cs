using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class WeaponContainerFiller : MonoBehaviour
{
    [SerializeField] private WeaponSystem weaponSystem;
    [SerializeField] private WeaponSlotUI[] slots;

    void OnEnable()
    {
        weaponSystem.OnWeaponsChanged += RefreshWeapons;
    }

    void OnDisable()
    {
        weaponSystem.OnWeaponsChanged -= RefreshWeapons;
    }

    void Start()
    {
        RefreshWeapons();    
    }

    public void RefreshWeapons()
    {
        foreach (var slot in slots)
            slot.Clear();

        int index = 0;

        foreach (var weapon in weaponSystem.Weapons)
        {
            if (weapon.level <= 0)
                continue;

            if (index >= slots.Length)
                break;

            slots[index].SetWeapon(weapon);
            index++;
        }
    }
}
