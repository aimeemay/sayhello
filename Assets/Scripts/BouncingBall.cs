using UnityEngine;
using System.Collections;

public class BouncingBall: MonoBehaviour
{
    public float speed = 0.001f;

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

        if (col.gameObject.tag == "goals")
        {
            Destroy(this.gameObject);
            DestroyGameObjectsWithTag("goals");
        }




    }
}

