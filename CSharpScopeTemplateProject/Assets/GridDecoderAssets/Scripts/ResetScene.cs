using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetScene : MonoBehaviour
{
    public float _resetAfterSeconds;

	void Start ()
	{
		StartCoroutine (ResetThisScene ());
	}

	IEnumerator ResetThisScene ()
	{
		yield return new WaitForSeconds (_resetAfterSeconds );
       
      	Webcam.Stop();

        Scene scene = SceneManager.GetActiveScene ();
		SceneManager.LoadScene (scene.name);

		EventManager.TriggerEvent ("reload");
		Debug.Log ("Reset scene.");
    }

    void OnApplicationQuit()
    {
        Debug.Log("Quitting.");
        Webcam.Stop();
    }
}