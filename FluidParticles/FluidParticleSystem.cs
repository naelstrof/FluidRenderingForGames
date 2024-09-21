using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class FluidParticleSystem {
    
    const int particleCountMax = 3000;
    public delegate void ParticleCollisionEventDelegate(RaycastHit hit, float particleVolume);

    //[SerializeField] private LightProbeProxyVolume lightProbeVolume;

    [Serializable]
    private struct Particle {
        public int index;
        public Vector3 position;
        public float volume;
    }

    private struct ParticlePhysics {
        public Vector3 velocity;
        public bool Colliding;
    }

    private Particle[] _particles;
    private ParticlePhysics[] _particlePhysics;
    private Material _material;
    private GraphicsBuffer _particleBuffer;
    private MaterialPropertyBlock _materialPropertyBlock;
    private RenderParams _renderParams;
    private int _particleSpawnIndex;
    private float _strength;
    private FluidParticleSystemSettings _fluidParticleSystemSettings;
    private LayerMask layerMask;
    
    private GraphicsBuffer _meshTriangles;
    private GraphicsBuffer _meshVertices;
    private GraphicsBuffer _meshNormals;
    private GraphicsBuffer _meshUVs;
    private static readonly int ParticleTriangles = Shader.PropertyToID("_ParticleTriangles");
    private static readonly int ParticlePositions = Shader.PropertyToID("_ParticlePositions");
    private static readonly int ParticleNormals = Shader.PropertyToID("_ParticleNormals");
    private static readonly int ParticleUVs = Shader.PropertyToID("_ParticleUVs");
    private static readonly int ParticleCount = Shader.PropertyToID("_ParticleCount");
    private static readonly int Particle1 = Shader.PropertyToID("_Particle");

    public event ParticleCollisionEventDelegate particleCollisionEvent;

    public FluidParticleSystem(Material material, FluidParticleSystemSettings fluidParticleSystemSettings) {
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
        _meshVertices?.Release();
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
    
    public void SpawnParticle(Vector3 position, Vector3 previousPosition, Vector3 forward, Vector3 previousForward, float strength, float previousStrength, float subT = 0f, bool colliding = false) {
        var subTime = Time.timeSinceLevelLoad - Time.deltaTime * subT;
        var velocityNoise = Vector3.one*(1f-_fluidParticleSystemSettings.noiseStrength*0.5f)+GenerateVelocityNoise(subTime)*_fluidParticleSystemSettings.noiseStrength;
        var velocity = Vector3.Lerp(forward, previousForward, subT);
        velocity.Scale(velocityNoise);
        velocity = velocity * Mathf.Lerp(strength, previousStrength, subT);
        _particles[_particleSpawnIndex] = new Particle {
            position = Vector3.Lerp(position, previousPosition, subT) - velocity * Time.deltaTime,
            volume = strength*(1f-velocityNoise.x*0.5f)
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
        for (var index = 0; index < _particles.Length; index++) {
            UpdateParticle(index);
        }
        _particleBuffer.SetData(_particles);
    }

    void UpdateParticle(int index) {
        var positionStep = _particlePhysics[index].velocity * Time.deltaTime;
        if (_particlePhysics[index].Colliding) {
            if (Physics.Raycast(_particles[index].position, positionStep, out var hit, positionStep.magnitude)) {
                particleCollisionEvent?.Invoke(hit, _particles[index].volume);
                var walk = index;
                do {
                    _particles[walk].volume = 0f;
                    walk = (walk + 1) % _particlePhysics.Length;
                } while (!_particlePhysics[walk].Colliding);
            }
        }
        _particles[index].position += positionStep;
        // TODO: can fade based on proximity to being respawned
        //_particles[index].volume = Mathf.Max(0f, _particles[index].volume-Time.deltaTime*0.3f);
        _particlePhysics[index].velocity += Physics.gravity * Time.deltaTime;
    }

    void Initialize() {
        // TODO: staticly initialize or separate out
        GenerateMeshData();
        _materialPropertyBlock ??= new MaterialPropertyBlock();
        _materialPropertyBlock.SetBuffer(ParticleTriangles, _meshTriangles);
        _materialPropertyBlock.SetBuffer(ParticlePositions, _meshVertices);
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
        layerMask = LayerMask.NameToLayer("FluidVFX");
        _renderParams = new RenderParams(_material) {
            // TODO: FIX BOUNDS
            worldBounds = new Bounds(Vector3.zero, Vector3.one*1000f),
            matProps = _materialPropertyBlock,
            //lightProbeUsage = LightProbeUsage.UseProxyVolume,
            //reflectionProbeUsage = ReflectionProbeUsage.BlendProbes,
            //lightProbeProxyVolume = lightProbeVolume,
            layer = layerMask
        };
        _materialPropertyBlock.SetBuffer(Particle1, _particleBuffer);
    }

    public void Render() {
        if (_renderParams.matProps == null) {
            return;
        }

        Graphics.RenderPrimitives(_renderParams, MeshTopology.Triangles, 6, _particles.Length);
    }

    private void GenerateMeshData() {
        var vertices = new Vector3[] {
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero
        };
        var normals = new Vector3[] {Vector3.forward, Vector3.forward, Vector3.forward,Vector3.forward};
        var triangles = new int[] {0, 1, 2, 0, 2, 3};
        var uvs = new Vector2[] {Vector2.up, Vector2.up + Vector2.right, Vector2.right, Vector2.zero};
        _meshVertices = new GraphicsBuffer(GraphicsBuffer.Target.Structured, vertices.Length, Marshal.SizeOf<Vector3>());
        _meshVertices.SetData(vertices);
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