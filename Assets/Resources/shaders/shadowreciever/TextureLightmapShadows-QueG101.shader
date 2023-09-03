Shader "Indestructible/ShadowReciever/TextureLightmapShadows-QueG101" {
Properties {
 _MainTex ("Main Texture", 2D) = "white" {}
}
SubShader { 
 LOD 200
 Tags { "QUEUE"="Geometry+101" "RenderType"="Opaque" "ShadowQuality0"="Static" "ShadowQuality1"="Static" }
 UsePass "Indestructible/ShadowReciever/TextureLightmapShadows/MAIN"
}
}