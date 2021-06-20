using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Networking;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using TMPro;


public class PostNewTrackableRequest
{
    public string name;
    public float width;
    public string image;
    public string application_metadata;
}

public class UnityMediaPicker : MonoBehaviour
{
    public Image image;

    private const int MaxImageSizeByte = 1024;
    private int ImageRectHeight = 833, ImageRectWidth = 1480;

    public delegate void ImageDelegate(string path);

    public delegate void ErrorDelegate(string message);

    public event ImageDelegate Completed;

    public event ErrorDelegate Failed;

    private UnityMediaPicker imagePicker;

    public static string _key="";


    //public Texture2D texture;
    public TMP_Text path;

    private string access_key = "5a966f7c1085f7f9824db2d4560a0a20a3ad3505";
    private string secret_key = "f24920ea15bd4eb8fb113d7a4cbc85493c0ce065";
    private string vuforiaUrl = @"https://vws.vuforia.com";
    private string targetName = ""; // must change when upload another Image Target, avoid same as exist Image on cloud
    private byte[] requestBytesArray;

    public void Path()
    {
        Debug.Log(Application.persistentDataPath);
        path.text = Application.persistentDataPath;
    }

    void Awake()
    {
        imagePicker = this;

        ImageRectWidth = Convert.ToInt32(image.rectTransform.rect.width);
        ImageRectHeight = Convert.ToInt32(image.rectTransform.rect.height);

        imagePicker.Completed += (string path) =>
        {
            StartCoroutine(LoadImage(path, image));
        };
    }

    public static void AssignKey(string k)
    {
        _key = k;
    }

    public void OnPressShowPicker()
    {
        Show("Select your card with ...", "UnityMediaPicker", ImageRectWidth);
    }

    public void OnPressShowCapture()
    {
        Capture("Kamera Seçiniz ...", "UnityMediaPicker", ImageRectWidth);
    }

    private IEnumerator LoadImage(string path, Image output)
    {

        //Loading of image starts here
        var url = "file://" + path;
        var www = new WWW(url);
        yield return www;

        var texture = www.texture;
        if (texture == null)
        {
            Debug.LogError("Failed to load texture url:" + url);
        }



        //Uploading to VuforiaCloud logic starts here
        string requestPath = "/targets";
        string serviceURI = vuforiaUrl + requestPath;
        string httpAction = "POST";
        string contentType = "application/json";
        string date = string.Format("{0:r}", DateTime.Now.ToUniversalTime());


        var imageBytes = File.ReadAllBytes(path);
        Texture2D texturezz = new Texture2D(2, 2); //must start with a placeholder Texture object
        texturezz.LoadImage(imageBytes);
        texturezz.name = "abcd";


        //if your texture2d has RGb24 type, don't need to redraw new texture2d
        Texture2D tex = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
        tex.SetPixels(texture.GetPixels());
        tex.Apply();
        byte[] image = tex.EncodeToPNG();



        targetName = _key;
        string metadataStr = _key;
        //string metadataStr = "0maaz0shaikh0gmailcom";//May use for key,name...in game
        byte[] metadata = System.Text.ASCIIEncoding.ASCII.GetBytes(metadataStr);
        PostNewTrackableRequest model = new PostNewTrackableRequest();
        model.name = targetName;
        model.width = 5; // don't need same as width of texture
        model.image = System.Convert.ToBase64String(image);

        model.application_metadata = System.Convert.ToBase64String(metadata);
        //string requestBody = JsonWriter.Serialize(model);
        string requestBody = JsonUtility.ToJson(model);

        WWWForm form = new WWWForm();

        var headers = form.headers;
        byte[] rawData = form.data;
        headers["host"] = url;
        headers["date"] = date;
        headers["Content-Type"] = contentType;

        HttpWebRequest httpWReq = (HttpWebRequest)HttpWebRequest.Create(serviceURI);

        MD5 md5 = MD5.Create();
        var contentMD5bytes = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(requestBody));
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < contentMD5bytes.Length; i++)
        {
            sb.Append(contentMD5bytes[i].ToString("x2"));
        }

        string contentMD5 = sb.ToString();

        string stringToSign = string.Format("{0}\n{1}\n{2}\n{3}\n{4}", httpAction, contentMD5, contentType, date, requestPath);

        HMACSHA1 sha1 = new HMACSHA1(System.Text.Encoding.ASCII.GetBytes(secret_key));
        byte[] sha1Bytes = System.Text.Encoding.ASCII.GetBytes(stringToSign);
        MemoryStream stream = new MemoryStream(sha1Bytes);
        byte[] sha1Hash = sha1.ComputeHash(stream);
        string signature = System.Convert.ToBase64String(sha1Hash);

        headers["Authorization"] = string.Format("VWS {0}:{1}", access_key, signature);

        Debug.Log("<color=green>Signature: " + signature + "</color>");

        WWW request = new WWW(serviceURI, System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(model)), headers);
        yield return request;

        Debug.Log("..........");
        if (request.error != null)
        {
            Debug.Log("request error: " + request.error);
        }
        else
        {
            Debug.Log("request success");
            Debug.Log("returned data" + request.text);
        }


        //output.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
        //output.rectTransform.sizeDelta = new Vector2(texture.width, texture.height);

        //float multiplayerX = texture.width / ImageRectWidth;
        //float multiplayerY = texture.height / ImageRectHeight;

        //if (multiplayerX >= multiplayerY)
        //{
        //    if (multiplayerX > 1)
        //    {
        //        output.rectTransform.sizeDelta = new Vector2(texture.width / multiplayerX, texture.height / multiplayerX);
        //    }
        //    else
        //    {
        //        float strech = ImageRectWidth / texture.width;
        //        output.rectTransform.sizeDelta = new Vector2(texture.width * strech, texture.height * strech);
        //    }
        //}
        //else  
        //{
        //    if (multiplayerY > 1)
        //    {
        //        output.rectTransform.sizeDelta = new Vector2(texture.width / multiplayerY, texture.height / multiplayerY);
        //    }
        //    else
        //    {
        //        float strech = ImageRectHeight / texture.height;
        //        output.rectTransform.sizeDelta = new Vector2(texture.width * strech, texture.height * strech);
        //    }
        //}
    }

    private IPicker picker = new PickerAndroid();
    //private IPicker picker = new Picker_editor();
    //#if UNITY_IOS && !UNITY_EDITOR
    //new PickeriOS();
    //#elif UNITY_ANDROID && !UNITY_EDITOR

    //#elif UNITY_EDITOR_OSX || UNITY_EDITOR_WIN
    //new Picker_editor();
    //#else
    //new PickerUnsupported();
    //#endif
    public void Show(string title, string outputFileName, int maxSize)
    {
        picker.Show(title, outputFileName, maxSize);
    }

    public void Capture(string title, string outputFileName, int maxSize)
    {
        picker.Capture(title, outputFileName, maxSize);
    }

    private void OnComplete(string path)
    {
        var handler = Completed;
        if (handler != null)
        {
            handler(path);
        }
    }

    private void OnFailure(string message)
    {
        var handler = Failed;
        if (handler != null)
        {
            handler(message);
        }
    }
}
