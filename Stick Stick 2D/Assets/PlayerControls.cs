using UnityEngine;
using System.Collections;

public class PlayerControls : MonoBehaviour {

	public KeyCode moveUp = KeyCode.W;
	public KeyCode moveDown  = KeyCode.S;
	public float speed = 10f;
	private Rigidbody2D rigidBody2D;

	void Start()
	{
		rigidBody2D = GetComponent<Rigidbody2D>();
	}

	void Update () 
	{
		if(Input.GetKey(moveUp))
		{
			rigidBody2D.velocity = new Vector2(0, speed);
		}
		else if(Input.GetKey(moveDown))
		{
			rigidBody2D.velocity = new Vector2(0, -speed);
		}
		else
		{
			rigidBody2D.velocity = Vector2.zero;
		}
	}
}
