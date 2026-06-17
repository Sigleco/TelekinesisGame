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
    
    private void Start()
    {
        _contactData.DirV = Vector3.right;
        _contactData.DirU = Vector3.up;
        _contactData.ContactPoint = new Vector3(0, 0.5f, 0);
        divider = new TriangleDivider(_contactData);
        builder = new TriangleBuilder(new TriangulatorDelauney(), new IntersectionPointProvider(new MatrixMathProvider()), _contactData);
        Compute();
    }

    private void Compute()
    {
        List<ITrianglePoint>[] newPoints = divider.DivideTriangle(Triangle.Create(new []{new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f,0.5f,-0.5f), new Vector3(-0.5f, 0.5f, -0.5f)},
            new []{Vector3.up, Vector3.up, Vector3.up}, new []{new Vector4(1, 0 , 0, 1), new Vector4(1,0,0), new Vector4(1, 0, 0)}, new []{0,1,2}));

        List<Triangle> newTriangles = builder.BuildTriangles(newPoints);
        Debug.Log("End");
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

        for (int i = 0; i < dividedPoints.Count(); i++)
        {
            triangles.Add(TransformToTriangle(
                _triangulator.Triangulate(
                    GetCoordinatesInPlaneBasis(
                        ProjectOnPlane(dividedPoints[i], normal))),
                dividedPoints[i]));
        }

        //foreach (var points in dividedPoints)
        //{
        //    triangles.Add(TransformToTriangle(
        //        _triangulator.Triangulate(
        //            GetCoordinatesInPlaneBasis(
        //                ProjectOnPlane(points, normal))),
        //        points));
        //}
        return triangles;
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

    private List<Vector2> GetCoordinatesInPlaneBasis(List<Vector3> projections)
    {
        List<Vector2> newVertices = new List<Vector2>();
        foreach (var projection in projections)
        {
            newVertices.Add(FindNewCoords(projection, _contactPoint, _dirU, _dirV));
        }

        return newVertices;
    }

    private Vector2 FindNewCoords(Vector3 projection, Vector3 contactPoint, Vector3 dirU, Vector3 dirV)
    {
        Vector3 vertex = projection - contactPoint;
        float[][] matrix = new float[3][];

        for (int i = 0; i < 3; i++)
        {
            matrix[0] = new float[] {vertex.x, dirU.x, dirV.x};
            matrix[1] = new float[] {vertex.y, dirU.y, dirV.y};
        }

        float multiplicator = matrix[1][0] / matrix[0][0];
        matrix[2] = new float[]
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

