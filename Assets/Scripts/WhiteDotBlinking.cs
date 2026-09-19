using UnityEngine;
using DG.Tweening;

public class WhiteDotBlinking : MonoBehaviour
{
    public float fadeDuration = 0.8f;
   
    public float minAlpha = 0.2f;

    private Material dotMaterial;

    private void Start()
    {
        if (TutorialManager.Instance!= null)
        {
            TutorialManager.Instance.RegisterGuidanceDot(this.gameObject);
        }
        
        Renderer renderer = GetComponent<Renderer>();
        if (renderer!= null)
        {
            // 获取材质实例
            dotMaterial = renderer.material; 
            
            // 使用 DOFade 实现透明度渐变
            dotMaterial.DOFade(minAlpha, fadeDuration)
                      .SetLoops(-1, LoopType.Yoyo)
                      .SetEase(Ease.InOutSine);
        }
    }

    private void OnDestroy()
    {
        // 物体被销毁时kill Tween动画
        if (dotMaterial!= null)
        {
            dotMaterial.DOKill();
        }
    }
}