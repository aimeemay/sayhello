using UnityEngine;
using System.Collections.Generic;

public class PersonInfo : MonoBehaviour {

    public ulong playerID;
    public Dictionary<ulong, float> counters;
    public Dictionary<ulong, int> lineRendererIDs;

	// Use this for initialization
	void Start () {
        counters = new Dictionary<ulong, float>();
        lineRendererIDs = new Dictionary<ulong, int>();
	}
}
