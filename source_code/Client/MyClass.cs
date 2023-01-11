using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.IO;
using System.Linq;


public class MyClass : MonoBehaviour
{
    #region Internal utilities

    public static Material GetDefaultMaterial()//public
    {

        Material material = Resources.Load<Material>("MyDefault");
        return material;
    }

    #endregion

    #region Internal data structure

    public enum DataProperty
    {
        Invalid,
        R8, G8, B8, A8,
        R16, G16, B16, A16,
        SingleX, SingleY, SingleZ,
        DoubleX, DoubleY, DoubleZ,
        Data8, Data16, Data32, Data64
    }

    static int GetPropertySize(DataProperty p)
    {
        switch (p)
        {
            case DataProperty.R8: return 1;
            case DataProperty.G8: return 1;
            case DataProperty.B8: return 1;
            case DataProperty.A8: return 1;
            case DataProperty.R16: return 2;
            case DataProperty.G16: return 2;
            case DataProperty.B16: return 2;
            case DataProperty.A16: return 2;
            case DataProperty.SingleX: return 4;
            case DataProperty.SingleY: return 4;
            case DataProperty.SingleZ: return 4;
            case DataProperty.DoubleX: return 8;
            case DataProperty.DoubleY: return 8;
            case DataProperty.DoubleZ: return 8;
            case DataProperty.Data8: return 1;
            case DataProperty.Data16: return 2;
            case DataProperty.Data32: return 4;
            case DataProperty.Data64: return 8;
        }
        return 0;
    }

    class DataHeader
    {
        public List<DataProperty> properties = new List<DataProperty>();
        public int vertexCount = -1;
    }

    class DataBody
    {
        public List<Vector3> vertices;
        public List<Color32> colors;

        public DataBody(int vertexCount)
        {
            vertices = new List<Vector3>(vertexCount);
            colors = new List<Color32>(vertexCount);
        }

        public void AddPoint(
            float x, float y, float z,
            byte r, byte g, byte b, byte a
        )
        {
            //vertices.Add(new Vector3(x, y, z));
            vertices.Add(new Vector3(x, -y, z));//realsense->unity
            colors.Add(new Color32(r, g, b, a));
        }
    }

    #endregion

    #region Reader implementation

    public static Mesh ImportAsMesh(Stream stream)//public static
    {
        try
        {
            var header = ReadDataHeader(new StreamReader(stream));
            var body = ReadDataBody(header, new BinaryReader(stream));

            var mesh = new Mesh();
            //mesh.name = Path.GetFileNameWithoutExtension(path);
            mesh.name = "New Point Cloud";

            mesh.indexFormat = header.vertexCount > 65535 ?
                IndexFormat.UInt32 : IndexFormat.UInt16;

            mesh.SetVertices(body.vertices);
            mesh.SetColors(body.colors);

            mesh.SetIndices(
                Enumerable.Range(0, header.vertexCount).ToArray(),
                MeshTopology.Points, 0
            );

            mesh.UploadMeshData(true);
            //Live.check_flag = false;
            return mesh;
        }
        catch (Exception e)
        {
            //Debug.LogError("Failed importing " + path + ". " + e.Message);
            Debug.LogError("Failed importing this Ply file. " + e.Message);//error msessage
            Live.check_flag = true;
            return null;
        }
    }
    static DataHeader ReadDataHeader(StreamReader reader)//static
    {
        var data = new DataHeader();
        var readCount = 0;

        // Magic number line ("ply")
        var line = reader.ReadLine();
        readCount += line.Length + 1;
        if (line != "ply")
            throw new ArgumentException("Magic number ('ply') mismatch.");

        // Data format: check if it's binary/little endian.
        line = reader.ReadLine();
        readCount += line.Length + 1;
        if (line != "format binary_little_endian 1.0")
            throw new ArgumentException(
                "Invalid data format ('" + line + "'). " +
                "Should be binary/little endian.");

        // Read header contents.
        for (var skip = false; ;)
        {
            // Read a line and split it with white space.
            line = reader.ReadLine();
            readCount += line.Length + 1;
            if (line == "end_header") break;
            var col = line.Split();

            // Element declaration (unskippable)
            if (col[0] == "element")
            {
                if (col[1] == "vertex")
                {
                    data.vertexCount = Convert.ToInt32(col[2]);
                    skip = false;
                }
                else
                {
                    // Don't read elements other than vertices.
                    skip = true;
                }
            }

            if (skip) continue;

            // Property declaration line
            if (col[0] == "property")
            {
                var prop = DataProperty.Invalid;

                // Parse the property name entry.
                switch (col[2])
                {
                    case "red": prop = DataProperty.R8; break;
                    case "green": prop = DataProperty.G8; break;
                    case "blue": prop = DataProperty.B8; break;
                    case "alpha": prop = DataProperty.A8; break;
                    case "x": prop = DataProperty.SingleX; break;
                    case "y": prop = DataProperty.SingleY; break;
                    case "z": prop = DataProperty.SingleZ; break;
                }

                // Check the property type.
                if (col[1] == "char" || col[1] == "uchar" ||
                    col[1] == "int8" || col[1] == "uint8")
                {
                    if (prop == DataProperty.Invalid)
                        prop = DataProperty.Data8;
                    else if (GetPropertySize(prop) != 1)
                        throw new ArgumentException("Invalid property type ('" + line + "').");
                }
                else if (col[1] == "short" || col[1] == "ushort" ||
                         col[1] == "int16" || col[1] == "uint16")
                {
                    switch (prop)
                    {
                        case DataProperty.Invalid: prop = DataProperty.Data16; break;
                        case DataProperty.R8: prop = DataProperty.R16; break;
                        case DataProperty.G8: prop = DataProperty.G16; break;
                        case DataProperty.B8: prop = DataProperty.B16; break;
                        case DataProperty.A8: prop = DataProperty.A16; break;
                    }
                    if (GetPropertySize(prop) != 2)
                        throw new ArgumentException("Invalid property type ('" + line + "').");
                }
                else if (col[1] == "int" || col[1] == "uint" || col[1] == "float" ||
                         col[1] == "int32" || col[1] == "uint32" || col[1] == "float32")
                {
                    if (prop == DataProperty.Invalid)
                        prop = DataProperty.Data32;
                    else if (GetPropertySize(prop) != 4)
                        throw new ArgumentException("Invalid property type ('" + line + "').");
                }
                else if (col[1] == "int64" || col[1] == "uint64" ||
                         col[1] == "double" || col[1] == "float64")
                {
                    switch (prop)
                    {
                        case DataProperty.Invalid: prop = DataProperty.Data64; break;
                        case DataProperty.SingleX: prop = DataProperty.DoubleX; break;
                        case DataProperty.SingleY: prop = DataProperty.DoubleY; break;
                        case DataProperty.SingleZ: prop = DataProperty.DoubleZ; break;
                    }
                    if (GetPropertySize(prop) != 8)
                        throw new ArgumentException("Invalid property type ('" + line + "').");
                }
                else
                {
                    throw new ArgumentException("Unsupported property type ('" + line + "').");
                }

                data.properties.Add(prop);
            }
        }

        // Rewind the stream back to the exact position of the reader.
        reader.BaseStream.Position = readCount;

        return data;
    }

    static DataBody ReadDataBody(DataHeader header, BinaryReader reader)//static
    {
        var data = new DataBody(header.vertexCount);

        float x = 0, y = 0, z = 0;
        Byte r = 255, g = 255, b = 255, a = 255;

        for (var i = 0; i < header.vertexCount; i++)
        {
            foreach (var prop in header.properties)
            {
                switch (prop)
                {
                    case DataProperty.R8: r = reader.ReadByte(); break;
                    case DataProperty.G8: g = reader.ReadByte(); break;
                    case DataProperty.B8: b = reader.ReadByte(); break;
                    case DataProperty.A8: a = reader.ReadByte(); break;

                    case DataProperty.R16: r = (byte)(reader.ReadUInt16() >> 8); break;
                    case DataProperty.G16: g = (byte)(reader.ReadUInt16() >> 8); break;
                    case DataProperty.B16: b = (byte)(reader.ReadUInt16() >> 8); break;
                    case DataProperty.A16: a = (byte)(reader.ReadUInt16() >> 8); break;

                    case DataProperty.SingleX: x = reader.ReadSingle(); break;
                    case DataProperty.SingleY: y = reader.ReadSingle(); break;
                    case DataProperty.SingleZ: z = reader.ReadSingle(); break;

                    case DataProperty.DoubleX: x = (float)reader.ReadDouble(); break;
                    case DataProperty.DoubleY: y = (float)reader.ReadDouble(); break;
                    case DataProperty.DoubleZ: z = (float)reader.ReadDouble(); break;

                    case DataProperty.Data8: reader.ReadByte(); break;
                    case DataProperty.Data16: reader.BaseStream.Position += 2; break;
                    case DataProperty.Data32: reader.BaseStream.Position += 4; break;
                    case DataProperty.Data64: reader.BaseStream.Position += 8; break;
                }
            }

            data.AddPoint(x, y, z, r, g, b, a);
        }

        return data;
    }
    #endregion
    public static long datasize;
    static public byte[] ReadBinaryData(Stream st)
    {
        byte[] buf = new byte[32768]; //temporary buffer
        using (MemoryStream ms = new MemoryStream())
        {
            while (true)
            {
                // read from stream to temporary buffer
                int read = st.Read(buf, 0, buf.Length);
                if (read > 0)
                {
                    // write from temporary buffer to memory stream
                    ms.Write(buf, 0, read);
                }
                else
                {
                    break;
                }
            }
            datasize = ms.Length;
            // store from memory stream to byte array
            return ms.ToArray();
        }
    }

}
[Serializable]
public class PostQue2
{
    public string id;
    public string type;
    public string timestamp;
    public string pointcloud_name;
    public PosLog2 log;
}
[Serializable]
public class PosLog2
{
    public string timestamp;
    public int data_size;
    public float download_time;
    public int buffer_size;
    public string state_name;
    public float display_time;
    public float invoke_time;
    public float delete_time;
}
/*LogPost.cs*/
[Serializable]
public class PostQue3
{
    public string id;
    public string type;
    public string timestamp;
    public string pointcloud_name;
    public PosLog3 log;
}
[Serializable]
public class PosLog3
{
    public string timestamp;
    public int data_size;
    public float download_time;
    public int buffer_size;
    public string state_name;
    public string dltimestamp;
    public int dl_frame_num;
    public int dp_frame_num;
    public PosDetailLog3 detail_logs;
}
[Serializable]
public class PosDetailLog3
{ 
    public PosDisplayLog3 display_state;
    public PosDisplayLog3 invoke_state;
    public PosDisplayLog3 delete_state;
}
[Serializable]
public class PosDisplayLog3
{
    public string timestamp;
    public float processing_time;
}
/*live.json*/
[Serializable]
public class LiveJson
{
    public string timestamp;
    public string mode;
    public int fps;
    public float voxel_size;
    public int current_frame_num;
}
/*metadata.json*/
[Serializable]
public class MetadataJson
{
    public string sequence_name;
    public DirName dir_name;
    public int num_of_frames;
    public int frame_rate;
    public int start_frame_num;
    public ServerInfo server_info;
    public BufferSize buffer_size;
    public List<Representation> representation;
}
[Serializable]
public class DirName
{
    public string root;
    public string org_ply;
    public string proc_ply;
}
[Serializable]
public class ServerInfo
{
    public string server_host;
    public string server_protocol;
}
[Serializable]
public class BufferSize
{
    public int init;
    public int max;
    public int min;
}
[Serializable]
public class Representation
{
    public int rep_id;
    public double voxel_size;
    public double total_data_size;
    public double avg_data_size;
    public double total_num_points;
    public double avgl_num_points;
    public double bitrate;
}
/*bin_meta.json*/
[Serializable]
public class MetadataJsonBin
{
    public string sequence_name;
    public DirName dir_name;
    public int num_of_frames;
    public int frame_rate;
    public int start_frame_num;
    public ServerInfo server_info;
    public BufferSize buffer_size;
    public List<Representationbin> representation;
}
[Serializable]
public class Representationbin
{
    public int rep_id;
    public double pqs;
    public double bitrate;
}
/*queue info*/
[Serializable]
public class FrameInfo
{
    public int en_content_id;
    public int frame_number;
    public string sequence_name;
    public byte[] content_data;
}
/*meta info model*/
[Serializable]
public class ContentInfoModel
{
    public int id;
    public string name;
    public int num_of_frames;
    public int fps;
    public PositionInfo position;
    public bool download_flag;
    public bool rendering_flag;
    public int next_frame;
    public double as_of_throughput;//add 
    public double distance;//add for DistanceQuality.cs
    public string choosed_pqs;
    public string choosed_percent;//add for DistanceQuality.cs
    public double bitrate;//add for DistanceQuality.cs
    public double psnr_p2point;//add for DistanceQuality.cs
    public double psnr_p2plane;//add for DistanceQuality.cs
    public double ms_ssim;//add for DistanceQuality.cs
    public int buffer_count;
    public int rendering_frame;//add for CompSeqAdap.cs
    public List<byte[]> deque_data;//add for CompSeqAdap.cs
    public string rendering_pqs;//add for CompSeqAdap.cs
    public string rendering_percent;//add for DQ
    public int rendering_segnum;//add for CompSeqAdap.cs
    public string rendering_timestamp;//add for CompSeqAdap.cs
    public long rendering_unixtime;//add for CompSeqAdap.cs
    public int frame_num_all;//add for CompSeqAdap.cs
    public bool log_rendring_flag; //add for CompSeqAdap.cs    
}
[Serializable]
public class PositionInfo
{
    public float x_pos;
    public float y_pos;
    public float z_pos;
    public float x_rot;
    public float y_rot;
    public float z_rot;
}
/*store log data*/
[Serializable]
public class StoreData
{
    public string timestamp;
    public long unixTime;
    public ContentResultList contentResultList;
    public double throughput;
    public int all_buffer_size;
}
[Serializable]
public class ContentResultList
{
    public ContentResult contentResult0;
    public ContentResult contentResult1;
    public ContentResult contentResult2;
}
[Serializable]
public class ContentResult
{
    public int content_id;
    public string choose_pqs;
    public int frame_number;
    public int buffer_size;
    //public double download_time;
    public double throughput;
}
/*store log data for rendering*/
[Serializable]
public class StoreData2
{
    public string timestamp;
    public long unixTime;
    public ContentResultList2 contentResultList;
    //public double throughput;
    public int all_buffer_size;
}
[Serializable]
public class ContentResultList2
{
    public ContentResult2 contentResult0;
    public ContentResult2 contentResult1;
    public ContentResult2 contentResult2;
}
[Serializable]
public class ContentResult2
{
    public int content_id;
    //public string choose_pqs;
    //public int frame_number;
    public int buffer_size;
    public double throughput;
}
[Serializable]
public class StoreData1
{
    public string timestamp;
    public long unixTime;
    public int content_id;
    public string choose_pqs;
    public string choose_percent;//add for DistanceQuality
    public double bitrate;
    public double psnr_p2point;
    public double psnr_p2plane;
    public double ms_ssim;//add for DistanceQuality
    public double distance;
    public int frame_number;
    public int buffer_size;
    public double download_time;
    public double throughput;
}
/*seg get*/
[Serializable]
public class BinSeg
{
    public int seg_num;
    public List<string> payload;
}
/*seg metadata*/
[Serializable]
public class SegMetadata
{
    public string input_path;
    public string output_path;
    public string pqs;
    public int frame_rate;
    public int num_of_seg;
    public List<BitrateList> bitrate_lists;
}
[Serializable]
public class BitrateList
{
    public int seg_num;
    public int bitrate;
}
/*seg average metadata for CompSeg*/
[Serializable]
public class AveSegMetadata
{
    public int frame_rate;
    public int num_of_seg;
    public List<AveBitrateList> ave_bitrate_lists;
}
[Serializable]
public class AveBitrateList
{
    public int pqs;
    public double ave_bitrate;
}
/*segment queue for CompSeg*/
[Serializable]
public class SegQue
{
    public string pqs;
    public string percent;//add for DistanceQuality.cs
    public string segdata;
}
/*rendering Log for CompSeg*/
[Serializable]
public class RenderingLog
{
    public string render_timestamp;
    public long render_unixtime;
    public string delete_timestamp;
    public long delete_unixtime;
    public int content_id;
    public string rendering_pqs;
    public string rendering_percent;//Add for DQ
    public int seg_num;
    public int frame_num_in_seg;
    public int frame_num_all;
}
/*seg average metadata w/psnr & buffer*/
[Serializable]
public class AveMetadata
{
    public int frame_rate;
    public int num_of_seg;
    public int max_buffer;
    public int min_buffer;
    public int rate_control;
    public List<AveBitPsnrList> ave_bitrate_lists;
}
[Serializable]
public class AveBitPsnrList
{
    public int percent;
    public double pqs;//int
    public double ave_bitrate;
    public double ave_psnr_p2point;
    public double ave_psnr_p2plane;
    public double curve_a;//add
    public double curve_b;//add
    public double curve_c;//add
    public double near_msssim;//add
}
/*seg grafana*/
[Serializable]
public class DataBaseLog
{
    public string id;
    public string type;
    public string timestamp;
    public SegLog log;
}
[Serializable]
public class SegLog
{
    public string timestamp;
    public int content_id;
    public int buffer_size;
    public float throughput;
    public float pqs;
    public float psnr_p2point;
    public float psnr_p2plane;
}
/*buffer store*/
[Serializable]
public class StoreData3
{
    public string timestamp;
    public long unixTime;
    public double all_capacity;//add for DistanceQuality.cs
    public List<ContentResult2> contentResultList;
}
/*rendering time*/
[Serializable]
public class RdTime
{
    public int pqs;
    public int frame_num;
    public int data_size;
    public float time_span;
};
