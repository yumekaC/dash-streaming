using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Threading;

public class ChangeDistance : MonoBehaviour
{
    public List<string> lists = new List<string>();
    public List<string> dist_lists = new List<string>();
    public static double distance = 0.5;
    public static float x_position = 0.5f;
    public static float y_position = 0;
    public static float z_position = 0;
    public static List<float> dist = new List<float>();
    public DateTimeOffset baseDt = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
    // Start is called before the first frame update
    void Start()
    {
        dist.Add(1.5f);
        dist.Add(2);
        dist.Add(2.5f);

        var unix_st = (DateTimeOffset.Now - baseDt).Ticks;
        string address = Path.Combine(Application.streamingAssetsPath, "distance.csv");//result8.csv
        StreamReader sr2 = new StreamReader(address);
        {
            while (!sr2.EndOfStream)
            {
                string line = sr2.ReadLine();
                string[] values = line.Split(',');
                dist_lists.AddRange(values);

            }
        }
        var unix_en = (DateTimeOffset.Now - baseDt).Ticks;
        long time_long = unix_en - unix_st;
        float time = (float)time_long / 10000000;
        Debug.Log(time);
        StartCoroutine(CreateCube());
    }

    // Update is called once per frame
    void Update()
    {

    }
    IEnumerator CreateCube()
    {
        for (int i = 0; i < 1001; i++)
        {
            float dis_0 = Single.Parse(dist_lists[i * 4 + 1]);
            float dis_1 = Single.Parse(dist_lists[i * 4 + 2]);
            float dis_2 = Single.Parse(dist_lists[i * 4 + 3]);
            dist[0] = dis_0;
            dist[1] = dis_1;
            dist[2] = dis_2;

            yield return new WaitForSeconds(0.1f);
        }
    }
}


