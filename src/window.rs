use std::time::Duration;

#[derive(Debug, Clone)]
pub struct Window {
    start: Duration,
    finish: Duration,
}

pub fn window_parser(s: &str) -> Result<Window, String> {
    if s.contains(':') {
        let spots: Vec<Duration> = s
            .split(':')
            .flat_map(|s| s.parse::<f64>().ok())
            .map(Duration::from_secs_f64)
            .collect();

        if spots.len() != 2 {
            Err("Invalid window".to_string())
        } else {
            let start = spots[0];
            let finish = spots[1];

            if start >= finish {
                Err("window start must be before window finish".to_string())
            } else {
                Ok(Window {
                    start: spots[0],
                    finish: spots[1],
                })
            }
        }
    } else {
        Err("Invalid window".to_string())
    }
}
