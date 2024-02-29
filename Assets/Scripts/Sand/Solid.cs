using SandConstants;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Solid : Sand
{
    public override IEnumerable<List<SandFallDirection>> FallDirectionPriority()
    {
        yield return null;
    }
}
