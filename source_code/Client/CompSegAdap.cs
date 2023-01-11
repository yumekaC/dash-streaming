using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class CompSegAdap : MonoBehaviour
{
    public const int MAX_QUE = 10000; //【the number of frames】295/305/165
    public List<Queue<string>> queues = new List<Queue<string>>();
    public bool runningFlag = false;//true;
    public int k = 0;
    public int ex = 0;
    public int dequeue_wait_count = 0;
    public int enqueue_wait_count = 0;
    public float dl_time;

    public int fps = 30;//【playing frame rate】30/15/10/5
    public int cur_num = 0;
    public int airtap_count = 0;
    public List<string> contents_name_list = new List<string>();

    /*initialize content info model*/
    public ContentInfoModel contentInfoModel = new ContentInfoModel();
    public PositionInfo positionInfo = new PositionInfo();
    public List<ContentInfoModel> content_info_list = new List<ContentInfoModel>();
    public int choose_content = 0;//=>for rendering
    public List<int> intervals = new List<int>();

    public double as_of_throughput;
    public int as_of_all_frames = 164;

    /*for playing at 30fps*/
    public int dl_count = 1;
    public int rd_num = 1;

    /*store data*/
    //public List<StoreData2> storeDatas2 = new List<StoreData2>();
    public List<long> download_start_list = new List<long>();
    public List<long> render_start_list = new List<long>();
    public List<List<StoreData1>> storeDatas_list = new List<List<StoreData1>>();
    public List<RenderingLog> renderingLog_list = new List<RenderingLog>();
    public DateTimeOffset baseDt = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
    public long startTime = 0;
    public List<StoreData3> storeDatas3 = new List<StoreData3>();

    /*pqs*/
    public string pqs = "10";
    /*seg metadata.json*/
    //public List<AveSegMetadata> AveSegMetadatas = new List<AveSegMetadata>();
    public List<AveMetadata> AveMetadatas = new List<AveMetadata>();

    /// <summary>
    /// Allows gestures recognition in HoloLens
    /// </summary>
    private GestureRecognizer recognizer;

    // Start is called before the first frame update
    void Start()
    {
        string sequence1 = "greeting";
        string sequence2 = "telecon";
        string sequence3 = "slab_chair";
        string sequence4 = "racecar";
        string sequence5 = "ted";
        string sequence6 = "spool";

        contents_name_list.Add(sequence4);
        contents_name_list.Add(sequence3);
        contents_name_list.Add(sequence6);
        contents_name_list.Add(sequence5);
        contents_name_list.Add(sequence1);
        contents_name_list.Add(sequence2);

        intervals.Add(0);
        intervals.Add(10);//3000
        intervals.Add(10);//3000
        intervals.Add(10);//3000
        intervals.Add(10);//3000
        intervals.Add(10);//3000
        intervals.Add(10);//3000
        startTime = (DateTimeOffset.Now - baseDt).Ticks;
        for (int i = 0; i < 6; i++)
        {
            Thread.Sleep(intervals[i]);
            TapHandler();
        }
        // Subscribing to the Microsoft HoloLens API gesture recognizer to track user gestures
        /*recognizer = new GestureRecognizer();
        recognizer.SetRecognizableGestures(GestureSettings.Tap);
        recognizer.Tapped += TapHandler;
        recognizer.StartCapturingGestures();*/

    }
    double Metaget(string bin_content_name)
    {
        string seg_meta_url = "http://XXXX/ply_dataset/bin_seg/" + bin_content_name + "/ave_metadata.json";
        float meta_throughput;
        while (true)
        {
            try
            {
                var unix_meta_st = (DateTimeOffset.Now - baseDt).Ticks;
                WebRequest req = WebRequest.Create(seg_meta_url);
                using (WebResponse res = req.GetResponse())
                {
                    using (Stream st = res.GetResponseStream())
                    {
                        StreamReader sr = new StreamReader(st, Encoding.GetEncoding("UTF-8"));
                        string txt = sr.ReadToEnd();
                        //deserialize
                        AveMetadata aveMetadata = JsonUtility.FromJson<AveMetadata>(txt);
                        AveMetadatas.Add(aveMetadata);
                        int num_of_seg = aveMetadata.num_of_seg;
                        int rate_control = aveMetadata.rate_control;
                        Debug.Log("rate_control:" + rate_control);
                        Debug.Log(bin_content_name + "-num of seg:" + num_of_seg);
                        var unix_meta_en = (DateTimeOffset.Now - baseDt).Ticks;
                        long meta_time_long = unix_meta_en - unix_meta_st;
                        float meta_time = (float)meta_time_long / 10000000;
                        int meta_size_bit = txt.Length * 8;
                        float meta_size_Mbit = (float)meta_size_bit / 1000000;
                        meta_throughput = meta_size_Mbit / meta_time;
                        break;
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
        if (meta_throughput > 0)
        {
            return meta_throughput;
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// Respond to Tap Input.
    /// </summary>
    //private void TapHandler(TappedEventArgs obj)
    private void TapHandler()//unity debug
    {
        airtap_count++;
        positionInfo = new PositionInfo //unity debug
        {
            x_pos = airtap_count - 1,
            y_pos = airtap_count - 1,
            z_pos = airtap_count - 1,
            x_rot = airtap_count - 1,
            y_rot = airtap_count - 1,
            z_rot = airtap_count - 1
        };
        double meta_throughput = Metaget(contents_name_list[airtap_count - 1]);
        contentInfoModel = new ContentInfoModel
        {
            id = airtap_count - 1,
            name = contents_name_list[airtap_count - 1],
            num_of_frames = AveMetadatas[airtap_count - 1].num_of_seg,
            fps = AveMetadatas[airtap_count - 1].frame_rate,
            position = positionInfo,
            download_flag = true,
            rendering_flag = false,
            next_frame = 0,
            as_of_throughput = meta_throughput,
            choosed_pqs = "3",//"10"
            buffer_count = 0,
            rendering_frame = 0,
            deque_data = new List<byte[]>(),
            rendering_segnum = 0,
            rendering_pqs = "10",
            rendering_timestamp = "",
            rendering_unixtime = 0,
            frame_num_all = 0,
            log_rendring_flag = false
        };
        content_info_list.Add(contentInfoModel);
        as_of_all_frames = 0;
        for (int dl = 0; dl < content_info_list.Count; dl++)
        {
            as_of_all_frames = as_of_all_frames + content_info_list[dl].num_of_frames - 1;
        }
        Queue<string> que = new Queue<string>(MAX_QUE);
        queues.Add(que);
        List<StoreData1> storeDatas1 = new List<StoreData1>();
        storeDatas_list.Add(storeDatas1);
        long unix_dl_start = (DateTimeOffset.Now - baseDt).Ticks;
        download_start_list.Add(unix_dl_start);
        var produceTask = Task.Run(() => {
            BufferController(airtap_count - 1);
        });
        if (dl_count == 1 && runningFlag == false)
        //if (airtap_count == 1)//unity debug
        {
            var postTask = Task.Run(() => {
                LogPost();
            });
            for (int b = 0; b < 1080; b++)//40-10sec/720-180sec//1080-270sec
            {
                if (que.Count >= AveMetadatas[0].min_buffer)//5
                {
                    Debug.Log("initial buffer:" + que.Count);
                    DeleteMethod();
                    break;
                }
                else
                {
                    Thread.Sleep(250);
                }
            }
        }
    }
    void Choice(int content_id)
    {
        if (content_info_list[content_id].as_of_throughput <= AveMetadatas[content_id].ave_bitrate_lists[0].ave_bitrate / 1000000)
        {
            content_info_list[content_id].choosed_pqs = AveMetadatas[content_id].ave_bitrate_lists[0].pqs.ToString();
        }
        //else if (content_info_list[content_id].as_of_throughput >= AveMetadatas[content_id].ave_bitrate_lists[9].ave_bitrate / 1000000)
        else if (content_info_list[content_id].as_of_throughput >= AveMetadatas[content_id].ave_bitrate_lists[4].ave_bitrate / 1000000)
        {
            //content_info_list[content_id].choosed_pqs = AveMetadatas[content_id].ave_bitrate_lists[9].pqs.ToString();
            content_info_list[content_id].choosed_pqs = AveMetadatas[content_id].ave_bitrate_lists[4].pqs.ToString();
        }
        else
        {
            //for (int j = 0; j < 10; j++)
            for (int j = 0; j < 5; j++)
            {
                if (content_info_list[content_id].as_of_throughput <= AveMetadatas[content_id].ave_bitrate_lists[j + 1].ave_bitrate / 1000000 && content_info_list[content_id].as_of_throughput > AveMetadatas[content_id].ave_bitrate_lists[j].ave_bitrate / 1000000)
                {
                    content_info_list[content_id].choosed_pqs = AveMetadatas[content_id].ave_bitrate_lists[j].pqs.ToString();
                    break;
                }
            }
        }
    }

    void BufferController(int content_id)
    {
        runningFlag = true;
        int max = AveMetadatas[content_id].max_buffer;// 10;
        int min = AveMetadatas[content_id].min_buffer; //5;

        while (true)
        {
            if (content_info_list[content_id].download_flag == false)
            {
                break;
            }
            while (true)
            {
                if (content_info_list[content_id].download_flag == false)
                {
                    break;
                }
                if (queues[content_id].Count > max)
                {
                    break;
                }
                if (content_info_list[content_id].rendering_flag == false && queues[content_id].Count >= min)
                {
                    content_info_list[content_id].rendering_flag = true;
                    long unix_rd_start = (DateTimeOffset.Now - baseDt).Ticks;
                    render_start_list.Add(unix_rd_start);
                }
                //if (content_info_list[content_id].next_frame < content_info_list[content_id].num_of_frames)
                if (content_info_list[content_id].next_frame <= 170)//50//100
                {
                    DownloadHandler(content_id);
                }
            }
            while (true)
            {
                if (queues[content_id].Count < min && content_info_list[content_id].download_flag == true)
                {
                    break;
                }
                if (content_info_list[content_id].download_flag == false)
                {
                    break;
                }
            }
        }
    }
    void DownloadHandler(int content_id)
    {
        ex++;
        long unix_dlst;
        long unix_dlen;
        string content_name = content_info_list[content_id].name;
        string choose_pqs = content_info_list[content_id].choosed_pqs;
        /*add adaptation*/
        if (AveMetadatas[content_id].rate_control == 0)
        {
            Choice(content_id);
            choose_pqs = content_info_list[content_id].choosed_pqs;
        }
        else
        {
            choose_pqs = AveMetadatas[content_id].rate_control.ToString();
        }
        int frame_num = content_info_list[content_id].next_frame;
        int loop_frame = frame_num % AveMetadatas[content_id].num_of_seg;
        string url = "http://XXXX/ply_dataset/bin_seg/" + content_name + "/" + choose_pqs + "/" + loop_frame + ".seg";
        content_info_list[content_id].choosed_pqs = choose_pqs;
        content_info_list[content_id].next_frame++;
        content_info_list[content_id].buffer_count++;
        if (content_info_list[content_id].next_frame == 170)//50//100
        {
            content_info_list[content_id].download_flag = false;
        }
        string txt;
        while (true)
        {
            try
            {
                unix_dlst = (DateTimeOffset.Now - baseDt).Ticks;
                WebRequest req = WebRequest.Create(url);
                using (WebResponse res = req.GetResponse())
                {
                    using (Stream st = res.GetResponseStream())
                    {
                        StreamReader sr = new StreamReader(st, Encoding.GetEncoding("UTF-8"));
                        txt = sr.ReadToEnd();
                        unix_dlen = (DateTimeOffset.Now - baseDt).Ticks;
                        break;
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

        lock (queues[content_id]) // get exclusive lock
        {
            while (queues[content_id].Count >= MAX_QUE)
            {
                // wait enqueue not to exceed MAX_QUE
                Debug.Log($"wait enqueue");
                enqueue_wait_count++;
                System.Threading.Monitor.Wait(queues[content_id]); // thread wait
            }
            var seg_que = new SegQue
            {
                pqs = content_info_list[content_id].choosed_pqs,
                segdata = txt
            };
            string content = JsonUtility.ToJson(seg_que);
            queues[content_id].Enqueue(content);
            System.Threading.Monitor.PulseAll(queues[content_id]); // thread restart
        } // release eclusive lock

        long unix_dlspan = unix_dlen - unix_dlst;
        float unix_dl = (float)unix_dlspan / 10000000;
        dl_time = unix_dl;
        //int datasize_bit = txt.Length * 8;
        long datasize_bit = txt.Length * 8;
        float datasize_Mbit = (float)datasize_bit / 1000000;
        //Debug.Log(datasize_bit + "/" + datasize_Mbit);
        float dl_throughput = datasize_Mbit / unix_dl;
        if (unix_dl > 0)
        {
            as_of_throughput = (double)dl_throughput;
            content_info_list[content_id].as_of_throughput = (double)dl_throughput;
        }
        /*store 1log dl*/
        DateTime date = DateTime.Now;
        string store_ts = date.Year + "-" + date.Month + "-" + date.Day + "T" + date.Hour + ":" + date.Minute + ":" + date.Second + "." + date.Millisecond; //+ "Z";
        var unixtime = (DateTimeOffset.Now - baseDt).Ticks;
        if (content_info_list.Count >= content_id)
        {
            var storeData1 = new StoreData1
            {
                timestamp = store_ts,
                unixTime = unixtime,
                content_id = content_id,
                choose_pqs = content_info_list[content_id].choosed_pqs,
                psnr_p2point = AveMetadatas[content_id].ave_bitrate_lists[int.Parse(content_info_list[content_id].choosed_pqs) - 1].ave_psnr_p2point,
                psnr_p2plane = AveMetadatas[content_id].ave_bitrate_lists[int.Parse(content_info_list[content_id].choosed_pqs) - 1].ave_psnr_p2plane,
                frame_number = content_info_list[content_id].next_frame - 1,
                buffer_size = queues[content_id].Count,
                download_time = unix_dl,
                throughput = content_info_list[content_id].as_of_throughput
            };
            storeDatas_list[content_id].Add(storeData1);
        }
    }

    void RenderingController()
    {
        if (content_info_list[choose_content].rendering_frame == 0)
        {
            lock (queues[choose_content]) // get exclusive lock
            {
                while (queues[choose_content].Count == 0 && runningFlag)
                {
                    // wait during empty queue
                    dequeue_wait_count++;
                    System.Threading.Monitor.Wait(queues[choose_content]); // thread wait
                }
                // dequeue
                if (queues[choose_content].Count > 0)
                {
                    string deque = queues[choose_content].Dequeue();
                    SegQue segQue = JsonUtility.FromJson<SegQue>(deque);
                    content_info_list[choose_content].rendering_pqs = segQue.pqs;
                    string segData = segQue.segdata;
                    var re = new Regex(" ");
                    string replace = re.Replace(segData, "_", 1);
                    //deserialize
                    BinSeg binSeg = JsonUtility.FromJson<BinSeg>(replace);
                    content_info_list[choose_content].rendering_segnum = binSeg.seg_num;
                    List<string> bindata_st = binSeg.payload;
                    //for (int i = 0; i < 30; i++)
                    for (int i = 0; i < content_info_list[choose_content].fps; i++)
                    {
                        byte[] vs = Convert.FromBase64String(bindata_st[i]);
                        content_info_list[choose_content].deque_data.Add(vs);
                    }
                    System.Threading.Monitor.PulseAll(queues[choose_content]); // thread restart
                }
            } // release exclusive lock
        }
        // data processing is after release lock
        if (content_info_list[choose_content].deque_data[content_info_list[choose_content].rendering_frame] != null)
        {
            RenderingHandler(choose_content);
            content_info_list[choose_content].rendering_frame++;
            //if (content_info_list[choose_content].rendering_frame == 30)
            if (content_info_list[choose_content].rendering_frame == 1)
            {
                content_info_list[choose_content].rendering_frame = 0;
                //content_info_list[choose_content].deque_data.Clear();
            }
            /*choose rendering content*/
            choose_content = choose_content + 1;
            if (choose_content == content_info_list.Count)
            {
                choose_content = 0;
            }
            while (content_info_list[choose_content].rendering_flag == false)
            {
                choose_content++;
                if (choose_content == content_info_list.Count)
                {
                    choose_content = 0;
                }
                int false_count = 0;
                for (int br = 0; br < content_info_list.Count; br++)
                {
                    if (content_info_list[br].rendering_flag == false)
                    {
                        false_count++;
                    }
                }
                if (false_count == content_info_list.Count)
                {
                    break;
                }
            }
            Debug.Log("choose_content:" + choose_content);

            /*if (rd_num > 0)
            {
                fps = 30 * rd_num;//30//5
            }*/
            double in_sp = 1.0 / fps;
            if (rd_num > 0)
            {
                in_sp = 1.0 / rd_num;
            }
            float invoke_span = (float)in_sp;
            Invoke("DeleteMethod", invoke_span);//0.2/0.1//0.033f/0.067f/0.100f/0.200f
        }
    }
    void RenderingHandler(int content_id)
    {

        //DateTime inv_ts = DateTime.UtcNow;
        DateTime inv_ts = DateTime.Now;
        string invts = inv_ts.Year + "-" + inv_ts.Month + "-" + inv_ts.Day + "T" + inv_ts.Hour + ":" + inv_ts.Minute + ":" + inv_ts.Second + "." + inv_ts.Millisecond + "Z";
        long render_unixtime = (DateTimeOffset.Now - baseDt).Ticks;
        content_info_list[content_id].rendering_timestamp = invts;
        content_info_list[content_id].rendering_unixtime = render_unixtime;
        content_info_list[content_id].frame_num_all = k;
        k++;
    }
    void DeleteMethod()
    {
        //DateTime del_ts = DateTime.UtcNow;
        DateTime del_ts = DateTime.Now;
        string delts = del_ts.Year + "-" + del_ts.Month + "-" + del_ts.Day + "T" + del_ts.Hour + ":" + del_ts.Minute + ":" + del_ts.Second + "." + del_ts.Millisecond + "Z";
        var deleting_unixtime = (DateTimeOffset.Now - baseDt).Ticks;
        if (k == 0)
        {
            RenderingController();
        }

        else if (k < 500)//5000/500/200
        {
            if (content_info_list[choose_content].rendering_segnum == 0 && content_info_list[choose_content].rendering_frame == 0 && content_info_list[choose_content].log_rendring_flag == false)
            {
                content_info_list[choose_content].log_rendring_flag = true;
            }
            else if (content_info_list[choose_content].rendering_frame == 0)
            {
                var renderingLog = new RenderingLog
                {
                    render_timestamp = content_info_list[choose_content].rendering_timestamp,
                    render_unixtime = content_info_list[choose_content].rendering_unixtime,
                    delete_timestamp = delts,
                    delete_unixtime = deleting_unixtime,
                    content_id = choose_content,
                    rendering_pqs = content_info_list[choose_content].rendering_pqs,
                    seg_num = content_info_list[choose_content].rendering_segnum,
                    frame_num_in_seg = 29,//content_info_list[choose_content].rendering_frame - 1,//
                    frame_num_all = content_info_list[choose_content].frame_num_all//
                };
                renderingLog_list.Add(renderingLog);
            }
            else
            {
                var renderingLog = new RenderingLog
                {
                    render_timestamp = content_info_list[choose_content].rendering_timestamp,
                    render_unixtime = content_info_list[choose_content].rendering_unixtime,
                    delete_timestamp = delts,
                    delete_unixtime = deleting_unixtime,
                    content_id = choose_content,
                    rendering_pqs = content_info_list[choose_content].rendering_pqs,
                    seg_num = content_info_list[choose_content].rendering_segnum,
                    frame_num_in_seg = content_info_list[choose_content].rendering_frame - 1,//
                    frame_num_all = content_info_list[choose_content].frame_num_all//
                };
                renderingLog_list.Add(renderingLog);
            }
            RenderingController();
        }
        else
        {
            if (content_info_list[choose_content].rendering_frame == 0)
            {
                var renderingLog = new RenderingLog
                {
                    render_timestamp = content_info_list[choose_content].rendering_timestamp,
                    render_unixtime = content_info_list[choose_content].rendering_unixtime,
                    delete_timestamp = delts,
                    delete_unixtime = deleting_unixtime,
                    content_id = choose_content,
                    rendering_pqs = content_info_list[choose_content].rendering_pqs,
                    seg_num = content_info_list[choose_content].rendering_segnum,
                    frame_num_in_seg = 29,//content_info_list[choose_content].rendering_frame - 1,//
                    frame_num_all = content_info_list[choose_content].frame_num_all//
                };
                renderingLog_list.Add(renderingLog);
            }
            else
            {
                var renderingLog = new RenderingLog
                {
                    render_timestamp = content_info_list[choose_content].rendering_timestamp,
                    render_unixtime = content_info_list[choose_content].rendering_unixtime,
                    delete_timestamp = delts,
                    delete_unixtime = deleting_unixtime,
                    content_id = choose_content,
                    rendering_pqs = content_info_list[choose_content].rendering_pqs,
                    seg_num = content_info_list[choose_content].rendering_segnum,
                    frame_num_in_seg = content_info_list[choose_content].rendering_frame - 1,//
                    frame_num_all = content_info_list[choose_content].frame_num_all//
                };
                renderingLog_list.Add(renderingLog);
            }
            /*if (content_info_list.Count >= 1)
            {
                content_info_list[0].rendering_flag = false;
                if (content_info_list.Count >= 2)
                {
                    content_info_list[1].rendering_flag = false;
                    if (content_info_list.Count >= 3)
                    {
                        content_info_list[2].rendering_flag = false;
                    }
                }
            }*/
            Debug.Log("Loop stop");
            LogStore();
        }
    }
    void LogPost()
    {
        Debug.Log("Log post");
        for (int m = 0; m < 2000; m++)//500//1600//3000*//2000*
        {
            /*store log rd*/
            DateTime date = DateTime.Now;
            string store_ts = date.Year + "-" + date.Month + "-" + date.Day + "T" + date.Hour + ":" + date.Minute + ":" + date.Second + "." + date.Millisecond; //+ "Z";
            var unixtime = (DateTimeOffset.Now - baseDt).Ticks;
            List<ContentResult2> contentResult2_list = new List<ContentResult2>();

            for (int log = 0; log < content_info_list.Count; log++)
            {
                if (content_info_list.Count >= (log + 1))
                {
                    var content_result0 = new ContentResult2
                    {
                        content_id = log,
                        buffer_size = queues[log].Count
                    };
                    contentResult2_list.Add(content_result0);
                }
            }

            var storedata3 = new StoreData3
            {
                timestamp = store_ts,
                unixTime = unixtime,
                contentResultList = contentResult2_list
            };
            storeDatas3.Add(storedata3);

            Thread.Sleep(100);//250/550//10*

        }
        Debug.Log("END");
    }
    void LogStore()
    {
        Debug.Log("Log store");
        for (int log = 0; log < content_info_list.Count; log++)
        {
            DateTime download1 = DateTime.Now;
            Debug.Log(download1.ToString());
            var re = new Regex("/");
            string replace = re.Replace(download1.ToString(), "-", 2);
            var re2 = new Regex(":");
            string replace2 = re2.Replace(replace, "-", 2);
            string filename = string.Format(@"all_" + replace2 + "_download_" + log + ".csv");
            Debug.Log(filename);
            string address = Path.Combine(Application.persistentDataPath, filename);
            Debug.Log(address);
            StreamWriter sw;
            if (!File.Exists(address))
            {
                sw = File.CreateText(address);
                sw.Flush();
                sw.Dispose();
            }
            sw = new StreamWriter(new FileStream(address, FileMode.Open));
            sw.WriteLine("timestamp,unixTime,time,pqs_" + log + ",psnr_p2point_" + log + ",psnr_p2plane_" + log + ",frame_num,buffer,dl_time,Throughput_" + log + ", content_id");
            for (int j = 0; j < storeDatas_list[log].Count; j++)
            {
                sw.WriteLine(storeDatas_list[log][j].timestamp + "," + storeDatas_list[log][j].unixTime + "," + (float)(storeDatas_list[log][j].unixTime - startTime) / 10000000 + "," + ((float)int.Parse(storeDatas_list[log][j].choose_pqs) / 10).ToString() + "," + storeDatas_list[log][j].psnr_p2point + "," + storeDatas_list[log][j].psnr_p2plane + "," + storeDatas_list[log][j].frame_number + "," + storeDatas_list[log][j].buffer_size + "," + storeDatas_list[log][j].download_time + "," + storeDatas_list[log][j].throughput + "," + storeDatas_list[log][j].content_id);
            }
            sw.Flush();
            sw.Dispose();
        }

        DateTime download2 = DateTime.Now;
        Debug.Log(download2.ToString());
        var re3 = new Regex("/");
        string replace3 = re3.Replace(download2.ToString(), "-", 2);
        var re4 = new Regex(":");
        string replace4 = re4.Replace(replace3, "-", 2);
        string filename2 = string.Format(@"all_" + replace4 + "_buffer.csv");
        Debug.Log(filename2);
        string address2 = Path.Combine(Application.persistentDataPath, filename2);        
        Debug.Log(address2);
        StreamWriter sw2;
        if (!File.Exists(address2))
        {
            sw2 = File.CreateText(address2);
            sw2.Flush();
            sw2.Dispose();
        }
        sw2 = new StreamWriter(new FileStream(address2, FileMode.Open));
        string buffer = "timestamp,unixtime,time";
        if (content_info_list.Count >= 1)
        {
            for (int buf = 0; buf < content_info_list.Count; buf++)
            {
                //buffer = buffer + ",content" + buf;
                buffer = buffer + ",Buffer_" + buf;
            }
            buffer = buffer + "," + startTime.ToString();
            for (int buf = 0; buf < content_info_list.Count; buf++)
            {
                buffer = buffer + "," + download_start_list[buf].ToString() + "," + render_start_list[buf].ToString();
            }
        }
        sw2.WriteLine(buffer);
        for (int j = 0; j < storeDatas3.Count; j++)
        {
            string buf_data = storeDatas3[j].timestamp + "," + storeDatas3[j].unixTime.ToString() + "," + (float)(storeDatas3[j].unixTime - startTime) / 10000000;
            if (content_info_list.Count >= 1)
            {
                for (int buf = 0; buf < storeDatas3[j].contentResultList.Count; buf++)
                {
                    buf_data = buf_data + "," + storeDatas3[j].contentResultList[buf].buffer_size;
                }
            }
            sw2.WriteLine(buf_data);
        }
        sw2.Flush();
        sw2.Dispose();

        DateTime download3 = DateTime.Now;
        Debug.Log(download3.ToString());
        var re5 = new Regex("/");
        string replace5 = re5.Replace(download3.ToString(), "-", 2);
        var re6 = new Regex(":");
        string replace6 = re6.Replace(replace5, "-", 2);
        string filename3 = string.Format(@"all_" + replace6 + "_rendering.csv");
        Debug.Log(filename3);
        string address3 = Path.Combine(Application.persistentDataPath, filename3);
        Debug.Log(address3);
        StreamWriter sw3;
        if (!File.Exists(address3))
        {
            sw3 = File.CreateText(address3);
            sw3.Flush();
            sw3.Dispose();
        }
        sw3 = new StreamWriter(new FileStream(address3, FileMode.Open));
        sw3.WriteLine("render_timestamp,render_unixtime,render_time,delete_timestamp,delete_unixtime,delete_time,content_id,rendering_pqs,real_pqs,seg_num,frame_num_in_seg,k,render_span(msec),render_span(sec)");
        for (int j = 0; j < renderingLog_list.Count; j++)
        {
            sw3.WriteLine(renderingLog_list[j].render_timestamp + "," + renderingLog_list[j].render_unixtime + "," + (float)(renderingLog_list[j].render_unixtime - startTime) / 10000000 + "," + renderingLog_list[j].delete_timestamp + "," + renderingLog_list[j].delete_unixtime + "," + (float)(renderingLog_list[j].delete_unixtime - startTime) / 10000000 + "," + renderingLog_list[j].content_id + "," + renderingLog_list[j].rendering_pqs + "," + ((float)int.Parse(renderingLog_list[j].rendering_pqs) / 10).ToString() + "," + renderingLog_list[j].seg_num + "," + renderingLog_list[j].frame_num_in_seg + "," + renderingLog_list[j].frame_num_all + "," + (renderingLog_list[j].delete_unixtime - renderingLog_list[j].render_unixtime) + "," + (float)(renderingLog_list[j].delete_unixtime - renderingLog_list[j].render_unixtime) / 10000000);
        }
        sw3.Flush();
        sw3.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        dl_count = 0;
        rd_num = 0;
        for (int dl = 0; dl < content_info_list.Count; dl++)
        {
            if (content_info_list[dl].download_flag == true)
            {
                dl_count++;
            }
            if (content_info_list[dl].rendering_flag == true)
            {
                rd_num++;
            }
        }
    }
}
