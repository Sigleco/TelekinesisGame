using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour {

	[SerializeField] private Transform focus = default;
	[SerializeField, Range(1f, 360f)] private float rotationSpeed;
	[SerializeField, Range(-89f, 89f)] private float minVerticalAngle = -45f, maxVerticalAngle = 45f;
	
	private Vector3 /*_focusPoint,*/ _previousFocusPoint;
	private Vector2 _orbitAngles = new Vector2(45f, 0f);
	

	private void OnValidate ()
	{
		if (maxVerticalAngle < minVerticalAngle)
		{
			maxVerticalAngle = minVerticalAngle;
		}
	}

	private void Awake ()
	{
		//_regularCamera = GetComponent<Camera>();
		//_focusPoint = focus.position;
		transform.localRotation = Quaternion.Euler(_orbitAngles);
	}

	private void LateUpdate ()
	{
		//UpdateFocusPoint();
		Quaternion lookRotation;
		
		if (ManualRotation())// || AutomaticRotation())
		{
			ConstrainAngles();
			lookRotation = Quaternion.Euler(_orbitAngles);
		}
		else
		{
			lookRotation = transform.localRotation;
		}

		//Vector3 lookDirection = lookRotation * Vector3.forward;
		//Vector3 lookPosition = _focusPoint - lookDirection * distance;
		//Vector3 rectOffset = lookDirection * _regularCamera.nearClipPlane;
		//Vector3 rectPosition = lookPosition + rectOffset;
		//Vector3 castFrom = focus.position;
		//Vector3 castLine = rectPosition - castFrom;
		//float castDistance = castLine.magnitude;
		//Vector3 castDirection = castLine / castDistance;

		/*if (Physics.BoxCast(castFrom, CameraHalfExtends, castDirection, out RaycastHit hit, lookRotation, castDistance, obstructionMask))
		{
			rectPosition = castFrom + castDirection * hit.distance;
			lookPosition = rectPosition - rectOffset;
		}*/
		
		transform.SetPositionAndRotation(focus.position, lookRotation);
	}

	private bool ManualRotation ()
	{
		Vector2 input = new Vector2(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
		const float e = 0.001f;
		
		if (input.x < -e || input.x > e || input.y < -e || input.y > e)
		{
			_orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
			//_lastManualRotationTime = Time.unscaledTime;
			return true;
		}
		
		return false;
	}

	private void ConstrainAngles ()
	{
		_orbitAngles.x = Mathf.Clamp(_orbitAngles.x, minVerticalAngle, maxVerticalAngle);

		if (_orbitAngles.y < 0f)
		{
			_orbitAngles.y += 360f;
		}
		else if (_orbitAngles.y >= 360f)
		{
			_orbitAngles.y -= 360f;
		}
	}
	
	//[SerializeField, Range(0f, 20f)] private float distance = 5f;
	//[SerializeField, Min(0f)] private float focusRadius = 5f;
	//[SerializeField, Range(0f, 1f)] private float focusCentering = 0.1f;
	/*[SerializeField, Min(0f)] private float alignDelay;
	[SerializeField, Range(0f, 90f)] private float alignSmoothRange;
	[SerializeField] private LayerMask obstructionMask = -1;

	//private Camera _regularCamera;*/
	/*private float _lastManualRotationTime;
	private Vector3 CameraHalfExtends
	{
		get
		{
			Vector3 halfExtends;
			halfExtends.y = _regularCamera.nearClipPlane * Mathf.Tan(0.5f * Mathf.Deg2Rad * _regularCamera.fieldOfView);
			halfExtends.x = halfExtends.y * _regularCamera.aspect;
			halfExtends.z = 0f;
			return halfExtends;
		}
	}*/
	
	/*private void UpdateFocusPoint ()
	{
		_previousFocusPoint = _focusPoint;
		Vector3 targetPoint = focus.position;
		
		if (focusRadius > 0f)
		{
			float distance = Vector3.Distance(targetPoint, _focusPoint);
			float t = 1f;
			
			if (distance > 0.01f && focusCentering > 0f)
			{
				t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime);
			}
			
			if (distance > focusRadius)
			{
				t = Mathf.Min(t, focusRadius / distance);
			}
			
			_focusPoint = Vector3.Lerp(targetPoint, _focusPoint, t);
		}
		else
		{
			_focusPoint = targetPoint;
		}
	}*/
	
	/*private bool AutomaticRotation ()
	{
		if (Time.unscaledTime - _lastManualRotationTime < alignDelay)
		{
			return false;
		}

		Vector2 movement = new Vector2(_focusPoint.x - _previousFocusPoint.x, _focusPoint.z - _previousFocusPoint.z);
		float movementDeltaSqr = movement.sqrMagnitude;
		
		if (movementDeltaSqr < 0.0001f)
		{
			return false;
		}

		float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
		float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(_orbitAngles.y, headingAngle));
		float rotationChange = rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
		
		if (deltaAbs < alignSmoothRange) 
		{
			rotationChange *= deltaAbs / alignSmoothRange;
		}
		else if (180f - deltaAbs < alignSmoothRange)
		{
			rotationChange *= (180f - deltaAbs) / alignSmoothRange;
		}
		
		_orbitAngles.y = Mathf.MoveTowardsAngle(_orbitAngles.y, headingAngle, rotationChange);
		return true;
	}*/
	
	/*private static float GetAngle (Vector2 direction)
	{
		float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
		return direction.x < 0f ? 360f - angle : angle;
	}*/
}