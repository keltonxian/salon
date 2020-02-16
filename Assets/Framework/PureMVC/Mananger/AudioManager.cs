using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

namespace PureMVC.Manager
{
    public class AudioManager : Manager
    {
        public enum LoadPath
        {
            Resources,
            StreamingAssets,
        }

        public float Volume
        {
            get
            {
                return AudioListener.volume;
            }
            set
            {
                AudioListener.volume = value;
            }
        }

        private Dictionary<string, AudioSource> _dicMusicGUID = new Dictionary<string, AudioSource>();
        private Dictionary<string, AudioSource> _dicMusicPath = new Dictionary<string, AudioSource>();
        private List<string> _listRemoveGUID = new List<string>();
        private AudioSource _bgmAudioSource;
        public AudioSource BGMAudioSource
        {
            get
            {
                return _bgmAudioSource;
            }
        }

        void Awake()
        {
            _bgmAudioSource = gameObject.AddComponent<AudioSource>();
            _bgmAudioSource.loop = true;
            _bgmAudioSource.playOnAwake = false;
        }

        void FixedUpdate()
        {
            if (_dicMusicGUID.Count > 0)
            {
                _listRemoveGUID.Clear();
                foreach(string guid in _dicMusicGUID.Keys)
                {
                    AudioSource audioSource = _dicMusicGUID[guid];
                    if (null == audioSource || (false == audioSource.loop && false == audioSource.isPlaying))
                    {
                        _listRemoveGUID.Add(guid);
                    }
                }
                for(int i = 0; i < _listRemoveGUID.Count; i++)
                {
                    StopMusicByGUID(_listRemoveGUID[i]);
                }
            }
        }

        public void PlaySound (string resourcePath, float volume = 1f, float delay = 0f, LoadPath loadPath = LoadPath.Resources, bool useSearchPath = true, string searchPath = "")
        {
            if (Volume < 0.01f)
            {
                return;
            }
            if (loadPath == LoadPath.Resources)
            {
                AudioClip audioClip = Resources.Load<AudioClip>((useSearchPath ? searchPath : "") + resourcePath);
                if (null != audioClip)
                {
                    audioClip.name = resourcePath;
                    PlaySound(audioClip, volume, delay);
                }
                else
                {
                    Debug.LogWarning("AudioManager PlaySound Not Found: " + resourcePath);
                }
            }
            else
            {
                LoadFromStreamingAssets(resourcePath, delegate (AudioClip audioClip)
                {
                    audioClip.name = resourcePath;
                    PlaySound(audioClip, volume, delay);
                }, useSearchPath, searchPath);
            }
        }

        public void PlaySound(AudioClip audioClip, float volume = 1f, float delay = 0f)
        {
            if (null == audioClip || Volume < 0.01f)
            {
                return;
            }
            GameObject go = new GameObject(audioClip.name);
            go.transform.SetParent(transform);
            go.transform.position = Vector3.zero;
            AudioSource audioSource = go.AddComponent<AudioSource>();
            audioSource.loop = false;
            audioSource.volume = volume;
            audioSource.clip = audioClip;
            if (delay <= 0f)
            {
                audioSource.Play();
                Destroy(go, audioClip.length);
            }
            else
            {
                StartCoroutine(DelayPlaySound(audioSource, delay, audioClip.length));
            }
        }

        public IEnumerator DelayPlaySound (AudioSource audioSource, float delay, float clipLength)
        {
            yield return new WaitForSeconds(delay);
            if (null != audioSource)
            {
                audioSource.Play();
                yield return new WaitForSeconds(clipLength);
                if (null != audioSource.gameObject)
                {
                    Destroy(audioSource.gameObject);
                }
            }
        }

        public void LoadFromStreamingAssets(string path, Action<AudioClip> onLoaded, bool useSearchPath = true, string searchPath = "")
        {
            string spath = useSearchPath ? searchPath : "";
            string url = "";
            if (System.IO.File.Exists(Application.persistentDataPath + spath + path))
            {
                url = DataManager.PathPersistentData + spath + path;
            }
            else
            {
                url = DataManager.PathStreamingAssets + spath + path;
            }
            NetworkManager.GetAudioClip(url, (AudioClip audioClip) =>
             {
                 if (null != audioClip)
                 {
                     onLoaded?.Invoke(audioClip);
                 }
                 else
                 {
                     Debug.LogWarning("LoadFromStreamingAssets Not Found: " + path);
                 }
             }, (string error) =>
             {
                 Debug.LogWarning("LoadFromStreamingAssets Request Error: " + error);
             }, AudioType.MPEG);
        }

        public void StopSound (string resourcePath)
        {
            AudioSource audioSource = null;
            foreach (AudioSource a in GetComponentsInChildren<AudioSource>())
            {
                if (a.transform != transform && a.loop == false && a.name.LastIndexOf(resourcePath, StringComparison.Ordinal) > -1)
                {
                    audioSource = a;
                    break;
                }
            }
            if (null != audioSource)
            {
                Destroy(audioSource.gameObject);
            }
        }

        public string PlayMusic(string resourcePath, float volume = 1f, bool isLoop = true, bool dontDestroy = false, LoadPath loadPath = LoadPath.Resources, bool useSearchPath = true, string searchPath = "")
        {
            if (_dicMusicPath.ContainsKey(resourcePath) && null != _dicMusicPath[resourcePath])
            {
                AudioSource audioSource = _dicMusicPath[resourcePath];
                if (!audioSource.isPlaying)
                {
                    audioSource.loop = isLoop;
                    audioSource.Play();
                }
                foreach (string key in _dicMusicGUID.Keys)
                {
                    if (_dicMusicGUID[key]==audioSource)
                    {
                        return key;
                    }
                }
            }
            else
            {
                if(loadPath == LoadPath.Resources)
                {
                    AudioClip audioClip = Resources.Load<AudioClip>((useSearchPath ? searchPath : "") + resourcePath);
                    if (null != audioClip)
                    {
                        string id = PlayMusic(audioClip, volume, isLoop, dontDestroy);
                        AudioSource audioSource = _dicMusicGUID[id];
                        _dicMusicPath[resourcePath] = audioSource;
                        return id;
                    }
                }
                else
                {
                    LoadFromStreamingAssets(resourcePath, delegate (AudioClip audioClip)
                     {
                         if (null != audioClip)
                         {
                             string id = PlayMusic(audioClip, volume, isLoop, dontDestroy);
                             AudioSource audioSource = _dicMusicGUID[id];
                             _dicMusicPath[resourcePath] = audioSource;
                         }
                     }, useSearchPath);
                }
            }
            return null;
        }

        public string PlayMusic (AudioClip audioClip, float volume = 1f, bool isLoop = true, bool dontDestroy = false)
        {
            if (null == audioClip)
            {
                return null;
            }
            string guid = Guid.NewGuid().ToString();
            GameObject go = new GameObject("Music_" + guid);
            if (dontDestroy)
            {
                DontDestroyOnLoad(go);
            }
            go.transform.SetParent(transform);
            go.transform.position = Vector3.zero;
            AudioSource audioSource = go.AddComponent<AudioSource>();
            audioSource.loop = isLoop;
            audioSource.volume = volume;
            audioSource.clip = audioClip;
            audioSource.Play();
            _dicMusicGUID[guid] = audioSource;
            return guid;
        }

        public void StopMusicByGUID (string guid)
        {
            if (!_dicMusicGUID.ContainsKey(guid))
            {
                return;
            }
            AudioSource audioSource = _dicMusicGUID[guid];
            if (null != audioSource)
            {
                audioSource.Stop();
                string id = null;
                foreach(string path in _dicMusicPath.Keys)
                {
                    if(_dicMusicPath[path] == audioSource)
                    {
                        id = path;
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(id))
                {
                    _dicMusicPath.Remove(id);
                }
                Destroy(audioSource.gameObject);
            }
            _dicMusicGUID.Remove(guid);
        }

        public void StopMusicByPath (string path)
        {
            if (!_dicMusicPath.ContainsKey(path))
            {
                return;
            }
            AudioSource audioSource = _dicMusicPath[path];
            if (null != audioSource)
            {
                audioSource.Stop();
                string id = null;
                foreach (string guid in _dicMusicGUID.Keys)
                {
                    if (_dicMusicGUID[guid] == audioSource)
                    {
                        id = guid;
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(id))
                {
                    _dicMusicGUID.Remove(id);
                }
                Destroy(audioSource.gameObject);
            }
            _dicMusicPath.Remove(path);
        }

        public AudioSource GetMusicByGUID(string guid)
        {
            if (_dicMusicGUID.ContainsKey(guid))
            {
                return _dicMusicGUID[guid];
            }
            return null;
        }

        public AudioSource GetMusicByPath(string path)
        {
            if (_dicMusicPath.ContainsKey(path))
            {
                return _dicMusicPath[path];
            }
            return null;
        }


        public void PlayBGM(AudioClip audioClip, bool isContinue = true)
        {
            if (null == audioClip)
            {
                return;
            }
            if (true == isContinue && audioClip == _bgmAudioSource.clip && true == _bgmAudioSource.isPlaying)
            {
                return;
            }
            _bgmAudioSource.clip = audioClip;
            _bgmAudioSource.Play();
        }

        public void StopBGM()
        {
            _bgmAudioSource.Stop();
        }

        public float BGMVolume
        {
            get
            {
                return _bgmAudioSource.volume;
            }
            set
            {
                _bgmAudioSource.volume = value;
            }
        }

        public void StopAll (bool isStopBGM = false)
        {
            foreach (var item in _dicMusicGUID)
            {
                AudioSource audioSource = item.Value;
                audioSource.Stop();
                Destroy(audioSource.gameObject);
            }
            _dicMusicGUID.Clear();
            _dicMusicPath.Clear();

            List<AudioSource> listSound = new List<AudioSource>();
            foreach (AudioSource tf in GetComponentsInChildren <AudioSource>(true))
            {
                if(tf.transform != transform)
                {
                    listSound.Add(tf);
                }
            }
            for (int i = 0; i < listSound.Count; i++)
            {
                Destroy(listSound[i].gameObject);
            }
            if (isStopBGM)
            {
                StopBGM();
            }
        }

    }
}
