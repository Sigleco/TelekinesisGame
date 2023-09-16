using UnityEngine;

public class Plate : Projectile
{
    //Позволяет резать обьекты. Для этого есть 2 ориентации
    //Третья ориентация позволяет оказыватьь дробящее действие на обьекты и защищаться от проджектайлов
    //Летает строго прямо

    private const float Speed = 4f;

    private Collider _col;
    private Vector3[] rotations = {new(0,-90,-90), new(0,90,0), new(0,0,90)};
    private int _rotIndex;
    private bool _underControl;
    
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
        if (other.CompareTag("Obstacle"))
        {
            Stuck();
        }

        if (other.CompareTag("Enemy"))
        {
            //Separate pieces
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _col.enabled = true;
    }

    protected override Trajectory GetTrajectory(Vector3 vX, float distance)
    {
        Velocity = vX.normalized * Speed;
        return new Trajectory(Velocity, VertDirection, distance);
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
