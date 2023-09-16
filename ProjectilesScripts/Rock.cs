using UnityEngine;
using System.Linq;

public class Rock : Projectile
{
    //Летает по параболической трактории
    //При контакте придает импульс обьекту, сам же импульс теряет полностью
    
    private const float Speed = 2f;

    //private Collider _col;
    private MonoBehaviour[] _mono = new MonoBehaviour[5];
    
    public override Vector3 VertDirection
    {
        get => Vector3.up;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (CheckIMovable(other.collider))
        {
            if (other.gameObject.CompareTag("Enemy"))
            {
                //SwapVelocities<AIAgent>(other.gameObject);
            }
            else if(other.gameObject.CompareTag("Projectile"))
            {
                SwapVelocities<Projectile>(other.gameObject);
            }
        }
    }
    
    //TODO delete this function
    private bool CheckIMovable(Collider col)
    {
        _mono = col.transform.gameObject.GetComponents<MonoBehaviour>();

        if (_mono == null) return false;
        
        for (int i = 0; i < _mono.Length; i++)
        {
            if (_mono[i].GetType().GetInterfaces().Contains(typeof(IMovable)))
            {
                _mono[0] = _mono[i]; 
                return true;
            }
        }

        return false;
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
