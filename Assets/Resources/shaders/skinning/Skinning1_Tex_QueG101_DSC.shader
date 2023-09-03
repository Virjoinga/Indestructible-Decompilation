Shader "Indestructible/Skinning/Skinning1_Tex_QueG101_DSC" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader { 
 LOD 200
 Tags { "QUEUE"="Geometry+101" "RenderType"="Opaque" "ShadowQuality0"="DynamicSkinned0" "ShadowQuality1"="DynamicSkinned0" }
 UsePass "Indestructible/Skinning/Skinning1_Tex/MAIN"
}
}