Shader "Custom/HeatWave"
{   
    Properties
    {
        _DistortionAmount("Distortion Amount", Range(0, 0.05)) = 0.008
        _DistortionSpeed("Distortion Speed", Range(0, 3)) = 0.8
        _WaveFrequency("Wave Frequency", Range(5, 50)) = 25.0
        _VerticalBias("Vertical Bias", Range(0, 2)) = 1.2
        
        [Header(Heat Color)]
        _HeatTint("Heat Tint", Color) = (1.0, 0.85, 0.6, 1.0)
        _HeatIntensity("Heat Intensity", Range(0, 1)) = 0.15
        _HeatShimmer("Heat Shimmer", Range(0, 0.5)) = 0.08
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        ZWrite Off Cull Off ZTest Always
        
        Pass
        {
            Name "HeatWave"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _DistortionAmount;
                float _DistortionSpeed;
                float _WaveFrequency;
                float _VerticalBias;
                float4 _HeatTint;
                float _HeatIntensity;
                float _HeatShimmer;
            CBUFFER_END

            float4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float time = _Time.y * _DistortionSpeed;

                float wave1 = sin(uv.y * _WaveFrequency + time * 1.3) * 0.5;
                float wave2 = sin(uv.y * _WaveFrequency * 0.7 + time * 0.9) * 0.3;
                float wave3 = cos(uv.y * _WaveFrequency * 1.5 - time * 1.1) * 0.2;
                
                float combinedWave = (wave1 + wave2 + wave3);
                
                uv.x += combinedWave * _DistortionAmount;

                uv.y += sin(uv.x * _WaveFrequency * 0.5 + time * 0.7) * _DistortionAmount * 0.3 * _VerticalBias;

                float heightFactor = 1.0 - uv.y;
                heightFactor = pow(heightFactor, 1.5);

                uv = lerp(IN.texcoord, uv, heightFactor);
                
                float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv);
                
                float heatNoise = sin(IN.texcoord.y * 15.0 + time * 2.0) * 0.5 + 0.5;
                heatNoise += sin(IN.texcoord.x * 8.0 - time * 1.5) * 0.3;
                heatNoise = saturate(heatNoise);

                float heatMask = heightFactor * _HeatIntensity;
                col.rgb = lerp(col.rgb, col.rgb * _HeatTint.rgb, heatMask);

                float shimmer = heatNoise * heightFactor * _HeatShimmer;
                col.rgb += _HeatTint.rgb * shimmer;
                
                float desaturation = heightFactor * _HeatIntensity * 0.3;
                float luminance = dot(col.rgb, float3(0.299, 0.587, 0.114));
                col.rgb = lerp(col.rgb, float3(luminance, luminance, luminance), desaturation * 0.5);
                col.rgb = lerp(col.rgb, col.rgb * _HeatTint.rgb, desaturation);
                
                return col;
            }
            
            ENDHLSL
        }
    }
}