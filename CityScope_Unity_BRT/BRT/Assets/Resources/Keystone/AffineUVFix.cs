using System;
using UnityEditor;
using UnityEngine;

public class AffineUVFix : MonoBehaviour {

    public Vector3[] vertices;
	public bool onoffkeystone = false;
    public int selectedCorner = 0;

    private bool keystonevisible;

    private GameObject[] corners;

    void Start () {
	    Mesh mesh = new Mesh();
	    mesh.vertices = new Vector3[4];
	    mesh.triangles = new int[] {0,1,2, 0,2,3};		
	    GetComponent<MeshFilter>().mesh = mesh;

        corners = new GameObject[vertices.Length];
        for(int i = 0; i < vertices.Length; i++)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.transform.SetParent(transform);
            obj.GetComponent<Renderer>().material.color = i == selectedCorner ? Color.green : Color.red;
            corners[i] = obj;
        }
        keystonevisible = true;
    }

    void Update () {
	    Mesh mesh = GetComponent<MeshFilter>().mesh;
	    mesh.vertices = vertices;
		
	    // Zero out the left and bottom edges, 
	    // leaving a right trapezoid with two sides on the axes and a vertex at the origin.
	    var shiftedPositions = new Vector2[] {
		    Vector2.zero,
		    new Vector2(0, vertices[1].y - vertices[0].y),
		    new Vector2(vertices[2].x - vertices[1].x, vertices[2].y - vertices[3].y),
		    new Vector2(vertices[3].x - vertices[0].x, 0)
	    };
	    mesh.uv = shiftedPositions;
	
	    var widths_heights = new Vector2[4];
	    widths_heights[0].x = widths_heights[3].x = shiftedPositions[3].x;
	    widths_heights[1].x = widths_heights[2].x = shiftedPositions[2].x;
	    widths_heights[0].y = widths_heights[1].y = shiftedPositions[1].y;
	    widths_heights[2].y = widths_heights[3].y = shiftedPositions[2].y;
	    mesh.uv2 = widths_heights;

		//if (onoffkeystone != keystonevisible){
			//onoffkeystone = !onoffkeystone;	
		onOffObjects(onoffkeystone);// toggles onoff at each click
        if(onoffkeystone)
        {
            OnSceneControl();
           // Debug.Log("Yes!!");
        }
		//} 

	}

    private void OnSceneControl()
    {
        if (!onoffkeystone)
            return;
        //Debug.Log("Yes!!");


        var corner = Input.GetKeyDown("1") ? 0 : Input.GetKeyDown("2") ? 1 : Input.GetKeyDown("3") ? 2 : Input.GetKeyDown("4") ? 3 : selectedCorner;
        if(corner != selectedCorner)
        {
            corners[selectedCorner].GetComponent<Renderer>().material.color = Color.red;
            corners[corner].GetComponent<Renderer>().material.color = Color.green;
            selectedCorner = corner;
        }

        float speed = 0.5f;
        if (Input.GetKey(KeyCode.LeftShift))
            speed *= 10;
        else if (Input.GetKey(KeyCode.LeftAlt))
            speed *= 0.1f;

        var v = vertices[selectedCorner];

        if (Input.GetKeyDown(KeyCode.UpArrow))
            v = v + speed * Vector3.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            v = v + speed * Vector3.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            v = v + speed * Vector3.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            v = v + speed * Vector3.right;

        vertices[selectedCorner] = v;
        
    }

    private void onOffObjects(bool visible)
    {
			
		for (int i=0; i < vertices.Length; i++)
		{
			//print ("point position: " + transform.TransformPoint(vertices[i]));
			GameObject sphere = corners[i];
			sphere.transform.position = transform.TransformPoint(vertices[i]);
            sphere.SetActive(visible);
            //sphere.GetComponent<MeshRenderer> ().material.color = Color.red;
			//Destroy (sphere, 0.1f);

		}
        keystonevisible = visible;
	}
}