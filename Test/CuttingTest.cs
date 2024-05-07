using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnumeratorTest : MonoBehaviour
{
    //Todo: надо отработать сценарий когда касание происходит в вершине. Довольно вероятный сценарий.
    //TODO: убрать вершины которые не существенны для разбиения
    
    private Vector3 _contactPoint = new Vector3(0.5f, 0.5f, 0.2f);
    private Vector3 _dirV = Vector3.right;
    private Vector3 _dirU = Vector3.up;
    private Mesh _mesh;
    private Vector3[] _vertices;
    private MeshFilter _filter;
    private List<Vector3> _potentialNewVertices = new List<Vector3>();
    private List<Vector3> _approvedNewVertices = new List<Vector3>();
    List<int> checkedVertices = new List<int>();
    private bool cutReady;

    private void Start()
    {
        _filter = gameObject.GetComponent<MeshFilter>();
        _mesh = _filter.mesh;
        _vertices = _mesh.vertices;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            checkedVertices.Clear();
            cutReady = false;
            Enumerate();
        }
    }

    //ready
    private void Enumerate()
    {
        int nextVertexIndex = FindClosestVertex();
        List<int> curIterationCheckedVertices = new List<int>();
        
        for (;!cutReady;) 
        {
            List<int> triangleIndicesOfCurrentVertices = GetAllTriangleIndices(FindSimilarVertexIndices(_mesh.vertices[nextVertexIndex]));
            checkedVertices.AddRange(triangleIndicesOfCurrentVertices);
            checkedVertices = checkedVertices.Distinct().ToList();
            
            curIterationCheckedVertices.AddRange(CheckTriangles(ref triangleIndicesOfCurrentVertices));
            ApproveIntersectionPoints(triangleIndicesOfCurrentVertices);

            nextVertexIndex = GetNextVertexIndex(curIterationCheckedVertices);
            
            checkedVertices.AddRange(curIterationCheckedVertices); 
            checkedVertices = checkedVertices.Distinct().ToList();
            curIterationCheckedVertices.Clear();
        }
        
        Debug.Log("END");
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
    private List<int> FindSimilarVertexIndices(Vector3 wantedVertex)
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
    private List<int> CheckTriangles(ref List<int> trianglesIndices)
    {
        List<int> newVertices = new List<int>();
        List<int> newTriangleIndices = new List<int>();
         
        for (int i = 0; i < trianglesIndices.Count; i++)
        {
            if (!GetTriangle(trianglesIndices[i]).TrueForAll(checkedVertices.Contains) && PrepareIntersectionPoints(trianglesIndices[i]))
            {
                newVertices.AddRange(GetTriangle(trianglesIndices[i]));
                newTriangleIndices.Add(trianglesIndices[i]);
            }
        }

        trianglesIndices = newTriangleIndices;
        return newVertices;
    }

    //ready
    private bool PrepareIntersectionPoints(int index)
    {
        int firstTriangleIndex = index - index % 3;
        Vector3 crossProduct = Vector3.Cross(_dirU, _dirV);
        Vector3[] trianglePoints = new Vector3[3];
        float[] tripleProducts = new float[3];

        for (int i = 0; i < 3; i++)
        {
            trianglePoints[i] = _mesh.vertices[_mesh.triangles[firstTriangleIndex+i]];
            tripleProducts[i] = Vector3.Dot(crossProduct, _contactPoint + trianglePoints[i]) / crossProduct.magnitude;
        }

        for (int i = 0; i < 3; i++)
        {
            if ((tripleProducts[i%3] < 0 && tripleProducts[(i+1)%3] >= 0 && tripleProducts[(i+2)%3] >= 0) ||
                (tripleProducts[i%3] >= 0 && tripleProducts[(i+1)%3] < 0 && tripleProducts[(i+2)%3] < 0))
            {
                Vector3 newPoint1 = ComputeIntersectionPoint(trianglePoints[(i+1)%3], trianglePoints[i%3]); 
                Vector3 newPoint2 = ComputeIntersectionPoint(trianglePoints[(i+2)%3], trianglePoints[i%3]);
                
                _potentialNewVertices.Add(newPoint1);
                _potentialNewVertices.Add(newPoint2);

                return true;
            }
        }

        return false;
    }
    
    //ready
    private Vector3 ComputeIntersectionPoint(Vector3 firstVertex, Vector3 secondVertex)
    {
        float[,] matrix = new float[3,3];

        Vector3 guide = secondVertex - firstVertex;

        for (int i = 0; i < 3; i++)
        {
            matrix[i, 0] = _dirU[i];
            matrix[i, 1] = _dirV[i];
            matrix[i, 2] = -guide[i];
        }

        matrix = FindInverseMatrix(matrix);

        Vector3 coefs = MultiplyMatrix3x3Vector3x1(matrix, secondVertex - _contactPoint);
        return secondVertex + (secondVertex - firstVertex) * coefs[2];
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

                float pow = (float) Math.Pow(-1, i + j);
                result[i,j] = (pow/det) * ComputeDeterminant2x2(minor);

                b = a;
                a = -1;
            }

            B = A;
            A = -1;
        }

        return result;
    }
    
    //ready
    private float ComputeDeterminant3x3(float[,] matrix)
    {
        float result = 0;
        result += matrix[0, 0] * matrix[1, 1] * matrix[2, 2];
        result += matrix[0, 1] * matrix[1, 2] * matrix[2, 0];
        result += matrix[0, 2] * matrix[1, 0] * matrix[2, 1];
        result -= matrix[0, 2] * matrix[1, 1] * matrix[2, 0];
        result -= matrix[0, 1] * matrix[1, 0] * matrix[2, 2];
        result -= matrix[0, 0] * matrix[1, 2] * matrix[2, 1];
        return result;
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
    
    //ready
    private void ApproveIntersectionPoints(List<int> similarVertices)
    {
        for (int i = 0; i < similarVertices.Count-1; i++)
        {
            if (similarVertices[i + 1] - similarVertices[i] <= 3)
            {
                for (int j = 0; j < 2; j++)
                {
                    int temp1 = 2 * i + j;
                    
                    //Todo: index out of range. solve pls)
                    bool canDeleteVertices = _potentialNewVertices[2 * i + j] == _potentialNewVertices[2 * i + 2] &&
                                             AreDotsLieOnStraightLine(_potentialNewVertices[2 * i + j],
                                                 _potentialNewVertices[2 * i + 3],
                                                 _potentialNewVertices[2 * i + (j + 1) % 2]) ||
                                             _potentialNewVertices[2 * i + j] == _potentialNewVertices[2 * i + 3] &&
                                             AreDotsLieOnStraightLine(_potentialNewVertices[2 * i + j],
                                                 _potentialNewVertices[2 * i + 2],
                                                 _potentialNewVertices[2 * i + (j + 1) % 2]);


                    if (canDeleteVertices)
                    {
                        Vector3 temp = _potentialNewVertices[2 * i + j];
                        _potentialNewVertices.RemoveAll(p => p == temp);
                    }
                }
            }
        }
        

        _approvedNewVertices.AddRange(_potentialNewVertices);
        _potentialNewVertices.Clear();
    }

    private bool AreDotsLieOnStraightLine(Vector3 sameVertex, Vector3 leftVertex, Vector3 rightVertex)
    {
        float temp = Vector3.Dot(leftVertex - sameVertex, rightVertex - sameVertex)/((leftVertex - sameVertex).magnitude * (rightVertex - sameVertex).magnitude);
        if (temp >= 0.99f || temp <= -0.99f)
        {
            return true;
        }

        return false;
    }

    //ready
    private List<int> GetTriangle(int triangleIndex)
    {
        List<int> result = new List<int>();
        
        result.Add(triangleIndex - triangleIndex%3);
        result.Add(triangleIndex - triangleIndex%3 + 1);
        result.Add(triangleIndex - triangleIndex%3 + 2);

        return result;
    }

    //ready
    private List<int> GetAllTriangleIndices(List<int> vertexIndices)
    {
        List<int> result = new List<int>();

        for (int i = 0; i < vertexIndices.Count; i++)
        {
            for (int j = 0; j < _mesh.triangles.Length; j++)
            {
                if (vertexIndices[i] == _mesh.triangles[j])
                {
                    result.Add(j);
                }
            }
        }

        return result;
    }

    //ready
    private int GetNextVertexIndex(List<int> newVertices)
    {
        int result = -1;
        
        if (!newVertices.TrueForAll(checkedVertices.Contains))
        {
            for (int i = 0; i < newVertices.Count; i++)
            {
                if(!checkedVertices.Contains(newVertices[i])) 
                {
                    result = _mesh.triangles[newVertices[i]];
                    break;
                }
            }
        }
        else
        {
            for (int i = 0; i < checkedVertices.Count; i++)
            {
                List<int> list = GetAllTriangleIndices(FindSimilarVertexIndices(_mesh.vertices[_mesh.triangles[checkedVertices[i]]]));
                if (!list.TrueForAll(checkedVertices.Contains))
                {
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (!checkedVertices.Contains(list[j]) && !GetTriangle(list[j]).TrueForAll(checkedVertices.Contains))
                        {
                            result = _mesh.triangles[list[j]];
                            break;
                        }
                    }
                
                    if (result != -1)
                    {
                        break;
                    }
                }
            }

            if (result == -1)
            {
                cutReady = true;
            }
        }
        
        return result;
    }
}
