use std::time::Duration;

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

    pub fn adjust_by(&self, duration: Duration) -> Option<Window> {
        let start = if self.start < 0.0 {
            duration.as_secs_f64() - self.start
        } else {
            self.start
        };

        let finish = if self.finish < 0.0 {
            duration.as_secs_f64() - self.finish
        } else {
            self.finish
        };

        if start >= finish {
            None
        } else {
            Some(Window { start, finish })
        }
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

    Ok(Window {
        start: spots[0],
        finish: spots[1],
    })
}
