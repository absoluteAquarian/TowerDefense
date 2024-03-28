using UnityEngine;

namespace AbsoluteCommons.Utility {
	partial class TypeExtensions {
		public static void DestroyAndSetNull(ref Object obj) {
			if (obj) {
				Object.Destroy(obj);
				obj = null;
			}
		}

		public static void DestroyAndSetNull<T>(ref T obj) where T : Object {
			if (obj) {
				Object.Destroy(obj);
				obj = null;
			}
		}
	}
}
