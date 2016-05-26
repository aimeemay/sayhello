using UnityEngine;
using System.Collections;

public class rotatingBubble : MonoBehaviour {
	public float rotSpeed = 10f;
    //public Color tenSecondColor = Color.red;
    private int seconds;
    private float rate = 0.025f;
    private float ratio = 0.15f;
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
        

		if (Time.time - seconds >= 5){
			//tenSecondColor = Color.Lerp(Color.blue, Color.red, Mathf.PingPong(Time.time, 5));
   //         Renderer renderer = this.GetComponent<Renderer>();
   //         renderer.material.color = tenSecondColor;
            //this.transform.localScale += new Vector3(0.001f, 0.001f, 0);
            this.transform.localScale = PingPong(Time.time * rate, ratio, 0.2f)* Vector3.one ;
            rotSpeed += 0.2f;
        }if (Time.time - seconds > 10) {
            rotSpeed = 40f;
            //this.transform.localScale = new Vector3(0.15f, 0.15f, 0);
			//this.transform.localScale -= new Vector3 (0.005f, 0.005f, 0);

		}
    }

     float PingPong(float aValue, float aMin, float aMax)
     {
         return Mathf.PingPong(aValue, aMax - aMin) + aMin;
     }

}