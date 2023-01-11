using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net;

public class Rendering1 : MonoBehaviour
{
    public DateTime download1;
    public DateTime download2;
    public DateTime display_time2;
    // Start is called before the first frame update
    void Start()
    {
        byte[] wwwdata = null;
        string url = "http://XXXX/ply_dataset/x.ply";//binary ply path
        while (true)
        {
            try
            {
               download1 = DateTime.Now;
                WebRequest req = WebRequest.Create(url);
                using (WebResponse res = req.GetResponse())
                {
                    using (Stream st = res.GetResponseStream())
                    {
                        wwwdata = MyClass.ReadBinaryData(st);
                        if (wwwdata != null)
                        {
                            download2 = DateTime.Now;
                            break;
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    Debug.Log(ex.Message);
                }
                else
                {
                    Debug.Log(ex.Message);
                }
            }
        }

        Stream stream = new MemoryStream(wwwdata);
        var gameObject = new GameObject();
        gameObject.name = "frame_chair";

        var mesh = MyClass.ImportAsMesh(stream);
        var meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;
        var meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = MyClass.GetDefaultMaterial();

        gameObject.transform.position = new Vector3(0, 0, 0.5f);
        gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
        gameObject.transform.localScale = new Vector3(1, 1, 1);
        display_time2 = DateTime.Now;
        
        TimeSpan download_ts = download2 - download1;
        Debug.Log(download_ts + ":download_ts");
        TimeSpan display_ts = display_time2 - download2;
        Debug.Log(display_ts + ":display_ts");
    }
}
