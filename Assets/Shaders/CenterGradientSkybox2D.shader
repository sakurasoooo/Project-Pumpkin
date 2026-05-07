Shader "Custom/CenterGradientSkybox2D"
{
    Properties
    {
        [Header(Gradient Colors)]
        _ColorCenter ("Center Color", Color) = (0.2, 0.5, 0.8, 1)
        _ColorEdge ("Edge Color", Color) = (0.05, 0.1, 0.2, 1)
        
        [Header(Gradient Settings)]
        _CenterPoint ("Center Point (UV)", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Radius Spread", Range(0.1, 3.0)) = 1.0
        
        [Toggle] _IsElliptical ("Match Aspect Ratio", Float) = 0.0
    }
    SubShader
    {
        // 背景层渲染，不需要光照，不需要深度写入
        Tags { "RenderType"="Opaque" "Queue"="Background" "IgnoreProjector"="True" "PreviewType"="Plane"}
        LOD 100
        
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _ColorCenter;
            float4 _ColorEdge;
            float2 _CenterPoint;
            float _Radius;
            float _IsElliptical;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                // 如果勾选了匹配屏幕比例，则将中心辐射变成椭圆（通常在UI或全屏Quad时使用）
                // 这里我们做简单的径向距离计算
                float dist = distance(uv, _CenterPoint);
                
                // 平滑过渡 (Smoothstep 可以让渐变更自然)
                float t = smoothstep(0.0, _Radius, dist);
                
                return lerp(_ColorCenter, _ColorEdge, t);
            }
            ENDCG
        }
    }
}
