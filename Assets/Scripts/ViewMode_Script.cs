using UnityEngine;
using DG.Tweening;
using UnityEngine.XR.ARFoundation;

public class ViewMode_Script : MonoBehaviour
{
    public static ViewMode_Script Instance;
    public MonoBehaviour swipingScript;

    public CocktailARManager arManager;
    public GameObject buttonEnter;         
    public GameObject buttonExit;

    private Transform targetGameObject;
    private bool inViewMode = false;

    private Transform initialParent;
    private Vector3 initialLocalPos;
    private Quaternion initialLocalRot;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (buttonEnter != null) buttonEnter.SetActive(false);
        if (buttonExit != null) buttonExit.SetActive(false);
        inViewMode = false;
    }

    void Update()
    {
        if (inViewMode && targetGameObject != null && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                float rotX = touch.deltaPosition.x * 0.5f;
                float rotY = touch.deltaPosition.y * 0.5f;

                targetGameObject.Rotate(Vector3.up, -rotX, Space.World);
                targetGameObject.Rotate(Vector3.right, rotY, Space.World);
            }
        }
    }
    
    public bool GetStateViewMode() 
    {
        return inViewMode;
    }

    public void EnterViewMode()
    {
        arManager = CocktailARManager.Instance;

        if (arManager == null) return;
        if (arManager.IsAnimating) return;

        Transform drink = arManager.GetCurrentDrink();
        if (drink == null) return;

        targetGameObject = drink;
        arManager.ClearActiveEffect();

        TutorialManager.Instance.TemporarilyHideGuidance(true);
        targetGameObject.DOKill(); 
        Rotator rot = targetGameObject.GetComponentInChildren<Rotator>();
        if (rot != null) 
        {
            rot.gameObject.SetActive(false); 
        }

        Animator anim = targetGameObject.GetComponentInChildren<Animator>();
        if (anim != null) anim.enabled = false; 
        if (arManager.baseModel != null) arManager.baseModel.gameObject.SetActive(false);
        if (arManager.clothModel != null) arManager.clothModel.gameObject.SetActive(false);

        initialParent = drink.parent;
        initialLocalPos = drink.localPosition;
        initialLocalRot = drink.localRotation;

        if (swipingScript != null) swipingScript.enabled = false;
        arManager.enabled = false; 
        if (buttonEnter != null) buttonEnter.SetActive(false);
        if (buttonExit != null) buttonExit.SetActive(true);

        drink.SetParent(null);
        inViewMode = true;

        Camera camera = Camera.main;
        if (camera == null) return;

        Transform cameraTransform = camera.transform;

        drink.position = cameraTransform.position + cameraTransform.forward * 0.4f;
        drink.rotation = Quaternion.identity;

        Debug.Log("VIEW MODE ENABLED: " + drink.name);
    }

    public void ExitViewMode()
    {
        if (targetGameObject == null) return;

        targetGameObject.SetParent(initialParent);
        targetGameObject.localPosition = initialLocalPos;
        targetGameObject.localRotation = initialLocalRot;

        TutorialManager.Instance.TemporarilyHideGuidance(false);
        Rotator rot = targetGameObject.GetComponentInChildren<Rotator>(true);
        if (rot != null)
        {
            rot.gameObject.SetActive(true);
            rot.enabled = true;
        }
        arManager.enabled = true;

        if (swipingScript != null) swipingScript.enabled = true;
        Animator anim = targetGameObject.GetComponentInChildren<Animator>();
        if (anim != null) anim.enabled = true;

        if (arManager.baseModel != null) arManager.baseModel.gameObject.SetActive(true);
        if (arManager.clothModel != null) arManager.clothModel.gameObject.SetActive(true);

        if (buttonExit != null) buttonExit.SetActive(false);
        if (buttonEnter != null) buttonEnter.SetActive(true);
        inViewMode = false;

        Debug.Log("VIEW MODE EXITED");
    }

    public void ToggleViewMode()
    {
        if (inViewMode)
            ExitViewMode();
        else
            EnterViewMode();
    }

    public bool IsInViewMode()
    {
        return inViewMode;
    }
}