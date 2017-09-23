Shader "Custom/BoxShader" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader 
	{
		LOD 200
		
		CGPROGRAM
		#pragma surface surf StandardSpecular  vertex:vert fullforwardshadows addshadow 
		#pragma target 3.0
			 
		sampler2D _MainTex;
		sampler2D _DataInSquare;

		struct Input 
		{
			float2 texcoord;
			float3 boxNormal;
		};

		float _CubeWidth;
		float _CubeHeight;
		float4 _UvScale;
		float4 _UvOffset;

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		UNITY_INSTANCING_CBUFFER_START(Props)
		UNITY_INSTANCING_CBUFFER_END

		float3 GetNormal(float4 baseTexcoord)
		{
			float4 xOffset = float4(0.01, 0, 0, 0);
			float4 yOffset = float4(0, 0.01, 0, 0);
			float heightA = tex2Dlod(_DataInSquare, baseTexcoord + xOffset).x;
			float heightB = tex2Dlod(_DataInSquare, baseTexcoord - xOffset).x;
			float heightC = tex2Dlod(_DataInSquare, baseTexcoord + yOffset).x;

			float3 pointA = float3(1, heightA, 0);
			float3 pointB = float3(-1, heightB, 0);
			float3 pointC = float3(0, heightC, 1);

			float3 diffA = pointC - pointA;
			float3 diffB = pointC - pointB;
			float3 ret = cross(diffA, diffB);
			return normalize(ret);
		}

		void vert(inout appdata_full v, out Input o)
		{ 
			v.texcoord = float4(1 - v.vertex.xz, 0, 0);
			v.texcoord *= _UvScale;
			v.texcoord -= _UvOffset;
			v.texcoord *= -1;
			v.texcoord += _UvScale / 2;
			v.texcoord.y = 1 - v.texcoord.y;
			v.vertex.x *= _CubeWidth;
			v.vertex.z *= _CubeHeight; 

			float height = tex2Dlod(_DataInSquare, v.texcoord).x;
			v.vertex.y *= height;
			o.texcoord = float2(1 - v.texcoord.x, height); 
			o.boxNormal = v.normal;
			float3 computedNormal = GetNormal(v.texcoord);
			v.normal = lerp(v.normal, computedNormal, abs(v.normal.y));
		}

		void surf (Input i, inout SurfaceOutputStandardSpecular o)
		{
			fixed4 c = tex2D (_MainTex, i.texcoord) * _Color;
			fixed3 xSideColor = float3(.9, 1.3, 1.5);
			fixed3 zSideColor = float3(0, 0.1, 0.2);
			fixed3 finalColor = lerp(c.rgb, zSideColor, abs(i.boxNormal.x));
			finalColor = lerp(finalColor, zSideColor, abs(i.boxNormal.z));
			finalColor = lerp(finalColor, c.rgb, .25);
			o.Albedo = finalColor;
			o.Specular = float3(0, .05, .1);
			o.Smoothness = .7;
			//o.Metallic = .9;
			//o.Smoothness = .9;// 1 - abs(i.boxNormal.y);

		}
		ENDCG
	}
	Fallback "Diffuse"
}
