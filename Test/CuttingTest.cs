using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class EnumeratorTest : MonoBehaviour
{
    private Vector3 _contactPoint = new Vector3(0, 0.5f, 0);
    private Vector3 _dirV = Vector3.right;
    private Vector3 _dirU = Vector3.up;
    private Mesh _mesh;
    private MeshFilter _filter;
    private List<Vector3> checkedVectors = new List<Vector3>();
    private List<Side> leftSides = new List<Side>();
    private List<Side> rightSides = new List<Side>();
    private Mesh leftMesh, rightMesh;

    private void Start()
    {
        _filter = gameObject.GetComponent<MeshFilter>();
        _mesh = _filter.mesh;
        leftMesh = new Mesh();
        rightMesh = new Mesh();
    }

    private void OnDrawGizmos()
    {
        if (checkedVectors == null || checkedVectors.Count == 0)
            return;

        Gizmos.color = Color.red;

        // Рисуем каждую вершину
        foreach (var vertex in checkedVectors)
        {
            Gizmos.DrawSphere(transform.position + vertex, 0.1f);
        }

        // Соединяем вершины линиями, если их больше одной
        for (int i = 0; i < checkedVectors.Count - 1; i++)
        {
            Gizmos.DrawLine(transform.position + checkedVectors[i], transform.position + checkedVectors[i + 1]);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriangleDivide();
            leftMesh = CreateNewMesh(leftSides);
            //leftMesh = CreateMeshFromSide(leftSides[5]);
            GameObject obj = new GameObject {transform = {position = Vector3.right * 1f}};
            MeshFilter filter = obj.AddComponent<MeshFilter>();
            filter.mesh = leftMesh;
            MeshRenderer rend = obj.AddComponent<MeshRenderer>();
            rend.material = _filter.gameObject.GetComponent<MeshRenderer>().material;
            obj.AddComponent<BoxCollider>();

            rightMesh = CreateNewMesh(rightSides);
            GameObject obj1 = new GameObject {transform = {position = Vector3.right * 2f}};
            MeshFilter filter1 = obj1.AddComponent<MeshFilter>();
            filter1.mesh = rightMesh;
            MeshRenderer rend1 = obj1.AddComponent<MeshRenderer>();
            rend1.material = _filter.gameObject.GetComponent<MeshRenderer>().material;
            obj1.AddComponent<BoxCollider>();

            //LogData(leftSides, leftMesh);
        }
    }

    private void TriangleDivide()
    {
        for (int i = 0; i < _mesh.triangles.Length; i += 3)
        {
            if (IsTriangleDivided(i))
            {
                DivideTriangle(i, out var leftSide, out var rightSide);
                AddNewVerticesProperties(ref leftSide, ref rightSide);
                CreateNewTriangles(ref leftSide, GetNormal(leftSide, i));
                CreateNewTriangles(ref rightSide, GetNormal(rightSide, i));
                leftSides.Add(leftSide);
                rightSides.Add(rightSide);
            }
            else
            {
                PutTriangleToSide(i);
            }
        }

        //MergeCoplanarFaces();

        MagnitudeTest(checkedVectors.Distinct(new Vector3Comparer()).ToList());
        CoplanarityTest(checkedVectors.Distinct(new Vector3Comparer()).ToList());
        FullRotationTest(checkedVectors.Distinct(new Vector3Comparer()).ToList());

        AddLastSide(checkedVectors.Distinct(new Vector3Comparer()).ToList(), ref leftSides, ref rightSides);
    }

    private bool IsTriangleDivided(int startTriangleIndex)
    {
        Vector3 crossProduct = Vector3.Cross(_dirU, _dirV);
        List<Vector3> triangle = GetTriangle(startTriangleIndex).ConvertAll(x => _mesh.vertices[_mesh.triangles[x]]);
        bool result, logAnd = true, logOr = false;

        for (int i = 0; i < triangle.Count; i++)
        {
            float temp = Vector3.Dot(crossProduct, _contactPoint + triangle[i]) / crossProduct.magnitude;
            if (temp >= 0)
            {
                logAnd = logAnd & true;
                logOr = true;
            }
            else
            {
                logAnd = false;
                logOr = logOr | false;
            }
        }

        if (logAnd != logOr)
        {
            return true;
        }

        return false;
    }

    private void PutTriangleToSide(int startTriangleIndex)
    {
        List<int> triangle = GetTriangle(startTriangleIndex);
        Vector3[] vertices = triangle.ConvertAll(x => _mesh.vertices[_mesh.triangles[x]]).ToArray();
        Vector3[] normals = triangle.ConvertAll(x => _mesh.normals[_mesh.triangles[x]]).ToArray();
        Vector4[] tangents = triangle.ConvertAll(x => _mesh.tangents[_mesh.triangles[x]]).ToArray();

        Vector3 crossProduct = Vector3.Cross(_dirU, _dirV);
        float temp = Vector3.Dot(crossProduct, _contactPoint + vertices[0]) / crossProduct.magnitude;
        if (temp >= 0)
        {
            leftSides.Add(new Side(new[] {0, 1, 2}, vertices, normals, tangents));
        }
        else
        {
            rightSides.Add(new Side(new[] {0, 1, 2}, vertices, normals, tangents));
        }
    }

    private void DivideTriangle(int startTriangleIndex, out Side leftSide, out Side rightSide)
    {
        List<Vector3> vertices = GetTriangle(startTriangleIndex).ConvertAll(x => _mesh.vertices[_mesh.triangles[x]]),
            leftProps = new List<Vector3>(),
            rightProps = new List<Vector3>();
        List<Vector4> leftTangents = new List<Vector4>(),
            rightTangents = new List<Vector4>();
        Vector3 crossProduct = Vector3.Cross(_dirU, _dirV);

        for (int i = 0; i < 3; i++)
        {
            float temp = Vector3.Dot(crossProduct, vertices[i] - _contactPoint);
            if (temp >= 0)
            {
                leftProps.Add(vertices[i]);
                leftProps.Add(_mesh.normals[_mesh.triangles[startTriangleIndex + i]]);
                leftTangents.Add(_mesh.tangents[_mesh.triangles[startTriangleIndex + i]]);
            }
            else
            {
                rightProps.Add(vertices[i]);
                rightProps.Add(_mesh.normals[_mesh.triangles[startTriangleIndex + i]]);
                rightTangents.Add(_mesh.tangents[_mesh.triangles[startTriangleIndex + i]]);
            }
        }

        leftSide = new Side(new int[leftProps.Count],
            leftProps.Where((value, index) => index % 2 == 0).ToArray(),
            leftProps.Where((value, index) => index % 2 == 1).ToArray(),
            leftTangents.ToArray());

        rightSide = new Side(new int[leftProps.Count],
            rightProps.Where((value, index) => index % 2 == 0).ToArray(),
            rightProps.Where((value, index) => index % 2 == 1).ToArray(),
            rightTangents.ToArray());
    }

    private void AddNewVerticesProperties(ref Side leftSide, ref Side rightSide)
    {
        Vector3[] leftVertices = leftSide.GetVertices(),
            rightVertices = rightSide.GetVertices(),
            leftNormals = leftSide.GetNormals(),
            rightNormals = rightSide.GetNormals();
        Vector3[] newVertices = new Vector3[2];
        Vector3[] newNormals = new Vector3[2];
        Vector4[] newTangents = new Vector4[2];

        for (int i = 0; i < leftVertices.Length; i++)
        {
            for (int j = 0; j < rightVertices.Length; j++)
            {
                newVertices[i + j] = ComputeIntersectionPoint(leftVertices[i], rightVertices[j]);
                newNormals[i + j] = ComputeNormal(leftNormals[i], rightNormals[j]);
                newTangents[i + j] = leftSide.GetTangents()[0];
            }
        }

        checkedVectors.AddRange(newVertices);

        leftSide = new Side(new int[leftVertices.Length + 2],
            leftVertices.Concat(newVertices).ToArray(),
            leftNormals.Concat(newNormals).ToArray(),
            leftSide.GetTangents().Concat(newTangents).ToArray());

        rightSide = new Side(new int[rightVertices.Length + 2],
            rightVertices.Concat(newVertices).ToArray(),
            rightNormals.Concat(newNormals).ToArray(),
            rightSide.GetTangents().Concat(newTangents).ToArray());
    }

    private void CreateNewTriangles(ref Side side, Vector3 normal)
    {
        Vector3[] vertices = side.GetVertices(),
            normals = side.GetNormals();
        Vector4[] tangents = side.GetTangents();
        int[] triangles = new int[(vertices.Length - 2) * 3];
        Vector3 mainLine = vertices[1] - vertices[0];
        List<(float, Vector3, Vector3, Vector4)> signedAngles = new List<(float, Vector3, Vector3, Vector4)>();
        signedAngles.Add((0, vertices[0], normals[0], tangents[0]));
        signedAngles.Add((0, vertices[1], normals[1], tangents[1]));

        for (int i = 2; i < vertices.Length; i++)
        {
            signedAngles.Add((Vector3.SignedAngle(mainLine, vertices[i] - vertices[0], normal), vertices[i], normals[i],
                tangents[i]));
        }

        signedAngles.Sort( 1, signedAngles.Count - 1,
            Comparer<(float, Vector3, Vector3, Vector4)>.Create((p1, p2) => p1.Item1.CompareTo(p2.Item1)));

        for (int i = 0; i < signedAngles.Count - 2; i++)
        {
            triangles[3 * i] = 0;
            triangles[3 * i + 1] = i + 1;
            triangles[3 * i + 2] = i + 2;
        }

        side = new Side(triangles,
            signedAngles.ConvertAll(x => x.Item2).ToArray(),
            signedAngles.ConvertAll(x => x.Item3).ToArray(),
            signedAngles.ConvertAll(x => x.Item4).ToArray());
    }

    private void MergeCoplanarFaces()
    {
        
    }

    private Vector3 GetNormal(Side side, int triangleIndex)
    {
        Vector3[] vertices = side.GetVertices();
        Vector3 normal = Vector3.Cross(vertices[^1] - vertices[0], vertices[^2] - vertices[0]);
        if (Vector3.Dot(normal, side.GetNormals()[0]) > 0)
        {
            return normal;
        }

        return -1 * normal;
    }

    private Vector3 ComputeNormal(Vector3 firstPoint, Vector3 secondPoint)
    {
        Vector3 temp = (firstPoint + secondPoint).normalized;
        Vector3 cross = Vector3.Cross(_dirU, _dirV).normalized;
        Vector3 normal = temp - Vector3.Dot(temp, cross) * cross;
        return normal.normalized;
    }
    
    private void AddLastSide(List<Vector3> vertices, ref List<Side> sides, ref List<Side> oppositeSides)
    {
        Vector3 nm = Vector3.zero;
        for (int i = 2; i < vertices.Count; i++)
        {
            nm = Vector3.Cross(vertices[0] - vertices[1], vertices[0] - vertices[i]);
            if (nm != Vector3.zero) break;
        }

        Vector3[] sideVertices = sides[0].GetVertices(), normals = new Vector3[vertices.Count];
        Vector4[] tangents = new Vector4[vertices.Count];
        float[] angles = new float[2];
        
        for (int i = 0; i < angles.Length; i++)
        {
            angles[i] = Vector3.Angle(nm, sideVertices[i] - vertices[0]);
        }

        if (angles[0] < 90 || angles[1] < 90)
        {
            nm *= -1;
        }

        int[] triangles = CreateTriangles(vertices, nm);
        
        Array.Fill(normals, nm);
        Vector3 temp = vertices[0] - vertices[1];
        Array.Fill(tangents, new Vector4(temp.x, temp.y, temp.z, -1f));
        sides.Add(new Side((int[])triangles.Clone(), vertices.ToArray(), (Vector3[])normals.Clone(), tangents));

        for (int i = 0; i < triangles.Length;i +=3)
        {
            (triangles[i + 1], triangles[i + 2]) = (triangles[i + 2], triangles[i + 1]);
        }
        
        Array.Fill(normals, -1 * nm);
        oppositeSides.Add(new Side(triangles, vertices.ToArray(), (Vector3[])normals.Clone(), tangents));
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
        
        maps.Sort(/*1, maps.Count - 1,*/ Comparer<(Vector3, int, float)>.Create((p1, p2) => p1.Item3.CompareTo(p2.Item3)));

        for (int i = 0; i < maps.Count - 2; i++)
        {
            triangles[3 * i] = maps[0].Item2;
            triangles[3 * i + 1] = maps[i + 1].Item2;
            triangles[3 * i + 2] = maps[i + 2].Item2;
        }

        return triangles;
    }
    
    private Mesh CreateNewMesh(List<Side> sides)
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

            Vector3[] nm = sides[i].GetNormals();
            Vector4[] tg = sides[i].GetTangents();
            
            for (int j = 0; j < tempV.Length; j++)
            {
                _normals[vCounter + j] = nm[j];
                _tangents[vCounter + j] = tg[j];
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
            matrix[i, 2] = -1 * guide[i];
        }

        matrix = FindInverseMatrix(matrix);

        Vector3 coefs = MultiplyMatrix3x3Vector3x1(matrix, secondVertex - _contactPoint);
        return secondVertex + guide * coefs[2];
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

    //Tests Sector
    
    private void MagnitudeTest(List<Vector3> vertices)
    {
        float length = 0.5f, maxError = 0.01f;
        int counter = 0;
        
        for (int i = 0; i < vertices.Count; i++)
        {
            if (vertices[i].magnitude <= length * (1 + maxError) &&
                vertices[i].magnitude >= length * (1 - maxError))
            {
                counter++;
            }
        }
        
        Debug.Log("Vertices in range " + counter + "/" + vertices.Count);
    }

    private void CoplanarityTest(List<Vector3> vertices)
    {
        int counter = 0;
        Vector3 normal = Vector3.forward;
        
        for (int i = 1; i < vertices.Count; i++)
        {
            float temp = Vector3.Dot(normal, vertices[0] - vertices[i]);
            if (temp is <= 0.01f and >= -0.01f)
            {
                counter++;
            }
        }

        Debug.Log("Coplanar vertices" + (counter + 1) + "/" + vertices.Count);
    }

    private void FullRotationTest(List<Vector3> vertices)
    {
        float angle = 0;
        Vector3 normal = Vector3.forward;
        List<(Vector3, float)> vertexAngelMap = new List<(Vector3, float)>();

        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] -=  _contactPoint;
        }
        
        for (int i = 0; i < vertices.Count; i++)
        {
            vertexAngelMap.Add((vertices[i], Vector3.SignedAngle(vertices[0], vertices[i], normal)));
        }
        
        vertexAngelMap.Sort(Comparer<(Vector3, float)>.Create((p1, p2) => p1.Item2.CompareTo(p2.Item2)));

        for (int i = 0; i < vertexAngelMap.Count-1; i++)
        {
            angle += Vector3.SignedAngle(vertexAngelMap[i].Item1, vertexAngelMap[i+1].Item1, normal);
            Debug.Log(angle);
        }
        
        angle += Vector3.SignedAngle(vertexAngelMap[^1].Item1, vertexAngelMap[0].Item1, normal);
        
        Debug.Log("Angle around axis is " + angle + " degrees");
    }
    
    private void LogData(List<Side> sides, Mesh mesh)
    {
        int vertexCounter = 0;
        for (int i = 0; i < sides.Count; i++)
        {
            Vector3[] vertices = sides[i].GetVertices();
            for (int j = 0; j < vertices.Length; j++)
            {
                File.AppendAllText("C:/Users/pandr/UnityProjects/PhisicsTest/Assets/Test txt/Sides.txt", $"Side: {i} — Index: {vertexCounter} — Vertex: {vertices[j]}\n");
                vertexCounter++;
            }
            File.AppendAllText("C:/Users/pandr/UnityProjects/PhisicsTest/Assets/Test txt/Sides.txt", "\n");
        }

        for (int i = 0; i < mesh.triangles.Length; i++)
        {
            File.AppendAllText("C:/Users/pandr/UnityProjects/PhisicsTest/Assets/Test txt/Triangles.txt", $"Triangle[{i}] — {mesh.triangles[i]}\n");
        }
    }
    
    public static Mesh CreateMeshFromSide(Side side)
    {
        Mesh mesh = new Mesh();

        mesh.vertices = side.GetVertices();
        mesh.triangles = side.GetTriangles();
        mesh.normals = side.GetNormals();
        mesh.tangents = side.GetTangents();

        return mesh;
    }
}

//Auxiliary classes
public class Vector3Comparer : IEqualityComparer<Vector3>
{
    private readonly float epsilon;

    public Vector3Comparer(float epsilon = 0.0001f)
    {
        this.epsilon = epsilon;
    }

    public bool Equals(Vector3 v1, Vector3 v2)
    {
        return Vector3.SqrMagnitude(v1 - v2) < epsilon * epsilon;
    }
    
    public int GetHashCode(Vector3 v)
    {

        int x = Mathf.RoundToInt(v.x / epsilon);
        int y = Mathf.RoundToInt(v.y / epsilon);
        int z = Mathf.RoundToInt(v.z / epsilon);
        
        int hash = 17;
        hash = hash * 31 + x;
        hash = hash * 31 + y;
        hash = hash * 31 + z;

        return hash;
    }
}