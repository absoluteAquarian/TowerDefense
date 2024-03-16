using UnityEngine;

[AddComponentMenu("Camera-Control/CameraLookTransformInterceptor")]
[RequireComponent(typeof(CameraFollow))]
public class CameraFollowTargetTransformInterceptor : MonoBehaviour {
	[SerializeField] private bool lockXAxis = false;
	[SerializeField] private bool lockYAxis = false;
	[SerializeField] private bool lockZAxis = false;

	public void AdjustTransform(Transform transform) {
		float x = lockXAxis ? 0 : transform.eulerAngles.x;
		float y = lockYAxis ? 0 : transform.eulerAngles.y;
		float z = lockZAxis ? 0 : transform.eulerAngles.z;

		transform.eulerAngles = new Vector3(x, y, z);
	}
}
