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
    class EnergyShield : BlockScript
    {
        public GameObject SHanchor;
        public GameObject Colliders;
        public GameObject Vis;
        public GameObject Shieldbody;
        public MeshRenderer VisRenderer;
        public MaterialPropertyBlock VisPropertyBlock = new MaterialPropertyBlock();

        public MeshFilter SHFilter;
        public MeshCollider SHcol;
        public MeshRenderer SHrenderer;

        public MKey Launch;
        public MMenu SHtype;
        public MToggle RGB;
        public MToggle UIactive;
        public MSlider massSlider;
        public MSlider healthSlider;
        public MSlider rechargeSpeed;
        public MSlider rechargeCD;
        public MSlider SHlength;
        public MSlider SHwidth;
        public MSlider SHheight;
        public MSlider SHdis;
        public MSlider streamSpeed;
        public MSlider streamCount;
        public MColourSlider BaseColor;
        public MColourSlider SHcolor;
        
        public float startHealth;
        public float updateRate = 0.25f;
        public float DamageAmount = 0f;
        public bool anchorSet = false;
        public bool isRecharging = false;
        public bool canUpdate = false;
        private Rect UIrect = new Rect(0, 100, 512, 512);
        public Material[] matArrayLauncher = new Material[2];
        public Color LauncherColorDif { get { return matArrayLauncher[0].color; } set { matArrayLauncher[0].SetColor("_Color", value); } }
        public Color LauncherColor { get { return matArrayLauncher[1].color; } set { matArrayLauncher[1].SetColor("_TintColor", value); } }


        public int MyGUID;
        private bool isheld = false;
        private bool shieldActive = false;
        private bool myClusterCode = false;

        public void initBase()
        {
            VisRenderer = BlockBehaviour.GetComponentInChildren<MeshRenderer>();
            matArrayLauncher[0] = VisRenderer.material;
            matArrayLauncher[0].mainTexture = ModResource.GetTexture("Shield Base Texture").Texture;
            matArrayLauncher[0].SetColor("_Color", new Color(1, 1, 1, 1));
            matArrayLauncher[1] = new Material(Shader.Find("Particles/Additive"));
            matArrayLauncher[1].mainTexture = ModResource.GetTexture("Shield Base Light").Texture;
            matArrayLauncher[1].SetColor("_TintColor", new Color(1, 1, 1, 1));
            VisRenderer.materials = matArrayLauncher;

        }
        public void initAnchor()
        {
            try
            {
                Destroy(SHanchor);
            }
            catch { }
            SHanchor = new GameObject("sheild anchor");
            SHanchor.SetActive(false);
            SHanchor.transform.SetParent(BlockBehaviour.transform);
            SHanchor.transform.localPosition = Vector3.zero;
            SHanchor.transform.localRotation = Quaternion.identity;
            SHanchor.transform.localScale = Vector3.one;
        }
        public void initShield()
        {
            if (BlockBehaviour.transform.FindChild("Shieldbody") == null) 
            {
                Shieldbody = new GameObject("Shieldbody");
                Shieldbody.SetActive(false);
                Shieldbody.transform.SetParent(BlockBehaviour.transform);
                Shieldbody.transform.localPosition = Vector3.zero;
                Shieldbody.transform.localScale = Vector3.one;
                Shieldbody.transform.localRotation = Quaternion.identity;

                SHFilter = Shieldbody.GetComponent<MeshFilter>() ?? Shieldbody.AddComponent<MeshFilter>();
                SHFilter.mesh = ModResource.GetMesh("Box Shield").Mesh;

                SHcol = Shieldbody.GetComponent<MeshCollider>() ?? Shieldbody.AddComponent<MeshCollider>();
                SHcol.sharedMesh = ModResource.GetMesh("Box Shield Col").Mesh;
                SHcol.convex = true;
                SHcol.isTrigger = false;

                SHrenderer = Shieldbody.GetComponent<MeshRenderer>() ?? Shieldbody.AddComponent<MeshRenderer>();
                SHrenderer.material = new Material(Shader.Find("Particles/Alpha Blended"));
                SHrenderer.material.mainTexture = ModResource.GetTexture("Quad Shield Texture").Texture;
                SHrenderer.material.SetColor("_TintColor", new Color(1f, 1f, 1f, 1f));
            }
        }
        public override void SafeAwake()
        {
            Vis = BlockBehaviour.gameObject.transform.FindChild("Vis").gameObject;
            Colliders = BlockBehaviour.gameObject.transform.FindChild("Colliders").gameObject;
            VisRenderer = Vis.GetComponent<MeshRenderer>();

            Launch = AddKey("启用", "launch", KeyCode.X);
            SHtype = AddMenu("shieldtype", 0, new List<string> { "方形", "六边形", "六边形2", "八边形", "半球" });
            RGB = AddToggle("R G B!", "rgb", false);
            UIactive = AddToggle("UI显示", "ui", true);
            massSlider = AddSlider("质量", "mass", 0.0f, 0.0f, 100.0f);
            healthSlider = AddSlider("盾量", "health", 10.0f, 10.0f, 200f);
            rechargeSpeed = AddSlider("充能速率", "rechargeSpeed", 1.0f, 0.0f, 10.0f);
            rechargeCD = AddSlider("重启CD", "rechargeCD", 1.0f, 0.0f, 10.0f);
            SHwidth = AddSlider("盾宽", "width", 4.0f, 0.0f, 10f);
            SHlength = AddSlider("盾长", "length", 4.0f, 0.0f, 10f);
            SHheight =AddSlider("盾厚", "height", 4.0f, 0.0f, 10f);
            SHdis = AddSlider("生成距离", "dis", 2.0f, 0.0f, 10f);
            streamSpeed = AddSlider("流光速度", "streamSpeed", 1.0f, 0.0f, 10f);
            streamCount = AddSlider("流光层数", "streamCount", 1.0f, 1.0f, 10f);
            BaseColor = AddColourSlider("底座颜色", "basecolor", new Color(1f, 1f, 1f, 1f), false);
            SHcolor = AddColourSlider("护盾颜色", "color", new Color(0f, 1f, 1f, 1f), false);

            initBase();
            initAnchor();
            initShield();
            SHtype.ValueChanged += (int value) =>
            {
                //try
                {
                    switch (SHtype.Value)
                    {
                        case 0:
                            {
                                SHFilter.mesh = ModResource.GetMesh("Box Shield").Mesh;
                                SHcol.sharedMesh = ModResource.GetMesh("Box Shield Col").Mesh;
                                break;
                            }
                        case 1:
                            {
                                SHFilter.mesh = ModResource.GetMesh("Hex Shield").Mesh;
                                SHcol.sharedMesh = ModResource.GetMesh("Hex Shield Col").Mesh;
                                break;
                            }
                        case 2:
                            {
                                SHFilter.mesh = ModResource.GetMesh("Hex Shield2").Mesh;
                                SHcol.sharedMesh = ModResource.GetMesh("Hex Shield2 Col").Mesh;
                                break;
                            }
                        case 3:
                            {
                                SHFilter.mesh = ModResource.GetMesh("Oct Shield").Mesh;
                                SHcol.sharedMesh = ModResource.GetMesh("Oct Shield Col").Mesh;
                                break;
                            }
                        case 4:
                            {
                                SHFilter.mesh = ModResource.GetMesh("Half Ball Shield").Mesh;
                                SHcol.sharedMesh = ModResource.GetMesh("Half Ball Shield").Mesh;
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
                //catch { }
            };
            try
            {
                
                SHwidth.ValueChanged += (float value) =>
                {
                    try
                    {
                        Shieldbody.transform.localScale = new Vector3(SHlength.Value, SHwidth.Value, SHheight.Value);
                    }
                    catch { }
                };
                SHlength.ValueChanged += (float value) =>
                {
                    try
                    {
                        Shieldbody.transform.localScale = new Vector3(SHlength.Value, SHwidth.Value, SHheight.Value);
                    }
                    catch { }
                };
                SHheight.ValueChanged += (float value) =>
                {
                    try
                    {
                        Shieldbody.transform.localScale = new Vector3(SHlength.Value, SHwidth.Value, SHheight.Value);
                    }
                    catch { }
                };
                SHdis.ValueChanged += (float value) =>
                {
                    try
                    {
                        Shieldbody.transform.localPosition = new Vector3(0f, 0f, Mathf.Max(0f, SHdis.Value));
                    }
                    catch { }
                };
                streamCount.ValueChanged += (float value) =>
                {
                    try
                    {
                        //Color thisColor = SHcolor.Value;
                        //thisColor.a = 0.2f;
                        SHrenderer.material.mainTextureScale = new Vector2(Mathf.Max(1f, streamCount.Value), 1f);
                    }
                    catch { }
                };
                SHcolor.ValueChanged += (Color color) =>
                {
                    try
                    {
                        //Color thisColor = SHcolor.Value;
                        //thisColor.a = 0.2f;
                        SHrenderer.material.SetColor("_TintColor", SHcolor.Value);
                        LauncherColor = SHcolor.Value;
                        VisRenderer.materials[1].SetColor("_TintColor", SHcolor.Value);
                    }
                    catch { }
                };
                BaseColor.ValueChanged += (Color color) =>
                {
                    try
                    {
                        //Color thisColor = SHcolor.Value;
                        //thisColor.a = 0.2f;
                        LauncherColorDif = BaseColor.Value;
                        VisRenderer.materials[0].SetColor("_Color", BaseColor.Value);
                    }
                    catch { }
                };
            }
            catch { }
            base.gameObject.name = "EnergyShield";
        }
        public void Start()
        {
            try
            {
                BlockBehaviour.blockJoint.breakForce = 100000f;
                BlockBehaviour.blockJoint.breakTorque = 100000f;
            }
            catch { }

            Shieldbody.transform.localPosition = new Vector3(0f, 0f, Mathf.Max(0f, SHdis.Value));
            Shieldbody.transform.localScale = new Vector3(SHlength.Value, SHwidth.Value, SHheight.Value);
            //Color thisColor = SHcolor.Value;
            //thisColor.a = 0.2f;
            SHrenderer.material.SetColor("_TintColor", SHcolor.Value);
            VisRenderer.GetPropertyBlock(VisPropertyBlock);

            Shieldbody.SetActive(false);

            anchorSet = false;
            isRecharging = false;
            canUpdate = true;
            startHealth = Mathf.Max(10f, healthSlider.Value);
            BlockBehaviour.BlockHealth.health = startHealth;
            BlockBehaviour.BlockHealth.Init(BlockBehaviour.ParentMachine, BlockBehaviour);
            BlockBehaviour.Rigidbody.mass = massSlider.Value;

            shieldActive = false;
            isheld = false;

            if (RGB.IsActive)
            {
                LauncherColor = RGBController.Instance.outputColor;
            }
            else
                LauncherColor = SHcolor.Value;
            LauncherColorDif = BaseColor.Value;
            VisRenderer.materials = matArrayLauncher;
        }
        public override void OnSimulateStart()
        {
            KeymsgController.Instance.keyheld[BlockBehaviour.ParentMachine.PlayerID].Add(MyGUID, false);
        }
        public override void OnSimulateStop()
        {
            KeymsgController.Instance.keyheld[BlockBehaviour.ParentMachine.PlayerID].Remove(MyGUID);
            shieldActive = false;
            isheld = false;
        }
        private IEnumerator updateShield()
        {
            canUpdate = false;
            if (BlockBehaviour.BlockHealth.health < startHealth)
            {
                if (BlockBehaviour.BlockHealth.health > 0f)
                {
                    float healing = Mathf.Max(0.1f, rechargeSpeed.Value);
                    if (BlockBehaviour.BlockHealth.health + healing > startHealth)
                        healing = startHealth - BlockBehaviour.BlockHealth.health;
                    BlockBehaviour.BlockHealth.DamageBlock(-healing);
                }
                else if (!isRecharging)
                    StartCoroutine(recharge());
            }
            yield return new WaitForSeconds(updateRate);
            canUpdate = true;
            yield break;
        }
        private IEnumerator recharge()
        {
            isRecharging = true;
            Shieldbody.SetActive(false);
            yield return new WaitForSeconds(Mathf.Max(0.1f, rechargeCD.Value));

            BlockBehaviour.BlockHealth.health = startHealth;
            BlockBehaviour.BlockHealth.Init(BlockBehaviour.ParentMachine, BlockBehaviour);
            BlockBehaviour.BlockHealth.DamageBlock(0f);
            isRecharging = false;
            Shieldbody.SetActive(true);
            yield break;
        }
        public void Update()
        {
            Vector2 thisOffset = SHrenderer.material.mainTextureOffset;
            thisOffset.x -= streamSpeed.Value * Time.deltaTime;
            if (thisOffset.x < 0f)
                thisOffset.x += 1f;
            SHrenderer.material.mainTextureOffset = thisOffset;
            if (BlockBehaviour.isSimulating)
            {
                if (!StatMaster.isClient)
                {
                    if (canUpdate)
                        StartCoroutine(updateShield());
                    if (anchorSet)
                    {
                        BlockBehaviour.Rigidbody.velocity = Vector3.zero;
                        BlockBehaviour.transform.position = SHanchor.transform.position;
                        BlockBehaviour.transform.rotation = SHanchor.transform.rotation;
                    }
                }
                if ((Launch.IsHeld || Launch.EmulationHeld() || KeymsgController.Instance.keyheld[BlockBehaviour.ParentMachine.PlayerID][MyGUID]) && !isRecharging) 
                {
                    shieldActive = true;
                    Shieldbody.SetActive(true);
                }
                else
                {
                    if (Time.timeScale > 0f)
                    {
                        shieldActive = false;
                        Shieldbody.SetActive(false);
                    }
                }
                if (!StatMaster.isClient)
                {
                    if (isheld != shieldActive)
                    {
                        isheld = shieldActive;
                        ModNetworking.SendToAll(KeymsgController.SendHeld.CreateMessage((int)BlockBehaviour.ParentMachine.PlayerID, (int)MyGUID, (bool)isheld));
                    }
                }
                VisRenderer.GetPropertyBlock(VisPropertyBlock);
                DamageAmount = VisPropertyBlock.GetFloat("_DamageAmount");
                Color thisColor = SHcolor.Value;
                if (RGB.IsActive)
                {
                    LauncherColor = RGBController.Instance.outputColor;
                    thisColor = RGBController.Instance.outputColor;
                }
                thisColor.a = 1f - DamageAmount;
                SHrenderer.material.SetColor("_TintColor", thisColor);
            }
            if (StatMaster.clusterCoded)
                myClusterCode = true;
            else
            {
                if (myClusterCode)
                {
                    VisRenderer.materials = matArrayLauncher;
                }
                myClusterCode = false;
            }
        }
        public override void SimulateFixedUpdateHost()
        {
            if (!anchorSet)
            {
                try
                {
                    SHanchor.transform.SetParent(BlockBehaviour.blockJoint.connectedBody.transform);
                    Destroy(BlockBehaviour.blockJoint);
                    Colliders.SetActive(false);
                    BlockBehaviour.Rigidbody.isKinematic = true;
                    anchorSet = true;
                }
                catch { }
            }
        }
        public override void BuildingUpdate()
        {
            if (VisRenderer.materials.Length < 2)
                VisRenderer.materials = matArrayLauncher;
            if (BlockBehaviour.Guid.GetHashCode() != 0 && BlockBehaviour.Guid.GetHashCode() != MyGUID)
                MyGUID = BlockBehaviour.Guid.GetHashCode();
        }

        readonly GUIStyle vec3Style = new GUIStyle()
        {
            fontStyle = FontStyle.Bold,
            fontSize = 16,
            normal = { textColor = new Color(1, 1, 1, 1) },
            alignment = TextAnchor.MiddleCenter,
        };
        void OnGUI()
        {
            //if (cameraController.activeCamera)
            //if (!isFirstFrame || BlockBehaviour.isSimulating)
            {            //Vector3 a = cameraController.activeCamera.GetComponentInParent<BlockBehaviour>().GetTransform().forward;

                //GUI.Box(UIrect, PrefabMaster.LevelPrefabs[levelBombCategory].GetValue(levelBombID).gameObject.GetComponent<ExplodeOnCollide>().explosionEffectPrefab.gameObject.transform.FindChild("PyroclasticPuff").gameObject.transform.FindChild("PyroclasticPuff").gameObject.GetComponent<ExplosionMat>().ExplosionMaterial.shader.name.ToString());
                //GUI.Box(UIrect, BlockBehaviour.gameObject.layer.ToString()+","+ torpedo.layer.ToString());
                //GUI.color = new Color(1, 1, 1, 1);

            }
            try
            {
                //string showmsg = BlockBehaviour.BlockHealth.health.ToString() + "\n";
                //VisRenderer.GetPropertyBlock(VisPropertyBlock);
                //showmsg += VisPropertyBlock.GetFloat("_DamageAmount").ToString();
                //showmsg += texttest.Value; 
                //GUI.Box(UIrect, SHcol.contactOffset.ToString());
                if(UIactive.IsActive && BlockBehaviour.isSimulating)
                {
                    Vector3 onScreenPosition = Camera.main.WorldToScreenPoint(Shieldbody.transform.position);
                    if (onScreenPosition.z >= 0)
                    {
                        string showmsg = ((int)Mathf.Lerp(startHealth, 0f, DamageAmount)).ToString() + "/" + startHealth.ToString();
                        GUI.Box(new Rect(onScreenPosition.x - 96 / 2, Camera.main.pixelHeight - onScreenPosition.y - 24 / 2, 96, 24), showmsg);
                    }
                }
            }
            catch { }
        }
        
    }
}
