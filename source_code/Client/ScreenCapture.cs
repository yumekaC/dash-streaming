using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System;
using System.Text.RegularExpressions;

[ExecuteInEditMode]
public class ScreenCapture
{
    [MenuItem("Tools/Screen Capture")]
    public static void Capture()
    {
        // Get game screen size
        var size = new Vector2Int((int)Handles.GetMainGameViewSize().x, (int)Handles.GetMainGameViewSize().y);
        var render = new RenderTexture(size.x, size.y, 24);
        var texture = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);
        var camera = Camera.main;
        //Debug.Log(cemara);
        try
        {
            // Rendering camera image to RenderTexture
            camera.targetTexture = render;
            camera.Render();

            // Reading image of RenderTexture
            RenderTexture.active = render;
            texture.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
            texture.Apply();
        }
        finally
        {
            camera.targetTexture = null;
            RenderTexture.active = null;
        }
        DateTime download1 = DateTime.Now;
        Debug.Log(download1.ToString());
        var re = new Regex("/");
        string replace = re.Replace(download1.ToString(), "-", 2);
        var re2 = new Regex(":");
        string replace2 = re2.Replace(replace, "-", 2);
        string filename = string.Format(@"point_cloud_" + replace2 + ".png");
        Debug.Log(filename);
        string address = Path.Combine(Application.persistentDataPath, filename);
        Debug.Log(address);
        // storing file as a PNG image
        File.WriteAllBytes(
            $"{Application.dataPath}/image.png",
            texture.EncodeToPNG());
        File.WriteAllBytes(
            $"{address}",
            texture.EncodeToPNG());
    }
}
