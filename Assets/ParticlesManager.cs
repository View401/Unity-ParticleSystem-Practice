﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
    ComputeBuffer particles,argsBuffer;

    const int WARP_SIZE = 256;

    public int size = 2560;
    int stride;

    int warpCount;

    int kernelIndex;
    int kernelIndexUpdate;
 
    uint[] _args = new uint[5] { 0, 0, 0, 0, 0 };
    // Use this for initialization
    void Start () {

        warpCount = Mathf.CeilToInt((float)size / WARP_SIZE);

        stride = Marshal.SizeOf(typeof(Particle));
        particles = new ComputeBuffer(size, stride);
        argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);


        kernelIndex = computeShader.FindKernel("emitter");
        kernelIndexUpdate = computeShader.FindKernel("Update");
        computeShader.SetBuffer(kernelIndex, "Particles", particles);

        
        computeShader.SetBuffer(kernelIndexUpdate, "Particles", particles);
        
        computeShader.SetFloats("cbStartColor",new float[]{ 0.0f, 0.0f, 0.5f, 1.0f});
        computeShader.SetFloats("cbEndColor", new float[]{ 0f, 1f, 0f, 1f});
        computeShader.SetFloat("cbStartSize", 2.5f);
        computeShader.SetFloat("cbEndSize", 0.5f);
        computeShader.SetInt("cbParticlesStorage", size);
        computeShader.SetFloat("cbLifeDecay", 0.3f);
        
        computeShader.SetInt("count", warpCount);
        computeShader.SetFloats("EmitterPosition", new float[] { transform.position.x, transform.position.y, transform.position.z, 1f });
        //computeShader.SetFloats("EmitterPosition", new float[] { -2f, 1f, 0f, 1f });
        computeShader.Dispatch(kernelIndex, warpCount, 1, 1);
        material.SetBuffer("Particles", particles);
        
    }
	
	// Update is called once per frame
	void Update () {

        computeShader.SetFloat("dt", Time.deltaTime);
        computeShader.SetFloats("EmitterPosition", new float[] { transform.position.x, transform.position.y, transform.position.z, 1f });
        material.SetFloat("_size", startSize);
        computeShader.Dispatch(kernelIndexUpdate, warpCount, 1, 1);
        _args[0] = (uint)instanceMesh.GetIndexCount(0);
        _args[1] = (uint)size;
        _args[2] = (uint)instanceMesh.GetIndexStart(0);
        _args[3] = (uint)instanceMesh.GetBaseVertex(0);
        argsBuffer.SetData(_args);
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
