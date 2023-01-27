Shader "Custom/Vertex Alpha Color Lightmap" {
Properties {
 _Color ("Main Color", Color) = (1,1,1,1)
 _MainTex ("Main Texture", 2D) = "white" {}
 _LightMap ("Lightmap", 2D) = "white" {}
}
SubShader { 
 Tags { "QUEUE"="Transparent" }
 Pass {
  Tags { "QUEUE"="Transparent" }
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
  ColorMaterial AmbientAndDiffuse
  Offset -1, -1
  SetTexture [_MainTex] { combine texture * previous }
  SetTexture [_LightMap] { combine texture * previous, previous alpha }
 }
}
}