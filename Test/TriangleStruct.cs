using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AssociatedTriangle
{
    private Vector3 _vertexPosition;
    private int[] _triangle;

    public AssociatedTriangle(Vector3 vertexPosition, int[] triangle)
    {
        _vertexPosition = vertexPosition;
        if (triangle.Length > 3)
        {
            throw new Exception("Sended array its not a triangle. Array must contain only three elements");
        }
        else
        {
            _triangle = triangle;
        }
    }

    public int[] GetTriangle()
    {
        return _triangle;
    }

    public Vector3 GetVertexPosition()
    {
        return _vertexPosition;
    }
}
