using System.Collections.Generic;
using UnityEngine;

namespace AbsoluteCommons.Utility {
	partial class TypeExtensions {
		public static GameObject FindChildRecursively(this GameObject parent, string name) {
			Queue<GameObject> scanQueue = new();
			scanQueue.Enqueue(parent);

			while (scanQueue.TryDequeue(out GameObject current)) {
				if (current.name == name)
					return current;

				foreach (Transform child in current.transform)
					scanQueue.Enqueue(child.gameObject);
			}

			return null;
		}
	}
}
