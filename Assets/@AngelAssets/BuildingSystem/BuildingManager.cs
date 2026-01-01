using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [Header("Build Objects")]
    [SerializeField] private List<GameObject> floorObjects = new List<GameObject>();
    [SerializeField] private List<GameObject> wallObjects = new List<GameObject>();

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
    [SerializeField] private int currentBuildingIndex;
    private GameObject ghostBuildGameobject;
    private bool isGhostInValidPosition;
    private Transform modelParent = null;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
            isBuilding = !isBuilding;

        if (Input.GetKeyDown(KeyCode.V))
            isDestroying = !isDestroying;

        if (isBuilding && !isDestroying)
        {
            GhostBuild();

            if (Input.GetMouseButtonDown(0))
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

            if (Input.GetMouseButtonDown(0))
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
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            ghostBuildGameobject.transform.position = hit.point;
        }
    }

    private void CheckBuildValidity()
    {
        Collider[] colliders = Physics.OverlapSphere(ghostBuildGameobject.transform.position, connectorOverlapRadious, connectorLayer);
        if (colliders.Length > 0)
        {
            GhostConnectBuild(colliders);
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

        GhostifyModel(modelParent, ghostMaterialValid);
        isGhostInValidPosition = true;
    }

    private void GhostSeparateBuild()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (currentBuildType == SelectedBuildType.wall)
            {
                GhostifyModel(modelParent, ghostMaterialInvalid);
                isGhostInValidPosition = false;
                return;
            }

            if (Vector3.Angle(hit.normal, Vector3.up) < maxGroundAngle)
            {
                GhostifyModel(modelParent, ghostMaterialInvalid);
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

            foreach (Connector connector in newBuild.GetComponentsInChildren<Connector>())
            {
                connector.UpdateConnectors(true);
            }
        }
    }

    private void GhostDestroy()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
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
}

[System.Serializable]
public enum SelectedBuildType
{
    floor,
    wall,
}
