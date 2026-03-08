use std::fmt::{Display, Formatter};
use std::time::Duration;

pub enum BlackSectionState {
    Ok,
    TooShort,
    OutsideOfWindows,
}

impl Display for BlackSectionState {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        match self {
            BlackSectionState::Ok => write!(f, "Ok"),
            BlackSectionState::TooShort => write!(f, "Too Short"),
            BlackSectionState::OutsideOfWindows => write!(f, "Outside Of Windows"),
        }
    }
}

pub struct BlackSection {
    start: Duration,
    finish: Duration,
    state: BlackSectionState,
}

impl BlackSection {
    pub fn new(start: f64, finish: f64, state: BlackSectionState) -> BlackSection {
        BlackSection {
            start: Duration::from_secs_f64(start),
            finish: Duration::from_secs_f64(finish),
            state,
        }
    }
}

impl Display for BlackSection {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        write!(
            f,
            "({}-{} => {})",
            duration_to_string(self.start),
            duration_to_string(self.finish),
            self.state
        )
    }
}

fn duration_to_string(duration: Duration) -> String {
    let total_seconds = duration.as_secs();
    let hours = total_seconds / 3600;
    let minutes = (total_seconds % 3600) / 60;
    let seconds = total_seconds % 60;
    let millis = duration.subsec_millis();
    format!("{hours:02}:{minutes:02}:{seconds:02}.{millis:03}")
}
