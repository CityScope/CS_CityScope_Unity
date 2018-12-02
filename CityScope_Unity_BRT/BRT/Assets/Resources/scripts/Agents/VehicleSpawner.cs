using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Utility;

public class VehicleSpawner : MonoBehaviour
{

	//vehicles varibles
	public float delaySecondsVehicle = 0.1f;
	public int randomFactor = 3;
	public GameObject agentPrefab;
	public List<WaypointCircuit> routes;
	//public bool boolMirrorVehicle = false;



	IEnumerator Start ()
	{
		while (true) {
			var checkResult = Physics.OverlapSphere (transform.position, 1); //make sure spawner is clear 
			if (checkResult.Length == 0) {
//				if (boolMirrorVehicle) {
//				agentPrefab.transform.rotation = Quaternion.Euler (0, 180f, 0);
//				agentPrefab.transform.localScale += new Vector3(1, 1, -1);
//
//				} else {
				var agent = Instantiate (agentPrefab, gameObject.transform.position, gameObject.transform.rotation, transform) as GameObject;
				var route = routes[0];
				agent.GetComponent<WaypointProgressTracker> ().circuit = route;

				//var rand = new System.Random ();
				//var idx = rand.Next (routes.Count);
				//var route = routes [idx];



			} else {

//				Debug.Log ("can't spawn due to collision");
			}
			yield return new WaitForSeconds (delaySecondsVehicle);
		}
	}
}