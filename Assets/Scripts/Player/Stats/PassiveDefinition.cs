using UnityEngine;

[CreateAssetMenu(fileName = "PassiveDefinition", menuName = "Scriptable Objects/PassiveDefinition")]
public class PassiveDefinition : ScriptableObject
{
    public string passiveName;

    public Sprite icon;

    [TextArea]
    public string description;


    public PlayerStatType type;

    [Header("Upgrade Roll")]
    public float minMultiplier = 1.1f;
    public float maxMultiplier = 1.2f;


    public int maxLevel = 5;
}
