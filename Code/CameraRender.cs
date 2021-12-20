using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraRender : MonoBehaviour
{
    [SerializeField]
    private Camera cam;
    [SerializeField]
    public int fps = 60;
    [SerializeField]
    public int totalFrames = 60;

    [SerializeField]
    private Vector2Int res = new Vector2Int(1080, 960);

    [SerializeField]
    public int frame;
    private void Start()
    {
        string filePath = $"Assets/Scenes/{SceneManager.GetActiveScene().name}/Render";
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        frame = 0;
    }


    public void LateUpdate()
    {
        if(frame <= totalFrames)
        {
            AnimationManager[] managers = FindObjectsOfType<AnimationManager>();
            foreach (AnimationManager manager in managers)
            {
                takeScreenShot();
            }
            frame++;
        }
        else
        {
            Application.Quit();
        }
    }

    private void takeScreenShot()
    {
        string filePath = $"Assets/Scenes/{SceneManager.GetActiveScene().name}/Render";
        string fileName = frame.ToString();
        int size = totalFrames.ToString().Length;
        while(fileName.Length < size)
        {
            fileName = "0"+ fileName;
        }

        fileName = $"{filePath}/{fileName}.png";

        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        ScreenCapture.CaptureScreenshot(fileName);
    }
}
