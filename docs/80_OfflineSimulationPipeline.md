# Offline simulation pipeline

The stable order is fish needs, aggregate bioload, simplified nitrogen conversion, water compounds, filter condition, cycle progress, welfare, event aggregation, journal entries, statistics and timestamps.

Calculations are direct hourly formulas over the effective duration. There are no lost-frame loops or random values, so identical input, policy and interval produce identical output. Inputs and outputs are clamped and non-finite values receive safe fallbacks.

The pipeline mutates the loaded domain DTO once. Runtime objects are restored afterward by `SaveMapper`; the coordinator then saves the new interval marker immediately. A failed save leaves the coordinator dirty for retry.
