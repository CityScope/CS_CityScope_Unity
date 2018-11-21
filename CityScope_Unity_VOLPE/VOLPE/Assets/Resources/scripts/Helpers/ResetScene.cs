using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetScene : MonoBehaviour
{

	public float _resetAfterMinutes;

	void Start ()
	{
		StartCoroutine (resetThisScene ());
	}

	IEnumerator resetThisScene ()
	{
		yield return new WaitForSeconds (_resetAfterMinutes*60);
		Scene scene = SceneManager.GetActiveScene ();
		SceneManager.LoadScene (scene.name);
	}

}