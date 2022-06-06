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
    public class RibbonTrail : MonoBehaviour
    {
        public ParticleSystem PSribbon;
        public LineRenderer ribbonRenderer;
        public ParticleSystem.Particle[] particles;
        public float shakiness = 0f;
        public float shakeSpeed = 1f;
        private float shakeCount = 0f;

        private Rect UIrect = new Rect(0, 100, 512, 512);
        public void Start()
        {
            try
            {
                PSribbon = base.GetComponent<ParticleSystem>();
                ribbonRenderer = base.GetComponent<LineRenderer>();
            }
            catch { }
        }
        public void Update()
        {
            try
            {
                int PScount = PSribbon.particleCount;
                particles = new ParticleSystem.Particle[PScount];
                PSribbon.GetParticles(particles);
                ribbonRenderer.SetVertexCount(PScount + 1);
                ribbonRenderer.SetPosition(0, base.transform.position);
                for (int i = 0; i < PScount; i++)
                {
                    int a = PSribbon.maxParticles - (int)(particles[i].lifetime / PSribbon.startLifetime * PSribbon.maxParticles);
                    ribbonRenderer.SetPosition(a, particles[i].position);
                }
            }
            catch { }
            try
            {
                shakeCount += Time.deltaTime * shakeSpeed;
                if (shakeCount > 1f)
                    shakeCount -= 1f;
                float xRot = Mathf.Sin(shakeCount * 2 * Mathf.PI) * shakiness * 45f;
                base.transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);
            }
            catch { }
        }
        public void OnEnable()
        {
            shakeCount = 0f;
            try
            {
                ribbonRenderer.SetVertexCount(0);
            }
            catch { }
        }
        /*
        readonly GUIStyle vec3Style = new GUIStyle()
        {
            fontStyle = FontStyle.Bold,
            fontSize = 16,
            normal = { textColor = new Color(1, 1, 1, 1) },
            alignment = TextAnchor.MiddleCenter,
        };
        void OnGUI()
        {
            try
            {
                string showmsg = "";
                for (int i=0;i<particles.Length;i++)
                {
                    int a = (int)(particles[i].lifetime / PSribbon.startLifetime * PSribbon.maxParticles);
                    showmsg += particles[i].lifetime + "," + ((int)PSribbon.maxParticles - a).ToString();
                    showmsg += "\n";
                }
                GUI.Box(UIrect, showmsg.ToString());
            }
            catch { }
        }
        */
    }
    class TrailFXBlock : BlockScript
    {
        public GameObject Vis;
        public GameObject Colliders;
        public GameObject ribbonBody;

        public Vector2 trailTexOffset;

        public MKey Launch;
        public MToggle RGB;
        public MMenu trailMenu;
        public MMenu texMenu;
        public MSlider trailLengthSlider;
        public MSlider trailStartWidthSlider;
        public MSlider trailEndWidthSlider;
        public MSlider trailStartAlphaSlider;
        public MSlider trailEndAlphaSlider;
        public MColourSlider trailStartColorSlider;
        public MColourSlider trailEndColorSlider;

        public MSlider ribbonGravitySlider;
        public MSlider ribbonHardness;
        public MSlider ribbonShakiness;
        public MSlider ribbonShakeSpeed;

        public ParticleSystem PSribbon;
        public ParticleSystemRenderer PSRribbon;
        public LineRenderer ribbonRenderer;
        public Material ribbonMat;
        public RibbonTrail ribbonTrail;
        
        public int MyGUID;
        private bool isheld = false;
        private bool trailActive = false;
        public void InitRibbon()
        {
            if (BlockBehaviour.transform.FindChild("ribbonBody") == null)
            {
                ribbonBody = new GameObject("ribbonBody");
                ribbonBody.SetActive(false);
                ribbonBody.transform.SetParent(BlockBehaviour.transform);
                ribbonBody.transform.localPosition = Vector3.zero;
                ribbonBody.transform.localRotation = Quaternion.identity;
                PSribbon = ribbonBody.GetComponent<ParticleSystem>() ?? ribbonBody.AddComponent<ParticleSystem>();
                PSRribbon = ribbonBody.GetComponent<ParticleSystemRenderer>();
                PSribbon.loop = true;

                ParticleSystem.ShapeModule SM = PSribbon.shape;
                SM.shapeType = ParticleSystemShapeType.Cone;
                SM.radius = 0.01f;
                SM.angle = 0.0f;

                float PSduration = PSribbon.duration;
                PSduration = 1f;
                PSribbon.startDelay = 0f;
                PSribbon.startLifetime = 0.3f;
                PSribbon.startSpeed = 100f;
                PSribbon.startSize = 1f;
                PSribbon.simulationSpace = ParticleSystemSimulationSpace.World;
                PSribbon.gravityModifier = 0f;

                ParticleSystem.CollisionModule CM = PSribbon.collision;
                CM.enabled = false;
                /*
                CM.enableDynamicColliders = true;
                CM.collidesWith = Game.BlockEntityLayerMask;
                CM.dampen = PSribbon.startSpeed;
                */
                PSribbon.scalingMode = ParticleSystemScalingMode.Shape;

                ParticleSystem.LimitVelocityOverLifetimeModule LVOLM = PSribbon.limitVelocityOverLifetime;
                //LVOLM.separateAxes = false;
                //LVOLM.space = ParticleSystemSimulationSpace.Local;
                LVOLM.dampen = 0.1f;
                LVOLM.limit = 0f;
                LVOLM.enabled = true;

                PSRribbon.material = new Material(Shader.Find("Particles/Additive"));
                PSRribbon.material.mainTexture = Texture2D.blackTexture;
                PSRribbon.material.SetColor("_TintColor", new Color(0, 0, 0, 0));

                ParticleSystem.EmissionModule EM = PSribbon.emission;
                EM.enabled = true;
                EM.rate = 100f;
                PSribbon.maxParticles = (int)(100f * PSribbon.startLifetime);

                ribbonRenderer = ribbonBody.GetComponent<LineRenderer>() ?? ribbonBody.AddComponent<LineRenderer>();
                ribbonRenderer.receiveShadows = false;
                ribbonRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                ribbonRenderer.SetWidth(1f, 1f);
                ribbonRenderer.sortingOrder = 10000;

                ribbonMat = new Material(Shader.Find("Particles/Alpha Blended"));
                ribbonMat.mainTexture = Texture2D.whiteTexture;
                ribbonMat.SetColor("_TintColor", new Color(1f, 1f, 1f, 1f));
                //matArray[1].renderQueue -= 1;

                ribbonRenderer.material = ribbonMat;

                ribbonRenderer.SetVertexCount(0);

                ribbonTrail = ribbonBody.GetComponent<RibbonTrail>() ?? ribbonBody.AddComponent<RibbonTrail>();
            }
        }
        public override void SafeAwake()
        {
            Vis = BlockBehaviour.gameObject.transform.FindChild("Vis").gameObject;
            Colliders = BlockBehaviour.gameObject.transform.FindChild("Colliders").gameObject;

            Launch = AddKey("启用", "launch", KeyCode.X);
            RGB = AddToggle("R G B!", "rgb", false);
            trailMenu = AddMenu("trailtype", 0, new List<string> { "飘带", "尾迹"});
            texMenu = AddMenu("trailtex", 0, new List<string> { "纯色", "条纹", "脉冲", "闪电" });
            trailLengthSlider = AddSlider("长度", "traillen", 1f, 0f, 10f);
            trailStartWidthSlider = AddSlider("起始宽度", "trailwidth1", 1f, 0f, 10f);
            trailEndWidthSlider = AddSlider("结束宽度", "trailwidth2", 1f, 0f, 10f);
            trailStartAlphaSlider = AddSlider("起始透明度", "trailalpha1", 1f, 0f, 1f);
            trailEndAlphaSlider = AddSlider("结束透明度", "trailalpha2", 1f, 0f, 1f);
            trailStartColorSlider = AddColourSlider("起始颜色", "color1", new Color(1, 1, 1, 1), false); 
            trailEndColorSlider= AddColourSlider("结束颜色", "color2", new Color(1, 1, 1, 1), false);

            ribbonGravitySlider = AddSlider("飘带重力", "ribbonGravity", 0f, 0f, 2f);
            ribbonHardness = AddSlider("飘带硬度", "ribbonHardness", 0.1f, -1f, 1f);
            ribbonShakiness = AddSlider("飘带抖动幅度", "ribbonShakiness", 0.0f, -1f, 1f);
            ribbonShakeSpeed = AddSlider("飘带抖动速度", "ribbonShakeSpeed", 1f, 0f, 10f);

            InitRibbon();
            try
            {
                trailLengthSlider.ValueChanged += (float value) =>
                {
                    try
                    {
                        PSribbon.startLifetime = Mathf.Max(trailLengthSlider.Value, 0f);
                        PSribbon.maxParticles = (int)(100f * PSribbon.startLifetime);
                        //PSribbon.startSpeed = 100f * Mathf.Max(trailLengthSlider.Value, 0f);
                    }
                    catch { }
                };
                trailStartWidthSlider.ValueChanged += (float value) =>
                {
                    try
                    {
                        ribbonRenderer.SetWidth(Mathf.Max(0f, trailStartWidthSlider.Value), Mathf.Max(0f, trailEndWidthSlider.Value));
                    }
                    catch { }
                };
                trailEndWidthSlider.ValueChanged += (float value) =>
                {
                    try
                    {
                        ribbonRenderer.SetWidth(Mathf.Max(0f, trailStartWidthSlider.Value), Mathf.Max(0f, trailEndWidthSlider.Value));
                    }
                    catch { }
                };
                ribbonHardness.ValueChanged += (float value) =>
                {
                    try
                    {
                        ParticleSystem.LimitVelocityOverLifetimeModule LVOLM = PSribbon.limitVelocityOverLifetime;
                        LVOLM.dampen = ribbonHardness.Value;
                    }
                    catch { }
                };
                ribbonGravitySlider.ValueChanged += (float value) =>
                {
                    try
                    {
                        PSribbon.gravityModifier = ribbonGravitySlider.Value;
                    }
                    catch { }
                };
                ribbonShakiness.ValueChanged += (float value) =>
                {
                    try
                    {
                        ribbonTrail.shakiness = ribbonShakiness.Value;
                    }
                    catch { }
                };
                ribbonShakeSpeed.ValueChanged += (float value) =>
                {
                    try
                    {
                        ribbonTrail.shakeSpeed = ribbonShakeSpeed.Value;
                    }
                    catch { }
                };
            }
            catch { }

            BlockBehaviour.name = "TrailFXBlock";
        }
        public void Start()
        {
            //BlockBehaviour.Rigidbody.drag = 0.0f;
            //BlockBehaviour.Rigidbody.angularDrag = 0.0f;
            trailTexOffset = Vector2.zero;
            try
            {
                BlockBehaviour.blockJoint.breakForce = Mathf.Infinity;
                BlockBehaviour.blockJoint.breakTorque = Mathf.Infinity;
            }
            catch { }
            BlockBehaviour.noRigidbody = false;

            trailActive = false;
            isheld = false;

            try
            {
                Color startColor = trailStartColorSlider.Value;
                Color endColor = trailEndColorSlider.Value;
                if (RGB.IsActive)
                {
                    startColor = RGBController.Instance.outputColor;
                    endColor = RGBController.Instance.outputColor;
                }
                ribbonRenderer = ribbonBody.GetComponent<LineRenderer>();
                PSribbon = ribbonBody.GetComponent<ParticleSystem>();



                startColor.a = Mathf.Max(0f, Mathf.Min(1f, trailStartAlphaSlider.Value));
                endColor.a = Mathf.Max(0f, Mathf.Min(1f, trailEndAlphaSlider.Value));
                ribbonRenderer.SetColors(startColor, endColor);
                ribbonRenderer.material.mainTextureScale = new Vector2(5f * trailLengthSlider.Value, 1f);

                try
                {
                    switch(trailMenu.Value)
                    {
                        case 0:
                            {
                                PSribbon.startSpeed = 100f;
                                break;
                            }
                        case 1:
                            {
                                PSribbon.startSpeed = 0f;
                                break;
                            }
                        default:
                            {
                                PSribbon.startSpeed = 100f;
                                break;
                            }
                    }
                }
                catch { }
                switch (texMenu.Value)
                {
                    case 0:
                        {
                            ribbonRenderer.material.mainTexture = Texture2D.whiteTexture;
                            break;
                        }
                    case 1:
                        {
                            ribbonRenderer.material.mainTexture = ModResource.GetTexture("Trail Strip").Texture;
                            break;
                        }
                    case 2:
                        {
                            ribbonRenderer.material.mainTexture = ModResource.GetTexture("Trail Flash").Texture;
                            break;
                        }
                    case 3:
                        {
                            ribbonRenderer.material.mainTexture = ModResource.GetTexture("Trail Lightning").Texture;
                            break;
                        }
                    default:
                        {
                            ribbonRenderer.material.mainTexture = Texture2D.whiteTexture;
                            break;
                        }
                }
            }
            catch { }


        }
        public override void BuildingUpdate()
        {
            if (BlockBehaviour.Guid.GetHashCode() != 0 && BlockBehaviour.Guid.GetHashCode() != MyGUID)
                MyGUID = BlockBehaviour.Guid.GetHashCode();
        }
        private void Update()
        {
            if (BlockBehaviour.isSimulating)
            {
                if (Launch.IsHeld || Launch.EmulationHeld() || KeymsgController.Instance.keyheld[BlockBehaviour.ParentMachine.PlayerID][MyGUID]) 
                {
                    trailActive = true;
                    ribbonBody.SetActive(true);
                }
                else
                {
                    if (Time.timeScale > 0f)
                    {
                        trailActive = false;
                        ribbonBody.SetActive(false);
                    }
                }
                if (!StatMaster.isClient)
                {
                    if (isheld != trailActive)
                    {
                        isheld = trailActive;
                        ModNetworking.SendToAll(KeymsgController.SendHeld.CreateMessage((int)BlockBehaviour.ParentMachine.PlayerID, (int)MyGUID, (bool)isheld));
                    }
                }
                if (RGB.IsActive)
                {
                    Color startColor = RGBController.Instance.outputColor;
                    Color endColor = RGBController.Instance.outputColor;
                    startColor.a = Mathf.Max(0f, Mathf.Min(1f, trailStartAlphaSlider.Value));
                    endColor.a = Mathf.Max(0f, Mathf.Min(1f, trailEndAlphaSlider.Value));
                    ribbonRenderer.SetColors(startColor, endColor);
                }
                trailTexOffset.x -= 10f * Time.deltaTime;
                if (trailTexOffset.x < -2f)
                    trailTexOffset.x += 2f;
                ribbonRenderer.material.mainTextureOffset = trailTexOffset;
            }
        }
        public override void OnSimulateStart()
        {
            Vis.SetActive(false);
            Colliders.SetActive(false);
            KeymsgController.Instance.keyheld[BlockBehaviour.ParentMachine.PlayerID].Add(MyGUID, false);
        }
        public override void OnSimulateStop()
        {
            KeymsgController.Instance.keyheld[BlockBehaviour.ParentMachine.PlayerID].Remove(MyGUID);
            trailActive = false;
            isheld = false;
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
