Shader "Indestructible/Silhouette/SilhouetteTexture-QueG50" {
Properties {
 _SilhouetteWidth ("Silhouette width", Float) = 0.05
 _SilhouetteColor ("Silhouette color", Color) = (0,0,0,1)
 _MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader { 
 LOD 200
 Tags { "QUEUE"="Geometry+50" "RenderType"="Opaque" }
 UsePass "Indestructible/Silhouette/Silhouette-QueG100/MAIN"
 UsePass "Indestructible/General/Texture/MAIN"
}
}