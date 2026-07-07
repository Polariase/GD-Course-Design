using UnityEngine;

public abstract class BaseContainer : MonoBehaviour, IInteractable
{
    protected bool isOpen = false;

    public virtual string ActionName => "´ò¿ª";

    public virtual void Interact()
    {
        if (isOpen)
        {
            var containerCtrl = InventoryManager.Instance.container as ContainerController;
            containerCtrl?.CloseCurrentContainer();
        }
        else
        {
            OpenContainer();
        }
    }

    protected abstract void OpenContainer();

    public virtual void OnContainerClosed()
    {
        isOpen = false;
    }
}