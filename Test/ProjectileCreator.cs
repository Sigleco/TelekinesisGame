using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileCreator : MonoBehaviour
{
    //временный класс для тестов
    
    public GameObject projectile;
    private GameObject[] _projectiles = new GameObject[10];

    private void Awake()
    {
       SpawnProjectiles();
    }

    private void SpawnProjectiles()
    {
        if (projectile == null) return;
        for (int i = 0; i < _projectiles.Length; i++)
        {
            Quaternion q = new Quaternion();
            q.eulerAngles = new Vector3(0f, 36 * i, 0);
            _projectiles[i] = Instantiate(projectile, 10 * (q * Vector3.forward) + 2 * Vector3.up, q);
        }
    }
    
    /*void Start()
    {
        
    }
    
    void Update()
    {
        
    }*/
}
