using UnityEngine;

public struct Trajectory
{
    private const float MaxDistance = 10f;
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
    }

    public Vector3 ComputeOffset(bool outOfBlow)
    {
        Vector3 temp =_vX * Time.deltaTime + _vY * ComputeDeltaHeight(_curTime, outOfBlow);
        _curTime += Time.deltaTime;
        _sumOfSteps += temp.x;
        
        if (_sumOfSteps >= _curDistance)
        {
            _sumOfSteps -= temp.x;
            float delta = _curDistance - _sumOfSteps;
            temp *= delta / temp.x;
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
