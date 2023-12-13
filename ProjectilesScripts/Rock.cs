using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//File contains rock and rock tracker classes!!!!!!!!!!

public class Rock : Projectile
{
    //Летает по параболической трактории
    //При контакте придает импульс обьекту, сам же импульс теряет полностью
    private const float Speed = 50f;
    private static RocksHitTracker _tracker = new RocksHitTracker();
    private LayerMask _rockMask;

    //private Collider _col;

    public override Vector3 VertDirection
    {
        get => Vector3.up;
    }

    private void Start()
    {
        _rockMask = LayerMask.GetMask("Rock");
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Enemy")) 
        { 
            //SwapVelocities<AIAgent>(other.gameObject);
        }
        else if (other.gameObject.CompareTag("Projectile")) 
        {
            if (LayerMask.GetMask(LayerMask.LayerToName(other.gameObject.layer)) == _rockMask)
            {
                _tracker.RegistreHit(gameObject, Velocity);
            }
            else if (LayerMask.GetMask(LayerMask.LayerToName(other.gameObject.layer)) != _rockMask && Velocity.magnitude != 0)
            {
                SwapVelocities<Projectile>(other.gameObject);
            }
        }
        else if (other.gameObject.CompareTag("Obstacle"))
        {
            StopMoving();
        }
    }

    private void SwapVelocities<T>(GameObject gb) where T:IMovable
    {
        T temp = gb.GetComponent<T>();
        Vector3 velocity = temp.GetVelocity();
        temp.StartMoving(-Velocity, temp.VertDirection, 0);
        
        if (velocity.magnitude != 0)
        {
            StartMoving(velocity, VertDirection, 0);
        }
        else
        {
            StopMoving();
        }
    }

    protected override Trajectory GetTrajectory(Vector3 vX, Vector3 vY, float distance)
    {
        Velocity = vX.normalized * Speed;
        return new Trajectory(Velocity, VertDirection.magnitude * vY, distance);
    }
}

public sealed class RocksHitTracker
{
    //class written with bias that in a moment can collide only two rocks
    //two rocks with zero velocity shouldn't collide
    
    private List<GameObject> _objs = new List<GameObject>();
    private List<Vector3> _velocities = new List<Vector3>();

    public void RegistreHit(GameObject obj, Vector3 vel)
    {
        _objs.Add(obj);
        _velocities.Add(vel);
        
        if (_objs.Count >= 2)
        {
            if (_velocities[1].magnitude != 0 || _velocities[0].magnitude != 0)
            {
                _objs[0].GetComponent<Projectile>().StartMoving(-_velocities[1], Vector3.up,  0);
                _objs[1].GetComponent<Projectile>().StartMoving(-_velocities[0], Vector3.up,  0);
            }
            
            _objs.Clear();
            _velocities.Clear();
        }
    }
}
