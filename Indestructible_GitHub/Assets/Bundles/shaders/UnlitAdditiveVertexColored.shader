Shader "Indestructible/Unlit Additive Vertex Colored" {
Properties {
 _MainTex ("Main Texture", 2D) = "white" {}
}
SubShader { 
 LOD 100
 Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
 Pass {
  Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
  BindChannels {
   Bind "vertex", Vertex
   Bind "color", Color
   Bind "texcoord", TexCoord
  }
  ZWrite Off
  Fog { Mode Off }
  Blend One One
  SetTexture [_MainTex] { combine texture * primary }
 }
}
}