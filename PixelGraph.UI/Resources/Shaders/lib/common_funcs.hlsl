#define CUBE_SIZE 4.0f
#define PI 3.14159265f
#define EPSILON 1e-6f
#define GAMMA 2.4

#pragma pack_matrix(row_major)

static const float InvGamma = 1.0f / GAMMA;
static const float3 lum_factor = float3(0.299f, 0.587f, 0.114f);


float3 pow2(const in float3 x) {return x*x;}
float pow4(const in float x) {return x*x*x*x;}

float4 clip_to_screen(const in float4 pos)
{
	float4 o = pos * 0.5f;
	o.xy = float2(o.x, -o.y) + o.w;
	o.zw = pos.zw;
	return o;
	
    //return float2(1.0f + pos.x / pos.z, 1.0f - pos.y / pos.z);

	//return 0.5f * float2(pos.x, -pos.y) + 0.5f;
}

float srgb_to_linear(const float srgb) {
	return pow(abs(srgb), GAMMA);
}

float2 srgb_to_linear(const float2 srg) {
	return pow(abs(srg), GAMMA);
}

float3 srgb_to_linear(const float3 srgb) {
	return pow(abs(srgb), GAMMA);
}

float3 linear_to_srgb(const float3 rgb) {
	return pow(abs(rgb), InvGamma);
}

float f0_to_ior(const in float f0) {
	const float sqrt_f0 = sqrt(f0);
	return (1.0f + sqrt_f0) / max(1.0f - sqrt_f0, EPSILON);
}

float3 f0_to_ior(const in float3 f0) {
	const float3 sqrt_f0 = sqrt(f0);
	return (1.0f + sqrt_f0) / max(1.0f - sqrt_f0, EPSILON);
}

float ior_to_f0(const in float ior) {
	return pow((ior - 1.0f) / (ior + 1.0f), 2.0f);
}

float3 ior_to_f0(const in float3 ior) {
	return pow((ior - 1.0f) / (ior + 1.0f), 2.0f);
}

float3 ior_to_f0_complex(const in float3 ior_n, const in float3 ior_k) {
	const float3 k2 = ior_k * ior_k;
	return (pow2(ior_n - 1.0) + k2) / (pow2(ior_n + 1.0) + k2);
}

float3 ior_to_f0_complex(const in float3 ior_n_in, const in float3 ior_n_out, const in float3 ior_k) {
	const float3 k2 = ior_k * ior_k;
	return (pow2(ior_n_out - ior_n_in) + k2) / (pow2(ior_n_out + ior_n_in) + k2);
}

float lengthSq(const in float3 vec)
{
	return vec.x*vec.x + vec.y*vec.y + vec.z*vec.z;
}

float lum(const in float3 color)
{
    return dot(color, lum_factor);
}

float3 calc_tex_normal(const in float2 tex, const in float3 normal, const in float3 tangent, const in float3 bitangent)
{
    float3 tex_normal = tex_normal_height.Sample(sampler_height, tex).xyz;
	tex_normal = mad(2.0f, tex_normal, -1.0f);
	
    return normalize(normal + mad(tex_normal.x, tangent, tex_normal.y * bitangent));
}
