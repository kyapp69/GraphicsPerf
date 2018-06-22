using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;


public class ScreenCaptureEditorTool : EditorWindow
{
    string msg = "no error";

    [MenuItem("Window/SimpleScreenCapture")]
	public static void ShowWindow ()
	{
		EditorWindow.GetWindow (typeof(ScreenCaptureEditorTool));
	}

    public void Update()
    {
        Repaint();
    }

    void OnGUI () 
	{
		GUI.color = Color.cyan;
		GUILayout.Label ("ScreenCapture", EditorStyles.boldLabel);
        GUI.color = Color.white;
        GUILayout.Label ("Please enter playmode!", EditorStyles.boldLabel);
        GUILayout.Space(15);

		if (GUILayout.Button ("Make screenshot"))
			DoJob();

        //---------------------------------------------

        //Message
        
        GUILayout.Label( msg , EditorStyles.wordWrappedLabel);
        GUILayout.Space(15);
	}

	void DoJob()
	{
        GUI.color = Color.white;
        Scene scene = SceneManager.GetActiveScene();
        string path = "screenshot_"+scene.name+DateTime.Now.ToString("dd-mm-yyyy-hh-mm-ss")+".PNG";
        ScreenCapture.CaptureScreenshot(path);

        msg = "Saved at "+path;
	}

}