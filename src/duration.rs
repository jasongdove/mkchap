use std::path::PathBuf;
use std::time::Duration;

use crate::error::MkChapError;

pub fn get_duration(input: PathBuf) -> Result<Duration, MkChapError> {
    let exists = input.exists();
    let path_string = input
        .into_os_string()
        .into_string()
        .unwrap_or("input file".to_string());

    if !exists {
        return Err(MkChapError::FileNotFound(path_string));
    }

    let output = std::process::Command::new("ffprobe")
        .args([
            "-v",
            "panic",
            "-show_entries",
            "format=duration",
            "-of",
            "default=nw=1:nokey=1",
            path_string.as_str(),
        ])
        .output()
        .map_err(|_| MkChapError::FfProbeFailed)?;

    if !output.status.success() {
        return Err(MkChapError::FfProbeFailed);
    }

    String::from_utf8(output.stdout)
        .map_err(|_| MkChapError::FfProbeFailed)
        .and_then(|s| {
            s.trim()
                .parse::<f64>()
                .map_err(|_| MkChapError::InvalidDuration)
        })
        .map(Duration::from_secs_f64)
        .and_then(|d| {
            if d.is_zero() {
                Err(MkChapError::InvalidDuration)
            } else {
                Ok(d)
            }
        })
}
