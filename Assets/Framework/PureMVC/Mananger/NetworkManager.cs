using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace PureMVC.Manager
{
    public class NetworkManager : Manager
    {
        /*
        UnityWebRequest uwr = new UnityWebRequest();
        uwr.url = "http://www.mysite.com";
        uwr.method = UnityWebRequest.kHttpVerbGET;   // can be set to any custom method, common constants privided

        uwr.useHttpContinue = false;
        uwr.chunkedTransfer = false;
        uwr.redirectLimit = 0;  // disable redirects
        uwr.timeout = 60;       // don't make this small, web requests do take some time 
        */

        /// <summary>
        /// get
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionResult"></param>
        public void Get(string url, Action<UnityWebRequest> actionResult)
        {
            StartCoroutine(Get_CO(url, actionResult));
        }

        /// <summary>
        /// get
        /// </summary>
        /// <param name="url">request url,like 'http://www.my-server.com/ '</param>
        /// <param name="actionResult">callback</param>
        /// <returns></returns>
        public IEnumerator Get_CO(string url, Action<UnityWebRequest> actionResult)
        {
            using (UnityWebRequest uwr = UnityWebRequest.Get(url))
            {
                yield return uwr.SendWebRequest();
                actionResult?.Invoke(uwr);
            }
        }

        /// <summary>
        /// send post to server
        /// </summary>
        /// <param name="serverURL">request server url,like "http://www.my-server.com/myform"</param>
        /// <param name="lstformData">form</param>
        /// <param name="lstformData">callback</param>
        /// <returns></returns>
        public void Post(string serverURL, List<IMultipartFormSection> lstformData, Action<UnityWebRequest> actionResult)
        {
            //List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            //formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));
            //formData.Add(new MultipartFormFileSection("my file data", "myfile.txt"));
            StartCoroutine(Post_CO(serverURL, lstformData, actionResult));
        }

        /// <summary>
        /// send post to server
        /// </summary>
        /// <param name="serverURL">request server url,like "http://www.my-server.com/myform"</param>
        /// <param name="lstformData">form</param>
        /// <param name="lstformData">callback</param>
        /// <returns></returns>
        public IEnumerator Post_CO(string serverURL, List<IMultipartFormSection> lstformData, Action<UnityWebRequest> actionResult)
        {
            //List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            //formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));
            //formData.Add(new MultipartFormFileSection("my file data", "myfile.txt"));
            UnityWebRequest uwr = UnityWebRequest.Post(serverURL, lstformData);
            yield return uwr.SendWebRequest();
            actionResult?.Invoke(uwr);
        }

        /// <summary>
        /// get file
        /// </summary>
        /// <param name="url">request url</param>
        /// <param name="downloadFilePathAndName">save directory like 'Application.persistentDataPath+"/unity3d.html"'</param>
        /// <param name="actionResult">callback</param>
        /// <returns></returns>
        public void GetFile(string url, string downloadFilePathAndName, Action<UnityWebRequest> actionResult)
        {
            StartCoroutine(GetFile_CO(url, downloadFilePathAndName, actionResult));
        }

        /// <summary>
        /// get file
        /// </summary>
        /// <param name="url">request url</param>
        /// <param name="downloadFilePathAndName">save directory like 'Application.persistentDataPath+"/unity3d.html"'</param>
        /// <param name="actionResult">callback</param>
        /// <returns></returns>
        public IEnumerator GetFile_CO(string url, string downloadFilePathAndName, Action<UnityWebRequest> actionResult)
        {
            var uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            uwr.downloadHandler = new DownloadHandlerFile(downloadFilePathAndName);
            yield return uwr.SendWebRequest();
            actionResult?.Invoke(uwr);
        }

        /// <summary>
        /// request text
        /// </summary>
        /// <param name="url">text url</param>
        /// <param name="onSuccess">callback</param>
        /// <param name="onFail">callback</param>
        /// <returns></returns>
        public void GetText(string url, Action<string> onSuccess, Action<string> onFail)
        {
            StartCoroutine(GetText_CO(url, onSuccess, onFail));
        }

        /// <summary>
        /// request text
        /// </summary>
        /// <param name="url">text url</param>
        /// <param name="onSuccess">callback</param>
        /// <param name="onFail">callback</param>
        /// <returns></returns>
        public IEnumerator GetText_CO(string url, Action<string> onSuccess, Action<string> onFail)
        {
            UnityWebRequest uwr = UnityWebRequest.Get(url);
            yield return uwr.SendWebRequest();
            if (!(uwr.isNetworkError || uwr.isHttpError))
            {
                onSuccess?.Invoke(uwr.downloadHandler.text);
            }
            else
            {
                onFail?.Invoke(uwr.error);
            }
        }

        /// <summary>
        /// request byte[]
        /// </summary>
        /// <param name="url">byte[] url</param>
        /// <param name="onSuccess">callback</param>
        /// <param name="onFail">callback</param>
        /// <returns></returns>
        public void GetBytes(string url, Action<byte[]> onSuccess, Action<string> onFail)
        {
            StartCoroutine(GetBytes_CO(url, onSuccess, onFail));
        }

        /// <summary>
        /// request byte[]
        /// </summary>
        /// <param name="url">byte[] url</param>
        /// <param name="onSuccess">callback</param>
        /// <param name="onFail">callback</param>
        /// <returns></returns>
        public IEnumerator GetBytes_CO(string url, Action<byte[]> onSuccess, Action<string> onFail)
        {
            UnityWebRequest uwr = UnityWebRequest.Get(url);
            yield return uwr.SendWebRequest();
            if (!(uwr.isNetworkError || uwr.isHttpError))
            {
                onSuccess?.Invoke(uwr.downloadHandler.data);
            }
            else
            {
                onFail?.Invoke(uwr.error);
            }
        }

        /// <summary>
        /// request image
        /// </summary>
        /// <param name="url">image url,like 'http://www.my-server.com/image.png '</param>
        /// <param name="onSuccess">callback</param>
        /// <param name="onFail">callback</param>
        /// <param name="isTexture2dReadable">texture2d readonly</param>
        /// <returns></returns>
        public void GetTexture(string url, Action<Texture2D> onSuccess, Action<string> onFail, bool isTexture2dReadable)
        {
            StartCoroutine(GetTexture_CO(url, onSuccess, onFail, isTexture2dReadable));
        }

        /// <summary>
        /// request image
        /// </summary>
        /// <param name="url">image url,like 'http://www.my-server.com/image.png '</param>
        /// <param name="onSuccess">callback</param>
        /// <param name="onFail">callback</param>
        /// <param name="isTexture2dReadable">texture2d readonly</param>
        /// <returns></returns>
        public IEnumerator GetTexture_CO(string url, Action<Texture2D> onSuccess, Action<string> onFail, bool isTexture2dReadable)
        {
            UnityWebRequest uwr = new UnityWebRequest(url);
            DownloadHandlerTexture downloadTexture = new DownloadHandlerTexture(isTexture2dReadable);
            uwr.downloadHandler = downloadTexture;
            yield return uwr.SendWebRequest();
            if (!(uwr.isNetworkError || uwr.isHttpError))
            {
                Texture2D texture2d = downloadTexture.texture;
                texture2d.wrapMode = TextureWrapMode.Clamp;
                onSuccess?.Invoke(texture2d);
            }
            else
            {
                onFail?.Invoke(uwr.error);
            }
            uwr.Dispose();
        }

        /// <summary>
        /// request image
        /// </summary>
        /// <param name="url">image url,like 'http://www.my-server.com/image.png '</param>
        /// <param name="onSuccess">callback</param>
        /// <param name="onFail">callback</param>
        /// <param name="isTexture2dReadable">texture2d readonly</param>
        /// <returns></returns>
        public void GetTextureEncrypted(string url, Action<Texture2D> onSuccess, Action<string> onFail, bool isTexture2dReadable)
        {
            StartCoroutine(GetTextureEncrypted_CO(url, onSuccess, onFail, isTexture2dReadable));
        }

        /// <summary>
        /// request image
        /// </summary>
        /// <param name="url">image url,like 'http://www.my-server.com/image.png '</param>
        /// <param name="onSuccess">callback</param>
        /// <param name="onFail">callback</param>
        /// <param name="isTexture2dReadable">texture2d readonly</param>
        /// <returns></returns>
        public IEnumerator GetTextureEncrypted_CO(string url, Action<Texture2D> onSuccess, Action<string> onFail, bool isTexture2dReadable)
        {
            UnityWebRequest uwr = UnityWebRequest.Get(url + ".txt");
            yield return uwr.SendWebRequest();
            if (!(uwr.isNetworkError || uwr.isHttpError))
            {
                Texture2D texture2d = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture2d.wrapMode = TextureWrapMode.Clamp;
                byte[] bytes = ResourcesManager.ReverseBytes(uwr.downloadHandler.data);
                texture2d.LoadImage(bytes, !isTexture2dReadable);
                onSuccess?.Invoke(texture2d);
            }
            else
            {
                onFail?.Invoke(uwr.error);
            }
            uwr.Dispose();
        }

        /// <summary>
        /// request image
        /// </summary>
        /// <param name="url">image url,like 'http://www.my-server.com/image.png '</param>
        /// <param name="onSuccess">callback</param>
        /// <param name="onFail">callback</param>
        /// <param name="isTexture2dReadonly">texture2d readonly</param>
        /// <returns></returns>
        public void GetTextureEncryptedPriority(string url, Action<Texture2D> onSuccess, Action<string> onFail, bool isTexture2dReadonly, bool isCheckEncryptFirst)
        {
            if (true == isCheckEncryptFirst)
            {
                StartCoroutine(GetTextureEncrypted_CO(url, onSuccess, (string error1) =>
                {
                    StartCoroutine(GetTexture_CO(url, onSuccess, (string error2) =>
                    {
                        onFail?.Invoke(string.Format("GetTextureEncryptedPriority Error[{0}][{1}]", error1, error2));
                    }, isTexture2dReadonly));
                }, isTexture2dReadonly));
            }
            else
            {
                StartCoroutine(GetTexture_CO(url, onSuccess, (string error1) =>
                {
                    StartCoroutine(GetTextureEncrypted_CO(url, onSuccess, (string error2) =>
                    {
                        onFail?.Invoke(string.Format("GetTextureEncryptedPriority Error[{0}][{1}]", error1, error2));
                    }, isTexture2dReadonly));
                }, isTexture2dReadonly));
            }
        }

        /// <summary>
        /// request assetbundle
        /// </summary>
        /// <param name="url">assetbundle url,like 'http://www.my-server.com/myData.unity3d'</param>
        /// <param name="onSuccess">callback</param>
        /// <param name="onFail">callback</param>
        /// <returns></returns>
        public void GetAssetBundle(string url, Action<AssetBundle> onSuccess, Action<string> onFail)
        {
            StartCoroutine(GetAssetBundle_CO(url, onSuccess, onFail));
        }

        /// <summary>
        /// request assetbundle
        /// </summary>
        /// <param name="url">assetbundle url,like 'http://www.my-server.com/myData.unity3d'</param>
        /// <param name="onSuccess">callback</param>
        /// <param name="onFail">callback</param>
        /// <returns></returns>
        public IEnumerator GetAssetBundle_CO(string url, Action<AssetBundle> onSuccess, Action<string> onFail)
        {
            UnityWebRequest uwr = new UnityWebRequest(url);
            DownloadHandlerAssetBundle handler = new DownloadHandlerAssetBundle(uwr.url, 0);
            uwr.downloadHandler = handler;
            yield return uwr.SendWebRequest();
            if (!(uwr.isNetworkError || uwr.isHttpError))
            {
                AssetBundle bundle = handler.assetBundle;
                onSuccess?.Invoke(bundle);
            }
            else
            {
                onFail?.Invoke(uwr.error);
            }
        }

        /// <summary>
        /// request audio
        /// </summary>
        /// <param name="url">audio url,like 'http://myserver.com/mysound.wav'</param>
        /// <param name="onSuccess">callback</param>
        /// <param name="onFail">callback</param>
        /// <param name="audioType">audio type</param>
        /// <returns></returns>
        public void GetAudioClip(string url, Action<AudioClip> onSuccess, Action<string> onFail, AudioType audioType = AudioType.WAV)
        {
            StartCoroutine(GetAudioClip_CO(url, onSuccess, onFail, audioType));
        }

        /// <summary>
        /// request audioclip
        /// </summary>
        /// <param name="url">audio url,like 'http://myserver.com/mysound.wav'</param>
        /// <param name="onSuccess">callback</param>
        /// <param name="onFail">callback</param>
        /// <param name="audioType">audio type</param>
        /// <returns></returns>
        public IEnumerator GetAudioClip_CO(string url, Action<AudioClip> onSuccess, Action<string> onFail, AudioType audioType = AudioType.WAV)
        {
            using (var uwr = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
            {
                yield return uwr.SendWebRequest();
                if (!(uwr.isNetworkError || uwr.isHttpError))
                {
                    onSuccess?.Invoke(DownloadHandlerAudioClip.GetContent(uwr));
                }
                else
                {
                    onFail?.Invoke(string.Format("GetAudioClip Fail Error[{0}]", uwr.error));
                }
            }
        }

        /// <summary>
        /// send byte by put
        /// </summary>
        /// <param name="url">server url like 'http://www.my-server.com/upload' </param>
        /// <param name="contentBytes">bytes</param>
        /// <param name="actionResult">callback</param>
        /// <returns></returns>
        public void UploadByPut(string url, byte[] contentBytes, Action<bool> actionResult)
        {
            StartCoroutine(UploadByPut_CO(url, contentBytes, actionResult, ""));
        }

        /// <summary>
        /// send byte by put
        /// </summary>
        /// <param name="url">server url like 'http://www.my-server.com/upload' </param>
        /// <param name="contentBytes">bytes</param>
        /// <param name="actionResult">callback</param>
        /// <param name="contentType">set header Content-Type property</param>
        /// <returns></returns>
        public IEnumerator UploadByPut_CO(string url, byte[] contentBytes, Action<bool> actionResult, string contentType = "application/octet-stream")
        {
            UnityWebRequest uwr = new UnityWebRequest(url);
            UploadHandler uploader = new UploadHandlerRaw(contentBytes);

            // Sends header: "Content-Type: custom/content-type";
            uploader.contentType = contentType;

            uwr.uploadHandler = uploader;

            yield return uwr.SendWebRequest();

            bool res = true;
            if (uwr.isNetworkError || uwr.isHttpError)
            {
                res = false;
            }
            actionResult?.Invoke(res);
        }
    }
}
