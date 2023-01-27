Shader "Indestructible/Toon/OutlineTexture" {
Properties {
 _SilhouetteWidth ("Silhouette width", Float) = 0.05
 _SilhouetteColor ("Silhouette color", Color) = (0,0,0,1)
 _MainTex ("Base (RGB)", 2D) = "white" {}
}
	//DummyShaderTextExporter
	
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard fullforwardshadows
#pragma target 3.0
		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
		}
		ENDCG
	}
}