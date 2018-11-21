using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

public class HeatMapMesh: MonoBehaviour
{
	private int _random;


	public GameObject _cityIoObj;
	public GameObject _reciever;
	public GameObject _targetsParent;
	public List <Transform> TargetsList;
	public  List<GameObject> _heatMapPixels = new List<GameObject> ();
	private Collider[] _radiusColliders;
	private int _agentsAtTarget;
	RaycastHit _hitInfo;




	void OnEnable ()
	{
		TargetsList = _targetsParent.GetComponentsInChildren<Transform> ().Skip (1).ToList (); //move to update for constant scan of list of points 
		transform.GetComponent<Renderer> ().shadowCastingMode = ShadowCastingMode.Off;
		tag = "heatmap"; 
		NewUpdate (); 

	}

	void NewUpdate ()
	{
		Mesh _recieverMesh = _reciever.GetComponent<MeshFilter> ().mesh;
		Mesh _heatmapMesh = GetComponent<MeshFilter> ().mesh;


			
		foreach (var i in TargetsList) {
			TargetController _targetsVars = i.gameObject.GetComponent<TargetController> (); //get vars of rich, poor, med from other script 
			_agentsAtTarget = (_targetsVars._medium + _targetsVars._poor + _targetsVars._rich); //should show more specific response to types !!

			if (Physics.Raycast (i.transform.position, Vector3.up, out _hitInfo, Mathf.Infinity)) {
				//Debug.DrawRay (i.transform.position, _hitInfo.collider.transform.position);
				//Debug.DrawRay (i.transform.position, new Vector3 (i.transform.position.x, _hitInfo.collider.transform.position.y , i.transform.position.z));

				var _meshCollider = _hitInfo.collider as MeshCollider;
				if (_meshCollider != null) {
					var index = _hitInfo.triangleIndex * 3;
					var hit1 = _recieverMesh.vertices [_recieverMesh.triangles [index]];
					//var hit2 = mesh.vertices [mesh.triangles [index + 1]];
					//var hit3 = mesh.vertices [mesh.triangles [index + 2]];

					Vector3[] vertices = _recieverMesh.vertices;
					int[] triangles = _recieverMesh.triangles;

					int x = 0;
					while (x < vertices.Length) {
						if (vertices [x].x == hit1.x && vertices [x].z == hit1.z && _agentsAtTarget > 0)
							//vertices [x] = new Vector3  (hit1.x, transform.localScale.x / _agentsAtTarget  , hit1.z);
							vertices [x] =  Vector3.Lerp (new Vector3(hit1.x,hit1.y,hit1.z),
								new Vector3 (hit1.x, transform.localScale.x / _agentsAtTarget,hit1.z), .1f);

						x++;
				

						_recieverMesh.vertices = vertices;
						_recieverMesh.RecalculateBounds ();

						_heatmapMesh.vertices = vertices;
						_heatmapMesh.RecalculateBounds ();

					}
				}
			}
		}
	}
}
