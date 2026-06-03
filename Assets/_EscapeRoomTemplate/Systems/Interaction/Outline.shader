Shader "EscapeRoom/Outline"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (1,1,0,1)
        _OutlineWidth("Outline Width", Float) = 0.02
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+100" }
        LOD 100

        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Front
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Convertim a World Space primer per saltar-nos l'escala de l'objecte (Transform Scale)
                float3 posWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                // L'extrusió d'amplada s'aplica en metres reals globals, ignorant les distorsions
                posWS += normalWS * _OutlineWidth;
                
                output.positionCS = TransformWorldToHClip(posWS);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}
