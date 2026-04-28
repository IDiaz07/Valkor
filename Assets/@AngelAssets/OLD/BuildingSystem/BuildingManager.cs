using System.Collections.Generic;
using Unity.Netcode;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingManager : NetworkBehaviour
{
    [Header("Build Objects")]
    [SerializeField] private List<GameObject> floorObjects = new List<GameObject>();
    [SerializeField] private List<GameObject> wallObjects = new List<GameObject>();
    [SerializeField] private List<GameObject> placeableObjects = new List<GameObject>();

    [Header("Build Settings")]
    [SerializeField] private SelectedBuildType currentBuildType;
    [SerializeField] private LayerMask connectorLayer;

    [Header("Destroy Settings")]
    [SerializeField] private bool isDestroying = false;
    private Transform lastHitDestroyTransform;
    private List<Material> lastHitMaterials = new List<Material>();

    [Header("Ghost Settings")]
    [SerializeField] private Material ghostMaterialValid;
    [SerializeField] private Material ghostMaterialInvalid;
    [SerializeField] private float connectorOverlapRadious = 1;
    [SerializeField] private float maxGroundAngle = 45f;

    [Header("Internal State")]
    [SerializeField] private bool isBuilding = false;
    [SerializeField] private bool isCurrentConnected = false;
    [SerializeField] private int currentBuildingIndex;
    private GameObject ghostBuildGameobject;
    private bool isGhostInValidPosition;
    private Transform modelParent = null;
    private Transform raycastObject;
    private PagesController pagesController;

    [Header("Hands")]
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;

    [Header("Input")]
    [SerializeField] private InputActionProperty accept;
    [SerializeField] private InputActionProperty acceptRight;

    [Header("Sounds")]
    [SerializeField] private AudioClip audioClip;

    private GameManager gameManager;
    private bool hasBeenInitialized = false;

    private void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }

    public Transform RaycastObject { get => raycastObject; set => raycastObject = value; }
    public PagesController PagesController { get => pagesController; set => pagesController = value; }

    void Update()
    {
        if (!hasBeenInitialized && gameManager.gameStarting)
        {
            if (leftHand == null || rightHand == null)
            {
                Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
                Camera playerCamera = null;
                foreach (Camera camera in cameras)
                {
                    if (camera.CompareTag("MainCamera"))
                    {
                        playerCamera = camera;
                        break;
                    }

                }
                leftHand = playerCamera.transform.root.transform.GetChild(0).GetChild(3);
                rightHand = playerCamera.transform.root.transform.GetChild(0).GetChild(5);
            }
        }

        if (isBuilding && !isDestroying)
        {
            GhostBuild();

            if (accept.action.WasPressedThisFrame() || acceptRight.action.WasPressedThisFrame())
                PlaceBuild();
        }
        else if (ghostBuildGameobject)
        {
            Destroy(ghostBuildGameobject);
            ghostBuildGameobject = null;
        }

        if (isDestroying)
        {
            GhostDestroy();

            if (accept.action.WasPressedThisFrame() || acceptRight.action.WasPressedThisFrame())
                DestroyBuild();
        }
    }


    private void GhostBuild()
    {
        GameObject currentBuild = GetCurrentBuild();
        CreateGhostPrefab(currentBuild);

        MoveGhostPrefabToRayCast();
        CheckBuildValidity();
    }

    // Te permite activar el modo construcción si está desactivado y viceversa.
    public void ChangeBuildingState()
    {
        isBuilding = !isBuilding;
    }

    // Te permite activar el modo desstrucción si está desactivado y viceversa.
    public void ChangeDestructionState()
    {
        isDestroying = !isDestroying;
    }
    private void CreateGhostPrefab(GameObject currentBuild)
    {
        if (ghostBuildGameobject == null)
        {
            ghostBuildGameobject = Instantiate(currentBuild);

            modelParent = ghostBuildGameobject.transform.GetChild(0);

            GhostifyModel(modelParent, ghostMaterialValid);
            GhostifyModel(ghostBuildGameobject.transform);
        }

    }

    private void MoveGhostPrefabToRayCast()
    {
        RaycastHit hit;
        Transform activeRayOrigin;
        if (TryGetBestRaycastHit(out hit, out activeRayOrigin))
        {
            raycastObject = activeRayOrigin;
            ghostBuildGameobject.transform.position = hit.point;
            if (!isCurrentConnected)
                ghostBuildGameobject.transform.rotation = Quaternion.Euler(ghostBuildGameobject.transform.eulerAngles.x, raycastObject.eulerAngles.z, ghostBuildGameobject.transform.eulerAngles.z);
        }
    }

    private void CheckBuildValidity()
    {
        Collider[] colliders = Physics.OverlapSphere(ghostBuildGameobject.transform.position, connectorOverlapRadious, connectorLayer);
        if (colliders.Length > 0)
        {
            if (currentBuildType != SelectedBuildType.placeableObject)
            {
                GhostConnectBuild(colliders);
            }
            else
            {
                foreach (Collider collider in colliders)
                {
                    if (collider.transform.root.CompareTag("Buildables") && collider.transform.root.gameObject.GetComponent<Buildable>().Type != SelectedBuildType.floor)
                    {
                        Debug.Log("Un objeto/pared impide poner la hoguera");
                        GhostifyModel(modelParent, ghostMaterialInvalid);
                        isGhostInValidPosition = false;
                        return;
                    }
                }
                GhostifyModel(modelParent, ghostMaterialValid);
                isGhostInValidPosition = true;
            }
        }
        else
        {
            GhostSeparateBuild();

            if (isGhostInValidPosition)
            {
                Collider[] overlapColliders = Physics.OverlapBox(ghostBuildGameobject.transform.position, new Vector3(2f, 2f, 2f), ghostBuildGameobject.transform.rotation);
                foreach (Collider overlapCollider in overlapColliders)
                {
                    if (overlapCollider.gameObject != ghostBuildGameobject && overlapCollider.transform.root.CompareTag("Buildables"))
                    {
                        if (currentBuildType == SelectedBuildType.placeableObject)
                        {
                            GhostifyModel(modelParent, ghostMaterialValid);
                            isGhostInValidPosition = true;
                            return;
                        }
                        GhostifyModel(modelParent, ghostMaterialInvalid);
                        isGhostInValidPosition = false;
                        return;

                    }
                }
            }
        }
    }

    private void GhostConnectBuild(Collider[] colliders)
    {
        Connector bestConnector = null;

        foreach (Collider collider in colliders)
        {
            Connector connector = collider.GetComponent<Connector>();

            if (connector.canConnectTo)
            {
                bestConnector = connector;
                break;
            }
        }

        if (bestConnector == null || currentBuildType == SelectedBuildType.floor && bestConnector.isConnectedToFloor || currentBuildType == SelectedBuildType.wall && bestConnector.isConnectedToWall)
        {
            GhostifyModel(modelParent, ghostMaterialInvalid);
            isGhostInValidPosition = false;
            return;
        }

        SnapGhostPrefabToConnector(bestConnector);
    }

    private void SnapGhostPrefabToConnector(Connector connector)
    {
        Transform ghostConnector = FindSnapConnector(connector.transform, ghostBuildGameobject.transform.GetChild(1));
        ghostBuildGameobject.transform.position = connector.transform.position - (ghostConnector.position - ghostBuildGameobject.transform.position);

        if (currentBuildType == SelectedBuildType.wall)
        {
            Quaternion newRotation = ghostBuildGameobject.transform.rotation;
            newRotation.eulerAngles = new Vector3(newRotation.eulerAngles.x, connector.transform.rotation.eulerAngles.y, newRotation.eulerAngles.z);
            ghostBuildGameobject.transform.rotation = newRotation;
        }
        else if (currentBuildType == SelectedBuildType.floor)
        {

            Quaternion newRotation = ghostBuildGameobject.transform.rotation;
            if (connector.transform.root.GetComponent<Buildable>().Type == SelectedBuildType.floor)
            {
                newRotation.eulerAngles = new Vector3(newRotation.eulerAngles.x, connector.transform.root.rotation.eulerAngles.y, newRotation.eulerAngles.z);
            }
            else
            {
                newRotation.eulerAngles = new Vector3(newRotation.eulerAngles.x, connector.transform.rotation.eulerAngles.y, newRotation.eulerAngles.z);
                newRotation *= Quaternion.Euler(Vector3.up * 90);
            }
            ghostBuildGameobject.transform.rotation = newRotation;
        }

        GhostifyModel(modelParent, ghostMaterialValid);
        isGhostInValidPosition = true;
        isCurrentConnected = true;
    }

    private void GhostSeparateBuild()
    {
        isCurrentConnected = false;
        RaycastHit hit;
        Transform activeRayOrigin;
        if (TryGetBestRaycastHit(out hit, out activeRayOrigin))
        {
            raycastObject = activeRayOrigin;
            if (currentBuildType == SelectedBuildType.wall)
            {
                GhostifyModel(modelParent, ghostMaterialInvalid);
                isGhostInValidPosition = false;
                return;
            }

            if (Vector3.Angle(hit.normal, Vector3.up) < maxGroundAngle)
            {
                GhostifyModel(modelParent, ghostMaterialValid);
                isGhostInValidPosition = true;
            }
            else
            {
                GhostifyModel(modelParent, ghostMaterialInvalid);
                isGhostInValidPosition = false;
            }
        }
    }

    private Transform FindSnapConnector(Transform snapConnector, Transform ghostConnectorParent)
    {
        ConnectorPosition oppositeConnectorTag = GetOppositePosition(snapConnector.GetComponent<Connector>());

        foreach (Connector connector in ghostConnectorParent.GetComponentsInChildren<Connector>())
        {
            if (connector.connectorPosition == oppositeConnectorTag)
                return connector.transform;
        }

        return null;
    }

    private ConnectorPosition GetOppositePosition(Connector connector)
    {
        ConnectorPosition position = connector.connectorPosition;

        if (currentBuildType == SelectedBuildType.wall && connector.connectorParentType == SelectedBuildType.floor)
            return ConnectorPosition.bottom;

        if (currentBuildType == SelectedBuildType.floor && connector.connectorParentType == SelectedBuildType.wall && connector.connectorPosition == ConnectorPosition.top)
        {
            if (connector.transform.root.rotation.y == 0)
            {
                return GetConnectorClosestToPlayer(true);
            }
            else
            {
                return GetConnectorClosestToPlayer(false);
            }
        }

        switch (position)
        {
            case ConnectorPosition.left:
                return ConnectorPosition.right;
            case ConnectorPosition.right:
                return ConnectorPosition.left;
            case ConnectorPosition.top:
                return ConnectorPosition.bottom;
            case ConnectorPosition.bottom:
                return ConnectorPosition.top;
            default:
                return ConnectorPosition.bottom;
        }
    }

    private ConnectorPosition GetConnectorClosestToPlayer(bool topBottom)
    {
        Transform cameraTransform = Camera.main.transform;

        if (topBottom)
            return cameraTransform.position.z >= ghostBuildGameobject.transform.position.z ? ConnectorPosition.bottom : ConnectorPosition.top;
        else
            return cameraTransform.position.x >= ghostBuildGameobject.transform.position.x ? ConnectorPosition.left : ConnectorPosition.right;
    }

    private void GhostifyModel(Transform modelParent, Material ghostMaterial = null)
    {
        if (ghostMaterial != null)
        {
            foreach (MeshRenderer meshRenderer in modelParent.GetComponentsInChildren<MeshRenderer>())
            {
                // 1. GET the copy
                Material[] mats = meshRenderer.materials;

                // 2. MODIFY the copy
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = ghostMaterial;
                }

                // 3. SET the copy back to the renderer
                meshRenderer.materials = mats;
            }
        }
        else
        {
            foreach (Collider modelColliders in modelParent.GetComponentsInChildren<Collider>())
            {
                modelColliders.enabled = false;
            }
        }
    }

    private GameObject GetCurrentBuild()
    {
        switch (currentBuildType)
        {
            case SelectedBuildType.floor:
                return floorObjects[currentBuildingIndex];
            case SelectedBuildType.wall:
                return wallObjects[currentBuildingIndex];
            case SelectedBuildType.placeableObject:
                return placeableObjects[currentBuildingIndex];
        }

        return null;
    }

    private void PlaceBuild()
    {
        if (ghostBuildGameobject != null && isGhostInValidPosition)
        {
            // 1. Ask the Server to spawn the real object for everyone
            SpawnBuildServerRpc((int)currentBuildType, currentBuildingIndex, ghostBuildGameobject.transform.position, ghostBuildGameobject.transform.rotation);

            // 2. Play the sound locally for the person building
            AudioSource.PlayClipAtPoint(audioClip, ghostBuildGameobject.transform.position, 2f);

            // 3. Clean up the local ghost
            Destroy(ghostBuildGameobject);
            ghostBuildGameobject = null;

            isBuilding = false;
            isCurrentConnected = false;
            pagesController.RemoveResourcesForCurrentBuild();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void SpawnBuildServerRpc(int buildTypeInt, int buildIndex, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        SelectedBuildType buildType = (SelectedBuildType)buildTypeInt;
        GameObject prefabToSpawn = null;

        // Figure out which prefab the client was holding
        switch (buildType)
        {
            case SelectedBuildType.floor:
                prefabToSpawn = floorObjects[buildIndex];
                break;
            case SelectedBuildType.wall:
                prefabToSpawn = wallObjects[buildIndex];
                break;
            case SelectedBuildType.placeableObject:
                prefabToSpawn = placeableObjects[buildIndex];
                break;
        }

        if (prefabToSpawn != null)
        {
            // The Server instantiates it
            GameObject newBuild = Instantiate(prefabToSpawn, spawnPosition, spawnRotation);

            if (buildType != SelectedBuildType.placeableObject)
            {
                foreach (Connector connector in newBuild.GetComponentsInChildren<Connector>())
                {
                    connector.UpdateConnectors(true);
                }
            }

            // The Server spawns it over the network so ALL players see it
            newBuild.GetComponent<NetworkObject>().Spawn();
        }
    }

    private void GhostDestroy()
    {
        RaycastHit hit;
        Transform activeRayOrigin;
        if (TryGetBestRaycastHit(out hit, out activeRayOrigin))
        {
            raycastObject = activeRayOrigin;
            if (hit.transform.root.CompareTag("Buildables"))
            {
                if (!lastHitDestroyTransform)
                {
                    lastHitDestroyTransform = hit.transform.root;

                    lastHitMaterials.Clear();
                    foreach (MeshRenderer lastHitMeshRenderers in lastHitDestroyTransform.GetComponentsInChildren<MeshRenderer>())
                    {
                        lastHitMaterials.Add(lastHitMeshRenderers.material);
                    }

                    GhostifyModel(lastHitDestroyTransform.GetChild(0), ghostMaterialInvalid);
                }
                else if (hit.transform.root != lastHitDestroyTransform)
                {
                    ResetLostHitDestroyTransform();
                }
            }
            else if (lastHitDestroyTransform)
            {
                ResetLostHitDestroyTransform();
            }
        }
    }

    private bool TryGetBestRaycastHit(out RaycastHit bestHit, out Transform bestOrigin)
    {
        bestHit = default;
        bestOrigin = null;

        bool gotLeft = TryRaycastFromOrigin(leftHand, out RaycastHit leftResult);
        bool gotRight = TryRaycastFromOrigin(rightHand, out RaycastHit rightResult);

        if (gotLeft && gotRight)
        {
            // La mano que apunta más lejos es la que el jugador está usando deliberadamente.
            // Una mano en reposo apunta al suelo cercano (distancia corta) y pierde.
            if (rightResult.distance >= leftResult.distance)
            {
                bestHit = rightResult;
                bestOrigin = rightHand;
            }
            else
            {
                bestHit = leftResult;
                bestOrigin = leftHand;
            }
            return true;
        }

        if (gotRight) { bestHit = rightResult; bestOrigin = rightHand; return true; }
        if (gotLeft) { bestHit = leftResult; bestOrigin = leftHand; return true; }

        if (TryRaycastFromOrigin(raycastObject, out RaycastHit fallbackHit))
        {
            bestHit = fallbackHit;
            bestOrigin = raycastObject;
            return true;
        }

        return false;
    }

    private bool TryRaycastFromOrigin(Transform origin, out RaycastHit hit)
    {
        hit = default;
        if (origin == null)
            return false;

        return Physics.Raycast(origin.position, origin.TransformDirection(Vector3.forward), out hit);
    }

    private void ResetLostHitDestroyTransform()
    {
        int counter = 0;
        foreach (MeshRenderer lastHitMeshRenderers in lastHitDestroyTransform.GetComponentsInChildren<MeshRenderer>())
        {
            lastHitMeshRenderers.material = lastHitMaterials[counter];
            counter++;
        }

        lastHitDestroyTransform = null;
    }

    private void DestroyBuild()
    {
        if (lastHitDestroyTransform)
        {
            // Grab the NetworkObject to tell the server exactly which item to destroy
            NetworkObject netObj = lastHitDestroyTransform.GetComponent<NetworkObject>();

            if (netObj != null)
            {
                DestroyBuildServerRpc(netObj.NetworkObjectId);
            }

            isDestroying = false;
            lastHitDestroyTransform = null;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyBuildServerRpc(ulong networkObjectId)
    {
        // The server looks up the specific object by its ID
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
        {
            // Update the connectors BEFORE destroying so the surrounding building updates
            foreach (Connector connector in netObj.GetComponentsInChildren<Connector>())
            {
                connector.gameObject.SetActive(false);
                connector.UpdateConnectors(true);
            }

            // Despawn removes it from the network and destroys the GameObject for everyone
            netObj.Despawn();
        }
    }

    public void ChangeSelectedBuildType(SelectedBuildType buildType)
    {
        currentBuildType = buildType;
    }
    public void ChargeCurrentBuildIndex(int newIndex)
    {
        currentBuildingIndex = newIndex;
    }
    public Transform GetOppositeHand(Transform hand)
    {
        if (hand == leftHand) return rightHand;
        if (hand == rightHand) return leftHand;
        Debug.Log("GetOppositeHand doesn't work");
        return leftHand;
    }
}

[System.Serializable]
public enum SelectedBuildType
{
    floor,
    wall,
    placeableObject,
}
