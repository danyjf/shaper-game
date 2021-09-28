Shader "Unlit/MeshInfo" {
    Properties {
        _MainTex("Texture", 2D) = "white" {}
        _WireThickness("Wire Thickness", RANGE(0, 800)) = 100
		_WireSmoothness("Wire Smoothness", RANGE(0, 20)) = 3
		_WireColor("Wire Color", Color) = (0.0, 1.0, 0.0, 1.0)
		_BaseColor("Base Color", Color) = (0.0, 0.0, 0.0, 1.0)
        _Shininess("Shininess", Float) = 0.5
        _AmbientColor("Ambient Color", Color) = (0.1, 0.0, 0.0, 1.0)
        _SpecularColor("Spec Color", Color) = (1.0, 1.0, 1.0, 1.0)
    }
	
    SubShader {
        Tags {
			"Queue"="Transparent"
            "RenderType"="Transparent"
		}
		
        LOD 100

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float4 worldPos : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
            };

            float _Shininess;
            float4 _AmbientColor;
            float4 _SpecularColor;

            v2f vert (appdata IN) {
                v2f OUT;
				
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.normal = UnityObjectToWorldNormal(IN.normal);
                OUT.worldPos = mul(unity_ObjectToWorld, IN.vertex);
                OUT.viewDir = normalize(UnityWorldSpaceViewDir(OUT.worldPos));
                OUT.uv = IN.uv;
                
				return OUT;
            }

            fixed4 frag (v2f IN) : SV_Target {
                float lambertian = max(dot(_WorldSpaceLightPos0, IN.normal), 0.0);
                float specular = 0.0;

                if (lambertian > 0.0) {
                    float3 halfDir = normalize(_WorldSpaceLightPos0 + IN.viewDir);
                    float specAngle = max(dot(halfDir, IN.normal), 0.0);
                    specular = pow(specAngle, _Shininess);
                }

                float3 colorLinear = _AmbientColor + lambertian * _LightColor0 + _SpecularColor * specular * _LightColor0;

                return float4(colorLinear,1.0);
            }
            ENDCG
        }

        Pass {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2g {
                float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float4 worldPos : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
            };

            struct g2f {
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
                float4 dis : TEXCOORD1;
                float3 normal : TEXCOORD2;
                float4 worldPos : TEXCOORD3;
                float3 viewDir : TEXCOORD4;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _WireThickness;
			float _WireSmoothness;
			float4 _WireColor; 
			float4 _BaseColor;

            v2g vert (appdata IN) {
                v2g OUT;
				
                UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.normal = UnityObjectToWorldNormal(IN.normal);
                OUT.worldPos = mul(unity_ObjectToWorld, IN.vertex);
                OUT.viewDir = normalize(UnityWorldSpaceViewDir(OUT.worldPos));
                OUT.uv = IN.uv;
                
				return OUT;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream) {
                // paper https://developer.download.nvidia.com/SDK/10/direct3d/Source/SolidWireframe/Doc/SolidWireframe.pdf used for reference

                // get the normalized device coordinates (NDC) from the clip space coordinates
                float2 p0 = IN[0].vertex.xy / IN[0].vertex.w;
                float2 p1 = IN[1].vertex.xy / IN[1].vertex.w;
                float2 p2 = IN[2].vertex.xy / IN[2].vertex.w;

                // calculate edge vectors of the triangle formed by the three points
                float2 e0 = p1 - p0;
                float2 e1 = p2 - p1;
                float2 e2 = p2 - p0;

                // calculate area of triangle using cross product
                float area = abs((e0.x * e1.y) - (e0.y * e1.x)) / 2;
                // calculate heights
                float h0 = 2 * area / length(e0);
                float h1 = 2 * area / length(e1);
                float h2 = 2 * area / length(e2);

                float3x3 distances = float3x3(float3(0,h1,0),float3(0,0,h2),float3(h0,0,0));

                float wireThickness = 800 - _WireThickness;

                g2f OUT;

                for(int i = 0; i < 3; i++) {
                    OUT.vertex = IN[i].vertex;
                    OUT.uv = IN[i].uv;
                    OUT.normal = IN[i].normal;
                    OUT.worldPos = IN[i].worldPos;
                    OUT.viewDir = IN[i].viewDir;
                    OUT.dis.xyz = distances[i] * OUT.vertex.w * wireThickness;
                    OUT.dis.w = 1.0 / OUT.vertex.w;
                    UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(IN[i], OUT);
                    triStream.Append(OUT);
                }

                triStream.RestartStrip();
            }

            fixed4 frag (g2f IN) : SV_Target {
                float minDistanceToEdge = min(IN.dis.x, min(IN.dis.y, IN.dis.z)) * IN.dis.w;

                float4 baseColor = _BaseColor * tex2D(_MainTex, IN.uv);

                if(minDistanceToEdge > 0.9) {
					return fixed4(baseColor.rgb,0);
				}

                float t = exp2(_WireSmoothness * -1.0 * minDistanceToEdge * minDistanceToEdge);
				fixed4 finalColor = lerp(baseColor, _WireColor, t);
				finalColor.a = t;
                
				return finalColor;
            }
            ENDCG
        }
    }
}
