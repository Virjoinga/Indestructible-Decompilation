Shader "Indestructible/Silhouette/Silhouette-QueG110" {
Properties {
 _SilhouetteWidth ("Silhouette width", Float) = 0.05
 _SilhouetteColor ("Silhouette color", Color) = (0,0,0,1)
}
SubShader { 
 LOD 200
 Tags { "QUEUE"="Geometry+110" }
 UsePass "Indestructible/Silhouette/Silhouette-QueG100/MAIN"
}
}