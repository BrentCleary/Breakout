using UnityEngine;
using UnityEngine.InputSystem;
public class scrBrick : MonoBehaviour
{

	private bool broken;
	public int   brickID;

	public void Break()
	{
		if (broken) return; // prevents double-trigger from multiple contacts
		broken = true;

		Destroy(gameObject);
	}


	public void DetectHit()
	{
		Debug.Log("Brick Hit");
	}
}
