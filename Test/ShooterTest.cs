using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterTest : MonoBehaviour
{
    public GameObject gb;
    
    /*void Start()
    {
       Shoot();
    }*/

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        gb = Instantiate(gb, transform.position, transform.rotation);
        gb.GetComponent<Projectile>().StartMoving(Vector3.right, Vector3.up, 10f);
    }
}
