using UnityEngine;
using DG.Tweening;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }
    public CanvasGroup gestureCanvasGroup; 
    private GameObject currentWhiteDot;
    private bool isTutorialCompleted = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        // 【演示模式配置】：每次重启程序强制删除存档，保证每次都能看到引导 
        PlayerPrefs.DeleteKey("TutorialCompleted"); 

        // 读取持久化数据，0代表未完成，1代表已完成
        isTutorialCompleted = PlayerPrefs.GetInt("TutorialCompleted", 0) == 1;

        // 如果已完成，初始化时直接隐藏屏幕UI
        if (gestureCanvasGroup!= null)
        {
            gestureCanvasGroup.alpha = 0;
            gestureCanvasGroup.gameObject.SetActive(false);
        }
    }
     // 供 MenuRoot 中的 WhiteDot 在Start时调用来注册自己
     public void RegisterGuidanceDot(GameObject dot)
    {
        if (isTutorialCompleted) 
        {
            Destroy(dot);
            return;
        }

        // 否则，将其登记，但立即将其隐藏，等待下落动画结束后呼出
        currentWhiteDot = dot;
        currentWhiteDot.SetActive(false); 
    }

    // 供 CocktailARManager 在模型下落动效播放完毕后调用
    public void ShowGuidance()
    {
        if (isTutorialCompleted) return;

        // 动画结束后，激活三维白点
        if (currentWhiteDot!= null) 
        {
            currentWhiteDot.SetActive(true);
        }

        // 动画结束后，平滑淡入屏幕手势引导
        if (gestureCanvasGroup!= null &&!gestureCanvasGroup.gameObject.activeInHierarchy)
        {
            gestureCanvasGroup.gameObject.SetActive(true);
            gestureCanvasGroup.DOFade(1, 0.8f);
        }
    }


    // 供 Swipe_Script 或 ComprehensivePressHandler 检测到有效操作后调用
    public void CompleteTutorial()
    {
        if (isTutorialCompleted) return;

        isTutorialCompleted = true;
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();

        // 销毁三维空间圆点
        if (currentWhiteDot!= null) 
        {
            Destroy(currentWhiteDot);
        }

        // 动画结束后，激活三维白点
        if (currentWhiteDot != null)
        {
            currentWhiteDot.SetActive(true);
        }
        
        // 平滑隐藏屏幕空间UI
        if (gestureCanvasGroup!= null)
        {
            gestureCanvasGroup.DOFade(0, 0.5f).OnComplete(() => 
            {
                gestureCanvasGroup.gameObject.SetActive(false);
            });
        }
    }
    // 供 ViewMode_Script 进入/退出预览模式时调用
    public void TemporarilyHideGuidance(bool hide)
    {
        if (isTutorialCompleted) return;

        if (currentWhiteDot!= null) currentWhiteDot.SetActive(!hide);
        if (gestureCanvasGroup!= null) gestureCanvasGroup.alpha = hide? 0 : 1;
    }
}