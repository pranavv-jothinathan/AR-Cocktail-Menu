using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class RadarChart : MaskableGraphic
{
    [Tooltip("雷达图的最大半径")]
    public float radius = 1f;

    [Tooltip("各个维度的当前值 (0~1)")]
    // Labels自动定位
    public Vector2 GetVertexPosition(int index, float radiusMultiplier = 1.2f)
{
    if (values == null || values.Length == 0) return Vector2.zero;

    float angleStep = 360f / values.Length;
    // 计算角度（从 90 度即正上方开始）
    float currentAngle = (90f - (index * angleStep)) * Mathf.Deg2Rad;
    
    // 计算坐标：cos对应X，sin对应Y
    // 使用传入的倍数来决定标签距离中心的距离
    float x = Mathf.Cos(currentAngle) * (radius * radiusMultiplier);
    float y = Mathf.Sin(currentAngle) * (radius * radiusMultiplier);
    
    return new Vector2(x, y);
}
    // 默认给 4 个维度的初始值
    [SerializeField] private float[] values = new float[4] { 1f, 1f, 1f, 1f };
    public void SetValues(float[] newValues)
    {
    values = newValues;
    SetVerticesDirty(); // 通知 Unity 重新调用 OnPopulateMesh
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        // 每次重绘前，先清空旧的网格数据
        vh.Clear();

        if (values == null || values.Length < 3) return;

        int dimensionCount = values.Length;
        // 计算每个顶点之间的夹角 (360度 / 维度数)
        float angleStep = 360f / dimensionCount;

        // 1. 添加中心点顶点 (索引为 0)
        // 中心点的值为 0，坐标在 (0,0)
        UIVertex centerVertex = UIVertex.simpleVert;
        centerVertex.position = Vector2.zero;
        centerVertex.color = color; // 使用 Image 组件面板上设置的颜色
        vh.AddVert(centerVertex);

        // 2. 计算并添加外围的顶点 (索引 1 到 dimensionCount)
        for (int i = 0; i < dimensionCount; i++)
        {
            // 从正上方 (Y轴正方向) 开始绘制，所以起始角度加上 90 度
            float currentAngle = (90f - (i * angleStep)) * Mathf.Deg2Rad;

            // 根据当前维度的值 (0~1) 和最大半径，计算实际距离
            float currentRadius = radius * Mathf.Clamp01(values[i]);

            // 利用三角函数计算顶点的 X 和 Y 坐标
            float x = Mathf.Cos(currentAngle) * currentRadius;
            float y = Mathf.Sin(currentAngle) * currentRadius;

            UIVertex vertex = UIVertex.simpleVert;
            vertex.position = new Vector2(x, y);
            vertex.color = color;
            vh.AddVert(vertex);
        }

        // 3. 将顶点连接成三角形 (绘制面)
        // Unity 规定必须按顺时针方向连接顶点，否则面会朝向背面（被剔除）
        for (int i = 0; i < dimensionCount; i++)
        {
            int currentVertIndex = i + 1;
            // 如果是最后一个顶点，则它需要与第一个外围顶点 (索引1) 连接
            int nextVertIndex = (i + 1 == dimensionCount) ? 1 : i + 2;

            // 连接: 中心点(0) -> 当前点 -> 下一个点
            vh.AddTriangle(0, currentVertIndex, nextVertIndex);
        }
    }
}

