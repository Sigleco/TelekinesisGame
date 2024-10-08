using UnityEngine;

public interface ICutter
{
    void SetCuttingParams(Vector3 contactPoint, Vector3 planeTangent1, Vector3 planeTangent2, GameObject cuttingObj);

    void StartCutting();
}
