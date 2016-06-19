/*Author: Tobias Zimmerlin
 * 30.01.2015
 * V1
 *
 */

#if UNITY_EDITOR

using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SimpleLibrary
{
	public class SimpleKeyboardShortcuts
	{
		//& = alt
		//# = shift

		[MenuItem("GameObject/Toggle Active State &a")]
		private static void ToggleGameObjectActiveState_Shift_A()
		{
			ToggleGameObjectActiveState();
		}

		[MenuItem("GameObject/Create Empty Child &n")]
		private static void CreateNewEmptyGameObjectChild_Shift_N()
		{
			CreateGameObjectChild();
		}

		[MenuItem("Edit/Rename &r")]
		private static void Rename_Shift_R()
		{
			Rename();
		}

		[MenuItem("GameObject/Wrap in Empty &w")]
		private static void WrapInObject_Shift_W()
		{
			WrapInGameObject();
		}

		#region Utils
		private static void ToggleGameObjectActiveState()
		{
			Selection.activeGameObject.SetActive(!Selection.activeGameObject.activeSelf);
		}
		private static void Rename()
		{
			var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
			var hierarchyWindow = EditorWindow.GetWindow(type);
			var rename = type.GetMethod("RenameGO", BindingFlags.Instance | BindingFlags.NonPublic);
			rename.Invoke(hierarchyWindow, null);
		}
		private static void WrapInGameObject()
		{
			if (Selection.gameObjects.Length == 0)
				return;
			GameObject go = new GameObject("Wrapper:NameMe");
			go.transform.parent = Selection.activeTransform.parent;
			go.transform.position = Vector3.zero;
			foreach (GameObject g in Selection.gameObjects)
			{
				g.transform.parent = go.transform;
			}

			Selection.activeTransform = go.transform;
			Rename();
		}
		private static void CreateGameObjectChild()
		{
			GameObject go = new GameObject("Child");
			go.transform.parent = Selection.activeTransform;
			go.transform.localPosition = Vector3.zero;
			Selection.activeTransform = go.transform;

			Rename();
		}
		#endregion Utils
	}
}

#endif