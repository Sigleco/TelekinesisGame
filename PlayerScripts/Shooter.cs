using System;
using UnityEditor.PackageManager;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    [SerializeField] private Transform playerInputSpace = default;
    private bool _haveProjectile;
    private Projectile _projectile;
    private LayerMask _canBeControlledMask;
    
    void Start()
    {
        _canBeControlledMask = LayerMask.GetMask("Spear", "Rock", "Enemy", "Plate", "Bomb");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (_haveProjectile)
            {
                Shoot();
            }
            else
            {
                TakeControl();
            } 
        }
    }

    private void Shoot()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, playerInputSpace.forward, out hit);
        Vector3 direction = (hit.point - transform.position) + IMovable.GetDefaultSpot();
        _projectile.LoseControl();
        _projectile.StartMoving(direction.normalized, direction.magnitude);
        _haveProjectile = false;
    }

    private void TakeControl()
    {
        RaycastHit hit;
        IMovable move;

        if (Physics.Raycast(transform.position, playerInputSpace.forward, out hit, _canBeControlledMask))
        {
            if (hit.transform.gameObject.CompareTag("Enemy"))
            {
                move = hit.transform.gameObject.GetComponent<AIMain>();
            }
            else
            {
                move = hit.transform.gameObject.GetComponent<Projectile>();
            }

            _projectile = (Projectile) move;
            move.GetUnderControl(transform);
            _haveProjectile = true;
        }
    }
}
