using UnityEngine;
using System;
using BestHTTP;
using BestHTTP.Cookies;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace HttpSDK
{
    public class MyHttpApp : MonoBehaviour
    {
        public bool enableCookie = false;
        public string cloudfrontPolicy = "";
        public string cloudfrontSignature = "";
        public string cloudfrontKeyID = "";
        public string savedFileName = "512MB.zip";

        public Stopwatch sw;

        [SerializeField]
        public string Logger = "";
        public static string persistentDataPath = "";


        public void Start()
        {
          
            savedFileName = "512MB.zip";

            sw = new Stopwatch();
            persistentDataPath = Application.persistentDataPath + "/";
         
        }

        public void OnGUI()
        {
            if (GUI.Button(new Rect(0, 50, 100, 50), "Clear Log"))
            {
                Logger = "";
            }
            GUI.Label(new Rect(0, 120, 300, 800), Logger);

        }

        
        public void DownloadData()
        {
            // Replace serverResourceUrl with yours 
            string serverResourceUrl = "https://xxxxxx.cloudfront.net/512MB.zip";

            // Replace cloudPolicy, cloudSignature, and cloudKeyPairId with yours 
            string cloudPolicy = "";
            string cloudSignature = "";
            string cloudKeyPairId = "";

            var request = new HTTPRequest(new Uri(serverResourceUrl), OnRequestFinished);

            Debug.Log(request.DumpHeaders());

            if (enableCookie)
            {
                request.Cookies.Add(new Cookie("CloudFront-Policy", cloudPolicy));
                request.Cookies.Add(new Cookie("CloudFront-Signature", cloudSignature));
                request.Cookies.Add(new Cookie("CloudFront-Key-Pair-Id", cloudKeyPairId));

            }

            request.DisableCache = true;

            request.OnStreamingData += OnData;
            request.OnDownloadProgress += OnDownloadProgress;

           
            sw.Start();

            request.Send();

        }

        private bool OnData(HTTPRequest req, HTTPResponse resp, byte[] dataFragment, int dataFragmentLength)
        {
            if (resp.IsSuccess)
            {
                var fs = req.Tag as System.IO.FileStream;
                if (fs == null)
                {
                    req.Tag = fs = new System.IO.FileStream(persistentDataPath + savedFileName, System.IO.FileMode.Create);
                }

                Debug.Log("Write dataFragmentLength:" + dataFragmentLength);

                fs.Write(dataFragment, 0, dataFragmentLength);
            }

            // Return true if dataFragment is processed so the plugin can recycle it
            return true;
        }

        private void OnDownloadProgress(HTTPRequest originalRequest, long downloaded, long downloadLength)
        {
            double downloadPercent = (downloaded / (double)downloadLength) * 100;
            var value = (float)downloadPercent;
            var downloadProgress = string.Format("{0:F1}%", downloadPercent);

            Debug.Log("downloadProgress:" + downloadProgress);
        }

        private void OnRequestFinished(HTTPRequest req, HTTPResponse resp)
        {
            Debug.Log("OnRequestFinished");
            UnityEngine.Debug.Log("Successfully to write file_name:" + persistentDataPath + "512MB.zip");
            Logger += "Successfully to write file_name:" + persistentDataPath + "512MB.zip";

            Logger += "\n\n";

            sw.Stop();

            TimeSpan ts = sw.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);

            Debug.Log("elapsedTime: " + elapsedTime);
            Logger += "elapsedTime: " + elapsedTime;

            var fs = req.Tag as System.IO.FileStream;
            if (fs != null)
                fs.Dispose();

            switch (req.State)
            {
                // The request finished without any problem.
                case HTTPRequestStates.Finished:
                    if (resp.IsSuccess)
                    {
                        Debug.Log("Done!");
                        Logger += "\n\n";
                        Logger += "Done:\n";

                    }
                    else
                    {
                        Debug.LogWarning(string.Format("Request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}",
                                                        resp.StatusCode,
                                                        resp.Message,
                                                        resp.DataAsText));
                    }
                    break;

                default:
                    // There were an error while downloading the content.
                    // The incomplete file should be deleted.
                    System.IO.File.Delete(persistentDataPath + savedFileName);
                    break;
            }
        }
    }
}