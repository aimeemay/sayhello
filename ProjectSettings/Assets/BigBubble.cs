using UnityEngine;
using System.Collections.Generic;


public class BigBubble : MonoBehaviour {
	public GameObject bubblePrefab;
	//public GameObject bubbleControllerObject;
	//private BubbleController bubbleController;

	public GameObject textCollide;
	GameObject collidingText;
	GameObject bigBubble;
	// Use this for initialization
	void Start () {
		//bubbleController = bubbleControllerObject.GetComponent<BubbleController> ();
	}

	// Update is called once per frame
	void Update () {
		
	}


	private List<string> collisions = new List<string>();


//	void OnTriggerEnter2D(Collider2D col)
//	{	
//		if (col.gameObject.tag == "bubbleCollider") {
//			collisions.Add (col.gameObject.name);
//
//			if (collisions.Count == 1) {
//		
//
//
//			
//				Vector3 size = new Vector3 (0.08f, 0.08f, 0);
//
//
//				collidingText = (GameObject)Instantiate (textCollide, transform.position, Quaternion.identity);
//				bigBubble = (GameObject)Instantiate (bubblePrefab, transform.position, Quaternion.identity);
//				collidingText.transform.localScale = size;
//			
//			}
//		}
//
//	}
//
//
//	void OnTriggerExit2D(Collider2D col)
//	{
//		Destroy(bigBubble);
//		Destroy(collidingText);
//		collisions.Remove (col.gameObject.name);
//
//	}


}
