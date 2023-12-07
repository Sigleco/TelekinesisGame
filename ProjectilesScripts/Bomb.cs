using System.Linq;
using UnityEngine;

public class Bomb : Projectile
{
    //Взрывается если в обьект данного класса попал любой из проджектайлов
    //Выталкивает из радиуса взрыва
    
    private const float ExplosionRadius = 2f, Speed = 100f;

    private Rigidbody _rb = new Rigidbody();
    private Collider _col = new Collider();
    private Collider[] _ar = new Collider[30];
    private LayerMask _antiBombMask;

    public override Vector3 VertDirection
    {
        get => Vector3.up;
    }
    
    void Start()
    {
        _antiBombMask = LayerMask.GetMask("Spear", "Rock", "Enemy", "Plate");
        _rb = gameObject.GetComponent<Rigidbody>();
        Collider[] ar = gameObject.GetComponents<Collider>();
        
        for (int i = 0; i < ar.Length; i++)
        {
            if (ar[i].isTrigger) continue;
            _col = ar[i];
            break;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        BlowUp();
    }

    private void BlowUp()
    {
        Physics.OverlapSphereNonAlloc(transform.position, ExplosionRadius, _ar, _antiBombMask);
        PushAllAround();
        Destroy(gameObject);
    }

    private void PushAllAround()
    {
        for (int i = 0; i < _ar.Length; i++)
        {
            //TODO: Is it work?
            if (CanBePushed(i))
            {
                Vector3 temp = _ar[i].transform.position - transform.position;
                MoveOutOfExplosion<Projectile>(temp, 0, _ar[i].gameObject);
            }
        }
    }

    private bool CanBePushed(int i)
    {
        return _ar[i] != null && _ar[i].gameObject && gameObject != _ar[i].gameObject;
    }

    private void MoveOutOfExplosion<T>(Vector3 vX, float distance, GameObject gb) where T:IMovable
    {
        gb.GetComponent<T>().StopMoving();
        Vector3 vY = gb.GetComponent<Projectile>().VertDirection;
        gb.GetComponent<T>().StartMovingOutOfBlow(vX.normalized * ExplosionRadius / IMovable.TimeToRunOff, vY, distance);
    }

    protected override Trajectory GetTrajectory(Vector3 vX, Vector3 vY, float distance)
    {
        Velocity = vX.normalized * Speed;
        return new Trajectory(Velocity, VertDirection.magnitude * vY, distance);
    }
}
