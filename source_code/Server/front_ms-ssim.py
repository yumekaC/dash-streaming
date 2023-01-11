import sys
import csv
from pytorch_msssim import ssim, ms_ssim, SSIM, MS_SSIM
from PIL import Image
import numpy as np
import torch
import os

def main():    
    content_name="ted"#"slab_chair"#"ted"
    dir_name="PSNR_p2point"#"bitrate"#"PSNR_p2point"
    csv_filename=content_name+"/ms-ssim_front_"+content_name+"_"+dir_name+".csv"
    with open(csv_filename,'w',newline='') as f:
            writer = csv.writer(f)
            logs=[]
            logs.append(["pqs_id","distance","SSIM","MS_SSIM"])
            # Loading image data (COLOR)
            y_list=[0,1,-1]
            for pqs in range(16):
                for i in range(41):
                    dis=0.5+float(i)/20
                    if(dis==1.0):
                        dis=1
                    elif(dis==2.0):
                        dis=2
                    #filename1:image of non-compress point cloud path
                    filename1 = content_name+"/"+content_name+"_original/"+content_name+"_ori_"+str(dis)+".png"
                    #filename2:image of compressed point cloud path  
                    filename2 = content_name+"/"+content_name+"_"+dir_name+"_"+str(pqs)+"_"+str(dis)+".png"
                    img = Image.open(filename1)
                    img = np.array(img).astype(np.float32)
                    img_torch = torch.from_numpy(img).unsqueeze(0).permute(0, 3, 1, 2)  # 1, C, H, W
                    X=img_torch
                    img = Image.open(filename2)
                    img = np.array(img).astype(np.float32)
                    img_torch = torch.from_numpy(img).unsqueeze(0).permute(0, 3, 1, 2)  # 1, C, H, W
                    Y=img_torch

                    # Evaluation with SSIM and MS-SSIM
                    print("distance:"+str(dis))
                    ssim=SSIM(X, Y)
                    ms_ssim=MS_SSIM(X, Y)
                    logs.append([pqs,dis,ssim,ms_ssim])
            #print(logs)
            writer.writerows(logs)

def SSIM(X,Y):
    ssim_val = ssim( X, Y, data_range=255, size_average=False) # return (N,)
    f_ssim=float(ssim_val.numpy())
    return f_ssim

def MS_SSIM(X,Y):
    # calculate ssim & ms-ssim for each image
    ms_ssim_val = ms_ssim( X, Y, data_range=255, size_average=False ) #(N,)
    f_ms_ssim=float(ms_ssim_val.numpy())
    return f_ms_ssim

if __name__ == "__main__":
    main()
