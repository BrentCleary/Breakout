using UnityEngine;

public class gameManager : MonoBehaviour
{
    float gravity = 0;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
  {
    Physics.gravity = new Vector3(0, -200f, 0);
	  gravity = Physics.gravity.y;
	}

  // Update is called once per frame
  void Update()
  {
        
  }
}
