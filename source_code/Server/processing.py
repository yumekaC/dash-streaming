import open3d as o3d
import numpy as np
import os
import csv
import time
content_name="ted"#"greeting"#"ted"#"racecar"#"slab_chair"#"spool"#telecon
dir_name="PSNR_p2point"#"bitrate"#"PSNR_p2point"
FRAME=1#295#305#1

PQS=16
inputScale=1000

for frame_num in range(FRAME):
    #g-pcc
    for pqs in range(PQS):
        os.system("mkdir "+content_name+"/"+dir_name+"/"+str(pqs))
        os.system("mkdir "+content_name+"/"+dir_name+"/"+str(pqs)+"/encode_log && mkdir "+content_name+"/"+dir_name+"/"+str(pqs)+"/bin")
        os.system("mkdir "+content_name+"/"+dir_name+"/"+str(pqs)+"/decode_ply && mkdir "+content_name+"/"+dir_name+"/"+str(pqs)+"/decode_log")
        os.system("mkdir "+content_name+"/"+dir_name+"/"+str(pqs)+"/pc_error && mkdir "+content_name+"/"+dir_name+"/"+str(pqs)+"/binary_ply")
        #encode
        #os.system("tmc3 -c "+content_name+"/"+dir_name+"/cfg/"+str(pqs)+"/encoder.cfg --uncompressedDataPath=../../dataset/"+content_name+"/ascii_ply/"+str(frame_num)+".ply --compressedStreamPath="+content_name+"/"+dir_name+"/"+str(pqs)+"/bin/"+str(frame_num)+".bin --inputScale="+str(inputScale)+" > "+content_name+"/"+dir_name+"/"+str(pqs)+"/encode_log/"+str(frame_num)+".log")
        os.system("tmc3 -c "+content_name+"/"+dir_name+"/cfg/"+str(pqs)+"/encoder.cfg --uncompressedDataPath=../../dataset/"+content_name+".ply --compressedStreamPath="+content_name+"/"+dir_name+"/"+str(pqs)+"/bin/"+str(frame_num)+".bin --inputScale="+str(inputScale)+" > "+content_name+"/"+dir_name+"/"+str(pqs)+"/encode_log/"+str(frame_num)+".log")
        #decode
        os.system("tmc3 -c "+content_name+"/"+dir_name+"/ex/"+str(pqs)+"/decoder.cfg --compressedStreamPath="+content_name+"/"+dir_name+"/"+str(pqs)+"/bin/"+str(frame_num)+".bin --reconstructedDataPath="+content_name+"/"+dir_name+"/"+str(pqs)+"/decode_ply/"+str(frame_num)+"_decode.ply --outputBinaryPly=0 --mergeDuplicatedPoints=1 --outputUnitLength="+str(inputScale)+" > "+content_name+"/"+dir_name+"/"+str(pqs)+"/decode_log/"+str(frame_num)+".log")
        #pcerror#
        #os.system("./pc_error -a ../contents/"+content_name+"/ascii_ply/"+str(frame_num)+".ply -b "+content_name+"/"+dir_name+"/"+str(pqs)+"/decode_ply/"+str(frame_num)+"_decode.ply > "+content_name+"/"+dir_name+"/"+str(pqs)+"/pc_error/"+str(frame_num)+".log")
        os.system("./pc_error -a ../contents/"+content_name+".ply -b "+content_name+"/"+dir_name+"/"+str(pqs)+"/decode_ply/"+str(frame_num)+"_decode.ply > "+content_name+"/"+dir_name+"/"+str(pqs)+"/pc_error/"+str(frame_num)+".log")
        #binary
        os.system("cat "+content_name+"/"+dir_name+"/"+str(pqs)+"/decode_ply/"+ str(frame_num) + "_decode.ply | sed s/float/double/ > "+content_name+"/"+dir_name+"/"+str(pqs)+"/decode_ply/"+str(frame_num)+"_double.ply")
        pcd_bi = o3d.io.read_point_cloud(content_name+"/"+dir_name+"/"+str(pqs)+"/decode_ply/"+str(frame_num)+"_double.ply")
        o3d.io.write_point_cloud(content_name+"/"+dir_name+"/"+str(pqs)+"/binary_ply/"+str(frame_num)+ "_binary.ply", pcd_bi, False, True)


csv_filename=content_name+"/"+content_name+"_"+dir_name+"_inf.csv"
with open(csv_filename,'w') as f:
    writer = csv.writer(f)
    logs=[]
    logs.append(["id","pqs","number_of_points","volume[m^3]","density[/m^3]", "bin_size[Mbit]","rms_p2point","rms_p2plane","psnr_p2point","psnr_p2plane","encode_time","decode_time"])
    for pqs in range(PQS):
        sum_num=0
        sum_volume=0
        sum_density=0
        sum_bin_size=0
        sum_rms_p2point=0
        sum_rms_p2plane=0
        sum_psnr_p2point=0
        sum_psnr_p2plane=0
        sum_encode_time=0
        sum_decode_time=0
        for frame_num in range(FRAME):
            with open(content_name+"/"+dir_name+"/"+str(pqs)+"/decode_ply/"+str(frame_num)+"_decode.ply","r") as lf:
                lines = lf.read().split('\n')
                line=lines[2].split(' ')
                num_points=line[2]
                sum_num=sum_num+int(num_points)
            pcd=o3d.io.read_point_cloud(content_name+"/"+dir_name+"/"+str(pqs)+"/decode_ply/"+str(frame_num)+"_decode.ply")
            obb = pcd.get_oriented_bounding_box()
            volume=obb.extent[0]*obb.extent[1]*obb.extent[2]
            sum_volume=sum_volume+volume
            density=float(num_points)/volume
            sum_density=sum_density+density
            bin_path=content_name+"/"+dir_name+"/"+str(pqs)+"/bin/"+str(frame_num)+".bin"
            bin_size=int(os.path.getsize(bin_path)) * 8 ## bytes => bits
            sum_bin_size=sum_bin_size+bin_size
            with open(content_name+"/"+dir_name+"/"+str(pqs)+"/encode_log/"+str(frame_num)+".log","r") as lf:
                lines = lf.read().split('\n')
                timeline=lines[138].split(' ')
                encode_time=timeline[3]
                sum_encode_time=sum_encode_time+float(encode_time)
                binline=lines[136].split(' ')#136
                bin_size=binline[3]#3
                pqsline=lines[20].split(':')
                en_pqs=pqsline[1]
            with open(content_name+"/"+dir_name+"/"+str(pqs)+"/decode_log/"+str(frame_num)+".log","r") as lf:
                lines = lf.read().split('\n')
                timeline=lines[21].split(' ')
                decode_time=timeline[3]
                sum_decode_time=sum_decode_time+float(decode_time)
            with open(content_name+"/"+dir_name+"/"+str(pqs)+"/pc_error/"+str(frame_num)+".log","r") as pf:
                pelines = pf.read().split('\n')
                p2point=pelines[27].split(' ')
                point=p2point[4].split(',')
                rms_p2point=point[3]
                sum_rms_p2point=sum_rms_p2point+float(rms_p2point)
                p2plane=pelines[29].split(' ')
                plane=p2plane[4].split(',')
                rms_p2plane=plane[3]
                sum_rms_p2plane=sum_rms_p2plane+float(rms_p2plane)
                psnrpoint=pelines[28].split(' ')
                psnr_point=psnrpoint[4].split(',')
                psnr_p2point=psnr_point[3]
                sum_psnr_p2point=sum_psnr_p2point+float(psnr_p2point)
                psnrplane=pelines[30].split(' ')
                psnr_plane=psnrplane[4].split(',')
                psnr_p2plane=psnr_plane[3]
                sum_psnr_p2plane=sum_psnr_p2plane+float(psnr_p2plane)
        ave_num=sum_num/FRAME
        ave_volume=sum_volume/FRAME
        ave_density=sum_density/FRAME
        ave_bin_size=sum_bin_size/FRAME
        ave_bin_Mbit=float(ave_bin_size)/1000000
        ave_rms_p2point=sum_rms_p2point/FRAME
        ave_rms_p2plane=sum_rms_p2plane/FRAME
        ave_psnr_p2point=sum_psnr_p2point/FRAME
        ave_psnr_p2plane=sum_psnr_p2plane/FRAME
        ave_encode_time=sum_encode_time/FRAME
        ave_decode_time=sum_decode_time/FRAME
        logs.append([pqs,en_pqs,ave_num,ave_volume,ave_density,ave_bin_Mbit,ave_rms_p2point,ave_rms_p2plane,ave_psnr_p2point,ave_psnr_p2plane,ave_encode_time,ave_decode_time])
        print("pqs,num_points,volume,density bin_size(Mbit),rms_p2point,rms_p2plane,psnr_p2point,psnr_p2plane,encode_time,decode_time {} {} {} {} {} {} {} {} {} {} {}".format(en_pqs,ave_num,ave_volume,ave_density,ave_bin_Mbit,ave_rms_p2point,ave_rms_p2plane,ave_psnr_p2point,ave_psnr_p2plane,ave_encode_time,ave_decode_time))
    writer.writerows(logs)
