use crate::error::MkChapError;

#[derive(Debug, Clone)]
pub struct Window {
    start: f64,
    finish: f64,
}

impl Window {
    pub fn contains(&self, midpoint: f64) -> bool {
        midpoint >= self.start && midpoint <= self.finish
    }
}

pub fn window_parser(s: &str) -> Result<Window, MkChapError> {
    if !s.contains(':') {
        return Err(MkChapError::InvalidWindow);
    }

    let spots: Vec<f64> = s.split(':').flat_map(|s| s.parse::<f64>().ok()).collect();

    if spots.len() != 2 {
        return Err(MkChapError::InvalidWindow);
    }

    let start = spots[0];
    let finish = spots[1];

    if start >= finish {
        return Err(MkChapError::WindowStartBeforeFinish);
    }

    Ok(Window { start, finish })
}
