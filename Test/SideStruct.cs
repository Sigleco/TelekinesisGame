using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public struct SideStruct
{
    private int[] triangles;
    private Vector3[] vertices;
    private Vector3 normal;
    private Vector3 tangent;

    public SideStruct(int[] _triangles, Vector3[] _vertices, Vector3 _normal, Vector3 _tangent)
    {
        triangles = _triangles;
        vertices = _vertices;
        normal = _normal;
        tangent = _tangent;
    }

    public Vector3[] GetVertices()
    {
        return vertices;
    }
}
