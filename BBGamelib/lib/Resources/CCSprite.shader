// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Custom sprite shader - no lighting, on/off alpha
Shader "BBGamelib/CCSprite" {
        Properties {
            _MainTex ("MainTex", 2D) = "white" {}
       		_Color ("Color", Color) = (1,1,1,1)
      		_AddColor ("AddColor", Color) = (0,0,0,0)
		    _CullMode ("CullMode", Float) = 0
		    _ZWrite("ZWrite", Float) = 0
        }

        SubShader {
            Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}

            ZWrite [_ZWrite]
            Blend SrcAlpha OneMinusSrcAlpha
            Lighting Off
            Cull [_CullMode]

            Pass {
            	 //SetTexture [_MainTex] { combine texture }
		     	 CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
					#pragma multi_compile _ PIXELSNAP_ON
					#pragma multi_compile _ UNITY_ETC1_EXTERNAL_ALPHA
					#include "UnityCG.cginc"
					
					struct appdata_t
					{
						float4 vertex   : POSITION;
						float4 color    : COLOR;
						float2 texcoord : TEXCOORD0;
					};

					struct v2f
					{
						float4 vertex   : SV_POSITION;
						fixed4 color    : COLOR;
						half2 texcoord  : TEXCOORD0;
					};

					fixed4 _Color;
            		fixed4 _AddColor;

					v2f vert(appdata_t IN)
					{
						v2f OUT;
						OUT.vertex = UnityObjectToClipPos(IN.vertex);
						OUT.texcoord = IN.texcoord;
						OUT.color = IN.color * _Color + _AddColor;
						#ifdef PIXELSNAP_ON
						OUT.vertex = UnityPixelSnap (OUT.vertex);
						#endif

						return OUT;
					}

					sampler2D _MainTex;
					sampler2D _AlphaTex;
					float _AlphaSplitEnabled;

					fixed4 SampleSpriteTexture (float2 uv)
					{
						fixed4 color = tex2D (_MainTex, uv);

		#if UNITY_ETC1_EXTERNAL_ALPHA
						if (_AlphaSplitEnabled)
							color.a = tex2D (_AlphaTex, uv).r;
		#endif //UNITY_ETC1_EXTERNAL_ALPHA
						return color;
					}

					fixed4 frag(v2f IN) : SV_Target
					{
						fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color * _Color + _AddColor;
						//will set opacityModifyRGB in CCSprite
//						c.rgb *= c.a;
						return c;
					}
				ENDCG
            }
        }
    }