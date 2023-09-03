Shader "Indestructible/Sea" {
Properties {
 _MainTex ("Main Texture", 2D) = "white" {}
 _Diffuse ("Diffuse", Float) = 1
 _Specular ("Specular", Float) = 1
 _Shininess ("Shininess", Float) = 64
 _ModConst ("Mod Const", Vector) = (1,1,1,1)
 _AddConst ("Add Const", Vector) = (0,0,0,0)
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