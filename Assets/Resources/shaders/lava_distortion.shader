Shader "Indestructible/Lava" {
Properties {
 _LavaTex ("LavaTex", 2D) = "white" {}
 _DistortionMap ("DistortionMap", 2D) = "white" {}
 _DistortionScale ("DistortionScale", Range(0,0.05)) = 0.01
 _DistortionOffset ("DistortionOffset", Range(0,1)) = 0.8
 _UVOffset ("UVScroll", Range(0,100)) = 0
 _LavaTile ("LavaTile", Vector) = (1,5,0,1)
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