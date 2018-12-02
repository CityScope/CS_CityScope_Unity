using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetScene : MonoBehaviour
{
    public float _resetAfterSeconds;

	void Start ()
	{
		StartCoroutine (resetThisScene ());
	}

	IEnumerator resetThisScene ()
	{
		yield return new WaitForSeconds (_resetAfterSeconds );
       
      	webcam.Stop();

        Scene scene = SceneManager.GetActiveScene ();
		SceneManager.LoadScene (scene.name);
        
       // webcam.Play();

		Debug.Log ("Reset scene.");
    }

    void OnApplicationQuit()
    {
        Debug.Log("Quitting.");
        webcam.Stop();
    }
}