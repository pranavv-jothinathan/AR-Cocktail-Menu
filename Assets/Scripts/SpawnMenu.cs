/*using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Threading.Tasks;

public class SpawnMenu : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager imageManager;
    [SerializeField] private ARAnchorManager anchorManager;
    [SerializeField] private GameObject menuPrefab;
    [SerializeField] ViewMode_Script viewModescript;

    private GameObject spawnedMenu = null;
    private bool isSpawning = false;

    async void Update()
    {
        if (spawnedMenu != null || isSpawning)
            return;

        foreach (ARTrackedImage trackedImage in imageManager.trackables)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                isSpawning = true;

                Pose pose = new Pose(
                    trackedImage.transform.position,
                    Quaternion.Euler(0, trackedImage.transform.eulerAngles.y, 0)
                );

                var result = await anchorManager.TryAddAnchorAsync(pose);

                if (result.status.IsSuccess())
                {
                    ARAnchor anchor = result.value;

                    
                    spawnedMenu = Instantiate(menuPrefab, anchor.transform);

                    Swipe_Script swipe = spawnedMenu.GetComponent<Swipe_Script>();
                    swipe.viewmodescript = viewModescript;
                    viewModescript.swipescript = swipe;


                    spawnedMenu.transform.localPosition = Vector3.zero;
                    spawnedMenu.transform.localRotation = Quaternion.identity;

                    Debug.Log("Menu spawned with anchor");

                    
                    imageManager.enabled = false;
                }
                else
                {
                    Debug.Log("Anchor creation failed");
                }

                break;
            }
        }
    }
}*/