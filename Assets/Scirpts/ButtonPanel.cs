﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonPanel : MonoBehaviour
{
    public List<ButtonInfo> Buttons;

    public bool DestroyButtons = false;
    public bool UpdateButtons = false;

    public string PrefabName = "ProductButton";

    private void UpdateSpawnedButtons()
    {
        if (DestroyButtons)
        {
            DestroyButtons = false;
            for (int i = 0; i < Buttons.Count; i++)
            {
                if (Buttons[i] == null)
                    continue;
                if (Buttons[i].Button != null)
                {
                    DestroyImmediate(Buttons[i].Button.gameObject);
                }
            }
        }

        if (UpdateButtons)
        {
            UpdateButtons = false;

            GameObject go = null;
            ProductButton button = null;
            for (int i = 0; i < Buttons.Count; i++)
            {
                if (Buttons[i] == null)
                    continue;

                Vector3 pos = Vector3.zero;
                Vector2 size = new Vector2(570, 200);
                string text = string.Empty;
                if (Buttons[i].Button != null)
                {
                    var rect = Buttons[i].Button.GetComponent<RectTransform>();
                    pos = rect.position;
                    size = rect.sizeDelta;
                    text = Buttons[i].Button.Label.text;

                    DestroyImmediate(Buttons[i].Button.gameObject);
                }

                if (Buttons[i].Button == null)
                {
                    string prefab = PrefabName;
                    if (string.IsNullOrEmpty(Buttons[i].Name))
                        prefab = string.Format("{0}Empty", PrefabName);
                    {
#if UNITY_EDITOR
                        if (!Application.isPlaying)
                        {
                            go = UnityEditor.PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>(prefab)) as GameObject;
                        }
#endif
                        if (go == null)
                            go = GameObject.Instantiate(Resources.Load<GameObject>(prefab));

                        if (go != null)
                        {
                            go.transform.SetParent(transform);
                            go.transform.localScale = Vector3.one;

                            button = go.GetComponent<ProductButton>();
                            if (button != null)
                            {
                                button.info = Buttons[i];
                                Buttons[i].Button = button;

                                var rect = Buttons[i].Button.GetComponent<RectTransform>();
                                rect.position = pos;
                                rect.sizeDelta = size;
                            }
                        }
                    }
                }
                else
                {
                    button = Buttons[i].Button;
                    go = button.gameObject;
                }

                if (go != null)
                {
                    go.name = Buttons[i].Name;
                    if (string.IsNullOrEmpty(go.name))
                        go.name = "Empty";
                }

                Buttons[i].UpdateText();
                if (!string.IsNullOrEmpty(text))
                    button.Label.text = text;

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    UnityEditor.EditorUtility.SetDirty(button);
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                }
#endif
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }
    }

    private void OnDrawGizmos()
    {
        UpdateSpawnedButtons();
    }
}