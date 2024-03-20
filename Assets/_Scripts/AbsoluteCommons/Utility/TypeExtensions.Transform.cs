using UnityEngine;

namespace AbsoluteCommons.Utility {
	partial class TypeExtensions {
		public static void RotateWithPivot(this Transform transform, Vector3 pivot, Quaternion rotation) {
			Vector3 offset = pivot - transform.position;
			transform.position = pivot;
			transform.rotation = rotation;
			transform.position -= rotation * offset;
		}
	}
}
