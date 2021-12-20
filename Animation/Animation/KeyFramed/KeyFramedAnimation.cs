using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class KeyFrame : IComparable<KeyFrame>
{
    public bool visible = false;
    protected float timeStamp;
    protected AnimationCurve motionCurve;

    public KeyFrame()
    {
        timeStamp = 0;
        motionCurve = AnimationCurve.Linear(0, 0, 1, 1);
    }

    public KeyFrame(JSON json)
    {
        timeStamp = float.Parse(json.getVal("timeStamp").getVal());


        UnityEngine.Keyframe[] keyframes = new UnityEngine.Keyframe[json.getVal("motionCurve").getCount()];
        JSON tmp;
        for (int i1 = 0; i1 < json.getVal("motionCurve").getCount(); i1++)
        {
            tmp = json.getVal("motionCurve").getVal(i1);
            keyframes[i1] = new UnityEngine.Keyframe
                (
                    float.Parse(tmp.getVal("time").getVal()),
                    float.Parse(tmp.getVal("value").getVal()),
                    float.Parse(tmp.getVal("inTangent").getVal()),
                    float.Parse(tmp.getVal("outTangent").getVal()),
                    int.Parse(tmp.getVal("inWeight").getVal()),
                    int.Parse(tmp.getVal("outWeight").getVal())
                );
        }

        motionCurve = new AnimationCurve(keyframes);
    }

    //getters
    public void setTimeStamp(float timeStamp)
    {
        if (timeStamp < 0)
        {
            this.timeStamp = 0;
        }
        else if (timeStamp > 1)
        {
            this.timeStamp = 1;
        }
        else
        {
            this.timeStamp = timeStamp;
        }
    }
    public float getTimeStamp()
    {
        return this.timeStamp;
    }

    public virtual string getJsonData()
    {
        string tmp = "";
        tmp += $"\"timeStamp\":\"{timeStamp}\",";

        tmp += "\"motionCurve\":[";
        foreach (UnityEngine.Keyframe key in motionCurve.keys)
        {
            tmp += $"{{" +
                        $"\"inTangent\":\"{key.inTangent}\"," +
                        $"\"inWeight\":\"{key.inWeight}\"," +
                        $"\"outTangent\":\"{key.outTangent}\"," +
                        $"\"outWeight\":\"{key.outWeight}\"," +
                        $"\"time\":\"{key.time}\"," +
                        $"\"value\":\"{key.value}\"" +
                    $"}},";
        }
        tmp = $"{tmp.TrimEnd(new char[] { ',' })}]";
        return tmp;
    }
    
    public int CompareTo(KeyFrame other)
    {
        if (other == null)
        {
            return 1;
        }
        return this.timeStamp.CompareTo(other.timeStamp);
    }

    public virtual KeyFrame Clone()
    {
        KeyFrame tmp = new KeyFrame();

        tmp.timeStamp = this.timeStamp;
        tmp.motionCurve = new AnimationCurve((Keyframe[])motionCurve.keys.Clone());

        return tmp;
    }

    public virtual void drawKeyFrame()
    {
        timeStamp = Mathf.Clamp(EditorGUILayout.FloatField("Timestamp", timeStamp), 0, 1);
        motionCurve = EditorGUILayout.CurveField(motionCurve);
    }
}

public abstract class KeyFramedAnimation<K> : Animation where K : KeyFrame 
{
    [SerializeField]
    protected List<K> keyFrames = new List<K>();

    public KeyFramedAnimation() :base()
    {
        addKeyFrame();
        addKeyFrame();
    }
    public KeyFramedAnimation(JSON json) : base(json)
    {
    }

    override public void drawMenu()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add KeyFrame"))
        {
            addKeyFrame();
        }
        else if (GUILayout.Button("Remove KeyFrame"))
        {
            if (keyFrames.Count > 0)
            {
                keyFrames.RemoveAt(keyFrames.Count - 1);
            }
        }
        GUILayout.EndHorizontal();
    }

    protected string getKeyFrames()
    {
        string tmp = $"[";

        foreach (K keyframe in keyFrames)
        {
            tmp += $"{keyframe.getJsonData()},";
        }
        tmp = tmp.TrimEnd(new char[] { ',' });
        tmp += $"]";

        return tmp;
    }

    protected int binarySearch(float pos)
    {
        if (pos < 0 || pos > 1)
        {
            return -1;
        }

        if (keyFrames.Count < 2)
        {
            return -1;
        }

        int pointer = 0;
        int min, max;

        min = 0;
        max = keyFrames.Count - 1;

        if (pos <= keyFrames[min].getTimeStamp())//pos is min point
        {
            return min;
        }
        else if (keyFrames[max].getTimeStamp() <= pos)//pos is max point
        {
            return max - 1;
        }

        while (max - min > 1)
        {
            pointer = min + (max - min) / 2;
            if (pos == keyFrames[pointer].getTimeStamp())//pos is center
            {
                return pointer;
            }
            else if (pos < keyFrames[pointer].getTimeStamp())//pos is less than center
            {
                max = pointer;
            }
            else if (pos > keyFrames[pointer].getTimeStamp())//pos is greater than center
            {
                min = pointer;
            }
        }
        pointer = min + (max - min) / 2;
        return pointer;
    }
    public abstract void addKeyFrame();
    public void orderKeyFrames()
    {
        keyFrames.Sort();
        keyFrames[0].setTimeStamp(0);
        keyFrames[keyFrames.Count - 1].setTimeStamp(1);
    }
}