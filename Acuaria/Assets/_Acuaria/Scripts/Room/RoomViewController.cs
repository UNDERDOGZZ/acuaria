using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Acuaria.Room
{
    public sealed class RoomViewController : MonoBehaviour
    {
        [SerializeField] private Camera roomCamera;
        [SerializeField] private AquariumInteractable[] interactables;
        [SerializeField] private GameObject focusedUi;
        [SerializeField] private Button backButton;
        [SerializeField] private CanvasGroup transitionVeil;
        [SerializeField, Range(0.45f, 0.8f)] private float duration = 0.6f;

        private readonly RoomViewStateMachine stateMachine = new();
        private Vector3 overviewPosition;
        private float overviewSize;
        private Coroutine transition;

        public RoomViewState State => stateMachine.State;
        public AquariumFocusTarget FocusedTarget { get; private set; }

        private void OnEnable()
        {
            if (roomCamera == null || backButton == null)
            {
                Debug.LogError("Room view references are incomplete.", this);
                enabled = false;
                return;
            }

            overviewPosition = roomCamera.transform.position;
            overviewSize = roomCamera.orthographicSize;
            focusedUi.SetActive(false);
            backButton.onClick.AddListener(ReturnToRoom);
            foreach (var interactable in interactables)
            {
                if (interactable != null)
                {
                    interactable.Selected += FocusAquarium;
                }
            }
        }

        private void OnDisable()
        {
            backButton?.onClick.RemoveListener(ReturnToRoom);
            foreach (var interactable in interactables)
            {
                if (interactable != null)
                {
                    interactable.Selected -= FocusAquarium;
                }
            }

            if (transition != null)
            {
                StopCoroutine(transition);
                transition = null;
            }
        }

        public void FocusAquarium(AquariumFocusTarget target)
        {
            if (target == null || !stateMachine.TryBeginFocus())
            {
                return;
            }

            FocusedTarget = target;
            SetInteraction(false);
            transition = StartCoroutine(Transition(target.Position, target.OrthographicSize, true));
        }

        public void ReturnToRoom()
        {
            if (!stateMachine.TryBeginReturn())
            {
                return;
            }

            backButton.interactable = false;
            transition = StartCoroutine(Transition(overviewPosition, overviewSize, false));
        }

        private IEnumerator Transition(Vector3 targetPosition, float targetSize, bool focusing)
        {
            var startPosition = roomCamera.transform.position;
            var endPosition = new Vector3(targetPosition.x, targetPosition.y, startPosition.z);
            var startSize = roomCamera.orthographicSize;
            var elapsed = 0f;
            transitionVeil.gameObject.SetActive(true);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                roomCamera.transform.position = Vector3.LerpUnclamped(startPosition, endPosition, progress);
                roomCamera.orthographicSize = Mathf.LerpUnclamped(startSize, targetSize, progress);
                transitionVeil.alpha = 0.12f * Mathf.Sin(progress * Mathf.PI);
                yield return null;
            }

            roomCamera.transform.position = endPosition;
            roomCamera.orthographicSize = targetSize;
            transitionVeil.alpha = 0f;
            transitionVeil.gameObject.SetActive(false);
            transition = null;

            if (focusing)
            {
                stateMachine.TryCompleteFocus();
                focusedUi.SetActive(true);
                backButton.interactable = true;
            }
            else
            {
                stateMachine.TryCompleteReturn();
                focusedUi.SetActive(false);
                FocusedTarget = null;
                SetInteraction(true);
            }
        }

        private void SetInteraction(bool enabledState)
        {
            foreach (var interactable in interactables)
            {
                interactable?.SetAvailable(enabledState);
            }
        }
    }
}
