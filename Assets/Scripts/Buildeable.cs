using UnityEngine;

public class Buildable : MonoBehaviour
{
    [SerializeField] private int buildId;

    [SerializeField] private SelectedBuildType type;
    public int BuildId { get => buildId; }
    public SelectedBuildType Type { get => type; }
}
