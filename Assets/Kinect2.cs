using UnityEngine;

using System.Collections.Generic;

using Windows.Kinect;
using System;
using System.Linq;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

public class Kinect2 : MonoBehaviour
{
	private KinectSensor kSensor;
	private BodyFrameReader bodyReader;
	private bool initialised;
	private int numberOfBodies;
	private Body[] bodies;
	public Dictionary<ulong, GameObject> persons;
	public GameObject playerPlaceholder;
	public GameObject biggerBubble;
	private GameObject BiggestBubble;
	private float xvalues;
	private float yvalues;
	public Sprite[] sprites;
    public Color32[] colors;
    public float duration = 3.0f;
    private int index = 0;
    private float timer = 0.0f;
    private Color32 currentColor;
    private Color32 startColor;
    public GameObject walkhere;
    public GameObject walkiePrefab;
    public float rotSpeed = 10f;
    //Counts where the line is in relation to its end point
    //public float counter = 0f;
    //public float[][] counterArray;



    //List Of bubbles In the game.
    public List<GameObject> bubblesInGame;


    // Use this for initialisation
    void Start ()
	{
		initialised = false;

    }

	void Update ()
	{
		if (initialised == false) {
			//start the kinect initialised
			StartKinect ();
		} else {
			//All done per tick of game run
			//update kinnect for every tick if initialised
			UpdateKinect ();
			//Converts the dictionary of people into list for detecting bubbles in game
			dictionaryConverter ();
            //Clear Lines
            clearLines();
            //Calculates distance between two pairs, can do this for multiple pairs. 
            distanceStuff (); 
			//resetCounters();
            //Clears the dead bubbles from game if the bubbles(people are not in game anymore)
            clearDeadBubbles();
            //Check if high fiving
            checkHighFive();
            //check if Hand Shaking
            checkHandShake();
            //removes walk here text
            RemoveWalkHere();

            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene("FirstPrototypeScene");
            }

        }
	}

	//Starts up the kinect. checks if sensors is working if not open it check for body sources and add any bodies to the dictionary. 
	private void StartKinect ()
	{
		kSensor = KinectSensor.GetDefault ();
		if (kSensor != null) {
			bodyReader = kSensor.BodyFrameSource.OpenReader ();
			if (kSensor.IsOpen == false) {
				kSensor.Open ();
				numberOfBodies = kSensor.BodyFrameSource.BodyCount;
				bodies = new Body[numberOfBodies];
				persons = new Dictionary<ulong, GameObject> ();
				initialised = true;

                //Setting counter for lines
               /* counterArray = new float[6][];
                for(int i=0; i<6; i++)
                {
                    counterArray[i] = new float[6];
                    for(int k=0; k<6; k++)
                    {
                        counterArray[i][k] = 0f;
                    }
                }*/
			}
		}
	}

	//Take latest frames if body reader is empty, get body data and refresh it. Get tracked ids of people detected. 
	private void UpdateKinect ()
	{
		if (bodyReader != null) {
			BodyFrame frame = bodyReader.AcquireLatestFrame ();
			if (frame == null) {
				return;
			}

			// You need to use the same body data array in each frame.
			frame.GetAndRefreshBodyData (bodies);

			// You need to make sure to dispose of the frame otherwise you can't get the next one.
			frame.Dispose ();
			frame = null;

			// Get the ids of the bodies that are currently being tracked.
			// Bodies are not guaranteed to be loaded in the same location frame to frame.
			List<ulong> trackedIDs = new List<ulong> ();
			foreach (Body body in bodies) {
				if (body == null) {
					continue;
				}
				if (body.IsTracked) {
					
					trackedIDs.Add (body.TrackingId);
				}
			}

			List<ulong> knownIDs = new List<ulong> (persons.Keys);

			// Delete persons that are no longer tracked
			foreach (ulong trackingID in knownIDs) {
				if (trackedIDs.Contains (trackingID) == false) {
					Destroy (persons [trackingID]);
					persons.Remove (trackingID);
				}
			}

			foreach (Body body in bodies) {
				if (body == null) {
					continue;
				}

				if (body.IsTracked) {
					if (persons.ContainsKey (body.TrackingId) == false) {
						persons.Add (body.TrackingId, CreateNewPerson (body.TrackingId));


					}
					if (persons.ContainsKey (body.TrackingId)) {
						RefreshPerson (body, persons [body.TrackingId]);
					}
				}
			}
		}
	}

	//Creates a new person using tracked ids kinnect has picked up
	private GameObject CreateNewPerson (ulong id)
	{
		GameObject person = (GameObject)Instantiate (playerPlaceholder, new Vector3 (0f, 0f, 0f), Quaternion.identity);
		person.name = id.ToString ();
		Renderer renderer = person.GetComponent<Renderer>();
		renderer.material.color = new Color (Random.Range (0f, 1f), Random.Range (0f, 1f), Random.Range (0f, 1f));

        PersonInfo pi = person.GetComponent<PersonInfo>();
        pi.playerID = id;
	
		return person;
	}
		

	//list of big bubbles that have been created in the game screen
	List<GameObject> bigBubsCreated = new List<GameObject>();
	//List of activated small bubbles in game: What this means is bubles that have paired and met distance requirements
	List<GameObject> activatedSmlBubs = new List<GameObject>();

	//creates a new big bubbles using two bubbles in game and their pair
	private GameObject CreateNewBigBubble(GameObject bubbleA, GameObject bubbleB, IntPair pair)
	{
		GameObject BiggestBubble = (GameObject)Instantiate (biggerBubble, new Vector3 (xvalues, yvalues, 0f), Quaternion.identity);
		//giving bubble context
		BiggestBubble.AddComponent<hasSmallBubbles>();
		hasSmallBubbles item = BiggestBubble.GetComponent<hasSmallBubbles>();
		item.spriteNum = 0;
        item.colorNum = 0;
		item.bubbleA = bubbleA;
		item.bubbleB = bubbleB;
		item.pair = pair;
        bigBubsCreated.Add(BiggestBubble);
		return BiggestBubble;
	}

	//converts persons dictionary into a list so we can iterate over people in game and give them bubbles;
	public List<GameObject> dictionaryConverter ()
	{
		bubblesInGame = persons.Values.ToList();
		return bubblesInGame;
	}


	//returns the distance betwen two game objects
	private float objectDistance (GameObject a, GameObject b)
	{
		float distance = Vector3.Distance (a.transform.position, b.transform.position);
		return distance;

	}

	public List<IntPair> pairs;

	public void grabbingPairs (){
		pairs = new List<IntPair> ();
		for (int i = 0; i < bubblesInGame.Count; i++) {
			for (int j = i + 1 ; j < bubblesInGame.Count; j++) {
				if (i != j) {
					IntPair pair = new IntPair (i, j);
					//GameObjectPair pair = new GameObjectPair (bubblesInGame [i], bubblesInGame [j]);

					if (pairs.Contains (pair)) {
						continue;
					} else {
						pairs.Add (pair);
					}
				}
			}
		}
	}
		

	private void distanceStuff ()
	{

		//Create/Update list of all possible pairs
		grabbingPairs();

		for (int k = 0; k < pairs.Count; k++) {
			IntPair pair = pairs[k];
			int pairFirstDigit = pair.firstDigit;
			int pairSecondDigit = pair.secondDigit;

			GameObject bubbleA = bubblesInGame[pairFirstDigit];
			GameObject bubbleB = bubblesInGame[pairSecondDigit];

			float distab = objectDistance(bubbleA, bubbleB);
			float firstPosx = (bubbleA.transform.position.x);
			float secondPosx = (bubbleB.transform.position.x);
			float firstPosy = (bubbleA.transform.position.y);
			float secondPosy = (bubbleB.transform.position.y);

			xvalues = ((firstPosx + secondPosx) / 2f);
			yvalues = ((firstPosy + secondPosy) / 2f);

			//if distance is close enough(simulated collision)
			if (distab <= 0.9f) {

                //Disables GameObject childs for lineRenderer
                //bubbleA.transform.GetChild(pairFirstDigit).GetComponent<SpriteRenderer>().enabled = false;
                //bubbleB.transform.GetChild(pairSecondDigit).GetComponent<SpriteRenderer>().enabled = false;


                //check if already activated small bubbles
                bool alreadyPaired = false;
				for (int t = 0; t < activatedSmlBubs.Count; t++) {

					if (activatedSmlBubs[t] == bubbleA
						|| activatedSmlBubs[t] == bubbleB ) {
						alreadyPaired = true;
					}
				}

				//If small bubbles in pair are not activated, active pair them!
				if (!alreadyPaired) {      

					//Check if Big Bubble already exists
					bool existsAlready = false;
					for (int t = 0; t < bigBubsCreated.Count; t++) {

						//get bigBubsCreated[t] context
						hasSmallBubbles item = bigBubsCreated [t].GetComponent<hasSmallBubbles> ();
						IntPair bigBubsPair = item.pair;

						if (pair.Equals (bigBubsPair)) {
							existsAlready = true;
						}
					}
					
					//If Big Bubble doesn't already exist, create it
					if (!existsAlready) {
						CreateNewBigBubble (bubbleA, bubbleB, pair);

						//add smaller bubbles to activatedSmBubs
						activatedSmlBubs.Add (bubbleA);
						activatedSmlBubs.Add (bubbleB);

						//remove smaller bubbles
						bubbleA.GetComponent<SpriteRenderer> ().enabled = false;
						bubbleB.GetComponent<SpriteRenderer> ().enabled = false;
						for (int p = 0; p < 6; p++)
						{
							LineRenderer lrA = bubbleA.transform.GetChild(p).GetComponent<LineRenderer>();
							lrA.enabled = false;
							LineRenderer lrB = bubbleB.transform.GetChild(p).GetComponent<LineRenderer>();
							lrB.enabled = false;

						}
					}
				}

			} else {
                

				//Destroy Big Bubble
				bool isEmpty = !bigBubsCreated.Any ();
				if (!isEmpty) {
					for (int t = 0; t < bigBubsCreated.Count; t++) {

						// Get context of bigBubsCreated[t]
						hasSmallBubbles item = bigBubsCreated [t].GetComponent<hasSmallBubbles> ();
						IntPair bigBubsPair = item.pair;

						//Check if bigBubsCreated[t] is this pair or not, if not leave it, if yes destroy it
						if (pair.Equals(bigBubsPair)) {

							//Destory Big Bubble
							Destroy(bigBubsCreated[t]);

							//Remove things from activated arrays
							bigBubsCreated.Remove(bigBubsCreated[t]);
							activatedSmlBubs.Remove(bubbleA);
							activatedSmlBubs.Remove(bubbleB);
				
							//Show small bubbles again
							bubbleA.GetComponent<SpriteRenderer>().enabled = true;
							bubbleB.GetComponent<SpriteRenderer>().enabled = true;
							

						}
					}
				}
				//Draw line between pair if small bubbles not already in a big bubble #nolinesluts
				bool bubblesCanLine = true;
				for (int p = 0; p < activatedSmlBubs.Count; p++)
					if (bubbleA == activatedSmlBubs [p]
						|| bubbleB == activatedSmlBubs [p]) {
						bubblesCanLine = false;
					}

				if(bubblesCanLine) {
					drawLineRenderer(pair);
				}
                
            }
		}
	}

	//Remove dead bubbles - kinect bug workaround
	private void clearDeadBubbles() {
		for (int t = 0; t < bigBubsCreated.Count; t++) {
			// Get context of bigBubsCreated[t]
			hasSmallBubbles item = bigBubsCreated[t].GetComponent<hasSmallBubbles> ();
			GameObject bubbleA;
			GameObject bubbleB;

			bubbleA = item.bubbleA;
            bubbleB = item.bubbleB;

			if ((bubbleA == null) || (bubbleB == null)) {
				//Destory Big Bubble
				Destroy(bigBubsCreated[t]);

				//Reshow Smaller Bubble if still exists
				if (bubbleA != null) {
					bubbleA.GetComponent<SpriteRenderer> ().enabled = true;
				}

				if (bubbleB != null) {
					bubbleB.GetComponent<SpriteRenderer> ().enabled = true;
				}
					
				//Remove things from activated arrays
				activatedSmlBubs.Remove(bubbleA);
				activatedSmlBubs.Remove(bubbleB);
				bigBubsCreated.Remove(bigBubsCreated[t]);
				t--;
			}


		}
	}

	private void RefreshPerson (Body body, GameObject person)
	{
		Windows.Kinect.Joint spinebase = body.Joints [JointType.SpineBase];
		CameraSpacePoint csp = spinebase.Position;
		Vector3 size = new Vector3 (0.08f, 0.08f, 0);
		//position of persons game object(edit this for positional reajustment)
		person.transform.position = new Vector3 (csp.X + 0.25f, -csp.Z + 0.45f, 0f);
		person.transform.localScale = size;
	}



	public Body KinectA;
	public Body KinectB;

	public void checkHighFive()
	{
		//Check through BigBubs to see if they are high fiving
		for (int k = 0; k < bigBubsCreated.Count; k++)
		{
			//Get each bubble from big bubble
			hasSmallBubbles item = bigBubsCreated[k].GetComponent<hasSmallBubbles>();
			GameObject BubbleA = item.bubbleA;
			GameObject BubbleB = item.bubbleB;


			//create list of kinect bodies
			List<Body> trackedIDs = new List<Body>();
			foreach (Body body in bodies)
			{
				if (body == null)
				{
					continue;
				}
				if (body.IsTracked)
				{
					trackedIDs.Add(body);
				}
			}

			//Get Kinect objects of Bubble A and Bubble B
			for (int i = 0; i < trackedIDs.Count; i++)
			{
				if (Convert.ToInt64(trackedIDs[i].TrackingId) == Int64.Parse(BubbleA.name))
				{
					KinectA = trackedIDs[i];
				}
			}

			for (int i = 0; i < trackedIDs.Count; i++)
			{
				if (Convert.ToInt64(trackedIDs[i].TrackingId) == Int64.Parse(BubbleB.name))
				{
					KinectB = trackedIDs[i];
				}
			}

			//Get Hand and shoulder of Bubble A and Bubble B
			Windows.Kinect.Joint KinectA_Hand = KinectA.Joints[JointType.HandRight];
            Windows.Kinect.Joint KinectA_LeftHand = KinectA.Joints[JointType.HandLeft];
			Windows.Kinect.Joint KinectB_Hand = KinectB.Joints[JointType.HandRight];
            Windows.Kinect.Joint KinectB_LeftHand = KinectB.Joints[JointType.HandLeft];
            Windows.Kinect.Joint KinectB_Shoulder = KinectB.Joints[JointType.ShoulderRight];
			Windows.Kinect.Joint KinectA_Shoulder = KinectA.Joints[JointType.ShoulderRight];

			bool highfiveDetected = false;
			//Check if they are highfiving
			if((KinectA_Hand.Position.Y > KinectA_Shoulder.Position.Y 
                || KinectA_LeftHand.Position.Y > KinectA_Shoulder.Position.Y )
                && (KinectB_Hand.Position.Y > KinectB_Shoulder.Position.Y 
                || KinectB_LeftHand.Position.Y > KinectB_Shoulder.Position.Y)
				&& (ObjectDistanceKinectJoint(KinectA_Hand, KinectB_Hand) < 0.1f
                || ObjectDistanceKinectJoint(KinectA_Hand, KinectB_LeftHand) < 0.1f
                || ObjectDistanceKinectJoint(KinectA_LeftHand, KinectB_Hand) < 0.1f
                || ObjectDistanceKinectJoint(KinectA_LeftHand, KinectB_LeftHand) < 0.1f)) {
				highfiveDetected = true;
			}

			//if highfiving do something (loop through list of different shapes)
			if (highfiveDetected) {
				//GameObject person = (GameObject)Instantiate (star, new Vector3 (0f, 0f, 0f), Quaternion.identity);

				//What is the current sprite
				if (bigBubsCreated [k].GetComponent<hasSmallBubbles> ().delay < 0) {
					if (bigBubsCreated [k].GetComponent<hasSmallBubbles> ().spriteNum == sprites.Length - 1) {
						bigBubsCreated [k].GetComponent<SpriteRenderer> ().sprite = sprites [0];
						bigBubsCreated [k].GetComponent<hasSmallBubbles> ().spriteNum = 0;
					} else {
						bigBubsCreated [k].GetComponent<hasSmallBubbles> ().spriteNum++;
						int changeme = bigBubsCreated [k].GetComponent<hasSmallBubbles> ().spriteNum;
						bigBubsCreated [k].GetComponent<SpriteRenderer> ().sprite = sprites [changeme];
					}

					bigBubsCreated [k].GetComponent<hasSmallBubbles> ().delay = 0.5f;
				}

			}

		}
	} // End checkHighFive



    public void checkHandShake()
    {
        //Check through BigBubs to see if they are hand shaking
        for (int k = 0; k < bigBubsCreated.Count; k++)
        {
            //Get each bubble from big bubble
            hasSmallBubbles item = bigBubsCreated[k].GetComponent<hasSmallBubbles>();
            GameObject BubbleA = item.bubbleA;
            GameObject BubbleB = item.bubbleB;


            //create list of kinect bodies
            List<Body> trackedIDs = new List<Body>();
            foreach (Body body in bodies)
            {
                if (body == null)
                {
                    continue;
                }
                if (body.IsTracked)
                {
                    trackedIDs.Add(body);
                }
            }

            //Get Kinect objects of Bubble A and Bubble B
            for (int i = 0; i < trackedIDs.Count; i++)
            {
                if (Convert.ToInt64(trackedIDs[i].TrackingId) == Int64.Parse(BubbleA.name))
                {
                    KinectA = trackedIDs[i];
                }
            }

            for (int i = 0; i < trackedIDs.Count; i++)
            {
                if (Convert.ToInt64(trackedIDs[i].TrackingId) == Int64.Parse(BubbleB.name))
                {
                    KinectB = trackedIDs[i];
                }
            }

            //Get Hand and shoulder of Bubble A and Bubble B
            Windows.Kinect.Joint KinectA_Hand = KinectA.Joints[JointType.HandRight];
            Windows.Kinect.Joint KinectB_Hand = KinectB.Joints[JointType.HandRight];
            Windows.Kinect.Joint KinectB_Shoulder = KinectB.Joints[JointType.ShoulderRight];
            Windows.Kinect.Joint KinectA_Shoulder = KinectA.Joints[JointType.ShoulderRight];

            bool handshakeDetected = false;
            //Check if they are highshaking
            if (KinectA_Hand.Position.Y < KinectA_Shoulder.Position.Y
                && KinectB_Hand.Position.Y < KinectB_Shoulder.Position.Y
                && ObjectDistanceKinectJoint(KinectA_Hand, KinectB_Hand) < 0.1f)
            {
                handshakeDetected = true;
            }

            //if highshaking do something (floor bubble change color)
            if (handshakeDetected)
            {
                //startColor = colors[0];

                //for(int i = 0; i<colors.Length; i++) { 
                //    currentColor = Color.Lerp(startColor, colors[i], Mathf.PingPong(Time.time, 5));
             

                //}
                Renderer renderer = bigBubsCreated[k].GetComponent<Renderer>();
                //renderer.material.color = currentColor;

                Debug.Log("I am in handshaker");


                
                //What is the current sprite
                if (bigBubsCreated[k].GetComponent<hasSmallBubbles>().delay < 0)
                {
                    if (bigBubsCreated[k].GetComponent<hasSmallBubbles>().colorNum == colors.Length -1)
                    {
                        bigBubsCreated[k].GetComponent<SpriteRenderer>().color = colors[0];
                        bigBubsCreated[k].GetComponent<hasSmallBubbles>().colorNum = 0;
                        
                        renderer.material.color = colors[0];
                        Debug.Log("I am in the last color list " + colors[0]);
                    }
                    else
                    {
                        bigBubsCreated[k].GetComponent<hasSmallBubbles>().colorNum++;
                        int changeme = bigBubsCreated[k].GetComponent<hasSmallBubbles>().colorNum;
                        bigBubsCreated[k].GetComponent<SpriteRenderer>().color = colors[changeme];

                        renderer.material.color = colors[changeme];
                        Debug.Log("I am in going through the list " + colors[changeme]);
                    }

                    bigBubsCreated[k].GetComponent<hasSmallBubbles>().delay = 0.5f;
                }
            }
            

        }
    } // End checkHandShake


    //Check Distance between two joints
    private float ObjectDistanceKinectJoint(Windows.Kinect.Joint JointA, Windows.Kinect.Joint JointB)
	{
		float a; //Higher Number
		float b;

		//Put Higher x axis in a
		if (JointA.Position.X > JointB.Position.X)
		{
			a = JointA.Position.X;
			b = JointB.Position.X;
		}
		else
		{
			b = JointA.Position.X;
			a = JointB.Position.X;
		}
		return (a - b);
	}
    //End ObjectDistanceKinectJoint

	 
   



    //Draw Line between two people
    public void drawLineRenderer(IntPair pair)
    {
		
        //get possible pair integers
        int pairFirstDigit = pair.firstDigit;
        int pairSecondDigit = pair.secondDigit;

        //transform pair integers to gameObjects
        GameObject bubbleA = bubblesInGame[pairFirstDigit];
        GameObject bubbleB = bubblesInGame[pairSecondDigit];

        //LineRenderers for personA and personB
        LineRenderer lineRendererA;
        LineRenderer lineRendererB;

        
        
        //Calculates distance of line
        float distance;
        float distanceFirst;
        //Speed of drawing the line. 
        float lineDrawSpeed = 7f;

        //Find midPoint
        float firstPosx = (bubbleA.transform.position.x);
        float secondPosx = (bubbleB.transform.position.x);
        float firstPosy = (bubbleA.transform.position.y);
        float secondPosy = (bubbleB.transform.position.y);

        Vector3 startA = bubbleA.transform.position;
        Vector3 startB = bubbleB.transform.position;

        xvalues = ((firstPosx + secondPosx) / 2f);
        yvalues = ((firstPosy + secondPosy) / 2f);


        //Sets the line renderer for personA and personB
        lineRendererA = bubbleA.transform.GetChild(pairSecondDigit).GetComponent<LineRenderer>();
        lineRendererB = bubbleB.transform.GetChild(pairFirstDigit).GetComponent<LineRenderer>();

        //Get X and Y - dy dx
        //float dxA = xvalues - bubbleA.transform.position.x;
        //float dyA = yvalues - bubbleA.transform.position.y;

        //Double angleA = Math.Atan(Convert.ToDouble(dyA/dxA));

        //float startAX = bubbleA.transform.position.x + (float)Math.Sin(angleA);
        //float startAY = bubbleA.transform.position.y + (float)Math.Cos(angleA);
        //distanceFirst = (objectDistance(bubbleA, bubbleB) / 2);
        //float startNum = Mathf.Lerp(0, distanceFirst, 5);

        //Vector3 midPointStart = new Vector3(xvalues, yvalues);

        //Vector3 startPointLineA = startNum * Vector3.Normalize(midPointStart - startA) + startA;
        //startPointLineA.z = -5f;
        //Vector3 startPointLineB = startNum * Vector3.Normalize(midPointStart - startB) + startB;
        //startPointLineB.z = -5f;


        //Sets the starting position of the lineRenderer for PersonA and PersonB
        lineRendererA.SetPosition(0, new Vector3(bubbleA.transform.position.x, bubbleA.transform.position.y, -5f));
        lineRendererB.SetPosition(0, new Vector3(bubbleB.transform.position.x, bubbleB.transform.position.y, -5f));

        lineRendererA.SetWidth(.0125f, .0125f);
        lineRendererB.SetWidth(.0125f, .0125f);

        lineRendererA.sortingOrder = 2;
        lineRendererB.sortingOrder = 2;

        distance = (objectDistance(bubbleA, bubbleB)/2);

        PersonInfo piA = bubbleA.GetComponent<PersonInfo>();
        PersonInfo piB = bubbleB.GetComponent<PersonInfo>();
        if (piA.lineRendererIDs != null && piB.lineRendererIDs != null)
        {
            piA.lineRendererIDs.Add(piB.playerID, pairSecondDigit);
            piB.lineRendererIDs.Add(piA.playerID, pairFirstDigit);

            if (piA.counters.ContainsKey(piB.playerID) && piB.counters.ContainsKey(piA.playerID))
            {
                piA.counters[piB.playerID] += (0.1f / lineDrawSpeed);
                piB.counters[piA.playerID] += (0.1f / lineDrawSpeed);

                piA.counters[piB.playerID] = Mathf.Min(piA.counters[piB.playerID], 1f);
                piB.counters[piA.playerID] = Mathf.Min(piB.counters[piA.playerID], 1f);
            }
            else
            {
                piA.counters.Add(piB.playerID, 0f);
                piB.counters.Add(piA.playerID, 0f);
            }

        
        



            //counter += .1f / lineDrawSpeed;

            //counterArray[pairFirstDigit][pairSecondDigit] += 0.1f;
            //counterArray[pairSecondDigit][pairFirstDigit] += 0.1f;

            float x = Mathf.Lerp(0, distance, piA.counters[piB.playerID]);  // counterArray[pairFirstDigit][pairSecondDigit]);

            

            
            Vector3 midPoint = new Vector3(xvalues, yvalues);

            Vector3 pointAlongLineA = x * Vector3.Normalize(midPoint - startA) + startA;
            pointAlongLineA.z = -5f;
            Vector3 pointAlongLineB = x * Vector3.Normalize(midPoint - startB) + startB;
            pointAlongLineB.z = -5f;

            //set end position of line
            lineRendererA.SetPosition(1, pointAlongLineA);
            lineRendererB.SetPosition(1, pointAlongLineB);

            lineRendererA.enabled = true;
            lineRendererB.enabled = true;

        }

    }
    //End drawLineRenderer


    private void resetCounters()
    {
        for(int i=0; i<6; i++)
        {
            List<ulong> toRemove = new List<ulong>();

            PersonInfo pi = bubblesInGame[i].GetComponent<PersonInfo>();

            foreach (ulong id in pi.counters.Keys)
            {
                if (pi.lineRendererIDs.ContainsKey(id) == false)
                {
                    toRemove.Add(id);
                }
            }

            foreach (ulong id in toRemove)
            {
                pi.counters.Remove(id);
            }
        }
    }


    private void clearLines()
    {
        
            for (int i = 0; i < bubblesInGame.Count; i++)
            {
                PersonInfo pi = bubblesInGame[i].GetComponent<PersonInfo>();

                if(pi.lineRendererIDs != null) {
                pi.lineRendererIDs.Clear();

                for (int k = 0; k < 6; k++)
                {
                    LineRenderer lr = bubblesInGame[i].transform.GetChild(k).GetComponent<LineRenderer>();
                    lr.enabled = false;
                }
            }
        }
    }

    private bool existsAlready = false;
    public void RemoveWalkHere()
    {
        Vector3 offset = new Vector3(0.3f, 4.25f, +7f);
        if (bubblesInGame.Count < 1){
            if (!existsAlready)
            {
                walkhere = (GameObject)Instantiate(walkiePrefab, transform.position + offset, Quaternion.identity);
                existsAlready = true;
            }

        }else
        {
            Destroy(walkhere);
            existsAlready = false;
            //bool otherBool = SpawnMemo. 
        }
    }



	void OnApplicationQuit ()
	{
		if (bodyReader != null) {
			bodyReader.Dispose ();
			bodyReader = null;
		}
		if (kSensor != null) {
			if (kSensor.IsOpen) {
				kSensor.Close ();
			}
			kSensor = null;
		}
	}



}

// Component to give big bubble context 
public class hasSmallBubbles : MonoBehaviour {
	public GameObject bubbleA;
	public GameObject bubbleB;
	public IntPair pair;
	public int spriteNum;
    public int colorNum;
	public float delay = 0.5f;

	void Update(){
		if (delay > 0) {
			delay -= Time.deltaTime;
		}
	}
}