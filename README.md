# mkchap

This is a small script to detect and add/replace chapter metadata in a video.

## Dependencies

This script requires ffprobe and ffmpeg

## Usage

```bash
./mkchap.py -i <inputfile> -o <outputfile> -s <min_black_seconds> -r <ratio_black_pixels> -b <black_pixel_threshold>
```

- `min_black_seconds` is the minimum number of seconds the video should be black to insert a chapter marker
  - A good starting value is `1.0`
- `ratio_black_pixels` is the minimum ratio (0 to 1) of black pixels to non-black pixels to consider a frame to be "black"
  - A good starting value is `0.9`
- `black_pixel_threshold` is the maximum luminance (0 to 1) for which a pixel is considered black. increasing this will detect dark greys as black
  - A good starting value is `0.0`

