using UnityEngine;
using System.Collections;

public class rotatingBubble : MonoBehaviour {
	public float rotSpeed = 10f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate (Vector3.forward, rotSpeed * Time.deltaTime);
	}
}
