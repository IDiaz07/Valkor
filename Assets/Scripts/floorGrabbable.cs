using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(WorldItem))]
public class floorGrabbable : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Distance in meters to pull before triggering.")]
    [SerializeField] private float pullThreshold = 0.4f;

    [Header("UI Appearance")]
    [SerializeField] private Vector3 sliderOffset = new Vector3(0, 0.2f, 0); // Height above item
    [Tooltip("Color transition from start (0%) to finish (100%)")]
    [SerializeField]
    private Gradient progressGradient = new Gradient()
    {
        // Setup default Red -> Green gradient for plug-and-play
        colorKeys = new GradientColorKey[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(Color.green, 1f) },
        alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
    };

    [Header("UI References (Optional)")]
    [Tooltip("Leave empty to auto-generate a slider")]
    [SerializeField] private Slider pullProgressSlider;
    [SerializeField] GameObject drop;

    private XRSimpleInteractable xRSimpleInteractable;
    private float initialDistance = 0;
    private float curDistance = 0;
    private Item item;
    private Transform cameraTransform;
    // Reference to the actual image component that changes color
    private Image fillImage;

    private void Awake()
    {
        xRSimpleInteractable = this.gameObject.GetComponent<XRSimpleInteractable>();
        item = this.GetComponent<WorldItem>().itemData;
        cameraTransform = Camera.main.transform;
        if(drop == null)
        {
            drop = gameObject;
        }

        // 1. Auto-Generate Slider if missing
        if (pullProgressSlider == null)
        {
            GenerateWorldSlider();
        }
        else
        {
            // If user provided a slider, try to find its fill image component
            if (pullProgressSlider.fillRect != null)
                fillImage = pullProgressSlider.fillRect.GetComponent<Image>();
        }

        // 2. Setup Initial State
        if (pullProgressSlider != null)
        {
            pullProgressSlider.gameObject.SetActive(false);
            pullProgressSlider.minValue = 0;
            pullProgressSlider.maxValue = 1;
            pullProgressSlider.interactable = false;
        }
    }
    private void Start()
    {
        pullProgressSlider.transform.parent.localScale = new Vector3(0.00005f, 0.00005f, 0.00005f);
    }

    private void Update()
    {
        if (initialDistance != 0)
        {
            curDistance = GetDistanceToInteractor();

            float movedDistance = curDistance - initialDistance;
            float progress = Mathf.Clamp01(movedDistance / pullThreshold);

            if (pullProgressSlider != null)
            {
                // Update Value
                pullProgressSlider.value = progress;

                // Update Color based on gradient
                if (fillImage != null)
                {
                    fillImage.color = progressGradient.Evaluate(progress);
                }

                // Billboard (Face Camera)
                pullProgressSlider.transform.LookAt(pullProgressSlider.transform.position + cameraTransform.rotation * Vector3.forward, cameraTransform.rotation * Vector3.up);
            }

            if (movedDistance > pullThreshold)
            {
                AddToInventory();
            }
        }
    }

    private void GenerateWorldSlider()
    {
        GameObject canvasObj = new GameObject("AutoCanvas");
        canvasObj.transform.SetParent(this.transform);
        canvasObj.transform.localPosition = sliderOffset;
        canvasObj.transform.localRotation = Quaternion.identity;
        canvasObj.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);


        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        GameObject sliderObj = new GameObject("AutoSlider");
        sliderObj.transform.SetParent(canvasObj.transform);
        sliderObj.transform.localPosition = Vector3.zero;
        sliderObj.transform.localScale = Vector3.one;

        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(200, 20);

        pullProgressSlider = sliderObj.AddComponent<Slider>();

        // Background
        GameObject backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(sliderObj.transform);
        Image bgImage = backgroundObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Dark background
        RectTransform bgRect = backgroundObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector3.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Fill Area
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform);
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector3.one;
        fillAreaRect.offsetMin = new Vector2(2, 2); // Slight padding
        fillAreaRect.offsetMax = new Vector2(-2, -2);

        // The Fill Image itself
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform);
        // --> Capture the reference here so we can change its color later <--
        fillImage = fillObj.AddComponent<Image>();
        // Set initial color to the start of the gradient
        fillImage.color = progressGradient.Evaluate(0f);

        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector3.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        pullProgressSlider.targetGraphic = bgImage;
        pullProgressSlider.fillRect = fillRect;
        pullProgressSlider.direction = Slider.Direction.LeftToRight;
    }

    private void AddToInventory()
    {
        // Hide immediately
        if (pullProgressSlider != null) pullProgressSlider.gameObject.SetActive(false);

        Debug.Log("AddingToInventory");
        Inventory inventory = FindFirstObjectByType<Inventory>();
        if (inventory != null)
        {
            inventory.AddItem(drop, item.description, item.itemID, item.type, item.icon);
        }
        Destroy(this.gameObject);
    }

    public void XrPullMotionDetector()
    {
        initialDistance = GetDistanceToInteractor();
        curDistance = initialDistance;

        if (pullProgressSlider != null)
        {
            pullProgressSlider.gameObject.SetActive(true);
            pullProgressSlider.value = 0;
            // Reset color to start of gradient on grab
            if (fillImage != null) fillImage.color = progressGradient.Evaluate(0f);
        }
    }

    public void XrStopPullMotionDetector()
    {
        initialDistance = 0;
        curDistance = 0;
        if (pullProgressSlider != null) pullProgressSlider.gameObject.SetActive(false);
    }

    private float GetDistanceToInteractor()
    {
        if (xRSimpleInteractable.firstInteractorSelecting == null) return initialDistance;
        return Vector3.Distance(this.transform.position, xRSimpleInteractable.firstInteractorSelecting.transform.position);
    }
}
