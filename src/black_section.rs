use std::fmt::{Display, Formatter};
use std::time::Duration;

use serde::{Serialize, Serializer};

#[derive(Serialize)]
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

#[derive(Serialize)]
#[serde(rename_all(serialize = "PascalCase"))]
pub struct BlackSection {
    #[serde(serialize_with = "serialize_duration")]
    start: Duration,
    #[serde(serialize_with = "serialize_duration")]
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

fn duration_to_string(duration: &Duration) -> String {
    let total_seconds = duration.as_secs();
    let hours = total_seconds / 3600;
    let minutes = (total_seconds % 3600) / 60;
    let seconds = total_seconds % 60;
    let nanos = duration.subsec_nanos() / 100;
    format!("{hours:02}:{minutes:02}:{seconds:02}.{nanos:07}")
}

fn serialize_duration<S>(duration: &Duration, serializer: S) -> Result<S::Ok, S::Error>
where
    S: Serializer,
{
    serializer.serialize_str(duration_to_string(duration).as_str())
}
