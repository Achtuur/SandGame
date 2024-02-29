using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.XR;
using SandConstants;
using ExtensionMethods;
using UnityEngine.Events;

public class FallingSand : MonoBehaviour
{
    // with grid scale = 1, there are this many cells in the y direction
    const int GRID_Y_SIZE = 10;
    // with grid scale = 1, there are this many cells in the x direction
    const int GRID_X_SIZE = 22;

    Unity.Mathematics.Random m_Random = new Unity.Mathematics.Random((uint)DateTime.Now.Ticks);

    int MapWidth = 450;
    int MapHeight = 200;
    int NPoints;

    int FloorHeight;
    int CeilingHeight;
    int WallXLimit;

    [SerializeField] GameObject m_SandPrefab;
    [SerializeField] GameObject m_WaterPrefab;
    [SerializeField] GameObject m_GasPrefab;
    [SerializeField] GameObject m_SolidPrefab;

    [SerializeField]
    TextMeshProUGUI m_TextMesh;

    [SerializeField]
    SandEventManager m_SandEventManager;


    /// <summary>
    /// Grid array that represents the entire grid. Non-zero values contain sand and the value is equal to the index in the Sands list.
    /// </summary>
    List<int> GridArray;
    List<int> NewGrid;
    List<Sand> Sands;


    [Header("Options")]
    [SerializeField] bool m_PauseSimulation = false;
    [SerializeField] bool m_EnableRNG = true;
    [SerializeField, Range(-10f, 10f)] float m_Gravity = 1f;

    [Header("Visualisations")]
    [SerializeField] bool m_VisualiseGrid = false;
    [SerializeField] bool m_VisualiseDuplicates = false;
    [SerializeField] bool m_VisualiseBorders = false;


    // Start is called before the first frame update
    void Start()
    {
        MapWidth = (int)(GRID_X_SIZE / SandGrid.Field.cellSize.x) + 1;
        MapHeight = (int) (GRID_Y_SIZE / SandGrid.Field.cellSize.y) + 1;
        NPoints = MapWidth * MapHeight;
        FloorHeight = 0;
        CeilingHeight = MapHeight - 1;
        WallXLimit = MapWidth / 2;

        ResetSands();
        m_SandEventManager.SandSpawnEvent.AddListener(OnSandSpawn);
        m_SandEventManager.SandDestroyEvent.AddListener(OnSandDestroy);
        m_SandEventManager.ResetEvent.AddListener(ResetSands);
        m_SandEventManager.PauseEvent.AddListener(OnPause);
        m_SandEventManager.UnpauseEvent.AddListener(OnUnpause);
    }

    private void OnUnpause()
    {
        m_PauseSimulation = false;
    }

    private void OnPause()
    {
        m_PauseSimulation = true;
    }

    private void ResetSands()
    {
        m_Random = new Unity.Mathematics.Random((uint)DateTime.Now.Ticks);
        Sands ??= new List<Sand>();

        foreach (Sand sand in Sands)
        {
            Destroy(sand.gameObject);
        }
        Sands.Clear();

        GridArray = new List<int>(NPoints);
        NewGrid = new List<int>(NPoints);
        for (int i = 0; i < NPoints; i++)
        {
            GridArray.Add(0);
        }
    }

    private void OnSandSpawn(Vector3Int spawn_pos, SandType sand_type)
    {
        CreateSand(spawn_pos.x, spawn_pos.y, sand_type);
    }

    private void OnSandDestroy(Vector3Int sand_pos)
    {
        int sand_idx = PosToIndex(sand_pos.x, sand_pos.y);
        if (GridArray[sand_idx] == 0)
            return;

        int sand_id = GridArray[sand_idx];
        Destroy(Sands[sand_id].gameObject);
        Sands.RemoveAt(sand_id);
        GridArray[sand_idx] = 0;
    }

    void CreateSand(int x, int y, SandType sand_type)
    {
        if (!CanMoveToCell(x, y))
            return;

        GameObject sandObj = null;
        if (sand_type == SandType.Sand)
        {
            sandObj = Instantiate(m_SandPrefab, SandGrid.Field.transform);
        }
        else if (sand_type == SandType.Water)
        {
            sandObj = Instantiate(m_WaterPrefab, SandGrid.Field.transform);
        }
        else if (sand_type == SandType.Gas)
        {
            sandObj = Instantiate(m_GasPrefab, SandGrid.Field.transform);
        }
        else if (sand_type == SandType.Solid)
        {
            sandObj = Instantiate(m_SolidPrefab, SandGrid.Field.transform);
        }

        Sand sand = sandObj.GetComponent<Sand>();
        sand.Initialize(x, y);
        Sands.Add(sand);
        GridArray[PosToIndex(sand)] = Sands.Count - 1;
    }

    int PosToIndex(int x, int y)
    {
        // negative x bound should be treated as index 0
        int adjusted_x = x + MapWidth / 2;
        return adjusted_x + y * MapWidth;
    }

    int IndexToPosX(int i)
    {
        int x = i % MapWidth;
        return x - MapWidth / 2;
    }

    int IndexToPosY(int i)
    {
        return i / MapWidth;
    }

    Vector3Int IndexToPos(int i)
    {
        return new Vector3Int(IndexToPosX(i), IndexToPosY(i), 0);
    }

    int PosToIndex(Sand sand)
    {
        return PosToIndex(sand.GridX, sand.GridY);
    }

    void DrawBoxAroundGrid(int grid_x, int grid_y, Color c)
    {
        Vector3Int gridPos = new(grid_x, grid_y, 0);
        Vector3 worldPos = SandGrid.Field.CellToWorld(gridPos);

        Vector3 to_edge = 0.5f * SandGrid.Field.cellSize;
        Vector3 topLeft = worldPos + new Vector3(-to_edge.x, to_edge.y, 0);
        Vector3 topRight = worldPos + new Vector3(to_edge.x, to_edge.y, 0);
        Vector3 bottomLeft = worldPos + new Vector3(-to_edge.x, -to_edge.y, 0);
        Vector3 bottomRight = worldPos + new Vector3(to_edge.x, -to_edge.y, 0);

        Debug.DrawLine(topLeft, topRight, c, 0.01f);
        Debug.DrawLine(topRight, bottomRight, c, 0.01f);
        Debug.DrawLine(bottomRight, bottomLeft, c, 0.01f);
        Debug.DrawLine(bottomLeft, topLeft, c, 0.01f);
    }


    void VisualiseGrid()
    {
#if UNITY_EDITOR
        if (!m_VisualiseGrid)
            return;

        for (int i = 0; i < NPoints; i++)
        {
            if (GridArray[i] == 0)
                continue;
            Vector3Int gridPos = IndexToPos(i);
            float hue = (float)i / NPoints;
            float value = (float)GridArray[i];
            Color c = Color.HSVToRGB(hue, 1f, value);
            DrawBoxAroundGrid(gridPos.x, gridPos.y, c);
        }
#endif
    }

    void VisualiseBorders()
    {
#if UNITY_EDITOR
        if (!m_VisualiseBorders)
            return;

        Vector3Int leftPos = IndexToPos(0);
        Vector3Int rightPos = IndexToPos(NPoints - 1);

        Vector3 worldBottomLeft = SandGrid.Field.CellToWorld(leftPos);
        Vector3 worldTopRight = SandGrid.Field.CellToWorld(rightPos);

        float left_x = worldBottomLeft.x;
        float right_x = worldTopRight.x;
        float bottom_y = worldBottomLeft.y;
        float top_y = worldTopRight.y;
        Color c = Color.cyan;

        Debug.DrawLine(new Vector3(left_x, bottom_y, 0), new Vector3(right_x, bottom_y, 0), c, 0.01f);
        Debug.DrawLine(new Vector3(right_x, bottom_y, 0), new Vector3(right_x, top_y, 0), c, 0.01f);
        Debug.DrawLine(new Vector3(right_x, top_y, 0), new Vector3(left_x, top_y, 0), c, 0.01f);
        Debug.DrawLine(new Vector3(left_x, top_y, 0), new Vector3(left_x, bottom_y, 0), c, 0.01f);
#endif
    }

    void VisualiseDuplicateSand()
    {
#if UNITY_EDITOR
        if (!m_VisualiseDuplicates)
            return;

        Sands.ForEach(sand =>
        {
            if (Sands.Any(sand2 => sand != sand2 && sand.GridX == sand2.GridX && sand.GridY == sand2.GridY))
            {
                DrawBoxAroundGrid(sand.GridX, sand.GridY, Color.white);
            }
        });
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_PauseSimulation)
            UpdateSands();

        UpdateText();
    }    

    private void UpdateText()
    {
        m_TextMesh.text = $"Sands: {Sands.Count}\n";
    }

    private void UpdateSands()
    {
        NewGrid = new List<int>(NPoints);
        for (int i = 0; i < NPoints; i++)
        {
            NewGrid.Add(0);
        }

        Profiler.BeginSample("Loop over sands");
        foreach ((Sand sand, int index) in Sands.Enumerate())
        {
            UpdateSandPosition(sand);
            NewGrid[PosToIndex(sand)] = index;
        }
        Profiler.EndSample();


        GridArray = new List<int>(NewGrid);

        VisualiseGrid();
        VisualiseDuplicateSand();
        VisualiseBorders();
    }

    private void UpdateSandPosition(Sand sand)
    {
        Profiler.BeginSample("Get next sand pos");
        sand.pos = GetSandNextPos(sand);
        Profiler.EndSample();

        Profiler.BeginSample("Update world position");
        sand.pos.x = Mathf.Clamp(sand.pos.x, -WallXLimit, WallXLimit);
        sand.pos.y = Mathf.Clamp(sand.pos.y, FloorHeight + 1, CeilingHeight - 1);
        sand.UpdateWorldPosition();
        Profiler.EndSample();
    }

    /// <summary>
    /// Sets sand fall direction to left or right is that is possible
    /// </summary>
    /// <param name="sand"></param>
    /// <returns></returns>
    Vector2Int GetSandNextPos(Sand sand)
    {
        foreach (List<SandFallDirection> fall_dir_list in sand.FallDirectionPriority())
        {
            if (fall_dir_list is null)
                continue;

            // chance for one of the directions to be picked
            float base_odds = 1f / fall_dir_list.Count;
            float current_odds = base_odds;
            foreach(SandFallDirection fall_dir in fall_dir_list)
            {
                // bad rng :(
                Profiler.BeginSample("calc rng");
                if (m_EnableRNG && m_Random.NextFloat() > current_odds)
                {
                    Profiler.EndSample();
                    // increment current_odds so that by the end of fall_dir_list, the odds are 1
                    current_odds += base_odds;
                    continue;
                }
                Profiler.EndSample();

                int dx = (int) (fall_dir.GetFallDirectionX() * sand.Spread());
                int dy = (int) (fall_dir.GetFallDirectionY() * sand.Mass() * m_Gravity);
                Vector2Int target_pos = sand.pos + new Vector2Int(dx, dy);

                Profiler.BeginSample("Calculating next pos");
                Vector2Int reached_pos = TryMoveToPos(sand.pos, target_pos);
                Profiler.EndSample();

                // no movement achieved, try another direction
                if (sand.pos == reached_pos)
                    continue;

                return reached_pos;       
            }
        }
        return sand.pos;
    }


    /// <summary>
    /// Attempts to move from start_pos to end_pos. Returns position that was reached
    /// </summary>
    /// <param name="start_pos"></param>
    /// <param name="end_pos"></param>
    /// <returns></returns>
    Vector2Int TryMoveToPos(Vector2Int start_pos, Vector2Int end_pos)
    {
        int rx = start_pos.x;
        int ry = start_pos.y;
        foreach ((int px, int py) in GetPathToPos(start_pos, end_pos))
        {
            if (!CanMoveToCell(px, py))
                break;

            rx = px;
            ry = py;
        }
        return new Vector2Int(rx, ry);
    }

    bool CanMoveToCell(int x, int y)
    {
        if (y <= FloorHeight + 1 || y >= CeilingHeight || Mathf.Abs(x) >= WallXLimit - 1)
            return false;

        return !PosContainsSand(x, y);
    }

    bool PosContainsSand(int x, int y)
    {
        int index = PosToIndex(x, y);
        if (index < 0 || index >= NPoints)
            return true;

        return GridArray[index] != 0 || NewGrid[index] != 0;
    }

    IEnumerable<(int, int)> GetPathToPos(Vector2Int start_pos, Vector2Int end_pos)
    {
        int dx = Mathf.Abs(end_pos.x - start_pos.x);
        int dy = Mathf.Abs(end_pos.y - start_pos.y);
        int iter_count = Mathf.Max(dx, dy);

        for (int i = 1; i <= iter_count; i++)
        {
            float p_x = Mathf.Lerp(start_pos.x, end_pos.x, (float) i / iter_count);
            float p_y = Mathf.Lerp(start_pos.y, end_pos.y, (float) i / iter_count);

             //randomly round up
            if (m_Random.NextBool())
                p_x = Mathf.Ceil(p_x);
            else
                p_x = Mathf.Floor(p_x);

            if (m_Random.NextBool())
                p_y = Mathf.Ceil(p_y);
            else
                p_y = Mathf.Floor(p_y);


            yield return ((int)p_x, (int)p_y);
        }
    }
}
