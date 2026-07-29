# Offline time validation

All persisted timestamps use round-trip UTC (`O`). The start is the newest valid value among last simulation, save update, session end and application pause. Local timezone is never part of the calculation.

Intervals below five minutes are ignored. Small negative intervals inside the rollback tolerance are treated as zero; larger clock rollbacks produce no simulation. Long forward jumps are reported and effective duration is capped at 48 hours. Actual, effective and truncated durations remain available in the report.

Pause and focus can arrive in either order on mobile. `SaveCoordinator` opens one background interval and resumes only after the application is both focused and unpaused.
