using UnityEngine;
using DG.Tweening;

public class IceDropEffect : MonoBehaviour
{
    [SerializeField] private GameObject splashPrefab; // 掉落产生的水花粒子预制体
    [SerializeField] private AudioSource audioSource; // 音效
    
    void Start()
    {
        if (transform.parent == null) return;
        // 获取酒杯父级的Y轴缩放比例
        float scaleY = transform.parent.localScale.y;
        // 将绝对高度换算为在酒杯内部的高度
        float startLocalY = 0.3f / scaleY;    // 冰块下落点
        float targetLocalY = 0.18f / scaleY;   // 下落后的高度
        float floatingLocalY = targetLocalY - (0.01f / scaleY); // 冰块漂浮高度
        // 对齐酒杯的X和Z轴，冰块在酒杯的正上方
        transform.localPosition = new Vector3(0, startLocalY,0);

        // 执行物理下落动画，使用DOLocalMoveY (局部移动)，让冰块始终位于杯子内部
            transform.DOLocalMoveY(floatingLocalY, 0.4f).SetEase(Ease.InQuad);
            transform.DORotate(new Vector3(180,180,180), 0.4f, RotateMode.LocalAxisAdd)
                .OnComplete(() => 
                {
                    // 模拟溅起水花
                    if (splashPrefab != null)
                    {
                        GameObject splash = Instantiate(splashPrefab, transform.parent);
                        splash.transform.localPosition = new Vector3(0, targetLocalY, 0);
                        Destroy(splash, 1.5f);
                    }

                    // 加入音效
                    if (audioSource != null && audioSource.clip != null)
                    {
                        audioSource.PlayOneShot(audioSource.clip);
                    }

                    // 模拟下沉后漂浮效果
                    float floatingRange = 0.005f / scaleY;
                    transform.DOLocalMoveY(floatingLocalY - floatingRange, 0.8f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
                });
    }
}
