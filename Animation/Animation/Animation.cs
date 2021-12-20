using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[Serializable]
public abstract class Animation
{
    [SerializeField]
    public string name = "";

    public bool visible = false;

    public static string[] types = new string[] 
        {
            TransformAnimation.animationType,
            MonoCompositeAnimation.animationType,
            MultiCompositeAnimation.animationType
        };

    public static Animation create(AnimationManager parent, string type)
    {
        switch (type)
        {
            case TransformAnimation.animationType:
                return new TransformAnimation();
            case MonoCompositeAnimation.animationType:
                return new MonoCompositeAnimation(parent);
            case MultiCompositeAnimation.animationType:
                return new MultiCompositeAnimation(parent);
        }
        return null;
    }
    public static Animation load(AnimationManager parent, JSON data)
    {
        switch(data.getVal("type").getVal())
        {
            case TransformAnimation.animationType:
                return new TransformAnimation(data);
            case MonoCompositeAnimation.animationType:
                return new MonoCompositeAnimation(parent, data);
            case TimeLineAnimation.animationType:
                return new TimeLineAnimation(parent, data);
            case MultiCompositeAnimation.animationType:
                return new MultiCompositeAnimation(parent, data);
        }
        return null;
    }

    public static Animation creationMenu(ref int i1, AnimationManager parent)
    {
        EditorGUILayout.BeginHorizontal();
        i1 = EditorGUILayout.Popup(i1, Animation.types);
        if(GUILayout.Button("Create"))
        {
            return Animation.create(parent, Animation.types[i1]);
        }
        EditorGUILayout.EndHorizontal();
        return null;
    }
    
    protected static Vector3 loadVector3(JSON json)
    {
        return new Vector3
            (
                float.Parse(json.getVal("x").getVal()),
                float.Parse(json.getVal("y").getVal()),
                float.Parse(json.getVal("z").getVal())
            );
    }

    protected static Vector4 loadVector4(JSON json)
    {
        return new Vector4
            (
                float.Parse(json.getVal("x").getVal()),
                float.Parse(json.getVal("y").getVal()),
                float.Parse(json.getVal("z").getVal()),
                float.Parse(json.getVal("w").getVal())
            );
    }

    protected static string VectorToJSON(Vector3 v)
    {
        return 
            $"{{" +
                $"\"x\":\"{v.x}\"," +
                $"\"y\":\"{v.y},\"" +
                $"\"z\":\"{v.z}\"" +
            $"}}";
    }
    protected static string VectorToJSON(Vector4 v)
    {
        return
            $"{{" +
                $"\"x\":\"{v.x}\"," +
                $"\"y\":\"{v.y}\"," +
                $"\"z\":\"{v.z}\"," +
                $"\"w\":\"{v.w}\"" +
            $"}}";
    }
    public Animation()
    {

    }

    public Animation(JSON json)
    {
        this.name = json.getVal("name").getVal();
    }

    abstract public void animate(float t);
    abstract public void drawMenu();
    public abstract Animation Clone();

    /// TODO
    /// implenet getJsonData for duration
    /// get rid of outer curls from all implementaions of getJsonData
    public virtual string getJsonData()
    {
        return 
            $"\"name\":\"{name}\"";
    }
    public override string ToString()
    {
        return "Animation";
    }
}
