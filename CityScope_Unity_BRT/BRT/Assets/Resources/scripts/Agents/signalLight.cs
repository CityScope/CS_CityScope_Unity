using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class signalLight : MonoBehaviour
{
	public int dealy;
	public GameObject groupA;
	public GameObject groupB;
	private Vector3 startLocationA;
	private Vector3 startLocationB;

	void Awake ()
	{

		Vector3 startLocationA = groupA.transform.position;
		Vector3 startLocationB = groupB.transform.position;
	}

	IEnumerator Start ()
	{
		while (true) {
	

			groupA.transform.Translate (startLocationA);
			groupB.transform.Translate (startLocationB.x, startLocationB.y -50,startLocationB.z);

			yield return new WaitForSeconds (dealy);

			groupA.transform.Translate (startLocationA);
			groupB.transform.Translate (startLocationB.x, startLocationB.y +50,startLocationB.z);
			yield return new WaitForSeconds (dealy * 2);

			groupA.transform.Translate (startLocationA.x, startLocationA.y -50,startLocationA.z);
			groupB.transform.Translate (startLocationB);

			yield return new WaitForSeconds (dealy);

			groupA.transform.Translate (startLocationA.x, startLocationA.y +50,startLocationA.z);
			groupB.transform.Translate (startLocationB);


		}
				
	}
}
	


