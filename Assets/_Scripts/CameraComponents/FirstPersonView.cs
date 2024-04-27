using System;
using TowerDefense.Player;
using UnityEngine;

// Code was derived from: https://stackoverflow.com/questions/8465323/unity-fps-rotation-camera

namespace TowerDefense.CameraComponents {
	[AddComponentMenu("Camera-Control/FirstPersonView")]
	public class FirstPersonView : MonoBehaviour {
		public enum Axes {
			MouseXAndY,
			MouseX,
			MouseY
		}

		public readonly struct Direction {
			private readonly int _horizontal, _vertical;

			public int Horizontal => _horizontal;
			public int Vertical => _vertical;

			public Direction(int horizontal, int vertical) {
				_horizontal = horizontal;
				_vertical = vertical;
			}
		}

		public Axes axes = Axes.MouseXAndY;
		public float sensitivityX = 15f;
		public float sensitivityY = 15f;

		public float minimumY = -60f;
		public float maximumY = 60f;

		float rotationX = 0f;
		float rotationY = 0f;

		private int _rotationDirectionHorizontal;
		private int _rotationDirectionVertical;

		public Vector3 ViewRotation => new Vector3(-rotationX, rotationY, 0);
		public Direction RotationDirection => new Direction(_rotationDirectionHorizontal, _rotationDirectionVertical);

		// Start is called before the first frame update
		void Start() {
			if (TryGetComponent<Rigidbody>(out var body))
				body.freezeRotation = true;
		}

		private bool _lockedCamera = false;

		public bool IsLocked => _lockedCamera;

		// Update is called once per frame
		void Update() {
			if (ClientInput.IsTriggered(KeyCode.Escape)) {
				_lockedCamera = !_lockedCamera;
				Cursor.lockState = _lockedCamera ? CursorLockMode.Locked : CursorLockMode.None;
			}

			Cursor.visible = !_lockedCamera;

			if (!_lockedCamera) {
				_rotationDirectionHorizontal = 0;
				_rotationDirectionVertical = 0;
				return;
			}

			switch (axes) {
				case Axes.MouseXAndY:
					SetRotationX();
					SetRotationY();
					break;
				case Axes.MouseX:
					SetRotationY();
					break;
				case Axes.MouseY:
					SetRotationX();
					break;
			}
		}

		private void SetRotationX() {
			float rotation = ClientInput.GetRaw("Mouse Y") * sensitivityY;

			_rotationDirectionVertical = Math.Sign(rotation);

			rotationX += rotation;
			rotationX = Mathf.Clamp(rotationX, minimumY, maximumY);
		}

		private void SetRotationY() {
			float rotation = ClientInput.GetRaw("Mouse X") * sensitivityX;

			_rotationDirectionHorizontal = Math.Sign(rotation);

			rotationY += rotation;
			rotationY %= 360;
		}
	}
}
