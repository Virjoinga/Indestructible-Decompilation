Shader "Indestructible/General/TextureLightmap" {
Properties {
 _MainTex ("Main Texture", 2D) = "white" {}
}
SubShader { 
 LOD 100
 Tags { "QUEUE"="Geometry" "RenderType"="Opaque" }
 Pass {
  Name "MAIN"
  Tags { "LIGHTMODE"="Vertex" "QUEUE"="Geometry" "RenderType"="Opaque" }
  BindChannels {
   Bind "vertex", Vertex
   Bind "texcoord", TexCoord
   Bind "texcoord1", TexCoord0
  }
  Fog { Mode Off }
  SetTexture [_MainTex] { combine texture }
 }
 Pass {
  Name "MAIN"
  Tags { "LIGHTMODE"="VertexLMRGBM" "QUEUE"="Geometry" "RenderType"="Opaque" }
  BindChannels {
   Bind "vertex", Vertex
   Bind "texcoord", TexCoord
   Bind "texcoord1", TexCoord0
  }
  Fog { Mode Off }
  SetTexture [unity_Lightmap] { Matrix [unity_LightmapMatrix] combine texture * texture alpha double }
  SetTexture [_MainTex] { combine texture * previous double }
 }
 Pass {
  Name "MAIN"
  Tags { "LIGHTMODE"="VertexLM" "QUEUE"="Geometry" "RenderType"="Opaque" }
  BindChannels {
   Bind "vertex", Vertex
   Bind "texcoord", TexCoord
   Bind "texcoord1", TexCoord0
  }
  Fog { Mode Off }
  SetTexture [unity_Lightmap] { Matrix [unity_LightmapMatrix] combine texture }
  SetTexture [_MainTex] { combine texture * previous }
 }
}
}