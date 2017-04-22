// Custom sprite shader - no lighting, on/off alpha
// Custom sprite shader - no lighting, on/off alpha
    Shader "BBGamelib/CCTexture" {
        Properties {
            _MainTex ("MainTex", 2D) = "white" {}
        }

        SubShader {
            Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}

            ZWrite off
            Blend SrcAlpha OneMinusSrcAlpha
            Lighting Off

            Pass {
                SetTexture [_MainTex] { combine texture }
            }
        }
    }