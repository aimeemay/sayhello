using UnityEngine;
using System.Collections.Generic;
using Windows.Kinect;
using System;
using System.Linq;
using Random=UnityEngine.Random;

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
	public GameObject star;
	public Sprite[] sprites;


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
			//Calculates distance between two pairs, can do this for multiple pairs. 
			distanceStuff ();
			//Check if high fiving
			checkHighFive();
			//Clears the dead bubbles from game if the bubbles(people are not in game anymore)
			clearDeadBubbles ();
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
		//renderer.material.color = new Color (Random.Range (0f, 1f), Random.Range (0f, 1f), Random.Range (0f, 1f));
		Color personColor = new Color (Random.Range (0f, 1f), Random.Range (0f, 1f), Random.Range (0f, 1f));
		renderer.material.SetColor ("_EmissionColor", personColor);

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
		item.bubbleA = bubbleA;
		item.bubbleB = bubbleB;
		item.pair = pair;

		Renderer renderer = BiggestBubble.GetComponent<Renderer> ();
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
                bubbleA.transform.GetChild(pairFirstDigit).GetComponent<SpriteRenderer>().enabled = false;
                bubbleB.transform.GetChild(pairSecondDigit).GetComponent<SpriteRenderer>().enabled = false;

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
						Debug.Log (activatedSmlBubs);

						//remove smaller bubbles
						bubbleA.GetComponent<SpriteRenderer> ().enabled = false;
						bubbleB.GetComponent<SpriteRenderer> ().enabled = false;
					}
				}

			} else {
                //Draw line between pair
                drawLineRenderer(pair);


				bool isEmpty = !bigBubsCreated.Any ();
				//If Big Bubbles exist, 
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
							Debug.Log (activatedSmlBubs);

						}
					}
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
			

			//GameObject bubbleA = item.bubbleA;
			Debug.Log (bubbleA);
			Debug.Log (bubbleB);

			//GameObject bubbleB = item.bubbleB;
			//Debug.Log (bubbleB);

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

			//Debug.Log (bubbleA + " " + bubbleB);

//			bool smlBubsHasDisappeared = true;
//			//if we can find both small bubbles, then don't destroy the big bubble
//			for (int k = 0; k < bubblesInGame.Count; k++) {
//				if (bubblesInGame[k] == bubbleA) {
//					//Debug.Log ("found the 1st bubble");
//					for (int n = 0; n < bubblesInGame.Count; n++) {
//						if (bubblesInGame[n] == bubbleB) {
//							//Debug.Log ("found the 2nd bubble");
//							smlBubsHasDisappeared = false;
//
//						}
//					}
//				}
//			}
//
//			if (smlBubsHasDisappeared) {
//				//Debug.Log ("destroying the big bubble");
//				//Destory Big Bubble
//				Destroy(bigBubsCreated[t]);
//
//				//Remove things from activated arrays
//				bigBubsCreated.Remove(bigBubsCreated[t]);
//				activatedSmlBubs.Remove(bubbleA);
//				activatedSmlBubs.Remove(bubbleB);
//			}
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
        //Counts where the line is in relation to its end point
        float counter = 0f;
        //Calculates distance of line
        float distance;
        //Speed of drawing the line. 
        float lineDrawSpeed = 6f;

        //Enables GameObject childs
        bubbleA.transform.GetChild(pairFirstDigit).GetComponent<SpriteRenderer>().enabled = true;
        bubbleB.transform.GetChild(pairSecondDigit).GetComponent<SpriteRenderer>().enabled = true;

        //Sets the line renderer for personA and personB
        lineRendererA = bubbleA.transform.GetChild(pairFirstDigit).GetComponent<LineRenderer>();
        lineRendererB = bubbleB.transform.GetChild(pairSecondDigit).GetComponent<LineRenderer>();

        //Sets the starting position of the lineRenderer for PersonA and PersonB
        lineRendererA.SetPosition(0, bubbleA.transform.position);
        lineRendererB.SetPosition(0, bubbleB.transform.position);

        lineRendererA.SetWidth(.5f, .5f);
        lineRendererB.SetWidth(.5f, .5f);

        distance = objectDistance(bubbleA, bubbleB);

        if(counter < (distance / 2))
            {
            counter += .1f / lineDrawSpeed;
            float x = Mathf.Lerp(0, distance, counter);

            float firstPosx = (bubbleA.transform.position.x);
            float secondPosx = (bubbleB.transform.position.x);
            float firstPosy = (bubbleA.transform.position.y);
            float secondPosy = (bubbleB.transform.position.y);

            xvalues = ((firstPosx + secondPosx) / 2f);
            yvalues = ((firstPosy + secondPosy) / 2f);

            Vector3 startA = bubbleA.transform.position;
            Vector3 startB = bubbleB.transform.position;
            Vector3 midPoint = new Vector3(xvalues, yvalues);

            Vector3 pointAlongLineA = x * Vector3.Normalize(midPoint - startA) + startA;
            Vector3 pointAlongLineB = x * Vector3.Normalize(midPoint - startB) + startB;

            //set end position of line
            lineRendererA.SetPosition(1, pointAlongLineA);
            lineRendererB.SetPosition(1, pointAlongLineB);

            

        }

    }
    //End drawLineRenderer



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
	public float delay = 0.5f;

	void Update(){
		if (delay > 0) {
			delay -= Time.deltaTime;
		}
	}
}