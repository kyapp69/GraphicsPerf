using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEngine.Profiling;
using UnityEngine.Rendering;

public class MiniProfiler : MonoBehaviour 
{
    public bool m_Enable = true;
	public Texture2D bktex;
    //Button
    bool btn_prev = false;
    bool btn_next = false;

    private int frameCount = 0;
    private const int kAverageFrameCount = 32;
    private float m_AccDeltaTime;
    private float m_AvgDeltaTime;
	private float m_CurrDeltaTimeIgnored;
    private float m_AccDeltaTimeIgnored;
    private float m_AvgDeltaTimeIgnored;

	//For my performance counting
	private int sampleCount;
    private int sampleSkipCount = 200;
    private int sampleDataCount = 500;
    private int sampleTotalCount;
	private float m_SampleAccDeltaTimeIgnored;
    private float m_SampleAvgDeltaTimeIgnored;
	private int sampleStatus = -1;




    internal class RecorderEntry
    {
        public bool ignore;
        public string name;
        public float time;
        public int count;
        public float avgTime;
        public float avgCount;
        public float accTime;
        public int accCount;
        public Recorder recorder;
    };

    RecorderEntry[] recordersList =
    {
        new RecorderEntry() { ignore = true, name="EditorOverhead" },
        new RecorderEntry() { ignore = false, name="Gfx.WaitForPresent" },
		new RecorderEntry() { ignore = true, name="WaitForTargetFPS" },
		new RecorderEntry() { ignore = true, name="GUI.Repaint" },
		//new RecorderEntry() { ignore = true, name="MiniProfiler.Update()" }
    };

    void Awake()
    {
		Application.targetFrameRate = 999;
        QualitySettings.vSyncCount = 0;
        Screen.SetResolution(1920, 1080, true);

        for (int i=0;i<recordersList.Length;i++)
        {
            var sampler = Sampler.Get(recordersList[i].name);
            if ( sampler != null )
            {
                recordersList[i].recorder = sampler.GetRecorder();
            }
        }

		sampleCount = 1;
		sampleTotalCount = sampleSkipCount + sampleDataCount;
		m_SampleAccDeltaTimeIgnored = 0;
		m_SampleAvgDeltaTimeIgnored = 0;
		sampleStatus = 0;
    }

    void Update()
    {
        if (m_Enable)
        {
            // get timing & update average accumulators
            for (int i = 0; i < recordersList.Length; i++)
            {
                recordersList[i].time = recordersList[i].recorder.elapsedNanoseconds / 1000000.0f;
                recordersList[i].count = recordersList[i].recorder.sampleBlockCount;
                recordersList[i].accTime += recordersList[i].time;
                recordersList[i].accCount += recordersList[i].count;
            }

            m_AccDeltaTime += Time.deltaTime;

			m_CurrDeltaTimeIgnored = Time.deltaTime * 1000.0f;
			for (int i = 0; i < recordersList.Length; i++)
            {
                if(recordersList[i].ignore) m_CurrDeltaTimeIgnored -= recordersList[i].time;
            }
			m_AccDeltaTimeIgnored += m_CurrDeltaTimeIgnored / 1000.0f;

			//My own sampling
			if(sampleCount > sampleSkipCount)
        	{
				if(sampleCount == sampleSkipCount + sampleDataCount) //Calculate Avg
				{
					m_SampleAvgDeltaTimeIgnored = m_SampleAccDeltaTimeIgnored / sampleDataCount;
					sampleStatus = 2;
				}
				else if(sampleCount < sampleSkipCount + sampleDataCount) //Do sampling
				{
					m_SampleAccDeltaTimeIgnored += m_CurrDeltaTimeIgnored / 1000.0f;
					sampleStatus = 1;
				}
			}

			sampleCount++;
            frameCount++;

            // time to time, update average values & reset accumulators
            if (frameCount >= kAverageFrameCount)
            {
                for (int i = 0; i < recordersList.Length; i++)
                {
                    recordersList[i].avgTime = recordersList[i].accTime * (1.0f / kAverageFrameCount);
                    recordersList[i].avgCount = recordersList[i].accCount * (1.0f / kAverageFrameCount);
                    recordersList[i].accTime = 0.0f;
                    recordersList[i].accCount = 0;
                }

                m_AvgDeltaTime = m_AccDeltaTime / kAverageFrameCount;
				m_AvgDeltaTimeIgnored = m_AccDeltaTimeIgnored / kAverageFrameCount;
                m_AccDeltaTime = 0.0f;
				m_AccDeltaTimeIgnored = 0.0f;
                frameCount = 0;
            }
        }

    }

    void OnGUI()
    {
        if (m_Enable)
        {
			GUI.skin.label.fontSize = 20;
			GUI.skin.box.normal.background = bktex;
			GUI.backgroundColor = new Color(0, 0, 0, .80f);
			GUI.color = new Color(1, 1, 1, 1);
            float w = 800, h = 50 + (recordersList.Length+25) * GUI.skin.label.fontSize + 8;
            GUILayout.BeginArea(new Rect(32, 50, w, h), GUI.skin.box);

            //Info ===============================================================
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(1, 1, 1, .80f);
            if(GUILayout.Button("Prev")) PrevScene();
            if(GUILayout.Button("Next")) NextScene();
			GUILayout.EndHorizontal();

            GUILayout.Label(
                "Screen name: "+SceneManager.GetActiveScene().name+"\n"+
                "Screen resolution: "+Screen.width+" x "+Screen.height+"\n"+
                "VSyncCount: "+QualitySettings.vSyncCount+"\n"
                );

			//FPS ================================================================
            GUI.skin.label.fontSize = 13;
			GUILayout.Label(
			"<b>Current FPS - Original</b> : "+ 
			System.String.Format( "{0:F2} FPS ({1:F2}ms)", 1.0f / Time.deltaTime, Time.deltaTime * 1000.0f) + "\n" +
			"    <b>Average FPS - Per "+CyanText(""+kAverageFrameCount)+" frames </b> : "+
			DarkCyanText( System.String.Format( "{0:F2} FPS ({1:F2}ms)", 1.0f / m_AvgDeltaTime, m_AvgDeltaTime * 1000.0f) )
			);

			//Real FPS============================================================
            GUI.skin.label.fontSize = 20;
			GUILayout.Label(
			"<b>Current FPS - Ignored*</b> : "+
			System.String.Format("{0:F2} FPS ({1:F2}ms)", 1.0f / (m_CurrDeltaTimeIgnored / 1000.0f), m_CurrDeltaTimeIgnored ) + "\n" +
			"    <b>Average FPS - Per "+CyanText(""+kAverageFrameCount)+" frames </b> : "+
			CyanText( System.String.Format( "{0:F2} FPS ({1:F2}ms)", 1.0f / m_AvgDeltaTimeIgnored, m_AvgDeltaTimeIgnored * 1000.0f) )
			);

			//IgnoreList note
			GUILayout.Label(GreyText("*Above FPS Ignored the following in grey color :"));
				
			string sName = "<b>Name</b>\n";
			string sAvgTime = "<b>AvgTime</b>\n";
			string sAvgCount = "<b>AvgCount</b>\n";
			string sTime = "<b>Time</b>\n";
			string sCount = "<b>Count</b>\n";
			string sPercent = "<b>Percent</b>\n";

            for (int i = 0; i < recordersList.Length; i++)
            {
                string colh = "<color=#fff>";
                string colt = "</color>";

                if(recordersList[i].ignore)
                {
                    colh = "";
                    colt = "";
                }

				sName       += colh + recordersList[i].name + colt +"\n";
				sAvgTime    += colh + System.String.Format("{0:F2}ms\n" + colt , recordersList[i].avgTime);
				sAvgCount   += colh + System.String.Format("{0:F2}\n" + colt , recordersList[i].avgCount);
				sTime       += colh + System.String.Format("{0:F2}ms\n" + colt , recordersList[i].time);
				sCount      += colh + System.String.Format("{0:F2}\n" + colt , recordersList[i].count);
				sPercent    += colh + System.String.Format("{0:F2}%\n" + colt , recordersList[i].avgTime / (m_AvgDeltaTime * 1000.0f) * 100.0f );
            }

			//GUILayout.Label(GreyText("Average numbers below are taken per "+kAverageFrameCount+" frames"));

			GUILayout.BeginHorizontal();

			GUILayout.Label(GreyText(sAvgTime));
			GUILayout.Label(GreyText(sAvgCount));
			GUILayout.Label(GreyText(sTime));
			GUILayout.Label(GreyText(sCount));
			GUILayout.Label(GreyText(sName));
			GUILayout.Label(GreyText(sPercent));

			GUILayout.EndHorizontal();
			

			//Memory =========================================================
			long num1 = UnityEngine.Profiling.Profiler.GetAllocatedMemoryForGraphicsDriver() / 1024 / 1024;
			long num2 = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024;
			long num3 = UnityEngine.Profiling.Profiler.GetTempAllocatorSize() / 1024 / 1024;
			long num4 = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong() / 1024 / 1024;
			long num5 = UnityEngine.Profiling.Profiler.GetTotalUnusedReservedMemoryLong() / 1024 / 1024;

			GUILayout.BeginHorizontal();
			GUILayout.Label(
				"Allocated Mem For GfxDriver\n"+
				"Total Allocated Mem\n"+
				"Temp Allocator Size\n"+
				"Total Reserved Mem\n"+
				"Total Unused Reserved Mem\n"
				);

			GUILayout.Label(
				num1+" mb\n"+
				num2+" mb\n"+
				num3+" mb\n"+
				num4+" mb\n"+
				num5+" mb\n"
				);

			GUILayout.Label(" \n \n \n \n ");
			GUILayout.Label(" \n \n \n \n ");
			GUILayout.Label(" \n \n \n \n ");
			GUILayout.EndHorizontal();

			//Sampling =================================================================
			switch(sampleStatus)
			{
				case 0: GUILayout.Label(RedText("skipping first "+ sampleSkipCount +" frames..."+sampleCount+"/"+sampleTotalCount)); break;
				case 1: GUILayout.Label(YellowText("now sampling " + sampleDataCount + " frames..."+sampleCount+"/"+sampleTotalCount)); break;
				case 2: GUILayout.Label(GreenText( System.String.Format( "{0:F2} FPS ({1:F2}ms)\n" + "Sampled Frames = "+sampleDataCount,
				1.0f / m_SampleAvgDeltaTimeIgnored, m_SampleAvgDeltaTimeIgnored * 1000.0f) )); break;
			}
			 
            GUILayout.EndArea();
        }
    }

    //========Scene Management============
    public void NextScene()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (sceneIndex < SceneManager.sceneCountInBuildSettings - 1)
            SceneManager.LoadScene(sceneIndex + 1);
        else
            SceneManager.LoadScene(0);
    }

    public void PrevScene()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (sceneIndex > 0)
            SceneManager.LoadScene(sceneIndex - 1);
        else
            SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 1);
    }

    //========Text Styles========
    private string DarkCyanText(string text)
    {
        return "<color=#099>" + text + "</color>";
    }
    private string CyanText(string text)
    {
        return "<color=#0ff>" + text + "</color>";
    }

    private string YellowText(string text)
    {
        return "<color=#ff0>" + text + "</color>";
    }

    private string RedText(string text)
    {
        return "<color=#f09>" + text + "</color>";
    }

    private string GreenText(string text)
    {
        return "<color=#0f0>" + text + "</color>";
    }

    private string GreyText(string text)
    {
        return "<color=#999>" + text + "</color>";
    }

    private string BooleanText(bool b)
    {
        if (b)
        {
            return "<color=#0f0>" + b.ToString() + "</color>";
        }
        else
        {
            return "<color=#f00>" + b.ToString() + "</color>";
        }
    }
}

