using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    public GameObject cellPrefab;
    [Header("Dungeon Grid")]
    public float cellW = 50f;
    public float cellH = 44f;

    public int numColumns = 10;
    public int numRows = 20;

    [Header("Dungeon Size and Spawn")]
    public int maxRooms = 25;
    public int minRooms = 15;
    public int starterRoomIndex = 5;

    [Header("Dungeon Branching")]
    [Range(1, 4)]
    public int starterRoomBranches = 1;
    [Range(0f, 1f)]
    public float branching = 0.5f;

    [Header("Dungeon Looping")]
    public int minLoops = 0;
    public int maxLoops = 3;

    [Header("Room Types")]
    public int roomTypes = 4;

    [Header("Dungeon Visibility")]
    [Tooltip("Press V")]
    public bool dungeonVisible = true;
    [Tooltip("Editor Only - Press G")]
    public bool gridVisible = true;

    [System.Serializable]
    public struct RoomInfo
    {
        public int index;
        public int x;
        public int y;
        public int type; // -1 = starter
        public bool neighborUp;
        public bool neighborDown;
        public bool neighborLeft;
        public bool neighborRight;
    }

    [Header("Generated Dungeon Data")]
    public List<RoomInfo> generatedRooms = new List<RoomInfo>();

    void OnValidate()
    {
        branching = Mathf.Round(branching * 10f) / 10f;
    }

    int[] floorplan;
    int[] distFromStart;
    int[] roomTypeMap;
    int floorplanCount = 0;
    int loopCount = 0;

    Queue<int> cellQueue = new Queue<int>();
    List<int> endrooms = new List<int>();
    List<GameObject> spawned = new List<GameObject>();

    void Start()
    {
        StartGeneration();
    }

    void StartGeneration()
    {
        generatedRooms.Clear();
        floorplan = new int[numColumns * numRows];
        distFromStart = new int[numColumns * numRows];
        roomTypeMap = new int[numColumns * numRows];

        for (int i = 0; i < roomTypeMap.Length; i++)
        {
            roomTypeMap[i] = -2;
        }

        foreach (var obj in spawned)
        {
            Destroy(obj);
        }
        spawned.Clear();

        floorplanCount = 0;
        cellQueue.Clear();
        endrooms.Clear();
        loopCount = 0;

        Visit(starterRoomIndex);
        StartCoroutine(GenerationRoutine());
    }

    IEnumerator GenerationRoutine()
    {
        while (cellQueue.Count > 0 && floorplanCount < maxRooms)
        {
            int i = cellQueue.Dequeue();
            int x = i % numColumns;
            int y = i / numColumns;

            if (i == starterRoomIndex)
            {
                List<int> options = new List<int>();

                if (x > 0) options.Add(i - 1);
                if (x < numColumns - 1) options.Add(i + 1);
                if (y > 0) options.Add(i - numColumns);
                if (y < numRows - 1) options.Add(i + numColumns);

                Shuffle(options);
                int branchesCreated = 0;

                foreach (int next in options)
                {
                    if (Visit(next))
                    {
                        branchesCreated++;
                        if (branchesCreated >= starterRoomBranches)
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                bool created = false;

                List<int> dirs = new List<int>();

                if (x > 0) dirs.Add(i - 1);
                if (x < numColumns - 1) dirs.Add(i + 1);
                if (i >= numColumns) dirs.Add(i - numColumns);
                if (i < numColumns * numRows - numColumns) dirs.Add(i + numColumns);

                for (int k = 0; k < dirs.Count; k++)
                {
                    int r = Random.Range(k, dirs.Count);
                    (dirs[k], dirs[r]) = (dirs[r], dirs[k]);
                }

                if (branching == 0)
                {
                    foreach (int next in dirs)
                    {
                        if (Visit(next))
                        {
                            created = true;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (int next in dirs)
                    {
                        if (branching < 1f && Random.value > branching)
                        {
                            continue;
                        }

                        created |= Visit(next);

                        if (branching == 0 && created)
                        {
                            break;
                        }
                    }
                }

                if (!created)
                {
                    endrooms.Add(i);
                }
            }

            yield return null;
        }

        ComputeAllRoomNeighbors();

        if (floorplanCount < minRooms || loopCount < minLoops)
        {
            StartGeneration();
            yield break;
        }

        Debug.Log("Rooms generated: " + floorplanCount);
    }

    int NCount(int i)
    {
        int count = 0;
        int x = i % numColumns;
        int y = i / numColumns;

        if (x > 0) count += floorplan[i - 1];
        if (x < numColumns - 1) count += floorplan[i + 1];
        if (y > 0) count += floorplan[i - numColumns];
        if (y < numRows - 1) count += floorplan[i + numColumns];

        return count;
    }

    bool Visit(int i)
    {
        if (floorplan[i] == 1) return false;

        int neighbors = NCount(i);

        if (i == starterRoomIndex)
        {
            if (neighbors >= 1) return false;
            roomTypeMap[i] = -1;
        }
        else
        {
            int maxNeighbors = 2;

            if (neighbors >= maxNeighbors)
            {
                if (loopCount < maxLoops)
                {
                    loopCount++;
                }
                else
                {
                    return false;
                }
            }
            roomTypeMap[i] = Random.Range(0, roomTypes);
        }

        floorplan[i] = 1;
        floorplanCount++;
        cellQueue.Enqueue(i);
        ComputeDistance(i);
        SpawnAtIndex(cellPrefab, i);

        generatedRooms.Add(new RoomInfo
        {
            index = i,
            x = i % numColumns,
            y = i / numColumns,
            type = roomTypeMap[i]
        });

        return true;
    }

    void ComputeDistance(int i)
    {
        if (i == starterRoomIndex)
        {
            distFromStart[i] = 0;
            return;
        }

        int x = i % numColumns;
        int y = i / numColumns;

        int best = int.MaxValue;

        if (x > 0 && distFromStart[i - 1] >= 0) best = Mathf.Min(best, distFromStart[i - 1]);
        if (x < numColumns - 1 && distFromStart[i + 1] >= 0) best = Mathf.Min(best, distFromStart[i + 1]);
        if (y > 0 && distFromStart[i - numColumns] >= 0) best = Mathf.Min(best, distFromStart[i - numColumns]);
        if (y < numRows - 1 && distFromStart[i + numColumns] >= 0) best = Mathf.Min(best, distFromStart[i + numColumns]);

        distFromStart[i] = best + 1;
    }

    void SpawnAtIndex(GameObject prefab, int index)
    {
        int x = index % numColumns;
        int y = index / numColumns;

        float worldX = (x - numColumns / 2f) * cellW;
        float worldY = (y - numRows / 2f) * cellH;

        var obj = Instantiate(prefab, new Vector3(worldX, worldY, 0), Quaternion.identity);
        obj.SetActive(dungeonVisible);
        spawned.Add(obj);
    }

    public void SetDungeonVisibility(bool visible)
    {
        dungeonVisible = visible;
        foreach (var obj in spawned)
        {
            if (obj != null)
                obj.SetActive(visible);
        }
    }

    void Shuffle<T>(List<T> list)
    {
        for (int k = list.Count - 1; k > 0; k--)
        {
            int r = Random.Range(0, k + 1);
            (list[k], list[r]) = (list[r], list[k]);
        }
    }

    void ComputeAllRoomNeighbors()
    {
        for (int k = 0; k < generatedRooms.Count; k++)
        {
            RoomInfo room = generatedRooms[k];
            int i = room.index;
            int x = i % numColumns;
            int y = i / numColumns;

            room.neighborLeft  = (x > 0 && floorplan[i - 1] == 1);
            room.neighborRight = (x < numColumns - 1 && floorplan[i + 1] == 1);
            room.neighborDown  = (y > 0 && floorplan[i - numColumns] == 1);
            room.neighborUp    = (y < numRows - 1 && floorplan[i + numColumns] == 1);

            generatedRooms[k] = room;
        }
    }

    void OnDrawGizmos()
    {
        if (!gridVisible) return;
        if (numColumns <= 0 || numRows <= 0) return;

        Gizmos.color = Color.gray;

        for (int x = 0; x <= numColumns; x++)
        {
            float worldX = (x - numColumns / 2f) * cellW;
            float y1 = -(numRows / 2f) * cellH;
            float y2 = (numRows / 2f) * cellH;
            Gizmos.DrawLine(new Vector3(worldX, y1, 0), new Vector3(worldX, y2, 0));
        }

        for (int y = 0; y <= numRows; y++)
        {
            float worldY = (y - numRows / 2f) * cellH;
            float x1 = -(numColumns / 2f) * cellW;
            float x2 = (numColumns / 2f) * cellW;
            Gizmos.DrawLine(new Vector3(x1, worldY, 0), new Vector3(x2, worldY, 0));
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
            SetDungeonVisibility(!dungeonVisible);

        if (Input.GetKeyDown(KeyCode.G))
            gridVisible = !gridVisible;
    }
}