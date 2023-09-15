using UnityEngine;

public interface IMovable
{
    const float TimeToRunOff = 0.2f, TimeToGetPosition = 0.2f;
    public Vector3 VertDirection
    {
        get;
    }

    public void StartMoving(Vector3 vX, float distance);

    public void StartMovingOutOfBlow(Vector3 vX, float distance);

    public void StopMoving();

    static Vector3 GetDefaultSpot()
    {
        return new Vector3(1f, 0, 1.5f);
    }

    public void GetUnderControl(Transform controller);

    public void LoseControl();
}
