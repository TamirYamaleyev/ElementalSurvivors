using UnityEngine;

public class LevelUpUI : MonoBehaviour
{
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private PlayerEXP expRef;

    void Start()
    {
        if (expRef != null)
            Bind();
    }

    private void Bind()
    {
        expRef.OnLevelUp += HandleLevelUp;
    }

    private void HandleLevelUp(int level)
    {
        Time.timeScale = 0f;
        levelUpPanel.SetActive(true);
    }

    public void ChoiceSelected()
    {
        Time.timeScale = 1f;
        levelUpPanel.SetActive(false);
    }
}
