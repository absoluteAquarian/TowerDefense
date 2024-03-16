using UnityEngine;

namespace AbsoluteCommons.Utility {
	public static class VectorMath {
		public static float DistanceSquared(Vector3 a, Vector3 b) => Vector3.SqrMagnitude(a - b);

		public static Vector3 DirectionTo(Vector3 from, Vector3 to) => Vector3.Normalize(to - from);

		public static Vector3 DirectionFrom(Vector3 from, Vector3 to) => Vector3.Normalize(from - to);
	}
}
