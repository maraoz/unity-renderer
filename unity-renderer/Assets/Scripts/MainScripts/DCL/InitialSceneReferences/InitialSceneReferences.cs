using System.Data.Common;
using DCL.Camera;
using DCL.Helpers;
using UnityEngine;

namespace DCL
{
    public class InitialSceneReferences : MonoBehaviour, IInititalSceneReference
    {
        public class Data
        {
            public MouseCatcher mouseCatcher;
            public GameObject groundVisual;
            public GameObject cameraParent;
            public InputController inputController;
            public GameObject cursorCanvas;
            public BuilderInWorldBridge builderInWorldBridge;
            public PlayerAvatarController playerAvatarController;
            public CameraController cameraController;
            public UnityEngine.Camera mainCamera;
            public GameObject bridgeGameObject;

            public Data () { }

            public Data(InitialSceneReferences component)
            {
                cameraController = component.cameraControllerReference;
                cameraParent = component.cameraParentReference;
                cursorCanvas = component.cursorCanvasReference;
                groundVisual = component.groundVisualReference;
                inputController = component.inputControllerReference;
                mainCamera = component.mainCameraReference;
                builderInWorldBridge = component.builderInWorldBridgeReference;
                playerAvatarController = component.playerAvatarControllerReference;
                mouseCatcher = component.mouseCatcherReference;
            }
        }

        public Data data;

        [SerializeField] private MouseCatcher mouseCatcherReference;
        [SerializeField] private GameObject groundVisualReference;
        [SerializeField] private GameObject cameraParentReference;
        [SerializeField] private InputController inputControllerReference;
        [SerializeField] private GameObject cursorCanvasReference;
        [SerializeField] private BuilderInWorldBridge builderInWorldBridgeReference;
        [SerializeField] private PlayerAvatarController playerAvatarControllerReference;
        [SerializeField] private CameraController cameraControllerReference;
        [SerializeField] private UnityEngine.Camera mainCameraReference;
        [SerializeField] private GameObject bridgeGameObjectReference;

        public GameObject groundVisual => data.groundVisual;
        public GameObject cameraParent => data.cameraParent;
        public GameObject cursorCanvas => data.cursorCanvas;
        public MouseCatcher mouseCatcher => data.mouseCatcher;
        public InputController inputController => data.inputController;
        public BuilderInWorldBridge builderInWorldBridge => data.builderInWorldBridge;
        public PlayerAvatarController playerAvatarController => data.playerAvatarController;
        public CameraController cameraController => data.cameraController;
        public UnityEngine.Camera mainCamera { get { return mainCameraReference; } }
        public GameObject bridgeGameObject { get { return bridgeGameObjectReference; } }

        public static InitialSceneReferences i { get; private set; }

        void Awake()
        {
            if (i != null)
            {
                Utils.SafeDestroy(this);
                return;
            }

            i = this;
            data = new Data(this);
        }

        void OnDestroy() { i = null; }
    }
}