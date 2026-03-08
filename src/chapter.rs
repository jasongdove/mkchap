use std::time::Duration;

use serde::Serialize;

use crate::black_section::{BlackSection, BlackSectionState, serialize_duration};

#[derive(Serialize)]
#[serde(rename_all(serialize = "PascalCase"))]
pub struct Chapter {
    #[serde(serialize_with = "serialize_duration")]
    start: Duration,
    #[serde(serialize_with = "serialize_duration")]
    finish: Duration,
}

pub fn from_black_sections(black_sections: &Vec<BlackSection>, duration: Duration) -> Vec<Chapter> {
    let valid_sections: Vec<&BlackSection> = black_sections
        .iter()
        .filter(|s| s.state == BlackSectionState::Ok)
        .collect();

    let mut markers: Vec<Duration> = Vec::with_capacity(valid_sections.len() * 2 + 2);
    markers.push(Duration::new(0, 0));

    for section in valid_sections {
        markers.push(section.midpoint());
        markers.push(section.midpoint());
    }

    markers.push(duration);

    markers
        .chunks(2)
        .map(|c| Chapter {
            start: c[0],
            finish: c[1],
        })
        .collect()
}
