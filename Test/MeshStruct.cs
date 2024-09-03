using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct IntermediateMesh
{
    private List<int> oldVertexIndices;
    private List<Vector3> vertexPositions;

    public IntermediateMesh(int i)
    {
        oldVertexIndices = new List<int>();
        vertexPositions = new List<Vector3>();
    }
}
