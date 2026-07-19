using UnityEngine;

public class PanelToggle : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    public void ToggleActive()
    {
        panel.SetActive(!panel.activeSelf);
    }
}
