using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour {

	public float _penaltyMultiplyer;
	public float _penaltyWeight;


	float[] targetsValuesArray = new float[13];
	Vector4[] targetsPositionsArray = new Vector4[13];

	double sync_penalty(int p, int m, int r) {
		double median = (p + m + r) / 3.0;
		double sd = ((p - median)*(p - median) + (m - median)*(m - median) + (r - median)*(r - median))/3.0;
		return sd / -22.222222222222225;
	}

	double gradient_ratio(int p, int m, int r, double penalty_multiplyer, double penalty_weight) {
		double ratio = ((1 - penalty_weight) * (p+m+r) * 3 + penalty_weight * penalty_multiplyer * sync_penalty(p, m, r)) / 15.0;
		if (ratio > 0) {
			ratio = ratio * 0.6 + 0.4;
		}
		if (ratio > 1.0) {
			ratio = 1.0;
		}
				return ratio;
	}

	// Use this for initialization
	void Start () {
		for (int i = 0; i < 13; i++) {
			targetsPositionsArray[transform.GetChild (i).gameObject.GetComponent<TargetController> ().GetID ()] = transform.GetChild (i).transform.position;
			//print (transform.GetChild (i).gameObject.GetComponent<TargetController> ().GetID ());
		}
	}
	


	public float[] GetTargetsValuesArray () {
		return targetsValuesArray;
	}

	public Vector4[] GetTargetsPositionsArray () {
		return targetsPositionsArray;
	}

	public void UpdateTargetArray( int id, int p, int m, int r) {
		targetsValuesArray [id] = (float)gradient_ratio(p, m, r, _penaltyMultiplyer, _penaltyWeight);
		//print (id);
		//print (value);
	}
}
