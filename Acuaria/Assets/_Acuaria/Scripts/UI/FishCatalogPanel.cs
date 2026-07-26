using System;
using System.Collections.Generic;
using Acuaria.Fish;
using Acuaria.Fish.Care;
using UnityEngine;
using UnityEngine.UI;

namespace Acuaria.UI
{
    public sealed class FishCatalogPanel : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] FishCatalogController controller;
        [SerializeField] Button closeButton, filterButton, sortButton;
        [SerializeField] Text filterLabel, sortLabel, emptyLabel;
        [SerializeField] Button[] speciesButtons = Array.Empty<Button>();
        [SerializeField] Text[] speciesLabels = Array.Empty<Text>();
        [SerializeField] FishSpeciesDetailPanel detailPanel;
        SwimmingLevel? zoneFilter;
        bool alphabetical;
        public bool IsOpen { get; private set; }

        public void Configure(CanvasGroup group, FishCatalogController catalog, Button close, Button filter, Text filterText,
            Button sort, Text sortText, Text empty, Button[] buttons, Text[] labels, FishSpeciesDetailPanel details)
        {
            canvasGroup = group; controller = catalog; closeButton = close; filterButton = filter; filterLabel = filterText;
            sortButton = sort; sortLabel = sortText; emptyLabel = empty; speciesButtons = buttons ?? Array.Empty<Button>();
            speciesLabels = labels ?? Array.Empty<Text>(); detailPanel = details;
        }

        void OnEnable()
        {
            closeButton?.onClick.AddListener(Close); filterButton?.onClick.AddListener(CycleFilter); sortButton?.onClick.AddListener(ToggleSort);
            if (controller != null) controller.Changed += Render;
            BindSpeciesButtons(); Render();
        }

        void OnDisable()
        {
            closeButton?.onClick.RemoveListener(Close); filterButton?.onClick.RemoveListener(CycleFilter); sortButton?.onClick.RemoveListener(ToggleSort);
            if (controller != null) controller.Changed -= Render;
            UnbindSpeciesButtons();
        }

        public void Open() { SetOpen(true); Render(); }
        public void Close() { detailPanel?.Close(); SetOpen(false); }
        public void SetOpen(bool open)
        {
            IsOpen = open; gameObject.SetActive(open);
            if (canvasGroup == null) return;
            canvasGroup.alpha = open ? 1 : 0; canvasGroup.interactable = open; canvasGroup.blocksRaycasts = open;
        }

        void CycleFilter()
        {
            zoneFilter = zoneFilter switch
            {
                null => SwimmingLevel.Upper,
                SwimmingLevel.Upper => SwimmingLevel.Middle,
                SwimmingLevel.Middle => SwimmingLevel.Lower,
                _ => null
            };
            controller?.Refresh(zone: zoneFilter); Render();
        }

        void ToggleSort() { alphabetical = !alphabetical; Render(); }
        void BindSpeciesButtons()
        {
            for (var i = 0; i < speciesButtons.Length; i++)
            {
                var captured = i;
                speciesButtons[i]?.onClick.AddListener(() => SelectVisible(captured));
            }
        }
        void UnbindSpeciesButtons()
        {
            for (var i = 0; i < speciesButtons.Length; i++) speciesButtons[i]?.onClick.RemoveAllListeners();
        }
        void SelectVisible(int index)
        {
            var items = Ordered();
            if (index >= items.Count) return;
            if (controller.Select(items[index].Species.SpeciesId)) detailPanel?.Show(controller.Selected);
        }
        List<FishSpeciesViewModel> Ordered()
        {
            var list = new List<FishSpeciesViewModel>();
            if (controller != null) list.AddRange(controller.VisibleSpecies);
            if (alphabetical) list.Sort((a, b) => string.Compare(a.Title, b.Title, StringComparison.CurrentCultureIgnoreCase));
            return list;
        }
        void Render()
        {
            var items = Ordered();
            if (filterLabel != null) filterLabel.text = $"Zona: {(zoneFilter?.ToString() ?? "Todas")}";
            if (sortLabel != null) sortLabel.text = alphabetical ? "Orden: A–Z" : "Orden: Catálogo";
            if (emptyLabel != null) emptyLabel.gameObject.SetActive(items.Count == 0);
            for (var i = 0; i < speciesButtons.Length; i++)
            {
                var show = i < items.Count;
                if (speciesButtons[i] != null) speciesButtons[i].gameObject.SetActive(show);
                if (show && i < speciesLabels.Length && speciesLabels[i] != null)
                {
                    var item = items[i];
                    speciesLabels[i].text = item.Discovery == FishDiscoveryState.Silhouette
                        ? "Silueta por descubrir" : $"{item.Title}\n<i>{item.ScientificName}</i>";
                }
            }
        }
    }
}
