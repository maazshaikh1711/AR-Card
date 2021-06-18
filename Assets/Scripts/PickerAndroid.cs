using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickerAndroid : IPicker
{
    private static readonly string PickerClass = "com.oktilyon.unitymediapicker.Picker";

    public void Capture(string title, string outputFileName, int maxSize)
    {
        using (var picker = new AndroidJavaClass(PickerClass))
        {
            picker.CallStatic("capture", title, outputFileName, maxSize);
        }
    }

    public void Show(string title, string outputFileName, int maxSize)
    {
        using (var picker = new AndroidJavaClass(PickerClass))
        {
            picker.CallStatic("show", title, outputFileName, maxSize);
        }
    }
}
