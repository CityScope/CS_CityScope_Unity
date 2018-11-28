using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPopulationGradient : MonoBehaviour
{
	Renderer render;

	public	GameObject targetManager;


	// Use this for initialization
	void Start ()
	{

		render = gameObject.GetComponent<Renderer> ();


	}

	// Update is called once per frame
	void Update ()
	{
		render.sharedMaterial.SetFloatArray ("_TargetValues", targetManager.GetComponent<TargetManager>().GetTargetsValuesArray ());
		render.sharedMaterial.SetVectorArray ("_TargetPositions", targetManager.GetComponent<TargetManager> ().GetTargetsPositionsArray ());
	}
}
