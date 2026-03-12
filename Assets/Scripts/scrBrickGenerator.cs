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
	public Transform brickArrTransform;

	[Header("Brick Array Settings")] // ! Array Size Controls
	public int arrColLen = 7;                                              // Array Dimensions - Column
	public int arrRowLen = 7;                                                 // Array Dimensions - Row
	
	[SerializeField] public GameObject[,] brickArr;
	private int	  arrBrickCount => arrColLen * arrRowLen;                         // arrColLen * arrRowLen

	[Header("Brick Spawn Spacing")] // ! Brick Spawn Position Controls
	// This needs to be edited to be dynamic based on the size of the prefab, or the prefab needs to be made to fit these parameters
	private float spawnX = 100f;                                             // Space Between Nodes
	private float spawnY = 10.5f;
	private float spawnZ = 20f;

	[Header("Board Spawn Offset")] // ! Board Spawn Position Controls
	private int brickSpawnOffsetX = 0;
	private int brickSpawnOffsetY = 0;
	private int brickSpawnOffsetZ = 0;

	public List<int> startBrickValMap;
	public List<int> crntBrickValMap;


	[Header("Select Brick Spawn Preset")]
	public bool presetClassic = true;
	public bool presetWaterfall = false;


	[Header("Continuous Waterfall Spawning PARAMETERS")]
	[SerializeField, Tooltip("Master switch - continuous spawning happens only when true and presetWaterfall is active")]
	public bool isWaterfallSpawningActive = true;

	[SerializeField, Tooltip("Time between spawn attempt cycles (seconds)")]
	private float waterfallSpawnInterval = .5f;													// PARAMETER

	[SerializeField, Tooltip("Chance a spawn attempt actually succeeds (0-1)")]
	private float waterfallSpawnSuccessChance = 1f;											// PARAMETER

	[SerializeField, Tooltip("World Z position where all waterfall bricks appear (fixed row)")]
	private float waterfallFixedRowZPosition = 300f;                    // PARAMETER

	[SerializeField, Tooltip("Y position (world space) where newly spawned bricks appear")]
	private float waterfallSpawnStartY = 400f;                          // PARAMETER - top of waterfall

	[SerializeField, Tooltip("If true, new bricks slowly move downward after spawn")]
	private bool shouldBricksFallDownward = false;                      // PARAMETER - OPTIONAL FEATURE

	[SerializeField, Tooltip("Downward speed if shouldBricksFallDownward = true (units/sec)")]
	private float brickFallSpeed = 30f;                                 // PARAMETER

	private float waterfallSpawnTimer = 0f;															// Tracks time until next spawn attempt





	// * ---------------------------------------- EVENTS ----------------------------------------

	private void Start()
	{
		ApplyPreset();
		CreateBoard();
	}

	private void Update()
	{
		SpawnWaterFallUpdate();
	}


	public void SpawnWaterFallUpdate()
	{
		if (!presetWaterfall) return;
		if (!isWaterfallSpawningActive) return;

		waterfallSpawnTimer += Time.deltaTime;

		if (waterfallSpawnTimer >= waterfallSpawnInterval)
		{
			waterfallSpawnTimer -= waterfallSpawnInterval;  // more accurate than =0

			if (Random.value <= waterfallSpawnSuccessChance)
			{
				SpawnWaterfallBrickAtRandomColumn();

			}
		}
	}





	// ------------------------------------------- Board Preset Parameters ----------------------------------------

	public void CreateBoard() //? Called in GameManager // Instantiates Variables, Calls Methods Below
	{
		brickArrTransform = gameObject.transform; // Set parent transform for bricks to this object
		brickArr = new GameObject[arrColLen, arrRowLen];

		brickList.Clear();
		scrBrickList.Clear();

		if (presetClassic)
		{
			InstantiateBricks();
			SetBrickTransformPosition();
			BuildBrickArray();
		}
		else if (presetWaterfall)
		{
			// Waterfall: start empty — bricks added continuously via SpawnWaterfallBrickAtRandomColumn()
			Debug.Log("Waterfall preset active: board starts empty. Bricks will appear over time if isWaterfallSpawningActive = true.");
		}
	}

	private void ApplyPreset()
	{
		if (presetClassic)	 { SetClassicParameters(); }
		if (presetWaterfall) { SetWaterfallParameters(); }
	}
	public void SetClassicParameters()
	{
		arrColLen = 7;
		arrRowLen = 7;

		spawnX = 100f;
		spawnY = 10.5f;
		spawnZ = 20f;

		brickSpawnOffsetX = -300;
		brickSpawnOffsetY = 0;
		brickSpawnOffsetZ = 300;
	}

	public void SetWaterfallParameters()
	{
		arrColLen = 7;
		arrRowLen = 1;

		spawnX = 100f;
		spawnY = 200f;
		spawnZ = 20f;

		brickSpawnOffsetX = -300;
		brickSpawnOffsetY = 300;
		brickSpawnOffsetZ = 300;

	}







	// *---------------------------------------- Classic Board Methods ----------------------------------------
	public void InstantiateBricks()                                                  // Instantiates Nodes, Assigns names and values, Adds them to brickList
	{
		for (int i = 0; i < arrBrickCount; i++) {
			GameObject brick = Instantiate(brickPrefab, brickArrTransform);
			brick.name = $"cntBrick ({i})";
			brickList.Add(brick);
			
			scrBrick brickScr = brick.GetComponentInChildren<scrBrick>();
			brickScr.brickID = i;																												//! Sets brickID in scrBrick
			scrBrickList.Add(brickScr);
			Debug.Log("Instantiated " + brick.name);
		}
	}
	public void SetBrickTransformPosition()                                          // Set Node transform.position
	{
		int counter = 0;                                                              // Increments Node reference in brickList 
		for (int i = 0; i < arrColLen; i++) {                                          // Assigns positions to each gNode in brickArr
			for (int j = 0; j < arrRowLen; j++) {
				brickList[counter].transform.position = 
					new Vector3	(	
						spawnX * i + brickSpawnOffsetX, 
						spawnY		 + brickSpawnOffsetY, 
						spawnZ * j + brickSpawnOffsetZ 
					);
				Debug.Log(brickList[counter].name + " position set to " + brickList[counter].transform.position);
				counter++;
			}
		}
	}

	public void BuildBrickArray()                                                    // Generate Array using Length x Row using nodes in brickList
	{
		int counter = 0;                                                            // Increments Node reference in brickList 
		for (int i = 0; i < arrColLen; i++) {                                          // Assigns positions to each gNode in brickArr
			for (int j = 0; j < arrRowLen; j++) {
				Debug.Log($"BBA brick {i},{j} = " + brickList[counter].name);
				brickArr[i, j] = brickList[counter];                                         // Set curent brickList object to current array position
				Debug.Log(brickArr[i, j].name + " added to brickArr at position [" + i + "," + j + "]");

				Debug.Log("scrBrickList count: " + scrBrickList.Count + " | brickList count: " + brickList.Count);
				// Maps Brick Array position for reference
				scrBrick brickScr = scrBrickList[counter];

				if (brickScr == null)
				{
					Debug.LogError(
							$"[BUILD] Null scrBrick at index {counter} (array pos [{i},{j}])! " +
							$"Was added as brickID {counter}. GameObject gone?",
							gameObject
					);
					counter++;
					continue;
				}

				Debug.Log(brickScr.brickID);
				brickScr.arrPos[0] = i;
				brickScr.arrPos[1] = j;

				// Add Array Position to Node Name
				brickList[counter].name = $"{brickList[counter].name} [{i},{j}]";

				counter++;
			}
		}
	}





	// *---------------------------------------- Waterfall Board Methods ----------------------------------------

	// PURPOSE: Spawns exactly one brick in a random column (waterfall style)
	//          Follows similar steps to classic: instantiate → position → build array
	private void SpawnWaterfallBrickAtRandomColumn()
	{
		int randomColumn = Random.Range(0, arrColLen);

		// NEW: prevent spawning on occupied column (single row → one brick per column max)
		int targetRow = 0;
		//if (brickArr[randomColumn, targetRow] != null)
		//{
		//	Debug.Log($"Waterfall: column {randomColumn} already has a brick — spawn skipped");
		//	return;
		//}

		// Step 1: Instantiate (mimic InstantiateBricks style)
		GameObject brick = Instantiate(brickPrefab, brickArrTransform);
		int brickID = brickList.Count;
		brick.name = $"cntBrick ({brickID})";
		brickList.Add(brick);

		scrBrick brickScr = brick.GetComponentInChildren<scrBrick>();
		if (brickScr != null)
		{
			brickScr.brickID = brickID;
			scrBrickList.Add(brickScr);
		}
		else
		{
			Debug.LogWarning("No scrBrick component found on new brick", brick);
		}

		// Step 2: Position (mimic SetBrickTransformPosition style — but single position)
		Vector3 position = new Vector3(
				spawnX * randomColumn + brickSpawnOffsetX,
				waterfallSpawnStartY,
				waterfallFixedRowZPosition   // all bricks in same Z row
		);
		brick.transform.position = position;
		Debug.Log(brick.name + " waterfall position set to " + position);

		// Step 3: Assign to array & set arrPos (mimic BuildBrickArray style)
		brickArr[randomColumn, targetRow] = brick;

		if (brickScr != null)
		{
			brickScr.arrPos[0] = randomColumn;
			brickScr.arrPos[1] = targetRow;
			brick.name += $" [{randomColumn},{targetRow}]";
		}

		Debug.Log($"Waterfall brick spawned at column {randomColumn}  pos {position}");
	}


	//// PURPOSE: Waterfall version - only instantiates one brick at a time when called
	////           (does NOT fill the whole grid)
	//public void WaterfallInstantiateBricks()
	//{
	//	// We don't loop here anymore — this method is now called once per new brick
	//	// Actual instantiation happens inside SpawnWaterfallBrick()
	//	// This method exists mainly to keep the naming pattern consistent
	//	Debug.Log("WaterfallInstantiateBricks called - waiting for continuous spawn requests");
	//}
	//
	//// PURPOSE: Waterfall version - sets position for a newly spawned brick
	////           Called immediately after instantiation in SpawnWaterfallBrick()
	//public void WaterfallSetBrickTransformPosition(GameObject newBrick, int columnIndex)
	//{
	//	Vector3 position = new Vector3(
	//			spawnX * columnIndex + brickSpawnOffsetX,
	//			waterfallSpawnStartY,
	//			waterfallFixedRowZPosition   // FIXED Z - single row waterfall
	//	);
	//
	//	newBrick.transform.position = position;
	//
	//	Debug.Log(newBrick.name + " waterfall position set to " + position);
	//}
	//
	//// PURPOSE: Waterfall version - assigns array position and updates lists for one brick
	////           Called after position is set
	//public void WaterfallBuildBrickArray(GameObject newBrick, int columnIndex, int brickGlobalID)
	//{
	//	int targetRowIndex = 0;  // always row 0 for single-row waterfall
	//
	//	if (brickArr[columnIndex, targetRowIndex] != null)
	//	{
	//		Debug.LogWarning($"Waterfall position [{columnIndex},{targetRowIndex}] already occupied - skipping array assignment");
	//		return;
	//	}
	//
	//	brickArr[columnIndex, targetRowIndex] = newBrick;
	//
	//	scrBrick brickScr = newBrick.GetComponentInChildren<scrBrick>();
	//	if (brickScr != null)
	//	{
	//		brickScr.arrPos[0] = columnIndex;
	//		brickScr.arrPos[1] = targetRowIndex;
	//
	//		// Improve name for debugging
	//		newBrick.name += $" [{columnIndex},{targetRowIndex}]";
	//	}
	//
	//	Debug.Log($"Waterfall brick added to brickArr at [{columnIndex},{targetRowIndex}]");
	//}









	/*public void AdjNodeMapper()                                           // Loop over scrBrickList, assign adjNodes
	{
		foreach (scrBrick brickScr in scrBrickList)
		{

			int[] arrPos = brickScr.arrPos;

			if (arrPos[0] == 0) { brickScr.LNDScr = null; }                                   // left index out of range 
			else
			{                                                                      // left index is brick
				brickScr.LNDScr = brickArr[arrPos[0] - 1, arrPos[1]].GetComponent<scrBrick>();
			}

			if (arrPos[0] == arrColLen - 1) { NDScr.RNDScr = null; }                        // right index out of range 
			else
			{                                                                      // right index is brick 
				NDScr.RNDScr = brickArr[arrPos[0] + 1, arrPos[1]].GetComponent<scrBrick>();
			}

			if (arrPos[1] == 0) { NDScr.BNDScr = null; }                                   // bottom index out of range
			else
			{                                                                      // bottom index is  brick
				NDScr.BNDScr = brickArr[arrPos[0], arrPos[1] - 1].GetComponent<scrBrick>();
			}

			if (arrPos[1] == arrRowLen - 1) { NDScr.TNDScr = null; }                       // top index out of range
			else
			{                                                                      // top index is brick
				NDScr.TNDScr = brickArr[arrPos[0], arrPos[1] + 1].GetComponent<scrBrick>();
			}

		}
	}*/




	//public void UpdateBoardNodeValues()  //? Called GameManager               // Calls Functions Step1, Step2, Step3 
	//{
	//	List<int> NDValMap = Create_NDValueMap_Step1();
	//	List<int> NDValMapUpdate = Set_NDValueMap_Step2(NDValMap);                 // Update NDValMap based on position and board state
	//	Update_NDValues_Step3(NDValMapUpdate);
	//}


	// *---------------------------------------- Node Value Update Methods ----------------------------------
	//public List<int> Create_NDValueMap_Step1()
	//{                                      // Displays Array based on nodeValues

	//	List<int> NDValMap = new List<int>();                                           // List to hold update values for arrayNodes

	//	foreach (scrBrick NDScr in scrBrickList)
	//	{                                        // Loops over all nodes in brickList    
	//		if (NDScr.shpVal == NDScr.shpValList[0])
	//		{                                     // No Sheep
	//			NDValMap.Add(NDScr.NDValList[4]);                                           // Assigns max NodeValue to each position
	//			NDScr.libVal = NDScr.libValList[1];
	//		}
	//		else
	//		{                                                                         // Sheep placed
	//			NDValMap.Add(NDScr.NDValList[0]);                                           // Assigns min NodeValue to each position
	//			NDScr.libVal = NDScr.libValList[0];
	//		}
	//	}

	//	return NDValMap;
	//}


	//public List<int> Set_NDValueMap_Step2(List<int> NDValMap)
	//{                    // Maps nodeValues for updating in Step 3

	//	List<int> newValMap = NDValMap;

	//	int crntNDVal = 0;

	//	// Map brick values to NDValMap based on current board state
	//	for (int i = 0; i < arrColLen; i++)
	//	{         // Assigns positions to each gNode in brickArr
	//		for (int j = 0; j < arrRowLen; j++)
	//		{

	//			// Left Index Check
	//			if (j == 0)
	//			{      // Check left index is not out of range 
	//				newValMap[crntNDVal] -= 1;
	//			}
	//			else if (j != 0)
	//			{      // Check left position
	//				if (brickArr[i, j - 1].GetComponent<scrBrick>().libVal == 0)
	//				{     // Check left crntNDVal is not null
	//					newValMap[crntNDVal] -= 1;
	//				}
	//			}

	//			// Right Index Check
	//			if (j == arrRowLen - 1)
	//			{      // Check right crntNDVal is not out of range 
	//				newValMap[crntNDVal] -= 1;
	//			}
	//			else if (j != arrRowLen - 1)
	//			{       // Check right position
	//				if (brickArr[i, j + 1].GetComponent<scrBrick>().libVal == 0)
	//				{     // Check right crntNDVal is not null
	//					newValMap[crntNDVal] -= 1;
	//				}
	//			}

	//			// Top Index Check
	//			if (i == 0)
	//			{      // Check top crntNDVal is not out of range 
	//				newValMap[crntNDVal] -= 1;
	//			}
	//			else if (i != 0)
	//			{      //  Check top position
	//				if (brickArr[i - 1, j].GetComponent<scrBrick>().libVal == 0)
	//				{
	//					newValMap[crntNDVal] -= 1;
	//				}
	//			}

	//			// Bottom Index Check
	//			if (i == arrColLen - 1)
	//			{      // Check bottom crntNDVal is not out of range 
	//				newValMap[crntNDVal] -= 1;
	//			}
	//			else if (i != arrColLen - 1)
	//			{      // Check bottom position 
	//				if (brickArr[i + 1, j].GetComponent<scrBrick>().libVal == 0)
	//				{
	//					newValMap[crntNDVal] -= 1;
	//				}
	//			}
	//			crntNDVal += 1;
	//		}
	//	}

	//	return newValMap;
	//}



	//public void Update_NDValues_Step3(List<int> NDValMap)                         // Sets Node Display values to Map Values
	//{
	//	int arrayIndex = 0;

	//	// Map NDValMap values to NDArray
	//	for (int i = 0; i < arrColLen; i++)
	//	{        // Assigns positions to each gNode in brickArr
	//		for (int j = 0; j < arrRowLen; j++)
	//		{
	//			GameObject crntND = brickArr[i, j];
	//			scrBrick crntNDScr = crntND.GetComponent<scrBrick>();

	//			// Debug.Log("NDmapValue" + arrayIndex + " is " + NDValMap[arrayIndex]);

	//			if (NDValMap[arrayIndex] < 0)
	//			{
	//				crntNDScr.NDVal = 0;
	//			}
	//			else { crntNDScr.NDVal = NDValMap[arrayIndex]; }

	//			// Debug.Log(crntND.name + " NDValue is " + crntNDScr.NDValue);
	//			arrayIndex += 1;
	//		}
	//	}
	//}


	//// *---------------------------------------- Node Display Update Method ---------------------------------
	//public void UpdateBoardDisplay()  //? Called in GameManager               // Updated Display of Nodes
	//{
	//	foreach (GameObject crntNode in brickList)
	//	{
	//		scrBrick crntNDScrpt = crntNode.GetComponent<scrBrick>();
	//		crntNDScrpt.UpdateNodeDisplay();
	//	}
	//}


	//// *---------------------------------------- Ko Check Methods ---------------------------------
	//public List<int> Create_ShpValMap()  //? Called in GameManager               // Displays Array based on nodeValues
	//{
	//	List<int> ShpValMap = new List<int>();                                         // List to hold update values for arrayNodes

	//	foreach (GameObject crntND in brickList)
	//	{                                         // Loops over all nodes in brickList    
	//		scrBrick NDScr = crntND.GetComponent<scrBrick>();                    // Set liberty value based on masterNode
	//		ShpValMap.Add(NDScr.shpVal);
	//	}

	//	return ShpValMap;
	//}


	//public bool Check_Map_For_Ko(List<int> prevShpValMap, int ND_ID, int ShpVal)        // Map of board state before last move, ND_ID and Value
	//{
	//	bool isKo = false;                                                              // Ko is initially set false

	//	List<int> newShpValMap = prevShpValMap.ToList();                                // Create a new copy of ShpValMap for updating and comparing
	//	newShpValMap[ND_ID] = ShpVal;                                                   // Change List val to ShpVal at ND_ID index

	//	bool sequenceCheck = newShpValMap.SequenceEqual(prevShpValMap);
	//	Debug.Log("sequenceCheck: " + sequenceCheck);

	//	List<int> differingIndices = FindDifferences(newShpValMap, prevShpValMap);
	//	if (differingIndices.Count > 0)
	//	{
	//		Debug.Log("Sequences differ at indices: " + string.Join(", ", differingIndices));
	//	}
	//	else
	//	{
	//		Debug.Log("Sequences are identical.");
	//	}

	//	if (sequenceCheck)
	//	{                                              // Compare to prev ShpValMap for Board state
	//		isKo = true;                                                                // If they match, the move will violate KO rules
	//		Debug.Log("** KO is True **");
	//		LogListValues<int>(prevShpValMap, "prevShpValMap");
	//		LogListValues<int>(newShpValMap, "newShpValMap");
	//	}
	//	else
	//	{
	//		isKo = false;
	//	}

	//	return isKo;                                                                    // Return the Ko bool Value
	//}

	//// DEBUG METHOD FOR KO CHECK METHODS
	//List<int> FindDifferences(List<int> newShpValMap, List<int> prevShpValMap)
	//{
	//	List<int> differences = new List<int>();

	//	for (int i = 0; i < newShpValMap.Count; i++)
	//	{
	//		if (newShpValMap[i] != prevShpValMap[i])
	//		{
	//			differences.Add(i); // Record the index where they differ
	//		}
	//	}

	//	return differences;

	//}


	//void LogListValues<T>(List<T> list, string listName)
	//{
	//	string values = string.Join(", ", list);
	//	Debug.Log($"{listName} values: [{values}]");
	//}



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
