Shader "Custom/EdgeHighlight"
{
    Properties
    {
        _EdgeColor ("Edge Color", Color) = (1, 0.85, 0.3, 1)
        _EdgePower ("Edge Power", Float) = 3
        _EdgeIntensity ("Edge Intensity", Float) = 3
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderQueue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
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
                float4 _EdgeColor;
                float  _EdgePower;
                float  _EdgeIntensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS   = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS  = GetCameraPositionWS() - TransformObjectToWorld(input.positionOS.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 N = normalize(input.normalWS);
                float3 V = normalize(input.viewDirWS);
                float fresnel = pow(1.0 - saturate(dot(N, V)), _EdgePower);
                float alpha = saturate(fresnel * _EdgeIntensity);
                return half4(_EdgeColor.rgb * _EdgeIntensity, alpha);
            }
            ENDHLSL
        }
    }
}
