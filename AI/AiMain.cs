using UnityEngine;

public abstract class AIMain : MonoBehaviour, IMovable
{
    //Базовый класс для всех врагов
    
    public Vector3 VertDirection { get; }
    
    public abstract void StartMoving(Vector3 vX, Vector3 vY, float d);
    
    public abstract void StartMovingOutOfBlow(Vector3 vX, Vector3 vY, float d);

    public abstract void StopMoving();

    public abstract void GetUnderControl(Transform T);
    
    public abstract void LoseControl();

    public abstract Vector3 GetVelocity();
}
