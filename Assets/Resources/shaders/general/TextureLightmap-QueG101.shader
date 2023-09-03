Shader "Indestructible/General/TextureLightmap-QueG101" {
Properties {
 _MainTex ("Main Texture", 2D) = "white" {}
}
SubShader { 
 LOD 200
 Tags { "QUEUE"="Geometry+101" "RenderType"="Opaque" }
 UsePass "Indestructible/General/TextureLightmap/MAIN"
}
}