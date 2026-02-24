using UnityEngine;
using UnityEngine.InputSystem;
public class scrBrick : MonoBehaviour
{

	private bool broken = false; // flag to prevent multiple breaks from multiple contacts;
	public int	 brickID;

	public GameObject cntBrickGenerator;
	public int[] arrPos = new int[2];

	public int durability = 2; // how many hits it takes to break this brick

	Transform		 objBrick;
	MeshRenderer meshRend;


	public void Awake()
	{
		if (arrPos == null || arrPos.Length != 2)
		{
			arrPos = new int[2];
			Debug.Log($"Fixed arrPos size on {gameObject.name} (was invalid)", this);
		}
	}

	private void Update()
	{
		if (durability <= 0)
		{
			broken = true;
			Destroy(gameObject);
		}

	}

	public void Start()
	{

		cntBrickGenerator = transform.parent.gameObject;

		//objBrick = gameObject.GetComponentInChildren<Transform>();
		//meshRend = objBrick.GetComponentInChildren<MeshRenderer>();

	}

	public void Break()
	{
		if (broken) return; // prevents double-trigger from multiple contacts
		
		broken = true;
		Destroy(gameObject);
		
	}


}
