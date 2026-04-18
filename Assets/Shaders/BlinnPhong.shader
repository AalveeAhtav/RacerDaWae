Shader "Custom/BlinnPhong"
{
    Properties
    {
        _Color          ("Albedo (Base Color)", Color) = (1, 1, 1, 1)
        _MainTex        ("Albedo Texture", 2D) = "white" {}
        _SpecularColor  ("Specular Color", Color) = (1, 1, 1, 1)
        _Shininess      ("Shininess", Range(1, 256)) = 64
        _SpecularStrength("Specular Strength", Range(0, 2)) = 0.5
        _AmbientStrength("Ambient Strength", Range(0, 1)) = 0.3
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        LOD 300

        Pass
        {
            Name "BlinnPhongMain"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _MainTex_ST;
                float4 _SpecularColor;
                float  _Shininess;
                float  _SpecularStrength;
                float  _AmbientStrength;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float2 uv          : TEXCOORD2;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = posInputs.positionCS;
                OUT.positionWS  = posInputs.positionWS;
                VertexNormalInputs normInputs = GetVertexNormalInputs(IN.normalOS);
                OUT.normalWS = normInputs.normalWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 texColor  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half4 baseColor = texColor * _Color;

                float3 N = normalize(IN.normalWS);
                float3 V = normalize(GetCameraPositionWS() - IN.positionWS);

                Light mainLight = GetMainLight();
                float3 L = normalize(mainLight.direction);
                float3 lightColor = mainLight.color;

                float3 ambient = _AmbientStrength * baseColor.rgb;

                float NdotL = max(dot(N, L), 0.0);
                float3 diffuse = NdotL * lightColor * baseColor.rgb;

                float3 H       = normalize(L + V);
                float  NdotH   = max(dot(N, H), 0.0);
                float  spec    = pow(NdotH, _Shininess);
                float3 specular = _SpecularStrength * spec * _SpecularColor.rgb * lightColor;

                float3 finalColor = ambient + diffuse + specular;
                return half4(finalColor, baseColor.a);
            }

            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }

    FallBack "Universal Render Pipeline/Lit"
}