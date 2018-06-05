using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRendererControl : MonoBehaviour 
{
	public Transform target;
	//public int intervals = 100;
	
	public LineRenderer[] line;
	public int positionCount = 50;
	//private int count=0;
	private int currentpoint = 0;
	private Vector3 position;

	void Start()
	{
		//line = GetComponent<LineRenderer>();
	}

	void Update () 
	{
		//if(count >= intervals)
		//{
			if( currentpoint >=  positionCount ) currentpoint = 0;
			for (int i=0; i<line.Length;i++)
			{
				line[i].SetPosition(currentpoint, target.position);
			}
			
			//line.Simplify(0.5f);
			currentpoint ++;

			//count = 0;
		//}
		//else
		//{
		//	count++;
		//}
	}
}
