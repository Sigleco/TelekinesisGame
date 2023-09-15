using UnityEngine;

public abstract class AIMain : MonoBehaviour, IMovable
{
    public Vector3 VertDirection { get; }
    
    public abstract void StartMoving(Vector3 vx, float d);
    
    public abstract void StartMovingOutOfBlow(Vector3 vx, float d);

    public abstract void StopMoving();

    public abstract void GetUnderControl(Transform T);
    
    public abstract void LoseControl();
}
