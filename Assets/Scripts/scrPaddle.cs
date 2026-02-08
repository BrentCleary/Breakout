using UnityEngine;
using UnityEngine.InputSystem;

public class scrPaddle : MonoBehaviour
{
	public float moveSpeed = 500f;

	public void DetectHit(){
		Debug.Log("Paddle Hit");
	}

	private void Update()
	{
		Movement();
	}

	public void Movement(){

		if (Keyboard.current.aKey.isPressed)
		{
			transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
		}
		if (Keyboard.current.dKey.isPressed)
		{
			transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
		}
	}



}
