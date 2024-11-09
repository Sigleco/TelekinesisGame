using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class Cutter: ICutter
{
    private Vector3 _contactPoint;
    private Vector3 _dirV;
    private Vector3 _dirU;
    private Mesh _mesh;
    private GameObject _parentObject;
    
    private List<Vector3> checkedVectors = new List<Vector3>();
    private List<SideStruct> leftSides = new List<SideStruct>();
    private List<SideStruct> rightSides = new List<SideStruct>();
    private Mesh leftMesh, rightMesh;

    public void SetCuttingParams(Vector3 contactPoint, Vector3 planeTangent1, Vector3 planeTangent2, GameObject cuttingObj)
    {
        _contactPoint = contactPoint;
        _dirV = planeTangent1.normalized;
        _dirU = planeTangent2.normalized;
        _parentObject = cuttingObj;
        if (_parentObject != null)
        {
            MeshFilter filter = _parentObject.GetComponent<MeshFilter>();
            _mesh = filter.mesh;
        }
    }

    //Temporary function
    private void InstantiateObjects()
    {
        GameObject obj = new GameObject();
        obj.transform.SetPositionAndRotation(_parentObject.transform.position + Vector3.forward * 2f, _parentObject.transform.rotation);
        MeshFilter filter = obj.AddComponent<MeshFilter>();
        filter.mesh = leftMesh;
        MeshRenderer rend = obj.AddComponent<MeshRenderer>();
        rend.material = _parentObject.GetComponent<MeshRenderer>().material;
        //obj.AddComponent<BoxCollider>();
            
        rightMesh = CreateNewMeshes(rightSides);
        GameObject obj1 = new GameObject();
        obj1.transform.SetPositionAndRotation(_parentObject.transform.position + Vector3.forward * 3f, _parentObject.transform.rotation);
        MeshFilter filter1 = obj1.AddComponent<MeshFilter>();
        filter1.mesh = rightMesh;
        MeshRenderer rend1 = obj1.AddComponent<MeshRenderer>();
        rend1.material = _parentObject.GetComponent<MeshRenderer>().material;
        //obj1.AddComponent<BoxCollider>();
        Object.Destroy(_parentObject);
    }

    public void StartCutting()
    {
        bool A = true;
        Vector3 temp = _mesh.vertices[0];
        List<int> ar = new List<int>();
        int counter = 0;
        for (int i = 1; i < _mesh.vertices.Length; i++)
        { 
            if (temp == _mesh.vertices[i])
            {
                ar.AddRange(_mesh.triangles);
                A = false;
                counter++;
            }
        }

        A = true;

        /*List<int> unUsedVertices = Enumerable.Range(0, _mesh.vertices.Length).ToList();
        while (unUsedVertices.Count > 0)
        {
            DivideSide(unUsedVertices);
        }
        
        checkedVectors = checkedVectors.Distinct().ToList();
        AddLastSide(checkedVectors, ref leftSides, ref rightSides);
        leftMesh = CreateNewMeshes(leftSides);
        rightMesh = CreateNewMeshes(rightSides);
        InstantiateObjects();*/
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
    
    //TODO
    private void SeparateSideVertices(List<int> side)
    {
        Vector3 crossProduct = Vector3.Cross(_dirU, _dirV);
        Vector3 newVertex1 = Vector3.zero, newVertex2 = Vector3.zero;

        List<(int, float)> left = new List<(int, float)>();
        List<(int, float)> right = new List<(int, float)>();
        
        for (int i = 0; i < side.Count; i++)
        {
            float temp = Vector3.Dot(crossProduct, _contactPoint +_mesh.vertices[side[i]]) / crossProduct.magnitude;
            if (temp >= 0)
            {
                left.Add((side[i], temp));
            }
            else
            {
                right.Add((side[i], temp));
            }
        }
        
        List<Vector3> _right = right.ConvertAll(x => _mesh.vertices[x.Item1]);
        List<Vector3> _left = left.ConvertAll(x => _mesh.vertices[x.Item1]);
        
        if (left.Count == 0 || right.Count == 0) 
        {
            if (left.Count > 0 && right.Count == 0)
            {
                int[] leftTriangles = CreateTriangles(_left, _mesh.normals[left[0].Item1]);
                leftSides.Add(new SideStruct(leftTriangles, _left.ToArray(), _mesh.normals[left[0].Item1], _mesh.tangents[left[0].Item1]));
            }
            else if (right.Count > 0 && left.Count == 0)
            {
                int[] rightTriangles = CreateTriangles(_right, _mesh.normals[right[0].Item1]);
                rightSides.Add(new SideStruct(rightTriangles, _right.ToArray(), _mesh.normals[right[0].Item1], _mesh.tangents[right[0].Item1]));
            }
            return;
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

        _right.InsertRange(0, new []{newVertex1, newVertex2});
        int[] _rightTriangles = CreateTriangles(_right, _mesh.normals[right[0].Item1]);
        rightSides.Add(new SideStruct(_rightTriangles, _right.ToArray(), _mesh.normals[right[0].Item1], _mesh.tangents[right[0].Item1]));
        
        _left.InsertRange(0, new []{newVertex1, newVertex2});
        int[] _leftTriangles = CreateTriangles(_left, _mesh.normals[left[0].Item1]);
        leftSides.Add(new SideStruct(_leftTriangles, _left.ToArray(), _mesh.normals[left[0].Item1], _mesh.tangents[left[0].Item1]));
        
        checkedVectors.Add(newVertex1);
        checkedVectors.Add(newVertex2);
    }

    private int[] CreateTriangles(List<Vector3> vertices, Vector3 normal)
    {
        int[] triangles = new int[(vertices.Count - 2) * 3];
        Vector3 mainLine = vertices[1] - vertices[0];
        List<(Vector3, int, float)> maps = vertices.ConvertAll(x => (x, vertices.IndexOf(x), 0f));
        for (int i = 2; i < vertices.Count; i++)
        {
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
            Vector3[] tempAr = sides[i].GetVertices();
            Array.Copy(tempAr, 0, _vertices, vCounter, tempAr.Length);
            tempAr = sides[i].GetNormals();
            Array.Copy(tempAr, 0, _normals, vCounter, tempAr.Length);
            Vector4[] tempTan = sides[i].GetTangents();
            Array.Copy(tempTan, 0, _tangents, vCounter, tempAr.Length);

            int[] tr = sides[i].GetTriangles();
            for (int j = 0; j < tempAr.Length - 2; j++)
            {
                _triangles[trCounter + 3 * j] = tr[3 * j] + vCounter;
                _triangles[trCounter + 3 * j + 1] = tr[3 * j + 1] + vCounter;
                _triangles[trCounter + 3 * j + 2] = tr[3 * j + 2] + vCounter;
            }

            vCounter += tempAr.Length;
            trCounter += tr.Length;
        }

        newMesh.vertices = _vertices;
        newMesh.normals = _normals;
        newMesh.tangents = _tangents;
        newMesh.triangles = _triangles;
        
        return newMesh;
    }
    
    private List<int> GetTriangle(int triangleIndex)
    {
        List<int> result = new List<int>();
        
        result.Add(triangleIndex - triangleIndex%3);
        result.Add(triangleIndex - triangleIndex%3 + 1);
        result.Add(triangleIndex - triangleIndex%3 + 2);

        return result;
    }
    
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

        Vector3 coefs = MultiplyMatrix3x3Vector3x1(matrix, /*secondVertex - */_contactPoint);
        return secondVertex + (secondVertex - firstVertex) * coefs[2];
    }

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
    
    private float ComputeDeterminant2x2(float[,] minorMatrix)
    {
        return minorMatrix[0,0] * minorMatrix[1,1] - minorMatrix[1,0] * minorMatrix[0,1];
    }
    
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
}