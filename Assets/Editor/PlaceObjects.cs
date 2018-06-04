using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;


public class PlaceObjects : EditorWindow
{

	Vector2 scrollPosition;
	bool[]  myfunctionlist         = new bool[20];
    bool    hasError               = false;
    string  errorMsg               = "no error";
    Transform[] objs;

	bool    togglegroup1           = false;
    bool    togglegroup2           = false;
    bool    togglegroup3           = false;

    Vector3 tg1_size               = Vector3.one; //-1 means no limit
    Vector3 tg1_amount             = new Vector3(10,10,10);
    int     tg1_anchorX            = 1;
    int     tg1_anchorY            = 1;
    int     tg1_anchorZ            = 1;
    string[] tg1_anchorlist        = new string[] 
    {
        "Negative", //0
        "Center", //1
        "Positive", //2
    };

    Vector3 tg1_ran_pos_min        = Vector3.zero;
    Vector3 tg1_ran_pos_max        = Vector3.zero;
    Vector3 tg1_ran_rot_min        = Vector3.zero;
    Vector3 tg1_ran_rot_max        = Vector3.zero;
    Vector3 tg1_ran_sca_min        = Vector3.zero;
    Vector3 tg1_ran_sca_max        = Vector3.zero;

    bool individualselection = false;

    [MenuItem("Cubic/Place Objects Tool")]
	public static void ShowWindow ()
	{
		EditorWindow.GetWindow (typeof(PlaceObjects));
	}

    public void Update()
    {
        Repaint();
    }

    void OnGUI () 
	{
        //***************
        scrollPosition = GUILayout.BeginScrollView(scrollPosition,GUILayout.Width(0),GUILayout.Height(0));
		//***************

		GUI.color = Color.cyan;
		GUILayout.Label ("Place Objects", EditorStyles.boldLabel);
		GUILayout.Label ("In Hierarchy, "+
        "select a parent GameObject (= all children) "+
        "or select muliple GameObjects (= only these objects)", EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("Random for 'Distribute Evenly' = small variations", EditorStyles.miniLabel);
        GUI.color = Color.white;
        GUILayout.Space(15);

        togglegroup1 = EditorGUILayout.BeginToggleGroup ("Distribute Evenly", togglegroup1); // Untick for free random
            EditorGUI.indentLevel++;
            //EditorGUILayout.LabelField("Put -1 in Area means unlimited", EditorStyles.miniLabel);
            tg1_size = EditorGUILayout.Vector3Field("Area", tg1_size);
            tg1_amount = EditorGUILayout.Vector3Field("Amount", tg1_amount);
            EditorGUILayout.LabelField("Anchor");
        //Rect r = EditorGUILayout.BeginHorizontal();
            tg1_anchorX = EditorGUILayout.Popup("X", tg1_anchorX, tg1_anchorlist);
            tg1_anchorY = EditorGUILayout.Popup("Y", tg1_anchorY, tg1_anchorlist);
            tg1_anchorZ = EditorGUILayout.Popup("Z", tg1_anchorZ, tg1_anchorlist);
        //EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel--;
        EditorGUILayout.EndToggleGroup();
        GUILayout.Space(15);

        EditorGUILayout.LabelField("Random Range", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField("Position");
            EditorGUI.indentLevel++;
            tg1_ran_pos_min = EditorGUILayout.Vector3Field("Min", tg1_ran_pos_min);
            tg1_ran_pos_max = EditorGUILayout.Vector3Field("Max", tg1_ran_pos_max);
            EditorGUI.indentLevel--;
        EditorGUILayout.LabelField("Rotation");
            EditorGUI.indentLevel++;
            tg1_ran_rot_min = EditorGUILayout.Vector3Field("Min", tg1_ran_rot_min);
            tg1_ran_rot_max = EditorGUILayout.Vector3Field("Max", tg1_ran_rot_max);
            EditorGUI.indentLevel--;
        EditorGUILayout.LabelField("Scale");
            EditorGUI.indentLevel++;
            tg1_ran_sca_min = EditorGUILayout.Vector3Field("Min", tg1_ran_sca_min);
            tg1_ran_sca_max = EditorGUILayout.Vector3Field("Max", tg1_ran_sca_max);
            EditorGUI.indentLevel--;
        EditorGUI.indentLevel--;
		GUILayout.Space (15);

        //Add mesh collider ray casting?

        Color original = GUI.backgroundColor;
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button ("Place Objects"))
			DoJob();
        GUI.backgroundColor = original;

        if (GUILayout.Button("Reset Random Numbers"))
            ResetRandom();

        if (GUILayout.Button("Reset Selection Transforms"))
            ResetTransform();

        //---------------------------------------------
        individualselection = Selection.gameObjects.Length>1;

        //Stat and Error
        GUI.color = Color.cyan;
        if(Selection.activeTransform != null)
        {
            GUILayout.Label("Stats", EditorStyles.boldLabel);
            if(individualselection)
            {
                GUILayout.Label("Objects Selected : " + Selection.gameObjects.Length, EditorStyles.wordWrappedLabel);
            }
            else
            {
                GUILayout.Label("No. Of Children Objects : " + Selection.activeTransform.childCount, EditorStyles.wordWrappedLabel);
            }
        }
        GUI.color = Color.red;
        if(hasError)
        {
            GUILayout.Label("Error", EditorStyles.boldLabel);
            GUILayout.Label(errorMsg, EditorStyles.wordWrappedLabel);
        }
        GUILayout.Space(15);

        //***************
		GUILayout.EndScrollView();
		//***************
	}

    void SetSelectionObjects()
    {
        if(individualselection)
        {
            int size = Selection.gameObjects.Length;
            objs = new Transform[size];
            for(int i=0; i<objs.Length; i++)
            {
                objs[i] = Selection.gameObjects[i].transform;
            }
            
        }
        else
        {
            objs = Selection.activeTransform.GetComponentsInChildren<Transform>();
        }
    }

    void ResetRandom()
    {
        tg1_ran_pos_min = Vector3.zero;
        tg1_ran_pos_max = Vector3.zero;
        tg1_ran_rot_min = Vector3.zero;
        tg1_ran_rot_max = Vector3.zero;
        tg1_ran_sca_min = Vector3.one;
        tg1_ran_sca_max = Vector3.one;
    }

    void ResetTransform()
    {
        SetSelectionObjects();

        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i] != Selection.activeTransform)
            {
                objs[i].localPosition = Vector3.zero;
                objs[i].localEulerAngles = Vector3.zero;
                objs[i].localScale = Vector3.one;
            }
        }
    }

    void DoJob()
	{
        // Prevent negative area
        if (
            (tg1_size.x <= 0) ||
            (tg1_size.y <= 0) ||
            (tg1_size.z <= 0)
          )
        {
            hasError = true;
            errorMsg = "Please put positive number for Area";
            return;
        }
        else
        {
            hasError = false;
        }

        // Prevent negative amount
        if (
            tg1_amount.x <= 0 ||
            tg1_amount.y <= 0 ||
            tg1_amount.z <= 0
          )
        {
            hasError = true;
            errorMsg = "Please put positive integer number for Amount";
            return;
        }
        else
        {
            hasError = false;

            tg1_amount.x = Mathf.Ceil(tg1_amount.x);
            tg1_amount.y = Mathf.Ceil(tg1_amount.y);
            tg1_amount.z = Mathf.Ceil(tg1_amount.z);
        }

        //-----------------------------------------------------------------------------

        SetSelectionObjects();

        //Prevent no object selected
        if (objs.Length <= 0)
        {
            hasError = true;
            errorMsg = "No object is selected in Hierarchy";
            return;
        }
        else
        {
            hasError = false;
        }

        //Temp
        Vector3 temp_p = Vector3.zero;
        Vector3 temp_r = Vector3.zero;
        Vector3 temp_s = Vector3.one;

        //Spacing
        Vector3 spacing;
        spacing.x = tg1_size.x / (tg1_amount.x-1+0.001f);
        spacing.y = tg1_size.y / (tg1_amount.y-1+0.001f);
        spacing.z = tg1_size.z / (tg1_amount.z-1+0.001f);

        //Coordinate
        Vector3 coordinate = Vector3.zero;
        //
        Vector3 coordinate_max = Vector3.zero;

        if (togglegroup1)
        {
            //Position Evenly
            for (int i = 0; i < objs.Length; i++)
            {
                if(objs[i] != Selection.activeTransform)
                {
                    temp_p.x = coordinate.x * spacing.x;
                    temp_p.y = coordinate.y * spacing.y;
                    temp_p.z = coordinate.z * spacing.z;
                    objs[i].localPosition = temp_p;

                    //Small Variations
                    Vector3 t;
                    t = objs[i].localPosition;
                    objs[i].localPosition = RandomV3(t, t + tg1_ran_pos_min, t + tg1_ran_pos_max);
                    t = objs[i].localEulerAngles;
                    objs[i].localEulerAngles = RandomV3(t, t + tg1_ran_rot_min, t + tg1_ran_rot_max);
                    t = objs[i].localScale;
                    objs[i].localScale = RandomV3(t, tg1_ran_sca_min, tg1_ran_sca_max);

                    if (coordinate.x < tg1_amount.x - 1)
                    {
                        coordinate.x++;
                        coordinate_max.x = Mathf.Max(coordinate_max.x, coordinate.x);
                    }
                    else
                    {
                        coordinate.x = 0;
                        if (coordinate.y < tg1_amount.y - 1)
                        {
                            coordinate.y++;
                            coordinate_max.y = Mathf.Max(coordinate_max.y, coordinate.y);
                        }
                        else
                        {
                            coordinate.y = 0;
                            coordinate.z++;
                            coordinate_max.z = Mathf.Max(coordinate_max.z, coordinate.z);
                        }
                    }
                }
            }
            //Move to Anchor
            coordinate.x++;
            coordinate.y++;
            coordinate.z++;

            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i] != Selection.activeTransform)
                {
                    Vector3 t;
                    t = objs[i].localPosition;

                    switch (tg1_anchorX)
                    {
                        case 0: break;
                        case 1: t.x -= (coordinate_max.x*spacing.x) / 2; break;
                        case 2: t.x -= (coordinate_max.x * spacing.x); break;
                    }
                    switch (tg1_anchorY)
                    {
                        case 0: break;
                        case 1: t.y -= (coordinate_max.y * spacing.y) / 2; break;
                        case 2: t.y -= (coordinate_max.y * spacing.y); break;
                    }
                    switch (tg1_anchorZ)
                    {
                        case 0: break;
                        case 1: t.z -= (coordinate_max.z * spacing.z) / 2; break;
                        case 2: t.z -= (coordinate_max.z * spacing.z); break;
                    }

                    objs[i].localPosition = t;
                }
            }
        }
        else
        {
            //Transform Randomly
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i] != Selection.activeTransform)
                {
                    objs[i].localPosition = RandomV3(objs[i].localPosition, tg1_ran_pos_min, tg1_ran_pos_max);
                    Vector3 rot = RandomV3(objs[i].localEulerAngles, tg1_ran_rot_min, tg1_ran_rot_max);
                    objs[i].localRotation = Quaternion.Euler(rot.x, rot.y, rot.z);
                    objs[i].localScale = RandomV3(objs[i].localScale, tg1_ran_sca_min, tg1_ran_sca_max);
                }
            }
        }
    }
    //==========
    private Vector3 RandomV3(Vector3 v, Vector3 rmin, Vector3 rmax)
    {
        v.x = UnityEngine.Random.Range(rmin.x, rmax.x);
        v.y = UnityEngine.Random.Range(rmin.y, rmax.y);
        v.z = UnityEngine.Random.Range(rmin.z, rmax.z);
        return v;
    }
    //==========

}