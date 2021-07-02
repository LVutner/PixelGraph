#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"

#pragma pack_matrix(row_major)


float main(ps_shadow input) : SV_Depth
{
	const float3 normal = normalize(input.nor);
	//const float3 view = normalize(input.eye);

	float3 shadow_tex;
    const float SNoV = saturate(dot(normal, SunDirection));
	get_parallax_texcoord(input.tex, input.poT, SNoV, shadow_tex);

	const float d = length(float3(input.tex - shadow_tex.xy, 1.0f - shadow_tex.z) * float3(1.0f, 1.0f, ParallaxDepth));

	//return input.pos.z;
	return input.pos.z + d * CUBE_SIZE * 0.01f;
}
