using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpawnMemo : MonoBehaviour {
    public Text funFacto;
	public GameObject videoBox;
	private bool exists = false;
	public GameObject[] randomObjects;
	public int index;
	public Vector3 offset = new Vector3 (0, 0, 0);
	// Use this for initialization
	void Start () {
		index = Random.Range (0, randomObjects.Length);
	}
	
	// Update is called once per frame
	void Update () {
	
	}



    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "bubbleCollider")
        {
            //this.gameObject.SetActive(false);
            //SetFactText();
			if (!exists) {
				Debug.Log ("Im running");
				GameObject video = (GameObject)Instantiate (randomObjects[index], this.transform.position, Quaternion.identity);
				exists = true;
			}
        }


    }


    void SetFactText()
    {
        funFacto.text = "Oreo's are more addictive than coke";
    }
}
