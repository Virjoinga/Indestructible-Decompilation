Shader "Indestructible/Unlit Texture Lightmap Fog" {
Properties {
 _MainTex ("Main Texture", 2D) = "white" {}
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
  SetTexture [_MainTex] { combine texture }
 }
 Pass {
  Tags { "LIGHTMODE"="VertexLMRGBM" "QUEUE"="Geometry" "RenderType"="Opaque" }
  BindChannels {
   Bind "vertex", Vertex
   Bind "texcoord", TexCoord
   Bind "texcoord1", TexCoord0
  }
  SetTexture [unity_Lightmap] { Matrix [unity_LightmapMatrix] combine texture * texture alpha double }
  SetTexture [_MainTex] { combine texture * previous double }
 }
 Pass {
  Tags { "LIGHTMODE"="VertexLM" "QUEUE"="Geometry" "RenderType"="Opaque" }
  BindChannels {
   Bind "vertex", Vertex
   Bind "texcoord", TexCoord
   Bind "texcoord1", TexCoord0
  }
  SetTexture [unity_Lightmap] { Matrix [unity_LightmapMatrix] combine texture }
  SetTexture [_MainTex] { combine texture * previous }
 }
}
}