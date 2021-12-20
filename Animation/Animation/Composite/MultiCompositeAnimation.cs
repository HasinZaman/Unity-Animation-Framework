using System;
using UnityEditor;
using UnityEngine;

public class MultiCompositeAnimation : CompositeAnimation<MonoCompositeAnimation>
{
    public const string animationType = "MultiCompositeAnimation";

    public MultiCompositeAnimation(AnimationManager parent) : base(parent)
    {
    }

    public MultiCompositeAnimation(AnimationManager parent, JSON json) : base(parent, json)
    {
        float maxDuration = 0;

        for (int i1 = 0; i1 < this.animations.Count; i1++)
        {
            if (this.animations[i1].getDuration() > maxDuration)
            {
                maxDuration = this.animations[i1].getDuration();
            }
        }

        this.duration = maxDuration;
    }

    //Editor UI
    private int editIndex = -1;

    //animation variables
    private float duration = 0;

    public override void drawMenu()
    {
        float maxDuration = 0;

        //Animation setting bar
        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("Add Layer"))
        {
            this.animations.Add(new MonoCompositeAnimation(this.parent));
        }
        EditorGUILayout.EndHorizontal();

        //animation time line

        for (int i1 = 0; i1 < this.animations.Count; i1++)
        {

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Duration");
            EditorGUILayout.FloatField(this.animations[i1].getDuration());
            if(GUILayout.Button("Delete"))
            {
                this.animations.RemoveAt(i1);
                break;
            }
            EditorGUILayout.EndHorizontal();
            this.animations[i1].drawMenu();
            if(this.animations[i1].getEditIndex() != -1 && i1 != editIndex)
            {
                if(editIndex!=-1)
                {
                    this.animations[editIndex].closeEdit();
                }
                editIndex = i1;
            }

            if(this.animations[i1].getDuration() > maxDuration)
            {
                maxDuration = this.animations[i1].getDuration();
            }
        }

        this.duration = maxDuration;
    }

    public override void animate(float t)
    {
        float time = t * this.duration;
        foreach (MonoCompositeAnimation anim in this.animations)
        {
            anim.animate(time / anim.getDuration());
        }
    }

    public override string getJsonData()
    {
        return
            $"\"type\":\"{animationType}\"," +
            $"{base.getJsonData()}";
    }

    public override Animation Clone()
    {
        MultiCompositeAnimation tmp = new MultiCompositeAnimation(this.parent);

        foreach (MonoCompositeAnimation a in this.animations)
        {
            tmp.animations.Add((MonoCompositeAnimation) a.Clone());
        }

        tmp.name = this.name;
        return tmp;
    }
}