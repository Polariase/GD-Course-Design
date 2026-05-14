using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    public RectTransform crosshairRoot;
    public Image dotImage;
    public Image[] linesImage;
    public RectTransform lines;
    public RectTransform[] linesTransform;
    public Camera mainCamera;
    public PlayerController pc;
    public WeaponController weapon;
    public Gradient spreadGradient;

    [SerializeField] private float _minSpread = 2f;
    [SerializeField] private float _maxSpread = 100f;
    [SerializeField] private float _smoothTime = 0.1f;
    private float _currentSpreadVelocity;
    private float _visualSpread;
    private PlayerInput _input;
    

    private void Start()
    {
        mainCamera = Camera.main;
        _input = PlayerController.Instance.GetComponent<PlayerInput>();
    }

    private void LateUpdate()
    {
        UpdateCrosshair();
    }

    public void UpdateCrosshair()
    {
        // 更新准星根位置到鼠标位置
        Vector2 mousePos = _input.actions["Look"].ReadValue<Vector2>();
        crosshairRoot.position = mousePos;

        // 获取落点和扩散角
        Vector3 impactPoint = pc.mouseWorldPosition;
        float spreadAngle = weapon.GetSpreadAngle();

        // 计算扩散在世界空间中的实际偏移
        float dist = Vector3.Distance(weapon.firePoint.position, impactPoint);
        float spreadWorldRadius = dist * Mathf.Tan(spreadAngle * Mathf.Deg2Rad);

        // 计算偏移后的世界坐标
        Vector3 offsetWorldPoint = impactPoint + mainCamera.transform.right * spreadWorldRadius;

        // 转回屏幕空间
        Vector3 offsetScreenPoint = mainCamera.WorldToScreenPoint(offsetWorldPoint);

        float targetSpread = Mathf.Clamp(Vector2.Distance(mousePos, offsetScreenPoint), _minSpread, _maxSpread);

        // 应用平滑
        _visualSpread = Mathf.SmoothDamp(_visualSpread, targetSpread, ref _currentSpreadVelocity, _smoothTime);

        ApplySpreadToLines(_visualSpread);
        UpdateCrosshairColor(Mathf.InverseLerp(_minSpread, _maxSpread, _visualSpread));
    }

    private void ApplySpreadToLines(float spread)
    {
        linesTransform[0].anchoredPosition = new Vector2(0, spread);
        linesTransform[1].anchoredPosition = new Vector2(spread, 0);
        linesTransform[2].anchoredPosition = new Vector2(-spread, 0);
        linesTransform[3].anchoredPosition = new Vector2(0, -spread);
    }

    private void UpdateCrosshairColor(float percent)
    {
        Color targetColor = spreadGradient.Evaluate(percent);

        foreach (var img in linesImage)
        {
            img.color = targetColor;
        }

        dotImage.color = new(targetColor.r, targetColor.g, targetColor.b, weapon.aimWeight);
    }
}
