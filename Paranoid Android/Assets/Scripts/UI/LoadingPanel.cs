using UnityEngine;

public class LoadingPanel : BasePanel
{
    public override void Open()
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        base.Open();
    }

    public override void Close()
    {
        base.Close();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
}