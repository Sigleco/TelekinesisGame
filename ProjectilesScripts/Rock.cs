using UnityEngine;
using System.Linq;

public class Rock : Projectile
{
    //Летает по параболической трактории
    //При контакте придает импульс обьекту, сам же импульс теряет полностью
    
    private const float Speed = 100f;

    //private Collider _col;
    private MonoBehaviour[] _mono = new MonoBehaviour[5];
    
    public override Vector3 VertDirection
    {
        get => Vector3.up;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Enemy")) 
        { 
            //SwapVelocities<AIAgent>(other.gameObject);
        }
        else if (other.gameObject.CompareTag("Projectile")) 
        { 
            SwapVelocities<Projectile>(other.gameObject);
        }
        else if (other.gameObject.CompareTag("Obstacle"))
        {
            StopMoving();
        }
    }

    private void SwapVelocities<T>(GameObject gb) where T:IMovable
    {
        T temp = gb.GetComponent<T>();
        temp.StartMoving(Velocity, 0);
        StopMoving();
    }

    protected override Trajectory GetTrajectory(Vector3 vX, float distance)
    {
        Velocity = vX.normalized * Speed;
        return new Trajectory(Velocity, VertDirection, distance);
    }
}
