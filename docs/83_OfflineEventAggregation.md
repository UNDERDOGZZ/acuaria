# Offline event aggregation

The pipeline emits only relevant recommendations and warnings such as feeding, water testing or filter cleaning. Events use a stable aquarium-plus-key identity, priority ordering and deterministic tie-breaking.

`OfflineEventAggregator` removes duplicates and limits output; the default maximum journal count is four. `OfflineJournalGenerator` writes those consolidated items to the corresponding aquarium only. Reapplying the same interval produces no additional entries.

Action missions, rewards and achievements are intentionally not inferred from elapsed time. They remain driven by verified player actions; offline statistics count sessions, effective seconds and capped sessions.
