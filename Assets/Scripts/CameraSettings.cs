using UnityEngine;

public class CameraSettings : MonoBehaviour
{

  Transform squareLevelTransform;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
  {
        
  }

  // Update is called once per frame
  void Update()
  {
        
  }

  void SetTransformValues()
  {
    squareLevelTransform.position = new Vector3(0, 625, -365);
    squareLevelTransform.rotation = Quaternion.Euler(71.801f, 0, 0);
    squareLevelTransform.localScale = new Vector3(1, 1, 1);
	}

}
