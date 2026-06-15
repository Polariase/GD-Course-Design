using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public InventoryPanel inventoryPanel;
    public LoadingPanel loadingPanel;

    public CrosshairController crosshair;
    public HUDController hud;

    private readonly Stack<BasePanel> _panelStack = new();
    private PlayerStateData _playerState;
    private PlayerInput _input;
    private PlayerController _pc;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetUIMode(GameState state)
    {
        switch (state)
        {
            case GameState.Entry:
                hud.gameObject.SetActive(false);
                inventoryPanel.gameObject.SetActive(false);
                ApplyCursorState(true, CursorLockMode.None, false);
                break;

            case GameState.Shelter:
            case GameState.Exploration:
                hud.gameObject.SetActive(true);
                if (_pc != null)
                {
                    UpdateCursorState(_pc.isArmed);
                }
                else
                {
                    ApplyCursorState(true, CursorLockMode.None, false);
                }
                inventoryPanel.gameObject.SetActive(true);
                break;
        }
        Back();
    }

    public void ShowLoading(bool state)
    {
        if (loadingPanel != null)
        {
            if (state)
            {
                loadingPanel.Open();
            }
            else
            {
                loadingPanel.Close();
            }
        }
    }

    public void OpenPanel(BasePanel panel)
    {
        if (panel == null || _panelStack.Count > 0) return;

        panel.Open();
        _panelStack.Push(panel);
        UpdateUIState();
    }

    public void Back()
    {
        if (_panelStack.Count == 0) return;

        BasePanel top = _panelStack.Pop();
        top.Close();
        UpdateUIState();
    }

    public void UpdateUIState()
    {
        if (_panelStack.Count > 0)
        {
            _input.SwitchCurrentActionMap("UI");
        }
        else
        {
            _input.SwitchCurrentActionMap("Player");
        }

        UpdateCursorState(_pc.isArmed);
    }

    private void UpdateCursorState(bool armed)
    {
        if (_panelStack.Count > 0)
        {
            ApplyCursorState(true, CursorLockMode.None, false);
            return;
        }

        if (_playerState != null && armed)
        {
            ApplyCursorState(false, CursorLockMode.Confined, true);
        }

        else
        {
            ApplyCursorState(true, CursorLockMode.None, false);
        }
    }

    private void ApplyCursorState(bool visible, CursorLockMode lockMode, bool showCrosshair)
    {
        Cursor.visible = visible;
        Cursor.lockState = lockMode;
        if (crosshair != null)
        {
            crosshair.gameObject.SetActive(showCrosshair);
        }
    }

    private void BindInputs()
    {
        if (_input == null) return;

        _input.actions["Player/Inventory"].performed += OnInventoryPerformed;
        _input.actions["UI/Inventory"].performed += OnInventoryPerformed;
        _input.actions["Cancel"].performed += OnCancelPerformed;
    }

    private void UnbindInputs()
    {
        if (_input == null) return;

        _input.actions["Inventory"].performed -= OnInventoryPerformed;
        _input.actions["Cancel"].performed -= OnCancelPerformed;
    }

    private void OnInventoryPerformed(InputAction.CallbackContext ctx)
    {
        if (inventoryPanel != null && inventoryPanel.isOpen)
            Back();
        else
            OpenPanel(inventoryPanel);
    }

    private void OnCancelPerformed(InputAction.CallbackContext ctx)
    {
        if (_panelStack.Count > 0)
            Back();
    }

    public void Initialize(PlayerController player)
    {
        Cleanup();
        _pc = player;
        if (_pc == null) return;
        _input = _pc.GetComponent<PlayerInput>();
        _playerState = _pc.stateData;
        _pc.OnArmed += UpdateCursorState;
        BindInputs();

        hud.Initialize(_pc.stateData);
        crosshair.Initialize(_pc);
        UpdateCursorState(_pc.isArmed);
    }

    private void Cleanup()
    {
        if (_pc != null)
        {
            _pc.OnArmed -= UpdateCursorState;
        }
        UnbindInputs();
    }

    private void OnDestroy()
    {
        Cleanup();
    }
}