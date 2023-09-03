Shader "Indestructible/Silhouette/SilhouetteTextureLightmap-QueG50" {
Properties {
 _SilhouetteWidth ("Silhouette width", Float) = 0.1
 _SilhouetteColor ("Silhouette color", Color) = (0,0,0,1)
 _MainTex ("Main Texture", 2D) = "white" {}
}
SubShader { 
 LOD 100
 Tags { "QUEUE"="Geometry+50" "RenderType"="Opaque" }
 UsePass "Indestructible/Silhouette/Silhouette-QueG100/MAIN"
 UsePass "Indestructible/General/TextureLightmap/MAIN"
}
}