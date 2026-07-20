using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSlotUI : MonoBehaviour
{
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TMP_Text weaponLevel;

    public void SetWeapon(WeaponInstance weapon)
    {
        weaponIcon.gameObject.SetActive(true);
        weaponLevel.gameObject.SetActive(true);

        weaponIcon.sprite = weapon.definition.icon;
        weaponLevel.text = weapon.level.ToString();
    }

    public void Clear()
    {
        weaponIcon.sprite = null;
        weaponLevel.text = "";
        weaponIcon.gameObject.SetActive(false);
        weaponLevel.gameObject.SetActive(false);
    }
}
