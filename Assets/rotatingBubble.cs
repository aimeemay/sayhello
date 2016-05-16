using UnityEngine;
using System.Collections;

public class rotatingBubble : MonoBehaviour {
	public float rotSpeed = 10f;
    public Color tenSecondColor = Color.red;
    private int seconds;
    
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate (Vector3.forward, rotSpeed * Time.deltaTime);
        bubbleColorChanger();
	}

     private void bubbleColorChanger(){
        seconds = Mathf.FloorToInt(Time.time);
        if (seconds >= 9 && seconds <= 9.1){
            tenSecondColor = Color.Lerp(Color.red, Color.blue, Mathf.PingPong(Time.time, 1));
            Renderer renderer = this.GetComponent<Renderer>();
            renderer.material.color = tenSecondColor;
            this.transform.localScale += new Vector3(0.005f, 0.005f, 0);
            rotSpeed += 1f;

        }
    }

}