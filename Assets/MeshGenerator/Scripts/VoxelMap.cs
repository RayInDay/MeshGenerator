using UnityEngine;

public class VoxelMap : MonoBehaviour {

	

	public float size = 2f;

	public int voxelResolution = 8;
	public int chunkResolution = 2;

	public float maxFeatureAngle = 135f;

	public VoxelGrid voxelGridPrefab;

	public Transform stencilVisualizations;
	public Transform[] TransformsofMeshDump;
	[SerializeField] private GameObject[] PrefabDumps;
	private GameObject[] ChunksSufaceDump ;
	private GameObject[] ChunkWallDump;
	public bool snapToGrid;

	private VoxelGrid[] chunks;
	
	private float chunkSize, voxelSize, halfSize;

	

	
	
	private void Awake () {
		halfSize = size * 0.5f;
		chunkSize = size / chunkResolution;
		voxelSize = chunkSize / voxelResolution;
		ChunksSufaceDump = new GameObject[chunkResolution * chunkResolution*2];
		ChunkWallDump= new GameObject[chunkResolution * chunkResolution * 2];
		chunks = new VoxelGrid[chunkResolution * chunkResolution];
		for (int i = 0, y = 0; y < chunkResolution; y++) {
			for (int x = 0; x < chunkResolution; x++, i++) {
				CreateChunk(i, x, y);
				CreateChunkCopy(i, x, y,PrefabDumps[0],TransformsofMeshDump[0],ChunksSufaceDump,0);
				CreateChunkCopy(i, x, y, PrefabDumps[0], TransformsofMeshDump[1], ChunksSufaceDump,4);
				CreateChunkCopy(i, x, y, PrefabDumps[1], TransformsofMeshDump[0], ChunkWallDump, 0);
				CreateChunkCopy(i, x, y, PrefabDumps[1], TransformsofMeshDump[1], ChunkWallDump, 4);
			}
		}
		BoxCollider box = gameObject.AddComponent<BoxCollider>();
		box.size = new Vector3(size, size);

	}
	private void CreateChunkCopy(int i, int x, int y,GameObject Pref,Transform TrasformMeshDump,GameObject[] List, int numberofDump)
	{
		List[i+numberofDump] = Instantiate(Pref, TrasformMeshDump.position,Quaternion.identity) ;
		List[i + numberofDump].transform.parent = TrasformMeshDump;
		List[i + numberofDump].transform.localRotation = Quaternion.Euler(0,0,0);
		List[i + numberofDump].transform.localScale = new Vector3(1,1,1);
		List[i + numberofDump].transform.localPosition =
			new Vector3(x * chunkSize - halfSize, y * chunkSize - halfSize);
		
		
		 
	}
	private void CopyChunkToDump()
    {
        for (int i = 0; i < chunks.Length; i++)
        {

			ChunksSufaceDump[i].GetComponent<MeshEditor>().SetMesh(chunks[i].GetSurfaceMesh());
			ChunksSufaceDump[i+4].GetComponent<MeshEditor>().SetMesh(chunks[i].GetSurfaceMesh()) ;
			ChunkWallDump[i].GetComponent<MeshEditor>().SetMesh(chunks[i].GetWallMesh());
			ChunkWallDump[i + 4].GetComponent<MeshEditor>().SetMesh(chunks[i].GetWallMesh());

		}
    }
	private void CreateChunk (int i, int x, int y) {
		VoxelGrid chunk = Instantiate(voxelGridPrefab) as VoxelGrid;
		chunk.Initialize(voxelResolution, chunkSize, maxFeatureAngle);
		chunk.transform.parent = transform;
		chunk.transform.localPosition =
			new Vector3(x * chunkSize - halfSize, y * chunkSize - halfSize);
		chunks[i] = chunk;
		if (x > 0) {
			chunks[i - 1].xNeighbor = chunk;
		}
		if (y > 0) {
			chunks[i - chunkResolution].yNeighbor = chunk;
			if (x > 0) {
				chunks[i - chunkResolution - 1].xyNeighbor = chunk;
			}
		}
	}

	private void Update()
	{
		Transform visualization = stencilVisualizations;
		RaycastHit hitInfo;
		Touch touch= Input.touches[0];
		if (Physics.Raycast(
			Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo) &&
			hitInfo.collider.gameObject == gameObject)
		{
			
			Vector2 center = transform.InverseTransformPoint(hitInfo.point);
			center.x += halfSize;
			center.y += halfSize;
			if (snapToGrid)
			{
				center.x = ((int)(center.x / voxelSize) + 0.5f) * voxelSize;
				center.y = ((int)(center.y / voxelSize) + 0.5f) * voxelSize;
			}
          
			
			center.x -= halfSize;
			center.y -= halfSize;
			visualization.localPosition = center;
			visualization.localScale = (Vector3.one * 0.5f * voxelSize * 2f);
			HandleTouch(touch.fingerId, Camera.main.ScreenToWorldPoint(touch.position), touch.phase,center);

		}
        if (touch.phase == TouchPhase.Ended)
        {
			CopyChunkToDump();

		}
	}
	private void HandleTouch(int touchFingerId, Vector3 touchPosition, TouchPhase touchPhase,Vector2 Position)
	{
		switch (touchPhase)
		{
			case TouchPhase.Began:
				ClearData();
				EditVoxels(Position);
				break;
			case TouchPhase.Moved:
				EditVoxels(Position);
				break;
			case TouchPhase.Ended:
				CopyChunkToDump();
				break;
		} }
		private void ClearData()
    {
        foreach (var item in chunks)
        {
			item.ClearDrawingMesh();
        }
				
		
	}
	private void EditVoxels (Vector2 center) {
		VoxelStencil activeStencil = new VoxelStencilCircle();
		activeStencil.Initialize(
			true, 0.5f * voxelSize);
		activeStencil.SetCenter(center.x, center.y);

		int xStart = (int)((activeStencil.XStart - voxelSize) / chunkSize);
		if (xStart < 0) {
			xStart = 0;
		}
		int xEnd = (int)((activeStencil.XEnd + voxelSize) / chunkSize);
		if (xEnd >= chunkResolution) {
			xEnd = chunkResolution - 1;
		}
		int yStart = (int)((activeStencil.YStart - voxelSize) / chunkSize);
		if (yStart < 0) {
			yStart = 0;
		}
		int yEnd = (int)((activeStencil.YEnd + voxelSize) / chunkSize);
		if (yEnd >= chunkResolution) {
			yEnd = chunkResolution - 1;
		}

		for (int y = yEnd; y >= yStart; y--) {
			int i = y * chunkResolution + xEnd;
			for (int x = xEnd; x >= xStart; x--, i--) {
				activeStencil.SetCenter(
					center.x - x * chunkSize, center.y - y * chunkSize);
				chunks[i].Apply(activeStencil);
			}
		}
	}

	
}