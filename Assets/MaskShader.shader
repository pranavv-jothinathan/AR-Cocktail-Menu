Shader "Custom/StencilMask" {
    SubShader {
        // 渲染队列必须早于液体
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1" }
        Cull Off      // 不闭合模型必须双面渲染，确保内壁也写标记
        ZWrite On     // 写深度，确保它能正确占据杯子的空间位置
        ColorMask 0   // 保持不可见
        Pass {
            Stencil {
                Ref 1
                Comp Always
                Pass Replace
            }
        }
    }
}
