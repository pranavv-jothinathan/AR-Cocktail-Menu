using UnityEngine;
using System.Collections;
using DG.Tweening;


public class CocktailARManager : MonoBehaviour
{
    public static CocktailARManager Instance;
    // 模型引用
    [HideInInspector] public Transform baseModel;
    [HideInInspector] public Transform clothModel; 
    [HideInInspector] public Transform drinksContainer;
    [HideInInspector] public Transform[] drinks;

    // 每杯鸡尾酒的特效
    public GameObject iceEffectPrefab;
    public GameObject fireEffectPrefab;
    public GameObject goldEffectPrefab;

    private GameObject currentMenuRootInstance;
    private int currentIndex = 0; // 当前鸡尾酒索引
    private GameObject currentActiveEffect; 
    private Sequence baseSequence;
    private Sequence drinkSequence;
    private int effectToken;
    private Coroutine activeSequence; // 识别当前执行动效，用于后进动作打断当前动效

    // 公开动效只读
    public bool IsAnimating { get; private set; } = false;

    // 模型的实际尺寸
    private Vector3 baseOriginalScale;
    private Vector3 clothOriginalScale;
    private Vector3 clothOriginalLocalPos;
    private Vector3[] drinksOriginalScales;

    private void Awake()
    {
        Instance = this; // // 当 AR 实例化clone时，把它注册给instance
    }

    // 重置状态
    private void ResetAllState()
    {
        baseModel.DOKill(); 
        baseModel.localScale = Vector3.zero; 
        baseModel.gameObject.SetActive(false);

        clothModel.DOKill(); 
        clothModel.localScale = Vector3.zero;
        clothModel.gameObject.SetActive(false);

        foreach (var drink in drinks) 
        {
            drink.DOKill();
            drink.localScale = Vector3.zero; 
            drink.gameObject.SetActive(false); 
        }
    }
        // 公开注册接口
    public void RegisterDynamicReferences(GameObject rootInstance, Transform cloneBase, Transform cloneCloth, Transform cloneDrinksParent)
    {
        // 如果当前已经有一个Simulator测试模型，此时收到了新的模型注册（AR追踪生成的Clone），则立刻销毁旧模型
        if (currentMenuRootInstance!= null && currentMenuRootInstance!= rootInstance)
        {
            Destroy(currentMenuRootInstance);
        }
        currentMenuRootInstance = rootInstance;

        StopAllCoroutines();
        if (activeSequence!= null) StopCoroutine(activeSequence);
        IsAnimating = false; 
        activeSequence = null;

        this.baseModel = cloneBase;
        this.clothModel = cloneCloth;
        this.drinksContainer = cloneDrinksParent;
        
        // 记录酒杯的初始尺寸
        int drinkCount = cloneDrinksParent.childCount;
        this.drinks = new Transform[drinkCount];
        this.drinksOriginalScales = new Vector3[drinkCount];
        for(int i = 0; i < drinkCount; i++)
        {
            this.drinks[i] = cloneDrinksParent.GetChild(i);
            this.drinksOriginalScales[i] = this.drinks[i].localScale;
        }

        // 记录Base cloth的初始尺寸
        baseOriginalScale = baseModel.localScale;
        if (clothModel!= null)
        {
            clothOriginalScale = clothModel.localScale;
            clothOriginalLocalPos = clothModel.localPosition;
        }

        ResetAllState(); // 统一调用重置状态

        StartCoroutine(WaitForSmoothFollowAndPlayIntro()); // 开启平滑追踪等待
    }

    private IEnumerator WaitForSmoothFollowAndPlayIntro()
    {
        yield return new WaitForSeconds(0f); // SmoothFollow方案已改为直接跟随，暂设为0
        if (baseModel!= null) baseModel.gameObject.SetActive(true);
        if (clothModel!= null) clothModel.gameObject.SetActive(true);
        if (drinksContainer!= null) drinksContainer.gameObject.SetActive(true);

        activeSequence = StartCoroutine(IntroSequence());
    }
    // 定义序列动画自动触发策略：弹出底座-> 杯布出现 -> 酒杯弹出 -> 酒杯特效
    public IEnumerator IntroSequence()
    {
        IsAnimating = true;
        PlayBaseAnimation(); // 触发底座动效sequence

        yield return new WaitForSeconds(2.3f); //底座动效播放1.8s时，衔接酒杯掉落动效

        ShowDrink(currentIndex); // 当前索引的酒杯动画
    }

    // 定义索引酒杯的动画效果（先出现后下落）
    private void ShowDrink(int index)
    {
        Transform drink = drinks[index];
        drink.gameObject.SetActive(true);
        drink.localScale = Vector3.zero;
        drink.localPosition = new Vector3(0,0.25f,0); // 从Y轴上方0.25m开始下落 
        
        drinkSequence = DOTween.Sequence(); // 酒杯下落动效sequence

        drinkSequence.Append(drink.DOLocalMoveY(0, 0.7f).SetEase(Ease.OutElastic, 1.0f, 0.4f)) // 酒杯下落到0
        .Join(drink.DOScale(drinksOriginalScales[index], 0.6f).SetEase(Ease.OutBack)) //同时放大回弹
        .OnComplete(() => 
        {
            // 通知 TutorialManager 弹出所有的交互引导
            if (TutorialManager.Instance!= null)
            {
                TutorialManager.Instance.ShowGuidance();
            }

            // 动效结束，触发鸡尾酒专属特效，同步接触动画锁定状态
            TriggerDrinkEffect(index);
            IsAnimating = false;

            // This tells the UI to show the button the moment the drink lands!
            if (ViewMode_Script.Instance != null)
            {
                ViewMode_Script.Instance.buttonEnter.SetActive(true);
            }
        });
        
    }

    // 暴露API接口
    public bool IsReady => drinks != null && drinks.Length >0 && baseModel != null && clothModel != null;
        // 1. 切换到下一个酒杯，调用 arManager.SwitchToNextDrink();
        public void SwitchToNextDrink()
    {
        if (!IsReady || IsAnimating) return;
        int nextIndex = (currentIndex + 1) % drinks.Length;
        SwitchToDrink(nextIndex);
    }
        // 2. 切换到上一个酒杯，调用 arManager.SwitchToPreviousDrink();
        public void SwitchToPreviousDrink()
    {
        if (!IsReady || IsAnimating) return;
        int prevIndex = (currentIndex - 1 + drinks.Length) % drinks.Length;
        SwitchToDrink(prevIndex);
    }
        // 3. 切换策略：后进打断当前
        public void SwitchToDrink(int targetIndex)
    {
        if (targetIndex == currentIndex) return;

        if (activeSequence != null)StopCoroutine(activeSequence);
        
        KillActiveAnimations();

        IsAnimating = true;
        drinks[currentIndex].localScale = Vector3.zero;
        drinks[currentIndex].gameObject.SetActive(false);

        currentIndex = targetIndex;

        activeSequence = StartCoroutine(SwitchSequence(targetIndex));
    }
        // 4. 切换后的新动效
        public IEnumerator SwitchSequence(int index)
    {
        baseModel.localScale = baseOriginalScale;

        clothModel.gameObject.SetActive(true);
        clothModel.localScale = clothOriginalScale;
        clothModel.localPosition = clothOriginalLocalPos;
        Renderer clothRenderer = clothModel.GetComponentInChildren<Renderer>();
        if (clothRenderer != null)
            {
                Color c = clothRenderer.material.GetColor("_BaseColor");
                c.a = 1f;
                clothRenderer.material.SetColor("_BaseColor",c);
            }
        ShowDrink(index);
        yield return null;
    }

    private void KillActiveAnimations()
    {
        baseSequence?.Kill();
        drinkSequence?.Kill();

        if (baseModel != null) baseModel.DOKill();
        if (clothModel != null) clothModel.DOKill();

        if (drinks != null)
        {
            foreach (var drink in drinks)
            {
                if (drink != null) drink.DOKill();
            }
        }

        effectToken++;
        ClearActiveEffect();
        IsAnimating = false;
    }

    // 暴露当前正在显示的酒杯模型，供 ViewMode_Script 调用
    public Transform GetCurrentDrink()
    {
        if (drinks != null && currentIndex >= 0 && currentIndex < drinks.Length)
        {
            return drinks[currentIndex];
        }
        return null;
    }
    // 公开清除鸡尾酒特效的方法， 供 ViewMode_Script 调用
    public void ClearActiveEffect()
    {
        if (currentActiveEffect != null)
        {
            Destroy(currentActiveEffect);
            currentActiveEffect = null;
        }
    }

    // 根据索引自动匹配并instantiate对应特效
    private void TriggerDrinkEffect(int index)
    {
        int token = ++effectToken;
        StartCoroutine(DelayedTriggerEffect(index, token));
    }

    private IEnumerator DelayedTriggerEffect(int index, int token)
    {
        // 冰块延迟半秒下落
        yield return new WaitForSeconds(0.5f);
        if (token != effectToken) yield break;
        if (currentIndex != index) yield break;
        if (ViewMode_Script.Instance != null && ViewMode_Script.Instance.IsInViewMode()) yield break;
        
        Transform targetDrink = drinks[index];
        Transform spawnPoint = targetDrink.Find("FirePoint");
        Vector3 spawnPos = (spawnPoint != null) ? spawnPoint.position : targetDrink.position;
        switch (index)
        {
            case 0:
                if (iceEffectPrefab != null)
                    currentActiveEffect = Instantiate(iceEffectPrefab, spawnPos, Quaternion.identity, targetDrink);
                break;
            case 1:
                if (fireEffectPrefab != null)
                {
                    currentActiveEffect = Instantiate(fireEffectPrefab, spawnPos, Quaternion.identity, targetDrink);
                }
                break;
            case 2:
                if (goldEffectPrefab != null)
                    currentActiveEffect = Instantiate(goldEffectPrefab, spawnPos, Quaternion.identity, targetDrink);
                break;
        }
    }

    public void PlayBaseAnimation()
    {
        if (baseModel == null) return;

        // 设置Base初始状态
        baseModel.gameObject.SetActive(true);
        baseModel.localScale = Vector3.zero;
        baseModel.localPosition = Vector3.zero;
        baseModel.localRotation = Quaternion.identity;

        // 创建动画Sequence
        baseSequence = DOTween.Sequence();
        // 1.空中弹出效果
        baseSequence.Append(baseModel.DOLocalMoveY(0.15f, 0.4f).SetEase(Ease.OutQuad)) // 0.4s内向上位移15cm（Ease缓动曲线）
        .Join(baseModel.DOScale(baseOriginalScale,0.4f).SetEase(Ease.OutCubic)) //同时放大到正常尺寸
        .Join(baseModel.DOLocalRotate(new Vector3(0,360,0), 0.7f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic)); //同时旋转360度

        // 2.落地回弹效果
        baseSequence.Append(baseModel.DOLocalMoveY(0f,0.6f).SetEase(Ease.OutBack, 1.2f)); // 0.6s落回地面并且触发outback回弹效果(1.2f 是 Overshoot（超出量）参数，值越小，砸地感越干净利落)

        // 3.杯布飘落动画
        clothModel.gameObject.SetActive(false);
        Renderer clothRenderer = clothModel.GetComponentInChildren<Renderer>(); // 获取杯布的renderer，将初始透明值设为0
        
        if (clothRenderer !=null && clothRenderer.material.HasProperty("_BaseColor"))
        {
            Color initColor = clothRenderer.material.GetColor("_BaseColor");
            initColor.a = 0f;
            clothRenderer.material.SetColor("_BaseColor", initColor);

            baseSequence.Insert(1.5f, clothRenderer.material.DOFade(1f, "_BaseColor", 0.4f).SetEase(Ease.Linear)); //渐显动画，0.4s内恢复透明度1
        }

        baseSequence.InsertCallback(1.5f, () => 
        {
            clothModel.gameObject.SetActive(true);
            clothModel.localScale = clothOriginalScale;
            clothModel.localPosition = clothOriginalLocalPos + new Vector3(0, 0.1f, 0); // 将杯布放到相对底座上方 0.1m 处
        });
        baseSequence.Insert(1.5f, clothModel.DOLocalMove(clothOriginalLocalPos, 0.7f).SetEase(Ease.OutCubic)); // 杯布下落,OutCubic 会非常有“飘落”的感觉
    }


}