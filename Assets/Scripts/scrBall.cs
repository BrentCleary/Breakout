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
	private float speed_1 = 300f;
	private float speed_2 = 400f;
	private float speed_3 = 500f;
	private float speed_4 = 600f;
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

	void Update()
	{
		transform.position += direction * currentSpeed * Time.deltaTime;
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


	void UpdateSpeed(int speedLevel){
		if(speedLevel >= speedList.Count-1)
		{
			currentSpeed = speedList[speedList.Count-1];
		}
		else{
			currentSpeed = speedList[speedLevel];
		}
	}


	public void Rebound(Collision collision)
	{
		ContactPoint contact = collision.contacts[0];
		Vector3 normal = contact.normal;
		Vector3 contactPoint = contact.point;

		// Nudge to prevent sticking (XZ plane only)
		transform.position += new Vector3(normal.x * 0.1f, 0f, normal.z * 0.1f);

		GameObject hitter = collision.collider.gameObject;

		if (hitter.CompareTag("Paddle"))
		{
			paddleHitCount++;
			UpdateSpeed(paddleHitCount); // Increase speed level each paddle hit, up to max defined in speedList. 

			// Position-based rebound using stable angle method
			Vector3 paddleCenter = collision.collider.bounds.center;
			float		halfWidth = collision.collider.bounds.size.x * 0.5f;

			// Normalized hit position across paddle (-1 to 1)
			float normalizedOffset = Mathf.Clamp(
					(contactPoint.x - paddleCenter.x) / halfWidth,
					-1f,
					1f
			);

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






