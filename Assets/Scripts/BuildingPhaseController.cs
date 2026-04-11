using Unity.Netcode;
using UnityEngine;

public class BuildingPhaseController : NetworkBehaviour
{
    [SerializeField] private GameObject buildingPhaseBarrier;
    [SerializeField] private BuildPhaseTimer timer;
    [SerializeField] private GameObject buildingBook;

    private bool hasTriggered = false;

    void Update()
    {
        if (timer.phaseEnded && !hasTriggered)
        {
                Debug.Log("ARRIVED");
                hasTriggered = true;
                DestroyObjectsRpc();
        }
    }

    [Rpc(SendTo.Everyone)]
    public void DestroyObjectsRpc()
    {
        if (buildingPhaseBarrier != null) Destroy(buildingPhaseBarrier);
        if (buildingBook != null) Destroy(buildingBook);
    }
}