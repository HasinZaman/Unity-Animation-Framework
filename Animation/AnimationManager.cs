using UnityEngine;
using System;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[ExecuteInEditMode]
public class AnimationManager : MonoBehaviour
{
    public static string  defaultID = "-1";
    [SerializeField]
    private string id = AnimationManager.defaultID;
    public Animation[] animationTemplates = new Animation[] { };
    public MultiCompositeAnimation animation = null;
    public int startFrame = 0;
    public int duration = 0;

    public static AnimationManager findManager(string id)
    {
        AnimationManager[] managers = FindObjectsOfType<AnimationManager>();
        for(int i1 = 0; i1 < managers.Length; i1++)
        {
            if(managers[i1].id == id)
            {
                return managers[i1];
            }
        }
        return null;
    }

    public void OnEnable()
    {
        if(id == AnimationManager.defaultID)
        {
            id = $"{this.transform.GetInstanceID()}";
        }

        loadAnimation();
        if(animation == null)
        {
            animation = new MultiCompositeAnimation(this);
        }
    }

    public void Start()
    {
        if (id == AnimationManager.defaultID)
        {
            id = $"{this.transform.GetInstanceID()}";
        }

        loadAnimation();
        if (animation == null)
        {
            animation = new MultiCompositeAnimation(this);
        }
    }

    public void FixedUpdate()
    {
        CameraRender cam = FindObjectOfType<CameraRender>();
        if (cam.frame <= cam.totalFrames)
        {
            AnimationManager[] managers = FindObjectsOfType<AnimationManager>();
            foreach (AnimationManager manager in managers)
            {
                manager.animate(cam.frame);
            }
        }
    }
    public string getInstanceID()
    {
        return $"{this.transform.GetInstanceID()}";
    }
    public string getId()
    {
        return id;
    }

    public void saveAnimations()
    {
        if(Application.isPlaying)
        {
            return;
        }

        string filePath = $"Assets/Scenes/{SceneManager.GetActiveScene().name}/Animation/";
        string fileName = $"{filePath}/{this.transform.name}{id}.json";
        string fileContent;
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        fileContent = 
            $"{{\n" +
                $"{arrayToJson(animationTemplates, "animationTemplates")},\n" +
                $"\"animation\":{{{animation.getJsonData()}}},\n" +
                $"\"startFrame\":\"{startFrame}\",\n" +
                $"\"duration\":\"{duration}\"\n" +
            $"}}";

        File.WriteAllText(fileName, fileContent);
    }
    public void loadAnimation()
    {
        string filePath = $"Assets/Scenes/{SceneManager.GetActiveScene().name}/Animation/";
        string fileName = $"{filePath}/{this.transform.name}{id}.json";
        
        if (!Directory.Exists(filePath))
        {
            return;
        }

        if(!File.Exists(fileName))
        {
            return;
        }

        //load file
        JSON json = JSON.parse(File.ReadAllText(fileName));
        JSON tmp;

        List<Animation> animationList = new List<Animation>();

        //AT => AnimationTemplateJson
        for (int i1 = 0; i1 < json.getVal("animationTemplates").getCount(); i1++)
        {
            tmp = json.getVal("animationTemplates").getVal(i1);

            animationList.Add(Animation.load(this, tmp));
        }

        animationTemplates = animationList.ToArray();

        animationList.Clear();

        if(json.contains("animation"))
        {
            Animation tmpAnim = Animation.load(this, json.getVal("animation"));
            if (tmpAnim is MultiCompositeAnimation)
            {
                animation = tmpAnim as MultiCompositeAnimation;
            }
            else
            {
                animation = new MultiCompositeAnimation(this);
            }
        }
        else
        {
            animation = new MultiCompositeAnimation(this);
        }
        
        startFrame = int.Parse(json.getVal("startFrame").getVal());
        duration = int.Parse(json.getVal("duration").getVal());
}

    private string arrayToJson(Animation[] array, string name)
    {
        string tmp = "";

        for(int i1 = 0; i1 < array.Length; i1++)
        {
            tmp += $"\t{{{array[i1].getJsonData()}}},";
        }
        tmp = tmp.Trim(new char[] { ',' });
        
        return $"\"{name}\":[{tmp}]";
    }

    private int lastFrame = 0;
    public void animate(int frame)
    {
        if(frame < startFrame || startFrame + duration < frame)
        {
            return;
        }
        
        animation.animate((float)(frame - startFrame) / (float)duration);
    }
    
}
