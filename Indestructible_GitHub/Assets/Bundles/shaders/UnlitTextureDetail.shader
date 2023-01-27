Shader "Indestructible/Unlit Texture Detail" {
Properties {
 _MainTex ("Main Texture", 2D) = "white" {}
 _Detail ("Detail", 2D) = "white" {}
}
SubShader { 
 LOD 100
 Tags { "QUEUE"="Geometry" "RenderType"="Opaque" }
 Pass {
  Tags { "QUEUE"="Geometry" "RenderType"="Opaque" }
  Fog { Mode Off }
  SetTexture [_MainTex] { combine texture }
  SetTexture [_Detail] { combine texture * previous double }
 }
}
}