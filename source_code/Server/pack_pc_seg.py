import os
import json
import base64
content_name='greeting'#'ted'#'spool'#'telecon'#'bear'#'racecar'#'slab_chair'#'greeting'
dir_name='bitrate'#'PSNR_p2point'#'bitrate'
for pqs in range(16):
    in_tar_dir = content_name+'/'+dir_name+'/'+str(pqs)+'/bin'#
    input_ext = '.bin'
    out_tar_dir = content_name+'/'+dir_name+'/'+str(pqs)+'/bin_seg'#
    output_ext = '.seg'
    out_meta_name = 'metadata.json'

    fps = 30#30#2#5
    seg_num = 9#1#9##10#5

    ### set input and output path
    in_pc_path = os.path.join(in_pc_dir, in_tar_dir)
    out_pc_path = os.path.join(out_pc_dir, out_tar_dir)
    print ("Input path: {}".format(in_pc_path))
    print ("Output path: {}".format(out_pc_path))
    
    def check_directory():
        ### check input directory
        if os.path.exists(in_pc_path) == False:
            print ("Input path is invalide")
            print ("Please set correct path")
            os.exit(1)
        else:
            pass

        ### check output directory
        if os.path.exists(out_pc_path) == False:
            os.makedirs(out_pc_path)

    def pack_point_cloud_sequence():
        metadata = {'input_path': in_pc_path,
                    'output_path': out_pc_path,
                    'pqs_id': pqs,
                    'frame_rate': fps,
                    'num_of_seg': seg_num,
                    'bitrate_lists': 'null'
                    }

        bps_lists = []
        for i in range(seg_num):
    
            temp = []

            for j in range(fps):
                frame_num = i*fps + j
                print ("frame num: {}".format(frame_num))
                in_frame_path = os.path.join(in_pc_path, str(frame_num) + input_ext)
                #in_frame_path = os.path.join(in_pc_path, str(0) + input_ext)#0#car_frame
                with open(in_frame_path, 'rb') as f:
                    bin_pc = f.read()

                #bytes => base645
                b64_pc = base64.b64encode(bin_pc).decode('utf-8')
                temp.append(b64_pc)

            pc_seg = {'seg num': i, 'payload': temp}
            _pc_seg = json.dumps(pc_seg)

            bin_pc_seg = _pc_seg.encode('utf-8')

            out_frame_path = os.path.join(out_pc_path, str(i) + output_ext)
            with open(out_frame_path, 'wb') as f:
                f.write(bin_pc_seg)

            #bitrate = int(os.path.getsize(out_frame_path)) * 8 ## bytes => bits
            bitrate = float(int(os.path.getsize(out_frame_path)) * 8)/1000000 ## bytes => Mbps
            bps_lists.append({'seg_num': i, 'bitrate': bitrate})
        
            print ("seg num: {}, bitrate: {}Mbps".format(i, bitrate))

        metadata['bitrate_lists'] = bps_lists
        print (json.dumps(metadata, indent=2))
        
        ### output metafile
        out_metadata_path = os.path.join(out_pc_path, out_meta_name)
        with open(out_metadata_path, 'w') as f:
            json.dump(metadata, f, ensure_ascii=False, indent=2)

    def unpack_point_cloud_sequence():
        for i in range(seg_num):

            tar_frame_path = os.path.join(out_pc_path, str(i) + output_ext)
            with open(tar_frame_path, 'rb') as f:
                bin_pc_seg = f.read()

            #bytes => json
            pc_seg = bin_pc_seg.decode('utf-8')
            _pc_seg = json.loads(pc_seg)

            seg_id = _pc_seg['seg num']
            num_frames = len(_pc_seg['payload'])

            print ('seg num {}, num of frames {}'.format(seg_id, num_frames))
            for j in range(num_frames):

                frame_id = i*num_frames + j

                print ("frame num: {}".format(frame_id))
                frame_path = os.path.join(str(frame_id) + input_ext)
                #base64 => bytes
                bin_pc = base64.b64decode(_pc_seg['payload'][j].encode('utf-8'))

                with open(frame_path, 'wb') as f:
                    f.write(bin_pc)

            print ("output complete")


    def main():

        check_directory()
        pack_point_cloud_sequence()
        #unpack_point_cloud_sequence()

    if __name__ == '__main__':
        main()
