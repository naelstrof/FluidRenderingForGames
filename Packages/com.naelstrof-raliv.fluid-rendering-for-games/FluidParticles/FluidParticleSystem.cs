using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public abstract class FluidParticleSystem {
    public delegate void ParticleCollisionEventDelegate(RaycastHit hit, float particleVolume);

    protected int particleCountMax;

    //[SerializeField] private LightProbeProxyVolume lightProbeVolume;

    [Serializable]
    protected struct Particle {
        public Vector3 position;
        public float volume;
    }

    protected struct ParticlePhysics {
        public Vector3 lastPosition;
        public Vector3 velocity;
        public bool Colliding;
    }

    protected Particle[] _particles;
    protected ParticlePhysics[] _particlePhysics;
    private Material _material;
    private GraphicsBuffer _particleBuffer;
    private MaterialPropertyBlock _materialPropertyBlock;
    private int _particleSpawnIndex;
    private float _strength;
    private FluidParticleSystemSettings _fluidParticleSystemSettings;
    protected LayerMask _collisionLayerMask;
    
    private GraphicsBuffer _meshTriangles;
    private GraphicsBuffer _meshNormals;
    private GraphicsBuffer _meshUVs;
    private static readonly int ParticleTriangles = Shader.PropertyToID("_ParticleTriangles");
    private static readonly int ParticleNormals = Shader.PropertyToID("_ParticleNormals");
    private static readonly int ParticleUVs = Shader.PropertyToID("_ParticleUVs");
    private static readonly int ParticleCount = Shader.PropertyToID("_ParticleCount");
    private static readonly int Particle1 = Shader.PropertyToID("_Particle");

    public event ParticleCollisionEventDelegate particleCollisionEvent;

    protected void TriggerParticleCollisionEvent(RaycastHit hit, float particleVolume) {
        particleCollisionEvent?.Invoke(hit,particleVolume);
    }

    public FluidParticleSystem(Material material, FluidParticleSystemSettings fluidParticleSystemSettings, LayerMask collisionLayerMask, int particleCountMax = 3000) {
        this.particleCountMax = particleCountMax;
        _collisionLayerMask = collisionLayerMask;
        _material = material;
        _fluidParticleSystemSettings = fluidParticleSystemSettings;
        _particles = new Particle[particleCountMax];
        _particlePhysics = new ParticlePhysics[particleCountMax];
        Initialize();
    }
    
    public void Cleanup() {
        _particleBuffer?.Release();
        _particleBuffer = null;
        _meshTriangles?.Release();
        _meshNormals?.Release();
        _meshUVs?.Release();
    }

    Vector3 GenerateNoiseOctave(float frequency, float t) {
        var noise = new Vector3(
            Mathf.PerlinNoise(t*frequency*-1.39f, t*frequency*3.33f),
            Mathf.PerlinNoise(t*frequency*2.19f, t*frequency*-2.11f),
            Mathf.PerlinNoise(t*frequency*0.74f, t*frequency*0.91f)
        );
        return noise;
    }

    Vector3 GenerateVelocityNoise(float t) {
        var noise = Vector3.zero;
        for (int i = 0; i < _fluidParticleSystemSettings.noiseOctaves; i++) {
            var octaveStrength = 1f / Mathf.Pow(2, i);
            noise += GenerateNoiseOctave(Mathf.Pow(_fluidParticleSystemSettings.noiseFrequency, i + 1), t) * octaveStrength;
        }
        return noise;
    }
    
    public void SpawnParticle(
        Vector3 position,
        Vector3 previousPosition,
        Vector3 forward,
        Vector3 previousForward,
        float strength,
        float previousStrength,
        float volume,
        float subT = 0f,
        bool colliding = false
        ) {
        var subTime = Time.timeSinceLevelLoad - Time.deltaTime * subT;
        var velocityNoise = Vector3.one*(1f-_fluidParticleSystemSettings.noiseStrength*0.5f)+GenerateVelocityNoise(subTime)*_fluidParticleSystemSettings.noiseStrength;
        var velocity = Vector3.Lerp(forward, previousForward, subT);
        velocity.Scale(velocityNoise);
        velocity *= Mathf.Lerp(strength, previousStrength, subT);
        _particles[_particleSpawnIndex] = new Particle {
            position = Vector3.Lerp(position, previousPosition, subT) - velocity * Time.deltaTime,
            volume = volume*(1f-velocityNoise.x*0.5f)
        };
        _particlePhysics[_particleSpawnIndex] = new ParticlePhysics {
            velocity = velocity,
        };
        _particles[_particleSpawnIndex].position += _particlePhysics[_particleSpawnIndex].velocity * (Time.deltaTime * subT);
        _particlePhysics[_particleSpawnIndex].velocity += Physics.gravity * (Time.deltaTime * subT);
        _particlePhysics[_particleSpawnIndex].Colliding = colliding;
        _particleSpawnIndex = (_particleSpawnIndex + 1) % _particles.Length;
    }

    public void FixedUpdate() {
        UpdateParticles();
        _particleBuffer.SetData(_particles);
    }

    protected abstract void UpdateParticles();

    void Initialize() {
        // TODO: staticly initialize or separate out
        GenerateMeshData();
        _materialPropertyBlock ??= new MaterialPropertyBlock();
        _materialPropertyBlock.SetBuffer(ParticleTriangles, _meshTriangles);
        _materialPropertyBlock.SetBuffer(ParticleNormals, _meshNormals);
        _materialPropertyBlock.SetBuffer(ParticleUVs, _meshUVs);
        _materialPropertyBlock.SetInt(ParticleCount, particleCountMax);
        //_materialPropertyBlock.SetInt("_ParticleIndexCount", 4);
        if (_particleBuffer == null || !_particleBuffer.IsValid()) {
            _particleBuffer?.Release();
            _particleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, particleCountMax, Marshal.SizeOf<Particle>());
        }
        //if (lightProbeVolume == null) {
        //    lightProbeVolume = new GameObject("FlockingLightProbeVolume", typeof(LightProbeProxyVolume)).GetComponent<LightProbeProxyVolume>();
        //}
        _materialPropertyBlock.SetBuffer(Particle1, _particleBuffer);
    }

    public void Render(CommandBuffer buffer) {
        buffer.DrawProcedural(Matrix4x4.identity, _material, -1, MeshTopology.Triangles, 6, _particles.Length, _materialPropertyBlock);
    }

    private void GenerateMeshData() {
        var normals = new[] {Vector3.forward, Vector3.forward, Vector3.forward,Vector3.forward};
        var triangles = new[] {0, 1, 2, 0, 2, 3};
        var uvs = new[] {Vector2.up, Vector2.one, Vector2.right, Vector2.zero};
        _meshNormals = new GraphicsBuffer(GraphicsBuffer.Target.Structured, normals.Length, Marshal.SizeOf<Vector3>());
        _meshNormals.SetData(normals);
        _meshTriangles = new GraphicsBuffer(GraphicsBuffer.Target.Structured, triangles.Length, Marshal.SizeOf<int>());
        _meshTriangles.SetData(triangles);
        _meshUVs = new GraphicsBuffer(GraphicsBuffer.Target.Structured, uvs.Length, Marshal.SizeOf<Vector2>());
        _meshUVs.SetData(uvs);
    }

    public void OnDrawGizmos() {
    }

}