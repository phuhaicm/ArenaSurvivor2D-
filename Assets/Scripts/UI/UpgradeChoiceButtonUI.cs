using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UpgradeChoiceButtonUI : HaiMonoBehaviour
{
    private Button button;
    private TextMeshProUGUI labelText;
    private Action onClicked;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadButton();
        LoadLabelText();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        ClearListeners();
    }

    private void LoadButton()
    {
        if (button != null) return;
        button = GetComponent<Button>();
    }

    private void LoadLabelText()
    {
        if (labelText != null) return;
        labelText = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    public void Bind(UpgradeOptionData option, Action callback)
    {
        onClicked = callback;

        if (labelText != null)
        {
            labelText.text = $"{option.DisplayName}\n{option.Description}";
        }

        if (button != null)
        {
            button.interactable = true;
            button.onClick.RemoveListener(HandleClicked);
            button.onClick.AddListener(HandleClicked);
        }
    }

    public void ClearListeners()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClicked);
        }

        onClicked = null;
    }

    private void HandleClicked()
    {
        onClicked?.Invoke();
    }
}