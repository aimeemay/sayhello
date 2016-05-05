using UnityEngine;
using System.Collections.Generic;
using Windows.Kinect;
using System.Linq;

public class Kinect2 : MonoBehaviour
{
	private KinectSensor kSensor;
	private BodyFrameReader bodyReader;
	private bool initialised;
	private int numberOfBodies;
	private Body[] bodies;
	public new Dictionary<ulong, GameObject> persons;
	public GameObject playerPlaceholder;
	public GameObject bubblePrefab;
	GameObject bigBubble;
	public GameObject biggerBubble;
	public GameObject BiggestBubble;
	public float xvalues;
	public float yvalues;
	public GameObject star;

	//two key value pair list of the dictionary
	public new List<GameObject> bubblesInGame;

	// Use this for initialisation
	void Start ()
	{
		initialised = false;
	}

	void Update ()
	{
		if (initialised == false) {
			StartKinect ();
		} else {
			UpdateKinect ();
			dictionaryConverter ();
			distanceStuff ();
		}
	}

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

	//have if bubble with script
	private GameObject CreateNewPerson (ulong id)
	{
		GameObject person = (GameObject)Instantiate (playerPlaceholder, new Vector3 (0f, 0f, 0f), Quaternion.identity);
		person.name = id.ToString ();
		//GameObject starObject = (GameObject)Instantiate(star, new Vector3(0f, 0f, 0f), Quaternion.identity);
		//Renderer starRenderer = starObject.GetComponent<Renderer>();
		//starRenderer.enabled = false;
		//star.GetComponent<SpriteRenderer>().enabled = false;
		Renderer renderer = person.GetComponent<Renderer> ();
		renderer.material.color = new Color (Random.Range (0f, 1f), Random.Range (0f, 1f), Random.Range (0f, 1f));
		return person;
	}
		
	List<GameObject> bigBubsCreated = new List<GameObject> ();

	private GameObject CreateNewBigBubble(IntPair pair)
	{
		GameObject BiggestBubble = (GameObject)Instantiate (biggerBubble, new Vector3 (xvalues, yvalues, 0f), Quaternion.identity);

		//giving bubble context
		BiggestBubble.AddComponent<hasSmallBubbles>();
		hasSmallBubbles item = BiggestBubble.GetComponent<hasSmallBubbles>();
		item.pair = pair;

		Renderer renderer = BiggestBubble.GetComponent<Renderer> ();
		bigBubsCreated.Add(BiggestBubble);

		Debug.Log (bigBubsCreated);

		//Vector3 size = new Vector3(xvalues, yvalues, 0);
		//BiggestBubble.transform.localScale = size;
		//Debug.Log(BiggestBubble);
		return BiggestBubble;
	}

	public List<GameObject> dictionaryConverter ()
	{
		bubblesInGame = persons.Values.ToList ();
		return bubblesInGame;
	}

	private float objectDistance (GameObject a, GameObject b)
	{
		float distance = Vector3.Distance (a.transform.position, b.transform.position);
		return distance;

	}
		
	private void distanceStuff ()
	{
		//Debug.Log(bubblesInGame.Count);
		List<IntPair> pairs = new List<IntPair> ();

		//Create/Update list of all possible pairs
		for (int i = 0; i < bubblesInGame.Count; i++) {
			for (int j = i + 1 ; j < bubblesInGame.Count; j++) {
				if (i != j) {
					IntPair pair = new IntPair (i, j);
					if (pairs.Contains (pair)) {
						continue;
					} else {
						pairs.Add (pair);
					}
				}
			}
		}

		//Debug.Log(pairs.Count);

		for (int k = 0; k < pairs.Count; k++) {
			IntPair pair = pairs[k];
			int pairFirstDigit = pair.firstDigit;
			int pairSecondDigit = pair.secondDigit;
			GameObject bubbleA = bubblesInGame[pairFirstDigit];
			GameObject bubbleB = bubblesInGame[pairSecondDigit];

			float distab = objectDistance(bubbleA, bubbleB);
			if (distab <= 0.9f) {
				float firstPosx = (bubbleA.transform.position.x);
				float secondPosx = (bubbleB.transform.position.x);
				float firstPosy = (bubbleA.transform.position.y);
				float secondPosy = (bubbleB.transform.position.y);

				xvalues = ((firstPosx + secondPosx) / 2f);
				yvalues = ((firstPosy + secondPosy) / 2f);

				bubbleA.GetComponent<SpriteRenderer>().enabled = false;
				bubbleB.GetComponent<SpriteRenderer>().enabled = false;
		        
				//Check if Big Bubble already exists
				bool existsAlready = false;
				for (int t = 0; t < bigBubsCreated.Count; t++) {

					//get bigBubsCreated[t] context
					hasSmallBubbles item = bigBubsCreated[t].GetComponent<hasSmallBubbles>();
					IntPair bigBubsPair = item.pair;

					if (pair.Equals(bigBubsPair)) {
						existsAlready = true;
					}
				}
					
				//If Big Bubble doesn't already exist, create it
				if (!existsAlready) {
					CreateNewBigBubble(pair);
				}

			} else {
				bool isEmpty = !bigBubsCreated.Any();
				//If Big Bubbles exist, 
				if (!isEmpty) {


					for (int t = 0; t < bigBubsCreated.Count; t++) {

						// Get context of bigBubsCreated[t]
						hasSmallBubbles item = bigBubsCreated[t].GetComponent<hasSmallBubbles>();
						IntPair bigBubsPair = item.pair;

						//Check if bigBubsCreated[t] is this pair or not, if not leave it, if yes destroy it
						//Debug.Log(pair);
						//Debug.Log(bigBubsPair);
						//Debug.Log(pair == bigBubsPair);
						if (pair.Equals(bigBubsPair)) {
							Destroy(bigBubsCreated[t]);
							bigBubsCreated.Remove(bigBubsCreated[t]);
						}
					}


					//Transform starObject = bubbleA.transform.GetChild (0);
					//starObject.GetComponent<SpriteRenderer>().enabled = false;
					bubbleA.GetComponent<SpriteRenderer>().enabled = true;
					bubbleB.GetComponent<SpriteRenderer>().enabled = true;
				}
			}
		}
	}

	private void RefreshPerson (Body body, GameObject person)
	{
		Windows.Kinect.Joint spinebase = body.Joints [JointType.SpineBase];
		CameraSpacePoint csp = spinebase.Position;


		//Windows.Kinect.Joint leftHand = body.Joints[JointType.HandLeft];
		//CameraSpacePoint cspL = leftHand.Position;

		//Windows.Kinect.Joint rightHand = body.Joints[JointType.HandRight];
		//CameraSpacePoint cspR = rightHand.Position;

		//Vector3 size = new Vector3(Mathf.Abs(Mathf.Max(cspL.X, cspR.X) - Mathf.Min(cspL.X, cspR.X)), Mathf.Abs(Mathf.Max(cspL.X, cspR.X) - Mathf.Min(cspL.X, cspR.X)), Mathf.Abs(Mathf.Max(cspL.X, cspR.X) - Mathf.Min(cspL.X, cspR.X)));
		Vector3 size = new Vector3 (0.08f, 0.08f, 0);
		person.transform.position = new Vector3 (csp.X + 0.25f, -csp.Z + 0.45f, 0f);
		person.transform.localScale = size;
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
	public IntPair pair;
}