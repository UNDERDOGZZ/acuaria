using UnityEngine;
using UnityEngine.UI;

namespace Acuaria.UI.Aquarium
{
    public enum AquariumHUDLayoutMode
    {
        Wide,
        Compact
    }

    public sealed class AquariumHUDResponsiveLayout : MonoBehaviour
    {
        [SerializeField] private RectTransform availableArea;
        [SerializeField] private RectTransform topBar;
        [SerializeField] private RectTransform wideRow;
        [SerializeField] private RectTransform compactStack;
        [SerializeField] private RectTransform compactPrimaryRow;
        [SerializeField] private RectTransform identityGroup;
        [SerializeField] private RectTransform flexibleSpacer;
        [SerializeField] private RectTransform statsGroup;
        [SerializeField] private RectTransform backButton;
        [SerializeField] private RectTransform detailsButton;
        [SerializeField, Min(800f)] private float compactBreakpoint = 2100f;
        private float lastWidth = -1f;

        public AquariumHUDLayoutMode Mode { get; private set; }
        public float CompactBreakpoint => compactBreakpoint;

        public void Configure(RectTransform area, RectTransform bar, RectTransform wide, RectTransform stack,
            RectTransform primaryRow, RectTransform identity, RectTransform spacer,
            RectTransform stats, RectTransform back, RectTransform details, float breakpoint)
        {
            availableArea = area;
            topBar = bar;
            wideRow = wide;
            compactStack = stack;
            compactPrimaryRow = primaryRow;
            identityGroup = identity;
            flexibleSpacer = spacer;
            statsGroup = stats;
            backButton = back;
            detailsButton = details;
            compactBreakpoint = Mathf.Max(800f, breakpoint);
        }

        private void OnEnable() => Refresh();
        private void OnRectTransformDimensionsChange() => Refresh();

        public AquariumHUDLayoutMode ChooseMode(float availableWidth)
        {
            if (!float.IsFinite(availableWidth) || availableWidth <= 0f) return AquariumHUDLayoutMode.Compact;
            return availableWidth < compactBreakpoint ? AquariumHUDLayoutMode.Compact : AquariumHUDLayoutMode.Wide;
        }

        public bool Refresh()
        {
            if (availableArea == null || topBar == null) return false;
            var width = availableArea.rect.width;
            var next = ChooseMode(width);
            if (Mathf.Approximately(lastWidth, width) && next == Mode) return false;
            lastWidth = width;
            Apply(next);
            return true;
        }

        public void SetBreakpoint(float value)
        {
            compactBreakpoint = Mathf.Max(800f, value);
            lastWidth = -1f;
        }

        private void Apply(AquariumHUDLayoutMode mode)
        {
            Mode = mode;
            var compact = mode == AquariumHUDLayoutMode.Compact;
            topBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, compact ? 206f : 112f);

            if (compact)
            {
                compactStack.gameObject.SetActive(true);
                compactPrimaryRow.gameObject.SetActive(true);
                backButton.SetParent(compactPrimaryRow, false);
                identityGroup.SetParent(compactPrimaryRow, false);
                flexibleSpacer.SetParent(compactPrimaryRow, false);
                detailsButton.SetParent(compactPrimaryRow, false);
                statsGroup.SetParent(compactStack, false);
                wideRow.gameObject.SetActive(false);
            }
            else
            {
                wideRow.gameObject.SetActive(true);
                backButton.SetParent(wideRow, false);
                identityGroup.SetParent(wideRow, false);
                flexibleSpacer.SetParent(wideRow, false);
                statsGroup.SetParent(wideRow, false);
                detailsButton.SetParent(statsGroup, false);
                compactStack.gameObject.SetActive(false);
            }
            LayoutRebuilder.MarkLayoutForRebuild(topBar);
        }
    }
}
