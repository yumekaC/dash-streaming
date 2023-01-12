# Make dash representations

1. Run python codes in the following order:

  cfg_make.py → processing.py → pack_pc_seg.py → seg_average.py

2. Copy data to published server 

3. In client side, capture compressed point cloud objects in the different position

4. Run front_ms-ssim.py

5. Create fitting curves (distance → MS-SSIM) and csv file including coefficients (We use Excel slover)

6. Run make_ave_json.py and
