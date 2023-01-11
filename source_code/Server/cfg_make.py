import os
import csv
content_name="ted"#"greeting"#"slab_chair"#"ted"
os.system("mkdir "+content_name)
dir_name="PSNR_p2point"#"bitrate"#PSNR_p2point
os.system("mkdir "+content_name+"/"+dir_name)
os.system("mkdir "+content_name+"/"+dir_name+"/ex")
os.system("mkdir "+content_name+"/"+dir_name+"/cfg")
#csv_file:value of estimated pqs
csv_file="uniform_bitrate_psnr/"+content_name+"_pqs_"+dir_name+".csv"
cfg_path="cfg/octree-predlift/lossy-geom-lossy-attrs/longdress_vox10_1300/r01/"
with open(csv_file,"r") as f:
    lines=f.read().split('\n')
for i in range(16):#2
    os.system("mkdir "+content_name+"/"+dir_name+"/ex/"+str(i))
    os.system("mkdir "+content_name+"/"+dir_name+"/cfg/"+str(i))
    os.system("cp "+cfg_path+"*.cfg "+content_name+"/"+dir_name+"/ex/"+str(i)+"/")
    line=lines[i+1].split(',')
    pqs=line[2]
    os.system("cat "+content_name+"/"+dir_name+"/ex/"+str(i)+"/encoder.cfg | sed s/0.125/"+str(pqs)+"/ > "+content_name+"/"+dir_name+"/cfg/"+str(i)+"/encoder.cfg")
