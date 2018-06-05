using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCollision : MonoBehaviour 
{
	void Start () 
	{
		this.GetComponent<Rigidbody> ().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
	}

}
