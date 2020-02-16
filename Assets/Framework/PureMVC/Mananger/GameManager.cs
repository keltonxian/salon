using UnityEngine;
using System.Collections;
using PureMVC.Const;
using UnityEngine.SceneManagement;
using System.Reflection;
using System;

namespace PureMVC.Manager
{
    public class GameManager : Manager
    {
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                return _instance;
            }
        }
        private string _nextSceneName;
        private AsyncOperation _asyncOperation;
        private bool _isLoadingNextScene = false;
        private bool _isShowLog = true;

        public enum LogLevel
        {
            Debug, Warning, Error
        }
        public void Log(string msg, LogLevel logLevel = LogLevel.Debug)
        {
            if (false == _isShowLog)
            {
                return;
            }

            // Get Current Method
            //MethodBase mb = MethodBase.GetCurrentMethod();

            // Get Upper Method
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
            // Index: 0: Self. 1: Upper. 2: Upper Upper ......
            MethodBase mb = st.GetFrame(1).GetMethod();

            //string systemModule = Environment.NewLine;
            //systemModule += "ModuleName:" + mb.Module + Environment.NewLine;
            //systemModule += "NameSpace:" + mb.DeclaringType.Namespace + Environment.NewLine;
            //systemModule += "Class:" + mb.DeclaringType.Name + Environment.NewLine;
            //systemModule += "Method:" + mb.Name;

            msg = string.Format("Class[{0}] Method[{1}] Msg[{2}]", mb.DeclaringType.Name, mb.Name, msg);

            if (logLevel == LogLevel.Debug)
            {
                Debug.Log(msg);
            }
            else if (logLevel == LogLevel.Warning)
            {
                Debug.LogWarning(msg);
            }
            else if (logLevel == LogLevel.Error)
            {
                Debug.LogError(msg);
            }
        }

        public enum RoleType
        {
            Kelton = 0,
            Kobe = 1,
            Tony = 2,
        }
        private RoleType _role = RoleType.Kobe;
        public RoleType Role
        {
            get
            {
                return _role;
            }
            set
            {
                _role = value;
            }
        }

        void Awake()
        {
            Init();
        }

        void Init()
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.targetFrameRate = AppConst.GameFrameRate;
        }

        public void LoadNextScene(string nextSceneName)
        {
            if (true == _isLoadingNextScene)
            {
                return;
            }
            _isLoadingNextScene = true;
            _nextSceneName = nextSceneName;
            StartCoroutine(LoadNextSceneCO("SceneLoading"));
        }

        public void LoadNextSceneInLoading()
        {
            if (true == _isLoadingNextScene)
            {
                return;
            }
            _isLoadingNextScene = true;
            StartCoroutine(LoadNextSceneCO(_nextSceneName));
        }

        private IEnumerator LoadNextSceneCO(string nextSceneName)
        {
            string preSceneName = SceneManager.GetActiveScene().name;
            _asyncOperation = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
            while (!_asyncOperation.isDone)
            {
                yield return 0;
            }
            _isLoadingNextScene = false;
        }

    }
}
