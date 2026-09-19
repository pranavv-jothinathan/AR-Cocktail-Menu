using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using TMPro;

public class ComprehensivePressHandler : MonoBehaviour
{
    [SerializeField] private Camera arCamera;
    [Header("Press Detect")]
    [SerializeField] private float longPressDuration = 1.0f;
    [SerializeField] private float shortClickDuration = 0.25f;
    [SerializeField] private float movementThreshold = 30f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private string glassTag = "CocktailGlass";

    [Header("Flavour Canvas")] // 长按
    [SerializeField] private GameObject infoCanvas;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;

    [Header("Liquid Canvas")] // 短按
    [SerializeField] private GameObject liquidInfoCanvas;
    [SerializeField] private TextMeshProUGUI liquidNameText;
    [SerializeField] private TextMeshProUGUI liquidDescText;
    [SerializeField] private TextMeshProUGUI liquidTopNameText;
    [SerializeField] private TextMeshProUGUI liquidTopDescText;
 
    [Header("Auto Hide")]
    [SerializeField] private float autoHideDelay = 3f;
    [SerializeField] private bool hideOnTouchEnd = true;

    // 私有变量

    private Vector2 touchStartPos;
    private float touchStartTime;
    private bool isTouching = false;
    private Coroutine hideCoroutine;
    private Coroutine liquidHideCoroutine;

    // 记录最后被触摸的物体
    private GameObject lastTouchedObject;

    void Start()
    {
        arCamera = GetComponent<Camera>();
        if (arCamera == null) arCamera = GetComponentInChildren<Camera>();
        if (arCamera == null) Debug.LogError("未找到AR相机，请确保脚本挂载在带有Camera的物体上");
        if (infoCanvas != null) infoCanvas.SetActive(false);
        if (liquidInfoCanvas != null) liquidInfoCanvas.SetActive(false);
    }
    void Update() 
    {
        // 如果当前处于viewmode，直接返回，不处理任何触摸逻辑
        if (ViewMode_Script.Instance != null && ViewMode_Script.Instance.GetStateViewMode()) 
        {
            return; 
        }
        HandleTouch();
    }

    private void HandleTouch()
    {
#if UNITY_EDITOR //用于PC模拟
        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Input.mousePosition;
            HandleTouchBegan(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            // 判断长按
            if (isTouching && Time.time - touchStartTime >= longPressDuration && Vector2.Distance(Input.mousePosition, touchStartPos) <= movementThreshold)
            {
                TriggerLongPress();
                isTouching = false;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            float touchDuration = Time.time - touchStartTime;
                Vector2 swipeDelta = (Vector2)Input.mousePosition - touchStartPos;

                if (isTouching && touchDuration <= shortClickDuration && swipeDelta.magnitude < movementThreshold) {
                    TriggerShortClick();
                }
                HandleTouchEnded();
        }
#endif

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPos = touch.position;
                    HandleTouchBegan(touch.position);
                    break;
                case TouchPhase.Stationary:
                case TouchPhase.Moved:
                    // 判断长按
                    if (isTouching && Time.time - touchStartTime >= longPressDuration && Vector2.Distance(touch.position, touchStartPos) <= movementThreshold)
                    {
                        TriggerLongPress();
                        isTouching = false; 
                    }
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    // 判断短按
                    float touchDuration = Time.time - touchStartTime;
                        Vector2 swipeDelta = touch.position - touchStartPos;

                        if (isTouching && touchDuration <= shortClickDuration && swipeDelta.magnitude < movementThreshold)
                        {
                            TriggerShortClick();
                        }
                        HandleTouchEnded();
                    break;
            }
        }
    }

    private void HandleTouchBegan(Vector2 screenPos)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPos);
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, interactableLayer);

        if (hits.Length > 0)
        {
            GameObject fallbackGlass = null;
            lastTouchedObject = null;

            // 检查是否点中液体层或酒杯外壳
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.GetComponent<LiquidLayerData>() != null) {
                    lastTouchedObject = hit.collider.gameObject;
                    break; 
                }
                if (!string.IsNullOrEmpty(glassTag) && hit.collider.CompareTag(glassTag)) {
                    fallbackGlass = hit.collider.gameObject;
                }
            }

            if (lastTouchedObject == null && fallbackGlass != null) {
                lastTouchedObject = fallbackGlass;
            }

            if (lastTouchedObject != null)
            {
                isTouching = true;
                touchStartTime = Time.time;
            }
        }
    }

    private void HandleTouchEnded()
    {
        if (hideOnTouchEnd) HideCanvas(infoCanvas, ref hideCoroutine);
        isTouching = false;
    }

    private void TriggerLongPress() // 定义长按显示风味雷达图
    {
        if (infoCanvas != null && lastTouchedObject != null)
        {
            // 震动反馈
#if !UNITY_EDITOR
        Handheld.Vibrate();
#endif
            FlavorData data = lastTouchedObject.GetComponentInParent<FlavorData>();
            RadarChartGrowth growthEffect = infoCanvas.GetComponentInChildren<RadarChartGrowth>();

            if (data != null && growthEffect != null)
            {
                if (nameText != null) nameText.text = data.cocktailName;
                if (descText != null) descText.text = data.cocktailDescription;
                // 在显示长按画布前，移除tutorial
                TutorialManager.Instance.CompleteTutorial();
                // 4.25变更： 在显示长按画布前，强行打断并隐藏短按画布
                HideCanvas(liquidInfoCanvas, ref liquidHideCoroutine);

                ShowCanvas(infoCanvas, ref hideCoroutine);
                growthEffect.AnimateRadarChart(data.GetValuesArray());
            }
        }
    }

    private void TriggerShortClick() // 定义短按显示液体名称和介绍
    {
        if (lastTouchedObject != null)
        {
            LiquidLayerData layerData = lastTouchedObject.GetComponent<LiquidLayerData>();
            if (layerData != null)
            {
                // 在显示短按画布前，移除tutorial
                TutorialManager.Instance.CompleteTutorial();
                // 4.25变更：在显示短按画布前，强行打断并隐藏长按画布
                HideCanvas(infoCanvas, ref hideCoroutine);
                
                if (liquidNameText != null) liquidNameText.text = layerData.layerName;
                if (liquidDescText != null) liquidDescText.text = layerData.layerDescription;
                
                {
                    bool hasTopGarnish = !string.IsNullOrEmpty(layerData.topLayerName);
                    
                    if (liquidTopNameText != null) 
                    { 
                        liquidTopNameText.gameObject.SetActive(hasTopGarnish);
                        if (hasTopGarnish)
                        {
                            liquidTopNameText.text = layerData.topLayerName; 
                            liquidTopNameText.color = layerData.topLayerTitleColor;
                        }
                    }

                    if (liquidTopDescText != null)
                    {
                        liquidTopDescText.gameObject.SetActive(hasTopGarnish);
                        if (hasTopGarnish)
                        {
                            liquidTopDescText.text = layerData.topLayerDescription;
                        }
                    }   
                }
                ShowCanvas(liquidInfoCanvas, ref liquidHideCoroutine);
            }
            else HideCanvas(liquidInfoCanvas, ref liquidHideCoroutine);
        }
    }

    private void ShowCanvas(GameObject canvas, ref Coroutine coroutine)
    {
        if (canvas == null) return;
        canvas.SetActive(true);

        if (coroutine != null) StopCoroutine(coroutine);
        if (autoHideDelay > 0) coroutine = StartCoroutine(HideAfterDelay(canvas, autoHideDelay, coroutine));
    }

    private void HideCanvas(GameObject canvas, ref Coroutine coroutine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        if (canvas != null) canvas.SetActive(false);
    }

    private IEnumerator HideAfterDelay(GameObject canvas, float delay, Coroutine coroutine)
    {
        yield return new WaitForSeconds(delay);
        if (canvas != null) canvas.SetActive(false);
        coroutine = null;
    }


    void OnDestroy()
    {
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        if (liquidHideCoroutine != null) StopCoroutine(liquidHideCoroutine);
    }
}