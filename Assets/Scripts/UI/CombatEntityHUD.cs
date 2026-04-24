using UnityEngine;
using UnityEngine.UI;

public class CombatEntityHUD : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider mpSlider;

    public void Bind(CombatEntity entity)
    {
        if (entity == null)
            return;

        if (hpSlider != null)
        {
            hpSlider.maxValue = entity.MaxHP;
            hpSlider.value = entity.CurrentHP;
        }

        if (mpSlider != null)
        {
            mpSlider.maxValue = entity.MaxMP;
            mpSlider.value = entity.CurrentMP;
        }
    }

    public void Refresh(CombatEntity entity)
    {
        if (entity == null)
            return;

        if (hpSlider != null)
            hpSlider.value = entity.CurrentHP;

        if (mpSlider != null)
            mpSlider.value = entity.CurrentMP;
    }
}
