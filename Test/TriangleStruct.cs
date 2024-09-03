using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public bool IsAdjacentTriangle(int[] triangle)
    {
        bool logicOrA = false;
        bool logicOrB = false;
        bool logicOrC = false;

        for (int i = 0; i < 3; i++)
        {
            logicOrA = logicOrA || triangle[0] == _triangle[i];
            logicOrB = logicOrB || triangle[1] == _triangle[i];
            logicOrC = logicOrC || triangle[2] == _triangle[i];
        }

        if (logicOrA && logicOrB && !logicOrC || logicOrC && logicOrA && !logicOrB || logicOrB && logicOrC && !logicOrA)
        {
            return true;
        }

        return false;
    }
    
    public bool IsAdjacentTriangle(AssociatedTriangle triangle)
    {
        bool logicOrA = false;
        bool logicOrB = false;
        bool logicOrC = false;
        int[] tr = triangle.GetTriangle().ToArray();

        for (int i = 0; i < 3; i++)
        {
            logicOrA = logicOrA || tr[0] == _triangle[i];
            logicOrB = logicOrB || tr[1] == _triangle[i];
            logicOrC = logicOrC || tr[2] == _triangle[i];
        }

        if (logicOrA && logicOrB && !logicOrC || logicOrC && logicOrA && !logicOrB || logicOrB && logicOrC && !logicOrA)
        {
            return true;
        }

        return false;
    }
}
