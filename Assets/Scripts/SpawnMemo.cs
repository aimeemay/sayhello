using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpawnMemo : MonoBehaviour {
    public Text funFacto;
	public GameObject videoBox;
	public bool exists = false;
	public GameObject[] randomObjects;
	public int index;
	public Vector3 offset = new Vector3 (0, 0, 0);
    public GameObject video;
    private int seconds;
    private float timeLeft = 5f;
    public Text text;
    // Use this for initialization
    void Start () {
		index = Random.Range (0, randomObjects.Length);
        
        text.text = Mathf.Round(timeLeft).ToString();
        gameObject.SetActive(true);
    }
	
	// Update is called once per frame
	void Update () {
        
    }
    


    void OnTriggerStay2D(Collider2D col)
    {   
        
        if (col.gameObject.tag == "bubbleCollider")
        {
            
            timeLeft -= Time.deltaTime;
            text.text = Mathf.Round(timeLeft).ToString();
            if (timeLeft < 0)
            {
                Destroy(this.gameObject);

                text.text = null;
                //SetFactText();
                if (!exists)
                {
                    video = (GameObject)Instantiate(randomObjects[index], this.transform.position, Quaternion.identity);
                    exists = true;
                }
            }
            
        }


    }


    void SetFactText()
    {
        funFacto.text = "Oreo's are more addictive than coke";
    }

   

    
}
