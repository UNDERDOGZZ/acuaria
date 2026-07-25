using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Acuaria.Food
{
    public sealed class FeedingUIController : MonoBehaviour
    {
        [SerializeField] private Button feedButton;
        [SerializeField] private Text buttonLabel;
        [SerializeField] private GameObject instructionRoot;
        [SerializeField] private Text feedbackLabel;
        [SerializeField] private FeedingInputController inputController;
        [SerializeField] private AquariumFoodController foodController;
        [SerializeField] private SpriteRenderer surfaceHighlight;
        [SerializeField] private AudioSource sharedAudioSource;
        [SerializeField] private AudioClip activateClip;
        private bool aquariumFocused;
        private bool tipShown;
        private Coroutine feedbackRoutine;
        public bool IsFeedingMode => inputController != null && inputController.IsFeedingMode;

        public void Configure(Button button, Text label, GameObject instruction, Text feedback,
            FeedingInputController input, AquariumFoodController controller, SpriteRenderer highlight)
        {
            feedButton = button;
            buttonLabel = label;
            instructionRoot = instruction;
            feedbackLabel = feedback;
            inputController = input;
            foodController = controller;
            surfaceHighlight = highlight;
        }

        private void OnEnable()
        {
            feedButton?.onClick.AddListener(ToggleFeedingMode);
            if (foodController != null) foodController.MaximumReached += ShowMaximumFeedback;
            Refresh();
        }

        private void OnDisable()
        {
            feedButton?.onClick.RemoveListener(ToggleFeedingMode);
            if (foodController != null) foodController.MaximumReached -= ShowMaximumFeedback;
            SetFeedingMode(false);
        }

        public void SetAquariumFocused(bool focused)
        {
            aquariumFocused = focused;
            if (!focused) SetFeedingMode(false);
            Refresh();
        }

        public void ToggleFeedingMode()
        {
            if (aquariumFocused) SetFeedingMode(!IsFeedingMode);
        }

        private void SetFeedingMode(bool active)
        {
            inputController?.SetFeedingMode(active);
            if (instructionRoot != null) instructionRoot.SetActive(active);
            if (surfaceHighlight != null) surfaceHighlight.enabled = active;
            if (buttonLabel != null) buttonLabel.text = active ? "Cancelar" : "Alimentar";
            if (active)
            {
                if (sharedAudioSource != null && activateClip != null) sharedAudioSource.PlayOneShot(activateClip);
                if (!tipShown)
                {
                    tipShown = true;
                    ShowFeedback("Alimenta en pequeñas porciones. El exceso puede deteriorar el agua.", 4f);
                }
            }
        }

        private void Refresh()
        {
            if (feedButton != null) feedButton.gameObject.SetActive(aquariumFocused);
            if (!aquariumFocused && instructionRoot != null) instructionRoot.SetActive(false);
            if (feedbackLabel != null && !aquariumFocused) feedbackLabel.gameObject.SetActive(false);
        }

        private void ShowMaximumFeedback() => ShowFeedback("Ya hay suficiente comida", 2f);

        private void ShowFeedback(string message, float duration)
        {
            if (feedbackLabel == null) return;
            if (feedbackRoutine != null) StopCoroutine(feedbackRoutine);
            feedbackRoutine = StartCoroutine(FeedbackRoutine(message, duration));
        }

        private IEnumerator FeedbackRoutine(string message, float duration)
        {
            feedbackLabel.text = message;
            feedbackLabel.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(duration);
            feedbackLabel.gameObject.SetActive(false);
            feedbackRoutine = null;
        }
    }
}
