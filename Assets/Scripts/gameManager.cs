using UnityEngine;

public class gameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      Physics.gravity = new Vector3(0, -50f, 0);
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
