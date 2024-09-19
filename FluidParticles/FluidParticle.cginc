#ifndef FLUIDPARTICLES
#define FLUIDPARTICLES

struct Particle {
    int index;
    float3 position;
    float volume;
};

StructuredBuffer<Particle> _Particle;
StructuredBuffer<int> _ParticleTriangles;
StructuredBuffer<float3> _ParticlePositions;
StructuredBuffer<float3> _ParticleNormals;
StructuredBuffer<float2> _ParticleUVs;
uniform uint _ParticleCount;

void GetParticle(uint vertexID, uint instanceID, out float3 localPosition, out float3 localNormal, out float2 uv) {
    int vertIndex = _ParticleTriangles[vertexID];
    float3 particleOffset = _Particle[instanceID].position-_Particle[(instanceID+1)%_ParticleCount].position;
    float particleDistance = length(particleOffset);
    float bunchFactor = saturate(1-particleDistance*10);
    localPosition = mul(unity_CameraToWorld, float4(_ParticleUVs[vertIndex] + float2(-0.5, -0.5), 0, 0)).xyz;
    localPosition*=0.1*_Particle[instanceID].volume * (1+bunchFactor);
    localPosition+=_Particle[instanceID].position;
    localNormal = _ParticleNormals[vertIndex];
    uv = _ParticleUVs[vertIndex];
}

// SHADER GRAPH DOES NOT SUPPORT INSTANCED RENDERING AT THIS TIME
//void GetParticle_float(float vertexID, float instanceID, out float3 localPosition, out float3 localNormal, out float2 uv) {
//    GetParticle(vertexID, instanceID, localPosition, localNormal, uv);
//}

#endif