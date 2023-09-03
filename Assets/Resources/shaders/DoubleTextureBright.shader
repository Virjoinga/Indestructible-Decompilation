Shader "Custom/DoubleTextureBright" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
 _AddTex ("Lightmap (RGB)", 2D) = "black" {}
}
SubShader { 
 LOD 100
 Tags { "RenderType"="Opaque" }
 Pass {
  Tags { "RenderType"="Opaque" }
  BindChannels {
   Bind "vertex", Vertex
   Bind "texcoord", TexCoord0
   Bind "texcoord1", TexCoord1
  }
  SetTexture [_MainTex] { combine texture }
  SetTexture [_AddTex] { combine texture * previous double }
 }
}
}