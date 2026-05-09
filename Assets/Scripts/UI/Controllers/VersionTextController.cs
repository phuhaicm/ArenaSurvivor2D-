using TMPro;
using UnityEngine;

public class VersionTextController : HaiMonoBehaviour
{
    private TextMeshProUGUI versionText;

    protected override void LoadComponents()
    {
        base.LoadComponents();
        LoadVersionText();
    }

    protected override void Start()
    {
        base.Start();
        RefreshUI();
    }

    private void LoadVersionText()
    {
        if (versionText != null) return;

        VersionTextUI marker = UIRootLookup.FindInRoot<MainMenuRoot, VersionTextUI>(this);
        if (marker == null) return;

        versionText = marker.GetComponent<TextMeshProUGUI>();
    }

    private void RefreshUI()
    {
        if (versionText == null) return;
        versionText.text = GameVersionInfo.VersionLabel;
    }
}