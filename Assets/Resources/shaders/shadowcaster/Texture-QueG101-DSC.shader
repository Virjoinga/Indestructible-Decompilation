Shader "Indestructible/ShadowCaster/Texture-QueG101-DSC" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader { 
 LOD 200
 Tags { "QUEUE"="Geometry+101" "RenderType"="Opaque" "ShadowQuality0"="Dynamic" "ShadowQuality1"="Dynamic" }
 UsePass "Indestructible/General/Texture/MAIN"
}
}