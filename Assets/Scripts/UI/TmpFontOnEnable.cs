using TMPro;
using UnityEngine;

/// <summary>
/// Assigns the project default TMP font on enable (edit mode and play mode).
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(TMP_Text))]
public sealed class TmpFontOnEnable : MonoBehaviour
{
    [SerializeField] private TMP_Text target;
    [SerializeField] private bool preserveSharedMaterial;

    private void Reset()
    {
        target = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (target == null)
            target = GetComponent<TMP_Text>();

        TmpFontUtility.EnsureAssigned(target, preserveSharedMaterial);
    }
}
