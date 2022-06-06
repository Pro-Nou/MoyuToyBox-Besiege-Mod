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
    class BeamRifleController : SingleInstance<BeamRifleController>
    {
        public override string Name { get; } = "BeamRifle Controller";
        public int BeamCount = 0;
        public Dictionary<int, List<KeyValuePair<int,float>>>[] BD = new Dictionary<int, List<KeyValuePair<int, float>>>[16];
        public Dictionary<int, bool> BE = new Dictionary<int, bool>();
        public Dictionary<int, int>[] SD = new Dictionary<int, int>[16]; 
        public BeamRifleController()
        {
            BeamCount = 0;
            for (int i = 0; i < 16; i++)
            {
                SD[i] = new Dictionary<int, int>();
                BD[i] = new Dictionary<int, List<KeyValuePair<int, float>>>();
            }
        }
        public void FixedUpdate()
        {
            if (!StatMaster.levelSimulating)
                BeamCount = 0;
        }
        public static void ShotgunLaunchedEvent(Message message)
        {
            if (StatMaster.isClient)
            {
                try
                {
                    Instance.SD[(int)message.GetData(1)][(int)message.GetData(0)] += 1;
                }
                catch { }
            }
        }
        public static void BeamLaunchedEvent(Message message)
        {
            if (StatMaster.isClient)
            {
                Instance.BD[(int)message.GetData(2)][(int)message.GetData(0)].Add(new KeyValuePair<int, float>((int)message.GetData(1),(float)message.GetData(3)));
            }
        }
        public static void BeamExplodeEvent(Message message)
        {
            if (StatMaster.isClient)
            {
                Instance.BE.Add((int)message.GetData(0), (bool)message.GetData(1));
            }
        }
    }
    class ShotgunCollisionHit : MonoBehaviour
    {
        public float charge = 1f;
        private float blockDamage = 1.0f;
        private float forceDamage = 10000f;
        public MPTeam team = MPTeam.None;
        public bool IFF = true;
        public ushort PlayerID = 0;
        void Start()
        {
        }
        void Update()
        {
        }
        void OnTriggerEnter(Collider col)
        {
            try
            {
                if (col.isTrigger)
                    return;
                if (col.transform.position == this.transform.position)
                    return;
                Ray rayH = new Ray(this.transform.position, col.transform.position - this.transform.position);
                float raylength = Vector3.Distance(this.transform.position, col.transform.position);
                RaycastHit hit;
                Physics.Raycast(rayH, out hit, raylength, Game.BlockEntityLayerMask, QueryTriggerInteraction.Ignore);

                if (hit.collider.GetInstanceID() != col.GetInstanceID())
                    return;

                try
                {
                    BlockBehaviour hitedBlock = col.attachedRigidbody.gameObject.GetComponent<BlockBehaviour>();
                    try
                    {
                        if (!IFF)
                        {
                            if (hitedBlock.Team == team)
                            {
                                if (hitedBlock.Team != MPTeam.None)
                                    return;
                                else if (hitedBlock.ParentMachine.PlayerID == PlayerID)
                                    return;
                            }
                        }
                    }
                    catch { }
                    try
                    {
                        if (hitedBlock.BlockID != 23)
                            hitedBlock.BlockHealth.DamageBlock(blockDamage);
                    }
                    catch { }
                    try
                    {
                        if (hitedBlock.BlockID == 11)
                        {
                            if (hitedBlock.blockJoint.breakForce > 50000f)
                            {
                                hitedBlock.blockJoint.breakForce = 50000f;
                            }
                            else
                            {
                                hitedBlock.blockJoint.breakForce -= forceDamage * charge;
                                if (hitedBlock.blockJoint.breakForce < 0)
                                    hitedBlock.blockJoint.breakForce = 0;
                            }
                        }
                    }
                    catch { }
                }
                catch { }
                try
                {
                    try
                    {
                        col.attachedRigidbody.AddExplosionForce(charge * forceDamage, this.transform.position, this.transform.localScale.x * 5f);
                        //Debug.Log(col.gameObject.name);
                    }
                    catch { }
                }
                catch { }
            }
            catch { }
        }
    }
    class BeamRifle : BlockScript
    {
        public int countlimit = 300;

        public float charge = 1f;
        private float blockDamage = 1.0f;
        private float forceDamage = 5000f;
        private float power = 10000f;

        public bool ShootEnabled = true;
        public float ShootCD = 0.2f;
        public float existTime = 5f;
        public float speed = 200f;
        public MSlider safetyDelay;
        public MSlider ShootCDS;
        public MSlider chargeS;
        public MSlider speedS;
        public MSlider BeamWidth;
        public MSlider BeamLength;
        public MSlider exploDis;
        public MMenu BeamType;
        private float MinCD = 0.1f;
        private float MaxCD = 10.0f;
        private float MinCharge = 0f;
        private float MaxCharge = 30f;
        private float MinLen = 0.2f;
        private float MaxLen = 10.0f;
        private float MinWidth = 0.2f;
        private float MaxWidth = 10.0f;
        private float PSBaseSpeed = -230f;
        private float PSBaseRate = 500f;
        private float PSBaseSize = 3f;

        public GameObject BeamBullet;
        public GameObject BeamExplo;
        public GameObject shotgunCollider;
        public GameObject gunFireBody;
        public GameObject gunFireEffect;
        public MeshCollider shotgunScan;
        public ShotgunCollisionHit shotgunHit;
        public MKey launch;
        public MColourSlider colourSliderDif;
        public MColourSlider colourSlider;
        public MColourSlider colourSlider1;
        private MToggle RGB;
        private MToggle IFF;
        
        public List<GameObject> n = new List<GameObject>();

        private MeshRenderer renderer1;
        private MeshRenderer renderer2;
        public Material[] matArray = new Material[2];
        public Material[] matArrayLauncher = new Material[2];
        public Material PSmat;
        public Color BeamColor { get { return renderer1.material.GetColor("_TintColor"); } set { renderer1.material.SetColor("_TintColor", value); } }
        public Color BeamColor2 { get { return PSmat.GetColor("_TintColor"); } set { value.a = 0.15f; PSmat.SetColor("_TintColor", value); } }
        public Color LauncherColorDif { get { return matArrayLauncher[0].color; } set { matArrayLauncher[0].SetColor("_Color", value); } }
        public Color LauncherColor { get { return matArrayLauncher[1].color; } set { matArrayLauncher[1].SetColor("_TintColor", value); } }

        public ParticleSystem PS;
        public ParticleSystemRenderer PSR;
        public ParticleSystem.EmissionModule EM;

        public ParticleSystem gunFirePS;
        public ParticleSystemRenderer gunFirePSR;
        public ParticleSystem.ColorOverLifetimeModule COLTgunFire;

        public ParticleSystem PSexplo;
        public ParticleSystemRenderer PSRexplo;
        public ParticleSystem.ColorOverLifetimeModule COLTexplo;
        public Gradient gradient = new Gradient();
        public Gradient gunFireGradient = new Gradient();
        public Gradient gunFireSpotGradient = new Gradient();

        public AudioClip BL;
        public AudioClip BE;

        public int MyGUID;
        private Rect UIrect = new Rect(0, 100, 512, 128);
        private bool myClusterCode = false;
        public static MessageType BeamLaunched = ModNetworking.CreateMessageType(DataType.Integer, DataType.Integer, DataType.Integer, DataType.Single);
        public static MessageType BeamExplode = ModNetworking.CreateMessageType(DataType.Integer, DataType.Boolean);
        public static MessageType ShotgunLaunched = ModNetworking.CreateMessageType(DataType.Integer, DataType.Integer);

        public void initGunfire()
        {
            if(BlockBehaviour.transform.FindChild("gunFireBody") ==null)
            {
                gunFireBody = new GameObject("gunFireBody");
                gunFireBody.SetActive(false);
                gunFireBody.transform.SetParent(BlockBehaviour.transform);
                gunFireBody.transform.localPosition = new Vector3(0f, 0f, 1.0f);
                gunFireBody.transform.localRotation = Quaternion.identity;

                gunFireEffect = new GameObject("gunFireEffect");
                gunFireEffect.transform.SetParent(gunFireBody.transform);
                gunFireEffect.transform.localPosition = new Vector3(0f, 0f, 0.5f);
                gunFireEffect.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);

                gunFirePS = gunFireEffect.GetComponent<ParticleSystem>() ?? gunFireEffect.AddComponent<ParticleSystem>();
                gunFirePSR = gunFireEffect.GetComponent<ParticleSystemRenderer>();
                gunFirePS.loop = true;

                ParticleSystem.ShapeModule gunFireSM = gunFirePS.shape;
                gunFireSM.shapeType = ParticleSystemShapeType.Cone;
                gunFireSM.radius = 0f;
                gunFireSM.angle = 5f;
                
                gunFirePS.scalingMode = ParticleSystemScalingMode.Shape;

                gunFirePS.startDelay = 0f;
                gunFirePS.startLifetime = 0.05f;
                gunFirePS.startSpeed = 0.1f;
                gunFirePS.startSize = 1f;
                gunFirePS.randomSeed = 0;
                gunFirePS.simulationSpace = ParticleSystemSimulationSpace.Local;
                gunFirePS.gravityModifier = 0f;


                ParticleSystem.LimitVelocityOverLifetimeModule gunFireLVOLM = gunFirePS.limitVelocityOverLifetime;
                //LVOLM.separateAxes = false;
                //LVOLM.space = ParticleSystemSimulationSpace.Local;
                gunFireLVOLM.dampen = 0.1f;
                gunFireLVOLM.limit = 0f;
                gunFireLVOLM.enabled = true;

                gunFirePSR.material = new Material(Shader.Find("Particles/Alpha Blended"));
                gunFirePSR.material.mainTexture = ModResource.GetTexture("Gun Fire").Texture;
                gunFirePSR.material.SetColor("_TintColor", new Color(1, 1, 1, 1));
                gunFirePSR.renderMode = ParticleSystemRenderMode.Stretch;
                gunFirePSR.lengthScale = 4f;

                ParticleSystem.EmissionModule gunFireEM = gunFirePS.emission;
                gunFireEM.enabled = true;
                gunFireEM.rate = 5000f;
                gunFirePS.maxParticles = 20;
                
                COLTgunFire = gunFirePS.colorOverLifetime;
                COLTgunFire.enabled = true;
            }
        }
        public void initLauncher()
        {
            renderer2 = BlockBehaviour.GetComponentInChildren<MeshRenderer>();
            matArrayLauncher[0] = renderer2.material;
            matArrayLauncher[0].mainTexture = ModResource.GetTexture("BeamRifle Texture").Texture;
            matArrayLauncher[0].SetColor("_Color", new Color(1, 1, 1, 1));
            matArrayLauncher[1] = new Material(Shader.Find("Particles/Additive"));
            matArrayLauncher[1].mainTexture = ModResource.GetTexture("BeamRifle Light").Texture;
            matArrayLauncher[1].SetColor("_TintColor", new Color(1, 1, 1, 1));
            renderer2.materials = matArrayLauncher;

        }
        public void initShotgun()
        {
            if(BlockBehaviour.transform.FindChild("shotgunCol")==null)
            {
                shotgunCollider = new GameObject("shotgunCol");
                shotgunCollider.transform.SetParent(BlockBehaviour.transform);
                shotgunCollider.transform.localPosition = new Vector3(0f, 0f, 1.6f);
                shotgunCollider.transform.localRotation = Quaternion.identity;
                shotgunCollider.transform.localScale = Vector3.one * 1.6f;
                shotgunScan = shotgunCollider.AddComponent<MeshCollider>();
                shotgunScan.sharedMesh = ModResource.GetMesh("ShotgunScan").Mesh;
                shotgunScan.convex = true;
                shotgunScan.isTrigger = true;
                /*
                shotgunCollider.AddComponent<MeshFilter>().mesh= ModResource.GetMesh("ShotgunScan").Mesh;
                MeshRenderer shotgunMesh = shotgunCollider.AddComponent<MeshRenderer>();
                shotgunMesh.material= new Material(Shader.Find("Particles/Alpha Blended"));
                shotgunMesh.material.mainTexture = Texture2D.whiteTexture;
                shotgunMesh.material.SetColor("_TintColor", new Color(1, 1, 1, 0.1f));
                */
                shotgunHit = shotgunCollider.AddComponent<ShotgunCollisionHit>();
                shotgunCollider.SetActive(false);
            }
        }
        public void initExplo()
        {
            try
            {
                Destroy(BeamExplo);
            }
            catch { }
            BeamExplo = new GameObject("Beam Explo Particle");
            BeamExplo.SetActive(false);
            PSexplo = BeamExplo.GetComponent<ParticleSystem>() ?? BeamExplo.AddComponent<ParticleSystem>();
            PSRexplo = BeamExplo.GetComponent<ParticleSystemRenderer>();
            PSexplo.loop = false;

            ParticleSystem.ShapeModule SM = PSexplo.shape;
            SM.shapeType = ParticleSystemShapeType.Sphere;
            SM.radius = 0.5f;
            SM.angle = 30.0f;

            ParticleSystem.CollisionModule CM = PSexplo.collision;
            CM.enabled = false;
            PSexplo.scalingMode = ParticleSystemScalingMode.Shape;

            float PSduration = PSexplo.duration;
            PSduration = 1f;
            PSexplo.startDelay = 0f;
            PSexplo.startLifetime = 0.3f;
            PSexplo.startSpeed = 100f;
            PSexplo.startSize = 0.2f;
            PSexplo.randomSeed = 1;
            PSexplo.simulationSpace = ParticleSystemSimulationSpace.Local;
            PSexplo.gravityModifier = 0f;


            ParticleSystem.LimitVelocityOverLifetimeModule LVOLM = PSexplo.limitVelocityOverLifetime;
            //LVOLM.separateAxes = false;
            //LVOLM.space = ParticleSystemSimulationSpace.Local;
            LVOLM.dampen = 0.3f;
            LVOLM.limit = 0f;
            LVOLM.enabled = true;

            PSRexplo.material = new Material(Shader.Find("Particles/Additive"));
            PSRexplo.material.mainTexture = ModResource.GetTexture("Glow").Texture;
            PSRexplo.material.SetColor("_TintColor", new Color(1, 1, 1, 1));
            PSRexplo.renderMode= ParticleSystemRenderMode.Stretch;
            PSRexplo.lengthScale = 4f;

            ParticleSystem.EmissionModule EM = PSexplo.emission;
            EM.enabled = true;
            EM.rate = 20000f;
            PSexplo.maxParticles = 50;

            ParticleSystem.SizeOverLifetimeModule SOLTM = PSexplo.sizeOverLifetime;
            AnimationCurve animationCurve = new AnimationCurve();
            animationCurve.AddKey(0f, 5f);
            //animationCurve.AddKey(0.5f, 0.8f);
            animationCurve.AddKey(1f, 1f);
            SOLTM.size = new ParticleSystem.MinMaxCurve(1f, animationCurve);
            SOLTM.enabled = true;

            COLTexplo = PSexplo.colorOverLifetime;
            COLTexplo.enabled = true;


            BE = ModResource.GetAudioClip("Missile Exploed").AudioClip;
            AudioSource ASBE = BeamExplo.GetComponent<AudioSource>() ?? BeamExplo.AddComponent<AudioSource>();
            ASBE.clip = BE;

            ASBE.spatialBlend = 1.0f;
            ASBE.volume = 1.0f;

            ASBE.SetSpatializerFloat(1, 1f);
            ASBE.SetSpatializerFloat(2, 0);
            ASBE.SetSpatializerFloat(3, 12);
            ASBE.SetSpatializerFloat(4, 1000f);
            ASBE.SetSpatializerFloat(5, 1f);

            BeamExplo.AddComponent<MakeAudioSourceFixedPitch>();
        }
        public void initBullet()
        {
            try
            {
                Destroy(BeamBullet);
            }
            catch { }
            BeamBullet = new GameObject("Beam Bullet");
            BeamBullet.SetActive(false);
            BeamBullet.AddComponent<MeshFilter>().mesh=ModResource.GetMesh("BeamBullet");
            renderer1 = BeamBullet.AddComponent<MeshRenderer>();
            renderer1.material = new Material(Shader.Find("Particles/Alpha Blended"));
            renderer1.material.mainTexture = Texture2D.whiteTexture;
            renderer1.material.SetColor("_TintColor", new Color(1, 0, 0, 1));

            Rigidbody bbody = BeamBullet.AddComponent<Rigidbody>();
            bbody.mass = 0.5f;
            bbody.freezeRotation = true;
            bbody.useGravity = false;
            bbody.interpolation = RigidbodyInterpolation.Interpolate;

            if (BeamBullet.transform.childCount == 0) 
            {
                GameObject BeamPS = new GameObject("Beam PS");
                BeamPS.transform.SetParent(BeamBullet.transform);
                BeamPS.transform.localPosition = new Vector3(0f, 0f, 11f);

                PS = BeamPS.AddComponent<ParticleSystem>();
                PSR = BeamPS.GetComponent<ParticleSystemRenderer>();

                PS.startSize = PSBaseSize;

                PS.playbackSpeed = 2f;
                PS.startLifetime = 0.05f;
                PS.startSpeed = PSBaseSpeed;
                PS.scalingMode = ParticleSystemScalingMode.Shape;
                
                PS.startDelay = 0f;
                PS.maxParticles = 10000;
                PS.gravityModifier = 0f;
                PS.randomSeed = 0;

                ParticleSystem.CollisionModule CM = PS.collision;
                CM.enabled = false;

                ParticleSystem.ShapeModule SM = PS.shape;
                SM.angle = 0;
                SM.shapeType = ParticleSystemShapeType.Mesh;

                EM = PS.emission;
                EM.rate = PSBaseRate;
                EM.enabled = true;

                AnimationCurve animationCurve = new AnimationCurve();
                animationCurve.AddKey(0f, 1f);
                animationCurve.AddKey(0.2f, 1.1f);
                animationCurve.AddKey(1f, 0.6f);

                ParticleSystem.SizeOverLifetimeModule SOLT = PS.sizeOverLifetime;
                SOLT.size = new ParticleSystem.MinMaxCurve(1f, animationCurve);
                SOLT.enabled = true;

                PS.simulationSpace = ParticleSystemSimulationSpace.Local;

                PSmat = new Material(Shader.Find("Particles/Additive"));
                PSmat.mainTexture = ModResource.GetTexture("Glow").Texture;
                PSmat.SetColor("_TintColor", new Color(1f, 1f, 1f, 0.15f));

                PSR.material = PSmat;
                PSR.sortingFudge = 30000;
                PSR.renderMode = ParticleSystemRenderMode.Billboard;
                PSR.lengthScale = 4f;
            }

            BL = ModResource.GetAudioClip("Beam Launched").AudioClip;

            AudioSource ASBL = BeamBullet.AddComponent<AudioSource>();
            ASBL.clip = BL;
            ASBL.spatialBlend = 1.0f;
            ASBL.volume = 0.5f;
            
            ASBL.SetSpatializerFloat(1, 1f);
            ASBL.SetSpatializerFloat(2, 0);
            ASBL.SetSpatializerFloat(3, 12);
            ASBL.SetSpatializerFloat(4, 1000f);
            ASBL.SetSpatializerFloat(5, 1f);

            BeamBullet.AddComponent<MakeAudioSourceFixedPitch>();
        }
        public override void SafeAwake()
        {
            launch = AddKey("发射", "launch", KeyCode.X);
            BeamType=AddMenu("beamtype",0, new List<string>() { "光弹","散弹"});
            IFF = AddToggle("开启友伤", "IFF", true);
            RGB = AddToggle("R G B!", "rgb", false);
            safetyDelay = AddSlider("安全延时", "delay", 0.1f, 0.0f, 1.0f);
            ShootCDS = AddSlider("CD", "cooldown", 1f, MinCD, MaxCD);
            chargeS = AddSlider("力量", "charge", 1f, MinCharge, MaxCharge);
            speedS = AddSlider("飞行速度", "speed", 1f, 0.0f, 10.0f);
            BeamLength = AddSlider("光弹长度", "BeamLength", 1f, MinLen, MaxLen);
            BeamWidth = AddSlider("光弹宽度", "BeamWidth", 1f, MinWidth, MaxWidth);
            exploDis = AddSlider("溅射范围", "explodis", 1f, 0f, 10f);
            //TransformMenuController

            colourSliderDif = AddColourSlider("炮身颜色", "colorDif", new Color(1, 1, 1, 1), false);
            colourSlider = AddColourSlider("高亮颜色", "color", new Color(1, 0, 0, 1), false);
            colourSlider1 = AddColourSlider("光弹外圈颜色", "colorOutSide", new Color(1, 1, 1, 1), false);
            initBullet();
            initExplo();
            initShotgun();
            initGunfire();
            initLauncher();
            try
            {
                colourSliderDif.ValueChanged += (Color color) =>
                {
                    try
                    {
                        LauncherColorDif = colourSliderDif.Value;
                        renderer2.materials[0].SetColor("_Color", colourSliderDif.Value);
                    }
                    catch { }
                };
                colourSlider.ValueChanged += (Color color) =>
                {
                    try
                    {
                        LauncherColor = colourSlider.Value;
                        renderer2.materials[1].SetColor("_TintColor", colourSlider.Value);
                    }
                    catch { }
                };
            }
            catch { }
            
            BlockBehaviour.name = "Beam Rifle";
        }
        public override void BuildingUpdate()
        {
            if (renderer2.materials.Length < 2)
                renderer2.materials = matArrayLauncher;


            if (BlockBehaviour.Guid.GetHashCode() != 0 && BlockBehaviour.Guid.GetHashCode() != MyGUID)
                MyGUID = BlockBehaviour.Guid.GetHashCode();
        }
        public void Start()
        {
            try
            {
                BlockBehaviour.blockJoint.breakForce = 100000f;
                BlockBehaviour.blockJoint.breakTorque = 100000f;
            }
            catch { }
            if (RGB.IsActive)
            {
                LauncherColor = RGBController.Instance.outputColor;
            }
            else
                LauncherColor = colourSlider.Value;
            LauncherColorDif = colourSliderDif.Value;
            BeamColor = colourSlider.Value;
            BeamColor2 = colourSlider1.Value;
            renderer2.materials = matArrayLauncher;

            gradient.SetKeys(new GradientColorKey[]
            {
                new GradientColorKey(colourSlider.Value,0f),
                new GradientColorKey(colourSlider1.Value,0.8f),
                new GradientColorKey(Color.black,1f)
            }, new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 0.9f),
                new GradientAlphaKey(0f, 1f)
            });
            gunFireGradient.SetKeys(new GradientColorKey[]
            {
                new GradientColorKey(Color.Lerp(Color.white, colourSlider.Value,0.5f),0f),
                new GradientColorKey(Color.Lerp(Color.white, colourSlider.Value,0.5f),1f)
            }, new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            });
            COLTexplo.color = gradient;
            COLTgunFire = gunFirePS.colorOverLifetime;
            COLTgunFire.color = gunFireGradient;
            gunFirePS.startSize = BeamWidth.Value * 1.5f;
            ShootCD = Mathf.Max(0.05f, ShootCDS.Value);
            speed = 50f * speedS.Value;

            BeamBullet.transform.localScale = new Vector3(BeamWidth.Value, BeamWidth.Value, BeamLength.Value);
            
            PS.startSpeed = PSBaseSpeed * BeamLength.Value;
            PS.startSize = PSBaseSize * BeamWidth.Value;
            EM.rate = (BeamLength.Value / BeamWidth.Value) * PSBaseRate;

            if(BeamType.Value==0)
            {
                ParticleSystem.ShapeModule SM = PSexplo.shape;
                SM.shapeType = ParticleSystemShapeType.Sphere;
            }
            else if(BeamType.Value==1)
            {
                ParticleSystem.ShapeModule SM = PSexplo.shape;
                SM.shapeType = ParticleSystemShapeType.Cone;
            }
            PSexplo.startSpeed = 100f * exploDis.Value;
            PSexplo.maxParticles = (int)(50 * exploDis.Value);
            if (BeamType.Value == 0)
                PSexplo.startSize = 0.2f * exploDis.Value;
            else if (BeamType.Value == 1)
                PSexplo.startSize = 0.2f * (Mathf.Abs(BlockBehaviour.transform.localScale.x) + Mathf.Abs(BlockBehaviour.transform.localScale.y)) / 2f;
            Vector3 shotgunColScale = BlockBehaviour.transform.localScale;
            shotgunColScale.x = 1f / shotgunColScale.x;
            shotgunColScale.y = 1f / shotgunColScale.y;
            shotgunColScale.z = 1f / shotgunColScale.z;
            shotgunCollider.transform.localScale = shotgunColScale * 1.6f * exploDis.Value;
            charge = chargeS.Value;
            shotgunHit.charge = charge;
            shotgunHit.team = BlockBehaviour.Team;
            shotgunHit.PlayerID = BlockBehaviour.ParentMachine.PlayerID;
            shotgunHit.IFF = IFF.IsActive;
            //ASBL.pitch = 1f / ShootCD;

        }
        private void Update()
        {
            
            if (BlockBehaviour.isSimulating)
            {
                if (RGB.IsActive)
                {
                    LauncherColor = RGBController.Instance.outputColor;
                }
            }
            
            if (StatMaster.clusterCoded)
                myClusterCode = true;
            else
            {
                if (myClusterCode)
                {
                    renderer2.materials = matArrayLauncher;
                }
                myClusterCode = false;
            }
        }
        public override void OnSimulateStart()
        {
            if (!StatMaster.isHosting)
            {
                BeamRifleController.Instance.BD[BlockBehaviour.ParentMachine.PlayerID].Add(MyGUID, new List<KeyValuePair<int, float>>());
                BeamRifleController.Instance.SD[BlockBehaviour.ParentMachine.PlayerID].Add(MyGUID, 0);
            }
        }
        public override void OnSimulateStop()
        {
            shotgunCollider.SetActive(false);
            for (int i = 0; i < n.Count; i++)
            {
                try
                {
                    Destroy(n[i]);
                }
                catch { }
            }
            n.Clear();
            if (!StatMaster.isHosting)
            {
                BeamRifleController.Instance.BD[BlockBehaviour.ParentMachine.PlayerID].Remove(MyGUID);
                BeamRifleController.Instance.SD[BlockBehaviour.ParentMachine.PlayerID].Remove(MyGUID);
            }
        }
        private IEnumerator CLShoot(int code, float speedOffSet)
        {
            
            //Vector3 speedOffSet = Vector3.Project(BlockBehaviour.Rigidbody.velocity, BlockBehaviour.transform.forward);

            n.Add((GameObject)Instantiate(BeamBullet, gunFireBody.transform.position, BlockBehaviour.transform.rotation));
            gunFireBody.SetActive(true);
            StartCoroutine(SetActiveByTime(0.05f, gunFireBody));
            int ccount = n.Count - 1;
            n[ccount].SetActive(true);
            n[ccount].isStatic = false;

            Rigidbody nbody = n[ccount].GetComponent<Rigidbody>();
            //AudioSource ASBL = n[ccount].GetComponent<AudioSource>();
            //ASBL.Play();
            nbody.useGravity = false;
            //MissileController.Instance.MP.Add(code, nbody.transform);

            float i = 0f;

            StartCoroutine(SetActiveByTime(existTime, n[ccount]));

            for (; i < existTime; i += Time.fixedDeltaTime)
            {
                if (!n[ccount].activeSelf)
                    yield break;
                //ASBL.pitch = Time.timeScale;
                nbody.velocity = n[ccount].transform.forward * speedOffSet;

                yield return new WaitForSeconds(Time.fixedDeltaTime);


                //nbody.transform.position = MissileController.Instance.MP[code].position;
                //nbody.transform.eulerAngles = MissileController.Instance.MP[code].position;


                if (BeamRifleController.Instance.BE.ContainsKey(code))
                {

                    Vector3 expos = nbody.transform.position;
                    n[ccount].SetActive(false);
                    //MissileController.Instance.MP.Remove(code);
                    if (BeamRifleController.Instance.BE[code])
                    {
                        try
                        {
                            StartCoroutine(PlayExplo(expos));
                        }
                        catch { }
                    }
                    BeamRifleController.Instance.BE.Remove(code);
                    yield break;
                }

            }
            yield break;
        }
        private IEnumerator Shoot()
        {
            //float speedAngle = Vector3.Angle(BlockBehaviour.Rigidbody.velocity, BlockBehaviour.transform.forward);
            Vector3 speedOffSet = Vector3.Project(BlockBehaviour.Rigidbody.velocity, BlockBehaviour.transform.forward);
            
            n.Add((GameObject)Instantiate(BeamBullet, gunFireBody.transform.position, BlockBehaviour.transform.rotation));
            gunFireBody.SetActive(true);
            StartCoroutine(SetActiveByTime(0.05f, gunFireBody));
            int ccount = n.Count - 1;
            if (StatMaster.isMP)
            {
                Message beamLaunched = BeamLaunched.CreateMessage((int)MyGUID, (int)n[ccount].GetHashCode(), (int)BlockBehaviour.ParentMachine.PlayerID, (float)(n[ccount].transform.forward * speed + speedOffSet).magnitude);
                ModNetworking.SendToAll(beamLaunched);
            }
            n[ccount].SetActive(true);
            n[ccount].isStatic = false;

            Rigidbody nbody = n[ccount].GetComponent<Rigidbody>();
            //AudioSource ASBL = n[ccount].GetComponent<AudioSource>();
            //ASBL.Play();
            float i = 0f;
            Vector3 lastPos = nbody.transform.position;
            StartCoroutine(SetActiveByTime(existTime, n[ccount]));

            int thiscount = BeamRifleController.Instance.BeamCount;
            BeamRifleController.Instance.BeamCount++;

            BlockBehaviour.Rigidbody.AddForce(BlockBehaviour.Rigidbody.transform.forward * -charge *100f);
            for (; i < existTime; i += Time.fixedDeltaTime)
            {
                
                if (!n[ccount].activeSelf)
                    yield break;
                if (BeamRifleController.Instance.BeamCount - thiscount > countlimit)
                {
                    n[ccount].SetActive(false);
                    if (StatMaster.isMP)
                    {
                        Message beamExploded = BeamExplode.CreateMessage((int)n[ccount].GetHashCode(),(bool) false);
                        ModNetworking.SendToAll(beamExploded);
                    }

                    yield break;
                }

                //ASBL.pitch = Time.timeScale;
                nbody.velocity = n[ccount].transform.forward * speed + speedOffSet;

                yield return new WaitForSeconds(Time.fixedDeltaTime);

                if (i < safetyDelay.Value)
                {
                    lastPos = nbody.transform.position;
                    continue;
                }

                float raycastLen = (nbody.transform.position - lastPos).magnitude + 11f * nbody.transform.localScale.z;
                RaycastHit hit;
                if (Physics.Raycast(lastPos,nbody.transform.forward, out hit, raycastLen, Game.BlockEntityLayerMask, QueryTriggerInteraction.Ignore))
                {
                    //Vector3 hitDir = nbody.transform.forward;
                    n[ccount].SetActive(false);
                    if (StatMaster.isMP)
                    {
                        Message beamExploded = BeamExplode.CreateMessage((int)n[ccount].GetHashCode(), (bool)true);
                        ModNetworking.SendToAll(beamExploded);
                    }
                    try
                    {
                        StartCoroutine(PlayExplo(hit.point));
                    }
                    catch { }

                    try
                    {
                        BlockBehaviour hitedBlock=hit.rigidbody.gameObject.GetComponent<BlockBehaviour>();
                        hit.rigidbody.WakeUp();
                        try
                        {
                            if (!IFF.IsActive)
                            {
                                if (hitedBlock.Team == BlockBehaviour.Team)
                                {
                                    if (hitedBlock.Team != MPTeam.None)
                                        yield break;
                                    else if (hitedBlock.ParentMachine.PlayerID == BlockBehaviour.ParentMachine.PlayerID)
                                        yield break;
                                }
                            }
                        }
                        catch { }
                        try
                        {
                            if (hitedBlock.BlockID == 11)
                            {
                                if (hitedBlock.blockJoint.breakForce > 50000f)
                                {
                                    hitedBlock.blockJoint.breakForce = 50000f;
                                }
                                else
                                {
                                    hitedBlock.blockJoint.breakForce -= forceDamage * charge;
                                    if (hitedBlock.blockJoint.breakForce < 0)
                                        hitedBlock.blockJoint.breakForce = 0;
                                }
                            }
                        }
                        catch { }
                        try
                        {
                            hitedBlock.BlockHealth.DamageBlock(blockDamage);
                        }
                        catch { }
                    }
                    catch { }
                    try
                    {
                        hit.rigidbody.AddExplosionForce(power * charge, hit.point, 1f);
                    }
                    catch { }
                    
                    yield break;
                }
                lastPos = nbody.transform.position;

            }
            yield break;
        }
        private IEnumerator PlayExplo(Vector3 pos)
        {
            GameObject bomb = (GameObject)Instantiate(BeamExplo);
            bomb.transform.position = pos;
            bomb.transform.Rotate(0f, UnityEngine.Random.value*360f, 0f);
            bomb.SetActive(true);
            Destroy(bomb, PSexplo.startLifetime * 0.9f);

            yield break;
        }
        private IEnumerator shotgunShoot()
        {
            Vector3 expos = BlockBehaviour.transform.position + BlockBehaviour.transform.forward * 2.1f * BlockBehaviour.transform.localScale.z;
            if (StatMaster.isMP)
            {
                Message shotgunLaunched = ShotgunLaunched.CreateMessage((int)MyGUID, (int)BlockBehaviour.ParentMachine.PlayerID);
                ModNetworking.SendToAll(shotgunLaunched);
            }
            shotgunCollider.SetActive(true);
            StartCoroutine(PlayExploShotgun(expos));
            BlockBehaviour.Rigidbody.AddForce(BlockBehaviour.Rigidbody.transform.forward * -charge * 300f);
            //Collider[] hits = Physics.OverlapSphere(expos, 5f * exploDis.Value, Game.BlockEntityLayerMask, QueryTriggerInteraction.Ignore);
            yield return new WaitForSeconds(Time.fixedDeltaTime);
            shotgunCollider.SetActive(false);
            yield break;
        }
        private IEnumerator CLshotgunShoot()
        {
            Vector3 expos = BlockBehaviour.transform.position + BlockBehaviour.transform.forward * 2.1f * BlockBehaviour.transform.localScale.z;
            StartCoroutine(PlayExploShotgun(expos));
            yield break;
        }
        private IEnumerator PlayExploShotgun(Vector3 expos)
        {
            GameObject bomb = (GameObject)Instantiate(BeamExplo);
            bomb.transform.position = expos;
            bomb.transform.rotation = BlockBehaviour.transform.rotation;
            bomb.transform.Rotate(0f, 0f, UnityEngine.Random.value * 360f);
            bomb.SetActive(true);
            Destroy(bomb, PSexplo.startLifetime * 0.9f);

            
            yield break;
        }
        private IEnumerator SetActiveByTime(float time, GameObject tgt)
        {
            yield return new WaitForSeconds(time);
            tgt.SetActive(false);
        }
        private IEnumerator resetShoot()
        {
            ShootEnabled = false;
            yield return new WaitForSeconds(ShootCD);
            ShootEnabled = true;
            yield break;
        }
        public override void SimulateFixedUpdateHost()
        {
            try
            {
                if ((launch.IsHeld || launch.EmulationHeld()) && ShootEnabled && BlockBehaviour.BlockHealth.health > 0f && (BlockBehaviour.blockJoint))
                {
                    //AudioSource.PlayClipAtPoint(TL, BlockBehaviour.transform.position);
                    StartCoroutine(resetShoot());
                    if (BeamType.Value == 0)
                        StartCoroutine(Shoot());
                    else if (BeamType.Value == 1)
                        StartCoroutine(shotgunShoot());
                }
            }
            catch { }
        }
        public override void SimulateFixedUpdateClient()
        {
            if (BeamType.Value == 0)
            {
                if (BeamRifleController.Instance.BD[BlockBehaviour.ParentMachine.PlayerID][MyGUID].Count > 0)
                {
                    int thiscode = (BeamRifleController.Instance.BD[BlockBehaviour.ParentMachine.PlayerID][MyGUID][0].Key);
                    float thisSpeedOffSet = (BeamRifleController.Instance.BD[BlockBehaviour.ParentMachine.PlayerID][MyGUID][0].Value);
                    StartCoroutine(CLShoot(thiscode, thisSpeedOffSet));
                    BeamRifleController.Instance.BD[BlockBehaviour.ParentMachine.PlayerID][MyGUID].RemoveAt(0);
                }
            }
            else if(BeamType.Value == 1)
            {
                if (BeamRifleController.Instance.SD[BlockBehaviour.ParentMachine.PlayerID][MyGUID] > 0)
                {
                    BeamRifleController.Instance.SD[BlockBehaviour.ParentMachine.PlayerID][MyGUID] -= 1;
                    StartCoroutine(CLshotgunShoot());
                }
            }
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
            //if (cameraController.activeCamera)
            //if (!isFirstFrame || BlockBehaviour.isSimulating)
            {            //Vector3 a = cameraController.activeCamera.GetComponentInParent<BlockBehaviour>().GetTransform().forward;

                //GUI.Box(UIrect, PrefabMaster.LevelPrefabs[levelBombCategory].GetValue(levelBombID).gameObject.GetComponent<ExplodeOnCollide>().explosionEffectPrefab.gameObject.transform.FindChild("PyroclasticPuff").gameObject.transform.FindChild("PyroclasticPuff").gameObject.GetComponent<ExplosionMat>().ExplosionMaterial.shader.name.ToString());
                //GUI.Box(UIrect, BlockBehaviour.gameObject.layer.ToString()+","+ torpedo.layer.ToString());
                //GUI.color = new Color(1, 1, 1, 1);

            }
            try
            {
                string showmsg = BeamRifleController.Instance.SD[BlockBehaviour.ParentMachine.PlayerID][MyGUID].ToString();
                //showmsg += texttest.Value; 
                GUI.Box(UIrect, showmsg.ToString());
            }
            catch { }
        }
        */
    }
}
