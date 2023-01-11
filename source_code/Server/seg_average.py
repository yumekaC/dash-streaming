import csv
import os
content_name="greeting"#"ted"#"spool"#"slab_chair"#"racecar"#"greeting"#"telecon"
dir_name="bitrate"#"PSNR_p2point"#"bitrate"
csv_filename=content_name+"/seg_"+dir_name+"_"+content_name+"_ave.csv"
seg_num=9#1#9#10#5
with open(csv_filename,'w') as f:
    writer = csv.writer(f)
    logs=[]
    logs.append(["pqs_id","bitrate[Mbps]"])
    for pqs in range(16):
        sum_bin_size=0
        for seg in range(seg_num):
            out_frame_path="/var/www/html/ply_dataset/pqs/"+content_name+"/"+dir_name+"/"+str(pqs)+"/bin_seg/"+str(seg)+".seg"
            bitrate = int(os.path.getsize(out_frame_path)) * 8 ## bytes => bits
            sum_bin_size=sum_bin_size+bitrate

        ave_bin_size=sum_bin_size/seg_num
        bitrate=float(ave_bin_size)/1000000

        logs.append([pqs,bitrate])
        print("pqs, bitrate(Mbps) {} {}".format(pqs,bitrate))
    writer.writerows(logs)
