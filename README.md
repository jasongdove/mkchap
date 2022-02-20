# mkchap

This is a small app to detect and add/replace chapter metadata in a video.

## Dependencies

This app requires ffprobe and ffmpeg

## Usage

```bash
./mkchap -i <inputfile> -s <min_black_seconds> -r <ratio_black_pixels> -b <black_pixel_threshold> -w <window_list> [-o <outputfile>]
```

- `min_black_seconds` is the minimum number of seconds the video should be black to insert a chapter marker
  - A good starting value is `1.0`
- `ratio_black_pixels` is the minimum ratio (0 to 1) of black pixels to non-black pixels to consider a frame to be "black"
  - A good starting value is `0.9`
- `black_pixel_threshold` is the maximum luminance (0 to 1) for which a pixel is considered black. increasing this will detect dark greys as black
  - A good starting value is `0.0`
- `window_list` is a comma-separated list of second ranges (i.e. `55-65,505-515`). any black sections occurring outside of these windows will be ignored

If no output file is specified, mkchap will operate in a "dry-run" mode and print the detected black sections and resulting chapters.