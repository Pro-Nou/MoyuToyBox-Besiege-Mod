using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using Modding;
using Modding.Blocks;

namespace MoyuToyBox
{
    class CannonStript: MonoBehaviour
    {
        public Action PropertiseChangedEvent;
        private Rect UIrect = new Rect(0, 100, 512, 128);

        public BlockBehaviour BB { get; internal set; }
        public MToggle HasTrail;
        public MColourSlider TrailColorS;
        public CanonBlock CB;

        public TrailRenderer TrailRenderer;
        public bool TrailEnable { get { return TrailRenderer.enabled; } set { TrailRenderer.enabled = value; } }
        public float TrailLength { get { return TrailRenderer.time; } set { TrailRenderer.time = value; } }
        public Color TrailColor { get { return TrailRenderer.material.color; } set { TrailRenderer.material.SetColor("_TintColor", value); } }

        public Material[] matArray = new Material[1];
        public Color BallColor { get { return matArray[0].color; } set { matArray[0].SetColor("_TintColor", value); } }
        
        private void Awake()
        {

            BB = GetComponent<BlockBehaviour>();
            
            SafeAwake();

            if (BB.isSimulating ) { return; }        


            //Enhancement.Toggled += (bool value) => {  PropertiseChangedEvent();};

            //LoadConfiguration();    

            //PropertiseChangedEvent += ChangedProperties;
            //PropertiseChangedEvent += () => { DisplayInMapper(Enhancement.IsActive); };
            //PropertiseChangedEvent?.Invoke();

            //Controller.Instance.OnSave += SaveConfiguration;
        }
        
        public void InitBullet()
        {

            CB.boltObject.gameObject.name = "ballchanged";

            Renderer renderer1 = CB.boltObject.gameObject.GetComponent<Renderer>();

            matArray[0] = new Material(Shader.Find("Particles/Alpha Blended"));
            matArray[0].mainTexture = Texture2D.whiteTexture;
            matArray[0].SetColor("_TintColor", new Color(1, 1, 1, 1));

            renderer1.materials = matArray;

            TrailRenderer = CB.boltObject.gameObject.GetComponent<TrailRenderer>() ?? CB.boltObject.gameObject.AddComponent<TrailRenderer>();
            TrailRenderer.autodestruct = false;
            TrailRenderer.receiveShadows = false;
            TrailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            TrailRenderer.startWidth = 0.5f;
            TrailRenderer.endWidth = 0.5f;

            TrailRenderer.material = new Material(Shader.Find("Particles/Additive"));

            TrailEnable = true;
            TrailLength = 0.5f;
        }

        public virtual void SafeAwake()
        {
            HasTrail = BB.AddToggle("HasTrail", "HasTrail", false);
            TrailColorS = BB.AddColourSlider("color", "color", new Color(1, 0, 0, 1), false);
            CB = BB.GetComponent<CanonBlock>();
            CB.knockbackSpeed = 0f;
            InitBullet();
            BB.name = "Changed Cannon";

            HasTrail.DisplayInMapper = !StatMaster.isMP;
            TrailColorS.DisplayInMapper = !StatMaster.isMP;
        }
        public MToggle AddToggle(string displayName, string key, bool defaultValue)
        {
            var mapper = BB.AddToggle(displayName, key, defaultValue);
            mapper.Toggled += (value) => { PropertiseChangedEvent(); };
            return mapper;
        }
        public MColourSlider AddColourSlider(string displayName, string key, Color defaultValue, bool snapColors)
        {
            var mapper = BB.AddColourSlider(displayName, key, defaultValue, snapColors);
            mapper.ValueChanged += (value) => { if (Input.GetKeyUp(KeyCode.Mouse0)) PropertiseChangedEvent(); };
            return mapper;
        }
        public void Start()
        {
            BallColor=TrailColor = TrailColorS.Value;
            if (HasTrail.IsActive)
                TrailEnable = true;
            else
                TrailEnable = false;

        }


        private void Update()
        {
            if (BB.isSimulating)
                SimulateUpdate();
        }
        private void SimulateUpdate()
        {
            if (!StatMaster.isClient)
                SimulateUpdateHost();
        }
        private void SimulateUpdateHost()
        {
            //CB.boltObject.gameObject.SetActive(true);
            //if (CB.ShootKey.IsHeld)
            {
            //    StartCoroutine(Shoot());
            }
        }
        /*
        readonly GUIStyle vec3Style = new GUIStyle()
        {
            fontStyle = FontStyle.Bold,
            fontSize = 16,
            normal = { textColor = new Color(1, 1, 1, 1) },
            alignment = TextAnchor.MiddleCenter
        };

        void OnGUI()
        {
            //if (cameraController.activeCamera)
            //if (!isFirstFrame || BlockBehaviour.isSimulating)
            {            //Vector3 a = cameraController.activeCamera.GetComponentInParent<BlockBehaviour>().GetTransform().forward;

                //GUI.Box(UIrect, PrefabMaster.LevelPrefabs[levelBombCategory].GetValue(levelBombID).gameObject.GetComponent<ExplodeOnCollide>().explosionEffectPrefab.gameObject.transform.FindChild("PyroclasticPuff").gameObject.transform.FindChild("PyroclasticPuff").gameObject.GetComponent<ExplosionMat>().ExplosionMaterial.shader.name.ToString());
                GUI.Box(UIrect, CB.boltObject.gameObject.name.ToString());
                //GUI.color = new Color(1, 1, 1, 1);

            }
        }
        */
    }
}
