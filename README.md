# mkchap

This is a small script to detect and add/replace chapter metadata in a video.

## Dependencies

This script requires ffprobe and ffmpeg

## Usage

```bash
./mkchap.py -i <inputfile> -o <outputfile>
```

## Customizing

There are two parameters at the top of the script that can be used to customize the black detection: `min_black_seconds` and `ratio_black_pixels`:

- `min_black_seconds` is the minimum number of seconds the video should be black to insert a chapter marker
- `ratio_black_pixels` is the minimum ratio of black pixels to non-black pixels to consider a frame to be "black"
