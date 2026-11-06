// Palette-swap shader for the menu pets. Pixels that exactly match one of
// the 4 cat or 4 dog fur-key colors are replaced by the matching ramp
// shade; everything else (eyes, outline, belly) passes through. Defaults
// are the key colors themselves, so an unconfigured material is identity.
// Otherwise a copy of UI/Default (stencil, rect mask and alpha clip intact).
Shader "Game/UI/FurRecolor"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _CatKey0 ("Cat Key 0", Color) = (0.431,0.165,0.078,1)
        _CatKey1 ("Cat Key 1", Color) = (0.643,0.282,0.102,1)
        _CatKey2 ("Cat Key 2", Color) = (0.941,0.494,0.133,1)
        _CatKey3 ("Cat Key 3", Color) = (1.000,0.745,0.361,1)
        _CatRamp0 ("Cat Ramp 0", Color) = (0.431,0.165,0.078,1)
        _CatRamp1 ("Cat Ramp 1", Color) = (0.643,0.282,0.102,1)
        _CatRamp2 ("Cat Ramp 2", Color) = (0.941,0.494,0.133,1)
        _CatRamp3 ("Cat Ramp 3", Color) = (1.000,0.745,0.361,1)

        _DogKey0 ("Dog Key 0", Color) = (0.361,0.204,0.141,1)
        _DogKey1 ("Dog Key 1", Color) = (0.604,0.376,0.220,1)
        _DogKey2 ("Dog Key 2", Color) = (0.839,0.643,0.431,1)
        _DogKey3 ("Dog Key 3", Color) = (0.925,0.784,0.565,1)
        _DogRamp0 ("Dog Ramp 0", Color) = (0.361,0.204,0.141,1)
        _DogRamp1 ("Dog Ramp 1", Color) = (0.604,0.376,0.220,1)
        _DogRamp2 ("Dog Ramp 2", Color) = (0.839,0.643,0.431,1)
        _DogRamp3 ("Dog Ramp 3", Color) = (0.925,0.784,0.565,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            float4 _CatKey0, _CatKey1, _CatKey2, _CatKey3;
            float4 _CatRamp0, _CatRamp1, _CatRamp2, _CatRamp3;
            float4 _DogKey0, _DogKey1, _DogKey2, _DogKey3;
            float4 _DogRamp0, _DogRamp1, _DogRamp2, _DogRamp3;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            float3 SwapIfKey(float3 rgb, float3 key, float3 target)
            {
                float3 d = rgb - key;
                return dot(d, d) < 0.00002 ? target : rgb;
            }

            float3 SwapFur(float3 rgb)
            {
                float3 o = rgb;
                o = SwapIfKey(o, _CatKey0.rgb, _CatRamp0.rgb);
                o = SwapIfKey(o, _CatKey1.rgb, _CatRamp1.rgb);
                o = SwapIfKey(o, _CatKey2.rgb, _CatRamp2.rgb);
                o = SwapIfKey(o, _CatKey3.rgb, _CatRamp3.rgb);
                o = SwapIfKey(o, _DogKey0.rgb, _DogRamp0.rgb);
                o = SwapIfKey(o, _DogKey1.rgb, _DogRamp1.rgb);
                o = SwapIfKey(o, _DogKey2.rgb, _DogRamp2.rgb);
                o = SwapIfKey(o, _DogKey3.rgb, _DogRamp3.rgb);
                return o;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 tex = tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd;
                half4 color = half4(SwapFur(tex.rgb), tex.a) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
        ENDCG
        }
    }
}
