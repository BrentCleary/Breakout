using NUnit.Framework;
using System.Collections.Generic;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.InputSystem;

public class scrBall : MonoBehaviour
{
	
	// ----- ----- ----- ----- ----- ----- CONFIG PARAMS ----- ----- ----- ----- ----- -----
	public int ID;
	private static int _nextID = 1; // Auto-incrementing ID for each ball instance


	// ----- ----- ----- ----- ----- ----- SPEED PARAMS ----- ----- ----- ----- ----- -----
	[Header("Speed")]
	public float currentSpeed;
	public int speedLevel = 0; // 0-4, corresponds to speedList index
	
	private float speed_0 = 0f;
	private float speed_1 = 800f;
	private float speed_2 = 1200f;
	private float speed_3 = 1600f;
	private float speed_4 = 2000f;
	List<float> speedList = new List<float>();

	public int   paddleHitCount = 0;

	public Vector3 direction;

	// ----- ----- ----- ----- ----- ----- COMPONENTS ----- ----- ----- ----- ----- -----

	public Rigidbody rb;



	// ----- ----- ----- ----- ----- ----- CONSTRUCTOR ----- ----- ----- ----- ----- -----
	public scrBall()
	{
		ID = _nextID;
		_nextID++;

		speedList = new List<float>() { speed_0, speed_1, speed_2, speed_3, speed_4 };
	
	}


	void Awake()
	{
		rb = gameObject.GetComponent<Rigidbody>();

		// Prevent random physics side effects for a breakout ball:
		rb.useGravity							= false;
		rb.freezeRotation					= true;
		rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
	}


	void Start()
	{

		Launch();
	}

	void FixedUpdate()
	{
		rb.linearVelocity = direction * currentSpeed;
	}


	void Launch()
	{
		direction = new Vector3(1f, 0f, 1f).normalized;
		currentSpeed = speedList[1];
	}



	void OnCollisionEnter(Collision collision)
	{
		Rebound(collision);

	}


	void UpdateSpeed(int level){

		if(level >= speedList.Count-1)
		{
			level = speedList.Count-1;
		}
		
		currentSpeed = speedList[level];
		speedLevel = level;
	}


	// Nudge to prevent sticking (XZ plane only)
	public void Nudge(Vector3 normal)
	{
		transform.position += new Vector3(normal.x * 0.1f, 0f, normal.z * 0.1f);
	}


	public void Rebound(Collision collision)
	{
		ContactPoint contact = collision.contacts[0];
		Vector3 normal = contact.normal;
		Vector3 contactPoint = contact.point;

		// Nudge(normal);

		GameObject hitter = collision.collider.gameObject;

		if (hitter.CompareTag("Paddle"))
		{
			paddleHitCount++;
			UpdateSpeed(paddleHitCount); // Increase speed level each paddle hit, up to max defined in speedList. 

			Vector3 paddleCenter = collision.collider.bounds.center;      // Position-based rebound using stable angle method
			float		halfWidth		 = collision.collider.bounds.size.x * 0.5f;

			float normalizedOffset = Mathf.Clamp(	(contactPoint.x - paddleCenter.x) / halfWidth, -1f,	1f);	// Normalized hit position across paddle (-1 to 1)

			// Maximum bounce angle from vertical (degrees)
			float maxBounceAngle = 70f;

			// Convert to radians
			float angle = normalizedOffset * maxBounceAngle * Mathf.Deg2Rad;

			// Construct normalized direction (XZ plane)
			float newX = Mathf.Sin(angle);
			float newZ = Mathf.Cos(angle);

			direction = new Vector3(newX, 0f, newZ).normalized;



		}
		else if (hitter.CompareTag("Brick"))
		{ 
			collision.collider.GetComponent<scrBrick>().durability -= 1;
			collision.collider.GetComponent<scrBrick>().ChangeColor();
			Debug.Log("Brick Hit");

			direction = Vector3.Reflect(direction, normal).normalized; // Mirror reflection for bricks/walls (normal-based)

		}
		else if (hitter.CompareTag("Wall"))
		{
			Debug.Log("Wall Hit");
			
			direction = Vector3.Reflect(direction, normal).normalized; // Mirror reflection for bricks/walls (normal-based)
		}

		if (currentSpeed < 300f)
		{
			Launch();
		}

	}
}






