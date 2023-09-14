using OpenCVForUnity.CoreModule;
using UnityEngine;

public class OCR_result
{
    public int center_X { get; set; }
    public int center_Y { get; set; }
    public int size_x { get; set; }
    public int size_Y { get; set; }
    public double angle { get; set; }
    public string text { get; set; }
    public Point[] points;

    public OCR_result(RotatedRect rb, Point[] points, string recognitionResult)
    {
        center_X = (int)rb.center.x;
        center_Y = (int)rb.center.y;
        size_x = (int)rb.size.width;
        size_Y = (int)rb.size.height;
        angle = rb.angle;
        text = recognitionResult;
        this.points = points;
    }

    public OCR_result(string recognitionResult)
    {        
        text = recognitionResult;
    }
}
