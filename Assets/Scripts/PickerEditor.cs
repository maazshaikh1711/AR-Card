#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN
using UnityEditor;
using UnityEngine;
using System.IO;
public class Picker_editor : IPicker
{
    public void Capture(string title, string outputFileName, int maxSize)
    {
        Show(title, outputFileName, maxSize);
    }

    public void Show(string title, string outputFileName, int maxSize)
    {
        var path = EditorUtility.OpenFilePanel(title, "", "png");
        if (path.Length != 0)
        {
            string destination = Application.persistentDataPath + "/" + outputFileName;
            if (File.Exists(destination))
                File.Delete(destination);
            File.Copy(path, destination);
            Debug.Log("PickerOSX:" + destination);
            var receiver = GameObject.Find("UnityMediaPicker");
            if (receiver != null)
            {
                receiver.SendMessage("OnComplete", Application.persistentDataPath + "/" + outputFileName);
            }
        }
    }
}
#endif