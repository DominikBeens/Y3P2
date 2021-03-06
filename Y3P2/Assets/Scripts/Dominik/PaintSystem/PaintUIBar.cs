﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PaintUIBar : MonoBehaviour
{

    private float barFillAmount;

    private PaintController.PaintType barType;
    public PaintController.PaintType BarType { get { return barType; } }

    [SerializeField] private Image barColorFill;
    [SerializeField] private Image barBackgroundFill;

    public void Initialise(PaintController.PaintValue paintValue, bool local)
    {
        barType = paintValue.paintType;
        barFillAmount = 0;
        barColorFill.fillAmount = 0;
        barColorFill.color = paintValue.paintColor;
        barBackgroundFill.color = new Color(paintValue.paintColor.r, paintValue.paintColor.g, paintValue.paintColor.b, 0.1f);

        if (local)
        {
            ToggleUI(false);
        }
    }

    public void IncrementBar(float amount)
    {
        float amountFixed = amount / 100;
        float percentage = barFillAmount + amountFixed;
        barFillAmount = percentage;
        barFillAmount = Mathf.Clamp01(barFillAmount);
        StartCoroutine(LerpBar(percentage));
    }

    public void ResetBar()
    {
        barFillAmount = 0;
        StartCoroutine(LerpBar(0));
    }

    private IEnumerator LerpBar(float percentage)
    {
        float startPercentage = barColorFill.fillAmount;
        float progress = 0f;

        while (progress < 0.2f)
        {
            progress += Time.deltaTime;
            barColorFill.fillAmount = Mathf.Lerp(startPercentage, percentage, progress / 0.2f);
            yield return null;
        }

        barColorFill.fillAmount = percentage;
    }

    public void ToggleUI(bool toggle)
    {
        transform.GetChild(0).gameObject.SetActive(toggle);
    }
}
