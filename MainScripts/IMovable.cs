using UnityEngine;

public interface IMovable
{
    //Определяет функционал обьекта, который может быть взят под контроль
    
    const float TimeToRunOff = 0.2f, TimeToGetPosition = 0.2f;
    public Vector3 VertDirection
    {
        get;
    }

    public Vector3 GetVelocity();

    public void StartMoving(Vector3 vX, Vector3 vY, float distance);

    public void StartMovingOutOfBlow(Vector3 vX, Vector3 vY, float distance);

    public void StopMoving();

    static Vector3 GetDefaultSpot()
    {
        return new Vector3(1f, 0, 1.5f);
    }

    public void GetUnderControl(Transform controller);

    public void LoseControl();
}
