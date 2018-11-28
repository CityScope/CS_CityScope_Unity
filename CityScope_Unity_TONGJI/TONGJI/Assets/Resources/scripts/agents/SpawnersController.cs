using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class SpawnersController : MonoBehaviour
{

	//Vars here
	public float delaySpwan = 1f;
	public float startAgents = 50;
	private GameObject agent;

	public GameObject agentPrefab;
	private UnityEngine.AI.NavMeshAgent agentNavMesh;


	public GameObject spawnParent;
	public GameObject targetParent;
	public  List <Transform> SpwanLocations;
	public  List <Transform> TargetLocations;


	private Renderer rend;

	void Awake ()
	{
		// go through all children of spawn parent and add them to a list 
		SpwanLocations = spawnParent.GetComponentsInChildren<Transform> ().Skip (1).ToList ();
		TargetLocations = targetParent.GetComponentsInChildren<Transform> ().Skip (1).ToList ();

		// hide the spawn/target objects on start 
		for (int i = 0; i < SpwanLocations.Count; i++) {
			rend = SpwanLocations [i].GetComponent<Renderer> ();
			rend.enabled = false;	
		}

		for (int i = 0; i < TargetLocations.Count; i++) {
			rend = TargetLocations [i].GetComponent<Renderer> ();
			rend.enabled = false;	
		}

		//first deplyment at start
		for (int i = 0; i < startAgents; i++) {
			spwanerMethod ();
		}
	}

	IEnumerator Start ()
	{

		while (true) {
			yield return new WaitForSeconds (delaySpwan);
			spwanerMethod (); //call creation method
		}

	}

	void spwanerMethod ()
	{

		// creating the agent //
		Transform randomSpwanLocation = SpwanLocations [Random.Range (0, SpwanLocations.Count)];
		agent = GameObject.Instantiate 
			(agentPrefab, randomSpwanLocation.position, randomSpwanLocation.rotation, transform) as GameObject; //make the agent
		UnityEngine.AI.NavMeshAgent agentNavMesh = agent.GetComponent<UnityEngine.AI.NavMeshAgent> ();


		// Target alocation // 

		var value = 100 * Random.value;
		if (value < 30) { //spwan 

			Transform randomTarget = TargetLocations [Random.Range (0, TargetLocations.Count / 3)];
			agentNavMesh.SetDestination (randomTarget.position);
			agentNavMesh.GetComponent<Renderer> ().material.color = Color.red;
			agent.GetComponent <AgentsController> ()._target_id = randomTarget.gameObject.GetComponent <TargetController> ().GetID ();
			agent.tag = "Rich"; 

		} else if (value > 30 && value < 60) { //or to random targets 

			Transform randomTarget = TargetLocations [Random.Range (TargetLocations.Count / 3, ((TargetLocations.Count * 60) / 100))];
			agentNavMesh.SetDestination (randomTarget.position);
			agentNavMesh.GetComponent<Renderer> ().material.color = Color.yellow;
			agent.GetComponent <AgentsController> ()._target_id = randomTarget.gameObject.GetComponent <TargetController> ().GetID ();
			agent.tag = "Medium";

		} else if (value > 60 && value < 100) { //or to random targets 

			Transform randomTarget = TargetLocations [Random.Range (((TargetLocations.Count * 60) / 100), TargetLocations.Count)];
			agentNavMesh.SetDestination (randomTarget.position);
			agentNavMesh.GetComponent<Renderer> ().material.color = Color.green;
			agent.GetComponent <AgentsController> ()._target_id = randomTarget.gameObject.GetComponent <TargetController> ().GetID ();
			agent.tag = "Poor";

		
		}
	}
}
	