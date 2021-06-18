using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPicker
{
    void Show(string title, string outputFileName, int maxSize);

    void Capture(string title, string outputFileName, int maxSize);
}
