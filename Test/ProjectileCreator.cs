using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileCreator : MonoBehaviour
{
    //временный класс для тестов
    
    public GameObject spear, bomb, rock, plate;
    private GameObject[] _projectiles = new GameObject[40];

    private void Awake()
    {
       SpawnSpears();
       SpawnRocks();
       SpawnBombs();
       SpawnPlates();
    }

    private void SpawnSpears()
    {
        if (spear == null) return;
        for (int i = 0; i < 9; i++)
        {
            Quaternion q = new Quaternion();
            q.eulerAngles = new Vector3(0f, 36 * i, 0);
            _projectiles[i] = Instantiate(spear, 10 * (q * Vector3.forward) + 2 * Vector3.up, q);
        }
    }
    
    private void SpawnRocks()
    {
        if (rock == null) return;
        for (int i = 0; i < 9; i++)
        {
            Quaternion q = new Quaternion();
            q.eulerAngles = new Vector3(0f, 36 * i, 0);
            _projectiles[10+i] = Instantiate(rock, 10 * (q * Vector3.forward) + 6 * Vector3.up, q);
        }
    }
    
    private void SpawnBombs()
    {
        if (bomb == null) return;
        for (int i = 0; i < 9; i++)
        {
            Quaternion q = new Quaternion();
            q.eulerAngles = new Vector3(0f, 36 * i, 0);
            _projectiles[20+i] = Instantiate(bomb, 10 * (q * Vector3.forward) + 10 * Vector3.up, q);
        }
    }
    
    private void SpawnPlates()
    {
        if (plate == null) return;
        for (int i = 0; i < 9; i++)
        {
            Quaternion q = new Quaternion();
            q.eulerAngles = new Vector3(0f, 36 * i, 0);
            _projectiles[30+i] = Instantiate(plate, 10 * (q * Vector3.forward) + 14 * Vector3.up, q);
        }
    }
    
    /*void Start()
    {
        
    }
    
    void Update()
    {
        
    }*/
}
