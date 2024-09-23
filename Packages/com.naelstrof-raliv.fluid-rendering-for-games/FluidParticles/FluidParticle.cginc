#ifndef FLUIDPARTICLES
#define FLUIDPARTICLES

struct Particle {
    float3 position;
    float size;
    float4 color;
    float heightStrength;
};

StructuredBuffer<Particle> _Particle;
StructuredBuffer<int> _ParticleTriangles;
StructuredBuffer<float3> _ParticleNormals;
StructuredBuffer<float2> _ParticleUVs;
StructuredBuffer<float> _ParticleOpacities;
uniform uint _ParticleCount;

void GetParticle(uint vertexID, uint instanceID, out float3 localPosition, out float3 localNormal, out float2 uv, out float4 color, out float heightStrength) {
    int vertIndex = _ParticleTriangles[vertexID];
    float3 particleOffset = _Particle[instanceID].position-_Particle[(instanceID+1)%_ParticleCount].position;
    float particleDistance = length(particleOffset);
    float bunchFactor = saturate(1-particleDistance*10);
    //localPosition = float3(_ParticleUVs[vertIndex] + float2(-0.5, -0.5), 0);
    //float3 cameraPos = GetCameraPositionWS();
    float3 cameraPos = mul(unity_CameraToWorld, float4(0, 0, 0, 1)).xyz;
    
    float3 orthoForward = normalize(cameraPos-_Particle[instanceID].position);
    float3 orthoRight = normalize(mul(unity_CameraToWorld, float4(1, 0, 0, 0)).xyz);
    float3 orthoUp = cross(orthoRight, orthoForward);
    orthoRight = cross(orthoForward, orthoUp);
    
    float3x3 rot = transpose(float3x3(orthoRight, orthoUp, orthoForward));
    
    localPosition = mul(rot, float3(_ParticleUVs[vertIndex] + float2(-0.5, -0.5), 0)).xyz;
    
    //localPosition = mul(unity_CameraToWorld, float4(_ParticleUVs[vertIndex] + float2(-0.5, -0.5), 0, 0)).xyz;
    localPosition*=_Particle[instanceID].size * (1+bunchFactor);
    localPosition+=_Particle[instanceID].position;
    localNormal = _ParticleNormals[vertIndex];
    color = _Particle[instanceID].color;
    heightStrength = _Particle[instanceID].heightStrength*(1+bunchFactor);
    uv = _ParticleUVs[vertIndex];
}

// SHADER GRAPH DOES NOT SUPPORT INSTANCED RENDERING AT THIS TIME
//void GetParticle_float(float vertexID, float instanceID, out float3 localPosition, out float3 localNormal, out float2 uv) {
//    GetParticle(vertexID, instanceID, localPosition, localNormal, uv);
//}

#endif