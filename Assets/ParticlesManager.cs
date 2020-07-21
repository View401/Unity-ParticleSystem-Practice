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
        
        //public Vector4 ambient;
        public Vector2 ageAndlife;
        public float psize;
    };

    public ComputeShader computeShader;
    public Material material;
    [SerializeField]
    public Mesh instanceMesh;

    ComputeBuffer particles;

    const int WARP_SIZE = 256;

    int size = 2560;
    int stride;

    int warpCount;

    int kernelIndex;
    int kernelIndexUpdate;
    Particle[] initBuffer;

    // Use this for initialization
    void Start () {

        warpCount = Mathf.CeilToInt((float)size / WARP_SIZE);

        stride = Marshal.SizeOf(typeof(Particle));
        particles = new ComputeBuffer(size, stride);

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
        computeShader.SetFloats("cbEndColor", new float[]{ 1f, 0f, 0f, 1f});
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

        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    particles.SetData(initBuffer);
        //}
        //computeShader.SetInt("shouldMove", Input.GetMouseButton(0) ? 1 : 0);
        //for(int i = 0; i < size; i++)
        //{
        //    if (particles[i].life < 0)
        //    {
        //        initBuffer[i].position = Vector2.zero;
        //        initBuffer[i].velocity = Random.insideUnitCircle;
        //        initBuffer[i].life = 1.0f;
        //    }
        //}
        //var mousePosition = GetMousePosition();
        //computeShader.SetFloats("mousePosition", mousePosition);
        //computeShader.SetBuffer(kernelIndexUpdate, "Particles", particles);

        computeShader.SetFloat("dt", Time.deltaTime);
        computeShader.Dispatch(kernelIndexUpdate, warpCount, 1, 1);
        //Graphics.DrawProceduralNow(MeshTopology.Points, 1, size);
        //Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, material, new Bounds(Vector3.zero,new Vector3(100f,100f,100f)),particles);
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
        Graphics.DrawProceduralNow(MeshTopology.Points, 1, size);
        //Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, material, new Bounds(Vector3.zero, new Vector3(100f, 100f, 100f)), particles);
    }

    void OnDestroy()
    {
        if (particles != null)
            particles.Release();
    }
}
