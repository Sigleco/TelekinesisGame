using UnityEngine;

public struct Trajectory
{
    //Определяет, как объект движется с заданными проекциями скоростей на оси и дистанцией до цели
    //TODO: необходимо следить что обьект долетал до задуманной точки? Проверить, исправить
    //TODO: trajectory after collision is wrong.
    
    private const float MaxDistance = 50f;
    public readonly float Vx;
    
    private Vector3 _vX, _vY;
    private float _curTime, _lastHeight, _curDistance, _sumOfSteps;

    public Trajectory(Vector3 vX, Vector3 vY,  float distance)
    {
        _vX = vX;
        _vY = vY;
        _curDistance = distance - 0.5f;//0.5f because basis of projectile lay in the centre of the model
        Vx = vX.magnitude;
        _curTime = _lastHeight = _sumOfSteps = 0;
        if (_vY.magnitude > 0)
        {
            _vY = GetVerticalVector();
        }
    }

    private Vector3 GetVerticalVector()
    {
        Vector3 temp = Vector3.Cross(_vX, new Vector3(_vX[2], 0f, _vX[0]));
        
        if (temp.y < 0)
        {
            temp *= -1;
        }
        
        return temp.normalized;
    }
    
    public Vector3 ComputeOffset(bool outOfBlow)
    {
        Vector3 deltaX = _vX * Time.deltaTime;
        Vector3 deltaY = _vY * ComputeDeltaHeight(_curTime, outOfBlow);
        Vector3 temp = deltaX + deltaY;
        _curTime += Time.deltaTime;
        _sumOfSteps += deltaX.magnitude;
        
        if (_sumOfSteps >= _curDistance)
        {
            _sumOfSteps -= deltaX.magnitude;
            float delta = _curDistance - _sumOfSteps;
            temp =  delta * deltaX.normalized + deltaY;
        }

        return temp;
    }

    private float ComputeDeltaHeight(float x, bool outOfBlow)
    {
        float temp;
        
        if (outOfBlow)
        {
            temp = ComputeBlowHeight(x) - _lastHeight;
        }
        else
        {
            temp = ComputeHeight(x) - _lastHeight;
        }

        _lastHeight = temp;

        return temp;
    }

    private float ComputeHeight(float x)
    {
        return  0.25f * x * (x - _curDistance/ MaxDistance);
    }
    
    private float ComputeBlowHeight(float x)
    {
        return (2/Mathf.PI) * Mathf.Atan(5 * x) * ComputeHeight(x);
    }
}
