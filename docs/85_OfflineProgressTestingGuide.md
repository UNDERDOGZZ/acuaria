# Offline progress testing guide

EditMode coverage includes minimum duration, rollback tolerance, large rollback, duration cap, deterministic output, idempotence, safety/no death, empty aquariums, three independent aquariums, event limits and v1-to-v2 migration.

For manual validation, duplicate a save outside the application, adjust only valid UTC timestamps and recalculate integrity through the save system. Test 10 minutes, 1, 8, 24, 48 and more than 48 hours; then test a future timestamp and both pause/focus callback orders. Confirm one report, one statistics increment and one persisted execution key.

Verify all three aquariums independently, including an empty one. Inspect fish needs, ammonia/nitrite/nitrate, filter dirt, cycle progress, welfare, journal count, summary visibility and immediate save. Reload the same file and confirm no value changes. Finally run EditMode tests and inspect Console for errors and missing references.
