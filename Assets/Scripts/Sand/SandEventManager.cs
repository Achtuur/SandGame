using SandConstants;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "SandEventManager", menuName = "ScriptableObjects/SandEventManager")]
public class SandEventManager : ScriptableObject
{
    public UnityEvent<Vector3Int, SandType> SandSpawnEvent;
    public UnityEvent<Vector3Int> SandDestroyEvent;
    public UnityEvent ResetEvent;
    public UnityEvent PauseEvent;
    public UnityEvent UnpauseEvent;
}
