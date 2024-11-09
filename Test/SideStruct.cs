using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;

public struct SideStruct
{
    private int[] _triangles;
    private Vector3[] _vertices;
    private Vector3[] _normal;
    private Vector4[] _tangent;

    public SideStruct(int[] triangles, Vector3[] vertices, Vector3 normal, Vector4 tangent)
    {
        _triangles = (int[])triangles.Clone();
        _vertices = (Vector3[])vertices.Clone();
        _normal = Enumerable.Repeat(normal, vertices.Length).ToArray();
        _tangent = Enumerable.Repeat(tangent, vertices.Length).ToArray();
    }
    
    public SideStruct(int[] triangles, Vector3[] vertices, Vector3[] normals, Vector4[] tangents)
    {
        _triangles = (int[])triangles.Clone();
        _vertices = (Vector3[])vertices.Clone();
        _normal = (Vector3[])normals.Clone();
        _tangent = (Vector4[])tangents.Clone();
    }

    public int[] GetTriangles()
    {
        return _triangles;
    }

    public Vector3[] GetVertices()
    {
        return _vertices;
    }

    public Vector3[] GetNormals()
    {
        return _normal;
    }
    
    public Vector4[] GetTangents()
    {
        return _tangent;
    }
}
