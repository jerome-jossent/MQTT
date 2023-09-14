using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OCR_image_results
{
    public string id;
    public string metadata;
    public List<OCR_result> results;

    public OCR_image_results(string metadata, List<OCR_result> results)
    {
        this.metadata = metadata;
        this.results = results;
    }
}