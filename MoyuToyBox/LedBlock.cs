using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Collections;

using Modding.Modules;
using Modding;
using Modding.Blocks;
using UnityEngine;

namespace MoyuToyBox
{
    class LedBlock : BlockScript
    {
        public GameObject Vis;
        public GameObject colliders;
        public MeshFilter meshFilter;
        public MeshRenderer renderer;
        public Material[] matArray = new Material[2];
        public Vector2 streamOffSet;

        public MMenu root;
        public MSlider posx;
        public MSlider posy;
        public MSlider posz;
        public MSlider rotx;
        public MSlider roty;
        public MSlider rotz;
        public MSlider scalex;
        public MSlider scaley;
        public MSlider scalez;

        public MMenu meshType;
        public MToggle hilight;
        public MToggle streamlight;
        public MToggle RGB;
        public MToggle nocol;
        public MSlider streamDisplaySpeed;
        public MColourSlider colourSlider;
        public MColourSlider streamColourSlider;

        public bool myClusterCode = false;
        public void initTransformSliders()
        {
            posx = AddSlider("Pos.x", "posx", 0f, -10f, 10f);
            posx.ValueChanged += (float value) =>
            {
                Vis.transform.localPosition = new Vector3(posx.Value, posy.Value, posz.Value);
                colliders.transform.localPosition = Vis.transform.localPosition;
            };
            posy = AddSlider("Pos.y", "posy", 0f, -10f, 10f);
            posy.ValueChanged += (float value) =>
            {
                Vis.transform.localPosition = new Vector3(posx.Value, posy.Value, posz.Value);
                colliders.transform.localPosition = Vis.transform.localPosition;
            };
            posz = AddSlider("Pos.z", "posz", 0f, -10f, 10f);
            posz.ValueChanged += (float value) =>
            {
                Vis.transform.localPosition = new Vector3(posx.Value, posy.Value, posz.Value);
                colliders.transform.localPosition = Vis.transform.localPosition;
            };
            rotx = AddSlider("Rot.x", "rotx", 0f, 0f, 360f);
            rotx.ValueChanged += (float value) =>
            {
                Vis.transform.localRotation = Quaternion.Euler(new Vector3(rotx.Value, roty.Value, rotz.Value));
            };
            roty = AddSlider("Rot.y", "roty", 0f, 0f, 360f);
            roty.ValueChanged += (float value) =>
            {
                Vis.transform.localRotation = Quaternion.Euler(new Vector3(rotx.Value, roty.Value, rotz.Value));

            };
            rotz = AddSlider("Rot.z", "rotz", 0f, 0f, 360f);
            rotz.ValueChanged += (float value) =>
            {
                Vis.transform.localRotation = Quaternion.Euler(new Vector3(rotx.Value, roty.Value, rotz.Value));

            };
            scalex = AddSlider("Scale.x", "scalex", 1f, 0f, 10f);
            scalex.ValueChanged += (float value) =>
            {
                Vis.transform.localScale = new Vector3(scalex.Value, scaley.Value, scalez.Value);
                colliders.transform.localScale = Vis.transform.localScale;

            };
            scaley = AddSlider("Scale.y", "scaley", 1f, 0f, 10f);
            scaley.ValueChanged += (float value) =>
            {
                Vis.transform.localScale = new Vector3(scalex.Value, scaley.Value, scalez.Value);
                colliders.transform.localScale = Vis.transform.localScale;
            };
            scalez = AddSlider("Scale.z", "scalez", 1f, 0f, 10f);
            scalez.ValueChanged += (float value) =>
            {
                Vis.transform.localScale = new Vector3(scalex.Value, scaley.Value, scalez.Value);
                colliders.transform.localScale = Vis.transform.localScale;
            };

        }
        public void displayTransformSliders(bool active)
        {
            posx.DisplayInMapper = active;
            posy.DisplayInMapper = active;
            posz.DisplayInMapper = active;
            rotx.DisplayInMapper = active;
            roty.DisplayInMapper = active;
            rotz.DisplayInMapper = active;
            scalex.DisplayInMapper = active;
            scaley.DisplayInMapper = active;
            scalez.DisplayInMapper = active;
        }
        public void initRenderToggles()
        {
            meshType = AddMenu("meshtype", 0, new List<string>() { "Box", "Circle", "halfcircle", "quartercircle" });
            meshType.ValueChanged += (int value) =>
            {
                repairMesh();
            };
            hilight = AddToggle("高光", "hilight", false);
            hilight.Toggled += (bool value) =>
            {
                repairMat();
            };

            streamlight = AddToggle("流光", "streamlight", false);
            streamlight.Toggled += (bool value) =>
            {
                repairMat();
            };
            RGB = AddToggle("R G B!", "rgb", false);
            nocol = AddToggle("禁用碰撞", "nocol", false);

            streamDisplaySpeed = AddSlider("流光速度", "streamSpeed", 1f, 0f, 10f);

            colourSlider = AddColourSlider("主体颜色", "color", new Color(1, 1, 1, 1), false);
            colourSlider.ValueChanged += (Color color) =>
            {
                matArray[0].SetColor("_Color", colourSlider.Value);
                matArray[0].SetColor("_EmissCol", colourSlider.Value);
                renderer.materials = matArray;
            };
            streamColourSlider = AddColourSlider("流光颜色", "streamcolor", new Color(1, 1, 1, 1), false);
            streamColourSlider.ValueChanged += (Color color) =>
            {
                matArray[1].SetColor("_TintColor", streamColourSlider.Value);
                renderer.materials = matArray;
            };
        }
        public void displayRenderToggles(bool active)
        {
            meshType.DisplayInMapper = active;
            hilight.DisplayInMapper = active;
            streamlight.DisplayInMapper = active;
            RGB.DisplayInMapper = active;
            nocol.DisplayInMapper = active;
            streamDisplaySpeed.DisplayInMapper = active;
            colourSlider.DisplayInMapper = active;
            streamColourSlider.DisplayInMapper = active;

        }
        public void initMat()
        {
            matArray[0] = renderer.material;

            matArray[1] = new Material(Shader.Find("Particles/Additive"));
            matArray[1].mainTexture = ModResource.GetTexture("streamlight").Texture;
            matArray[1].SetColor("_TintColor", Color.black);
            renderer.materials = matArray;
        }
        public void repairMat()
        {
            try
            {
                if (hilight.IsActive)
                {
                    matArray[0].SetTexture("_EmissMap", Texture2D.whiteTexture);
                    matArray[0].SetTexture("_MainTex", Texture2D.blackTexture);
                }
                else
                {
                    matArray[0].SetTexture("_MainTex", Texture2D.whiteTexture);
                    matArray[0].SetTexture("_EmissMap", Texture2D.blackTexture);
                }
                matArray[0].SetColor("_Color", colourSlider.Value);
                matArray[0].SetColor("_EmissCol", colourSlider.Value);
                if (streamlight.IsActive)
                    matArray[1].SetColor("_TintColor", streamColourSlider.Value);
                else
                    matArray[1].SetColor("_TintColor", Color.black);

                streamOffSet = Vector2.zero;
            }
            catch { }
            renderer.materials = matArray;
        }
        public void repairMesh()
        {
            try
            {
                switch (meshType.Value)
                {
                    case 0:
                        {
                            meshFilter.mesh = ModResource.GetMesh("Box").Mesh;
                            break;
                        }
                    case 1:
                        {
                            meshFilter.mesh = ModResource.GetMesh("circle").Mesh;
                            break;
                        }
                    case 2:
                        {
                            meshFilter.mesh = ModResource.GetMesh("halfcircle").Mesh;
                            break;
                        }
                    case 3:
                        {
                            meshFilter.mesh = ModResource.GetMesh("quartercircle").Mesh;
                            break;
                        }
                    default:
                        {
                            meshFilter.mesh = ModResource.GetMesh("Box").Mesh;
                            break;
                        }
                }
            }
            catch { }
        }
        public override void SafeAwake()
        {
            Vis = BlockBehaviour.gameObject.transform.FindChild("Vis").gameObject;
            colliders = BlockBehaviour.gameObject.transform.FindChild("Colliders").gameObject;
            meshFilter = Vis.GetComponent<MeshFilter>();
            renderer = Vis.GetComponent<MeshRenderer>();
            initMat();
            initTransformSliders();
            initRenderToggles();
            root = AddMenu("rootmenu", 0, new List<string> { "renderer", "transform" });

            root.ValueChanged += (int value) =>
            {
                if (root.Value == 1)
                {
                    displayRenderToggles(false);
                    displayTransformSliders(true);
                }
                else if (root.Value == 0)
                {
                    displayTransformSliders(false);
                    displayRenderToggles(true);
                }
            };

            BlockBehaviour.name = "Led Block";
        }
        public override void BuildingUpdate()
        {

        }
        public void Start()
        {
            //BlockBehaviour.Rigidbody.drag = 0.0f;
            //BlockBehaviour.Rigidbody.angularDrag = 0.0f;
            try
            {
                BlockBehaviour.blockJoint.breakForce = Mathf.Infinity;
                BlockBehaviour.blockJoint.breakTorque = Mathf.Infinity;
            }
            catch { }
            BlockBehaviour.noRigidbody = false;
            repairMat();
            repairMesh();

        }
        public void Update()
        {
            if (BlockBehaviour.isSimulating)
            {
                if (RGB.IsActive)
                {
                    if(hilight.IsActive)
                    {
                        matArray[0].SetColor("_EmissCol", RGBController.Instance.outputColor); 
                        
                    }
                }
            }
            if(streamlight.IsActive)
            {
                streamOffSet.x -= streamDisplaySpeed.Value * Time.unscaledDeltaTime;
                if (streamOffSet.x < -2f)
                    streamOffSet.x += 2f;
                matArray[1].mainTextureOffset = streamOffSet;
            }
            if (StatMaster.clusterCoded) 
                myClusterCode = true;
            else
            {
                if (myClusterCode)
                {
                    repairMat();
                    repairMesh();
                }
                myClusterCode = false;
            }
        }
        public override void OnSimulateStart()
        {
            if (nocol.IsActive)
                try
                {
                    colliders.SetActive(false);
                }
                catch { }
        }
        public override void OnSimulateStop()
        {
            colliders.SetActive(true);
        }
        public override void SimulateFixedUpdateHost()
        {
            if (!BlockBehaviour.noRigidbody)
            {
                try
                {
                    BlockBehaviour.transform.SetParent(BlockBehaviour.blockJoint.connectedBody.transform);
                    Destroy(BlockBehaviour.blockJoint);
                    Destroy(BlockBehaviour.Rigidbody);
                    BlockBehaviour.noRigidbody = true;
                }
                catch { }
            }
        }
    }
}
