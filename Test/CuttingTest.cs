using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnumeratorTest : MonoBehaviour
{
    private Vector3 _contactPoint, _dirU, _dirV;
    private MeshFilter _filter;
    private Mesh _mesh;
    private Vector3[] _vertices;
    private List<Vector3> _potentialNewVertices = new List<Vector3>();
    private List<Vector3> _approvedNewVertices = new List<Vector3>();
    private List<int> _intersectedTriangles;
    private bool cutReady;

    private void Start()
    {
        _mesh = gameObject.GetComponent<Mesh>();
        _filter = gameObject.GetComponent<MeshFilter>();
        _vertices = _mesh.vertices;
        Enumerate();
    }
    
    //ready
    private void Enumerate()
    {
        int nextVertexIndex = FindClosestVertex();
        
        for (;!cutReady;) 
        {
            List<int> similarVertices = FindSimilarVertices(_mesh.vertices[nextVertexIndex]);
            CheckTriangles(similarVertices);
            nextVertexIndex = GetNextVertex(similarVertices);
        }
    }

    //ready
    private int FindClosestVertex()
    {
        int j = 0;
        float curMinDistance = GetDistanceVertexToContactPoint(_mesh.vertices[0]);
        
        for (int i = 1; i < _mesh.vertices.Length; i++)
        {
            float nextDistance = GetDistanceVertexToContactPoint(_mesh.vertices[i]);
            if (nextDistance < curMinDistance)
            {
                curMinDistance = nextDistance;
                j = i;
            }
        }

        return j;
    }

    //ready
    private float GetDistanceVertexToContactPoint(Vector3 vertex)
    {
        return (_contactPoint - vertex).magnitude;
    }

    //ready
    private List<int> FindSimilarVertices(Vector3 wantedVertex)
    {
        List<int> vertices = new List<int>();
        
        for (int i = 0; i < _mesh.vertices.Length; i++)
        {
            if (wantedVertex == _mesh.vertices[i])
            {
                vertices.Add(i);
            }
        }
        
        return vertices;
    }

    //ready
    private void CheckTriangles(List<int> similarVertexIndices)
    {
        for (int i = 0; i < similarVertexIndices.Count; i++)
        {
            int curIndex = 0;
            for (;curIndex>=0;)
            {
                curIndex = FindFirstTriangleIndex(curIndex, similarVertexIndices[i]);
                if (PrepareIntersectionPoints(similarVertexIndices[i]) && CheckIntersectedTriangle(similarVertexIndices[i]))
                {
                    ApproveIntersectionPoints();    
                }
            }
        }
    }
    
    //ready
    private int FindFirstTriangleIndex(int startIndex, int vertexIndex)
    {
        int i = startIndex;
        for (; i < _mesh.triangles.Length; i++)
        {
            if (_mesh.triangles[i] == vertexIndex)
            {
                return i;
            }
        }

        return -1;
    }

    //ready
    private bool CheckIntersectedTriangle(int index)
    {
        if (_intersectedTriangles.FindIndex(p => p == index) >= 0)
        {
            return false;
        }
        
        return true;
    }

    /*private void PrepareIntersectionPoints(int index)
    {
        int firstTriangleIndex = index - index % 3;
        Vector3 newPoint1 = Vector3.zero, newPoint2 = Vector3.zero, crossProduct = Vector3.Cross(_dirU, _dirV);
        
        Vector3 firstTrianglePoint = _mesh.vertices[firstTriangleIndex];
        Vector3 secondTrianglePoint = _mesh.vertices[firstTriangleIndex+1];
        Vector3 thirdTrianglePoint = _mesh.vertices[firstTriangleIndex+2];
        
        //Может быть проблема со свойствами смешаннного произведения
        float result1 = Vector3.Dot(crossProduct, _contactPoint + firstTrianglePoint) / crossProduct.magnitude;
        float result2 = Vector3.Dot(crossProduct, _contactPoint + secondTrianglePoint) / crossProduct.magnitude;
        float result3 = Vector3.Dot(crossProduct, _contactPoint + thirdTrianglePoint) / crossProduct.magnitude;

        if ((result1 < 0 && result2 >= 0 && result3 >= 0) || (result1 >= 0 && result2 < 0 && result3 < 0))
        {
            newPoint1 = ComputeIntersectionPoint(firstTrianglePoint - secondTrianglePoint, firstTrianglePoint); 
            newPoint2 = ComputeIntersectionPoint(firstTrianglePoint - thirdTrianglePoint, firstTrianglePoint);
        }
        else if ((result1 >= 0 && result2 < 0 && result3 >= 0) || (result1 < 0 && result2 >= 0 && result3 < 0))
        {
            newPoint1 = ComputeIntersectionPoint(secondTrianglePoint - firstTrianglePoint, secondTrianglePoint); 
            newPoint2 = ComputeIntersectionPoint(secondTrianglePoint - thirdTrianglePoint, secondTrianglePoint);
        }
        else if ((result1 >= 0 && result2 >= 0 && result3 < 0) || (result1 < 0 && result2 < 0 && result3 >= 0))
        {
            newPoint1 = ComputeIntersectionPoint(thirdTrianglePoint - secondTrianglePoint, thirdTrianglePoint); 
            newPoint2 = ComputeIntersectionPoint(thirdTrianglePoint - firstTrianglePoint, thirdTrianglePoint);
        }
        
        _potentialNewVertices.Add(newPoint1);
        _potentialNewVertices.Add(newPoint2);
    }*/
    
    //ready. old version higher
    private bool PrepareIntersectionPoints(int index)
    {
        int firstTriangleIndex = index - index % 3;
        Vector3 crossProduct = Vector3.Cross(_dirU, _dirV);
        Vector3[] trianglePoints = new Vector3[3];
        float[] tripleProducts = new float[3];

        for (int i = 0; i < 3; i++)
        {
            //Может быть проблема со свойствами смешаннного произведения
            trianglePoints[i] = _mesh.vertices[firstTriangleIndex+i];
            tripleProducts[i] = Vector3.Dot(crossProduct, _contactPoint + trianglePoints[i]) / crossProduct.magnitude;
        }

        for (int i = 0; i < 3; i++)
        {
            if ((tripleProducts[i%3] < 0 && tripleProducts[(i+1)%3] >= 0 && tripleProducts[(i+2)%3] >= 0) ||
                (tripleProducts[i%3] >= 0 && tripleProducts[(i+1)%3] < 0 && tripleProducts[(i+2)%3] < 0))
            {
                Vector3 newPoint1 = ComputeIntersectionPoint(trianglePoints[i%3] - trianglePoints[(i+1)%3], trianglePoints[i%3]); 
                Vector3 newPoint2 = ComputeIntersectionPoint(trianglePoints[i%3] - trianglePoints[(i+2)%3], trianglePoints[i%3]);
                _potentialNewVertices.Add(newPoint1);
                _potentialNewVertices.Add(newPoint2);

                for (int j = 0; j < 3; j++)
                {
                    _intersectedTriangles.Add(firstTriangleIndex+i);    
                }
                
                return true;
            }
        }

        return false;
    }
    
    //ready
    private Vector3 ComputeIntersectionPoint(Vector3 guide, Vector3 straightStartPoint)
    {
        float[,] matrix = new float[3,3];
        
        for (int i = 0; i < 3; i++)
        {
            matrix[i, 0] = _dirU[i];
            matrix[i, 1] = _dirV[i];
            matrix[i, 2] = -guide[i];
        }

        matrix = FindInverseMatrix(matrix);

        return MultiplyMatrix3x3Vector3x1(matrix, straightStartPoint - _contactPoint);
    }

    //ready
    private float[,] FindInverseMatrix(float[,] matrix)
    {
        float[,] result = new float[3, 3];
        float[,] minor = new float[2,2];

        float det = ComputeDeterminant3x3(matrix);
        
        int A = 0, B = 0;
        for (int j = 0; j < 3; j++)
        {
            int a = 0, b = 0;
            for (int i = 0; i < 3; i++)
            {
                //первый столбец
                minor[0, 0] = matrix[(A+1)%3, (a+1)%3];
                minor[1, 0] = matrix[(B+2)%3, (a+1)%3];
                //второй столбец
                minor[0, 1] = matrix[(A+1)%3, (b+2)%3];
                minor[1, 1] = matrix[(B+2)%3, (b+2)%3];
                
                result[j,i] = (1/det)*ComputeDeterminant2x2(minor);
                
                b = a;
                a = -1;
            }

            B = A;
            A = -1;
        }

        return matrix;
    }
    
    //ready
    private float ComputeDeterminant3x3(float[,] matrix)
    {
        return matrix[0, 0] * matrix[1, 1] * matrix[2, 2] +
               matrix[0, 1] * matrix[1, 2] * matrix[2, 0] +
               matrix[0, 2] * matrix[1, 0] * matrix[2, 1] -
               matrix[0, 2] * matrix[1, 1] * matrix[2, 0] -
               matrix[0, 1] * matrix[1, 0] * matrix[2, 2] -
               matrix[0, 0] * matrix[1, 2] * matrix[2, 1];
    }
    
    //ready
    private float ComputeDeterminant2x2(float[,] minorMatrix)
    {
        return minorMatrix[0,0] * minorMatrix[1,1] - minorMatrix[1,0] * minorMatrix[0,1];
    }
    
    //ready
    private Vector3 MultiplyMatrix3x3Vector3x1(float[,] matrix, Vector3 vec)
    {
        Vector3 result = new Vector3();
        
        for (int j = 0; j < 3; j++)
        {
            for (int i = 0; i < 3; i++)
            {
                result[j] += matrix[j, i] * vec[i];
            }
        }

        return result;
    }
    
    //TODO: probably should save indices too
    private void ApproveIntersectionPoints()
    {
        for (int j = 0; j < _potentialNewVertices.Count; j++)
        {
            int counter = 0;
            for (int i = 0; i < _potentialNewVertices.Count; i++)
            {
                if (_potentialNewVertices[j] == _potentialNewVertices[i])
                {
                    counter++;
                }
            }

            if (counter > 1)
            {
                _potentialNewVertices.RemoveAll(p => p == _potentialNewVertices[j]);
            }
        }

        _approvedNewVertices.AddRange(_potentialNewVertices);
        _potentialNewVertices.Clear();
    }

    //ready
    private int GetNextVertex(List<int> vertexIndices)
    {
        for (int i = 0; i < vertexIndices.Count; i++)
        {
            for (int j = 0; j < _intersectedTriangles.Count; i++)
            {
                int firstTriangleIndex = vertexIndices[i] - vertexIndices[i] % 3;
                for (int k = 0; k < 3; k++)
                {
                    if (firstTriangleIndex + k != _intersectedTriangles[j])
                    {
                        return firstTriangleIndex + k;
                    }
                }
            }
        }

        cutReady = true;
        return -1;
    }
}
