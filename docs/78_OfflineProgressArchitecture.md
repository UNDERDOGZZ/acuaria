# Offline progress architecture

Sprint 15 adds a deterministic domain-only offline flow. `SaveCoordinator` acts as the coordinator: it loads and migrates the save, validates the UTC interval, runs `OfflineSimulationService`, restores runtime state, publishes a report and persists the result before regular autosave takes over.

The service uses `IOfflineTimeProvider`, `OfflineTimeValidator`, `OfflineSimulationPipeline`, `OfflineEventAggregator` and `OfflineJournalGenerator`. None depends on GameObjects, views, frame updates or `Time.deltaTime`. Cold start and background resume use the same service. Pause/focus callbacks share one open-interval flag, preventing duplicate resume executions.

Idempotence is enforced by the last applied interval, an execution key made from save ID, start, end and simulation version, and immediate persistence. The default policy ignores intervals below five minutes, caps effective time at 48 hours, limits deterioration and never permits offline death.
