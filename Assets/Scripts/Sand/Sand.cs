using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SandConstants;

public class Sand : MonoBehaviour
{
    public SandFallDirection fall_direction;
    public bool CanFall => fall_direction != SandFallDirection.None;
    public Vector2Int pos;
    public Vector2Int? cached_target_pos;
    public int GridX => pos.x;
    public int GridY => pos.y;

    public virtual float Mass()
    {
        return 1f;
    }

    public virtual float Spread()
    {
        return 1f;
    }

    public void Initialize(int start_x, int start_y)
    {
        this.pos = new Vector2Int(start_x, start_y);
        this.transform.localScale = SandGrid.Field.cellSize;
        UpdateWorldPosition();
    }
    public void UpdateGridPosition()
    {
        pos.x += fall_direction.GetFallDirectionX();
        pos.y += fall_direction.GetFallDirectionY();
    }

    public void UpdateWorldPosition()
    {
        Vector3 worldCoords = SandGrid.Field.CellToWorld((Vector3Int) pos);
        this.transform.position = worldCoords;
    }

    /// <summary>
    /// Return the priority list for which direction to fall in.
    /// 
    /// The higher the index, the higher the priority. If an index contains multiple directions, one is randomly chosen using a uniform distribution.
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerable<List<SandFallDirection>> FallDirectionPriority()
    {
        yield return new() { SandFallDirection.Down };
        yield return new() { SandFallDirection.DownLeft, SandFallDirection.DownRight };
    }


}
