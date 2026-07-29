# Offline fish simulation

Each persisted fish stores hunger, satiety, stress, health and welfare. Hunger rises and satiety falls with effective hours. Severe hunger and poor water can increase stress and reduce health, but session caps constrain every change.

The default minimum offline health is 0.55 and offline death is disabled. Short intervals are ignored and long absences use the same 48-hour cap. Species-specific metabolism is not yet present in the species data, so Sprint 15 uses the documented general rate and leaves policy extension as the future override point.
