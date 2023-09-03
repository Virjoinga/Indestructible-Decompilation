Shader "Custom/VertexLit Colored Alpha 4lightmap" {
Properties {
 _Color ("Main Color", Color) = (1,1,1,1)
 _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}
SubShader { 
 Tags { "QUEUE"="Transparent" }
 Pass {
  Tags { "QUEUE"="Transparent" }
  Lighting On
  ZWrite Off
  Blend SrcAlpha OneMinusSrcAlpha
  AlphaTest Greater 0
  ColorMask RGB
  ColorMaterial AmbientAndDiffuse
  Offset 0, -1
  SetTexture [_MainTex] { ConstantColor [_Color] combine texture, previous alpha * texture alpha }
 }
}
Fallback "Alpha/VertexLit"
}