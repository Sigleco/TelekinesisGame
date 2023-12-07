using UnityEngine;

public class Spear : Projectile
{
    //При нормальной скорости застревает в обьектах и придает им импульс для продолжения по той же траектории с той же скоростью,
    //при скорости превышающей критический предел пробивает предметы. Пробивает все кроме статичных обьектов окружения
    //Летает только по прямой тракетории
    
    //TODO с увеличением скорости снаряд может пролетать сквозь обьекты из за реализации движения, а конкретно функции Transform.Translate()
    private const float CriticalValue = 3f, NormalSpeed = 250f, Length = 1f;
    private Collider _col;
    
    public override Vector3 VertDirection
    {
        get => Vector3.zero;
    }

    void Start()
    {
        GetCollider();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            Stuck();
        }

        if (other.CompareTag("Enemy"))
        {
            _col.enabled = false;
            if (Trajectory.Vx <= CriticalValue)
            {
                StuckInBody(other.transform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _col.enabled = true;
    }

    protected override Trajectory GetTrajectory(Vector3 vX, Vector3 vY, float distance)
    {
        Velocity = vX.normalized * NormalSpeed;
        return new Trajectory(Velocity, VertDirection.magnitude * vY, distance);
    }
    
    private void Stuck()
    {
        StopMoving();
    }

    private void StuckInBody(Transform parent)
    {
        StopMoving();
        gameObject.transform.Translate(Velocity.normalized * 0.1f * Length);
        gameObject.transform.SetParent(parent);
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
