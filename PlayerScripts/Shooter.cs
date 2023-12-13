using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    //Класс отвечает за то как обьект берется под контроль и как контроль над ним теряется в процессе стрельбы
    
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

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            InvokeStasis();
        }
    }

    private void Shoot()
    {
        RaycastHit hit;
        Vector3 direction;
        
        if(Physics.Raycast(transform.position, playerInputSpace.forward, out hit))
        {
            direction = (hit.point - transform.position) - transform.rotation * IMovable.GetDefaultSpot();
        }
        else
        {
            direction = 100f * playerInputSpace.forward - transform.rotation * IMovable.GetDefaultSpot();
        }
        
        _projectile.LoseControl();
        _projectile.StartMoving(direction.normalized, playerInputSpace.up, direction.magnitude);
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
                //todo extra check for spear
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

    private void InvokeStasis()
    {
        RaycastHit hit;
        IMovable move;

        if(Physics.Raycast(transform.position, playerInputSpace.forward, out hit, _canBeControlledMask))
        {
            if (hit.transform.gameObject.CompareTag("Enemy"))
            {
                //todo extra check for spear
                move = hit.transform.gameObject.GetComponent<AIMain>();
            }
            else
            {
                move = hit.transform.gameObject.GetComponent<Projectile>();
            }
            
            move.StopMoving();
        }
    }
}
