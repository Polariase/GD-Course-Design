using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    private GameObject _currentActivePrompt;
    private List<IInteractable> _interactableStack = new List<IInteractable>();
    public float promptHeightOffset = 1.5f;

    private void Start()
    {
        PlayerController.Instance.GetComponent<PlayerInput>().actions["Interact"].performed +=  OnInteract;
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance == null) return;
        PlayerController.Instance.GetComponent<PlayerInput>().actions["Interact"].performed -= OnInteract;
    }


    private void OnInteract(InputAction.CallbackContext ctx)
    {
        CleanInvalidInteractables();

        if (_interactableStack.Count > 0)
        {
            int topIndex = _interactableStack.Count - 1;
            var target = _interactableStack[topIndex];
            ClearCurrentPrompt();
            target.Interact();
            _interactableStack.RemoveAt(topIndex);
            RefreshCurrentPrompt();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IInteractable>(out var interactable))
        {
            if (!_interactableStack.Contains(interactable))
            {
                ClearCurrentPrompt();
                _interactableStack.Add(interactable);
                RefreshCurrentPrompt();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IInteractable>(out var i))
        {
            if (_interactableStack.Contains(i))
            {
                int topIndex = _interactableStack.Count - 1;
                bool isTargetCurrentTop = (_interactableStack[topIndex] == i);

                if (isTargetCurrentTop)
                {
                    ClearCurrentPrompt();
                }

                _interactableStack.Remove(i);

                if (isTargetCurrentTop)
                {
                    RefreshCurrentPrompt();
                }
            }
        }
    }

    private void ClearCurrentPrompt()
    {
        if (_currentActivePrompt != null)
        {
            if (PopupManager.Instance != null)
            {
                PopupManager.Instance.HideInteractPrompt(_currentActivePrompt);
            }
            _currentActivePrompt = null;
        }
    }

    private void RefreshCurrentPrompt()
    {
        CleanInvalidInteractables();

        if (_interactableStack.Count > 0 && _currentActivePrompt == null)
        {
            int topIndex = _interactableStack.Count - 1;
            var topInteractable = _interactableStack[topIndex];

            if (topInteractable is MonoBehaviour mb)
            {
                if (PopupManager.Instance != null)
                {
                    _currentActivePrompt = PopupManager.Instance.ShowInteractPrompt(mb.transform.position, promptHeightOffset);
                    if (_currentActivePrompt != null && _currentActivePrompt.TryGetComponent<InteractPrompt>(out var promptScript))
                    {
                        string actionText = topInteractable.ActionName;
                        promptScript.Init(mb.transform, promptHeightOffset, actionText);
                    }
                }
            }
        }
    }

    private void CleanInvalidInteractables()
    {
        _interactableStack.RemoveAll(x => x == null || (x is MonoBehaviour mb && !mb.gameObject.activeInHierarchy));
    }
}