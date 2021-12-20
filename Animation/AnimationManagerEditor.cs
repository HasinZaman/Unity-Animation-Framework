using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(AnimationManager))]
public class AnimationManagerEditor : Editor
{
    private int state = 0;
    private string[] windows = new string[] {"Timeline", "Templates"};

    private AnimationManager animationManager;
    SerializedProperty id;
    SerializedObject targetObj;

    int previewIndex = 0;
    float previewPos = 0;
    void OnEnable()
    {
        animationManager = (AnimationManager) target;
        targetObj = new UnityEditor.SerializedObject(animationManager);
        id = targetObj.FindProperty("id");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        if(id.stringValue.Equals(AnimationManager.defaultID))
        {
            id.stringValue = animationManager.getInstanceID();
            Debug.Log($"SETTING VALUE TO:{id.stringValue}");
        }
        id.stringValue = EditorGUILayout.TextField("Object ID", id.stringValue);

        drawTabs();
        switch (state)
        {
            case 0:
                timelineGUI();
                break;
            case 1:
                animationEditorGUI();
                break;
        }

        targetObj.ApplyModifiedProperties();
        if (GUI.changed && !EditorApplication.isPlaying)
        {
            animationManager.saveAnimations();
            EditorUtility.SetDirty(animationManager);
            EditorSceneManager.MarkSceneDirty(animationManager.gameObject.scene);
        }
    }
    private void drawTabs()
    {
        EditorGUILayout.BeginHorizontal();
        for (int i1 = 0; i1 < windows.Length; i1++)
        {
            if (state == i1)
            {
                GUILayout.Label(windows[i1]);
            }
            else if (GUILayout.Button(windows[i1]))
            {
                state = i1;
                previewIndex = 0;
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    public void timelineGUI()
    {
        animationManager.startFrame = EditorGUILayout.IntField("Start Frame:", animationManager.startFrame);
        animationManager.duration = EditorGUILayout.IntField("Duration:", animationManager.duration);

        GUILayout.Label("Preview");

        float intialVal = previewPos;
        previewPos = EditorGUILayout.Slider("Pos", previewPos, 0, 1);

        if (intialVal != previewPos)
        {
            animationManager.animation.animate(previewPos);
        }

        animationManager.animation.drawMenu();
    }

    
    public void animationEditorGUI()
    {
        string[] animationsNames = new string[animationManager.animationTemplates.Length];

        for(int i1 = 0; i1 < animationsNames.Length; i1++)
        {
            animationsNames[i1] = $"{animationManager.animationTemplates[i1]}:{animationManager.animationTemplates[i1].name}";
        }

        GUILayout.Label("Preview");

        float intialVal = previewPos;
        previewPos = EditorGUILayout.Slider("Pos", previewPos, 0, 1);

        previewIndex = EditorGUILayout.Popup("Animation", previewIndex, animationsNames);

        if(intialVal != previewPos)
        {
            animationManager.animationTemplates[previewIndex].animate(previewPos);
        }


        GUILayout.Space(10);
        //listing templates
        GUILayout.Label("Templates");
        drawList(ref animationManager.animationTemplates);
    }

    int i1 = 0;
    int i2 = 0;
    private void drawList(ref Animation[] animations)
    {
        GUILayout.BeginHorizontal();
        Animation tmp = Animation.creationMenu(ref i1, animationManager);
        if(tmp != null)
        {
            animations = animations.Concat(new Animation[1] { tmp }).ToArray();
        }
        if (GUILayout.Button("Reset"))
        {
            animations = new Animation[] { };
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.IntField("Size", animations.Length);

        if(animations.Length > 0)
        {

            EditorGUILayout.BeginHorizontal();

            string[] animationNames = new string[animations.Length];
            for (int i = 0; i < animations.Length; i++)
            {
                animationNames[i] = $"{i}:{animations[i].name}";
            }

            GUILayout.Label("Selected:");
            i2 = clamp(EditorGUILayout.IntField(i2), 0, animations.Length - 1);
            i2 = EditorGUILayout.Popup(i2, animationNames);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"{i2}:{animationManager.animationTemplates[i2]}");

            animations[i2].name = EditorGUILayout.TextField(animations[i2].name);

            if (GUILayout.Button("Delete"))
            {
                List<Animation> animationsTmp = new List<Animation>(animations);
                animationsTmp.RemoveAt(i2);
                animations = animationsTmp.ToArray();
            }

            EditorGUILayout.EndHorizontal();

            animations[i2].drawMenu();
        }
    }

    private int clamp(int i, int min, int max)
    {
        if(i < min)
        {
            return min;
        }
        if(max < i)
        {
            return max;
        }
        return i;
    }
}
