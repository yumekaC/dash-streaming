import os
import json
import base64
import json
import csv

content_name='greeting'#'ted'#'spool'#'racecar'#'slab_chair'#'greeting'#'telecon'#'bear'
dir_name='bitrate'#'PSNR_p2point'#'bitrate'
fps = 30#10#30#5
seg_num = 9#1#9#10#5
max_buffer = 10#10#20#1
min_buffer = 5#5#5#3
rate_control = 0#1
in_metajson_dir = '/XXXX/ply_dataset/pqs/'+content_name+'/'+dir_name #published server path
csv_filename=content_name+"/"+content_name+"_"+dir_name+"_inf.csv"
curve_filename=content_name+"/"+content_name+"_"+dir_name+"_curve.csv"
def make_ave_json():
    ave_data = {'frame_rate':fps,
                'num_of_seg':seg_num,
                'max_buffer':max_buffer,
                'min_buffer':min_buffer,
                'rate_control':rate_control,
                'ave_bitrate_lists':'null'
               }
    ave_bps_lists = []

    with open(csv_filename,'r') as pf:
        lines=pf.read().split('\n')
    with open(curve_filename,'r') as cf:
        curves=cf.read().split('\n')
    for j in range(16):#4
        pqs=j
        metajson_path=os.path.join(in_metajson_dir+'/'+str(pqs)+'/bin_seg/metadata.json')
        json_open=open(metajson_path,'r')
        json_load=json.load(json_open)
        sum_bitrate=0
        for seg in range(seg_num):
            sum_bitrate=sum_bitrate+json_load['bitrate_lists'][seg]['bitrate']
        ave_bitrate=sum_bitrate/seg_num
        line=lines[j+1].split(',')
        curve=curves[j+1].split(',')
        ave_bps_lists.append({'pqs':pqs,'ave_bitrate':ave_bitrate,'ave_psnr_p2point':line[8],'ave_psnr_p2plane':line[9],'curve_a':curve[1],'curve_b':curve[2],'curve_c':curve[3]})
    ### output metafile
    ave_data['ave_bitrate_lists'] = ave_bps_lists
    out_metadata_path = os.path.join(in_metajson_dir+'/ave_metadata.json')
    with open(out_metadata_path, 'w') as f:
        json.dump(ave_data, f, ensure_ascii=False, indent=2)

def main():
    make_ave_json()

if __name__ == '__main__':
        
    main()
