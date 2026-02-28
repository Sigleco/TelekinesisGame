using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelaunatorSharp;
using DelaunatorSharp.Unity.Extensions;

public class DelanuatorTest : MonoBehaviour
{
    // Start is called before the first frame update
    private Delaunator delaunator;
    private List<Vector2> points = new List<Vector2>(){new Vector2(1,2), new Vector2(3,5), 
        new Vector2(5,2), new Vector2(4,6)};
    void Start()
    {
        delaunator = new Delaunator(points.ToPoints());
        int[] triangles = delaunator.Triangles;
        Debug.Log(delaunator.Triangles.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
