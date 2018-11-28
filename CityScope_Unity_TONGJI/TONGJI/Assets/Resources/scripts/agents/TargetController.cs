using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class TargetController : MonoBehaviour
{
	public	int _id;
	public  int _poor;
	public  int _medium;
	public  int _rich;


	void OnTriggerEnter (Collider other)
	{
		if (other.gameObject.tag == "Poor") {

			if (_poor >= 15) {
				_poor = 0;
			} else {
				_poor += 1;
			
			}

		} else if (other.gameObject.tag == "Rich") {
			
			if (_rich >= 15) {
				_rich = 0;
			} else {
				_rich += 1;

			}

		}
		if (other.gameObject.tag == "Medium") {


			if (_medium >= 15) {
				_medium = 0;
			} else {
				_medium += 1;

			}

		} else {
			return; 
		} 


		this.transform.parent.GetComponent<TargetManager>().UpdateTargetArray(_id, _poor, _medium, _rich);

	}

//	void Update() {
//		this.transform.parent.GetComponent<TargetManager>().UpdateTargetArray(_id, _poor, _medium, _rich);
//	}

	public int GetID() {
		return _id;
	}
}
	
