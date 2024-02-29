using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SandConstants;
using TMPro;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class MouseControl : MonoBehaviour
{
    [SerializeField]
    SandEventManager m_SandEventManager;

    [SerializeField]
    TextMeshProUGUI m_TextMesh;

    [Header("Place settings")]
    [SerializeField]
    SandType m_Placing = SandType.Sand;
    [SerializeField]
    int m_PlaceRadius = 1;

    bool m_Paused = false;

    private void Start()
    {
        m_SandEventManager.PauseEvent.AddListener(() => m_Paused = true);
        m_SandEventManager.UnpauseEvent.AddListener(() => m_Paused = false);
    }

    // Update is called once per frame
    void Update()
    {
        CheckUserInput();
        UpdateText();
    }

    private void UpdateText()
    {
        m_TextMesh.text = $"Place Radius: {m_PlaceRadius}\n";
        m_TextMesh.text += $"Placing: {m_Placing}";
    }
    private void CheckUserInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            m_SandEventManager.ResetEvent.Invoke();
        }


        // if left mouse click
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3Int gridPos = SandGrid.Field.WorldToCell(Camera.main.ScreenToWorldPoint(mousePos));
            int r = m_PlaceRadius;
            for (int i = 0; i <= r; i++)
            {
                foreach(Vector2Int p in GetPointsInRadius((Vector2Int) gridPos, i))
                {
                    m_SandEventManager.SandSpawnEvent.Invoke((Vector3Int) p, m_Placing);
                }
            }
        }


        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            m_PlaceRadius = Mathf.Min(m_PlaceRadius + 1, 7);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            m_PlaceRadius = Mathf.Max(m_PlaceRadius - 1, 0);
        }


        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            m_Placing = SandType.Sand;
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            m_Placing = SandType.Water;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            m_Placing = SandType.Gas;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            m_Placing = SandType.Solid;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (m_Paused)
            {
                m_SandEventManager.UnpauseEvent.Invoke();
            }
            else
            {
                m_SandEventManager.PauseEvent.Invoke();
            }
        }
    }

    /// <summary>
    /// https://en.wikipedia.org/wiki/Midpoint_circle_algorithm
    /// </summary>
    /// <param name="center"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public static IEnumerable<Vector2Int> GetPointsInRadius(Vector2Int center, int radius)
    {
        int x = 0;
        int y = 0;
        int r_sq = radius * radius;

        while (y <= radius)
        {
            int y_sq = y * y;
            while (x*x + y_sq < r_sq)
            {
                foreach (Vector2Int p in MirrorInQuadrants(center, x, y))
                    yield return p;

                x++;
            }
            x = 0;
            y++;
        }
    }

    public static IEnumerable<Vector2Int> MirrorInQuadrants(Vector2Int center, int x, int y)
    {
        if (x == 0 && y == 0)
        {
            yield return center;
        }
        if (x == 0)
        {
            yield return center + new Vector2Int(0, y);
            yield return center + new Vector2Int(0, -y);
        }
        else if (y == 0)
        {
            yield return center + new Vector2Int(x, 0);
            yield return center + new Vector2Int(-x, 0);
        }
        else
        {
            yield return center + new Vector2Int(x, y);
            yield return center + new Vector2Int(x, -y);
            yield return center + new Vector2Int(-x, y);
            yield return center + new Vector2Int(-x, -y);
        }
    }
    public static IEnumerable<Vector2Int> MirrorInOctants(Vector2Int center, int x, int y)
    {
        // Mirror in 4 quadrants
        foreach (Vector2Int quad in MirrorInQuadrants(center, x, y))
            yield return quad;

        // if x == y, then mirrored_quad == quad
        if (x == y)
            yield break;

        // Mirror in 4 quadrants with x and y flipped, resulting in mirrored octants
        foreach (Vector2Int mirrored_quad in MirrorInQuadrants(center, y, x))
        {
            yield return mirrored_quad;
        }
    }
}
