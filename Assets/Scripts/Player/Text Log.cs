using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TextLog : MonoBehaviour
{
    public static TextLog Instance;

    public TextMeshProUGUI textBox;

    private List<string> messages = new List<string>();
    public int maxLines = 3;

    private void Awake()
    {
        Instance = this;
    }

    public void AddMessage(string message)
    {
        messages.Add(message);

        if (messages.Count > maxLines)
            messages.RemoveAt(0);

        UpdateUI();
    }

    private void UpdateUI()
    {
        List<string> display = new List<string>(messages);
        display.Reverse();

        textBox.text = string.Join("\n", display);
        
    }
}