using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Side
{
    private int[] triangles;
    private Vector3[] vertices;
    private Vector3[] normals;
    private Vector4[] tangents;

    public Side()
    {
        
    }
    
    public Side(int[] _triangles, Vector3[] _vertices, Vector3[] _normals, Vector4[] _tangents)
    {
        triangles = _triangles;
        vertices = _vertices;
        normals = _normals;
        tangents = _tangents;
    }

    public int[] GetTriangles()
    {
        return triangles;
    }

    public Vector3[] GetVertices()
    {
        return vertices;
    }

    public Vector3[] GetNormals()
    {
        return normals;
    }
    
    public Vector4[] GetTangents()
    {
        return tangents;
    }
}
