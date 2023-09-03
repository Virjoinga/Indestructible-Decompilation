Shader "Indestructible/Unlit Texture Detail Lightmap" {
Properties {
 _MainTex ("Main Texture", 2D) = "white" {}
 _Detail ("Detail", 2D) = "white" {}
}
SubShader { 
 LOD 100
 Tags { "QUEUE"="Geometry" "RenderType"="Opaque" }
 Pass {
  Tags { "LIGHTMODE"="Vertex" "QUEUE"="Geometry" "RenderType"="Opaque" }
  BindChannels {
   Bind "vertex", Vertex
   Bind "texcoord", TexCoord
   Bind "texcoord1", TexCoord0
  }
  Fog { Mode Off }
  SetTexture [_MainTex] { combine texture }
  SetTexture [_Detail] { combine texture * previous double }
 }
 Pass {
  Tags { "LIGHTMODE"="VertexLMRGBM" "QUEUE"="Geometry" "RenderType"="Opaque" }
  BindChannels {
   Bind "vertex", Vertex
   Bind "texcoord", TexCoord
   Bind "texcoord1", TexCoord0
  }
  Fog { Mode Off }
  SetTexture [unity_Lightmap] { Matrix [unity_LightmapMatrix] combine texture * texture alpha double }
  SetTexture [_MainTex] { combine texture * previous double, texture alpha }
  SetTexture [_Detail] { combine texture * previous double }
 }
 Pass {
  Tags { "LIGHTMODE"="VertexLM" "QUEUE"="Geometry" "RenderType"="Opaque" }
  BindChannels {
   Bind "vertex", Vertex
   Bind "texcoord", TexCoord
   Bind "texcoord1", TexCoord0
  }
  Fog { Mode Off }
  SetTexture [unity_Lightmap] { Matrix [unity_LightmapMatrix] combine texture }
  SetTexture [_MainTex] { combine texture * previous }
  SetTexture [_Detail] { combine texture * previous double }
 }
}
}