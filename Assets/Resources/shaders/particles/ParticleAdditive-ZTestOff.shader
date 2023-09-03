Shader "Indestructible/Particles/ParticleAdditive-ZTestOff" {
Properties {
 _MainTex ("Particle Texture", 2D) = "white" {}
}
SubShader { 
 Tags { "QUEUE"="Transparent+200" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
 Pass {
  Tags { "QUEUE"="Transparent+200" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
  BindChannels {
   Bind "vertex", Vertex
   Bind "color", Color
   Bind "texcoord", TexCoord
  }
  ZTest False
  ZWrite Off
  Cull Off
  Fog { Mode Off }
  Blend SrcAlpha One
  SetTexture [_MainTex] { combine texture * primary }
 }
}
}