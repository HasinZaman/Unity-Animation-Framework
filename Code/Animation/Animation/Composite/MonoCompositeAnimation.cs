using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class MonoCompositeAnimation : CompositeAnimation<TimeLineAnimation>
{
    public const string animationType = "MonoCompositeAnimation";

    //Editor UI
    private int menuIndex = 0;
    private Vector2 scrollPos = Vector2.zero;

    private int editIndex = -1;

    //animation variables
    private float duration = 0;

    //cache variables
    private float lastT = -1;
    private int lastAnim = 0;

    public MonoCompositeAnimation(AnimationManager parent) : base(parent)
    {
    }

    public MonoCompositeAnimation(AnimationManager parent, JSON json) : base(parent, json)
    {
        updateAnimations();
    }

    /// <summary>
    ///     getDuration provides value of animation durations
    /// </summary>
    /// <returns>float of duration</returns>
    public float getDuration()
    {
        return duration;
    }

    /// <summary>
    ///     Creates inspector editor for MonoCompositeAnimation
    /// </summary>
    public override void drawMenu()
    {
        TimeLineAnimation anim;
        int i2 = 0;

        //Animation setting bar
        EditorGUILayout.BeginHorizontal();
        Animation newAnimation = creationMenu(ref menuIndex, parent.animationTemplates);
        if(newAnimation != null)
        {
            this.animations.Add(new TimeLineAnimation(newAnimation, this.getLastTime(), 0));
        }
        EditorGUILayout.EndHorizontal();

        //animation time line
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, true, false);
        GUIStyle style1 = new GUIStyle();
        style1.fixedHeight = 120;
        GUIStyle style2 = new GUIStyle();
        style2.fixedWidth = 300;
        EditorGUILayout.BeginHorizontal(style1);
        for(int i1 = 0; i1 < this.animations.Count; i1++)
        {
            anim = this.animations[i1];
            EditorGUILayout.BeginVertical(style2);

            //animation position and name
            EditorGUILayout.BeginHorizontal();
            i2 = clamp(EditorGUILayout.IntField(i1), this.animations.Count, 0);

            if (i1 != i2 && i1 != i2)
            {
                swap(i1, i2);
            }

            anim.anim.name = EditorGUILayout.TextField(anim.anim.name);

            EditorGUILayout.EndHorizontal();

            ///TODO
            /// - change animation type to another template

            //Duration
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Duration");
            anim.duration = EditorGUILayout.FloatField(anim.duration);
            if(anim.duration < 0)
            {
                anim.duration = 0;
            }
            EditorGUILayout.EndHorizontal();

            //Swapping animations with neighbors
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Swap Left") && i1 != 0)
            {
                swap(i1, i1 - 1);
            }
            else if(GUILayout.Button("Swap Right") && i1 != this.animations.Count - 1)
            {
                swap(i1, i1 + 1);
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Delete"))
            {
                this.animations.RemoveAt(i1);
                break;
            }

            //Edit animation
            if(GUILayout.Button("Edit"))
            {
                editIndex = i1;
            }

            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();

        if (0 <= editIndex && editIndex < this.animations.Count)
        {
            if(GUILayout.Button("close"))
            {
                editIndex = -1;
            }
            else
            {
                this.animations[editIndex].drawMenu();
            }
        }

        if(GUI.changed)
        {
            updateAnimations();
        }
    }

    /// <summary>
    ///     Method close timeline animation editor
    /// </summary>
    public void closeEdit()
    {
        editIndex = -1;
    }
    
    /// <summary>
    ///     GetEditIndex returns the index of animation on timeline that is currently being edited
    /// </summary>
    /// <returns>int of index of animation that is being edited or -1 if no animation is being edited</returns>
    public int getEditIndex()
    {
        return editIndex;
    }

    /// <summary>
    ///     Utility method that swaps animations on timeline
    /// </summary>
    /// <param name="i1">index 1 on animation</param>
    /// <param name="i2">index 2 on animation</param>
    private void swap(int i1, int i2)
    {
        TimeLineAnimation tmpAnim = this.animations[i1];

        this.animations[i1] = this.animations[i2];

        this.animations[i2] = tmpAnim;
    }

    /// <summary>
    ///     Utility method that clamp the value of input between min and max
    /// </summary>
    /// <param name="i1"></param>
    /// <param name="max"></param>
    /// <param name="min"></param>
    /// <returns>clamped value of i1</returns>
    private int clamp(int i1, int max, int min)
    {
        if(i1 < min)
        {
            return min;
        }
        if(i1 > max)
        {
            return max;
        }

        return i1;
    }

    /// <summary>
    ///     Utility method that updates start position of each animation
    /// </summary>
    private void updateAnimations()
    {
        this.duration = 0;
        foreach(TimeLineAnimation anim in this.animations)
        {
            anim.start = duration;
            duration += anim.duration;
        }
    }

    /// <summary>
    ///     Utility method returns index of animation that contains float t between start and end
    /// </summary>
    /// <param name="t">time stamp</param>
    /// <returns>index of animation</returns>
    private int getAnimation(float t)
    {
        if(this.animations.Count == 0)
        {
            return -1;
        }

        int pointer = 0;
        int min, max;
        TimeLineAnimation anim;
        min = 0;
        max = animations.Count - 1;

        if (t < 0)
        {
            return min;
        }
        else if (this.duration < t)
        {
            return max;
        }

        while (max - min > 1)
        {
            pointer = min + (max - min) / 2;
            
            anim = this.animations[pointer];

            if (anim.start <= t && t <= anim.start + anim.duration)
            {
                return pointer;
            }

            if(t < anim.start)
            {
                max = pointer;
            }
            else
            {
                min = pointer;
            }
        }
        anim = this.animations[min];
        if (anim.start <= t && t <= anim.start + anim.duration)
        {
            return min;
        }
        return max;
    }

    /// <summary>
    ///     animate method plays animations on timeline
    /// </summary>
    /// <param name="t">time</param>
    public override void animate(float t)
    {
        if(t < 0 && lastT < 0)
        {
            return;
        }
        else if(t > 1 && t > lastT)
        {
            return;
        }

        float time = Mathf.Clamp(t, 0, 1) * this.duration;
        int index = getAnimation(time);

        if(index == -1)
        {
            throw new Exception();
        }

        TimeLineAnimation anim = this.animations[index];

        if(lastAnim < index)
        {
            for(int i1 = lastAnim; i1 < index; i1++)
            {
                this.animations[i1].animate(1);
            }
        }
        else if(lastAnim > index)
        {
            for(int i1 = lastAnim; i1 >= index; i1--)
            {
                this.animations[i1].animate(0);
            }
        }

        time -= anim.start;
        time /= anim.duration;

        anim.animate(time);

        lastT = t;
        lastAnim = index;
    }

    /// <summary>
    ///     Method creates deep copy of MonOCompositeAnimation
    /// </summary>
    /// <returns>deep copy of object</returns>
    public override Animation Clone()
    {
        MonoCompositeAnimation tmp = new MonoCompositeAnimation(this.parent);

        foreach(TimeLineAnimation a in this.animations)
        {
            tmp.animations.Add(a.Clone() as TimeLineAnimation);
        }

        tmp.duration = this.duration;
        tmp.name = this.name;
        return tmp;
    }

    /// <summary>
    ///     utility method used to find the last time value
    /// </summary>
    /// <returns>float of last time</returns>
    private float getLastTime()
    {
        if(this.animations.Count == 0)
        {
            return 0;
        }
        TimeLineAnimation tmp = this.animations[this.animations.Count - 1];
        return tmp.start + tmp.duration;
    }

    /// <summary>
    ///     getJsonData returns a JSON representation of object
    /// </summary>
    /// <returns>string JSON representation of object</returns>
    public override string getJsonData()
    {
        return
            $"\"type\":\"{animationType}\","+
            $"{base.getJsonData()}";
    }
}
