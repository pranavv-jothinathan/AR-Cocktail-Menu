using UnityEngine;
using TMPro;

// 执行顺序 先修改文字效果再弯曲
[ExecuteInEditMode]
[DefaultExecutionOrder(100)]

public class OddCylinderText : MonoBehaviour
{
    private TMP_Text m_TextComponent;
    
   
    public float radius = 5.0f;

    private void Awake()
    {
        m_TextComponent = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        // 订阅网格渲染前的事件，这是最高效的生命周期节点
        m_TextComponent.OnPreRenderText += ApplyCylinderWrap;
        m_TextComponent.ForceMeshUpdate();
    }

    private void OnDisable()
    {
        // 确保组件禁用时注销事件，防止内存泄漏
        m_TextComponent.OnPreRenderText -= ApplyCylinderWrap;
    }

    // 核心的曲面数学转换方法
    private void ApplyCylinderWrap(TMP_TextInfo textInfo)
    {
        if (radius <= 0.01f) return;

        int characterCount = textInfo.characterCount;
        if (characterCount == 0) return;

        // 遍历所有字符
        for (int i = 0; i < characterCount; i++)
        {
            // 跳过空格等不可见字符
            if (!textInfo.characterInfo[i].isVisible) continue;

            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            // 遍历单个字符的4个顶点（一个四边形Quad）
            for (int j = 0; j < 4; j++)
            {
                Vector3 origPos = vertices[vertexIndex + j];
                float x = origPos.x;
                float y = origPos.y;
                float z = origPos.z;

                // 计算弧度中心角
                float theta = x / radius;

                // 极坐标到笛卡尔三维坐标的映射
                float newX = radius * Mathf.Sin(theta);
                float newY = y;
                float newZ = radius * Mathf.Cos(theta) - radius + z;

                // 将换算后的新坐标写回顶点矩阵
                vertices[vertexIndex + j] = new Vector3(newX, newY, newZ);
            }
        }

        // 推送更新后的网格数据至GPU进行渲染
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            m_TextComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }

    
}
