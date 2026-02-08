using UnityEngine;
using UnityEngine.InputSystem;
public class scrBrick : MonoBehaviour
{

	private bool broken = false; // flag to prevent multiple breaks from multiple contacts;
	public int brickID;
	public int[] arrPos = new int[2];

	public int durability = 2; // how many hits it takes to break this brick

	Transform objBrick;
	MeshRenderer meshRend;

	public void Start(){
			objBrick = gameObject.GetComponentInChildren<Transform>();
			meshRend = objBrick.GetComponentInChildren<MeshRenderer>();

	}

	public void Break()
	{
		if (broken) return; // prevents double-trigger from multiple contacts
		broken = true;

		Destroy(gameObject);
	}


	private void OnCollisionEnter(Collision collision)
	{
		scrBall ball = collision.collider.GetComponent<scrBall>();
		if (ball != null)
		{
			Debug.Log("Brick Hit");
			
			ball.Rebound(collision);

			durability = durability - 1;

			if (durability <= 0) {
				Break();
			}
		}
	}


}
