use std::path::PathBuf;

use crate::black_section::{BlackSection, BlackSectionState};
use crate::error::MkChapError;
use crate::window::Window;

pub fn black_detect(
    input: PathBuf,
    min_black_seconds: f64,
    ratio_black_pixels: f64,
    black_pixel_threshold: f64,
    windows: Vec<Window>,
) -> Result<Vec<BlackSection>, MkChapError> {
    let path_string = input
        .into_os_string()
        .into_string()
        .map_err(|_| MkChapError::BlackDetectFailed)?;

    let fixed_path_string = fix_path_string(path_string);

    let output = std::process::Command::new("ffprobe")
        .args([
            "-f",
            "lavfi",
            "-i",
            format!("movie={fixed_path_string},blackdetect=d={min_black_seconds}:pic_th={ratio_black_pixels}:pix_th={black_pixel_threshold}[out0]").as_str(),
            "-show_entries",
            "frame_tags=lavfi.black_start,lavfi.black_end",
            "-of",
            "default=nw=1",
            "-v",
            "panic"
        ])
        .output()
        .map_err(|_| MkChapError::BlackDetectFailed)?;

    if !output.status.success() {
        return Err(MkChapError::BlackDetectFailed);
    }

    String::from_utf8(output.stdout)
        .map_err(|_| MkChapError::BlackDetectFailed)
        .and_then(|s| get_black_sections(s, min_black_seconds, windows))
}

fn get_black_sections(
    output: String,
    min_black_seconds: f64,
    windows: Vec<Window>,
) -> Result<Vec<BlackSection>, MkChapError> {
    let floats = output
        .lines()
        .flat_map(split_and_parse)
        .collect::<Vec<f64>>();

    let chunks: Vec<&[f64]> = floats.chunks(2).filter(|c| c.len() == 2).collect();

    if chunks.is_empty() {
        return Err(MkChapError::BlackDetectFailed);
    }

    let midpoints = chunks
        .iter()
        .map(|c| {
            let start = c[0];
            let finish = c[1];
            let midpoint: f64 = start + (finish - start) / 2.0;
            if start < min_black_seconds || (finish - start) < min_black_seconds {
                BlackSection::new(start, finish, BlackSectionState::TooShort)
            } else if windows.iter().all(|w| !w.contains(midpoint)) {
                BlackSection::new(start, finish, BlackSectionState::OutsideOfWindows)
            } else {
                BlackSection::new(start, finish, BlackSectionState::Ok)
            }
        })
        .collect();

    Ok(midpoints)
}

fn split_and_parse(line: &str) -> Option<f64> {
    let trimmed = line.trim();

    if trimmed.is_empty() || !trimmed.contains('=') {
        return None;
    }

    trimmed
        .split('=')
        .next_back()
        .and_then(|s| s.parse::<f64>().ok())
}

fn fix_path_string(path_string: String) -> String {
    let file_name = match std::env::consts::OS {
        "windows" => path_string.replace("\\", "/").replace(":/", "\\:/"),
        _ => path_string,
    };

    file_name.replace("'", "\\\\\\'").replace(",", "\\\\\\,")
}
