using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum CursorState
{
    Combat,
    NonCombat
}

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [Header("ÒýÓÃ")]
    public CrosshairController crosshair;
    public PlayerStateData stateData;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        stateData.OnSlotChanged += OnSlotChanged;
    }

    public void OnSlotChanged(int index)
    {
        if (index != 0)
            SetCursorState(CursorState.Combat);
        else
            SetCursorState(CursorState.NonCombat);
    }

    public void SetCursorState(CursorState newState)
    {
        switch (newState)
        {
            case CursorState.Combat:
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Confined;
                crosshair.gameObject.SetActive(true);
                break;

            case CursorState.NonCombat:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                crosshair.gameObject.SetActive(false);
                break;
        }
    }
}
