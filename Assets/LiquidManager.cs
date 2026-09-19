using UnityEngine;

[ExecuteAlways]
public class LiquidLayerController : MonoBehaviour
{
    [Header("视觉控制")]
    public Renderer liquidRenderer;
    public float cupHeight = 0.2f;
    public Transform cupBottom;

    [Header("物理分层 (Collider)")]
    public Transform bottomLayerCollider; // 下层液体的圆柱体
    public Transform topLayerCollider;    // 上层液体的圆柱体

    [Range(0, 1)]
    public float fillAmount = 0.5f; // 0是全是上层，1是全是下层

    void Update()
    {
        if (liquidRenderer == null || cupBottom == null) return;

        // --- 1. 之前的 Shader 逻辑保持不变 ---
        float worldBottom = cupBottom.position.y;
        float currentHeight = worldBottom + (cupHeight * fillAmount);

        Material mat = liquidRenderer.sharedMaterial;
        mat.SetFloat("_BottomLevel", worldBottom);
        mat.SetFloat("_CurrentFillHeight", currentHeight);

        // --- 2. 新增：物理碰撞体同步逻辑 ---
        if (bottomLayerCollider != null && topLayerCollider != null)
        {
            UpdatePhysicalLayers(worldBottom, currentHeight);
        }
    }

    void UpdatePhysicalLayers(float worldBottom, float currentHeight)
    {
        // 计算两层的高度
        float bottomHeight = currentHeight - worldBottom;
        float topHeight = cupHeight - bottomHeight;

        // Unity Cylinder 默认高度是 2 个单位，所以 scale.y = 实际高度 / 2
        // 下层设置
        if (bottomHeight > 0.001f)
        {
            bottomLayerCollider.gameObject.SetActive(true);
            bottomLayerCollider.localScale = new Vector3(bottomLayerCollider.localScale.x, bottomHeight / 2f, bottomLayerCollider.localScale.z);
            // 位置设在底部坐标 + 高度的一半
            bottomLayerCollider.position = new Vector3(cupBottom.position.x, worldBottom + (bottomHeight / 2f), cupBottom.position.z);
        }
        else
        {
            bottomLayerCollider.gameObject.SetActive(false);
        }

        // 上层设置
        if (topHeight > 0.001f)
        {
            topLayerCollider.gameObject.SetActive(true);
            topLayerCollider.localScale = new Vector3(topLayerCollider.localScale.x, topHeight / 2f, topLayerCollider.localScale.z);
            // 位置设在分界线高度 + 剩余高度的一半
            topLayerCollider.position = new Vector3(cupBottom.position.x, currentHeight + (topHeight / 2f), cupBottom.position.z);
        }
        else
        {
            topLayerCollider.gameObject.SetActive(false);
        }
    }
}
