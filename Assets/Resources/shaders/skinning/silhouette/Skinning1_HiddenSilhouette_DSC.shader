Shader "Indestructible/Skinning/Silhouette/Skinning1_HiddenSilhouette_DSC" {
Properties {
 _SilhouetteWidth ("Silhouette width", Float) = 0.05
 _SilhouetteColor ("Silhouette color", Color) = (0.6,0.75,0.8,1)
}
SubShader { 
 LOD 400
 Tags { "QUEUE"="Overlay+1002" "ShadowQuality0"="DynamicSkinned1" }
 UsePass "Indestructible/Skinning/Silhouette/Skinning1_HiddenSilhouette/MAIN"
}
}