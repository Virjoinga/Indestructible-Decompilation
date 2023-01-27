Shader "Indestructible/Skinning/Silhouette/Skinning1_Tex_Outline_QueG101_DSC" {
Properties {
 _SilhouetteWidth ("Silhouette width", Float) = 0.05
 _SilhouetteColor ("Silhouette color", Color) = (0,0,0,1)
 _MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader { 
 LOD 200
 Tags { "QUEUE"="Geometry+101" "RenderType"="Opaque" "ShadowQuality0"="DynamicSkinned0" "ShadowQuality1"="DynamicSkinned0" }
 UsePass "Indestructible/Skinning/Silhouette/Skinning1_Silhouette_QueG100/MAIN"
 UsePass "Indestructible/Skinning/Skinning1_Tex/MAIN"
}
}