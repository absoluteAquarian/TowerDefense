using UnityEngine;

namespace AbsoluteCommons.Utility {
	partial class TypeExtensions {
		public static void IncrementFloat(this Animator animator, string parameterName, float value) {
			if (animator.HasParameter(parameterName))
				animator.SetFloat(parameterName, animator.GetFloat(parameterName) + value);
		}

		public static void IncrementInt(this Animator animator, string parameterName, int value) {
			if (animator.HasParameter(parameterName))
				animator.SetInteger(parameterName, animator.GetInteger(parameterName) + value);
		}

		public static bool HasParameter(this Animator animator, string parameterName) => AnimatorTracker.HasParameter(animator, parameterName);

		public static bool HasParameter(this Animator animator, int parameterHash) => AnimatorTracker.HasParameter(animator, parameterHash);

		public static void SetTriggerSafely(this Animator animator, string triggerName) {
			if (animator.HasParameter(triggerName))
				animator.SetTrigger(triggerName);
		}

		public static void ResetTriggerSafely(this Animator animator, string parameterName) {
			if (animator.HasParameter(parameterName))
				animator.ResetTrigger(parameterName);
		}

		public static void SetBoolSafely(this Animator animator, string parameterName, bool value) {
			if (animator.HasParameter(parameterName))
				animator.SetBool(parameterName, value);
		}

		public static void SetFloatSafely(this Animator animator, string parameterName, float value) {
			if (animator.HasParameter(parameterName))
				animator.SetFloat(parameterName, value);
		}

		public static void SetIntSafely(this Animator animator, string parameterName, int value) {
			if (animator.HasParameter(parameterName))
				animator.SetInteger(parameterName, value);
		}

		public static float GetFloatSafely(this Animator animator, string parameterName, float defaultValue = 0f) {
			if (animator.HasParameter(parameterName))
				return animator.GetFloat(parameterName);

			return defaultValue;
		}

		public static int GetIntSafely(this Animator animator, string parameterName, int defaultValue = 0) {
			if (animator.HasParameter(parameterName))
				return animator.GetInteger(parameterName);

			return defaultValue;
		}

		public static bool GetBoolSafely(this Animator animator, string parameterName, bool defaultValue = false) {
			if (animator.HasParameter(parameterName))
				return animator.GetBool(parameterName);

			return defaultValue;
		}
	}
}
