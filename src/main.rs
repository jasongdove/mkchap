mod black_detect;
mod black_section;
mod duration;
mod error;
mod window;

use std::path::PathBuf;

use crate::black_section::BlackSection;
use crate::error::MkChapError;
use crate::window::Window;
use clap::Parser;
use serde::Serialize;

#[derive(Debug, Parser)]
struct Args {
    input: PathBuf,

    #[arg(
        short = 's',
        long = "seconds",
        help = "minimum number of seconds the video must be black",
        default_value_t = 1.0
    )]
    min_black_seconds: f64,

    #[arg(
        short = 'r',
        long = "ratio",
        help = "ratio (0 to 1) of black pixels to non-black pixels to treat a frame as black",
        default_value_t = 0.9,
        value_parser = black_pixel_ratio_in_range
    )]
    ratio_black_pixels: f64,

    #[arg(
        short = 'b',
        long = "black",
        help = "maximum luminance (0 to 1) for which a pixel is considered black; increasing this will treat dark greys as black",
        default_value_t = 0.1,
        value_parser = black_pixel_threshold_in_range
    )]
    black_pixel_threshold: f64,

    #[arg(
        short = 'w',
        long = "windows",
        help = "comma-separated list of second ranges; any black sections outside of these ranges will be ignored",
        value_parser = window::window_parser,
        value_delimiter = ','
    )]
    windows: Vec<Window>,
}

#[derive(Serialize)]
#[serde(rename_all(serialize = "PascalCase"))]
struct AnalysisResult {
    black_sections: Vec<BlackSection>,
}

fn main() {
    let args = Args::parse();
    if let Err(err) = run(args) {
        eprintln!("{err}");
        std::process::exit(1);
    }
}

fn run(args: Args) -> Result<(), MkChapError> {
    let duration = duration::get_duration(args.input.clone())?;

    let valid_windows = args
        .windows
        .iter()
        .flat_map(|w| w.adjust_by(duration))
        .collect();

    let detect_result = black_detect::black_detect(
        args.input,
        args.min_black_seconds,
        args.ratio_black_pixels,
        args.black_pixel_threshold,
        valid_windows,
    )?;

    let analysis_result = AnalysisResult {
        black_sections: detect_result,
    };

    let output =
        serde_json::to_string_pretty(&analysis_result).unwrap_or("failed to serialize".to_string());

    println!("{output}");

    Ok(())
}

fn black_pixel_threshold_in_range(s: &str) -> Result<f64, String> {
    validate_float(s, "black pixel threshold")
}

fn black_pixel_ratio_in_range(s: &str) -> Result<f64, String> {
    validate_float(s, "ratio of black pixels")
}

fn validate_float(s: &str, description: &str) -> Result<f64, String> {
    let value: f64 = s
        .parse()
        .map_err(|_| format!("`{s}` isn't a valid {description}"))?;

    if (0.0..=1.0).contains(&value) {
        Ok(value)
    } else {
        Err(format!("{description} not in range 0-1"))
    }
}
