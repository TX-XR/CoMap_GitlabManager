using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugCanvas : MonoBehaviour
{
    public static DebugCanvas Instance;
    public TextMeshProUGUI debugText;
    private Queue<string> messages = new Queue<string>();
    private const int maxMessages = 5;  // Limit to 10 lines

    public DebugCanvas()
    {
        Instance = this;
    }

    public void AddMessage(string text)
    {
        if (debugText == null)
            return;

        // Add new message to the queue
        messages.Enqueue(text);

        // Remove oldest message if exceeding limit
        if (messages.Count > maxMessages)
        {
            messages.Dequeue();
        }
        Debug.Log(text);
        // Update displayed text
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        debugText.text = string.Join("\n", messages);
    }
}
