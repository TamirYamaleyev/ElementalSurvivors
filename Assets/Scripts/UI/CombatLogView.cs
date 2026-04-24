using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatLogView : MonoBehaviour
{
    [SerializeField] private Text logText;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentRect;

    private const int MaxLines = 48;
    private readonly List<string> lines = new List<string>();

    public void Bind(Text text, ScrollRect scroll, RectTransform content)
    {
        logText = text;
        scrollRect = scroll;
        contentRect = content;
    }

    public void Clear()
    {
        lines.Clear();
        if (logText != null)
            logText.text = string.Empty;
        ResizeContent();
    }

    public void Append(string line)
    {
        if (logText == null)
            return;

        lines.Add(line);
        while (lines.Count > MaxLines)
            lines.RemoveAt(0);

        logText.text = string.Join("\n", lines);
        ResizeContent();
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }

    private void ResizeContent()
    {
        if (logText == null || contentRect == null)
            return;

        float h = Mathf.Max(80f, logText.preferredHeight + 16f);
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, h);
    }
}
