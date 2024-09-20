using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class FluidParticleSystem {
    
    const int particleCountMax = 3000;
    public delegate void ParticleCollisionEventDelegate(Vector3 position, Vector3 normal);

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
    
    private GraphicsBuffer _meshTriangles;
    private GraphicsBuffer _meshVertices;
    private GraphicsBuffer _meshNormals;
    private GraphicsBuffer _meshUVs;

    private event ParticleCollisionEventDelegate particleCollisionEvent;

    public FluidParticleSystem(Material material) {
        _material = material;
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
    
    public void SpawnParticle(Vector3 position, Vector3 previousPosition, Vector3 forward, Vector3 previousForward, float strength, float previousStrength, float subT = 0f, bool colliding = false) {
        var subTime = Time.timeSinceLevelLoad - Time.deltaTime * subT;
        var noiseFrequency = 4f;
        var velocityNoise = new Vector3(
            1f+Mathf.PerlinNoise(subTime*noiseFrequency*-1.39f, subTime*noiseFrequency*3.33f)*0.3f-0.2f,
            1f+Mathf.PerlinNoise(subTime*noiseFrequency*2.19f, subTime*noiseFrequency*-2.11f)*0.3f-0.2f,
            1f+Mathf.PerlinNoise(subTime*noiseFrequency*0.74f, subTime*noiseFrequency*0.91f)*0.3f-0.2f
        );
        noiseFrequency = 12f;
        velocityNoise += new Vector3(
            1f+Mathf.PerlinNoise(subTime*noiseFrequency*-1.39f, subTime*noiseFrequency*3.33f)*0.3f-0.2f,
            1f+Mathf.PerlinNoise(subTime*noiseFrequency*2.19f, subTime*noiseFrequency*-2.11f)*0.3f-0.2f,
            1f+Mathf.PerlinNoise(subTime*noiseFrequency*0.74f, subTime*noiseFrequency*0.91f)*0.3f-0.2f
        ) * 0.3f;
        var velocity = Vector3.Lerp(forward, previousForward, subT);
        velocity.Scale(velocityNoise);
        noiseFrequency = 4f;
        velocity *= 1f + Mathf.PerlinNoise(subTime * noiseFrequency * -1.39f, subTime * noiseFrequency * 3.33f) * 0.3f -
                    0.2f;
        velocity = velocity * Mathf.Lerp(strength, previousStrength, subT);
        _particles[_particleSpawnIndex] = new Particle {
            position = Vector3.Lerp(position, previousPosition, subT) - velocity * Time.deltaTime,
            volume = strength*(1f-velocityNoise.x*0.5f)
        };
        _particlePhysics[_particleSpawnIndex] = new ParticlePhysics {
            velocity = velocity,
        };
        _particles[_particleSpawnIndex].position += _particlePhysics[_particleSpawnIndex].velocity * Time.deltaTime * subT;
        _particlePhysics[_particleSpawnIndex].velocity += Physics.gravity * Time.deltaTime * subT;
        _particlePhysics[_particleSpawnIndex].Colliding = colliding;
        _particleSpawnIndex = (_particleSpawnIndex + 1) % _particles.Length;
    }

    public void FixedUpdate() {
        for (var index = 0; index < _particles.Length; index++) {
            UpdateParticle(index);
        }
    }

    void UpdateParticle(int index) {
        var positionStep = _particlePhysics[index].velocity * Time.deltaTime;
        if (_particlePhysics[index].Colliding) {
            if (Physics.Raycast(_particles[index].position, positionStep, out var hit, positionStep.magnitude)) {
                particleCollisionEvent?.Invoke(hit.point, hit.normal);
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
        _materialPropertyBlock.SetBuffer("_ParticleTriangles", _meshTriangles);
        _materialPropertyBlock.SetBuffer("_ParticlePositions", _meshVertices);
        _materialPropertyBlock.SetBuffer("_ParticleNormals", _meshNormals);
        _materialPropertyBlock.SetBuffer("_ParticleUVs", _meshUVs);
        _materialPropertyBlock.SetInt("_ParticleCount", particleCountMax);
        //_materialPropertyBlock.SetInt("_ParticleIndexCount", 4);
        if (_particleBuffer == null || !_particleBuffer.IsValid()) {
            _particleBuffer?.Release();
            _particleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, particleCountMax, Marshal.SizeOf<Particle>());
        }
        int i = 0;
        //if (lightProbeVolume == null) {
        //    lightProbeVolume = new GameObject("FlockingLightProbeVolume", typeof(LightProbeProxyVolume)).GetComponent<LightProbeProxyVolume>();
        //}

        _renderParams = new RenderParams(_material) {
            // TODO: FIX BOUNDS
            worldBounds = new Bounds(Vector3.zero, Vector3.one*1000f),
            //material = foliagePack.GetMaterial(),
            matProps = _materialPropertyBlock,
            //lightProbeUsage = LightProbeUsage.UseProxyVolume,
            //reflectionProbeUsage = ReflectionProbeUsage.BlendProbes,
            //lightProbeProxyVolume = lightProbeVolume,
            layer = LayerMask.NameToLayer("FluidVFX")
        };
        _materialPropertyBlock.SetBuffer("_Particle", _particleBuffer);
    }

    public void Render() {
        if (_renderParams.matProps == null) {
            return;
        }
        _particleBuffer.SetData(_particles);
        Graphics.RenderPrimitives(_renderParams, MeshTopology.Triangles, 6, _particles.Length);
    }

    public void GenerateMeshData() {
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