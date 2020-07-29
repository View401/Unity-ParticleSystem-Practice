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

    Mesh mesh;

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
    int lastUsedParticle;
    int initSize;
    float initduration;
    float emitterPerSec;
    float residuals;
    uint[] _args = new uint[5] { 0, 0, 0, 0, 0 };
    void Awake()
    {
        Application.targetFrameRate = 60;
    }
    // Use this for initialization
    void Start () {

        //warpCount = Mathf.CeilToInt((float)size / WARP_SIZE);

        stride = Marshal.SizeOf(typeof(Particle));
       // startColor = new Color(0,0,1,1);
        //endColor = Color.red;
        size = 2560;
        initSize = size;
        gravity = 0.5f;
        startSpeed = 1;
        lifeDecay = 1;
        duration = 5;
        residuals=0;
        lastUsedParticle = 0;
        particleCount = 0;
        particles = new ComputeBuffer(size, stride);
        argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);


        kernelIndex = computeShader.FindKernel("emitter");
        kernelIndexUpdate = computeShader.FindKernel("Update");

        computeShader.SetBuffer(kernelIndex, "Particles", particles);
        computeShader.SetBuffer(kernelIndexUpdate, "Particles", particles);
        //computeShader.SetFloat("cbEndSize", 0.5f);
        material.SetBuffer("Particles", particles);
        
        

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        //Mesh mesh;
        mesh = new Mesh();
        float width = 1, height = 1;
        Vector3[] vertices = new Vector3[4]
        {
        new Vector3(0,0,0),
        new Vector3(width,0,0),
        new Vector3(0,height,0),
        new Vector3(width,height,0)
        };
        mesh.vertices = vertices;

        int[] tris = new int[6]
        {
            0,2,1,
            2,3,1
        };
        mesh.triangles = tris;

        Vector3[] normals = new Vector3[4]
        {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };

        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0,0),
            new Vector2(1,0),
            new Vector2(0,1),
            new Vector2(1,1)
        };
        mesh.uv = uv;
        meshFilter.mesh = mesh;
    }
	
	// Update is called once per frame
	void Update () {
        if (size != initSize)
        {
            initSize = size;
            particles = new ComputeBuffer(size, stride);
            lastUsedParticle = 0;
            particleCount = 0;
            residuals = 0;
        }
        computeShader.SetInt("lastUsedParticle", lastUsedParticle);
      
        float dt = Time.deltaTime;
        emitterPerSec = size / duration * lifeDecay;
        residuals += dt * emitterPerSec;
        int newPaticles = Mathf.FloorToInt(residuals);
        residuals -= newPaticles;
        //Debug.Log(newPaticles);
        computeShader.SetFloat("dt", dt);
        computeShader.SetFloats("EmitterPosition", new float[] { transform.position.x, transform.position.y, transform.position.z, 1f });
        computeShader.SetFloat("gravity", gravity);
        computeShader.SetFloat("startSpeed", startSpeed);
        computeShader.SetFloats("cbStartColor", new float[] { startColor.r, startColor.g, startColor.b, startColor.a });
        computeShader.SetFloats("cbEndColor", new float[] { endColor.r, endColor.g, endColor.b, endColor.a });
        computeShader.SetFloat("cbStartSize", startSize);
        computeShader.SetFloats("forceCenter", forceCenter);
        computeShader.SetFloat("cbLifeDecay", lifeDecay);
        computeShader.SetFloat("duration", duration);            //重新发射
        computeShader.SetInt("size", size);
        
        computeShader.SetInt("count", newPaticles);

        if (newPaticles > 0) {
            if (particleCount < size)
            {
                particleCount +=  Mathf.Min(newPaticles,size-particleCount);
          
            }
           // Debug.Log("lastUsedParticles:" + lastUsedParticle);
            lastUsedParticle = (lastUsedParticle + newPaticles ) % size;
            
            warpCount = Mathf.CeilToInt(newPaticles / 256.0f);
            computeShader.Dispatch(kernelIndex, warpCount, 1, 1);
        }

      //  Debug.Log(dt);
       // Debug.Log("ParticleCount "+particleCount);
        warpCount = Mathf.CeilToInt(particleCount / 256.0f);
        computeShader.SetInt("cbParticlesStorage", particleCount);
        if(particleCount>0)
            computeShader.Dispatch(kernelIndexUpdate, warpCount, 1, 1);

        //_args[0] = (uint)instanceMesh.GetIndexCount(0);
        _args[0] = (uint)mesh.GetIndexCount(0);
        _args[1] = (uint)size;
        //_args[2] = (uint)instanceMesh.GetIndexStart(0);
        //_args[3] = (uint)instanceMesh.GetBaseVertex(0);
        _args[2] = (uint)mesh.GetIndexStart(0);
        _args[3] = (uint)mesh.GetBaseVertex(0);
        argsBuffer.SetData(_args);
        //Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, material, new Bounds(Vector3.zero,new Vector3(100f,100f,100f)),argsBuffer);
        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, new Bounds(Vector3.zero, new Vector3(100f, 100f, 100f)), argsBuffer);
        // Debug.Log(lastUsedParticle);
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
