using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class CompositeAnimation<A> : Animation where A : Animation 
{
    protected List<A> animations = new List<A>();
    protected AnimationManager parent;
    public CompositeAnimation(AnimationManager parent)
    {
        this.parent = parent;
    }

    public CompositeAnimation(AnimationManager parent, JSON json) : base(json)
    {
        this.parent = parent;

        JSON animationsTmp = json.getVal("animations");
        for(int i1 = 0; i1 < animationsTmp.getCount(); i1++)
        {
            animations.Add((A)Animation.load(parent, animationsTmp.getVal(i1)));
        }
    }

    public override void drawMenu()
    {
    }

    public override string getJsonData()
    {
        return 
                $"{base.getJsonData()},"+
                $"\"animations\":{animationsJson()}";
    }
    private string animationsJson()
    {
        string tmp = "[";

        foreach(Animation a in animations)
        {
            tmp += $"{{{a.getJsonData()}}},";
        }
        tmp = tmp.Trim(new char[] { ',' });
        tmp += "]";
        return tmp;
    }
    protected Animation creationMenu(ref int i1, Animation[] animationList)
    {
        List<string> str = new List<string>();

        for (int i2 = 0; i2 < animationList.Length; i2++)
        {
            if ($"{animationList[i2].ToString()}:{animationList[i2].name}" != $"{this.ToString()}:{this.name}")
            {
                str.Add($"{i2}:{animationList[i2].ToString()}:{animationList[i2].name}");
            }
        }

        EditorGUILayout.BeginHorizontal();
        i1 = EditorGUILayout.Popup(i1, str.ToArray());
        if (GUILayout.Button("Create"))
        {
            return animationList[i1].Clone();
        }
        EditorGUILayout.EndHorizontal();
        return null;
    }
}