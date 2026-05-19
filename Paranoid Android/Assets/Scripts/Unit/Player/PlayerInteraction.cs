using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    private IInteractable _currentInteraction;
    private List<IInteractable> _interactableList = new List<IInteractable>();

    private void Start()
    {
        PlayerController.Instance.GetComponent<PlayerInput>().actions["Interact"].performed +=  OnInteract;
    }

    private void OnDestroy()
    {
        PlayerController.Instance.GetComponent<PlayerInput>().actions["Interact"].performed -= OnInteract;
    }


    private void OnInteract(InputAction.CallbackContext ctx)
    {
        _interactableList.RemoveAll(x => x == null || (x is MonoBehaviour mb && !mb.gameObject.activeInHierarchy));

        if (_interactableList.Count > 0)
        {
            var target = _interactableList[0];
            target.Interact();
            _interactableList.Remove(target);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IInteractable>(out var interactable))
        {
            if (!_interactableList.Contains(interactable))
                _interactableList.Add(interactable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IInteractable>(out var i))
            _interactableList.Remove(i);
    }
}