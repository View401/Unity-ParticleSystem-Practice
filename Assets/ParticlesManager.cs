using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
//using System.Drawing;
//using System.Security.Cryptography;
using UnityEngine;

public class ParticlesManager : MonoBehaviour {
    public struct Particle
    {
        public Vector4 position;
        public Vector4 color;
        public Vector4 initialPosition;    //
        public Vector4 initialVelocity;
        public Vector4 velocity;
        public Vector2 ageAndlife;
        public float psize;
    };

    public ComputeShader computeShader;
    public Material material;
    [SerializeField]
    public Mesh instanceMesh;
    [Range(0.01f, 0.08f)]
    public float startSize = 0.01f;
    public Color startColor; 
    public Color endColor;
    public float gravity;
    public float startSpeed;
    ComputeBuffer particles,argsBuffer;
    
    public int size;
    public float lifeDecay;
    public float duration;
    public float[] forceCenter = new float[] { 0f, 0f, 0f };
    int WARP_SIZE;
    int stride;
    int warpCount;
    int particleCount=0;
    int kernelIndex;
    int kernelIndexUpdate;
    int cbParticlesStorage;
    uint[] _args = new uint[5] { 0, 0, 0, 0, 0 };
    // Use this for initialization
    void Start () {

        warpCount = Mathf.CeilToInt((float)size / WARP_SIZE);

        stride = Marshal.SizeOf(typeof(Particle));
        startColor = new Color(0,0,1,1);
        endColor = Color.red;
        size = 2560;
        gravity = 0.5f;
        startSpeed = 1;
        lifeDecay = 1;
        duration = 5;
        cbParticlesStorage = size;

        particles = new ComputeBuffer(size, stride);
        argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);


        kernelIndex = computeShader.FindKernel("emitter");
        kernelIndexUpdate = computeShader.FindKernel("Update");


        //computeShader.SetFloat("cbEndSize", 0.5f);
        
    }
	
	// Update is called once per frame
	void Update () {
        if (size != cbParticlesStorage)
        {
            cbParticlesStorage = size;
            particleCount = 0;
            particles = new ComputeBuffer(size, stride);
            particleCount = 0;
        }

        float dt = Time.deltaTime;
        computeShader.SetFloat("dt", dt);
        computeShader.SetInt("idx", particleCount);
        computeShader.SetFloats("EmitterPosition", new float[] { transform.position.x, transform.position.y, transform.position.z, 1f });
        computeShader.SetFloat("gravity", gravity);
        computeShader.SetFloat("startSpeed", startSpeed);
        computeShader.SetFloats("cbStartColor", new float[] { startColor.r, startColor.g, startColor.b, startColor.a });
        computeShader.SetFloats("cbEndColor", new float[] { endColor.r, endColor.g, endColor.b, endColor.a });
        computeShader.SetFloat("cbStartSize", startSize);
        computeShader.SetFloats("forceCenter", forceCenter);
        computeShader.SetFloat("cbLifeDecay", lifeDecay);
        computeShader.SetFloat("duration", duration);
        computeShader.SetInt("cbParticlesStorage", cbParticlesStorage);
        computeShader.SetBuffer(kernelIndex, "Particles", particles);
        computeShader.SetBuffer(kernelIndexUpdate, "Particles", particles);
        
        if (particleCount < size)
        {
            WARP_SIZE = Math.Min(Mathf.CeilToInt(0.003f / duration * size),size-particleCount);
            particleCount += WARP_SIZE;
            warpCount = Mathf.CeilToInt(WARP_SIZE / 256.0f);
            computeShader.SetFloat("count", particleCount);
            computeShader.Dispatch(kernelIndex, warpCount, 1, 1);
        }
        Debug.Log(dt);
        Debug.Log(particleCount);
        warpCount = Mathf.CeilToInt(particleCount / 256.0f);
        Debug.Log(warpCount);
        computeShader.Dispatch(kernelIndexUpdate, warpCount, 1, 1);
        _args[0] = (uint)instanceMesh.GetIndexCount(0);
        _args[1] = (uint)particleCount;
        _args[2] = (uint)instanceMesh.GetIndexStart(0);
        _args[3] = (uint)instanceMesh.GetBaseVertex(0);
        argsBuffer.SetData(_args);
        material.SetBuffer("Particles", particles);
        Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, material, new Bounds(Vector3.zero,new Vector3(100f,100f,100f)),argsBuffer);
    }

    void OnRenderObject()
    {
        material.SetPass(0);
    }

    void OnDestroy()
    {
        if (particles != null)
            particles.Release();
        if (argsBuffer != null)
            argsBuffer.Release();
    }
}
