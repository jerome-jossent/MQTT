using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GeoImage
{
    public byte[] ImageData { get; set; }

    public DateTime dateTime { get; set; }
    public long time { get; set; }
    public long counterLatch { get; set; }

    public long counterwindow_begin { get; set; }
    public long counterwindow_end { get; set; }

    public string GetJson()
    {
        string jsonString = System.Text.Json.JsonSerializer.Serialize(this);
        return jsonString;
    }

    public byte[] GetJsonUTF8()
    {
        byte[] jsonUtf8Bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(this);
        return jsonUtf8Bytes;
    }

    public static GeoImage FromJson(string json)
    {
        GeoImage geoImage = System.Text.Json.JsonSerializer.Deserialize<GeoImage>(json)!;
        return geoImage;
    }

    public static GeoImage FromJson(byte[] jsonUtf8Bytes)
    {
        var readOnlySpan = new ReadOnlySpan<byte>(jsonUtf8Bytes);
        GeoImage geoImage = System.Text.Json.JsonSerializer.Deserialize<GeoImage>(readOnlySpan)!;
        return geoImage;
    }
}

