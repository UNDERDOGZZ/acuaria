# Offline summary UI

`OfflineProgressSummaryPanel` appears only when an applied report contains relevant events. It shows effective simulated time, whether the cap was reached and consolidated recommendations. Minor or ignored intervals produce no modal.

The panel lives below `SafeArea`, above normal HUD siblings. Its `CanvasGroup` blocks UI/world raycasts only while visible and does not pause simulation. The close button is labelled `Continuar`; hidden state uses alpha 0, non-interactable and no raycast blocking.
