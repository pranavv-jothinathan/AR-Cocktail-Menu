using UnityEngine;

[ExecuteAlways] // 在编辑模式也能看到效果
public class LiquidController : MonoBehaviour
{
    public Renderer liquidRenderer; // 拖入圆柱体
    [Range(0, 1)] public float fillAmount = 0.5f; // 0是空，1是满
    public Transform cupBottom; // 创建一个空物体放在杯底内部，拖进来
    public float cupHeight = 0.2f; // 手动测量杯子内部有多高

    void Update()
    {
        if (liquidRenderer == null || cupBottom == null) return;

        Material mat = liquidRenderer.sharedMaterial;

        // 1. 计算世界空间的底部和顶部高度
        float worldBottom = cupBottom.position.y;
        float worldTop = worldBottom + cupHeight;

        // 2. 计算当前水位的世界高度
        float currentHeight = worldBottom + (cupHeight * fillAmount);

        // 3. 将这些数值传给 Shader
        mat.SetFloat("_BottomLevel", worldBottom);
        mat.SetFloat("_TopLevel", worldTop);
        mat.SetFloat("_CurrentFillHeight", currentHeight);
    }
}
