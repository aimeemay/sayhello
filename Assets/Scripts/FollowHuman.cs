using UnityEngine;
using System.Collections;

public class FollowHuman : MonoBehaviour {
    public float moveSpeed = 0.50f;
    private Vector3 humanPosition;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 offset = new Vector3(0, -0.5f, 0);
        humanPosition = GameObject.Find("PlayerPlaceholder").transform.position;
        
        transform.position = Vector2.Lerp(transform.position + offset, humanPosition, moveSpeed);
	}
}
