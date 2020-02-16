using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PaintCheckData : ScriptableObject
{
    public Vector2 gridSize;
    public List<Vector2> checkPoints;
}
