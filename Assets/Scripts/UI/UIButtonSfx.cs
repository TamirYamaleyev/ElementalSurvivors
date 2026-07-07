using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSfx : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance.PlayUIClick();
    }
}
