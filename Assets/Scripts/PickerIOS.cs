#if UNITY_IOS
using System.Runtime.InteropServices;

{
    internal class PickeriOS : IPicker
    {
        [DllImport("__Internal")]
        private static extern void UnityMediaPicker_show(string title, string outputFileName, int maxSize);

        public void Show(string title, string outputFileName, int maxSize)
        {
            UnityMediaPicker_show(title, outputFileName, maxSize);
        }
    }
}
#endif