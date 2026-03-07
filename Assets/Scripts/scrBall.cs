using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class scrBall : MonoBehaviour
{

	// GROK https://grok.com/c/bd04f5cf-7121-482e-8bfc-bdbcfd919700?rid=614cd0a1-b396-4ebf-9671-f59f1693ee59
	// GPT  https://chatgpt.com/g/g-p-697c58815bd88191a93c756aa16a3d86-breakout/project



	// ----- ----- ----- ----- ----- ----- CONFIG PARAMS ----- ----- ----- ----- ----- -----
	public int ID;
	private static int _nextID = 1; // Auto-incrementing ID for each ball instance

	// ----- ----- ----- ----- ----- ----- SPEED PARAMS ----- ----- ----- ----- ----- -----
	[Header("Speed")]
	public float currentSpeed;
	public int speedLevel = 0; // 0-4, corresponds to speedList index
	public string ColDetMod;
	private float speed_0 = 0f;
	private float speed_1 = 600f;
	private float speed_2 = 800f;
	private float speed_3 = 1000f;
	private float speed_4 = 1200f;
	List<float> speedList = new List<float>();
	
	public int paddleHitCount = 0;
	public Vector3 direction;
	public bool hasReflectedThisStep;

	// ----- ----- ----- ----- ----- ----- COMPONENTS ----- ----- ----- ----- ----- -----
	public Rigidbody rb;

	// ----- ----- ----- ----- ----- ----- CONSTRUCTOR ----- ----- ----- ----- ----- -----
	public scrBall()
	{
		ID = _nextID;
		_nextID++;
		speedList = new List<float>() { speed_0, speed_1, speed_2, speed_3, speed_4 };
		speedLevel = 1; // Start at speed level 1 (800f)
	}

	void Awake()
	{
		rb = gameObject.GetComponent<Rigidbody>();
		// Prevent random physics side effects for a breakout ball:
		rb.useGravity = false;
		rb.freezeRotation = true;
		rb.isKinematic = true;
		// rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // Commented as original
		// ColDetMod = rb.collisionDetectionMode.ToString();
	}

	void Start()
	{
		Launch();
	}

	void FixedUpdate()
	{
		float remainingDistance = currentSpeed * Time.fixedDeltaTime;
		float radius = GetComponent<SphereCollider>().radius * transform.localScale.x;
		int safetyCounter = 0; // prevents infinite loops
		Collider ballCollider = GetComponent<SphereCollider>();
		Collider[] overlaps = Physics.OverlapSphere(transform.position, radius);
		foreach (Collider col in overlaps)
		{
			if (col.CompareTag("Paddle") ||
					col.CompareTag("Brick") ||
					col.CompareTag("Wall"))
			{
				Vector3 directionToResolve;
				float distanceToResolve;
				if (Physics.ComputePenetration(
						ballCollider, transform.position, transform.rotation,
						col, col.transform.position, col.transform.rotation,
						out directionToResolve, out distanceToResolve))
				{
					// Push fully out
					transform.position += directionToResolve * distanceToResolve;
					// Only reflect if we were moving INTO the surface
					float intoSurface = Vector3.Dot(direction, -directionToResolve);
					if (intoSurface > 0f)
					{
						direction = Vector3.Reflect(direction, directionToResolve).normalized;
					}
					break;
				}
			}
		}
		// FIXED: Reverted to ORIGINAL while condition (> 0f) and SphereCast (no layer/query params) → ball launches
		while (remainingDistance > 0f && safetyCounter < 5)
		{
			RaycastHit hit;
			// FIXED: Exact original SphereCast call → no blocking params
			if (Physics.SphereCast(transform.position, radius, direction, out hit, remainingDistance))
			{
				// Move to impact point
				float visualPenetration = 0.03f; // tune between 0.01–0.08
				transform.position = hit.point + hit.normal * (radius - visualPenetration);
				// Reduce remaining distance
				remainingDistance -= hit.distance;
				HandleHit(hit);
				safetyCounter++;
			}
			else
			{
				// No hit — move remaining distance
				transform.position += direction * remainingDistance;
				remainingDistance = 0f;
			}
		}
	}

	void HandleHit(RaycastHit hit)
	{
		GameObject hitter = hit.collider.gameObject;
		if (hitter.CompareTag("Paddle"))
		{
			Debug.Log("Paddle Hit");
			paddleHitCount++;
			UpdateSpeed(paddleHitCount);

			// CHANGED: Hybrid logic preserves your exact position-based angle code.
			//          Detects top/bottom (Z-normal dominant) vs sides (X-normal).
			//          Top: upward angles. Bottom: same angles but downward (flipped Z).
			//          Sides: standard reflect.
			Vector3 paddleCenter = hit.collider.bounds.center;
			float halfWidth = hit.collider.bounds.size.x * 0.5f;
			float normalizedOffset = Mathf.Clamp(
					(hit.point.x - paddleCenter.x) / halfWidth,
					-1f,
					1f
			);
			float maxBounceAngle = 70f;
			float angle = normalizedOffset * maxBounceAngle * Mathf.Deg2Rad;
			float newX = Mathf.Sin(angle);

			// Detect face: Z-dominant = top/bottom (use position angles)
			if (Mathf.Abs(hit.normal.z) > 0.707f) // > cos(45°) threshold for "mostly Z-facing"
			{
				float newZ = Mathf.Sign(hit.normal.z) * Mathf.Cos(angle); // +Z top (up), -Z bottom (down)
				direction = new Vector3(newX, 0f, newZ).normalized;
			}
			else
			{
				// Side hit (X-dominant): standard reflection
				direction = Vector3.Reflect(direction, hit.normal).normalized;
			}
		}
		else if (hitter.CompareTag("Brick"))
		{
			Debug.Log("Brick Hit");
			scrBrick brick = hitter.GetComponent<scrBrick>();
			brick.durability -= 1;
			brick.ChangeColor();
			direction = Vector3.Reflect(direction, hit.normal).normalized;
		}
		else if (hitter.CompareTag("Wall"))
		{
			Debug.Log("Wall Hit");
			direction = Vector3.Reflect(direction, hit.normal).normalized;
		}
	}

	// FIXED: Exact original Launch → guarantees launch at speedList[1]
	void Launch()
	{
		direction = new Vector3(1f, 0f, 1f).normalized;
		currentSpeed = speedList[1]; // Explicit [1] as original
	}

	void UpdateSpeed(int level)
	{
		if (level >= speedList.Count - 1)
		{
			level = speedList.Count - 1;
		}
		currentSpeed = speedList[level];
		speedLevel = level;
	}

	// POWER UPS
	/*
		Ball Moves through bricks and eliminates them without bouncing
		Ball sticks on Paddle and waits to be launched
		Ball splits into 2 balls
		Ball Becomes Larger
		Ball Splits emits two orbitting balls that eliminate all bricks touched
	  Ball Charges Bricks into small bombs that explode after 1 second (keep short for testing)
	 
	 
	 
	*/

}