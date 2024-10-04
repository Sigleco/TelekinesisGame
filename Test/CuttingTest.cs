using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using UnityEngine;

public class EnumeratorTest : MonoBehaviour
{
    private Vector3 _contactPoint = new Vector3(0.5f, 0.5f, 0.2f);
    private Vector3 _dirV = Vector3.right;
    private Vector3 _dirU = Vector3.up;
    private Mesh _mesh;
    private MeshFilter _filter;
    private List<int> checkedVertices = new List<int>();
    private List<Vector3> checkedVectors = new List<Vector3>();
    private List<SideStruct> leftSides = new List<SideStruct>();
    private List<SideStruct> rightSides = new List<SideStruct>();
    private List<AssociatedTriangle> _potentialVertices = new List<AssociatedTriangle>();
    private List<AssociatedTriangle> _approvedVertices = new List<AssociatedTriangle>();
    private Mesh leftMesh, rightMesh;
    private bool cutReady;

    private void Start()
    {
        _filter = gameObject.GetComponent<MeshFilter>();
        _mesh = _filter.mesh;
        leftMesh = new Mesh();
        rightMesh = new Mesh();
    }

    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            checkedVertices.Clear();
            cutReady = false;
            Enumerate();
            SeparateVertices();
        }*/
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Divide();
            leftMesh = CreateNewMeshes(leftSides);
            GameObject obj = new GameObject();
            obj.transform.position = Vector3.right * 1f;
            obj.AddComponent<MeshFilter>();
            MeshFilter filter = obj.GetComponent<MeshFilter>();
            filter.mesh = leftMesh;
            obj.AddComponent<MeshRenderer>();
            obj.AddComponent<BoxCollider>();
            
            rightMesh = CreateNewMeshes(rightSides);
            GameObject obj1 = new GameObject();
            obj1.transform.position = Vector3.right * 2f;
            obj1.AddComponent<MeshFilter>();
            MeshFilter filter1 = obj1.GetComponent<MeshFilter>();
            filter1.mesh = rightMesh;
            obj1.AddComponent<MeshRenderer>();
            obj1.AddComponent<BoxCollider>();
        }
    }

    private void Divide()
    {
        List<int> unUsedVertices = Enumerable.Range(0, _mesh.vertices.Length).ToList();
        while (unUsedVertices.Count > 0)
        {
            DivideSide(unUsedVertices);
        }
        
        checkedVectors = checkedVectors.Distinct().ToList();
        AddLastSide(checkedVectors, ref leftSides, ref rightSides);
    }

    private void DivideSide(List<int> unUsedVertices)
    {
        List<int> side = FindSide(unUsedVertices);
        SeparateSideVertices(side);
    }

    private List<int> FindSide(List<int> unusedVertices)
    {
        List<int> sideVertices = new List<int>();
        sideVertices.Add(unusedVertices[0]);
        unusedVertices.Remove(sideVertices[0]);

        CheckNextGenDots(ref sideVertices,  0,  1);
        unusedVertices.RemoveAll(x => sideVertices.Contains(x));
        return sideVertices;
    }

    private void CheckNextGenDots(ref List<int> sideVertices, int startIndex, int amountToCheck)
    {
        for (int j = startIndex; j < amountToCheck; j++)
        {
            for (int i = 0; i < _mesh.triangles.Length; i++)
            {
                if (sideVertices[j] == _mesh.triangles[i])
                {
                    List<int> triangle = GetTriangle(i);
                    foreach (int vert in triangle)
                    {
                        sideVertices.Add(_mesh.triangles[vert]);
                    }
                }
            }
        }

        int rawSize = sideVertices.Count;
        sideVertices = sideVertices.Distinct().ToList();
        if (rawSize != sideVertices.Count)
        {
            startIndex += amountToCheck;
            int temp = sideVertices.Count - startIndex;
            CheckNextGenDots( ref sideVertices,  startIndex, temp);
        }
    }
    
    private void SeparateSideVertices(List<int> side)
    {
        Vector3 crossProduct = Vector3.Cross(_dirU, _dirV);
        Vector3 newVertex1 = Vector3.zero, newVertex2 = Vector3.zero;

        List<(int, float)> left = new List<(int, float)>();
        List<(int, float)> right = new List<(int, float)>();
        
        for (int i = 0; i < side.Count; i++)
        {
            float temp = Vector3.Dot(crossProduct, _contactPoint + _mesh.vertices[side[i]]) / crossProduct.magnitude;
            if (temp >= 0)
            {
                left.Add((side[i], temp));
            }
            else
            {
                right.Add((side[i], temp));
            }
        }

        if (left.Count > 1 && right.Count > 1)
        {
            List<float> fl = new List<float>();
            List<Vector3> points = new List<Vector3>();
            
            left.Sort((p1, p2) => p1.Item2.CompareTo(p2.Item2));
            right.Sort((p1, p2) => -1 * p1.Item2.CompareTo(p2.Item2));

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    points.Add(ComputeIntersectionPoint(_mesh.vertices[left[i].Item1], _mesh.vertices[right[j].Item1]));
                }
            }

            Vector3 cross = Vector3.Cross(points[0], points[1]);
            for (int i = 1; i < 4; i++)
            {
                fl.Add(Vector3.SignedAngle(points[0], points[i], cross));
            }

            if (fl[0] > 0 && fl[1] > 0 && fl[2] > 0)
            {
                newVertex1 = points[0];
                newVertex2 = points[fl.IndexOf(fl.Max()) + 1];
            }
            else if (-fl[0] > 0 && -fl[1] > 0 && -fl[2] > 0)
            {
                newVertex1 = points[0];
                newVertex2 = points[fl.IndexOf(fl.Min()) + 1];
            }
            else
            {
                newVertex1 = points[fl.IndexOf(fl.Max()) + 1];
                newVertex2 = points[fl.IndexOf(fl.Min()) + 1];
            }
        }
        else if (left.Count == 1)
        {
            right.Sort((p1, p2) => -1 * p1.Item2.CompareTo(p2.Item2));
            newVertex1 = ComputeIntersectionPoint(_mesh.vertices[left[0].Item1], _mesh.vertices[right[0].Item1]);
            newVertex2 = ComputeIntersectionPoint(_mesh.vertices[left[0].Item1], _mesh.vertices[right[1].Item1]);
        }
        else if(right.Count == 1)
        {
            left.Sort((p1, p2) => p1.Item2.CompareTo(p2.Item2));
            newVertex1 = ComputeIntersectionPoint(_mesh.vertices[right[0].Item1], _mesh.vertices[left[0].Item1]);
            newVertex2 = ComputeIntersectionPoint(_mesh.vertices[right[0].Item1], _mesh.vertices[left[1].Item1]);
        }
        
        List<Vector3> _right = right.ConvertAll(x => _mesh.vertices[x.Item1]);
        List<Vector3> _left = left.ConvertAll(x => _mesh.vertices[x.Item1]);
        
        if (right.Count > 0 && left.Count > 0)
        {
            _right.InsertRange(0, new []{newVertex1, newVertex2});
            int[] rightTriangles = CreateTriangles(_right, _mesh.normals[right[0].Item1]);
            rightSides.Add(new SideStruct(rightTriangles, _right.ToArray(), _mesh.normals[right[0].Item1], _mesh.tangents[right[0].Item1]));
            
            _left.InsertRange(0, new []{newVertex1, newVertex2});
            int[] leftTriangles = CreateTriangles(_left, _mesh.normals[left[0].Item1]);
            leftSides.Add(new SideStruct(leftTriangles, _left.ToArray(), _mesh.normals[left[0].Item1], _mesh.tangents[left[0].Item1]));
            
            checkedVectors.Add(newVertex1);
            checkedVectors.Add(newVertex2);
        }
        else if (left.Count > 0 && right.Count == 0)
        {
            int[] leftTriangles = CreateTriangles(_left, _mesh.normals[left[0].Item1]);
            leftSides.Add(new SideStruct(leftTriangles, _left.ToArray(), _mesh.normals[left[0].Item1], _mesh.tangents[left[0].Item1]));
        }
        else if (right.Count > 0 && left.Count == 0)
        {
            int[] rightTriangles = CreateTriangles(_right, _mesh.normals[right[0].Item1]);
            rightSides.Add(new SideStruct(rightTriangles, _right.ToArray(), _mesh.normals[right[0].Item1], _mesh.tangents[right[0].Item1]));
        }
    }

    private int[] CreateTriangles(List<Vector3> vertices, Vector3 normal)
    {
        int[] triangles = new int[(vertices.Count - 2) * 3];
        Vector3 mainLine = vertices[1] - vertices[0];
        List<(Vector3, int, float)> maps = vertices.ConvertAll(x => (x, vertices.IndexOf(x), 0f));
        for (int i = 2; i < vertices.Count; i++)
        {
            //Возможно придется развернуть нормаль стороны
            maps[i] = (maps[i].Item1, maps[i].Item2, Vector3.SignedAngle(mainLine, vertices[i] - vertices[0],  normal));
        }
        
        maps.Sort(1, maps.Count - 1, Comparer<(Vector3, int, float)>.Create((p1, p2) => p1.Item3.CompareTo(p2.Item3)));

        for (int i = 0; i < maps.Count - 2; i++)
        {
            triangles[3 * i] = maps[0].Item2;
            triangles[3 * i + 1] = maps[i + 1].Item2;
            triangles[3 * i + 2] = maps[i + 2].Item2;
        }

        return triangles;
    }

    private void AddLastSide(List<Vector3> vertices, ref List<SideStruct> sides, ref List<SideStruct> oppositeSides)
    {
        vertices = vertices.Distinct().ToList();
        Vector3 nm = Vector3.Cross(vertices[0] - vertices[1], vertices[0] - vertices[2]);

        Vector3[] sideVertices = sides[0].GetVertices();
        float[] angles = new float[2];
        
        for (int i = 0; i < angles.Length; i++)
        {
            angles[i] = Vector3.Angle(nm, sideVertices[i] - vertices[0]);
        }

        if (angles[0] < 90 && angles[1] < 90)
        {
            nm *= -1;
        }

        int[] triangles = CreateTriangles(vertices, nm);
        
        sides.Add(new SideStruct((int[])triangles.Clone(), vertices.ToArray(), nm, vertices[0] - vertices[1]));

        for (int i = 0; i < triangles.Length;i +=3)
        {
            (triangles[i + 1], triangles[i + 2]) = (triangles[i + 2], triangles[i + 1]);
        }

        oppositeSides.Add(new SideStruct(triangles, vertices.ToArray(), -1 * nm, vertices[0] - vertices[1]));
    }
    
    private Mesh CreateNewMeshes(List<SideStruct> sides)
    {
        Mesh newMesh = new Mesh();
        int vCounter = 0;
        int trCounter = 0;
        for (int i = 0; i < sides.Count; i++)
        {
            vCounter += sides[i].GetVertices().Length;
            trCounter += sides[i].GetTriangles().Length;
        }

        Vector3[] _vertices = new Vector3[vCounter];
        Vector3[] _normals = new Vector3[vCounter];
        Vector4[] _tangents = new Vector4[vCounter];
        int[] _triangles = new int[trCounter];

        vCounter = 0;
        trCounter = 0;
        for (int i = 0; i < sides.Count; i++)
        {
            Vector3[] tempV = sides[i].GetVertices();
            Array.Copy(tempV, 0, _vertices, vCounter, tempV.Length);

            for (int j = 0; j < tempV.Length; j++)
            {
                Vector3 nm = sides[i].GetNormal();
                Vector3 tg = sides[i].GetTangent();
                _normals[vCounter + j] = nm;
                _tangents[vCounter + j] = new Vector4(tg.x, tg.y, tg.z, -1f);
            }
            
            int[] tr = sides[i].GetTriangles();
            for (int j = 0; j < tempV.Length - 2; j++)
            {
                _triangles[trCounter + 3 * j] = tr[3 * j] + vCounter;
                _triangles[trCounter + 3 * j + 1] = tr[3 * j + 1] + vCounter;
                _triangles[trCounter + 3 * j + 2] = tr[3 * j + 2] + vCounter;
            }

            vCounter += tempV.Length;
            trCounter += tr.Length;
        }

        newMesh.vertices = _vertices;
        newMesh.normals = _normals;
        newMesh.tangents = _tangents;
        newMesh.triangles = _triangles;
        
        return newMesh;
    }

    /// <summary>
    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    //ready
    private void Enumerate()
    {
        int nextVertexIndex = FindClosestVertex();
        List<int> curIterationCheckedVertices = new List<int>();
        
        for (;!cutReady;) 
        {
            List<int> triangleIndicesOfCurrentVertices = GetAllTriangleStartIndices(FindSimilarVertexIndices(_mesh.vertices[nextVertexIndex]));
            checkedVertices.AddRange(triangleIndicesOfCurrentVertices);
            checkedVertices = checkedVertices.Distinct().ToList();
            
            curIterationCheckedVertices.AddRange(CheckTriangles(ref triangleIndicesOfCurrentVertices));
            ApproveIntersectionPoints(triangleIndicesOfCurrentVertices);

            nextVertexIndex = GetNextVertexIndex(curIterationCheckedVertices);
            
            checkedVertices.AddRange(curIterationCheckedVertices); 
            checkedVertices = checkedVertices.Distinct().ToList();
            curIterationCheckedVertices.Clear();
        }
        Debug.Log("");
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
                int[] temp = new int [3] {firstTriangleIndex, firstTriangleIndex + 1, firstTriangleIndex + 2};
                _potentialVertices.Add(new AssociatedTriangle(ComputeIntersectionPoint(trianglePoints[(i+1)%3], trianglePoints[i%3]), temp));
                _potentialVertices.Add(new AssociatedTriangle(ComputeIntersectionPoint(trianglePoints[(i+2)%3], trianglePoints[i%3]), temp));
                
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
                //first column
                minor[0, 0] = matrix[(A+1)%3, (a+1)%3];
                minor[1, 0] = matrix[(B+2)%3, (a+1)%3];
                //second column
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
        List<Vector3> extraVertices = new List<Vector3>();
        for (int i = 0; i < similarVertices.Count-1; i++)
        {
            //this condition may cause a problem
            if (similarVertices[i + 1] - similarVertices[i] <= 3)
            {
                for (int j = 0; j < 2; j++)
                {
                    bool canDeleteVertices = _potentialVertices[2 * i + j].GetVertexPosition() == _potentialVertices[2 * i + 2].GetVertexPosition() &&
                                             AreDotsLieOnStraightLine(_potentialVertices[2 * i + j].GetVertexPosition(),
                                                 _potentialVertices[2 * i + 3].GetVertexPosition(),
                                                 _potentialVertices[2 * i + (j + 1) % 2].GetVertexPosition()) ||
                                             _potentialVertices[2 * i + j].GetVertexPosition() == _potentialVertices[2 * i + 3].GetVertexPosition() &&
                                             AreDotsLieOnStraightLine(_potentialVertices[2 * i + j].GetVertexPosition(),
                                                 _potentialVertices[2 * i + 2].GetVertexPosition(),
                                                 _potentialVertices[2 * i + (j + 1) % 2].GetVertexPosition());

                    if (canDeleteVertices)
                    {
                        extraVertices.Add(_potentialVertices[2 * i + j].GetVertexPosition());
                    }
                }
            }
        }

        if (extraVertices.Count > 0)
        {
            for (int i = 0; i < extraVertices.Count; i++)
            {
                _potentialVertices.RemoveAll(p => p.GetVertexPosition() == extraVertices[i]);
            }
        }

        _approvedVertices.AddRange(_potentialVertices);
        _potentialVertices.Clear();
    }

    //ready
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
    private List<int> GetAllTriangleStartIndices(List<int> vertexIndices)
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
    
    private List<int> GetAllTriangleStartIndices(int vertexIndex)
    {
        List<int> result = new List<int>();

        for (int j = 0; j < _mesh.triangles.Length; j++)
        {
            if (vertexIndex == _mesh.triangles[j])
            {
                result.Add(j);
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
                List<int> list = GetAllTriangleStartIndices(FindSimilarVertexIndices(_mesh.vertices[_mesh.triangles[checkedVertices[i]]]));
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

    private void PrepareMeshesData()
    {
        SeparateVertices();
        //SetTriangles();
    }

    private void SeparateVertices()
    {
        List<Vector3> leftVertices = new List<Vector3>(), rightVertices = new List<Vector3>();
        List<int> leftVertexIndices = new List<int>(), rightVertexIndices = new List<int>();
        Vector3 crossProduct = Vector3.Cross(_dirU, _dirV);

        for (int i = 0; i < _mesh.vertices.Length; i++)
        {
            if(Vector3.Dot(crossProduct, _contactPoint + _mesh.vertices[i]) / crossProduct.magnitude < 0)
            {
                leftVertices.Add(_mesh.vertices[i]);
                leftVertexIndices.Add(i);
            }
            else
            {
                rightVertices.Add(_mesh.vertices[i]);
                rightVertexIndices.Add(i);
            }
        }

        //PrepareNewMesh(leftVertices, leftVertexIndices, _approvedVertices);
        //PrepareNewMesh(rightVertices, rightVertexIndices, _approvedVertices);
        CreateNewMesh(leftVertices, leftVertexIndices, _approvedVertices);

        leftMesh.vertices = leftVertices.ToArray();
        rightMesh.vertices = rightVertices.ToArray();
    }

    private void SetTriangles()
    {
        Vector3[] vertices = leftMesh.vertices;
        List<int> leftTriangles = new List<int>(), rightTriangles = new List<int>();
        for (int i = 0; i < _mesh.triangles.Length; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                /*if (leftMesh.vertices.)
                {
                    
                }
                */
            }
        }
    }

    private void CreateNewMesh(List<Vector3> vertices, List<int> indices, List<AssociatedTriangle> sectionPoints)
    {
        Vector3[] vertexArray = new Vector3[24];
        int[] newTriangleIndices = new int[36];
        for (int i = 0; i < indices.Count; i++)
        {
            vertexArray[indices[i]] = vertices[i];

        }

        for (int i = 0; i < _mesh.triangles.Length; i++)
        {
            if (indices.Contains(_mesh.triangles[i]))
            {
                newTriangleIndices[i] = _mesh.triangles[i];
            }
            else
            {
                newTriangleIndices[i] = -1;
            }
        }
        
        for (int i = 0; i < newTriangleIndices.Length; i += 3)
        {
            if (newTriangleIndices[i] != -1 && newTriangleIndices[i + 1] != -1 && newTriangleIndices[i + 2] != -1 ||
                newTriangleIndices[i] == -1 && newTriangleIndices[i + 1] == -1 && newTriangleIndices[i + 2] == -1)
            {
                continue;
            }
            else if(newTriangleIndices[i] == -1 && newTriangleIndices[i + 1] != -1 && newTriangleIndices[i + 2] != -1 ||
                    newTriangleIndices[i] != -1 && newTriangleIndices[i + 1] == -1 && newTriangleIndices[i + 2] != -1 ||
                    newTriangleIndices[i] != -1 && newTriangleIndices[i + 1] != -1 && newTriangleIndices[i + 2] == -1)
            {
                //check if triangle should add 2 section points to 1 empty triangle slot
                
            }
            else if(newTriangleIndices[i] == -1 && newTriangleIndices[i + 1] == -1 && newTriangleIndices[i + 2] != -1 ||
                    newTriangleIndices[i] != -1 && newTriangleIndices[i + 1] == -1 && newTriangleIndices[i + 2] == -1 ||
                    newTriangleIndices[i] == -1 && newTriangleIndices[i + 1] != -1 && newTriangleIndices[i + 2] == -1)
            {
                int keyVertexIndex = -1;
                int keyTriangleIndex = -1;
                for (int j = 0; j < 3; j++)
                {
                    if (newTriangleIndices[i+j] != -1)
                    {
                        keyVertexIndex = newTriangleIndices[i+j];
                        keyTriangleIndex = i + j;
                    }    
                }

                int nextTriangleIndex = FindAdjacentTriangleIndex(keyVertexIndex, newTriangleIndices);
                int newVertexIndex = -1;
                int newTriangleIndex = 0;
                if (nextTriangleIndex != -1)
                {
                    InsertNewVertex(sectionPoints, nextTriangleIndex, newTriangleIndices, vertexArray, ref newVertexIndex, ref newTriangleIndex);

                    if (newVertexIndex != -1)
                    {
                        //023 302 230
                        //0   -1  -2
                        //031 103 310
                        //0   -1  -2
                        int nullIndexOffset = 3 - (3 - nextTriangleIndex % 3 + newTriangleIndex % 3) % 3;
                        int newIndexOffset = (keyTriangleIndex % 3 + nullIndexOffset)%3;
                        newTriangleIndices[keyTriangleIndex - keyTriangleIndex%3 + newIndexOffset]= newVertexIndex;

                        for (int m = 0;  m < 3; m++)
                        {
                            if (newTriangleIndices[i+m] == -1)
                            {
                                InsertNewVertex(sectionPoints, i+m, newTriangleIndices, vertexArray, ref newVertexIndex, ref newTriangleIndex);
                            }
                        }
                    }
                }
            }
        }

        Debug.Log("Done");
    }

    private void InsertNewVertex(List<AssociatedTriangle> sectionPoints, int wantedTriangleIndex, int[]newTriangleIndices, Vector3[] vertexArray, ref int newVertexIndex, ref int newTriangleIndex)
    {
        for(int j = 0; j < sectionPoints.Count; j++)
        {
            bool isVertexSetted = false;
            if (sectionPoints[j].GetTriangle().Contains(wantedTriangleIndex))
            {
                int temp = wantedTriangleIndex - wantedTriangleIndex % 3;
                for (int k = 0; k < 3; k++)
                {
                    if (newTriangleIndices[temp + k] == -1)
                    {
                        newVertexIndex = SetNewVertex(vertexArray, sectionPoints[j].GetVertexPosition());
                        if (temp != -1)
                        {
                            newTriangleIndices[temp + k] = newVertexIndex;
                            newTriangleIndex = temp + k;
                            isVertexSetted = true;
                            break;
                        }
                    }
                }
            }

            if (isVertexSetted)
            {
                break;
            }
        }
    }

    private int FindAdjacentTriangleIndex(int keyValueIndex, int[] triangles)
    {
        for (int j = 0; j < triangles.Length; j += 3)
        {
            if(triangles[j] == -1 && triangles[j + 1] != -1 && triangles[j + 2] != -1 ||
                    triangles[j] != -1 && triangles[j + 1] == -1 && triangles[j + 2] != -1 ||
                    triangles[j] != -1 && triangles[j + 1] != -1 && triangles[j + 2] == -1)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (triangles[j+i] == keyValueIndex)
                    {
                        return j+i;
                    }
                }
            }
        }

        return -1;
    }

    private int SetNewVertex(Vector3[] array, Vector3 point)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == Vector3.zero)
            {
                array[i] = point;
                return i;
            }
        }

        return -1;
    }

    private void PrepareNewMesh(List<Vector3> vertices, List<int> indices, List<AssociatedTriangle> sectionPoints)
    {
        //создаем массивы для финальных значений
        int counter = 0;
        Vector3[] finalVertices = new Vector3[indices.Count + (int) (sectionPoints.Count * 1.5f)];
        List<int> finalTriangles = new List<int>();

        //заполняем массив финальных значений с начала без пропусков старыми врешинами
        for (int i = 0; i < indices.Count; i++)
        {
            finalVertices[i] = vertices[i];
            counter++;
        }

        List<List<int>> trianglesIndices = new List<List<int>>();

        //Цикл который проверяет наличие у каждой вершины полных треугольников
        for (int i = 0; i < indices.Count; i++)
        {
            trianglesIndices.Clear();
            
            //выбираем индексы треугольников, в которые входит выбранная вершина
            for (int j = 0; j < _mesh.triangles.Length; j++)
            {
                if (indices[i] == _mesh.triangles[j])
                {
                    trianglesIndices.Add(GetTriangle(j));
                }
            }
            
            //цикл для добавления треугольников в финальный массив и дополнения недостающих треугольников вершинами
            List<int> triangle = new List<int>();
            for (int j = 0; j < trianglesIndices.Count; j++)
            {
                //todo: необходима проверка на повтор треугольника

                //переводим индексы треугольников в значения 
                triangle.Clear();
                for (int ind = 0; ind < 3; ind++)
                {
                    triangle.Add(_mesh.triangles[trianglesIndices[j][ind]]);    
                }

                //перебирает возможные варианты пересечения треугольника и добавляет в него вершины
                switch (SectionCase(triangle, indices))
                {
                    //треугольник полностью есть в старых вершинах
                    case 1:
                        for (int ind = 0; ind < indices.Count; ind++)
                        {
                            if (triangle.Contains(indices[ind]))
                            {
                                finalTriangles.Add(ind);
                            }
                        }
                        break;
                    //не хватает одной вершины
                    case  0:
                        for (int ind = 0; ind < sectionPoints.Count; ind++)
                        {
                            if (triangle.ToArray() == sectionPoints[ind].GetTriangle())
                            {
                                for (int temp = 0; temp < 3; temp++)
                                {
                                    if (!indices.Contains(triangle[temp]))
                                    {
                                        finalVertices[counter] = sectionPoints[ind].GetVertexPosition();
                                        finalTriangles.Add(counter);
                                        counter++;
                                    }
                                    else
                                    {
                                        finalTriangles.Add(triangle[temp]);
                                    }
                                }

                                break;
                            }
                        }
                        break;
                    //не хватает двух вершин 
                    case -1:
                        for (int ind = 0; ind < sectionPoints.Count; ind++)
                        {
                            if (triangle.ToArray() == sectionPoints[ind].GetTriangle())
                            {
                                int temp;
                                int[] tempTriangle = new int[3];
                                for (temp = 0; temp < 3; temp++)
                                {
                                    if (indices.Contains(triangle[temp]))
                                    {
                                        tempTriangle[temp] = triangle[temp];
                                    }
                                }

                                finalVertices[counter] = sectionPoints[ind].GetVertexPosition();
                                triangle[(temp + 1) % 3] = counter;
                                counter++;

                                for (int index = 0; index < sectionPoints.Count; index++)
                                {
                                    if (sectionPoints[index].IsAdjacentTriangle(triangle.ToArray()))
                                    {
                                        finalVertices[counter] = sectionPoints[ind].GetVertexPosition();
                                        triangle[(temp + 2) % 3] = counter;
                                        counter++;
                                        finalTriangles.AddRange(tempTriangle);
                                        break;
                                    }
                                }

                                break;
                            }
                        }
                        break;
                }
            }
        }
    }

    private int SectionCase(List<int> triangle, List<int> indices)
    {
        int result = 0;
        bool a = indices.Contains(triangle[0]);
        bool b = indices.Contains(triangle[1]);
        bool c = indices.Contains(triangle[2]);

        if (a && b && c)
        {
            return 1;
        }
        else if (a && b && !c || a && !b && c || !a && b && c)
        {
            return 0;
        }
        else if (a && !b && !c || !a && !b && c || !a && b && !c)
        {
            return -1;
        }

        return result;
    }
}


