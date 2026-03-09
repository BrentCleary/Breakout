using UnityEngine;

public class PoolBallSpawner : MonoBehaviour  // Back to original name for your setup
{
	[Header("Container")]
	[Tooltip("The cntPoolBall GameObject - balls will spawn as its children")]
	public Transform poolBallContainer;

	[Header("Prefab")]
	[Tooltip("Single PoolBall prefab (with collider & components)")]
	public  Transform poolBallPrefab;
	private Transform poolBallModelPrefab; // Optional: separate visual model for diameter calculation
	public Vector3 scale = new Vector3(1f, 1f, 1f); // Default scale for spawned balls

	[Header("Ball Size Override")]
	[Tooltip("If >0, forces diameter. Else auto from prefab scale.")]
	[Min(0.01f)]
	public float forcedDiameter = 1f;

	[Header("Spawn Count")]
	[Range(1, 50)]
	public int targetCount = 15;

	[Header("Presets")]
	public bool preset1 = false;
	public bool preset3 = false;
	public bool preset6 = false;
	public bool preset10 = false;
	public bool preset15 = false;

	[Header("Debug")]
	public bool logSpawn = true;


	private void Start()
	{
		ApplyPreset();
		ClearContainer();
		SpawnRack();
	}


	private void ApplyPreset()
	{
		if (preset1) targetCount  = 1;
		if (preset3) targetCount  = 3;
		if (preset6) targetCount  = 6;
		if (preset10) targetCount = 10;
		if (preset15) targetCount = 15;
	}


	private float GetDiameter()
	{
		if (forcedDiameter > 0f) return forcedDiameter;
		if (poolBallPrefab == null) return 1f;

		// Use the ROOT prefab's scale — this is usually what you actually want for physics/collision
		Vector3 rootScale = poolBallPrefab.localScale;

		// If root is scaled very small (e.g. 0.025), and model is 40 inside → effective diameter ≈ 40 * 0.025 = 1
		float effectiveDiameter = (rootScale.x + rootScale.y + rootScale.z) / 3f * 40f;

		// Or simpler — just force what makes sense visually/physically
		// return effectiveDiameter;   ← try this first

		Debug.Log($"Pool ball root scale = {rootScale}, model inner scale = 40 → effective diam ≈ {effectiveDiameter:F3}", this);

		return effectiveDiameter > 0.01f ? effectiveDiameter : 1f;
	}


	private void ClearContainer()
	{
		if (poolBallContainer == null) return;

		for (int i = poolBallContainer.childCount - 1; i >= 0; i--)
		{
			Transform child = poolBallContainer.GetChild(i);
#if UNITY_EDITOR
			if (!Application.isPlaying)
				DestroyImmediate(child.gameObject);
			else
				Destroy(child.gameObject);
#else
            Destroy(child.gameObject);
#endif
		}
	}


	private void SpawnRack()
	{
		if (poolBallContainer == null)
		{
			Debug.LogError("poolBallContainer is NULL! Assign cntPoolBall.", this);
			return;
		}

		if (poolBallPrefab == null)
		{
			Debug.LogError("poolBallPrefab is NULL!", this);
			return;
		}

		float diam  = GetDiameter();
		float hStep =  diam;
		float vStep =  diam * Mathf.Sqrt(3f) * 0.5f;
		float yStep = (diam * .5f) + .5f; // Optional: vertical offset to prevent z-fighting if balls are exactly on the same plane
		float gap   = 0.05f; // Optional: small gap to prevent z-fighting if balls are exactly touching

		int spawned = 0;
		int row			= 0;

		while (spawned < targetCount)
		{
			int		ballsInRow = row + 1;
			float rowWidth	 = (ballsInRow - 1f) * hStep;
			float xStart		 = -rowWidth * 0.5f;
			float zPos			 = row * vStep + gap;
			float yPos			 = yStep + gap; // Optional: stagger vertically to prevent z-fighting

			for (int col = 0; col < ballsInRow; col++)
			{
				if (spawned >= targetCount) break;

				float xPos = xStart + col * hStep + gap;
				Vector3 localPos = new Vector3(xPos, yPos, zPos);

				// Instantiate at (0,0,0) local, THEN set localPosition → preserves prefab pivot/scale
				Transform ball = Instantiate(poolBallPrefab, poolBallContainer);
				ball.localPosition = localPos;
				ball.localRotation = Quaternion.identity;
				ball.name = $"PoolBall_{(spawned + 1):D2}_R{row + 1}C{col + 1}";

				spawned++;
			}

			row++;
		}

		if (logSpawn)
		{
			Debug.Log($"[{name}] Spawned {spawned}/{targetCount} balls in {poolBallContainer.name} (diam={diam:F2})", this);
		}
	}


	[ContextMenu("Re-Spawn Rack")]
	public void Respawn()
	{
		ApplyPreset();
		ClearContainer();
		SpawnRack();
	}
}