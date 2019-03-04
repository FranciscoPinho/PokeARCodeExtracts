using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraTakePicture : MonoBehaviour {
    WebCamTexture webCamTexture;
    bool photoTaken1 = false;
    bool photoTaken2 = false;
    Texture2D photo;
    GameObject p1frame;
    GameObject p2frame;

    void Awake()
    {
        p1frame = GameObject.Find("P1Picture");
        p2frame = GameObject.Find("P2Picture");
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length > 1)
        {
            webCamTexture = new WebCamTexture(devices[1].name);
            webCamTexture.Play();
        }
        else{
            webCamTexture = new WebCamTexture(devices[0].name);
            webCamTexture.Play();
        }
    }

    void Update()
    {
        if (!photoTaken1)
        {
            p1frame.GetComponent<RawImage>().texture = webCamTexture;
        }
        if(photoTaken1 && !photoTaken2)
        {
            p2frame.GetComponent<RawImage>().texture = webCamTexture;
        }
    }

    public IEnumerator TakePicturePlayer1()
    {
        yield return new WaitForEndOfFrame();
        photoTaken1 = true;
        photo = new Texture2D(webCamTexture.width, webCamTexture.height);
        photo.SetPixels(webCamTexture.GetPixels());
        photo.Apply();
        p1frame.GetComponent<RawImage>().texture = photo;
        GameObject capt2button = p2frame.transform.Find("Capture2").gameObject;
        GameObject capt1button = GameObject.Find("Capture1");
        GameObject pictureHolder = GameObject.Find("PictureHolder");
        pictureHolder.GetComponent<PictureHolderVariables>().player1 = photo;
        capt2button.SetActive(true);
        capt1button.SetActive(false);
       
    }

    public IEnumerator TakePicturePlayer2()
    {
        yield return new WaitForEndOfFrame();
        photoTaken2 = true;
        photo = new Texture2D(webCamTexture.width, webCamTexture.height);
        photo.SetPixels(webCamTexture.GetPixels());
        photo.Apply();
        p2frame.GetComponent<RawImage>().texture = photo;
        GameObject capt2button = GameObject.Find("Capture2");
        GameObject pictureHolder = GameObject.Find("PictureHolder");
        pictureHolder.GetComponent<PictureHolderVariables>().player2 = photo;
        capt2button.SetActive(false);
        if(webCamTexture.isPlaying)
            webCamTexture.Stop();
        GameObject app = GameObject.Find("ApplicationManager");
        if(app!=null)
            app.GetComponent<ApplicationManager>().ChangeScene(2);
        
    }
}
