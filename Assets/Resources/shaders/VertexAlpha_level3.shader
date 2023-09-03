Shader "Custom/VertexLit Colored Alpha level3" {
Properties {
 _Color ("Main Color", Color) = (1,1,1,1)
 _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}
SubShader { 
 Tags { "QUEUE"="Transparent-1" }
 Pass {
  Tags { "QUEUE"="Transparent-1" }
  ZWrite Off
  Blend SrcAlpha OneMinusSrcAlpha
  AlphaTest Greater 0
  ColorMask RGB
  ColorMaterial AmbientAndDiffuse
  Offset -2, -2
  SetTexture [_MainTex] { ConstantColor [_Color] combine texture, previous alpha * texture alpha }
 }
}
Fallback "Alpha/VertexLit"
}