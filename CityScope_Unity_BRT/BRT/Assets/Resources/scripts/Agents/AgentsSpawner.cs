using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class AgentsSpawner : MonoBehaviour
{

	//Vars here
	public float delaySpwan = 0.1f;
	public GameObject agentPrefab;
	private UnityEngine.AI.NavMeshAgent agentNavMesh;
	private List<GameObject> agents;
	public Transform ThisStateGoal;
	[Range (0, 100)]
	public int GoToStationPrecentage;
	public int LeaveStationAfterTime = 25;

	//List for spawners and Targets
	[System.Serializable]
	public class spwanLocation
	{
		public Transform Spwaner;

	}

	public List<Transform> SpwanLocations;


	// spwaner function
	IEnumerator Start ()
	{
		agents = new List<GameObject> ();
		while (true) {
			Transform randomLocation = SpwanLocations [Random.Range (0, SpwanLocations.Count)];
			var agent = GameObject.Instantiate (agentPrefab, randomLocation.position, randomLocation.rotation, transform) as GameObject;
			UnityEngine.AI.NavMeshAgent agentNavMesh = agent.GetComponent<UnityEngine.AI.NavMeshAgent> ();
			var value = 100 * Random.value;
			if (value < GoToStationPrecentage) { //spwan to BRT
				agentNavMesh.SetDestination (ThisStateGoal.position);
				agentNavMesh.GetComponent<Renderer> ().material.color = Color.yellow;
				agent.tag = "AgentBRT";
			} else { //or to random targets 
				Transform randomTarget = SpwanLocations [Random.Range (0, SpwanLocations.Count)];
				agentNavMesh.SetDestination (randomTarget.position);
				agentNavMesh.GetComponent<Renderer> ().material.color = Color.white;
				agent.tag = "Untagged";
			}
			agents.Add (agent);
			yield return new WaitForSeconds (delaySpwan);
		}
	}

	//runs every frame to test for agent's goal and location
	void Update ()
	{
		for (int i = 0; i < agents.Count; i++) {
			var thisagent = agents [i];
			UnityEngine.AI.NavMeshAgent tempNav = thisagent.GetComponent<UnityEngine.AI.NavMeshAgent> ();
			float dist = tempNav.remainingDistance; 
			if (dist != Mathf.Infinity && dist > 0f && tempNav.hasPath && dist < 1f) { //check if agent is on its way, has path and close to destination
				if (thisagent.CompareTag ("AgentBRT")) {
					thisagent.tag = "Untagged";
					StartCoroutine (InStation (tempNav)); //passes tempVav to Corutine so it can test proximity to station
				} else {
					DestroyObject (thisagent);
					agents [i] = null;
				}
			}

		}
		agents.RemoveAll (a => a == null);
	}

	IEnumerator InStation (UnityEngine.AI.NavMeshAgent agent)
	{
		Transform randomTarget = SpwanLocations [Random.Range (1, SpwanLocations.Count)];
		agent.SetDestination (randomTarget.position);
        agent.Stop();
		yield return new WaitForSeconds (LeaveStationAfterTime);
        agent.Resume ();
		agent.GetComponent<Renderer> ().material.color = Color.red;
	}

}
	

