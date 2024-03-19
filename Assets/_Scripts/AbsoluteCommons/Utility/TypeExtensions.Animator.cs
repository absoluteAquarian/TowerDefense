using UnityEngine;

namespace AbsoluteCommons.Utility {
	partial class TypeExtensions {
		public static void IncrementFloat(this Animator animator, string parameterName, float value) {
			animator.SetFloat(parameterName, animator.GetFloat(parameterName) + value);
		}

		public static void IncrementInt(this Animator animator, string parameterName, int value) {
			animator.SetInteger(parameterName, animator.GetInteger(parameterName) + value);
		}
	}
}
