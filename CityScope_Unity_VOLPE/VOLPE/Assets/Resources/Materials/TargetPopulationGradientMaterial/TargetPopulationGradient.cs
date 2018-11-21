using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPopulationGradient : MonoBehaviour
{
	Renderer render;

	public	GameObject targetManager;


	// Use this for initialization
	void OnEnable () //due to restart in stateManager -- start won't work! 
	{

		render = gameObject.GetComponent<Renderer> ();
		NewUpdate (); 

	}

	// Update is called once per frame
	void NewUpdate ()
	{
			render.sharedMaterial.SetFloatArray ("_TargetValues", targetManager.GetComponent<TargetsManager> ().GetTargetsValuesArray ());
			render.sharedMaterial.SetVectorArray ("_TargetPositions", targetManager.GetComponent<TargetsManager> ().GetTargetsPositionsArray ());
	}
}
