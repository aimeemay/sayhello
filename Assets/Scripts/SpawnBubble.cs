using UnityEngine;
using System.Collections;

public class SpawnBubble : MonoBehaviour {

    public GameObject bubble;
    public GameObject spawn;
    public GameObject Floor;
    
	// Use this for initialization
	void Start () {
        //Instantiate(spawn, transform.position + offset, Quaternion.identity);
        
	}
	
	// Update is called once per frame
	void Update () {

       
	}


    void OnTriggerEnter2D(Collider2D col)
    {
        
        if (col.gameObject.tag == "Floor")
        {
            
            
                bubble = (GameObject)Instantiate(spawn, transform.position, Quaternion.identity);
          


        }
                
    }

    void OnTriggerExit2D(Collider2D col)
    {
        Destroy(bubble);
        
    }


}
