using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace AbsoluteCommons.Utility {
	partial class TypeExtensions {
		public static void SmartDespawn(this NetworkObject obj, bool destroy, bool includeSelf = true) {
			// Calling despawn will move child objects to the root of the scene
			// This is a workaround to keep the hierarchy clean
			Stack<NetworkObject> despawnStack = new Stack<NetworkObject>();
			Queue<Transform> despawnQueue = new Queue<Transform>();
			despawnQueue.Enqueue(obj.gameObject.transform);

			while (despawnQueue.TryDequeue(out Transform current)) {
				if ((includeSelf || current != obj.gameObject.transform) && current.TryGetComponent(out NetworkObject networkObject))
					despawnStack.Push(networkObject);

				foreach (Transform child in current)
					despawnQueue.Enqueue(child);
			}

			while (despawnStack.TryPop(out NetworkObject networkObject)) {
				if (networkObject.IsSpawned)
					networkObject.Despawn(destroy);
			}
		}

		public static void SmartSpawn(this NetworkObject obj, bool destroyWithScene = false) {
			// Calling spawn will not spawn child network objects
			Queue<Transform> spawnQueue = new Queue<Transform>();
			Queue<NetworkObject> networkParents = new Queue<NetworkObject>();
			spawnQueue.Enqueue(obj.gameObject.transform);

			while (spawnQueue.TryDequeue(out Transform current)) {
				if (current.TryGetComponent(out NetworkObject networkObject)) {
					if (!networkObject.IsSpawned)
						networkObject.Spawn(destroyWithScene);

					networkParents.Enqueue(networkObject);
				}

				foreach (Transform child in current)
					spawnQueue.Enqueue(child);
			}

			while (networkParents.TryDequeue(out NetworkObject networkParent)) {
				foreach (Transform child in networkParent.gameObject.transform) {
					if (child.TryGetComponent(out NetworkObject networkObject))
						SmartSpawnEnsureParentConnectionServerRpc(networkParent.NetworkObjectId, networkObject.NetworkObjectId);
				}
			}
		}

		[ServerRpc]
		private static void SmartSpawnEnsureParentConnectionServerRpc(ulong parentNetworkObjectID, ulong childNetworkObjectID) {
			if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(parentNetworkObjectID, out NetworkObject parentNetworkObject))
				return;

			if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(childNetworkObjectID, out NetworkObject childNetworkObject))
				return;

			childNetworkObject.transform.SetParent(parentNetworkObject.transform, false);
		}
	}
}
