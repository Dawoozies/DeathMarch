using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionPreset : MonoBehaviour
{
    public RectTransform parentRect;
    public TMP_InputField AddressInput;
    public TMP_InputField PortInput;
    public GameObject presetPrefab;
    public List<AddressPortPreset> presets = new();
    [System.Serializable]
    public class AddressPortPreset
    {
        public string address;
        public string port;
    }
    void Start()
    {
        foreach (var preset in presets)
        {
            GameObject presetClone = Instantiate(presetPrefab, parentRect.transform);
            TMP_Text presetText = presetClone.GetComponentInChildren<TMP_Text>();
            presetText.text = $"Use Preset {preset.address}:{preset.port}";
            Button presetButton = presetClone.GetComponentInChildren<Button>();
            presetButton.onClick.AddListener(() => UsePreset(preset));
        }
    }
    public void UsePreset(AddressPortPreset preset)
    {
        AddressInput.text = preset.address;
        PortInput.text = preset.port;
    }
}
