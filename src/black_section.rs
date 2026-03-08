use std::fmt::{Display, Formatter};

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
            BlackSectionState::OutsideOfWindows => write!(f, "Outside Of Windows")
        }
    }
}

pub struct BlackSection {
    start: f64,
    finish: f64,
    state: BlackSectionState,
}

impl BlackSection {
    pub fn new(start: f64, finish: f64, state: BlackSectionState) -> BlackSection {
        BlackSection {
            start,
            finish,
            state,
        }
    }

    pub fn midpoint(&self) -> f64 {
        self.start + (self.finish - self.start) / 2.0
    }
}

impl Display for BlackSection {
    fn fmt(&self, f: &mut Formatter<'_>) -> std::fmt::Result {
        write!(f, "({}-{} => {})", self.start, self.finish, self.state)
    }
}
