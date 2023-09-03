Shader "Indestructible/Silhouette/SilhouetteTexture" {
Properties {
 _SilhouetteWidth ("Silhouette width", Float) = 0.05
 _SilhouetteColor ("Silhouette color", Color) = (0,0,0,1)
 _MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader { 
 LOD 200
 Tags { "QUEUE"="Geometry+50" "RenderType"="Opaque" "ShadowQuality0"="Dynamic" "ShadowQuality1"="Dynamic" }
 UsePass "Indestructible/Silhouette/Silhouette-QueG100/MAIN"
 UsePass "Indestructible/General/Texture/MAIN"
}
}