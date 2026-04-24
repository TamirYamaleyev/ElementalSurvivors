using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [SerializeField] private BGMType currentFloorType = BGMType.Jazz;

    public BGMType CurrentFloorType => currentFloorType;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetFloorType(BGMType floorType)
    {
        currentFloorType = floorType;
    }
}
