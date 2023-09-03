Shader "Indestructible/Unlit Transparent Alpha Mask" {
Properties {
 _MainTex ("Main Texture", 2D) = "white" {}
 _MaskTex ("Mask Texture", 2D) = "white" {}
}
SubShader { 
 LOD 100
 Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
 Pass {
  Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
  Fog { Mode Off }
  Blend SrcAlpha OneMinusSrcAlpha
  SetTexture [_MainTex] { combine texture }
  SetTexture [_MaskTex] { combine previous, texture alpha * previous alpha }
 }
}
}