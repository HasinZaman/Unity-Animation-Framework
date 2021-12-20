using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class TransformAnimation : KeyFramedAnimation<TransformAnimation.TransformKeyFrame>
{

    public const string animationType = "TransformAnimation";

    Vector2 scrollPos = Vector2.zero;

    public GameObject target;
    public bool local = true;
    public bool absoluteStart = false;
    public TYPE type = TYPE.Position;

    bool startPosSet = false;

    /// <summary>
    ///     TYPE enum defines the types of transformations TransformAnimation can modify
    /// </summary>
    public enum TYPE
    {
        Position,
        Rotation,
        Scale
    }

    /// <summary>
    ///     TransformKeyFrame stores vector of transformed component (rotation, scale, position) 
    /// </summary>
    public class TransformKeyFrame : KeyFrame
    {
        public Vector4 vector = Vector4.zero;

        private TransformAnimation parent;

        /// <summary>
        ///     Constructor Creates TransformKeyFrame object
        /// </summary>
        /// <param name="vector">intial vector</param>
        /// <param name="parent">TransformAnimation parent that instantiated object</param>
        public TransformKeyFrame(Vector4 vector, TransformAnimation parent)
        {
            this.vector = vector;
            this.parent = parent;
        }

        /// <summary>
        ///     Constructor Creates TransformKeyFrame object
        /// </summary>
        /// <param name="json">json object of TransformKeyFrame</param>
        /// <param name="parent">TransformAnimation parent that instantiated object</param>
        public TransformKeyFrame(JSON json, TransformAnimation parent) : base(json)
        {
            vector = Animation.loadVector4(json.getVal("vector"));

            this.parent = parent;
        }

        /// <summary>
        ///     getJsonData returns string of JSON representation of object
        /// </summary>
        /// <returns>String representation of object</returns>
        public override string getJsonData()
        {
            string tmp = "";

            tmp += $"\"vector\":{VectorToJSON(vector)}";

            return $"{{{tmp},{base.getJsonData().Trim(new char[] { '{', '}' })}}}";
        }

        public override KeyFrame Clone()
        {
            TransformKeyFrame tmp = new TransformKeyFrame(this.vector, this.parent);
            tmp.timeStamp = this.timeStamp;
            tmp.motionCurve = new AnimationCurve((Keyframe[])motionCurve.keys.Clone());

            return tmp;
        }

        /// <summary>
        ///     DrawKeyFrame of object in inspector
        /// </summary>
        public override void drawKeyFrame()
        {
            base.drawKeyFrame();

            EditorGUILayout.BeginHorizontal();
            switch(parent.type)
            {
                case TYPE.Position:
                case TYPE.Scale:
                    vector = EditorGUILayout.Vector3Field(GUIContent.none, vector);
                    break;
                case TYPE.Rotation:
                    vector = EditorGUILayout.Vector4Field(GUIContent.none, vector);
                    break;
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Update KeyFrame"))
            {
                switch(parent.type)
                {
                    case TYPE.Position:
                        if(parent.local)
                        {
                            vector = parent.target.transform.localPosition;
                        }
                        else
                        {
                            vector = parent.target.transform.position;
                        }
                        break;
                    case TYPE.Rotation:
                        if (parent.local)
                        {
                            vector = new Vector4
                                (
                                    parent.target.transform.localRotation.x,
                                    parent.target.transform.localRotation.y,
                                    parent.target.transform.localRotation.z,
                                    parent.target.transform.localRotation.w
                                );
                        }
                        else
                        {
                            vector = new Vector4
                                (
                                    parent.target.transform.rotation.x,
                                    parent.target.transform.rotation.y,
                                    parent.target.transform.rotation.z,
                                    parent.target.transform.rotation.w
                                );
                        }
                        break;
                    case TYPE.Scale:
                        if (parent.local)
                        {
                            vector = parent.target.transform.localScale;
                        }
                        else
                        {
                            vector = parent.target.transform.localScale;
                        }
                        break;
                }
            }
        }

        /// <summary>
        ///     interpolate method interpolates between current keyframe and next key frame
        /// </summary>
        /// <param name="k">next keyframe</param>
        /// <param name="t">current time value of TransformAnimation</param>
        /// <returns>Interpolated Vector4</returns>
        public Vector4 interpolate(TransformKeyFrame k, float t)
        {
            Vector4 v;
            float dist = k.getTimeStamp() - this.timeStamp;
            v = (k.vector - this.vector);

            return this.vector + v * motionCurve.Evaluate((t - timeStamp)/dist);
        }
    }
    public TransformAnimation() : base()
    {
    }

    /// <summary>
    ///     TransformAnimation constructor created from JSON
    /// </summary>
    /// <param name="data">JSON representation of TransformAnimation</param>
    public TransformAnimation(JSON data) : base(data)
    {
        if(data.getVal("type").getVal() != typeof(TransformAnimation).ToString())
        {
            throw new Exception("Invalid type");
        }

        JSON tmp;

        //set target
        UnityEngine.Transform transform = null;
        if(data.getVal("target").type == typeof(string).ToString())
        {
            if(data.getVal("target").getVal() == "NULL")
            {
                this.target = null;

            }
        }
        else
        {
            for (int i1 = 0; i1 < data.getVal("target").getCount(); i1++)
            {
                tmp = data.getVal("target").getVal(i1);
                switch (tmp.getVal("type").getVal())
                {
                    case "ManagerID":
                        transform = AnimationManager.findManager(tmp.getVal("id").getVal()).transform;
                        break;
                    case "child":
                        if (transform != null)
                        {
                            transform = transform.GetChild(int.Parse(tmp.getVal("id").getVal()));
                        }
                        else
                        {
                            throw new Exception("Invalid target structure");
                        }
                        break;
                }
            }
            this.target = transform.gameObject;
        }

        //getting animation type
        string s1 = data.getVal("transformType").ToString().Trim(new char[] { '\"' });
        if (s1 == TYPE.Position.ToString())
        {
            this.type = TYPE.Position;
        }
        else if(s1 == TYPE.Rotation.ToString())
        {
            this.type = TYPE.Rotation;
        }
        else if(s1 == TYPE.Scale.ToString())
        {
            this.type = TYPE.Scale;
        }
        else
        {
            throw new ArgumentException();
        }

        absoluteStart = bool.Parse(data.getVal("absoluteStart").getVal());

        //setting up key frames
        for (int i1 = 0; i1 < data.getVal("keyFrames").getCount(); i1++)
        {
            keyFrames.Add(new TransformKeyFrame(data.getVal("keyFrames").getVal(i1), this));
        }
    }

    /// <summary>
    ///     drawMenu of Animation object in inspector
    /// </summary>
    public override void drawMenu()
    {
        EditorGUILayout.BeginHorizontal();

        visible = EditorGUILayout.Foldout(visible, GUIContent.none);
        target = EditorGUILayout.ObjectField(target, typeof(GameObject)) as GameObject;
        type = (TYPE) EditorGUILayout.EnumPopup(type);
        local = EditorGUILayout.Toggle("Local", local);

        EditorGUILayout.EndHorizontal();

        absoluteStart = EditorGUILayout.Toggle("Absolute Start", absoluteStart);

        if (visible)
        {
            base.drawMenu();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, true, false);
            GUIStyle style = new GUIStyle();
            style.fixedHeight = 110;
            EditorGUILayout.BeginHorizontal(style);
            foreach (TransformKeyFrame kf in keyFrames)
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(100));
                EditorGUILayout.BeginHorizontal();
                
                if(GUILayout.Button("Delete Keyframe"))
                {
                    keyFrames.Remove(kf);
                    break;
                }
                
                EditorGUILayout.EndHorizontal();
                kf.drawKeyFrame();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();

            if(GUI.changed)
            {
                orderKeyFrames();
            }
        }
    }

    /// <summary>
    ///     String representation of TransformAnimation
    /// </summary>
    /// <returns>String representation of TransformAnimation</returns>
    public override string ToString()
    {
        return typeof(TransformAnimation).ToString();
    }

    /// <summary>
    ///     Clone method creates a copy of TransformAnimation
    /// </summary>
    /// <returns>Copy of object</returns>
    public override Animation Clone()
    {
        TransformAnimation tmp = new TransformAnimation();

        foreach(TransformKeyFrame key in this.keyFrames)
        {
            tmp.keyFrames.Add(key.Clone() as TransformKeyFrame);
        }
        tmp.target = this.target;
        tmp.local = this.local;
        tmp.absoluteStart = this.absoluteStart;
        tmp.type = this.type;
        tmp.startPosSet = this.startPosSet;

        tmp.name = this.name;

        return tmp;
    }

    /// <summary>
    ///     getJsonData returns a JSON representation of object
    /// </summary>
    /// <returns>string JSON representation of object</returns>
    public override string getJsonData()
    {
        string tmp =
                $"{base.getJsonData()}," +
                $"\"type\":\"{TransformAnimation.animationType}\"," +
                $"\"target\":{getTargetID()}," +
                $"\"transformType\":\"{this.type}\"," +
                $"\"absoluteStart\":\"{absoluteStart.ToString()}\"," +
                $"\"keyFrames\":{getKeyFrames()}";

        return tmp;
    }

    /// <summary>
    ///     getTargetID returns a path on the object hierarchy in the scene
    /// </summary>
    /// <returns>String representation of target path on in object hierarchy</returns>
    private string getTargetID()
    {
        if (target == null)
        {
            return "\"NULL\"";
        }

        UnityEngine.Transform pointer = target.transform;
        string pathId = "";
        while(!(target.GetComponent<AnimationManager>() != null || pointer.parent == null))
        {
            //add to path
            for(int i1 = 0; i1 < pointer.parent.childCount; i1++)
            {
                if(pointer.parent.GetChild(i1).transform == pointer)
                {
                    pathId = $"{{" +
                        $"\"type\":\"child\"," +
                        $"\"id\":\"{i1}\"" +
                        $"}}," +
                    $"{pathId}";
                    break;
                }
            }    
            
            pointer = pointer.transform.parent;
        }

        if(pointer.GetComponent<AnimationManager>() != null)
        {
            string animationManagerId = pointer.GetComponent<AnimationManager>().getId();
            pathId = $"{{" +
                    $"\"type\":\"ManagerID\"," +
                    $"\"id\":\"{animationManagerId}\"" +
                    $"}}," +
                $"{pathId}";
        }
        else
        {
            pathId = $"{{" +
                    $"\"type\":\"name\"," +
                    $"\"id\":\"{pointer.transform.name}\"" +
                    $"}}," +
                $"{pathId}";
        }
        pathId = pathId.Substring(0, pathId.Length - 1);
        return $"[{pathId}]";
    }

    /// <summary>
    ///     addKeyFrame method adds a keyFrame to Animation
    /// </summary>
    public override void addKeyFrame()
    {
        if(target != null)
        {
            keyFrames.Add
                (
                    new TransformKeyFrame
                    (
                        Vector3.zero,
                        this
                    )
                );
        }
    }

    /// <summary>
    ///     animate method updates target based on time value of animation
    /// </summary>
    /// <param name="t">time</param>
    public override void animate(float t)
    {
        if(keyFrames.Count < 2)
        {
            throw new Exception("Invalid State");
        }

        Vector4 v1;
        TransformKeyFrame startFrame, endFrame;
        int midPoint = binarySearch(t);

        //check if start pos is absolute to key frame or realtive to current val
        startFrame = keyFrames[midPoint] as TransformKeyFrame;
        endFrame = keyFrames[midPoint + 1] as TransformKeyFrame;
        if (midPoint == 0)
        {
            if (!absoluteStart && !startPosSet)
            {
                startPosSet = true;
                switch (type)
                {
                    case TYPE.Position:
                        if (local)
                        {
                            startFrame.vector = target.transform.localPosition;
                        }
                        else
                        {
                            startFrame.vector = target.transform.position;
                        }
                        break;
                    case TYPE.Rotation:
                        if (local)
                        {
                            startFrame.vector = new Vector4
                                (
                                    target.transform.localRotation.x,
                                    target.transform.localRotation.y,
                                    target.transform.localRotation.z,
                                    target.transform.localRotation.w
                                );
                        }
                        else
                        {
                            startFrame.vector = new Vector4
                                (
                                    target.transform.rotation.x,
                                    target.transform.rotation.y,
                                    target.transform.rotation.z,
                                    target.transform.rotation.w
                                );
                        }
                        break;
                    case TYPE.Scale:
                        startFrame.vector = target.transform.localScale;
                        break;
                }
            }
        }

        //get midpoint from start and end

        v1 = startFrame.interpolate
            (
                endFrame,
                t
            );
        //apply vector to animation
        switch (type)
        {
            case TYPE.Position:
                if(local)
                {
                    target.transform.localPosition = v1;
                }
                else
                {
                    target.transform.position = v1;
                }
                break;
            case TYPE.Rotation:
                if (local)
                {
                    target.transform.localRotation = new Quaternion(v1.x, v1.y, v1.z, v1.w);
                }
                else
                {
                    target.transform.rotation = new Quaternion(v1.x, v1.y, v1.z, v1.w);
                }
                break;
            case TYPE.Scale:
                target.transform.localScale = v1;
                break;
        }
    }
}
