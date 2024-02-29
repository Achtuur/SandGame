using SandConstants;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gas : Sand
{
    public override IEnumerable<List<SandFallDirection>> FallDirectionPriority()
    {
        yield return new() { SandFallDirection.Up };
        yield return new() { SandFallDirection.UpLeft, SandFallDirection.UpRight };
        yield return new() { SandFallDirection.Left, SandFallDirection.Right };
    }
}
