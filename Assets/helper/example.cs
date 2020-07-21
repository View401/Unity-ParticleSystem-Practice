using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;   //没有这个Marshal会出错
using UnityEngine;

public class example : MonoBehaviour
{
    public struct AllParticles
    {

    }
    public Texture2D initTexture;
    public ComputeShader computeShader;
    public Material material;
    public Mesh instanceMesh;
    public struct Particle
    {
        public Vector3 position;
        public Vector3 CustomPos;
        public Vector3 color;
        public Vector2 uv;
    }
    const int WARP_SIZE = 256;
    int width, height, size;
    int warpCount;
    int stride;
    float lerpt;
    public float time = 4f;
    public float psize = 0.05f;
    ComputeBuffer particles,argsBuffer;
    private uint[] _args;
    // Start is called before the first frame update
    void Start()
    {
        if (initTexture != null)
        {
            width = initTexture.width;
            height = initTexture.height;
            size = width * height;
        }
        Particle[] initBuffer = new Particle[size];
        warpCount = Mathf.CeilToInt((float)size / WARP_SIZE);
        stride = Marshal.SizeOf(typeof(Particle));   //为什么不是sizeof()
        particles = new ComputeBuffer(size, stride);

        _args = new uint[5] { 0, 0, 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                int id = i * height + j;
                float x = (float)i / (width-1);
                float y = (float)j / (height-1);
                initBuffer[id].position = new Vector3((x - 0.5f), (y - 0.5f), 0f);  //坐标空间[-1.1]
                initBuffer[id].CustomPos = new Vector3((x - 0.5f), (y - 0.5f), 0f);
                initBuffer[id].uv = new Vector2(x, y);
            }
            particles.SetData(initBuffer);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;
        time -= dt;
        transform.Rotate(new Vector3(0f, dt * 15f, 0f));
        if (time < 0)
        {
            lerpt = Mathf.Lerp(lerpt, 1f, 0.008f);
        }
        updateBuffer();
        argsBuffer.SetData(_args);
        Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, material, new Bounds(Vector3.zero, new Vector3(100f, 100f, 100f)), argsBuffer);
    }
    void updateBuffer()
    {
        int kernelId = computeShader.FindKernel("CSMain");
        computeShader.SetFloat("_Time", time);
        computeShader.SetBuffer(kernelId, "particles", particles);        //应该可以只传一次
        computeShader.Dispatch(kernelId, warpCount, 1, 1);
        //我不懂args用处
        _args[0] = (uint)instanceMesh.GetIndexCount(0);
        _args[1] = (uint)size;
        _args[2] = (uint)instanceMesh.GetIndexStart(0);
        _args[3] = (uint)instanceMesh.GetBaseVertex(0);

        material.SetBuffer("particles", particles);
        material.SetMatrix("GameobjectMatrix", transform.localToWorldMatrix);
        material.SetFloat("size", size);
        material.SetFloat("lerpt", lerpt);
    }
    void OnDestroy()
    {
        if (particles != null)
            particles.Release();
        if (argsBuffer != null)
            argsBuffer.Release();
    }
}
