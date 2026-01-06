using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingManager : MonoBehaviour
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

    [Header("Sounds")]
    [SerializeField] private AudioClip audioClip;

    private void Awake()
    {
        leftHand = FindAnyObjectByType<XROrigin>().transform.GetChild(0).GetChild(3);
        rightHand = FindAnyObjectByType<XROrigin>().transform.GetChild(0).GetChild(5);
    }

    public Transform RaycastObject { get => raycastObject; set => raycastObject = value; }
    public PagesController PagesController { get => pagesController; set => pagesController = value; }

    void Update()
    {

        if (isBuilding && !isDestroying)
        {
            GhostBuild();

            if (accept.action.WasPressedThisFrame())
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

            if (accept.action.WasPressedThisFrame())
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
        if (Physics.Raycast(raycastObject.position, raycastObject.TransformDirection(Vector3.forward), out hit))
        {
            ghostBuildGameobject.transform.position = hit.point;
            if(!isCurrentConnected)
            ghostBuildGameobject.transform.rotation = Quaternion.Euler(ghostBuildGameobject.transform.eulerAngles.x,raycastObject.eulerAngles.z, ghostBuildGameobject.transform.eulerAngles.z);
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
        }else if (currentBuildType == SelectedBuildType.floor)
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
        if (Physics.Raycast(raycastObject.position, raycastObject.TransformDirection(Vector3.forward), out hit))
        {
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
                meshRenderer.material = ghostMaterial;
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
            GameObject newBuild = Instantiate(GetCurrentBuild(), ghostBuildGameobject.transform.position, ghostBuildGameobject.transform.rotation);

            Destroy(ghostBuildGameobject);
            ghostBuildGameobject = null;

            isBuilding = false;
            if (currentBuildType != SelectedBuildType.placeableObject)
            {
                foreach (Connector connector in newBuild.GetComponentsInChildren<Connector>())
                {
                    connector.UpdateConnectors(true);
                }
            }
            isCurrentConnected = false;
            pagesController.RemoveResourcesForCurrentBuild();
            AudioSource.PlayClipAtPoint(audioClip, newBuild.transform.position, 2f);
        }
    }

    private void GhostDestroy()
    {
        RaycastHit hit;
        if (Physics.Raycast(raycastObject.position, raycastObject.TransformDirection(Vector3.forward), out hit))
        {
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
            foreach (Connector connector in lastHitDestroyTransform.GetComponentsInChildren<Connector>())
            {
                connector.gameObject.SetActive(false);
                connector.UpdateConnectors(true);
            }

            Destroy(lastHitDestroyTransform.gameObject);

            isDestroying = false;
            lastHitDestroyTransform = null;
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
