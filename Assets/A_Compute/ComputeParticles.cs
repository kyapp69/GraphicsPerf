using UnityEngine;
using System.Collections;

public class ComputeParticles : MonoBehaviour 
{
	struct Particle
	{
		public Vector3 position;
	};

	public int warpCount = 5;
	public Material material;
	public ComputeShader computeShader;

	private const int warpSize = 32;
	private ComputeBuffer particleBuffer;
	private ComputeBuffer argsBuffer;	
	private uint[] args = new uint[4] { 0, 0, 0, 0 };
	private int particleCount;
	private Particle[] plists;

	void Start () 
	{
		particleCount = warpCount * warpSize;
		
		// Init particles
		plists = new Particle[particleCount];
		for (int i = 0; i < particleCount; ++i)
		{
            plists[i].position = Random.insideUnitSphere * 4f;
        }
		
		//Set data to buffer
		particleBuffer = new ComputeBuffer(particleCount, 12); // 12 = sizeof(Particle)
		particleBuffer.SetData(plists);
		
		//Set buffer to computeShader and Material
		computeShader.SetBuffer(0, "particleBuffer", particleBuffer);
		material.SetBuffer ("particleBuffer", particleBuffer);

		//Args
		argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        args[0] = (uint)1; //vertex count per instance
        args[1] = (uint)particleCount; //instance count
		args[2] = (uint)0; //start vertex location
		args[3] = (uint)0; //start instance location
        argsBuffer.SetData(args);
	}

	void Update () 
	{
		computeShader.Dispatch(0, warpCount, 1, 1);
	}

	void OnRenderObject()
	{
		material.SetPass(0);
		Graphics.DrawProceduralIndirect(MeshTopology.Points,argsBuffer);
	}

	void OnDestroy()
	{
		particleBuffer.Release();
		argsBuffer.Release();
	}
}
