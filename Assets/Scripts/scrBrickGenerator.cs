using System.Collections.Generic;
using UnityEngine;

public class scrBlockGenerator : MonoBehaviour
{
  [SerializeField] private GameObject brickPrefab;

	//Dimensions of the grid of blocks
	// X 100, Y 20, Z 20

	// Block Spawn Parameters
	// Z Position - Increment by 20 for each block
	// Y Position - Increment by 20 for each block
	// X Position - Increment by 100 for each block

	Vector3 initSpawnPositions = new Vector3(0, 0, 0);

	//* ---------------------------------------- PROPERTIES ----------------------------------------

	[SerializeField] public List<GameObject> brickList;
	[SerializeField] public List<scrBrick> scrBrickList;
	[SerializeField] public GameObject[,] brickArr;
	public Transform brickArrTransform;

	[Header("Brick Array Settings")] // ! Array Size Controls
	public int arrColLen = 3;                                              // Array Dimensions - Column
	public int arrRowLen = 3;                                                 // Array Dimensions - Row
	public int arrBrickCount => arrColLen * arrRowLen;                         // arrColLen * arrRowLen
	public float brickSpacing = 2f;                                             // Space Between Nodes

	public List<int> startNDValMap;
	public List<int> crntNDValMap;



	public void CreateBoard()  //? Called in GameManager                      // Instantiates Variables, Calls Methods Below
	{
		brickArr = new GameObject[arrColLen, arrRowLen];
		brickArrTransform = gameObject.transform;
		brickList = new List<GameObject>();
		scrBrickList = new List<scrBrick>();

		InstantiateNodes();
		SetNodeTransformPosition();

		BuildNodeArray();
		AdjNodeMapper();
	}
	// *---------------------------------------- CreateBoard Methods ----------------------------------------
	public void InstantiateNodes()                                                  // Instantiates Nodes, Assigns names and values, Adds them to brickList
	{
		for (int i = 0; i < arrBrickCount; i++)
		{
			GameObject node = Instantiate(brickPrefab, brickArrTransform);
			node.name = $"Node ({i})";
			node.GetComponent<scrBrick>().brickID = i;                                   //! Sets brickID in scrBrick
			brickList.Add(node);
			scrBrickList.Add(node.GetComponent<scrBrick>());
		}
	}
	public void SetNodeTransformPosition()                                          // Set Node transform.position
	{
		int NDCount = 0;                                                              // Increments Node reference in brickList 
		for (int i = 0; i < arrColLen; i++)
		{                                          // Assigns positions to each gNode in brickArr
			for (int j = 0; j < arrRowLen; j++)
			{
				brickList[NDCount].transform.position = new Vector3(i * brickSpacing, 0, j * brickSpacing);
				NDCount++;
			}
		}
	}

	public void BuildNodeArray()                                                    // Generate Array using Length x Row using nodes in brickList
	{
		int NDCount = 0;                                                            // Increments Node reference in brickList 
		for (int i = 0; i < arrColLen; i++)
		{                                          // Assigns positions to each gNode in brickArr
			for (int j = 0; j < arrRowLen; j++)
			{
				brickArr[i, j] = brickList[NDCount];                                         // Set curent brickList object to current array position

				// Maps Board Array position for reference
				scrBrick NDScr = scrBrickList[NDCount];
				NDScr.arrPos[0] = i;
				NDScr.arrPos[1] = j;

				// Add Array Position to Node Name
				brickArr[i, j].name = $"{brickArr[i, j].name} [{i},{j}]";

				NDCount++;
			}
		}
	}

	public void AdjNodeMapper()                                           // Loop over scrBrickList, assign adjNodes
	{
		foreach (scrBrick NDScr in scrBrickList)
		{

			int[] arrPos = NDScr.arrPos;

			if (arrPos[0] == 0) { NDScr.LNDScr = null; }                                   // left index out of range 
			else
			{                                                                      // left index is node
				NDScr.LNDScr = brickArr[arrPos[0] - 1, arrPos[1]].GetComponent<scrBrick>();
			}

			if (arrPos[0] == arrColLen - 1) { NDScr.RNDScr = null; }                        // right index out of range 
			else
			{                                                                      // right index is node 
				NDScr.RNDScr = brickArr[arrPos[0] + 1, arrPos[1]].GetComponent<scrBrick>();
			}

			if (arrPos[1] == 0) { NDScr.BNDScr = null; }                                   // bottom index out of range
			else
			{                                                                      // bottom index is  node
				NDScr.BNDScr = brickArr[arrPos[0], arrPos[1] - 1].GetComponent<scrBrick>();
			}

			if (arrPos[1] == arrRowLen - 1) { NDScr.TNDScr = null; }                       // top index out of range
			else
			{                                                                      // top index is node
				NDScr.TNDScr = brickArr[arrPos[0], arrPos[1] + 1].GetComponent<scrBrick>();
			}

		}
	}




	public void UpdateBoardNodeValues()  //? Called GameManager               // Calls Functions Step1, Step2, Step3 
	{
		List<int> NDValMap = Create_NDValueMap_Step1();
		List<int> NDValMapUpdate = Set_NDValueMap_Step2(NDValMap);                 // Update NDValMap based on position and board state
		Update_NDValues_Step3(NDValMapUpdate);
	}


	// *---------------------------------------- Node Value Update Methods ----------------------------------
	public List<int> Create_NDValueMap_Step1()
	{                                      // Displays Array based on nodeValues

		List<int> NDValMap = new List<int>();                                           // List to hold update values for arrayNodes

		foreach (scrBrick NDScr in scrBrickList)
		{                                        // Loops over all nodes in brickList    
			if (NDScr.shpVal == NDScr.shpValList[0])
			{                                     // No Sheep
				NDValMap.Add(NDScr.NDValList[4]);                                           // Assigns max NodeValue to each position
				NDScr.libVal = NDScr.libValList[1];
			}
			else
			{                                                                         // Sheep placed
				NDValMap.Add(NDScr.NDValList[0]);                                           // Assigns min NodeValue to each position
				NDScr.libVal = NDScr.libValList[0];
			}
		}

		return NDValMap;
	}


	public List<int> Set_NDValueMap_Step2(List<int> NDValMap)
	{                    // Maps nodeValues for updating in Step 3

		List<int> newValMap = NDValMap;

		int crntNDVal = 0;

		// Map node values to NDValMap based on current board state
		for (int i = 0; i < arrColLen; i++)
		{         // Assigns positions to each gNode in brickArr
			for (int j = 0; j < arrRowLen; j++)
			{

				// Left Index Check
				if (j == 0)
				{      // Check left index is not out of range 
					newValMap[crntNDVal] -= 1;
				}
				else if (j != 0)
				{      // Check left position
					if (brickArr[i, j - 1].GetComponent<scrBrick>().libVal == 0)
					{     // Check left crntNDVal is not null
						newValMap[crntNDVal] -= 1;
					}
				}

				// Right Index Check
				if (j == arrRowLen - 1)
				{      // Check right crntNDVal is not out of range 
					newValMap[crntNDVal] -= 1;
				}
				else if (j != arrRowLen - 1)
				{       // Check right position
					if (brickArr[i, j + 1].GetComponent<scrBrick>().libVal == 0)
					{     // Check right crntNDVal is not null
						newValMap[crntNDVal] -= 1;
					}
				}

				// Top Index Check
				if (i == 0)
				{      // Check top crntNDVal is not out of range 
					newValMap[crntNDVal] -= 1;
				}
				else if (i != 0)
				{      //  Check top position
					if (brickArr[i - 1, j].GetComponent<scrBrick>().libVal == 0)
					{
						newValMap[crntNDVal] -= 1;
					}
				}

				// Bottom Index Check
				if (i == arrColLen - 1)
				{      // Check bottom crntNDVal is not out of range 
					newValMap[crntNDVal] -= 1;
				}
				else if (i != arrColLen - 1)
				{      // Check bottom position 
					if (brickArr[i + 1, j].GetComponent<scrBrick>().libVal == 0)
					{
						newValMap[crntNDVal] -= 1;
					}
				}
				crntNDVal += 1;
			}
		}

		return newValMap;
	}
	public void Update_NDValues_Step3(List<int> NDValMap)                         // Sets Node Display values to Map Values
	{
		int arrayIndex = 0;

		// Map NDValMap values to NDArray
		for (int i = 0; i < arrColLen; i++)
		{        // Assigns positions to each gNode in brickArr
			for (int j = 0; j < arrRowLen; j++)
			{
				GameObject crntND = brickArr[i, j];
				scrBrick crntNDScr = crntND.GetComponent<scrBrick>();

				// Debug.Log("NDmapValue" + arrayIndex + " is " + NDValMap[arrayIndex]);

				if (NDValMap[arrayIndex] < 0)
				{
					crntNDScr.NDVal = 0;
				}
				else { crntNDScr.NDVal = NDValMap[arrayIndex]; }

				// Debug.Log(crntND.name + " NDValue is " + crntNDScr.NDValue);
				arrayIndex += 1;
			}
		}
	}


	// *---------------------------------------- Node Display Update Method ---------------------------------
	public void UpdateBoardDisplay()  //? Called in GameManager               // Updated Display of Nodes
	{
		foreach (GameObject crntNode in brickList)
		{
			scrBrick crntNDScrpt = crntNode.GetComponent<scrBrick>();
			crntNDScrpt.UpdateNodeDisplay();
		}
	}


	// *---------------------------------------- Ko Check Methods ---------------------------------
	public List<int> Create_ShpValMap()  //? Called in GameManager               // Displays Array based on nodeValues
	{
		List<int> ShpValMap = new List<int>();                                         // List to hold update values for arrayNodes

		foreach (GameObject crntND in brickList)
		{                                         // Loops over all nodes in brickList    
			scrBrick NDScr = crntND.GetComponent<scrBrick>();                    // Set liberty value based on masterNode
			ShpValMap.Add(NDScr.shpVal);
		}

		return ShpValMap;
	}


	public bool Check_Map_For_Ko(List<int> prevShpValMap, int ND_ID, int ShpVal)        // Map of board state before last move, ND_ID and Value
	{
		bool isKo = false;                                                              // Ko is initially set false

		List<int> newShpValMap = prevShpValMap.ToList();                                // Create a new copy of ShpValMap for updating and comparing
		newShpValMap[ND_ID] = ShpVal;                                                   // Change List val to ShpVal at ND_ID index

		bool sequenceCheck = newShpValMap.SequenceEqual(prevShpValMap);
		Debug.Log("sequenceCheck: " + sequenceCheck);

		List<int> differingIndices = FindDifferences(newShpValMap, prevShpValMap);
		if (differingIndices.Count > 0)
		{
			Debug.Log("Sequences differ at indices: " + string.Join(", ", differingIndices));
		}
		else
		{
			Debug.Log("Sequences are identical.");
		}

		if (sequenceCheck)
		{                                              // Compare to prev ShpValMap for Board state
			isKo = true;                                                                // If they match, the move will violate KO rules
			Debug.Log("** KO is True **");
			LogListValues<int>(prevShpValMap, "prevShpValMap");
			LogListValues<int>(newShpValMap, "newShpValMap");
		}
		else
		{
			isKo = false;
		}

		return isKo;                                                                    // Return the Ko bool Value
	}

	// DEBUG METHOD FOR KO CHECK METHODS
	List<int> FindDifferences(List<int> newShpValMap, List<int> prevShpValMap)
	{
		List<int> differences = new List<int>();

		for (int i = 0; i < newShpValMap.Count; i++)
		{
			if (newShpValMap[i] != prevShpValMap[i])
			{
				differences.Add(i); // Record the index where they differ
			}
		}

		return differences;

	}


	void LogListValues<T>(List<T> list, string listName)
	{
		string values = string.Join(", ", list);
		Debug.Log($"{listName} values: [{values}]");
	}



	/* Depreciated Methods - Commented out for reference
	// GNODE LIST MAPPER
	// public List<int> AdjacentNodeMapIndexer(List<int> NDValMap)
	// {
	//     // Counter to increment through index in NDValMap
	//     int index = 0;     

	//     // Map node values to NDValMap based on current board state
	//     foreach(GameObject node in brickList)
	//     {
	//         scrBrick nScript = node.GetComponent<scrBrick>();
	//         List<scrBrick> adjList = nScript.adjscrBrickList;

	//         foreach(scrBrick adjScript in adjList)
	//         {
	//             if(adjScript == null || adjScript.libertyVal == 0)
	//             {
	//                 NDValMap[index] -= 1;
	//                 Debug.Log("Adj Script " + adjScript.name + " is null at " + node.name);
	//             }
	//         }

	//         index += 1;
	//     }   

	//     return NDValMap;
	// }




	// --------------------------------------------- // gNodeValue Updater Part 3 ---------------------------------------------
																								//* Calls SetGrassTileDisplay
	// public void NodeValueListUpdater(List<int> NDValMap)
	// {
	//     int arrayIndex = 0;

	//     // Map NDValMap values to NDArray
	//     foreach(GameObject node in brickList)
	//     {         

	//         scrBrick crntNDScr = node.GetComponent<scrBrick>();

	//         if(NDValMap[arrayIndex] < 0) {
	//             crntNDScr.NDValue = 0;
	//         }
	//         else {
	//             crntNDScr.NDValue = NDValMap[arrayIndex];
	//         }

	//         node.GetComponent<scrBrick>().SetGrassTileDisplay();
	//         Debug.Log(node.GetComponent<scrBrick>().name + "'s NDValue is " + node.GetComponent<scrBrick>().NDValue);

	//         arrayIndex += 1;

	//     }

	//     // Debug.Log("brickArr Update Complete");
	// }

	*/

}
