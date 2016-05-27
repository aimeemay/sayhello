using UnityEngine;
using System.Collections;

public class PlayVideo : MonoBehaviour {
	public Renderer r;
	public MovieTexture movie;

	// Use this for initialization
	void Start () {
		r = GetComponent<Renderer>();
		movie = (MovieTexture)r.material.mainTexture;
	}
	
	// Update is called once per frame
	void Update () {
		
		if (Input.GetButtonDown("Jump")) {
			if (movie.isPlaying) {
				movie.Pause();
			}
			else {
				movie.Play();
			}

		}
	}

}
