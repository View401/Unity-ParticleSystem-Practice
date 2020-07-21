using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ParticlesManager : MonoBehaviour {
    public struct Particle
    {
        public Vector4 position;
        public Vector4 color;
        public Vector4 initialPosition;
        public Vector4 initialVelocity;
        public Vector4 velocity;
        public Vector2 ageAndlife;
        public float psize;
    };

    public ComputeShader computeShader;
    public Material material;
    [SerializeField]
    public Mesh instanceMesh;
    [Range(0.001f, 10f)]
    public float startSize = 0.001f;
    ComputeBuffer particles,argsBuffer;

    const int WARP_SIZE = 256;

    public int size = 2560;
    int stride;

    int warpCount;

    int kernelIndex;
    int kernelIndexUpdate;
    Particle[] initBuffer;
    uint[] _args = new uint[5] { 0, 0, 0, 0, 0 };
    // Use this for initialization
    void Start () {

        warpCount = Mathf.CeilToInt((float)size / WARP_SIZE);

        stride = Marshal.SizeOf(typeof(Particle));
        particles = new ComputeBuffer(size, stride);
        argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        initBuffer = new Particle[size];
        //    //initBuffer[i].position = Random.insideUnitCircle * 10f;

        for (int i = 0; i < size; i++)
        {
            initBuffer[i] = new Particle();
            initBuffer[i].position = new Vector4(0,0,0,1);
            //    //initBuffer[i].velocity = Vector2.zero;
            //    initBuffer[i].velocity = Random.insideUnitCircle;
            initBuffer[i].ageAndlife.x= Random.Range(0.0f, 1.0f);
            initBuffer[i].ageAndlife.y = 1.0f + Random.Range(0.0f, 1.0f);
            initBuffer[i].color = new Vector4(1, 1, 1, 1);
        }

        //particles.SetData(initBuffer);

        kernelIndex = computeShader.FindKernel("emitter");
        kernelIndexUpdate = computeShader.FindKernel("Update");
        computeShader.SetBuffer(kernelIndex, "Particles", particles);
        
        //computeShader.SetFloat("count", size);

        //_HighSpeedValue("High speed Value", Range(0, 50)) = 25
        
        computeShader.SetBuffer(kernelIndexUpdate, "Particles", particles);
        
        computeShader.SetFloats("cbStartColor",new float[]{ 0.0f, 0.0f, 0.5f, 1.0f});
        computeShader.SetFloats("cbEndColor", new float[]{ 0f, 1f, 0f, 1f});
        computeShader.SetFloat("cbStartSize", 2.5f);
        computeShader.SetFloat("cbEndSize", 0.5f);
        computeShader.SetInt("cbParticlesStorage", size);
        computeShader.SetFloat("cbLifeDecay", 0.3f);
        
        computeShader.SetInt("count", warpCount);
        computeShader.SetFloats("EmitterPosition", new float[] { -2f, 1f, 0f, 1f });
        //Debug.Log("kernelIndexUpdate:" + kernelIndexUpdate);
        computeShader.Dispatch(kernelIndex, warpCount, 1, 1);
        material.SetBuffer("Particles", particles);
        
    }
	
	// Update is called once per frame
	void Update () {

        computeShader.SetFloat("dt", Time.deltaTime);
        material.SetFloat("_size", startSize);
        computeShader.Dispatch(kernelIndexUpdate, warpCount, 1, 1);
        _args[0] = (uint)instanceMesh.GetIndexCount(0);
        _args[1] = (uint)size;
        _args[2] = (uint)instanceMesh.GetIndexStart(0);
        _args[3] = (uint)instanceMesh.GetBaseVertex(0);
        argsBuffer.SetData(_args);
        Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, material, new Bounds(Vector3.zero,new Vector3(100f,100f,100f)),argsBuffer);
    }

    //float[] GetMousePosition()
    //{
    //    //var mp = Input.mousePosition;
    //    //var v = Camera.main.ScreenToWorldPoint(mp);
    //    return new float[] { 0, 0 };
    //}

    void OnRenderObject()
    {
        material.SetPass(0);
        //Graphics.DrawProceduralNow(MeshTopology.Points, 1, size);
        //Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, material, new Bounds(Vector3.zero, new Vector3(100f, 100f, 100f)), argsBuffer);
    }

    void OnDestroy()
    {
        if (particles != null)
            particles.Release();
        if (argsBuffer != null)
            argsBuffer.Release();
    }
}
