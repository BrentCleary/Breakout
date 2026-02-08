using UnityEngine;
using UnityEngine.InputSystem;

public class scrPaddle : MonoBehaviour
{
	public float moveSpeed = 50f;

	public void DetectHit(){
		Debug.Log("Paddle Hit");
	}

}
