using System;
using UnityEngine;

public class Plate : Projectile
{
    private const float Speed = 4f;

    private Collider _col;
    private Vector3[] rotations = {new(0,-90,-90), new(0,90,0), new(0,0,90)};
    private int _rotIndex;
    private bool _underControl;
    private Cutter _cutter = new Cutter();
    
    public override Vector3 VertDirection
    {
        get => Vector3.zero;
    }
    
    void Start()
    {
        GetCollider();
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _rotIndex = (_rotIndex+1)%3;
            Quaternion q = new Quaternion();
            q.eulerAngles = transform.rotation.eulerAngles + rotations[_rotIndex];
            transform.rotation = q;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        /*if (other.CompareTag("Obstacle"))
        {
            Stuck();
        }

        if (other.CompareTag("Enemy"))
        {
            _cutter.SetCuttingParams(other.,Velocity);
        }*/
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Stuck();
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            //TODO
            Debug.Log("Contact");
            Vector3 temp = collision.gameObject.transform.InverseTransformPoint(collision.contacts[0].point);
            _cutter.SetCuttingParams(temp,
                collision.gameObject.transform.InverseTransformDirection(Velocity), 
                collision.gameObject.transform.InverseTransformDirection(transform.up - Velocity * (Vector3.Dot(transform.up, Velocity)/ Velocity.magnitude)),
                collision.gameObject);
            _cutter.StartCutting();
            GetComponent<Collider>().enabled = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _col.enabled = true;
    }

    protected override Trajectory GetTrajectory(Vector3 vX, Vector3 vY, float distance)
    {
        Velocity = vX.normalized * Speed;
        return new Trajectory(Velocity, VertDirection.magnitude * vY, distance);
    }
    
    private void Stuck()
    {
        StopMoving();
    }

    private void GetCollider()
    {
        Collider[] ar = gameObject.GetComponents<Collider>();

        for (int i = 0; i < ar.Length; i++)
        {
            if (ar[i].isTrigger != true)
            {
                _col = ar[i];
                break;
            }
        }
    }
}
