using UnityEngine;
using System.Collections;

public class rotatingBubble : MonoBehaviour {
	public float rotSpeed = 10f;
    public Color tenSecondColor = Color.red;
    private int seconds;
    
	// Use this for initialization
	void Start () {
		seconds = Mathf.FloorToInt(Time.time);
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate (Vector3.forward, rotSpeed * Time.deltaTime);
        bubbleColorChanger();
	}

     private void bubbleColorChanger(){
        

		if (Time.time - seconds >= 5 && Time.time - seconds <= 8){
			tenSecondColor = Color.Lerp(Color.blue, Color.magenta, Mathf.PingPong(Time.time, 3));
            Renderer renderer = this.GetComponent<Renderer>();
            renderer.material.color = tenSecondColor;
            this.transform.localScale += new Vector3(0.005f, 0.005f, 0);
            rotSpeed += 1f;

		}if (Time.time - seconds > 8 && Time.time - seconds <= 11) {

			this.transform.localScale -= new Vector3 (0.005f, 0.005f, 0);

		}
    }

}