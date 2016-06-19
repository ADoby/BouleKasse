/*Author: Tobias Zimmerlin
 * 30.01.2015
 * V1
 *
 */
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SimpleLibrary
{
	[CustomEditor(typeof(Transform))]
	public class TransformInspector : Editor
	{
		[MenuItem("GameObject/SortChildrenByName")]
		public static void SortChildrenByName()
		{
			foreach (GameObject obj in Selection.gameObjects)
			{
				List<Transform> children = new List<Transform>();
				for (int i = obj.transform.childCount - 1; i >= 0; i--)
				{
					Transform child = obj.transform.GetChild(i);
					children.Add(child);
					child.parent = null;
				}
				children.Sort((Transform t1, Transform t2) => { return t1.name.CompareTo(t2.name); });
				foreach (Transform child in children)
				{
					child.parent = obj.transform;
				}
			}
		}

		public struct TransInfo
		{
			public Transform trans;
			public Vector3 pos;
			public Quaternion rot;
			public Vector3 scale;
		}

		private static bool showParams = false;

		public void ResetWithoutChilds(Transform trans, bool pos = true, bool rot = true, bool scale = true)
		{
			Undo.RecordObject(trans, "Transform Reset Only Me");

			List<TransInfo> childs = new List<TransInfo>();
			foreach (Transform child in trans)
			{
				if (child != trans)
				{
					Undo.RecordObject(child, "Transform Reset All Without Children");
					childs.Add(new TransInfo() { trans = child, pos = child.position, rot = child.rotation, scale = child.localScale });
				}
			}

			Vector3 scaleMults = new Vector3(1, 1, 1);
			Vector3 newScale = new Vector3(1, 1, 1);
			if (!scale)
				newScale = trans.localScale;
			scaleMults.x = trans.localScale.x / newScale.x;
			scaleMults.y = trans.localScale.y / newScale.y;
			scaleMults.z = trans.localScale.z / newScale.z;

			if (pos)
				trans.localPosition = Vector3.zero;
			if (rot)
				trans.localEulerAngles = Vector3.zero;
			if (scale)
				trans.localScale = newScale;

			foreach (var child in childs)
			{
				child.trans.position = child.pos;
				child.trans.rotation = child.rot;
				newScale = child.scale;
				newScale.x *= scaleMults.x;
				newScale.y *= scaleMults.y;
				newScale.z *= scaleMults.z;
				child.trans.localScale = newScale;
			}
			childs.Clear();
		}

		public override void OnInspectorGUI()
		{
			Transform trans = (Transform)target;
			EditorGUI.indentLevel = 0;

			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button(new GUIContent("Reset", "Resets this transform, childs will change"), GUILayout.MinWidth(50)))
			{
				Undo.RecordObject(trans, "Transform Reset All");
				trans.localPosition = Vector3.zero;
				trans.localEulerAngles = Vector3.zero;
				trans.localScale = new Vector3(1, 1, 1);
			}
			if (GUILayout.Button(new GUIContent("Reset without children", "Resets this transform, childs will stay the same")))
			{
				ResetWithoutChilds(trans);
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("Round all", "Rounds values to full numbers")))
			{
				Undo.RecordObject(trans, "Transform Reset All");
				trans.localPosition = new Vector3(Mathf.Round(trans.localPosition.x), Mathf.Round(trans.localPosition.y), Mathf.Round(trans.localPosition.z));
				trans.localEulerAngles = new Vector3(Mathf.Round(trans.localRotation.eulerAngles.x), Mathf.Round(trans.localRotation.eulerAngles.y), Mathf.Round(trans.localRotation.eulerAngles.z));
				trans.localScale = new Vector3(Mathf.Round(trans.localScale.x), Mathf.Round(trans.localScale.y), Mathf.Round(trans.localScale.z));
			}
			if (GUILayout.Button("position"))
			{
				Undo.RecordObject(trans, "Transform Reset Position");
				trans.localPosition = new Vector3(Mathf.Round(trans.localPosition.x), Mathf.Round(trans.localPosition.y), Mathf.Round(trans.localPosition.z));
			}
			if (GUILayout.Button("rotation"))
			{
				Undo.RecordObject(trans, "Transform Reset Rotation");
				trans.localEulerAngles = new Vector3(Mathf.Round(trans.localRotation.eulerAngles.x), Mathf.Round(trans.localRotation.eulerAngles.y), Mathf.Round(trans.localRotation.eulerAngles.z));
			}
			if (GUILayout.Button("scale"))
			{
				Undo.RecordObject(trans, "Transform Reset Scale");
				trans.localScale = new Vector3(Mathf.Round(trans.localScale.x), Mathf.Round(trans.localScale.y), Mathf.Round(trans.localScale.z));
			}
			EditorGUILayout.EndHorizontal();

			//Position
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("R", "Reset Position to (0,0,0)"), EditorStyles.miniButton, GUILayout.Width(19)))
			{
				Undo.RecordObject(trans, "Transform Reset Position");
				trans.localPosition = Vector3.zero;
			}
			if (GUILayout.Button(new GUIContent("RwC", "Reset Position to (0,0,0) without changing children"), EditorStyles.miniButton, GUILayout.Width(35)))
			{
				ResetWithoutChilds(trans, true, false, false);
			}
			Vector3 position = EditorGUILayout.Vector3Field("LocalPosition", trans.localPosition);
			EditorGUILayout.EndHorizontal();

			//Rotation
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("R", "Reset Rotation to (0,0,0)"), EditorStyles.miniButton, GUILayout.Width(19)))
			{
				Undo.RecordObject(trans, "Transform Reset Rotation");
				trans.localEulerAngles = Vector3.zero;
			}
			if (GUILayout.Button(new GUIContent("RwC", "Reset Rotation to (0,0,0) without changing children"), EditorStyles.miniButton, GUILayout.Width(35)))
			{
				ResetWithoutChilds(trans, false, true, false);
			}
			Vector3 eulerAngles = EditorGUILayout.Vector3Field("LocalRotation", trans.localEulerAngles);
			EditorGUILayout.EndHorizontal();

			//Scale
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("R", "Reset Scale to (0,0,0)"), EditorStyles.miniButton, GUILayout.Width(19)))
			{
				Undo.RecordObject(trans, "Transform Reset Scale");
				trans.localScale = new Vector3(1, 1, 1);
			}
			if (GUILayout.Button(new GUIContent("RwC", "Reset Scale to (0,0,0) without changing children"), EditorStyles.miniButton, GUILayout.Width(35)))
			{
				ResetWithoutChilds(trans, false, false, true);
			}
			Vector3 scale = EditorGUILayout.Vector3Field("LocalScale", trans.localScale);
			EditorGUILayout.EndHorizontal();

			//World Attributes
			showParams = EditorGUILayout.Foldout(showParams, "World Attributes:");
			if (showParams)
			{
				Vector3FieldEx("Position", trans.position);
				Vector3FieldEx("Rotation", trans.eulerAngles);
				Vector3FieldEx("Scale", trans.lossyScale);
			}

			if (GUI.changed)
			{
				Undo.RecordObject(trans, "Transform Change");

				trans.localPosition = FixIfNaN(position);
				trans.localEulerAngles = FixIfNaN(eulerAngles);
				trans.localScale = FixIfNaN(scale);
			}
		}

		private Vector3 FixIfNaN(Vector3 v)
		{
			if (float.IsNaN(v.x))
			{
				v.x = 0;
			}
			if (float.IsNaN(v.y))
			{
				v.y = 0;
			}
			if (float.IsNaN(v.z))
			{
				v.z = 0;
			}
			return v;
		}

		public static Vector3 Vector3FieldEx(string label, Vector3 value, float labelWidth = 60f)
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label(label, GUILayout.Width(50));

			EditorGUILayout.LabelField("X", GUILayout.Width(12));
			value.x = EditorGUILayout.FloatField(value.x);

			EditorGUILayout.LabelField("Y", GUILayout.Width(12));
			value.y = EditorGUILayout.FloatField(value.y);

			EditorGUILayout.LabelField("Z", GUILayout.Width(12));
			value.z = EditorGUILayout.FloatField(value.z);
			EditorGUILayout.EndHorizontal();
			return value;
		}
	}
}

#endif