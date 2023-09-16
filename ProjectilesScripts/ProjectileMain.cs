using System;
using UnityEngine;

public abstract class Projectile : MonoBehaviour, IMovable
{
    //Базовый класс для всех проджектайлов

    private bool _isMoving, _outOfBlow;
    private bool _onStasis, _underControl;

    protected Trajectory Trajectory;
    protected Vector3 Velocity;
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = gameObject.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (_isMoving)
        {
            transform.position += Trajectory.ComputeOffset(_outOfBlow);
        }
    }

    protected abstract Trajectory GetTrajectory(Vector3 vX, float distance);
    
    public abstract Vector3 VertDirection
    {
        get;
    }
    
    public void StartMoving(Vector3 vX, float distance)
    {
        Trajectory = GetTrajectory(vX, distance);
        _isMoving = true;
    }
    
    public void StartMovingOutOfBlow(Vector3 vX, float distance)
    {
        StartMoving(vX, distance);
        _outOfBlow = true;
    }

    public void StopMoving()
    {
        _isMoving = false;
        _outOfBlow = false;
    }
    
    public void GetUnderControl(Transform controller)
    {
        StopMoving();
        transform.position = controller.rotation * IMovable.GetDefaultSpot() + controller.position;
        SetProjectileRotation(controller); 
        transform.SetParent(controller);
        _rb.constraints = RigidbodyConstraints.FreezeAll;
        _underControl = true;
    }

    private void SetProjectileRotation(Transform controller)
    {
        Quaternion q = new Quaternion();
        q.eulerAngles = new Vector3(0, 90f, 0) + controller.rotation.eulerAngles;
        transform.rotation = q;
    }

    public void LoseControl()
    {
        transform.SetParent(null);
        _rb.constraints = RigidbodyConstraints.None;
        _underControl = false;
    }
}
