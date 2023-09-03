Shader "Custom/Vertex Alpha Terrain Lightmap" {
Properties {
 _Color ("Main Color", Color) = (1,1,1,1)
 _MainTex ("Main Texture", 2D) = "white" {}
}
SubShader { 
 Tags { "LIGHTMODE"="VertexLM" "QUEUE"="Transparent-90" }
 Pass {
  Tags { "LIGHTMODE"="VertexLM" "QUEUE"="Transparent-90" }
  BindChannels {
   Bind "vertex", Vertex
   Bind "color", Color
   Bind "texcoord", TexCoord0
   Bind "texcoord1", TexCoord1
  }
  Color [_Color]
  ZWrite Off
  Fog { Mode Off }
  Blend SrcAlpha OneMinusSrcAlpha
  ColorMask RGB
  SetTexture [_MainTex] { combine texture * primary }
  SetTexture [unity_Lightmap] { Matrix [unity_LightmapMatrix] combine texture * previous, previous alpha }
 }
}
}