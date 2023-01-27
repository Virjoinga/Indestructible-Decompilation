Shader "Indestructible/General/Texture-QueG101" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader { 
 LOD 200
 Tags { "QUEUE"="Geometry+101" "RenderType"="Opaque" }
 UsePass "Indestructible/General/Texture/MAIN"
}
}