using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ARImageSmoothFollow : MonoBehaviour
{
    [SerializeField] private GameObject menuRootPrefab;
    [SerializeField] private bool hideWhenTrackingLost = false;
    [SerializeField] private string targetImageName = "";

    private ARTrackedImageManager imageManager;
    private GameObject menuRootInstance;
    private ARTrackedImage activeTrackedImage;

    private void Awake()
    {
        imageManager = GetComponent<ARTrackedImageManager>();

        if (menuRootPrefab == null)
        {
            Debug.LogError("MenuRoot Prefab is missing.");
            enabled = false;
            return;
        }

        menuRootInstance = Instantiate(menuRootPrefab);
        menuRootInstance.SetActive(false);
    }

    private void OnEnable()
    {
        if (imageManager != null)
            imageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    private void OnDisable()
    {
        if (imageManager != null)
            imageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }

    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        foreach (var trackedImage in args.added)
            UpdateMenuRootBinding(trackedImage);

        foreach (var trackedImage in args.updated)
            UpdateMenuRootBinding(trackedImage);
    }

    private void UpdateMenuRootBinding(ARTrackedImage trackedImage)
    {
        if (menuRootInstance == null)
            return;

        if (!string.IsNullOrEmpty(targetImageName) &&
            trackedImage.referenceImage.name != targetImageName)
        {
            return;
        }

        if (activeTrackedImage != null && activeTrackedImage != trackedImage)
            return;

        activeTrackedImage = trackedImage;

        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            if (menuRootInstance.transform.parent != trackedImage.transform)
            {
                menuRootInstance.transform.SetParent(trackedImage.transform, false);
                menuRootInstance.transform.localPosition = Vector3.zero;
                menuRootInstance.transform.localRotation = Quaternion.identity;
            }

            if (!menuRootInstance.activeSelf)
                menuRootInstance.SetActive(true);
        }
        else if (hideWhenTrackingLost)
        {
            menuRootInstance.SetActive(false);
        }
    }
}