using TMPro;
using UnityEngine;

public class IridescentText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float speed = 0.5f;

    private float hue;

    void Update()
    {
        float pulse = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;

        Color gold = Color.Lerp(
            new Color(1f, 0.65f, 0.1f),
            new Color(1f, 0.95f, 0.5f),
            pulse
        );

        text.color = gold;
    }
}
