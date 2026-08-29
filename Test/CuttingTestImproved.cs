using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DelaunatorSharp;
using DelaunatorSharp.Unity.Extensions;

public class CuttingTestImproved : MonoBehaviour
{
    private TriangleDivider divider;
    private TriangleBuilder builder;
    private readonly ContactData _contactData = new ContactData();
    private Mesh _mesh;
    private Mesh leftMesh, rightMesh;
    private MeshFilter _filter;
    
    private void Start()
    {
        _filter = gameObject.GetComponent<MeshFilter>();
        _mesh = _filter.mesh;
        _contactData.DirV = Vector3.forward;
        _contactData.DirU = Vector3.up;
        _contactData.ContactPoint = new Vector3(0, 0.5f, 0);
        divider = new TriangleDivider(_contactData);
        builder = new TriangleBuilder(new TriangulatorDelauney(), new IntersectionPointProvider(new MatrixMathProvider()), _contactData);
        Compute();
    }

    private void Compute()
    {
        List<ITrianglePoint>[] newPoints = divider.DivideTriangle(Triangle.Create(
            new[] {new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f)},
            new[] {Vector3.up, Vector3.up, Vector3.up},
            new[] {new Vector4(1, 0, 0, 1), new Vector4(1, 0, 0), new Vector4(1, 0, 0)}, new[] {0, 1, 2}));

        List<ITrianglePoint> points = new List<ITrianglePoint>(3);
        var p0 = new TrianglePoint();
        p0.SetProperties(new Vector3(0.5f, 0.5f, 0.5f), Vector3.up, new Vector4(1, 0, 0, 1));
        points.Add(p0);

        var p1 = new TrianglePoint();
        p1.SetProperties(new Vector3(0.5f, 0.5f, -0.5f), Vector3.up, new Vector4(1, 0, 0));
        points.Add(p1);

        var p2 = new TrianglePoint();
        p2.SetProperties(new Vector3(-0.5f, 0.5f, -0.5f), Vector3.up, new Vector4(1, 0, 0));
        points.Add(p2);

        //List<Triangle> newTriangles = builder.BuildTriangles(newPoints);
        List<Triangle>[] newTriangles = builder.BuildTrianglesTest(newPoints, points, new[]{2, 0, 1});

        //leftMesh = CreateNewMesh(newTriangles[0]);
        //GameObject obj = new GameObject {transform = {position = Vector3.right * 1f}};
        //MeshFilter filter = obj.AddComponent<MeshFilter>();
        //filter.mesh = leftMesh;
        //MeshRenderer rend = obj.AddComponent<MeshRenderer>();
        //rend.material = _filter.gameObject.GetComponent<MeshRenderer>().material;
        //obj.AddComponent<BoxCollider>();
        //
        //rightMesh = CreateNewMesh(newTriangles[1]);
        //GameObject obj1 = new GameObject {transform = {position = Vector3.right * 2f}};
        //MeshFilter filter1 = obj1.AddComponent<MeshFilter>();
        //filter1.mesh = rightMesh;
        //MeshRenderer rend1 = obj1.AddComponent<MeshRenderer>();
        //rend1.material = _filter.gameObject.GetComponent<MeshRenderer>().material;
        //obj1.AddComponent<BoxCollider>();
        
        Debug.Log("End");
    }
    
    private Mesh CreateNewMesh(List<Triangle> triangles)
    {
        Mesh newMesh = new Mesh();
        int vCounter = triangles.Count * 3;
        int trCounter = triangles.Count * 3;

        Vector3[] _vertices = new Vector3[vCounter];
        Vector3[] _normals = new Vector3[vCounter];
        Vector4[] _tangents = new Vector4[vCounter];
        int[] _triangles = new int[trCounter];

        vCounter = 0;
        trCounter = 0;
        for (int i = 0; i < triangles.Count; i++)
        {
            int lenght = triangles[i].Vertices.Length;
            Array.Copy(triangles[i].Vertices, 0, _vertices, vCounter, lenght);

            for (int j = 0; j < lenght; j++)
            {
                _normals[vCounter + j] = triangles[i].Normals[j];
                _tangents[vCounter + j] = triangles[i].Tangents[j];
            }
            
            for (int j = 0; j < lenght - 2; j++)
            {
                _triangles[trCounter + 3 * j] = triangles[i].Triangles[3 * j] + vCounter;
                _triangles[trCounter + 3 * j + 1] = triangles[i].Triangles[3 * j + 1] + vCounter;
                _triangles[trCounter + 3 * j + 2] = triangles[i].Triangles[3 * j + 2] + vCounter;
            }
            
            vCounter += lenght;
            trCounter += triangles[i].Triangles.Length;
        }

        newMesh.vertices = _vertices;
        newMesh.normals = _normals;
        newMesh.tangents = _tangents;
        newMesh.triangles = _triangles;
        
        return newMesh;
    }
}

public class TrianglePoint : ITrianglePoint
{
    private Vector3 _vertex, _normal;
    private Vector4 _tangent;

    public Vector3 Vertex
    {
        get => _vertex;
        set => _vertex = value;
    }
    
    public Vector3 Normal
    {
        get => _normal;
        set => _normal = value;
    }

    public Vector4 Tangent
    {
        get => _tangent;
        set => _tangent = value;
    }

    public void SetProperties(Vector3 vertex, Vector3 normal, Vector4 tangent)
    {
        _vertex = vertex != Vector3.zero ? vertex : throw new ArgumentNullException(nameof(vertex));
        _normal = normal != Vector3.zero ? normal : throw new ArgumentNullException(nameof(normal));
        _tangent = tangent != Vector4.zero ? tangent : throw new ArgumentNullException(nameof(tangent));
    }

    public Vector3 GetVertex()
    {
        return _vertex;
    }
    
    public Vector3 GetNormal()
    {
        return _normal;
    }
    
    public Vector4 GetTangent()
    {
        return _tangent;
    }
}

public class TriangleDivider : ITriangleDivider
{
    private readonly Vector3 _contactPoint;
    private readonly Vector3 _crossProduct;
    
    public TriangleDivider(ContactData contectData)
    {
        _crossProduct = Vector3.Cross(contectData.DirU, contectData.DirV);
        _contactPoint = contectData.ContactPoint;
    }

    public List<ITrianglePoint>[] DivideTriangle(Triangle triangle)
    {
        //Проверить, будет ли это работать
        List<ITrianglePoint>[] points = new List<ITrianglePoint>[2];
        points[0] = new List<ITrianglePoint>();
        points[1] = new List<ITrianglePoint>();

        //Зачем мне нужна фабрика, если мне нужен пустой объект?
        TrianglePoint newPoint;

        Vector3[] vertices = triangle.Vertices;
        Vector3[] normals = triangle.Normals;
        Vector4[] tangents = triangle.Tangents;

        for (int i = 0; i < 3; i++)
        {
            newPoint = new TrianglePoint();
            float temp = Vector3.Dot(_crossProduct, vertices[i] - _contactPoint);
            newPoint.SetProperties(vertices[i], normals[i], tangents[i]);
            if (temp >= 0)
            {
                points[0].Add(newPoint);
            }
            else
            {
                points[1].Add(newPoint);
            }
        }

        return points;
    }
}

public class TriangleBuilder : ITriangleBuilder
{
    private readonly Vector3 _dirU, _dirV, _contactPoint;
    private readonly ITriangulator _triangulator;
    private readonly IIntersectionPointProvider _intersectionPointProvider;
    private readonly ContactData _contactData;

    public TriangleBuilder(ITriangulator triangulator, IIntersectionPointProvider intersectionPointProvider, ContactData contactData)
    {
        _intersectionPointProvider = intersectionPointProvider ?? throw new ArgumentNullException(nameof(intersectionPointProvider));
        _triangulator = triangulator ?? throw new ArgumentNullException(nameof(triangulator));
        _dirU = contactData.DirU;
        _dirV = contactData.DirV;
        _contactPoint = contactData.ContactPoint;
        _contactData = contactData;
    }

    public List<Triangle> BuildTriangles(List<ITrianglePoint>[] dividedPoints)
    {
        List<Triangle> triangles = new List<Triangle>();
        Vector3 normal = Vector3.Cross(_dirU, _dirV);
        AddNewTrianglePoints(ref dividedPoints, _intersectionPointProvider);

        /*for (int i = 0; i < dividedPoints.Count(); i++)
        {
            triangles.Add(TransformToTriangle(
                _triangulator.Triangulate(
                    GetCoordinatesInPlaneBasis(
                        ProjectOnPlane(dividedPoints[i], normal))),
                dividedPoints[i]));
        }*/
        
        //Тестовый цикл для отработки гипотезы без проекции сторон на плоскость
        for (int i = 0; i < dividedPoints.Count(); i++)
        {
            List<Vector3> temp = dividedPoints[i].Select(p => p.GetVertex()).ToList();
            triangles.Add(TransformToTriangle(
                _triangulator.Triangulate(
                    GetCoordinatesInPlaneBasisTest(temp)),
                dividedPoints[i]));
        }
        
        return triangles;
    }
    
    public List<Triangle>[] BuildTrianglesTest(List<ITrianglePoint>[] dividedPoints, List<ITrianglePoint> points, int[] oldRotation)
    {
        List<Triangle> triangles = new List<Triangle>();
        Vector3 normal = Vector3.Cross(_dirU, _dirV);
        points.AddRange(FindNewTrianglePoints(dividedPoints, _intersectionPointProvider));
        
        
        List<Vector3> temp = points.Select(p => p.GetVertex()).ToList();
        return TransformToTriangleList(
            _triangulator.Triangulate(
                GetCoordinatesInPlaneBasisTest(temp)),
            points, dividedPoints);
    }

    private Triangle TransformToTriangle(int[] rotation, List<ITrianglePoint> rotatedPoints)
    {
        List<Vector3> vertices = new List<Vector3>(), normals = new List<Vector3>();
        List<Vector4> tangents = new List<Vector4>();
        
        foreach (var point in rotatedPoints)
        {
            vertices.Add(point.GetVertex());
            normals.Add(point.GetNormal());
            tangents.Add(point.GetTangent());
        }
        
        return Triangle.Create(vertices.ToArray(), normals.ToArray(), tangents.ToArray(), rotation);
    }
    
    private Triangle TransformToTrianglesTest(int[] rotation, List<ITrianglePoint> rotatedPoints, int[] oldRotation)
    {
        Vector3[] vertices = new Vector3[3], normals = new Vector3[3];
        Vector4[] tangents = new Vector4[3];

        for (int i = 0; i < 3; i++)
        {
            vertices[oldRotation[i]] = rotatedPoints[i].GetVertex();
            normals[oldRotation[i]] = rotatedPoints[i].GetNormal();
            tangents[oldRotation[i]] = rotatedPoints[i].GetTangent();
        }

        rotation = AdjustRotation(rotation, oldRotation);
        
        return Triangle.Create(vertices.ToArray(), normals.ToArray(), tangents.ToArray(), rotation);
    }

    private List<Triangle>[] TransformToTriangleList(int[] rotation, List<ITrianglePoint> rotatedPoints,
        List<ITrianglePoint>[] dividedPoints)
    {
        List<Triangle>[] triangles = new List<Triangle>[2];
        triangles[0] = new List<Triangle>();
        triangles[1] = new List<Triangle>();
        List<int>[] rotations = new List<int>[2];
        rotations[0] = new List<int>();
        rotations[1] = new List<int>();
        
        for (int i = 0; i < 3; i++)
        {
            for (int k = 0; k < 3; k++)
            {
                if (rotation[3 * i + k] < 3)
                {
                    if (dividedPoints[0].Contains(rotatedPoints[rotation[3 * i + k]], TrianglePointComparer.Instance))
                    {
                        rotations[0].Add(rotation[3 * i]);
                        rotations[0].Add(rotation[3 * i + 1]);
                        rotations[0].Add(rotation[3 * i + 2]);
                    }
                    else if (dividedPoints[1].Contains(rotatedPoints[rotation[3 * i + k]], TrianglePointComparer.Instance))
                    {
                        rotations[1].Add(rotation[3 * i]);
                        rotations[1].Add(rotation[3 * i + 1]);
                        rotations[1].Add(rotation[3 * i + 2]);
                    }

                    break;
                }
            }
        }

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < rotations[i].Count; j += 3)
            {
                int[] rot = rotations[i].Skip(j).Take(3).ToArray();
                if (rot[0] >= rot[1] && rot[0] >= rot[2])
                {
                    if (rot[1] >= rot[2]) { rot[1] = 1; rot[2] = 0; }
                    else { rot[1] = 0; rot[2] = 1; }
                    rot[0] = 2;
                }
                else if (rot[1] >= rot[0] && rot[1] >= rot[2])
                {
                    if (rot[0] >= rot[2]) { rot[0] = 1; rot[2] = 0; }
                    else { rot[0] = 0; rot[2] = 1; }
                    rot[1] = 2;
                }
                else
                {
                    if (rot[0] >= rot[1]) { rot[0] = 1; rot[1] = 0; }
                    else { rot[0] = 0; rot[1] = 1; }
                    rot[2] = 2;
                }

                Vector3[] verts = new Vector3[3];
                Vector3[] normals = new Vector3[3];
                Vector4[] tangents = new Vector4[3];
                for (int k = 0; k < 3; k++)
                {
                    int idx = rotations[i][j + k];
                    verts[rot[k]] = rotatedPoints[idx].GetVertex();
                    normals[rot[k]] = rotatedPoints[idx].GetNormal();
                    tangents[rot[k]] = rotatedPoints[idx].GetTangent();
                }

                triangles[i].Add(Triangle.Create(
                    verts, normals, tangents, rot));
            }
        }
        
        return triangles;
    }
    
    public class TrianglePointComparer : IEqualityComparer<ITrianglePoint>
    {
        public static readonly TrianglePointComparer Instance = new();

        public bool Equals(ITrianglePoint a, ITrianglePoint b)
        {
            if (a is null || b is null) return a is null && b is null;
            return a.GetVertex() == b.GetVertex()
                   && a.GetNormal() == b.GetNormal()
                   && a.GetTangent() == b.GetTangent();
        }

        public int GetHashCode(ITrianglePoint p) =>
            HashCode.Combine(p.GetVertex(), p.GetNormal(), p.GetTangent());
    }

    private int[] AdjustRotation(int[] newRotation, int[] oldRotation)
    {
        for (int i = 0; i < newRotation.Length; i++)
        {
            if (newRotation[i] < 3)
            {
                newRotation[i] = oldRotation[newRotation[i]];
            }
        }

        return newRotation;
    }

    private List<Vector2> GetCoordinatesInPlaneBasis(List<Vector3> projections)
    {
        List<Vector2> newVertices = new List<Vector2>();
        foreach (var projection in projections)
        {
            newVertices.Add(FindNewCoords(projection, _contactPoint, _dirU, _dirV));
        }

        return newVertices;
    }
    
    private List<Vector2> GetCoordinatesInPlaneBasisTest(List<Vector3> projections)
    {
        Vector2[] newVertices = new Vector2[projections.Count];
        
        for (int i = 0; i < projections.Count - 2; i++)
        {
            if (Vector3.Angle(projections[(i+1) % 3] - projections[i], projections[3] - projections[i]) < 0.001f &&
                Vector3.Angle(projections[(i+2) % 3] - projections[i], projections[4] - projections[i]) < 0.001f)
            {
                newVertices[i] = new Vector2(0f, 0f);
                newVertices[(i+1) % 3] = new Vector2(1f, 0f);
                newVertices[(i+2) % 3] = new Vector2(0f, 1f);
                newVertices[3] = new Vector2((projections[3] - projections[i]).magnitude/(projections[(i+1) % 3] - projections[i]).magnitude, 0f);
                newVertices[4] = new Vector2(0f, (projections[4] - projections[i]).magnitude/(projections[(i+2) % 3] - projections[i]).magnitude);
                break;
            }
            
            if(Vector3.Angle(projections[(i+1) % 3] - projections[i], projections[4] - projections[i]) < 0.001f &&
                    Vector3.Angle(projections[(i+2) % 3] - projections[i], projections[3] - projections[i]) < 0.001f) 
            {
                newVertices[i] = new Vector2(0f, 0f);
                newVertices[(i+1) % 3] = new Vector2(1f, 0f);
                newVertices[(i+2) % 3] = new Vector2(0f, 1f);
                newVertices[3] = new Vector2((projections[4] - projections[i]).magnitude/(projections[(i+1) % 3] - projections[i]).magnitude, 0f);
                newVertices[4] = new Vector2(0f, (projections[3] - projections[i]).magnitude/(projections[(i+2) % 3] - projections[i]).magnitude);
                break;
            }
        }

        return newVertices.ToList();
    }

    private Vector2 FindNewCoords(Vector3 projection, Vector3 contactPoint, Vector3 dirU, Vector3 dirV)
    {
        Vector3 vertex = projection - contactPoint;
        float[][] matrix = new float[3][];

        matrix[0] = new[] {vertex.x, dirU.x, dirV.x};
        matrix[1] = new[] {vertex.y, dirU.y, dirV.y};

        float multiplicator = matrix[1][0] / matrix[0][0];
        matrix[2] = new[]
        {
            matrix[1][0] - multiplicator*matrix[0][0],
            matrix[1][1] - multiplicator*matrix[0][1],
            matrix[1][2] - multiplicator*matrix[0][2]
        };

        float yCoef = matrix[2][2] / matrix[2][1];
        float xCoef = (matrix[0][2] - matrix[0][1]) * yCoef / matrix[0][0];
        return new Vector2(xCoef, yCoef);
    }

    private List<Vector3> ProjectOnPlane(List<ITrianglePoint> trianglePoints, Vector3 normal)
    {
        List<Vector3> projections = new List<Vector3>();
        foreach (var point in trianglePoints)
        {
            projections.Add(point.GetVertex() - (float)FindNormalCoef(point, normal) * normal);
        }

        return projections;
    }

    private double FindNormalCoef(ITrianglePoint point, Vector3 normal)
    {
        return Vector3.Dot(normal, (point.GetVertex() - _contactPoint)) / Math.Pow(normal.magnitude, 2);
    }

    private void AddNewTrianglePoints(ref List<ITrianglePoint>[] dividedPoints, IIntersectionPointProvider intersectionPointProvider)
    {
        if (dividedPoints[0].Count > 0 && dividedPoints[1].Count > 0)
        {
            TrianglePoint[] newVertices = new TrianglePoint[2];
            
            for (int i = 0; i < dividedPoints[0].Count; i++)
            {
                for (int j = 0; j < dividedPoints[1].Count; j++)
                {
                    newVertices[i + j] = new TrianglePoint();
                    newVertices[i + j].SetProperties(intersectionPointProvider.ComputeIntersectionPoint(dividedPoints[0][i].GetVertex(), dividedPoints[1][j].GetVertex(), _contactData),
                        (dividedPoints[0][i].GetNormal() + dividedPoints[1][j].GetNormal()).normalized,
                        (dividedPoints[0][i].GetTangent() + dividedPoints[1][j].GetTangent()).normalized);
                }
            }
        
            dividedPoints[0].AddRange(newVertices);
            dividedPoints[1].AddRange(newVertices);
        }
    }
    
    private List<ITrianglePoint> FindNewTrianglePoints(List<ITrianglePoint>[] dividedPoints, IIntersectionPointProvider intersectionPointProvider)
    {
        if (dividedPoints[0].Count > 0 && dividedPoints[1].Count > 0)
        {
            List<ITrianglePoint> list = new List<ITrianglePoint>(); 
            TrianglePoint[] newVertices = new TrianglePoint[2];
            
            for (int i = 0; i < dividedPoints[0].Count; i++)
            {
                for (int j = 0; j < dividedPoints[1].Count; j++)
                {
                    newVertices[i + j] = new TrianglePoint();
                    newVertices[i + j].SetProperties(intersectionPointProvider.ComputeIntersectionPoint(dividedPoints[0][i].GetVertex(), dividedPoints[1][j].GetVertex(), _contactData),
                        (dividedPoints[0][i].GetNormal() + dividedPoints[1][j].GetNormal()).normalized,
                        (dividedPoints[0][i].GetTangent() + dividedPoints[1][j].GetTangent()).normalized);
                }
            }
        
            list.AddRange(newVertices);
            return list;
        }

        return new List<ITrianglePoint>();
    }
}

public interface IIntersectionPointProvider
{
    Vector3 ComputeIntersectionPoint(Vector3 firstVertex, Vector3 secondVertex, ContactData contactData);
}

public class IntersectionPointProvider : IIntersectionPointProvider
{
    private readonly IMatrixMathProvider _matrixMathProvider;

    public IntersectionPointProvider(IMatrixMathProvider matrixMathProvider)
    {
        _matrixMathProvider = matrixMathProvider ?? throw new ArgumentNullException(nameof(matrixMathProvider));
    }

    public Vector3 ComputeIntersectionPoint(Vector3 firstVertex, Vector3 secondVertex, ContactData contactData)
    {
        float[,] matrix = new float[3,3];

        Vector3 guide = secondVertex - firstVertex;

        for (int i = 0; i < 3; i++)
        {
            matrix[i, 0] = contactData.DirU[i];
            matrix[i, 1] = contactData.DirV[i];
            matrix[i, 2] = -1 * guide[i];
        }

        matrix = _matrixMathProvider.FindInverseMatrix(matrix);

        Vector3 coefs = _matrixMathProvider.MultiplyMatrix3X3Vector3X1(matrix, secondVertex - contactData.ContactPoint);
        return secondVertex + guide * coefs[2];
    }
}

public interface IMatrixMathProvider
{
    float[,] FindInverseMatrix(float[,] matrix);
    Vector3 MultiplyMatrix3X3Vector3X1(float[,] matrix, Vector3 vec);
}

public class MatrixMathProvider : IMatrixMathProvider
{
    public float[,] FindInverseMatrix(float[,] matrix)
    {
        float[,] result = new float[3, 3];
        float[,] minor = new float[2,2];
        
        float det = ComputeDeterminant3x3(matrix);
        
        int indexA = 0, indexB = 0;
        for (int j = 0; j < 3; j++)
        {
            int a = 0, b = 0;
            for (int i = 0; i < 3; i++)
            {
                //first column
                minor[0, 0] = matrix[(indexA+1)%3, (a+1)%3];
                minor[1, 0] = matrix[(indexB+2)%3, (a+1)%3];
                //second column
                minor[0, 1] = matrix[(indexA+1)%3, (b+2)%3];
                minor[1, 1] = matrix[(indexB+2)%3, (b+2)%3];

                float pow = (float) Math.Pow(-1, i + j);
                result[i,j] = (pow/det) * ComputeDeterminant2x2(minor);

                b = a;
                a = -1;
            }

            indexB = indexA;
            indexA = -1;
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
    
    public Vector3 MultiplyMatrix3X3Vector3X1(float[,] matrix, Vector3 vec)
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

public class TriangulatorDelauney : ITriangulator
{
    public int[] Triangulate(List<Vector2> pointLists)
    {
        var delaunator = new Delaunator(pointLists.ToPoints());
        return delaunator.Triangles;
    }
}

public class Triangle
{
    private Triangle() {} // запрет прямого вызова

    public Vector3[] Vertices { get; private set; }
    public Vector3[] Normals  { get; private set; }
    public Vector4[] Tangents { get; private set; }
    
    public int[] Triangles { get; private set; }

    public static Triangle Create(Vector3[] vertices, Vector3[] normals, Vector4[] tangents, int[] triangles)
    {
        if (vertices is null || vertices.Length != 3)
            throw new ArgumentException("Triangle must have exactly 3 vertices");
        
        return new Triangle
        {
            Vertices = vertices,
            Normals  = normals,
            Tangents = tangents,
            Triangles  = triangles
        };
    }
}

public interface ITrianglePoint
{
    void SetProperties(Vector3 vertex, Vector3 normal, Vector4 tangent);
    Vector3 GetVertex();
    Vector3 GetNormal();
    Vector4 GetTangent();
}

public interface ITriangulator
{
    public int[] Triangulate(List<Vector2> pointLists);
}

public interface ITriangleDivider
{
    List<ITrianglePoint>[] DivideTriangle(Triangle triangle);
}

public interface ITriangleBuilder
{
    List<Triangle> BuildTriangles(List<ITrianglePoint>[] dividedPoints);
}

public interface ITriangleStorer
{
    void Store(Triangle triangle);
    List<Triangle>[] GetStoredValue();
}

public class ContactData
{
    public Vector3 DirV { get; set; }
    public Vector3 DirU { get; set; }
    public Vector3 ContactPoint { get; set; }
}

public interface IMeshBuilder
{
    Mesh CreateMesh(Triangle[] triangles);
}

