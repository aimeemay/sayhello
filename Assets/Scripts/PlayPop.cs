using UnityEngine;
using System.Collections;

public class PlayPop : MonoBehaviour {
	public AudioClip[] clips;
	// Use this for initialization
	void Start () {
		PlayRandomClip ();

	}
	
	// Update is called once per frame
	void Update () {

	}
	void PlayRandomClip(){
		//choose random clip from 0 - 9
		int randomClip = Random.Range (0, clips.Length);
		AudioSource hello = gameObject.AddComponent<AudioSource> ();
		hello.clip = clips[randomClip];
		hello.Play ();
		Destroy (hello, clips[randomClip].length);
	}

}
