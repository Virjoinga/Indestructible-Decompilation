Shader "Indestructible/Unlit Texture No Fog" {
Properties {
 _MainTex ("Main Texture", 2D) = "white" {}
}
SubShader { 
 LOD 100
 Tags { "QUEUE"="Geometry" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
 Pass {
  Tags { "QUEUE"="Geometry" "IGNOREPROJECTOR"="True" "RenderType"="Opaque" }
  Fog { Mode Off }
  SetTexture [_MainTex] { combine texture }
 }
}
}