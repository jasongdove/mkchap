use std::fmt::Formatter;

#[derive(Debug)]
pub enum MkChapError {
    FileNotFound(String),
    FfProbeFailed,
    InvalidDuration,
    InvalidWindow,
    BlackDetectFailed,
}

impl std::fmt::Display for MkChapError {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        match self {
            MkChapError::FileNotFound(path) => write!(f, "{path} does not exist"),
            MkChapError::FfProbeFailed => write!(f, "unable to determine input file duration"),
            MkChapError::InvalidDuration => write!(f, "unable to determine input file duration"),
            MkChapError::InvalidWindow => write!(f, "invalid window"),
            MkChapError::BlackDetectFailed => write!(f, "black detect failed"),
        }
    }
}

impl std::error::Error for MkChapError {}
