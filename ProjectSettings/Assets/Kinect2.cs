using UnityEngine;
using System.Collections.Generic;
using Windows.Kinect;
using System.Linq;

public class Kinect2 : MonoBehaviour {

    private KinectSensor kSensor;
	private BodyFrameReader bodyReader;
	private bool initialised;
	private int  numberOfBodies;
	private Body [] bodies;
	public new Dictionary<ulong, GameObject> persons;
	public GameObject playerPlaceholder;
	public GameObject bubblePrefab;
	GameObject bigBubble;
	public GameObject biggerBubble;
    public GameObject BiggestBubble;
    public float xvalues;
    public float yvalues;
    public GameObject star;
   
    //Experimenting


    //two key value pair list of the dictionary 
    public new List<GameObject> bubblesInGame;





    // Use this for initialization
    void Start () {
		initialised = false;
        

	}
	

	void Update (){
		if(initialised == false)
		{
			StartKinect();
		}else
		{
			UpdateKinect ();
			dictionaryConverter ();
			distanceStuff ();
            
            

        }
    }
		
	
	private void StartKinect()
	{
			kSensor = KinectSensor.GetDefault();
			
			if(kSensor != null){
				bodyReader = kSensor.BodyFrameSource.OpenReader();
				if (kSensor.IsOpen == false) {
				kSensor.Open ();
				numberOfBodies = kSensor.BodyFrameSource.BodyCount;
				bodies = new Body[numberOfBodies];
				persons = new Dictionary<ulong, GameObject>();
				initialised = true;

				}
				
			
			}
	
	}
	
	private void UpdateKinect()
    {
        if (bodyReader != null)
        {
            BodyFrame frame = bodyReader.AcquireLatestFrame();
            if (frame == null)
            {
                return;
            }
            // You need to use the same body data array in each frame.
            frame.GetAndRefreshBodyData(bodies);

            // You need to make sure to dispose of the frame otherwise you can't get the next one.
            frame.Dispose();
            frame = null;

            // Get the ids of the bodies that are currently being tracked.
            // Bodies are not guaranteed to be loaded in the same location frame to frame.
            List<ulong> trackedIDs = new List<ulong>();
            foreach (Body body in bodies)
            {
                if (body == null)
                {
                    continue;
                }
                if (body.IsTracked)
                {
					
                    trackedIDs.Add(body.TrackingId);
                }
            }

            List<ulong> knownIDs = new List<ulong>(persons.Keys);

            // Delete persons that are no longer tracked
            foreach(ulong trackingID in knownIDs)
            {
                if (trackedIDs.Contains(trackingID) == false)
                {
                    Destroy(persons[trackingID]);
                    persons.Remove(trackingID);
                }
            }

            foreach (Body body in bodies)
            {
                if (body == null)
                {
                    continue;
                }

                if (body.IsTracked)
                {
                    if (persons.ContainsKey(body.TrackingId) == false)
                    {
                        persons.Add(body.TrackingId, CreateNewPerson(body.TrackingId));


                    }
                    if (persons.ContainsKey(body.TrackingId))
                    {
                        RefreshPerson(body, persons[body.TrackingId]);
                    }
                }
            }
        }
    }
	//have if bubble with script 
    private GameObject CreateNewPerson(ulong id)
    {
		GameObject person = (GameObject)Instantiate(playerPlaceholder, new Vector3(0f, 0f, 0f), Quaternion.identity);
		person.name = id.ToString();
        //GameObject starObject = (GameObject)Instantiate(star, new Vector3(0f, 0f, 0f), Quaternion.identity);
        //Renderer starRenderer = starObject.GetComponent<Renderer>();
        //starRenderer.enabled = false;
        //star.GetComponent<SpriteRenderer>().enabled = false;
        Renderer renderer = person.GetComponent<Renderer>();
        renderer.material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        return person;

    }


    List<GameObject> bigBubsCreated = new List<GameObject>();
    private GameObject CreateNewBigBubble()
    {
        GameObject BiggestBubble = (GameObject)Instantiate(biggerBubble, new Vector3(xvalues, yvalues, 0f), Quaternion.identity);
        Renderer renderer = BiggestBubble.GetComponent<Renderer>();
        bigBubsCreated.Add(BiggestBubble);
        //Vector3 size = new Vector3(xvalues, yvalues, 0);
       	//BiggestBubble.transform.localScale = size;
        return BiggestBubble;

    }

    public List<GameObject> dictionaryConverter(){

		bubblesInGame = persons.Values.ToList();
		return bubblesInGame;
	}

	private float objectDistance(GameObject a, GameObject b){
		float distance = Vector3.Distance (a.transform.position, b.transform.position);
		return distance;

	}


	private void distanceStuff(){
		//Debug.Log (bubblesInGame.Count);

		List<IntPair> pairs = new List<IntPair>();
        for (int i = 0; i < bubblesInGame.Count; i++) {
			for (int j = 0; j < bubblesInGame.Count; j++) {
				if (i != j) {
					IntPair pair = new IntPair (i, j);
					if (pairs.Contains (pair)) {
						continue;
					} else {
						pairs.Add (pair);
						for (int k = 0; k < pairs.Count; k++) {
							int pairFirstDigit = pairs [k].firstDigit;
							int pairSecondDigit = pairs [k].secondDigit;
                            GameObject bubbleA = bubblesInGame[pairFirstDigit];
                            GameObject bubbleB = bubblesInGame[pairSecondDigit];



                            float distab = objectDistance (bubbleA, bubbleB);
							if (distab <= 0.9f) {
								float firstPosx = (bubbleA.transform.position.x);
								float secondPosx = (bubbleB.transform.position.x);
								float firstPosy = (bubbleA.transform.position.y);
								float secondPosy = (bubbleB.transform.position.y);

							    xvalues = ((firstPosx + secondPosx)/2f);
								yvalues = ((firstPosy + secondPosy)/2f);

                                

                                //bubbleA.GetComponentInChildren<SpriteRenderer>().enabled = true;
                                //Transform starObject = bubbleA.transform.GetChild(0);
                                //starObject.GetComponent<SpriteRenderer>().enabled = true;
                                bubbleA.GetComponent<SpriteRenderer>().enabled = false;
								bubbleB.GetComponent<SpriteRenderer>().enabled = false;
                                

	                            bool isEmpty = !bigBubsCreated.Any();
	                            if (isEmpty){
	                                  CreateNewBigBubble();
									}




	                            }

	                        else
							{		
								
									bool isEmpty = !bigBubsCreated.Any();
									if (!isEmpty){
										for(int t =0; t<bigBubsCreated.Count; t++){
											Destroy (bigBubsCreated[t]);
											bigBubsCreated.Remove(bigBubsCreated[t]);
										}
									}
									
	                                Transform starObject = bubbleA.transform.GetChild(0);
	                                //starObject.GetComponent<SpriteRenderer>().enabled = false;
	                                bubbleA.GetComponent<SpriteRenderer>().enabled = true;
									bubbleB.GetComponent<SpriteRenderer>().enabled = true;
									
									
	                            }
                            
						}
					}
					 
				}
			}
		}
	}

    //public void CheckBubbleCreated(GameObject bubble, GameObject first, GameObject second)
    //{
    //    List<GameObject> listOfBubbles = new List<GameObject>();
    //    List<List<GameObject>> objectsInBubbles = new List<List<GameObject>>();
    //    for (int i=0; i<objectsInBubbles.Count; i++)
    //    {
    //        for (int k = 0; k < objectsInBubbles[i].Count; k++)
    //        {
    //            if (objectsInBubbles[i][k] == first)
    //            {

    //            } if (objectsInBubbles[i][k] == second)
    //            {

    //            }
    //            else
    //            {
                
    //            }
    //        }
    //    }
    //}








    private void RefreshPerson(Body body, GameObject person)
    {
        Windows.Kinect.Joint spinebase = body.Joints[JointType.SpineBase];
        CameraSpacePoint csp = spinebase.Position;


        //Windows.Kinect.Joint leftHand = body.Joints[JointType.HandLeft];
        //CameraSpacePoint cspL = leftHand.Position;

        //Windows.Kinect.Joint rightHand = body.Joints[JointType.HandRight];
        //CameraSpacePoint cspR = rightHand.Position;

        //Vector3 size = new Vector3(Mathf.Abs(Mathf.Max(cspL.X, cspR.X) - Mathf.Min(cspL.X, cspR.X)), Mathf.Abs(Mathf.Max(cspL.X, cspR.X) - Mathf.Min(cspL.X, cspR.X)), Mathf.Abs(Mathf.Max(cspL.X, cspR.X) - Mathf.Min(cspL.X, cspR.X)));
		Vector3 size = new Vector3(0.08f, 0.08f,0);
		person.transform.position = new Vector3(csp.X + 0.25f, -csp.Z + 0.45f, 0f);
        person.transform.localScale = size;
	
    }



    void OnApplicationQuit()
    {
        if (bodyReader != null)
        {
			bodyReader.Dispose();
			bodyReader = null;
        }
        if (kSensor != null)
        {
            if (kSensor.IsOpen)
            {
                kSensor.Close();
            }
            kSensor = null;
        }
    }




}

