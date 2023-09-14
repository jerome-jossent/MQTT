#if !UNITY_WSA_10_0

using CompactExifLib;
using Newtonsoft.Json;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.DnnModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UtilsModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


namespace OpenCVForUnityExample
{
    /// <summary>
    /// Text OCR Example
    /// This example demonstrates text detection and recognition model using the TextDetectionMode and TextRecognitionModel class.
    /// https://github.com/opencv/opencv_zoo/tree/master/models/text_detection_db
    /// https://github.com/opencv/opencv_zoo/tree/master/models/text_recognition_crnn
    /// https://docs.opencv.org/4.x/d4/d43/tutorial_dnn_text_spotting.html
    /// </summary>
    public class OCR_from_binary : MonoBehaviour
    {
        #region PARAMETRES OCR OPENCVFORUNITY
        // Preprocess input image by resizing to a specific width. It should be multiple by 32.
        const float detection_inputSize_w = 736f;

        // Preprocess input image by resizing to a specific height. It should be multiple by 32.
        const float detection_inputSize_h = 736f;

        const double detection_inputScale = 1.0 / 255.0;

        Scalar detection_inputMean = new Scalar(122.67891434, 116.66876762, 104.00698793);

        // Threshold of the binary map.
        const float detection_binary_threshold = 0.3f;

        // Threshold of polygons.
        const float detection_polygon_threshold = 0.5f;

        // Max candidates of polygons.
        const int detection_max_candidates = 200;

        // The unclip ratio of the detected text region, which determines the output size.
        const double detection_unclip_ratio = 2.0;


        // Preprocess input image by resizing to a specific width.
        const float recogniton_inputSize_w = 100f;

        // Preprocess input image by resizing to a specific height.
        const float recogniton_inputSize_h = 32f;

        const double recogniton_inputScale = 1.0 / 127.5;

        Scalar recogniton_inputMean = new Scalar(127.5);


        /// <summary>
        /// Path to a binary .onnx file contains trained detection network.
        /// </summary>
        string DETECTIONMODEL_FILENAME = "OpenCVForUnity/dnn/text_detection_DB_IC15_resnet18_2021sep.onnx";

        /// <summary>
        /// The detection model filepath.
        /// </summary>
        string detectionmodel_filepath;

        /// <summary>
        /// Path to a binary .onnx file contains trained recognition network.
        /// </summary>
        string RECOGNTIONMODEL_FILENAME = "OpenCVForUnity/dnn/text_recognition_CRNN_EN_2021sep.onnx";

        /// <summary>
        /// The recognition model filepath.
        /// </summary>
        string recognitionmodel_filepath;

        /// <summary>
        /// Path to a .txt file contains charset.
        /// </summary>
        string CHARSETTXT_FILENAME = "OpenCVForUnity/dnn/charset_36_EN.txt";

        /// <summary>
        /// The charset txt filepath.
        /// </summary>
        string charsettxt_filepath;
        #endregion


        TextDetectionModel_DB detectonModel = null;
        TextRecognitionModel recognitonModel = null;
        Mat croppedMat = null;
        Mat croppedGrayMat = null;

        bool isOCR_Initialized = false;
        public bool isBusy = false;
        bool ocr_to_process = false;

        byte[] imagedata;
        public bool alreadycropped;
        public bool forceZone;
        public Vector2 centre;
        public Vector2 taille;

        TickMeter tickMeter;
        RotatedRect[] detection_arr;
        List<OCR_result> results;
        OCR_result result;
        public UnityEvent<OCR_image_results> onNewResults;
        public UnityEvent<string> onNewResults_json;

        public bool display_image;


        void Start()
        {
            detectionmodel_filepath = Utils.getFilePath(DETECTIONMODEL_FILENAME);
            recognitionmodel_filepath = Utils.getFilePath(RECOGNTIONMODEL_FILENAME);
            charsettxt_filepath = Utils.getFilePath(CHARSETTXT_FILENAME);

            if (!OCR_Init() && (detectonModel == null || recognitonModel == null))
            {
                //                    Imgproc.putText(img, "model file is not loaded.", new Point(5, img.rows() - 30), Imgproc.FONT_HERSHEY_SIMPLEX, 0.7, new Scalar(255, 255, 255), 2, Imgproc.LINE_AA, false);
                //                    Imgproc.putText(img, "Please read console message.", new Point(5, img.rows() - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 0.7, new Scalar(255, 255, 255), 2, Imgproc.LINE_AA, false);
            }
            else
                StartCoroutine(_OCR_Loop());
        }

        bool OCR_Init()
        {
            if (isOCR_Initialized)
                return true;

            if (string.IsNullOrEmpty(detectionmodel_filepath) || string.IsNullOrEmpty(recognitionmodel_filepath) || string.IsNullOrEmpty(charsettxt_filepath))
            {
                Debug.LogError(DETECTIONMODEL_FILENAME + " or " + RECOGNTIONMODEL_FILENAME + " or " + CHARSETTXT_FILENAME + " is not loaded. Please read “StreamingAssets/OpenCVForUnity/dnn/setup_dnn_module.pdf” to make the necessary setup.");
                isOCR_Initialized = false;
                return false;
            }
            else
            {
                // Create TextDetectionModel.
                detectonModel = new TextDetectionModel_DB(detectionmodel_filepath);
                detectonModel.setBinaryThreshold(detection_binary_threshold);
                detectonModel.setPolygonThreshold(detection_polygon_threshold);
                detectonModel.setUnclipRatio(detection_unclip_ratio);
                detectonModel.setMaxCandidates(detection_max_candidates);
                detectonModel.setInputParams(detection_inputScale, new Size(detection_inputSize_w, detection_inputSize_h), detection_inputMean);

                // Create TextRecognitonModel.
                recognitonModel = new TextRecognitionModel(recognitionmodel_filepath);
                recognitonModel.setDecodeType("CTC-greedy");
                recognitonModel.setVocabulary(loadCharset(charsettxt_filepath));
                recognitonModel.setInputParams(recogniton_inputScale, new Size(recogniton_inputSize_w, recogniton_inputSize_h), recogniton_inputMean);

                croppedMat = new Mat(new Size(recogniton_inputSize_w, recogniton_inputSize_h), CvType.CV_8SC3);
                croppedGrayMat = new Mat(croppedMat.size(), CvType.CV_8SC1);
                isOCR_Initialized = true;
                return true;
            }
        }

        void Run(string filename)
        {
            byte[] imagedata = System.IO.File.ReadAllBytes(filename);
            _RunOCR(imagedata);
        }

        public void _RunOCR(byte[] imagedata)
        {
            if (isBusy)
                return;

            isBusy = true;
            this.imagedata = imagedata;
            ocr_to_process = true;
        }

        Mat GetMat(byte[] imagedata)
        {
            Mat buff = new Mat(1, imagedata.Length, CvType.CV_8UC1);
            MatUtils.copyToMat(imagedata, buff);
            Mat img = Imgcodecs.imdecode(buff, Imgcodecs.IMREAD_COLOR);
            return img;
        }

        IEnumerator _OCR_Loop()
        {
            while (true)
            {
                isBusy = false;

                while (!ocr_to_process)
                    yield return new WaitForSeconds(0.01f);

                ocr_to_process = false;
                Utils.setDebugMode(true);

                Mat img = GetMat(imagedata);
                yield return null;

                string meta = GetMetadataImage(imagedata);
                //Debug.Log(meta);
                yield return null;

                if (img.empty())
                    continue;

                if (alreadycropped)
                {
                    OCR_result res1 = OCR(img);
                    OCR_image_results res = new OCR_image_results(meta, new() { res1 });
                    PublishResults(res);
                    yield return null;


                }
                else
                {
                    if (forceZone)
                        detection_arr = new RotatedRect[] { new RotatedRect(new Point(centre.x, centre.y), new Size(taille.x, taille.y), 0) };
                    else
                        detection_arr = FindTextZones(img);

                    OCR(img, detection_arr);

                    DrawZones(img, results);
                    yield return null;

                    OCR_image_results res = new OCR_image_results(meta, results);
                    PublishResults(res);
                    yield return null;

                    AdjustQuad(img);
                    yield return null;

                    DrawImageInQuad(img);
                    yield return null;
                }

                Utils.setDebugMode(false);
            }
        }

        string GetMetadataImage(byte[] data)
        {
            MemoryStream imageStream = new MemoryStream(data);
            ExifData exif = new ExifData(imageStream);
            exif.GetTagValue(ExifTag.ImageDescription, out string metadatas, StrCoding.Utf8);
            return metadatas;
        }

        void DrawImageInQuad(Mat img)
        {
            Imgproc.cvtColor(img, img, Imgproc.COLOR_BGR2RGB);

            Texture2D texture = new Texture2D(img.cols(), img.rows(), TextureFormat.RGB24, false);

            Utils.matToTexture2D(img, texture);

            gameObject.GetComponent<Renderer>().material.mainTexture = texture;
        }

        void AdjustQuad(Mat img)
        {
            gameObject.transform.localScale = new Vector3(img.width(), img.height(), 1);
            //Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            float imageWidth = img.width();
            float imageHeight = img.height();

            float widthScale = (float)Screen.width / imageWidth;
            float heightScale = (float)Screen.height / imageHeight;
            if (widthScale < heightScale)
                Camera.main.orthographicSize = (imageWidth * Screen.height / Screen.width) / 2;
            else
                Camera.main.orthographicSize = imageHeight / 2;
        }

        RotatedRect[] FindTextZones(Mat img)
        {
            tickMeter = new TickMeter();

            MatOfRotatedRect detections = new MatOfRotatedRect();
            MatOfFloat confidences = new MatOfFloat();

            tickMeter.start();
            detectonModel.detectTextRectangles(img, detections, confidences);
            tickMeter.stop();

            RotatedRect[] detectons_arr = detections.toArray();
            Debug.Log("FindTextZones time, ms: " + tickMeter.getTimeMilli());
            return detectons_arr;
        }

        void OCR(Mat img, RotatedRect[] detections_arr)
        {
            if (tickMeter == null)
                tickMeter = new TickMeter();

            results = new();
            foreach (RotatedRect detection_arr in detections_arr)
            {
                Point[] vertices = new Point[4];
                detection_arr.points(vertices);

                // Create transformed and cropped image.
                fourPointsTransform(img, croppedMat, vertices);
                Imgproc.cvtColor(croppedMat, croppedGrayMat, Imgproc.COLOR_BGR2GRAY);

                tickMeter.start();
                string recognitionResult = recognitonModel.recognize(croppedGrayMat);
                tickMeter.stop();

                result = new OCR_result(detection_arr, vertices, recognitionResult);
                results.Add(result);
            }

            Debug.Log("Inference time, ms: " + tickMeter.getTimeMilli());
            tickMeter = null;
        }

        OCR_result OCR(Mat img)
        {
            Imgproc.cvtColor(img, croppedGrayMat, Imgproc.COLOR_BGR2GRAY);
            string recognitionResult = recognitonModel.recognize(croppedGrayMat);
            return new OCR_result(recognitionResult);
        }

        void DrawZones(Mat img, List<OCR_result> results)
        {
            foreach (OCR_result result in results)
            {
                //encadre
                for (int i = 0; i < result.points.Length; i++)
                {
                    if (i == result.points.Length - 1)
                        Imgproc.line(img, result.points[i], result.points[0], new Scalar(0, 255, 0), 1);
                    else
                        Imgproc.line(img, result.points[i], result.points[i + 1], new Scalar(0, 255, 0), 1);
                }
                //écrit texte
                Imgproc.putText(img, result.text, result.points[1], Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar(0, 0, 255), 1, Imgproc.LINE_AA, false);
            }
        }

        void PublishResults(OCR_image_results results)
        {
            onNewResults?.Invoke(results);
            if (onNewResults_json != null)
            {
                string json = JsonConvert.SerializeObject(results);
                onNewResults_json.Invoke(json);
            }
        }

        /// <summary>
        /// Raises the disable event.
        /// </summary>
        void OnDisable()
        {
            detectonModel?.Dispose();
            recognitonModel?.Dispose();
            croppedMat?.Dispose();
            croppedGrayMat?.Dispose();
        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick()
        {
            SceneManager.LoadScene("OpenCVForUnityExample");
        }

        protected void fourPointsTransform(Mat src, Mat dst, Point[] vertices)
        {
            Size outputSize = dst.size();

            Point[] targetVertices = new Point[] { new Point(0, outputSize.height - 1),
                                                   new Point(0, 0),
                                                   new Point(outputSize.width - 1, 0),
                                                   new Point(outputSize.width - 1, outputSize.height - 1),
            };

            MatOfPoint2f verticesMat = new MatOfPoint2f(vertices);
            MatOfPoint2f targetVerticesMat = new MatOfPoint2f(targetVertices);
            Mat rotationMatrix = Imgproc.getPerspectiveTransform(verticesMat, targetVerticesMat);

            Imgproc.warpPerspective(src, dst, rotationMatrix, outputSize);
        }

        protected List<string> loadCharset(string charsetPath)
        {
            return new List<string>(File.ReadAllLines(charsetPath));
        }
    }
}
#endif