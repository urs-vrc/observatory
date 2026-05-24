// Copyright 2026 Ayase Minori and Umamusume Racing Society
// Licensed under the BSD-3-Clause License
// See LICENSE for details
//
// In memoriam of Liaku, thank you for bringing warmth for everyone in the URS <3
Shader "Skybox/LiaNightSky"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID 
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 viewDir : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO 
            };

            float hash3(float3 p)
            {
                p = frac(p * 0.3183099f + float3(0.1f, 0.1f, 0.1f));
                p *= 17.0f;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                // Transform the local vertex coordinate into a clean 3D direction vector
                o.viewDir = v.vertex.xyz; 
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // Use the normalized 3D structural view direction passed from vertex properties
                float3 viewDir = normalize(i.viewDir);
                float3 finalColor = float3(0.002f, 0.002f, 0.006f);

                // High-Density Procedural Stars Generation
                float3 starGrid = viewDir * 150.0f;
                float3 gridId = floor(starGrid);
                float3 localUV = frac(starGrid);
                
                float starProbability = hash3(gridId);
                
                if (starProbability > 0.35f)
                {
                    float3 starCenter = float3(
                        hash3(gridId * 1.1f),
                        hash3(gridId * 1.4f),
                        hash3(gridId * 1.7f)
                    ) * 0.6f + 0.2f;
                    
                    float dist = length(localUV - starCenter);
                    float sizeFactor = frac(starProbability * 124.45f) * 0.12f + 0.03f;
                    float stars = smoothstep(sizeFactor, 0.0f, dist);
                    
                    float twinkleSpeed = 2.0f + frac(starProbability * 3.0f) * 3.0f;
                    float twinkleTime = _Time.y * twinkleSpeed;
                    float twinkle = sin(twinkleTime + starProbability * 6.2831f) * 0.14f + 0.86f;
                    
                    stars *= twinkle;
                    finalColor += stars * float3(0.85f, 0.92f, 1.0f);
                }

                // Dedicated Golden Memorial Star (LiaStar Alignment)
                float3 natiePos = normalize(float3(0.2f, 0.5f, -1.0f));
                float natieDot = dot(viewDir, natiePos);
                float natieAngle = acos(clamp(natieDot, -1.0f, 1.0f));
                float natieCore = smoothstep(0.0018f, 0.0f, natieAngle);
                
                float natieGlowBloom = smoothstep(0.04f, 0.0f, natieAngle) * 0.4f;
                float natieGlowPulse = sin(_Time.y * 1.2f) * 0.08f + 0.92f;
                float3 goldenColor = float3(1.0f, 0.85f, 0.45f);
                
                finalColor += (natieCore + natieGlowBloom) * goldenColor * natieGlowPulse * 2.2f;

                return float4(finalColor, 1.0f);
            }
            ENDCG
        }
    }
}