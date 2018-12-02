using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AgentsController : MonoBehaviour
{

	void OnTriggerEnter (Collider other)
	{

		if (other.gameObject.tag == "StopSignal") {
			
			gameObject.GetComponent<UnityEngine.AI.NavMeshAgent> ().Stop ();

			if (other.gameObject.tag == "kill") {
				Destroy (gameObject);
			}
	
		}

		if (other.gameObject.tag == "vehicle") {
			StartCoroutine (moveAfterAwhile ());
		}
	}

	void OnTriggerExit (Collider other)
	{
		//print ("yes!");
		gameObject.GetComponent<UnityEngine.AI.NavMeshAgent> ().Resume ();
	}

	IEnumerator moveAfterAwhile () //very dirty result
	{
		gameObject.GetComponent<UnityEngine.AI.NavMeshAgent> ().Stop ();
		yield return new WaitForSeconds (2f);
		gameObject.GetComponent<UnityEngine.AI.NavMeshAgent> ().Resume ();
	}
}
