using UnityEngine;
using System.Collections;

public class test : MonoBehaviour {

	bool ranonce = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		if(!ranonce)
		{
			for(int i = 0; i < 3; ++i)
			{
				for(int j = 0; j < i; ++j)
					Debug.Log("WWW");
			}
			ranonce = true;
		}
	}
}
