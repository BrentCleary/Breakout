using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class scrBall : MonoBehaviour
{
	[Header("Speed")]
	public float baseSpeed = 400f;
	public float currentSpeed;
	public float speedMultiplier = 1f; 

	public Rigidbody rb;

	void Awake()
	{
		rb = gameObject.GetComponent<Rigidbody>();

		// Prevent random physics side effects for a breakout ball:
		rb.useGravity							= false;
		rb.freezeRotation					= true;
		rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
	}
	private void Start()
	{
		currentSpeed = baseSpeed;
		Launch(Vector3.forward);  // or your preferred start direction
	}

	public void Launch(Vector3 direction)	{
		rb.linearVelocity = direction.normalized * currentSpeed;
	}


	void OnCollisionEnter(Collision collision)
	{
		scrBrick  brick  = collision.collider.GetComponent<scrBrick>();
		scrPaddle paddle	= collision.collider.GetComponent<scrPaddle>();

		if (brick == null && paddle == null) return;


		if (brick != null)
		{
			brick.DetectHit();
			brick.Break();
		}

		if (paddle != null){
			paddle.DetectHit();
		}

		Rebound(collision);

	}


	public void Rebound(Collision collision){

		// Compute bounce direction away from contact normal (XZ only)
		// normal points from brick -> ball at the contact.

		Vector3 collisionDirection = rb.linearVelocity;
		Debug.Log(collisionDirection);


		Vector3 normal = collision.contacts[0].normal;
		normal.y = 0f;
		
		if (normal.sqrMagnitude < 0.0001f) {
			normal = (transform.position - collision.transform.position); // fallback
		}
		normal.y = 0f;
		normal   = normal.normalized;

		// Keep current speed magnitude, then apply 25% increase
		float currentSpeed = rb.linearVelocity.magnitude;
		float newSpeed		 = currentSpeed * speedMultiplier;

		// Repel in opposite direction (away from the brick)
		rb.linearVelocity = normal * newSpeed;
		currentSpeed		  = newSpeed; // store if you want it tracked
	}

}




