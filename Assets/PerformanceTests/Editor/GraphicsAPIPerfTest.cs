using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Unity.PerformanceTesting;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

public class GraphicsAPIPerfTest
{
    SampleGroupDefinition[] m_definitions =
    {
        new SampleGroupDefinition("Gfx.WaitForPresent"),
        new SampleGroupDefinition("Render.OpaqueGeometry"),
		new SampleGroupDefinition("ParticleSystem.Draw"),
		new SampleGroupDefinition("Render.LineOrTrail"),
		new SampleGroupDefinition("RenderDeferred.Lighting"),
		new SampleGroupDefinition("Shadows.PrepareShadowmap"),
		new SampleGroupDefinition("CullResults.CreateSharedRendererScene"),
		new SampleGroupDefinition("Culling"),
		new SampleGroupDefinition("DestroyCullResults"),
		new SampleGroupDefinition("Compute.Dispatch")
    };

	SampleGroupDefinition m_allocatedGfx = new SampleGroupDefinition("TotalAllocatedMemoryForGraphicsDriver", SampleUnit.Megabyte);
	SampleGroupDefinition m_allocated = new SampleGroupDefinition("TotalAllocatedMemory", SampleUnit.Megabyte);
	SampleGroupDefinition m_reserved = new SampleGroupDefinition("TotalReservedMemory", SampleUnit.Megabyte);

	
    [PerformanceUnityTest, Version("1")]
    public IEnumerator Measure_Profiler([ValueSource("ScenesToTest")]string sceneName)
    {
	    SceneManager.LoadScene(sceneName);
	    yield return null;
	    
	    // sample memory before run
	    Measure.Custom(m_allocatedGfx, Profiler.GetAllocatedMemoryForGraphicsDriver() / 1048576f);
	    Measure.Custom(m_allocated, Profiler.GetTotalAllocatedMemoryLong() / 1048576f);
	    Measure.Custom(m_reserved, Profiler.GetTotalReservedMemoryLong() / 1048576f);
	    
	    // sample frame time and profiler markers
        yield return Measure.Frames()
		.ProfilerMarkers(m_definitions)
		.MeasurementCount(200)
		.Run();
	    
	    // sample memory after run
	    Measure.Custom(m_allocatedGfx, Profiler.GetAllocatedMemoryForGraphicsDriver() / 1048576f);
	    Measure.Custom(m_allocated, Profiler.GetTotalAllocatedMemoryLong() / 1048576f);
	    Measure.Custom(m_reserved, Profiler.GetTotalReservedMemoryLong() / 1048576f);
    }


	private static string[] ScenesToTest()
	{
		return new[]
		{
			"Compute",
			"DynamicVBO",
			"Lights",
			"Materials",
			"Objects",
			"Particles",
			"ParticlesCollision",
			"Physics",
			"Tessellation"
		};
	}
}
