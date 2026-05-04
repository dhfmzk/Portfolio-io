using System;
using System.Collections.Generic;
using System.Linq;
using Input;
using Player;
using UnityEngine;

namespace Portfolio
{
    public class InteractionSystem : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private float interactionRadius = 2.8f;

        private InteractableExhibit _current;

        public bool IsOpen { get; private set; }
        public event Action<PortfolioExhibitData> ExhibitOpened;
        public event Action ExhibitClosed;
        public event Action<InteractableExhibit> FocusedExhibitChanged;

        public void Configure(
            Transform playerTransform,
            PlayerInputReader reader,
            PlayerController controller)
        {
            player = playerTransform;
            inputReader = reader;
            playerController = controller;
        }

        private void Update()
        {
            if (IsOpen)
            {
                if (inputReader != null && inputReader.ClosePressedThisFrame)
                {
                    ClosePanel();
                }

                return;
            }

            var origin = player != null ? player.position : transform.position;
            var exhibits = FindObjectsOfType<InteractableExhibit>()
                .Where(exhibit => Vector3.Distance(exhibit.transform.position, origin) <= interactionRadius)
                .ToArray();

            SetCurrent(SelectNearest(origin, exhibits));
            foreach (var exhibit in FindObjectsOfType<InteractableExhibit>())
            {
                exhibit.SetHighlighted(exhibit == _current);
            }

            if (_current != null && inputReader != null && inputReader.InteractionPressedThisFrame)
            {
                OpenPanel(_current);
            }
        }

        public void OpenPanel(InteractableExhibit exhibit)
        {
            if (exhibit == null)
            {
                return;
            }

            IsOpen = true;
            playerController?.SetMovementPaused(true);
            ExhibitOpened?.Invoke(exhibit.Data);
        }

        public void ClosePanel()
        {
            IsOpen = false;
            playerController?.SetMovementPaused(false);
            ExhibitClosed?.Invoke();
        }

        private void SetCurrent(InteractableExhibit exhibit)
        {
            if (_current == exhibit)
            {
                return;
            }

            _current = exhibit;
            FocusedExhibitChanged?.Invoke(_current);
        }

        public static InteractableExhibit SelectNearest(Vector3 origin, IEnumerable<InteractableExhibit> exhibits)
        {
            return exhibits?
                .Where(exhibit => exhibit != null)
                .OrderBy(exhibit => Vector3.SqrMagnitude(exhibit.transform.position - origin))
                .FirstOrDefault();
        }
    }
}
