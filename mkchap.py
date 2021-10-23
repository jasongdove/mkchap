#! /usr/bin/env python3

import sys
import getopt
import subprocess
import os
from io import StringIO
import tempfile
import pathlib
import shutil

# minimum number of seconds the video should be black
min_black_seconds = 1.0

# threshold (% of pixels) for considering a picture black
ratio_black_pixels = 0.90

# threshold for considering a pixel black. may need to bump this up if black isn't fully black
pixel_threshold = 0.00


def main(argv):
    if len(argv) != 4:
        print('chapterprobe.py -i <inputfile> -o <outputfile>')
        sys.exit()

    inputfile = ''
    outputfile = ''
    try:
        opts, args = getopt.getopt(argv, "i:o:", ["input=", "output="])
    except getopt.GetoptError:
        print('chapterprobe.py -i <inputfile> -o <outputfile>')
        sys.exit(2)
    for opt, arg in opts:
        if opt in ("-i", "--input"):
            inputfile = arg
        elif opt in ("-o", "--output"):
            outputfile = arg

    process(inputfile, outputfile)


def process(inputfile, outputfile):
    duration = float(get_output(['ffprobe', '-v', 'panic', '-show_entries',
                                 'format=duration', '-of', 'default=nw=1:nokey=1', inputfile]))
    chapters = get_chapters(inputfile, duration)
    ffmetadata = get_ffmetadata(chapters)

    temp_metadata = tempfile.NamedTemporaryFile(delete=False)
    try:
        temp_metadata.write(bytes(ffmetadata, 'utf-8'))
        temp_metadata.close()

        if outputfile == inputfile:
            source_extension = pathlib.Path(inputfile).suffix
            temp_outfile = tempfile.NamedTemporaryFile(
                delete=False, suffix=source_extension)
            get_output(['ffmpeg', '-hide_banner', '-v', 'error', '-i', inputfile, '-i', temp_metadata.name,
                        '-map_metadata', '1', '-map_chapters', '1', '-codec', 'copy', '-y', temp_outfile.name])
            shutil.move(temp_outfile.name, outputfile)
        else:
            get_output(['ffmpeg', '-hide_banner', '-v', 'error', '-i', inputfile, '-i', temp_metadata.name,
                        '-map_metadata', '1', '-map_chapters', '1', '-codec', 'copy', '-y', outputfile])
    finally:
        os.remove(temp_metadata.name)

    print('created {} chapter markers'.format(max(len(chapters) - 1, 0)))


def get_chapters(inputfile, duration):
    escaped_inputfile = escape(inputfile)
    output = get_output(['ffprobe', '-f', 'lavfi', '-i', 'movie={},blackdetect=d={}:pic_th={}:pix_th={}[out0]'.format(escaped_inputfile, min_black_seconds, ratio_black_pixels, pixel_threshold),
                         '-show_entries', 'frame_tags=lavfi.black_start,lavfi.black_end', '-of', 'default=nw=1', '-v', 'panic'])
    raw_pairs = filter(lambda c: len(c) == 2, chunks(output.splitlines(), 2))
    numeric_pairs = list(map(
        lambda p: [float(p[0].split('=')[1]), float(p[1].split('=')[1])], raw_pairs))
    valid_pairs = list(filter(lambda p: p[0] > min_black_seconds and p[1] - p[0]
                         >= min_black_seconds, numeric_pairs))
    chapters = list(map(lambda p: p[0] + (p[1] - p[0]) / 2.0, valid_pairs))
    chapters = [val for val in chapters for _ in range(2)]
    chapters.insert(0, 0)
    chapters.append(duration)
    return list(chunks(chapters, 2))


def get_ffmetadata(chapters):
    ffmetadata = StringIO()
    ffmetadata.write(";FFMETADATA1\n")
    ffmetadata.write("\n")
    for i in range(len(chapters)):
        ffmetadata.write("[CHAPTER]\n")
        ffmetadata.write("TIMEBASE=1/1000\n")
        ffmetadata.write("START={}\n".format(int(chapters[i][0] * 1000)))
        ffmetadata.write("END={}\n".format(int(chapters[i][1] * 1000)))
        ffmetadata.write("title=Chapter {}\n".format(i + 1))
        ffmetadata.write("\n")
    return ffmetadata.getvalue()


def get_output(args):
    p = subprocess.Popen(args, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    out, err = p.communicate({})
    if p.returncode != 0:
        raise Exception(args[0], out, err)
    return out.decode('utf-8').strip()


def chunks(lst, n):
    for i in range(0, len(lst), n):
        yield lst[i:i + n]

def escape(text):
    return text.replace('\'', '\\\\\\\'')

if __name__ == "__main__":
    main(sys.argv[1:])
