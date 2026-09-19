using UnityEngine;
using DG.Tweening; // 引入 DOTween

public class RadarChartGrowth : MonoBehaviour
{
    [Header("雷达图当前数值 (0~1)")]
    // 这里的数值将被 DOTween 动态修改，并驱动 UI 刷新
    public float[] currentValues = new float[4]; 
    public RadarChart radarMeshComponent;

    [Header("动画设置")]
    public float duration = 1.2f; // 生长动画时长
    public Ease easeType = Ease.OutBack; // 推荐 OutBack：会有轻微的超出再弹回的“Q弹”感


    /// <summary>
    /// 当长按射线击中酒液时，调用此方法并传入这杯酒的真实配方数据
    /// </summary>
    /// <param name="targetValues">例如：{0.8f, 0.3f, 0.1f, 0.6f}</param>
    public void AnimateRadarChart(float[] targetValues)
    {
        // 1. 安全检查
        if (targetValues.Length != currentValues.Length)
        {
            Debug.LogError("传入的目标数值维度与雷达图维度不匹配！");
            return;
        }

        // 2. 动画前先将所有当前值归零（从中心点开始生长）
        for (int i = 0; i < currentValues.Length; i++)
        {
            currentValues[i] = 0f;
        }

        // 3. 杀死当前正在运行的雷达图动画（防止用户频繁点击导致动画冲突）
        DOTween.Kill("RadarGrowth");

        // 4. 为每个维度创建独立的插值动画
        for (int i = 0; i < targetValues.Length; i++)
        {
            // 【闭包陷阱防范】在循环中使用 DOTween 时，必须将迭代变量存入局部变量
            int index = i; 

            DOTween.To(
                () => currentValues[index],        // 获取当前值
                x => 
                {
                    currentValues[index] = x;      // 每一帧将插值结果赋给当前值
                    UpdateRadarMesh();             // 核心：每次数值变化，通知 UI 重新绘制多边形
                }, 
                targetValues[index],               // 目标值
                duration                           // 时长
            )
            .SetEase(easeType)                     // 设置缓动曲线
            .SetId("RadarGrowth")                  // 贴上标签，方便上面的 DOTween.Kill 管理
            .SetDelay(index * 0.1f);               // 【视觉优化】让相邻的顶点错开 0.1 秒依次生长，视觉极其丝滑
        }
        
        // 【可选】让整个雷达图 UI 面板也有一个从小变大的弹出动画
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// 通知 UI 重绘的方法
    /// </summary>
    private void UpdateRadarMesh()
    {
        if (radarMeshComponent != null)
        {
        // 每一帧动画数值变化时，都同步给渲染脚本
        radarMeshComponent.SetValues(currentValues);
        }
    }
}