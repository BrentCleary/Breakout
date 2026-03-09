using System.Collections.Generic;
using UnityEngine;

public class PoolBallSpawner : MonoBehaviour
{
	[Header("Ball")]
	public Transform poolBallPrefab;

	[Header("Spawn Settings")]
	public int numberOfBalls = 15;

	[Header("Presets")]
	public bool preset1;
	public bool preset3;
	public bool preset6;
	public bool preset10;
	public bool preset15;

	private void Start()
	{
		ApplyPreset();
		SpawnBalls();
	}

	private void ApplyPreset()
	{
		if (preset1) { numberOfBalls = 1; }
		if (preset3) { numberOfBalls = 3; }
		if (preset6) { numberOfBalls = 6; }
		if (preset10) { numberOfBalls = 10; }
		if (preset15) { numberOfBalls = 15; }
	}

	private void SpawnBalls()
	{
		if (poolBallPrefab == null)
		{
			return;
		}

		float diameter = poolBallPrefab.localScale.x;
		float radius = diameter * 0.5f;

		float Dx = diameter;
		float Dy = diameter * Mathf.Sqrt(3f) * 0.5f;

		int spawned = 0;
		int row = 0;

		while (spawned < numberOfBalls)
		{
			int ballsInRow = row + 1;

			float y = -row * Dy;
			float xStart = -(ballsInRow - 1) * Dx * 0.5f;

			for (int i = 0; i < ballsInRow; i++)
			{
				if (spawned >= numberOfBalls)
				{
					break;
				}

				float x = xStart + i * Dx;

				Vector3 position = new Vector3(x, 0f, y);
				Instantiate(poolBallPrefab, position, Quaternion.identity, transform);

				spawned++;
			}

			row++;
		}
	}
}