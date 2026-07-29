# Offline water simulation

`BioloadCalculator` derives a bounded load from fish count, water volume and filter efficiency. The gameplay model then advances waste through ammonia, nitrite and nitrate in that order; conversion depends on filter efficiency and nitrogen-cycle progress.

Filter dirt increases with bioload and efficiency decreases gradually. Cycle progress advances at a capped rate. Temperature, pH, GH and KH remain unchanged because Acuaria currently has no persisted offline mechanism that should alter them.

This is a simplified educational gameplay model, not a laboratory prediction. Per-session and absolute clamps prevent negative, NaN, infinite or extreme compound values.
