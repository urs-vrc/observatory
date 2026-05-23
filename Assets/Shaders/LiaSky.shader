Shader "Unlit/SkyboxShader"
{
Properties
    {
        // Properties are now primarily populated externally by the WorldManager script
        _CustomSunDir ("Sun Direction", Vector) = (0, 1, 0, 0)
        _WeatherIntensity ("Weather Intensity", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

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

            float4 _CustomSunDir;
            float _WeatherIntensity;

            float hash3(float3 p)
            {
                p = frac(p * 0.3183099 + float3(0.1, 0.1, 0.1));
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 viewDir = normalize(float3((i.uv - 0.5) * float2(_ScreenParams.x / _ScreenParams.y, 1.0), -1.0));
                float3 sunDir = normalize(_CustomSunDir.xyz);
                float skyVisibility = lerp(1.0, 0.08, _WeatherIntensity);

                // Evaluate Day/Night state strictly from our actual physical sun height vector
                float isNight = smoothstep(0.2, -0.2, sunDir.y);
                
                
                float3 daySkyTone = float3(0.15, 0.4, 0.7);
                float goldenHour = smoothstep(0.3, -0.1, abs(sunDir.y));
                float3 horizonGlow = float3(0.95, 0.5, 0.25) * goldenHour * max(0.0, sunDir.y + 0.3);
                float3 daySkyColor = lerp(horizonGlow, daySkyTone, max(0.0, viewDir.y * 0.5 + 0.5));
                float3 nightSkyColor = float3(0.002, 0.002, 0.006);
                float3 clearSky = lerp(daySkyColor, nightSkyColor, isNight);
                
                float3 stormSky = lerp(float3(0.25, 0.27, 0.3), float3(0.001, 0.001, 0.003), isNight);
                float3 currentSky = lerp(clearSky, stormSky, _WeatherIntensity);
                
                float3 finalColor = currentSky;

                // High-Density Procedural Stars Generation
                if (isNight > 0.0)
                {
                    float3 starGrid = viewDir * 150.0;
                    float3 gridId = floor(starGrid);
                    float3 localUV = frac(starGrid);
                    
                    float starProbability = hash3(gridId);
                    
                    if (starProbability > 0.35)
                    {
                        float3 starCenter = float3(
                            hash3(gridId * 1.1),
                            hash3(gridId * 1.4),
                            hash3(gridId * 1.7)
                        ) * 0.6 + 0.2;
                        
                        float dist = length(localUV - starCenter);
                        float sizeFactor = frac(starProbability * 124.45) * 0.12 + 0.03;
                        float stars = smoothstep(sizeFactor, 0.0, dist);
                        
                        // Keep time-based twinkle local to engine runtime clock tracking arrays
                        float twinkleSpeed = 2.0 + frac(starProbability * 3.0) * 3.0;
                        float twinkle = sin(_Time.y * twinkleSpeed + starProbability * 6.2831) * 0.14 + 0.86;
                        
                        stars *= twinkle;
                        finalColor += stars * float3(0.85, 0.92, 1.0) * skyVisibility * isNight;
                    }
                }

                // Dedicated Golden Memorial Star (LiaStar)
                float3 natiePos = normalize(float3(0.2, 0.5, -1.0));
                float natieDot = dot(viewDir, natiePos);
                
                float exclusionMask = smoothstep(0.994, 0.997, natieDot) * isNight;
                finalColor *= (1.0 - exclusionMask);
              
                float natieAngle = acos(clamp(natieDot, -1.0, 1.0));
                float natieCore = smoothstep(0.0018, 0.0, natieAngle);
                float natieGlowBloom = smoothstep(0.02, 0.0, natieAngle) * 0.4;
                float natieGlowPulse = sin(_Time.y * 1.2) * 0.08 + 0.92;
                float3 goldenColor = float3(1.0, 0.85, 0.45);

                finalColor += (natieCore + natieGlowBloom) * goldenColor * natieGlowPulse * 2.2 * skyVisibility * isNight;

                // Daytime Sun Disk & Outer Corona
                float sunDot = max(0.0, dot(viewDir, sunDir));
                float sunDisk = smoothstep(0.994, 0.997, sunDot) * (1.0 - isNight);
                float sunGlow = smoothstep(0.98, 0.75, sunDot) * 0.4 * (1.0 - isNight);
                
                float sunWeatherDim = lerp(1.0, 0.25, _WeatherIntensity);
                float3 sunColor = lerp(float3(1.0, 0.96, 0.88), float3(0.4), _WeatherIntensity);
                
                finalColor += (sunDisk + sunGlow) * sunColor * sunWeatherDim;

                return float4(finalColor, 1.0);
            }
            ENDCG
        }
   }
}