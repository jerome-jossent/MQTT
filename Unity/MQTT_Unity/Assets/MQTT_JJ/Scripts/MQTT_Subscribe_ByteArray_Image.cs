using UnityEngine;
using UnityEngine.UI;

public class MQTT_Subscribe_ByteArray_Image : MQTT_JJ
{
    public Texture2D _texture;
    [SerializeField] RawImage rawimage;
    RectTransform rect;

    public int val;
    int val_tot = 0;
    int nval = 0;

    public float fps;
    float t;
    public float nextT;
    float p = 1;

    protected new void Start()
    {
        base.Start();
        rect = rawimage.gameObject.GetComponent<RectTransform>();
        t = Time.time;
        nextT = t + p;
    }

    protected new void Update()
    {
        base.Update();
        if (nextT > Time.time) return;

        nextT += p;

        if (nval == 0) return;

        val = val_tot / nval;
        fps = (float)nval / (Time.time - t);

        val_tot = 0;
        nval = 0;
        t = Time.time;
    }

    //spécifique à "image"
    internal override void _DecodeMessage(byte[] message)
    {
        Texture2D tex = new Texture2D(2, 2);
        try
        {
            tex.LoadImage(message);
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
        }

        if (rawimage != null)
        {
            rawimage.texture = tex;

            //resize
            if (tex.height < tex.width)
                rect.sizeDelta = new Vector2(1, (float)tex.height / tex.width);
            else
                rect.sizeDelta = new Vector2((float)tex.width / tex.height, 1);
        }

        //vital !
        Resources.UnloadUnusedAssets();

        val_tot += message.Length;
        nval++;
    }
}
