using System.Collections.Generic;
using UnityEngine;

// Script for handling ball behavior in a hybrid Breakout and pool game.
// This script manages movement, collisions, and specific rules for normal balls and pool balls.
public class scrBall : MonoBehaviour
{
	// ----- IDENTIFICATION BLOCK -----
	// Purpose: Unique identifier for each ball instance.
	public int ID;
	private static int _nextID = 1;

	// ----- SPEED AND MOVEMENT VARIABLES BLOCK -----
	// Purpose: Variables related to the ball's speed, direction, and hit tracking.
	[Header("Speed & Movement")]
	public float	 currentSpeed;
	public int		 speedLevel = 1;          // 0–4
	public Vector3 direction = Vector3.zero;
	public int	   paddleHitCount = 0;

	private readonly List<float> speedList = new List<float> { 0f, 600f, 800f, 1000f, 1200f };

	// ----- COMPONENTS BLOCK -----
	// Purpose: References to Unity components attached to the ball.
	private Rigidbody rb;
	private SphereCollider ballCollider;


	// ----- PARAMETERS BLOCK -----
	// Purpose: Tunable values that affect gameplay feel, such as angles, friction, and restitution.
	// PARAMETERS: These can be adjusted in the Unity Inspector for easy tweaking.
	[Header("Tuning")]
	[SerializeField, Tooltip("Maximum angle in degrees for ball deflection off paddle.")]
	private float maxBounceAngle = 65f;  // PARAMETERS: Max paddle deflection angle.

	[SerializeField, Tooltip("Small offset to prevent immediate re-collision after hit.")]
	private float collisionSkin = 0.02f;  // PARAMETERS: Skin depth for collision avoidance.

	[SerializeField, Tooltip("Deceleration rate for pool balls on felt (m/s²). Lower values mean longer rolls.")]
	private float poolDeceleration = 0.09f;  // PARAMETERS: Felt deceleration for pool balls.

	[SerializeField, Tooltip("Speed retention multiplier for pool balls when hitting static objects like paddle, brick, or wall.")]
	private float poolHitRestitution = 0.90f;  // PARAMETERS: Restitution (bounciness) for pool ball hits.

	[SerializeField, Tooltip("Minimum vertical (z) component after paddle hit to prevent horizontal trapping.")]
	private float minVerticalComponent = 0.20f;  // PARAMETERS: Min z-speed after paddle.

	// ADDED: New PARAMETER for substepping to prevent tunneling at high speeds.
	[SerializeField, Tooltip("Maximum distance per SphereCast step to prevent tunneling through thin objects.")]
	private float maxDistancePerCast = 2f;  // PARAMETERS: Max step distance per cast for anti-tunneling.

	// ADDED: New PARAMETER for layer mask to control what the ball collides with.
	[SerializeField, Tooltip("Layer mask for SphereCast and OverlapSphere. Default -1 (all layers).")]
	private LayerMask collisionLayerMask = -1;  // PARAMETERS: Collision detection layers.

	// Awake method: Initializes components and sets up physics properties.
	// Purpose: Called when the script instance is being loaded.
	void Awake()
	{
		ID = _nextID++;
		rb = GetComponent<Rigidbody>();
		ballCollider = GetComponent<SphereCollider>();


		// Set up for manual control in Breakout style.
		rb.isKinematic = true;
		rb.useGravity = false;
		rb.freezeRotation = true;
	}

	// Start method: Initializes ball state based on tag.
	// Purpose: Called once when the script becomes enabled.
	void Start()
	{
		if (CompareTag("PoolBall"))
		{
			currentSpeed = 0f;
			direction = Vector3.zero;
		}
		else
		{
			Launch();
		}
	}

	// FixedUpdate method: Handles physics updates for movement and collisions.
	// Purpose: Runs every physics step to update ball position, apply friction, and detect hits.
	void FixedUpdate()
	{
		bool isPoolBall = CompareTag("PoolBall");

		// Apply friction only to pool balls before any movement.
		if (isPoolBall && currentSpeed > 0.01f)
		{
			currentSpeed -= poolDeceleration * Time.fixedDeltaTime;
			if (currentSpeed < 0f) currentSpeed = 0f;
		}

		// Early exit if the ball has no speed.
		if (currentSpeed <= 0.001f) return;

		// Unified movement logic for all balls using SphereCast for collision detection.
		float remainingDistance = currentSpeed * Time.fixedDeltaTime;
		float radius = ballCollider.radius * transform.localScale.x;

		// Resolve any current overlaps or penetrations.
		ResolvePenetrations(radius);

		// Loop to handle multiple potential collisions in one physics step.
		int safetyCounter = 0;  // CHANGED: Renamed to safetyCounter for descriptiveness.
		while (remainingDistance > 0.001f && safetyCounter < 20)  // CHANGED: Increased safety limit to 20 for high-speed reliability.
		{
			// CHANGED: Added substepping to prevent tunneling through bricks/walls at high speeds.
			float stepDistance = Mathf.Min(remainingDistance, maxDistancePerCast);

			// CHANGED: Added layerMask to SphereCast for precise collision detection.
			if (Physics.SphereCast(transform.position, radius, direction, out RaycastHit hit, stepDistance, collisionLayerMask))
			{
				// Move to the hit point with a small skin offset.
				transform.position = hit.point + hit.normal * (radius + collisionSkin);
				remainingDistance -= hit.distance;

				HandleHit(hit);
			}
			else
			{
				// No collision in this step: move the step distance.
				transform.position += direction * stepDistance;
				remainingDistance -= stepDistance;
			}

			safetyCounter++;
		}

		// CHANGED: Unified Y lock for ALL balls to prevent inconsistent collision detection and sinking/drift.
		// All ball centers now at radius above playfield/table floor for consistent SphereCast behavior.
		Vector3 position = transform.position;
		position.y = radius;
		transform.position = position;
	}

	// ResolvePenetrations method: Fixes any current overlaps with colliders.
	// Purpose: Pushes the ball out of penetrating states and reflects if necessary.
	private void ResolvePenetrations(float radius)
	{
		// CHANGED: Added layerMask to OverlapSphere for efficiency and precision.
		Collider[] overlaps = Physics.OverlapSphere(transform.position, radius, collisionLayerMask);

		foreach (var col in overlaps)
		{
			if (col == ballCollider) continue;
			if (!col.CompareTag("Paddle") && !col.CompareTag("Brick") &&
					!col.CompareTag("Wall") && !col.CompareTag("PoolBall")) continue;

			if (Physics.ComputePenetration(
					ballCollider, transform.position, transform.rotation,
					col, col.transform.position, col.transform.rotation,
					out Vector3 directionToResolve, out float distance))
			{
				transform.position += directionToResolve * (distance + 0.01f);

				// Reflect only if the ball was moving into the surface.
				if (Vector3.Dot(direction, -directionToResolve) > 0.01f)
				{
					direction = Vector3.Reflect(direction, directionToResolve).normalized;
				}
			}
		}
	}

	// HandleHit method: Processes collisions based on what was hit.
	// Purpose: Applies reflection, damage, speed changes, or momentum exchange depending on the collider tag.
	private void HandleHit(RaycastHit hit)
	{
		string tag = hit.collider.tag;
		GameObject otherObject = hit.collider.gameObject;  // CHANGED: Renamed for descriptiveness.
		bool isPoolBall = CompareTag("PoolBall");

		if (tag == "Paddle")
		{
			// Calculate deflection angle based on hit position on paddle.
			Bounds paddleBounds = hit.collider.bounds;
			Vector3 paddleCenter = paddleBounds.center;
			float halfWidth = paddleBounds.extents.x;

			float offset = (hit.point.x - paddleCenter.x) / halfWidth;
			offset = Mathf.Clamp(offset, -1f, 1f);

			float angleRad = offset * maxBounceAngle * Mathf.Deg2Rad;
			float xComponent = Mathf.Sin(angleRad);  // CHANGED: Renamed for descriptiveness.
			float zComponent = Mathf.Cos(angleRad);

			// Adjust for hits on the bottom of the paddle.
			if (hit.normal.z < -0.4f)
			{
				zComponent = -zComponent;
			}

			direction = new Vector3(xComponent, 0f, zComponent).normalized;

			// Ensure a minimum vertical component to avoid flat trajectories.
			if (Mathf.Abs(direction.z) < minVerticalComponent)
			{
				direction.z = Mathf.Sign(direction.z) * minVerticalComponent;
				direction = direction.normalized;
			}

			// Apply speed changes: boost for normal balls, loss for pool balls.
			if (!isPoolBall)
			{
				paddleHitCount++;
				UpdateSpeed(paddleHitCount);
				Debug.Log($"Paddle hit – angle: {offset:F2}, speed level: {speedLevel}");
			}
			else
			{
				currentSpeed *= poolHitRestitution;
			}
		}
		else if (tag == "Brick")
		{
			if (otherObject.TryGetComponent(out scrBrick brick))
			{
				brick.durability--;
				brick.ChangeColor();
			}
			direction = Vector3.Reflect(direction, hit.normal).normalized;

			if (isPoolBall)
			{
				currentSpeed *= poolHitRestitution;
			}

			Debug.Log("Brick hit");
		}
		else if (tag == "Wall")
		{
			direction = Vector3.Reflect(direction, hit.normal).normalized;

			if (isPoolBall)
			{
				currentSpeed *= poolHitRestitution;
			}

			Debug.Log("Wall hit");
		}
		else if (tag == "PoolBall")
		{
			if (!otherObject.TryGetComponent(out scrBall otherBall)) return;

			Vector3 delta = otherBall.transform.position - transform.position;
			delta.y = 0f;
			float distSq = delta.sqrMagnitude;
			if (distSq < 0.0001f) return;

			Vector3 normal = delta.normalized;

			Vector3 velocityOfThisBall = direction * currentSpeed;
			Vector3 velocityOfOtherBall = otherBall.direction * otherBall.currentSpeed;

			float velocityComponentThisAlongNormal = Vector3.Dot(velocityOfThisBall, normal);
			float velocityComponentOtherAlongNormal = Vector3.Dot(velocityOfOtherBall, normal);

			float newVelocityComponentThisAlongNormal, newVelocityComponentOtherAlongNormal;

			if (!isPoolBall) // Normal ball hitting pool ball: maintain full momentum for this ball.
			{
				newVelocityComponentThisAlongNormal = velocityComponentThisAlongNormal;         // No loss for cue ball.
				newVelocityComponentOtherAlongNormal = velocityComponentThisAlongNormal + velocityComponentOtherAlongNormal;    // Full transfer to target.
			}
			else // Pool ball hitting pool ball: exchange momentum.
			{
				newVelocityComponentThisAlongNormal = velocityComponentOtherAlongNormal;
				newVelocityComponentOtherAlongNormal = velocityComponentThisAlongNormal;
			}

			Vector3 newVelocityOfThisBall = velocityOfThisBall - normal * velocityComponentThisAlongNormal + normal * newVelocityComponentThisAlongNormal;
			Vector3 newVelocityOfOtherBall = velocityOfOtherBall - normal * velocityComponentOtherAlongNormal + normal * newVelocityComponentOtherAlongNormal;

			currentSpeed = newVelocityOfThisBall.magnitude;
			direction = currentSpeed > 0.01f ? newVelocityOfThisBall.normalized : Vector3.zero;

			otherBall.currentSpeed = newVelocityOfOtherBall.magnitude;
			otherBall.direction = otherBall.currentSpeed > 0.01f ? newVelocityOfOtherBall.normalized : Vector3.zero;

			// Separate the balls slightly to prevent sticking.
			Vector3 separation = normal * 0.05f;
			transform.position -= separation * 0.5f;
			otherBall.transform.position += separation * 0.5f;

			Debug.Log($"Pool ball collision – {name} → {otherBall.name}");
		}
	}

	// UpdateSpeed method: Sets speed based on paddle hit count.
	// Purpose: Increases speed level for normal balls after paddle hits.
	private void UpdateSpeed(int hits)
	{
		speedLevel = Mathf.Clamp(hits, 0, speedList.Count - 1);
		currentSpeed = speedList[speedLevel];
	}

	// Launch method: Starts the ball moving.
	// Purpose: Initializes direction and speed for normal balls.
	private void Launch()
	{
		direction = new Vector3(1f, 0f, 1f).normalized;
		speedLevel = 1;
		currentSpeed = speedList[1];
	}

	// ResetBall method: Public method to reset or relaunch the ball.
	// Purpose: Allows external scripts to reset the ball's state.
	public void ResetBall(Vector3 newDirection, float newSpeed)
	{
		direction = newDirection.normalized;
		currentSpeed = newSpeed;
		paddleHitCount = 0;
		speedLevel = 1;
	}
}