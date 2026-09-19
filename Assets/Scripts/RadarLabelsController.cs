using UnityEngine;
using TMPro; 

public class RadarLabelsController : MonoBehaviour
{
    public RadarChart radarChart;
    public RectTransform[] labelTransforms; // 拖入 4 个文字物体的 RectTransform
    public float offset = 1.25f; // 标签偏移倍数

    [ContextMenu("Align Labels")] // 可以在编辑器右键点击组件执行
    void OnEnable() 
{
    // 每次 Canvas 被激活（长按触发）时，延迟一帧刷新，确保坐标计算准确
    Invoke("AlignLabels", 0.05f);
}
    public void AlignLabels()
    {
        if (radarChart == null || labelTransforms == null) return;
        Debug.Log("正在尝试对齐标签...");

        for (int i = 0; i < labelTransforms.Length; i++)
        {
            if (labelTransforms[i] != null)
            {
            // 获取计算后的坐标
            Vector2 pos = radarChart.GetVertexPosition(i, offset);
            labelTransforms[i].anchoredPosition = pos;

            // 强制将文字的 Z 轴拉近相机，防止埋在模型里
            //labelTransforms[i].localPosition = new Vector3(pos.x, pos.y, -1f);
            
            // 打印日志，在 Console 窗口确认坐标是否正确
            Debug.Log($"标签 {labelTransforms[i].name} 已移动到: {pos}");
            }
        }
    }

    void Start() => AlignLabels(); // 运行即定位
}