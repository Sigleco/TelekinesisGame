using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    List<Triangle>[] BuildTriangles(List<ITrianglePoint>[] dividedPoints, List<ITrianglePoint> points);
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

public interface IMatrixMathProvider
{
    float[,] FindInverseMatrix(float[,] matrix);
    Vector3 MultiplyMatrix3X3Vector3X1(float[,] matrix, Vector3 vec);
}

public interface IIntersectionPointProvider
{
    Vector3 ComputeIntersectionPoint(Vector3 firstVertex, Vector3 secondVertex, ContactData contactData);
}

