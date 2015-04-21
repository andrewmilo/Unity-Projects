using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldInteraction : MonoBehaviour {

	public static GameObject activeGameObject;
	public static RaycastHit hitInfo;

	public float distance = 7f;
	public LayerMask layerMask;
	public Transform origin;

	void Update ()
	{
		activeGameObject = null;

		Ray ray = new Ray(origin.position, origin.transform.forward);
		if (Physics.Raycast (ray, out hitInfo, distance, layerMask))
			activeGameObject = hitInfo.transform.gameObject;
	}
}
