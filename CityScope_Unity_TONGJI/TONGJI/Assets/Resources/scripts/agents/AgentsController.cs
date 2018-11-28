using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;

public class AgentsController : MonoBehaviour
{

	private GameObject _spwanerParent;
	public int _dieAfter = 10;
	private NavMeshAgent _thisAgentNavMesh;
	public float _timeAtAmenity;
	private Color _thisColor;
	public int _target_id;



	void Start ()
	{
		//Destroy (gameObject, _dieAfter);
		StartCoroutine (killAnyway()); 


	}

	void OnTriggerEnter (Collider other)
	{
		if (other.gameObject.tag == "amenity") {
//			_thisAgentNavMesh = this.GetComponent<NavMeshAgent> ();
			if (other.gameObject.GetComponent<TargetController> ().GetID () == _target_id) {
				StartCoroutine (killAfterTime ());
			} 
		}
	}

	IEnumerator killAnyway () // killed after x time at target with Tag y 
	{
		yield return new WaitForSeconds (_dieAfter);
		Destroy (gameObject);

	}


	IEnumerator killAfterTime () // killed after x time at target with Tag y 
	{
//		_thisAgentNavMesh.Stop ();
//		if (this.GetComponent<NavMeshAgent> ().velocity.x < 1 && this.GetComponent<NavMeshAgent> ().velocity.z < 1) { //really stopped 

			gameObject.tag = "StoppedAgent"; 
//		}
		yield return new WaitForSeconds (_timeAtAmenity);
		Destroy (gameObject);

	}

}

