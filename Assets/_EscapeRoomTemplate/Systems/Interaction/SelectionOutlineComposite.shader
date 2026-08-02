Shader "Hidden/EscapeRoom/SelectionOutlineComposite"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "SelectionOutlineComposite"
            ZWrite Off ZTest Always Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            TEXTURE2D(_ERSelectionMask);
            SAMPLER(sampler_ERSelectionMask);
            float4 _ERSelectionMask_TexelSize;
            float4 _OutlineColor;
            float _OutlineThickness;
            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                half4 scene = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                float center = SAMPLE_TEXTURE2D(_ERSelectionMask, sampler_ERSelectionMask, uv).r;
                float2 stepUv = _ERSelectionMask_TexelSize.xy * _OutlineThickness;
                float edge = 0;
                edge = max(edge, SAMPLE_TEXTURE2D(_ERSelectionMask, sampler_ERSelectionMask, uv + float2(stepUv.x, 0)).r);
                edge = max(edge, SAMPLE_TEXTURE2D(_ERSelectionMask, sampler_ERSelectionMask, uv - float2(stepUv.x, 0)).r);
                edge = max(edge, SAMPLE_TEXTURE2D(_ERSelectionMask, sampler_ERSelectionMask, uv + float2(0, stepUv.y)).r);
                edge = max(edge, SAMPLE_TEXTURE2D(_ERSelectionMask, sampler_ERSelectionMask, uv - float2(0, stepUv.y)).r);
                float outline = saturate(edge - center);
                return lerp(scene, _OutlineColor, outline * _OutlineColor.a);
            }
            ENDHLSL
        }
    }
}
