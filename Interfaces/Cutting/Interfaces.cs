using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITrianglePoint
{
    void SetProperties(Vector3 vertex, Vector3 normal, Vector4 tangent);
    Vector3 GetVertex();
    Vector3 GetNormal();
    Vector4 GetTangent();
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

public interface ITriangulator
{
    public int[] Triangulate(List<Vector2> pointLists);
}

public interface ITriangleDivider
{
    List<ITrianglePoint>[] DivideTrianglePoints(Triangle triangle);
}

public interface ITriangleBuilder
{
    List<Triangle>[] BuildTriangles(List<ITrianglePoint>[] dividedPoints, List<ITrianglePoint> points, int[] originalRotation);
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

