using UnityEngine;
using System.Collections;

public class test : MonoBehaviour {

	InventoryElement i;
	ElementType e;

	// Use this for initialization
	void Start () {
		ICooldown ic = i as ICooldown;
		ICooldown ic2 = e as ICooldown;

		if(ic == i)
			Debug.Log ("YEAAA");

		Debug.Log (ic == i);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
