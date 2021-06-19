#define PI 3.14159265f
#define MEDIUMP_FLT_MAX    65504.0
#define saturateMediump(x) min(x, MEDIUMP_FLT_MAX)

#pragma pack_matrix(row_major)


//float3 Fresnel_Shlick(const in float3 f0, const in float NoH)
//{
//    return f0 + (1.0 - f0) * pow(1.0 - NoH, 5.0);
//}

//float Filament_F_Schlick(const float f0, const float VoH) {
//	const float f = pow(1.0 - VoH, 5.0);
//    return f + f0 * (1.0 - f);
//}

float3 F_Schlick(const float3 f0, const float VoH) {
	const float f = pow(1.0 - VoH, 5.0);
    return f + f0 * (1.0 - f);
}

//float Diffuse_Burley(const in float3 f0, const in float NoL, const in float NoV)
//{
//    return Fresnel_Shlick(f0, , NoL).x * Filament_F_Schlick(f0, NoV).x;
//}
//
//float Diffuse_Burley(const in float3 f0, const in float NdotL, const in float NdotV, in float LdotH, in float roughness)
//{
//	return Diffuse_Burley(f0, NdotL, NdotV);
//	
//    //float fd90 = 0.5 + 2 * roughness * LdotH * LdotH;
//    //return Fresnel_Shlick(f0, fd90, NdotL).x * Fresnel_Shlick(1, fd90, NdotV).x;
//}

float Fd_Lambert() {
    return 1.0 / PI;
}

//float Fd_Burley(float3 NoV, float3 NoL, float3 LoH, float roughness) {
//    float f90 = 0.5 + 2.0 * roughness * LoH * LoH;
//    float3 lightScatter = F_Schlick(f90, NoL);
//    float3 viewScatter = F_Schlick(f90, NoV);
//    return lightScatter * viewScatter * (1.0 / PI);
//}

float3 disney_diffuse(const in float3 albedo, float NoV, float NoL, float LoH, float rough)
{
	const float fd90 = 0.5 + 2.0 * rough * rough * LoH * LoH;
	const float light = 1.0 + (fd90 - 1.0) * pow(1.0 - NoL, 5);
	const float view = 1.0 + (fd90 - 1.0) * pow(1.0 - NoV, 5);
	return (albedo) * light * view;
}

float D_GGX(const in float NoH, const in float3 n, const in float3 h, const in float roughness) {
	const float3 NxH = cross(n, h);
	const float a = NoH * roughness;
	const float k = roughness / (dot(NxH, NxH) + a * a);
	const float d = k * k * (1.0 / PI);
    return saturateMediump(d);
}

float V_SmithGGXCorrelated(float NoV, float NoL, float roughness) {
    float a2 = roughness * roughness;
    float GGXV = NoL * sqrt(NoV * NoV * (1.0 - a2) + a2);
    float GGXL = NoV * sqrt(NoL * NoL * (1.0 - a2) + a2);
    return 0.5 / (GGXV + GGXL);
}

float G_Shlick_Smith_Hable(const float LdotH, const float alpha)
{
    return rcp(lerp(LdotH * LdotH, 1.0, alpha * alpha * 0.25f));
}

float3 Specular_BRDF(const in float3 f0, const in float3 N, const in float3 H, const in float LoH, const in float NoH, const in float VoH, const in float rough)
{
    // Specular D (microfacet normal distribution) component
    const float specular_d = D_GGX(NoH, N, H, rough);//Specular_D_GGX(alpha, NdotH);

    // Specular Fresnel
    const float3 specular_f = F_Schlick(f0, VoH);//Fresnel_Shlick(specularColor, 1, LdotH);

    // Specular G (visibility) component
    const float specular_g = G_Shlick_Smith_Hable(LoH, rough * rough);

    return specular_d * specular_g * specular_f;
}


// IBL

float3 fresnelSchlickRoughness(const in float3 f0, const in float cosTheta, const in float rough)
{
	const float3 smooth = 1.0 - rough;
    return f0 + (max(float3(smooth), f0) - f0) * pow(max(1.0 - cosTheta, 0.0), 5.0);
}

float get_specular_occlusion(float NoV, float ao, float rough) {
	const float f = max(NoV + ao, 0.0);
	const float x1 = pow(f, exp2(-16.0 * rough - 1.0));
    return saturate(x1 - 1.0 + ao);
}

float3 diffuse_IBL(const in float3 normal, const in float3 kS)
{
	//const float NoV = max(dot(normal, view), 0.0);
	//const float3 kS = fresnelSchlickRoughness(f0, NoV, rough);
	const float3 irradiance = tex_irradiance.SampleLevel(sampler_irradiance, normal, 0);
	return (1.0 - kS) * irradiance;
}

float3 specular_IBL(const in float3 ref, const in float3 kS, const in float roughP)
{
	const float mip = roughP * NumEnvironmentMapMipLevels;
    return tex_environment.SampleLevel(sampler_environment, ref, mip) * kS;
}

float3 IBL(float3 n, float3 v, float3 diffuse, float3 f0, float3 f90, float occlusion, float rough)
{
    float3 indirect_diffuse = srgb_to_linear(vLightAmbient.rgb);
    float3 indirect_specular = indirect_diffuse;
    float3 specular_occlusion = 1.0;
		
	const float NoV = max(dot(n, v), 0.0);
	const float3 kS = fresnelSchlickRoughness(f0, NoV, rough * rough);
    const float3 specular_color = lerp(f0, 1.0, kS); // WARN: wrong af
	
    if (bHasCubeMap) {
		const float3 ref = reflect(-v, n);
    	
    	indirect_diffuse = diffuse_IBL(n, kS);
		indirect_specular = specular_IBL(ref, kS, rough);
		specular_occlusion = get_specular_occlusion(NoV, occlusion, rough);
    }
	
	/*float3 env_color = tex_cube.SampleLevel(sampler_cube, n, 0);
	env_color = env_color / (env_color + 1.0);
	env_color = linear_to_srgb(env_color);
	return float4(env_color, 1.0);*/
	
	

    // Specular indirect
    //const float2 env = prefilteredDFG_LUT(NoV, roughP);
    //const float3 specular_color = f0 * env.x + f90 * env.y;

    // Diffuse indirect
    // We multiply by the Lambertian BRDF to compute radiance from irradiance
    // With the Disney BRDF we would have to remove the Fresnel term that
    // depends on NoL (it would be rolled into the SH). The Lambertian BRDF
    // can be baked directly in the SH to save a multiplication here
    //const float3 indirect_diffuse = max(irradianceSH(n), 0.0) * Fd_Lambert();

    //float3 NoV = dot(normal, view);

	
    // Indirect contribution
    return diffuse * indirect_diffuse * occlusion + indirect_specular * specular_color * specular_occlusion;
}
