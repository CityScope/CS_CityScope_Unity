using UnityEngine;
using System.Collections;

public class VehicleController : MonoBehaviour
{

	public Transform target;

	public float speed = 0.8f;
	public float angularSpeed = 0.5f;
	public float secondsOnStop = 3f;

	private bool moving = true;

	void OnTriggerEnter (Collider other)
	{

		if (other.gameObject.tag == "kill") {
			Destroy (gameObject);
		} 

		if (other.gameObject.tag == "StopSignal") {
			moving = false;
		}

		if (other.CompareTag ("BusStop")) {
			StartCoroutine (BusStopAnimation ());
		} 
		
		if (other.gameObject.tag == "vehicle") {
			StartCoroutine (moveAfterAwhile ());
		}
				
	}


	void OnTriggerExit (Collider other)
	{
		moving = true;
	}

	IEnumerator moveAfterAwhile () //very dirty result
	{
		moving = false;
		yield return new WaitForSeconds (1f);
		moving = true;	
	}


	IEnumerator BusStopAnimation ()
	{
		moving = false;
		yield return new WaitForSeconds (secondsOnStop);
		moving = true;

	}



	void Update ()
	{
		if (moving) {
			gameObject.transform.position = Vector3.MoveTowards (gameObject.transform.position, target.position, speed * Time.deltaTime);
			var targetAngle = Mathf.Atan2 (target.position.x, target.position.z) * Mathf.Rad2Deg;

			var targetQuat = Quaternion.Euler ((target.transform.rotation.eulerAngles - target.transform.parent.localRotation.eulerAngles));
			gameObject.transform.rotation = Quaternion.RotateTowards (gameObject.transform.rotation, targetQuat, Time.deltaTime * angularSpeed);
		}
	}
}
