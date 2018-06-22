using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomObjects : MonoBehaviour 
{
	//public Texture[] texs;
	public Renderer[] renderers;

	void Start () 
	{
		Randomize();
	}
	
	[ContextMenu ("Update List")]
	public void UpdateList()
	{
		renderers = GetComponentsInChildren<Renderer>();
	}

	//For assigning properties
	//int texid = 0;
	float colid = 0;

	public void Randomize()
	{
		if(renderers==null) UpdateList();
		if(renderers==null) return;

		StartCoroutine("AssignMaterial");
	}

	IEnumerator AssignMaterial() 
	{
		for(int i = 0; i <renderers.Length; i++)
		{
			//Make instance material
			Material mat = renderers[i].material;
			mat.name = "Mat object "+i;

			//Random texture on material
			//if(texid >= texs.Length) texid = 0;
			//mat.SetTexture("_MainTex",texs[texid]);
			//texid++;

			//Random texture on material
			if(colid > 1.0f) colid = 0;
			Color col = Color.HSVToRGB( colid , 1.0f , 1.0f );
			mat.SetColor("_Color", col );
			colid+=0.05f;
		}
		yield return null;
	}

}
