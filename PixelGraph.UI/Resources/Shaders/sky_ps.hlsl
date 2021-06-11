#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/sky.hlsl"

#pragma pack_matrix(row_major)


float4 main(const in ps_input_cube input) : SV_TARGET
{
	const float3 view = normalize(input.tex);
    const float3 col = get_sky_color(view);
	return float4(col, 1);
}
