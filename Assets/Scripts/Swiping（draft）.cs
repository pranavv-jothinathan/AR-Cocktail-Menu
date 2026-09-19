using UnityEngine;

public class Swiping : MonoBehaviour 
{
    // 改为私有，不再需要在面板拖拽，稍后会用 CocktailARManager.Instance 自动寻找
    private CocktailARManager arManager;

    // 1. 【注释掉】查看模式的变量声明
    // public ViewMode_Script viewmodescript;

    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    
    // 删掉固定的 50f，改为在 Start 里动态计算
    private float swipeThreshold;

    void Start()
    {
        Debug.Log("Swipe Script Started - 等待滑动输入");
        
        // 动态计算滑动阈值：屏幕宽度的 10%，完美适配电脑 Simulator 和各种手机
        swipeThreshold = Screen.width * 0.1f; 
    }

    void Update()
    {
        // 2. 【注释掉】查看模式的拦截逻辑
        // if (viewmodescript != null && viewmodescript.IsInViewMode())
        //     return;

        // 设置一个标记，记录这一帧是否已经处理过滑动抬起，防止电脑端双重触发
        bool isSwipedThisFrame = false;

        // ==========================================
        // 3. 增加 Simulator (Unity Editor) 鼠标滑动模拟
        // ==========================================
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            endTouchPosition = Input.mousePosition;
            isSwipedThisFrame = true; // 鼠标触发了滑动
        }
#endif

        // ==========================================
        // 4. 手机端物理触摸逻辑 
        // (!isSwipedThisFrame 确保不会和上面的鼠标逻辑重叠打架)
        // ==========================================
        if (!isSwipedThisFrame && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                startTouchPosition = touch.position;
            }

            // 兼容 Ended 和 Canceled（滑到手机屏幕边缘时也会触发）
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                endTouchPosition = touch.position;
                isSwipedThisFrame = true; // 触摸触发了滑动
            }
        }

        // 当这一帧成功抬起手指/鼠标时，执行处理逻辑
        if (isSwipedThisFrame)
        {
            ProcessSwipe();
        }
    }

    // 将判断滑动的核心逻辑提取出来
    private void ProcessSwipe()
    {
        float swipeDistance = endTouchPosition.x - startTouchPosition.x;

        // 判断滑动距离是否超过阈值
        if (Mathf.Abs(swipeDistance) > swipeThreshold)
        {
            // 核心修复：直接通过单例找到 AR 召唤出来的那个“真模型”
            arManager = CocktailARManager.Instance;

            if (arManager == null)
            {
                Debug.LogWarning("⚠️ 场景中没找到 CocktailARManager，请确保模型已出现！");
                return;
            }

            // 动画防卡死拦截
            if (arManager.IsAnimating)
            {
                Debug.LogWarning("⏳ 正在播放切换动画，滑动指令被忽略");
                return;
            }

            // 执行切换
            if (swipeDistance > 0)
            {
                Debug.Log("👉 向右滑动：切换上一杯");
                arManager.SwitchToPreviousDrink();
            }
            else
            {
                Debug.Log("👈 向左滑动：切换下一杯");
                arManager.SwitchToNextDrink();
            }
        }
    }
}