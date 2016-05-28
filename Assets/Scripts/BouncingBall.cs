using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BouncingBall: MonoBehaviour
{
    public float speed = 0.001f;
    public int personScore1 = 0;
    public int personScore2 = 0;
    public Text score1;
    public Text score2;

    // Use this for initialization
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {

    }
    float hitFactor(Vector2 ballPos, Vector2 racketPos,
        float racketHeight)
    {
        // ascii art:
        // ||  1 <- at the top of the racket
        // ||
        // ||  0 <- at the middle of the racket
        // ||
        // || -1 <- at the bottom of the racket
        return (ballPos.y - racketPos.y) / racketHeight;
    }



    public static void DestroyGameObjectsWithTag(string tag)
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject target in gameObjects)
        {
            GameObject.Destroy(target);
        }
    }

    

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "bubbleCollider")
        {
            
            // Calculate hit Factor
            float y = hitFactor(transform.position,
                col.transform.position,
                col.collider.bounds.size.y);

            // Calculate direction, make length=1 via .normalized
            Vector2 dir = new Vector2(-1, y).normalized;

            // Set Velocity with dir * speed
            GetComponent<Rigidbody2D>().velocity = dir * speed;
        }


        if (col.gameObject.name == "Goal1")
        {
            personScore1 += 1;
            //score1.text = personScore1.ToString();
            this.transform.position = new Vector3(0.22f, -1.54f, -4.71f);
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
            StartCoroutine(waitingCoroutine());
        }

        if (col.gameObject.name == "Goal2")
        {
            personScore2 += 1;
            //score1.text = personScore2.ToString();
            GetComponent<Rigidbody2D>().velocity = new Vector2(0,0);
            this.transform.position = new Vector3(0.22f, -1.54f, -4.71f);
            StartCoroutine(waitingCoroutine());
            
        }

        if (personScore1 >= 3 || personScore2 >= 3)
        {
                DestroyGameObjectsWithTag("ballgoals");
                //Destroy(this.gameObject);
                //DestroyGameObjectsWithTag("goals");
                personScore1 = 0;
                personScore2 = 0;
        }

        




    }
    IEnumerator waitingCoroutine(){
            this.GetComponent<CircleCollider2D>().enabled = false;
            Renderer renderer = this.GetComponent<Renderer>();
            renderer.material.color = Color.red;
            yield return new WaitForSeconds(2);
            renderer.material.color = Color.green;
            this.GetComponent<CircleCollider2D>().enabled = true;
        }

        public Text getScore1()
        {
            return  score1;
        }

        public Text getScore2()
        {
            return score2;
        }
}

