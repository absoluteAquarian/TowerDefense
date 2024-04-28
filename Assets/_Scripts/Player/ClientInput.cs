﻿using AbsoluteCommons.Input;
using Unity.Netcode;
using UnityEngine;

namespace TowerDefense.Player {
	[AddComponentMenu("Tower Defense/Input/Client Input")]
	public class ClientInput : NetworkBehaviour {
		private InputMap _map;

		public static ClientInput Singleton { get; private set; }

		private void Awake() {
			_map = new InputMap()
				.DefineAxis("Sprint")
				.DefineAxis("Horizontal")
				.DefineAxis("Vertical")
				.DefineAxis("Jump")
				.DefineAxis("Taunt")
				.DefineAxis("Fire")
				.DefineKey(KeyCode.F)
				.DefineKey(KeyCode.Escape)
				.DefineAxis("Deploy Weapon")
				.DefineAxis("Holster Weapon")
				.DefineAxis("Mouse X")
				.DefineAxis("Mouse Y");

			if (Singleton == null)
				Singleton = this;
			else
				Destroy(this);
		}

		private void Update() {
			_map.Update();
		}

		public override void OnDestroy() {
			if (Singleton == this)
				Singleton = null;

			base.OnDestroy();
		}

		private static bool IsInputValid() => Singleton && Singleton._map != null;

		public static float GetRaw(string name) => IsInputValid() ? Singleton._map.GetRaw(name) : 0;

		public static float GetRaw(KeyCode key) => IsInputValid() ? Singleton._map.GetRaw(key) : 0;

		public static bool IsInactive(string name) => IsInputValid() && Singleton._map.IsInactive(name);

		public static bool IsInactive(KeyCode key) => IsInputValid() && Singleton._map.IsInactive(key);

		public static bool IsTriggered(string name) => IsInputValid() && Singleton._map.IsTriggered(name);

		public static bool IsTriggered(KeyCode key) => IsInputValid() && Singleton._map.IsTriggered(key);

		public static bool IsPressed(string name) => IsInputValid() && Singleton._map.IsPressed(name);

		public static bool IsPressed(KeyCode key) => IsInputValid() && Singleton._map.IsPressed(key);

		public static bool IsReleased(string name) => IsInputValid() && Singleton._map.IsReleased(name);

		public static bool IsReleased(KeyCode key) => IsInputValid() && Singleton._map.IsReleased(key);
	}
}
