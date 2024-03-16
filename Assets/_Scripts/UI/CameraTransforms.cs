using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Debug/Camera-Transforms")]
public class CameraTransforms : MonoBehaviour {
	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}

	private void OnGUI() {
		// Get the "Player" object
		GameObject player = GameObject.Find("Player");

		// Get the "View" child object of the "Player" object
		Camera view = player.transform.Find("Visual").Find("View").GetComponent<Camera>();

		// Get the "FirstPersonView" component of the "View" object
		FirstPersonView viewScript = view.gameObject.GetComponent<FirstPersonView>();

		GUILayout.BeginArea(new Rect(10, 100, 300, 400));

		// Print the rotations of the "Player" and "View" objects
		GUILayout.Label("Player: ");
		GUILayout.Label("  Pos: " + FormatVector(player.transform.position));
		GUILayout.Label("  For: " + FormatVector(player.transform.forward));
		GUILayout.Label("  Rot: " + FormatVector(player.transform.rotation.eulerAngles));
		GUILayout.Label("View: ");
		GUILayout.Label("  Pos: " + FormatVector(view.transform.position));
		GUILayout.Label("  For: " + FormatVector(view.transform.forward));
		GUILayout.Label("  Rot: " + FormatVector(view.transform.rotation.eulerAngles));
		GUILayout.Label("Control: ");
		GUILayout.Label("  Locked: " + viewScript.IsLocked);
		GUILayout.Label("  Rot: " + FormatVector(viewScript.ViewRotation));

		GUILayout.EndArea();
	}

	private static string FormatVector(Vector3 vector) {
		return $"({Format(vector.x)}, {Format(vector.y)}, {Format(vector.z)})";
	}

	private static string Format(float value) {
		return $"{value:+0.000;-0.000}";
	}
}
