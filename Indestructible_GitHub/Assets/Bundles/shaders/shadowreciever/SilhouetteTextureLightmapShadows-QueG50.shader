Shader "Indestructible/ShadowReciever/SilhouetteTextureLightmapShadows" {
Properties {
 _MainTex ("Main Texture", 2D) = "white" {}
 _SilhouetteWidth ("Silhouette width", Float) = 0.1
 _SilhouetteColor ("Silhouette color", Color) = (0,0,0,1)
}
SubShader { 
 LOD 100
 Tags { "QUEUE"="Geometry+50" "RenderType"="Opaque" "ShadowQuality0"="Static" "ShadowQuality1"="Static" }
 UsePass "Indestructible/Silhouette/Silhouette-QueG100/MAIN"
 UsePass "Indestructible/ShadowReciever/TextureLightmapShadows/MAIN"
}
}