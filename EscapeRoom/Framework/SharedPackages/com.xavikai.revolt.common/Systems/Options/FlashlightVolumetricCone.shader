Shader "EscapeRoom/FlashlightVolumetricCone"
{
    Properties
    {
        [HDR] _Color("Beam Color", Color) = (1, 1, 1, 1)
        _Intensity("Intensity", Range(0, 5)) = 1.0
    }
    SubShader
    {
        // Transparent, additive fake-volumetric beam. No depth write so cones can overlap and
        // blend with each other and with the scene behind them; still depth-tested so walls
        // correctly occlude the beam.
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Name "VolumetricBeam"
            Tags { "LightMode"="SRPDefaultUnlit" }

            Blend One One
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
                float3 viewDirWS  : TEXCOORD1;
                float4 color      : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Intensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 posWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(posWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(posWS);
                output.color = input.color;
                return output;
            }

            // Vertex alpha carries the radial/length fade baked in at mesh-build time; RGB is
            // always white so the material's own tint/intensity fully controls the look. The
            // "facing" term hides the beam when the camera looks straight down its own axis
            // (holding the flashlight and looking forward) - without it the tube's near wall
            // fills the whole screen at a grazing angle; viewed from the side it reads as a beam.
            float4 frag(Varyings input) : SV_Target
            {
                float facing = saturate(dot(normalize(input.normalWS), normalize(input.viewDirWS)));
                float3 rgb = _Color.rgb * _Intensity * input.color.a * facing;
                return float4(rgb, 0);
            }
            ENDHLSL
        }
    }
}
