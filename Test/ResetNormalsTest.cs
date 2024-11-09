using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetNormalsTest : MonoBehaviour
{
    private Vector3 _contactPoint = new Vector3(0.5f, 0.5f, 0.2f);
    private Vector3 _dirV = Vector3.right;
    private Vector3 _dirU = Vector3.up;
    private Mesh _mesh;
    private MeshFilter _filter;
    private List<Vector3> checkedVectors = new List<Vector3>();
    private List<SideStruct> leftSides = new List<SideStruct>();
    private List<SideStruct> rightSides = new List<SideStruct>();
    private Mesh leftMesh, rightMesh;

    private void Start()
    {
        _filter = gameObject.GetComponent<MeshFilter>();
        _mesh = _filter.mesh;
        leftMesh = new Mesh();
        rightMesh = new Mesh();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ResetNormals();
        }
    }

    private void ResetNormals()
    {
        Vector3[] normals = _mesh.normals;
        Vector3[] vertices = _mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            normals[i] = vertices[i].normalized;
        }

        _mesh.normals = normals;
        _filter.mesh = _mesh;
    }
}
