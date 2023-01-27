Shader "Indestructible/ShadowCaster/TextureLightmap-DSC" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader { 
 LOD 200
 Tags { "QUEUE"="Geometry" "RenderType"="Opaque" "ShadowQuality0"="Dynamic" "ShadowQuality1"="Dynamic" }
 UsePass "Indestructible/General/TextureLightmap/MAIN"
}
}