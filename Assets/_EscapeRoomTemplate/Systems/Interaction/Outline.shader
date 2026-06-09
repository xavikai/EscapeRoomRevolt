Shader "EscapeRoom/Outline"
{
    Properties
    {
        [HDR] _OutlineColor("Highlight Color", Color) = (1, 1, 0, 1)
        _OutlineWidth("Fresnel Power", Range(0.1, 10)) = 3.0
        _HighlightIntensity("Highlight Intensity", Range(0, 10)) = 1.5
    }
    SubShader
    {
        // Transparent queue so it renders over the object
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+100" }
        LOD 100

        Pass
        {
            Name "Highlight"
            Tags { "LightMode"="SRPDefaultUnlit" }
            
            // Additive blending: adds the color to what's behind it
            Blend SrcAlpha One
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
                float3 viewDirWS  : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
                float _HighlightIntensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                float3 posWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(posWS);
                
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(posWS);
                
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float3 normal = normalize(input.normalWS);
                float3 viewDir = normalize(input.viewDirWS);
                
                // Calculate Fresnel
                float NdotV = saturate(dot(normal, viewDir));
                float fresnel = pow(1.0 - NdotV, _OutlineWidth);
                
                float4 color = _OutlineColor;
                // Modulate alpha/intensity by fresnel
                color.a = fresnel * _HighlightIntensity;
                
                // Since it's Additive (SrcAlpha One), returning rgb * a will add nicely
                return float4(color.rgb * color.a, color.a);
            }
            ENDHLSL
        }
    }
}
