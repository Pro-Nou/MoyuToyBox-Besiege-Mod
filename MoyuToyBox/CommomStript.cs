using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using Modding;
using Modding.Blocks;

namespace MoyuToyBox
{
    class CommomStript: MonoBehaviour
    {
        public BlockBehaviour BB { get; internal set; }
        public Collider[] myColliders;

        private Rect UIrect = new Rect(0, 500, 512, 128);
        private void Awake()
        {

            BB = GetComponent<BlockBehaviour>();

            SafeAwake();

            if (BB.isSimulating) { return; }


            //Enhancement.Toggled += (bool value) => {  PropertiseChangedEvent();};

            //LoadConfiguration();    

            //PropertiseChangedEvent += ChangedProperties;
            //PropertiseChangedEvent += () => { DisplayInMapper(Enhancement.IsActive); };
            //PropertiseChangedEvent?.Invoke();

            //Controller.Instance.OnSave += SaveConfiguration;
        }
        public void Start()
        {

        }
        public virtual void SafeAwake()
        {
            
        }
        public void Update()
        {
            if (StatMaster.isClient)
            {
                if (myColliders.Length == 0&&BB.isSimulating)
                {
                    myColliders = BB.gameObject.GetComponentsInChildren<Collider>();

                }
                foreach (var a in myColliders)
                    a.enabled = true;
            }
        }
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

                //GUI.Box(UIrect, BB.gameObject.transform.GetChild(0).name.ToString());
                
                if (StatMaster.isClient)
                {
                    string outstr= "";
                    foreach (var a in myColliders)
                        outstr += a.enabled;
                    GUI.Box(UIrect, outstr);
                }

                //GUI.color = new Color(1, 1, 1, 1);

            }
        }
    }
}
