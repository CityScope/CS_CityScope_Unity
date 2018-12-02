using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HideUIafter : MonoBehaviour
{
	public int dealy;
	//List for spawners and Targets
	public GameObject objToHide;

	void Awake ()
	{
		objToHide.SetActive (true);
	}

	IEnumerator Start ()
	{
		yield return new WaitForSeconds (dealy);
		objToHide.SetActive (false);

				
	}
}
	


