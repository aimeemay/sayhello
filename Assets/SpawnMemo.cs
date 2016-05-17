using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpawnMemo : MonoBehaviour {
    public Text funFacto;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}



    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "bubbleCollider")
        {
            this.gameObject.SetActive(false);
            SetFactText();
        }
    }


    void SetFactText()
    {
        funFacto.text = "Oreo's are more addictive than coke";
    }
}
