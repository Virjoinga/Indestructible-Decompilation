Shader "Indestructible/General/Texture-QueG111" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader { 
 LOD 200
 Tags { "QUEUE"="Geometry+111" "RenderType"="Opaque" }
 UsePass "Indestructible/General/Texture/MAIN"
}
}