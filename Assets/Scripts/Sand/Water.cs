using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SandConstants;

public class Water : Sand
{

    public override float Spread()
    {
        return 2f;
    }
    public override IEnumerable<List<SandFallDirection>> FallDirectionPriority()
    {
        yield return new() { SandFallDirection.Down };
        yield return new() { SandFallDirection.DownLeft, SandFallDirection.DownRight };
        yield return new() { SandFallDirection.Left, SandFallDirection.Right };
    }
}
