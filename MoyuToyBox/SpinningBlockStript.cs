using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using Modding;
using Modding.Blocks;

namespace MoyuToyBox
{
    class SpinningBlockStript : MonoBehaviour
    {
        public BlockBehaviour BB { get; internal set; }

        private Rect UIrect = new Rect(0, 100, 512, 128);
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
        public virtual void SafeAwake()
        {
            BB.name = "Changed SpinningBlock";
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

                try
                {
                    GUI.Box(UIrect, BB.blockJoint.breakForce.ToString());
                }
                catch { }
                //GUI.color = new Color(1, 1, 1, 1);

            }
        }
    }
}
