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
    class LaserController : SingleInstance<LaserController>
    {
        public override string Name { get; } = "Laser Controller";
        public Dictionary<int, float[]>[] lengthRcv = new Dictionary<int, float[]>[16];

        public LaserController()
        {
            for (int i = 0; i < 16; i++)
            {
                lengthRcv[i] = new Dictionary<int, float[]>();
            }
        }
        public static void lengthRcved(Message message)
        {
            if (StatMaster.isClient)
            {
                Instance.lengthRcv[(int)message.GetData(0)][(int)message.GetData(1)][0] = Instance.lengthRcv[(int)message.GetData(0)][(int)message.GetData(1)][1];//当前位置
                Instance.lengthRcv[(int)message.GetData(0)][(int)message.GetData(1)][1] = (float)message.GetData(2);//目标位置
                Instance.lengthRcv[(int)message.GetData(0)][(int)message.GetData(1)][2] = 0f;//收到上一个包后经过的时常
            }
        }
    }
    class LaserLauncher: BlockScript
    {
        public MKey Launch;
        public MToggle RGB;
        private MToggle IFF;
        public MToggle Animated;
        public MToggle AnimateLoop;
        public MSlider length;
        public MSlider Width;
        public MSlider StartWidth;
        public MSlider EndWidth;
        public MSlider AnimateSpeed;
        public MSlider Charge;
        public MColourSlider LaserColor;
        public MColourSlider LaserColor1;
        public MColourSlider colourSliderDif;

        public Vector3 StartPos;
        public Vector3 EndPos;
        public float animateTime = 0f;
        public float HitLength;
        public float StartLength = 1.2f;
        public RaycastHit hit;
        private bool CanDamage = true;
        private bool CanSend = true;
        private float damageRate = 0.25f;
        private float blockDamage = 1f;
        private float forceDamage = 5000f;
        private float power = 10000f;
        private float MinLength = 1f;
        private float MaxLength = 1000f;


        public bool islaunched = false;
        public bool isheld = false;

        public GameObject LaserPS;
        public GameObject LaserAS1;
        public GameObject LaserAS2;
        public LineRenderer LaserLine;
        public Vector2 LaserTexOffset;

        public float displaySpeed = 10f;
        public float LaserWidth = 0.5f;
        private Rect UIrect = new Rect(0, 100, 512, 128);
        private bool Inited = false;

        public Material[] matArray = new Material[2];
        public Material[] matHitArray = new Material[2];
        public Material[] matArrayLauncher = new Material[3];
        public Color LauncherColor { get { return matArrayLauncher[1].color; } set { matArrayLauncher[1].SetColor("_TintColor", value); } }
        public Color LauncherInsideColor { get { return matArrayLauncher[2].color; } set { matArrayLauncher[2].SetColor("_TintColor", value); } }
        public Color LauncherColorDif { get { return matArrayLauncher[0].color; } set { matArrayLauncher[0].SetColor("_Color", value); } }

        private MeshRenderer renderer2;
        private ParticleSystem PS;
        private ParticleSystemRenderer PSR;
        private ParticleSystem.ColorOverLifetimeModule COLT;
        private Gradient gradient = new Gradient();

        private AudioClip LL;
        private AudioClip LN;
        private AudioSource ASLL;
        private AudioSource ASLN;

        public int MyGUID;
        public static MessageType SendLength = ModNetworking.CreateMessageType(DataType.Integer, DataType.Integer, DataType.Single);
        //public static MessageType SendHeld = ModNetworking.CreateMessageType(DataType.Integer, DataType.Integer, DataType.Boolean);

        public bool myClusterCode = false;

        public void InitLauncher()
        {
            renderer2 = BlockBehaviour.GetComponentInChildren<MeshRenderer>();
            matArrayLauncher[0] = renderer2.material;
            matArrayLauncher[0].mainTexture = ModResource.GetTexture("LaserLauncher Texture").Texture;
            matArrayLauncher[0].SetColor("_Color", new Color(1, 1, 1, 1));
            matArrayLauncher[1] = new Material(Shader.Find("Particles/Additive"));
            matArrayLauncher[1].mainTexture = ModResource.GetTexture("LaserLauncher Light").Texture;
            matArrayLauncher[1].SetColor("_TintColor", new Color(1, 1, 1, 1));
            matArrayLauncher[2] = new Material(Shader.Find("Particles/Additive"));
            matArrayLauncher[2].mainTexture = ModResource.GetTexture("LaserLauncher Light2").Texture;
            matArrayLauncher[2].SetColor("_TintColor", new Color(1, 1, 1, 1));
            renderer2.materials = matArrayLauncher;
        }
        public void InitLaser()
        {

            LaserLine = BlockBehaviour.gameObject.GetComponent<LineRenderer>() ?? BlockBehaviour.gameObject.AddComponent<LineRenderer>();
            //LaserLine.material = 

            LaserLine.receiveShadows = false;
            LaserLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            LaserLine.SetWidth(LaserWidth, LaserWidth);
            //LaserLine.SetColors(Color.red, Color.red);



            matArray[0] = new Material(Shader.Find("Particles/Alpha Blended"));
            matArray[0].mainTexture = ModResource.GetTexture("Beam").Texture;
            matArray[0].SetColor("_TintColor", new Color(1, 0, 0, 1));

            //matArray[0].renderQueue -= 1;

            matArray[1] = new Material(Shader.Find("Particles/Additive"));
            matArray[1].mainTexture = ModResource.GetTexture("BeamAdditive").Texture;
            matArray[1].SetColor("_TintColor", new Color(1f, 1f, 1f, 1f));
            //matArray[1].renderQueue -= 1;

            LaserLine.materials = matArray;

            LaserLine.SetVertexCount(0);


            

            LaserTexOffset = new Vector2(0f, 0f);

            if (BlockBehaviour.gameObject.transform.FindChild("Laser Particle") == null)
            {
                LaserPS = new GameObject("Laser Particle");
                LaserPS.transform.SetParent(BlockBehaviour.gameObject.transform);
                LaserPS.transform.localPosition = new Vector3(0f, 0f, StartLength);
                LaserAS1 = new GameObject("Laser AS1");
                LaserAS2 = new GameObject("Laser AS2");
                LaserAS1.transform.SetParent(BlockBehaviour.gameObject.transform);
                LaserAS2.transform.SetParent(BlockBehaviour.gameObject.transform);
                LaserAS1.transform.localPosition = new Vector3(0f, 0f, StartLength);
                LaserAS2.transform.localPosition = new Vector3(0f, 0f, StartLength);
                LaserAS1.AddComponent<AudioSource>();
                LaserAS2.AddComponent<AudioSource>();
                LaserAS1.AddComponent<MakeAudioSourceFixedPitch>();
                LaserAS2.AddComponent<MakeAudioSourceFixedPitch>();


            }
            LaserPS.SetActive(false);
            LaserAS1.SetActive(false);
            LaserAS2.SetActive(false);
            LL = ModResource.GetAudioClip("Laser Launched").AudioClip;
            LN = ModResource.GetAudioClip("Laser Noise").AudioClip;

            ASLL = LaserAS1.GetComponent<AudioSource>() ?? LaserAS1.AddComponent<AudioSource>();
            ASLN = LaserAS2.GetComponent<AudioSource>() ?? LaserAS2.AddComponent<AudioSource>();
            ASLL.clip = LL;
            ASLN.clip = LN;

            ASLN.spatialBlend = ASLL.spatialBlend = 1.0f;
            ASLN.volume = ASLL.volume = 1.0f;

            ASLL.SetSpatializerFloat(1, 1f);
            ASLL.SetSpatializerFloat(2, 0);
            ASLL.SetSpatializerFloat(3, 12);
            ASLL.SetSpatializerFloat(4, 1000f);
            ASLL.SetSpatializerFloat(5, 1f);

            ASLN.SetSpatializerFloat(1, 1f);
            ASLN.SetSpatializerFloat(2, 0);
            ASLN.SetSpatializerFloat(3, 12);
            ASLN.SetSpatializerFloat(4, 1000f);
            ASLN.SetSpatializerFloat(5, 1f);

            ASLN.loop = true;
            
            PS = LaserPS.GetComponent<ParticleSystem>() ?? LaserPS.AddComponent<ParticleSystem>();

            PS.startSize = LaserWidth * 3;

            
            //PS.startColor = new Color(1, 1, 1, 1);

            PS.startLifetime = 0.1f;
            PS.startSpeed = 0f;
            PS.scalingMode = ParticleSystemScalingMode.Shape;
            PS.startDelay = 0f;
            PS.maxParticles = 50;
            PS.gravityModifier = 0f;
            PS.randomSeed = 0;

            ParticleSystem.CollisionModule CM = PS.collision;
            CM.enabled = false;

            ParticleSystem.ShapeModule SM = PS.shape;
            SM.angle = 0;
            SM.shapeType = ParticleSystemShapeType.Mesh;

            ParticleSystem.EmissionModule EM = PS.emission;
            EM.rate = 15f;
            EM.enabled = true;

            
            
            AnimationCurve animationCurve = new AnimationCurve();
            animationCurve.AddKey(0f, 1f);
            animationCurve.AddKey(1f, 2f);
            

            
            ParticleSystem.SizeOverLifetimeModule SOLT = PS.sizeOverLifetime;
            SOLT.size= new ParticleSystem.MinMaxCurve(1f, animationCurve);
            SOLT.enabled = true;
            
            /*
            COLT = PS.colorOverLifetime;

            COLT.enabled = true;
            */
            PS.simulationSpace = ParticleSystemSimulationSpace.Local;
            PSR = LaserPS.GetComponent<ParticleSystemRenderer>();

            matHitArray[0] = new Material(Shader.Find("Particles/Alpha Blended"));
            matHitArray[0].mainTexture = ModResource.GetTexture("BeamHited").Texture;
            matHitArray[0].SetColor("_TintColor", new Color(1, 0, 0, 1));
            //matArray[0].renderQueue -= 1;

            matHitArray[1] = new Material(Shader.Find("Particles/Additive"));
            matHitArray[1].mainTexture = ModResource.GetTexture("BeamHitedAdditive").Texture;
            matHitArray[1].SetColor("_TintColor", new Color(1f, 1f, 1f, 1f));

            PSR.materials = matHitArray;
            PSR.sortingFudge = -10f;
            //PSR.renderMode = ParticleSystemRenderMode.Billboard;
            //PSR.lengthScale = 4f;

            //LaserLine.sortingOrder = PSR.sortingOrder;

            Inited = true;
        }

        public override void SafeAwake()
        {
            Launch = AddKey("发射", "launch", KeyCode.X);
            RGB = AddToggle("R G B!", "rgb", false);
            IFF = AddToggle("开启友伤", "IFF", true);
            Animated = AddToggle("宽度变化", "Animated", false);
            AnimateLoop = AddToggle("循环", "AnimateLoop", false);
            Charge = AddSlider("力量", "Charge", 1f, 0f, 10f);
            length = AddSlider("最大射程", "length", 200f, MinLength, MaxLength);
            Width = AddSlider("激光宽度", "width", 0.5f, 0f, 100f);
            StartWidth = AddSlider("起始宽度", "StartWidth", 0.5f, 0f, 1f);
            EndWidth = AddSlider("结束宽度", "EndWidth", 0.5f, 0f, 1f);
            AnimateSpeed = AddSlider("变化速度", "AnimateSpeed", 1f, 0f, 10f);
            colourSliderDif = AddColourSlider("炮身颜色", "colorDif", new Color(1, 1, 1, 1), false);
            LaserColor = AddColourSlider("激光颜色", "color", new Color(1, 0, 0, 1), false);
            LaserColor1 = AddColourSlider("激光内部颜色", "insideColor", new Color(1, 1, 1, 1), false);

            if (!Inited)
            {
                InitLauncher();
                InitLaser();

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
                    LaserColor.ValueChanged += (Color color) =>
                    {
                        try
                        {
                            LauncherColor = LaserColor.Value;
                            renderer2.materials[1].SetColor("_TintColor", LaserColor.Value);
                        }
                        catch { }
                    };
                    LaserColor1.ValueChanged += (Color color) =>
                    {
                        try
                        {
                            LauncherInsideColor = LaserColor1.Value;
                            renderer2.materials[2].SetColor("_TintColor", LaserColor1.Value);
                        }
                        catch { }
                    };
                }
                catch { }
            }

            BlockBehaviour.gameObject.name = "LaserLauncher";
        }
        public override void OnSimulateStart()
        {
            LaserController.Instance.lengthRcv[BlockBehaviour.ParentMachine.PlayerID].Add(MyGUID, new float[3]);
            KeymsgController.Instance.keyheld[BlockBehaviour.ParentMachine.PlayerID].Add(MyGUID, false);
            LaserController.Instance.lengthRcv[BlockBehaviour.ParentMachine.PlayerID][MyGUID][0] = length.Value;
            LaserController.Instance.lengthRcv[BlockBehaviour.ParentMachine.PlayerID][MyGUID][1] = length.Value;
        }
        public override void OnSimulateStop()
        {
            LaserController.Instance.lengthRcv[BlockBehaviour.ParentMachine.PlayerID].Remove(MyGUID);
            KeymsgController.Instance.keyheld[BlockBehaviour.ParentMachine.PlayerID].Remove(MyGUID);
            //myCollider.enabled = true;
        }
        public override void BuildingUpdate()
        {
            
            if (renderer2.materials.Length < 3)
                renderer2.materials = matArrayLauncher;

            AnimateLoop.DisplayInMapper = Animated.IsActive;
            StartWidth.DisplayInMapper = EndWidth.DisplayInMapper = AnimateSpeed.DisplayInMapper = Animated.IsActive;
            if (BlockBehaviour.Guid.GetHashCode() != 0 && BlockBehaviour.Guid.GetHashCode() != MyGUID)
                MyGUID = BlockBehaviour.Guid.GetHashCode();
            
        }
        public void Update()
        {
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
        public void LateUpdate()
        {
            if (BlockBehaviour.isSimulating)
            {
                if (RGB.IsActive)
                {
                    LauncherColor = RGBController.Instance.outputColor;
                    PSR.materials[0].SetColor("_TintColor", RGBController.Instance.outputColor);
                    LaserLine.materials[0].SetColor("_TintColor", RGBController.Instance.outputColor);
                }
            }
            if (BlockBehaviour.isSimulating) 
            {
                if (Launch.IsHeld || Launch.EmulationHeld() || KeymsgController.Instance.keyheld[BlockBehaviour.ParentMachine.PlayerID][MyGUID])   
                {
                    if (StatMaster.isClient)
                        solveLength();
                    LaserTexOffset.x -= displaySpeed * Time.unscaledDeltaTime;
                    if (LaserTexOffset.x < -2f)
                        LaserTexOffset.x += 2f;
                    LaserLine.materials[0].mainTextureOffset = LaserTexOffset;
                    LaserLine.materials[1].mainTextureOffset = LaserTexOffset;
                    LaserLine.materials[0].mainTextureScale = new Vector2(HitLength / (10 * LaserWidth), 1);
                    LaserLine.materials[1].mainTextureScale = new Vector2(HitLength / (10 * LaserWidth), 1);
                    

                    StartPos = BlockBehaviour.gameObject.transform.position + BlockBehaviour.gameObject.transform.forward * BlockBehaviour.gameObject.transform.localScale.z * StartLength;
                    EndPos = StartPos + BlockBehaviour.transform.forward * (HitLength);

                    LaserLine.SetVertexCount(2);
                    LaserLine.SetPositions(new Vector3[] { StartPos, EndPos });

                    if(Animated.IsActive)
                    {
                        float CurrentWidth = Mathf.Lerp(StartWidth.Value * LaserWidth, EndWidth.Value * LaserWidth, animateTime);
                        LaserLine.SetWidth(CurrentWidth, CurrentWidth);

                        if(animateTime<=1f)
                            animateTime += Time.deltaTime * AnimateSpeed.Value;
                        if (AnimateLoop.IsActive)
                        {
                            if (animateTime > 1f)
                                animateTime = 0f;
                        }
                    }

                    LaserPS.transform.localPosition = new Vector3(0f, 0f, StartLength + HitLength / BlockBehaviour.gameObject.transform.localScale.z);

                    LaserPS.SetActive(true);
                    LaserAS1.SetActive(true);
                    LaserAS2.SetActive(true);
                    //ASLL.pitch = Time.timeScale;
                    //ASLN.pitch = Time.timeScale;
                    float ASz = BlockBehaviour.gameObject.transform.InverseTransformPoint(Camera.main.transform.position).z;
                    if (ASz > HitLength)
                        ASz = HitLength + StartLength;
                    if (ASz < StartLength)
                        ASz = StartLength;
                    LaserAS2.transform.localPosition = new Vector3(0f, 0f, ASz);

                    PS.startRotation = UnityEngine.Random.Range(-Mathf.PI, Mathf.PI);
                }
                else 
                {
                    if (Time.timeScale > 0f)
                    {
                        LaserLine.SetVertexCount(0);

                        LaserPS.SetActive(false);
                        LaserAS1.SetActive(false);
                        LaserAS2.SetActive(false);

                        if (Animated.IsActive)
                        {
                            LaserLine.SetWidth(StartWidth.Value * LaserWidth, StartWidth.Value * LaserWidth);
                            animateTime = 0f;
                        }
                    }
                }
            }
            //myCollider.enabled = true;
        }
        public void Start()
        {
            /*
            gradient.SetKeys(new GradientColorKey[]
            {
            }, new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            });

            COLT.color = gradient;
            */
            animateTime = 0f;
            try
            {
                BlockBehaviour.blockJoint.breakForce = 100000f;
                BlockBehaviour.blockJoint.breakTorque = 100000f;
            }
            catch { }
            LaserPS.SetActive(false);
            LaserAS1.SetActive(false);
            LaserAS2.SetActive(false);
            CanDamage = true;
            CanSend = true;
            islaunched = false;
            isheld = false;
            LaserWidth = Width.Value;
            PS.startSize = (LaserWidth * 3f);
            LaserLine.SetWidth(LaserWidth, LaserWidth);


            
            LaserLine.materials[1].SetColor("_TintColor", LaserColor1.Value);
            PSR.materials[1].SetColor("_TintColor", LaserColor1.Value);
            if (RGB.IsActive)
            {
                PSR.materials[0].SetColor("_TintColor", RGBController.Instance.outputColor);
                LaserLine.materials[0].SetColor("_TintColor", RGBController.Instance.outputColor);
                LauncherColor = RGBController.Instance.outputColor;
            }
            else
            {
                PSR.materials[0].SetColor("_TintColor", LaserColor.Value);
                LaserLine.materials[0].SetColor("_TintColor", LaserColor.Value);
                LauncherColor = LaserColor.Value;
            }
            LauncherColorDif = colourSliderDif.Value;
            LauncherInsideColor = LaserColor1.Value;
            renderer2.materials = matArrayLauncher;
            HitLength = length.Value;
            //LaserLine.SetColors(LaserColor.Value, LaserColor.Value);


        }
        public override void SimulateFixedUpdateHost()
        {
            if (Launch.IsHeld || Launch.EmulationHeld()) 
            {
                islaunched = true;
                Hit(StartPos, BlockBehaviour.transform.forward);
            }
            else
            {
                islaunched = false;
            }
            if(isheld!=islaunched)
            {
                isheld = islaunched;
                ModNetworking.SendToAll(KeymsgController.SendHeld.CreateMessage((int)BlockBehaviour.ParentMachine.PlayerID, (int)MyGUID, (bool)isheld));
            }
        }
        public void Hit(Vector3 Pos, Vector3 Dir)
        {
            if (BlockBehaviour.BlockHealth.health > 0f && BlockBehaviour.blockJoint)
            {
                Ray rayH = new Ray(Pos, Dir);
                if (Physics.Raycast(rayH, out hit, length.Value, Game.BlockEntityLayerMask, QueryTriggerInteraction.Ignore))
                {
                    HitLength = hit.distance;

                }
                else
                    HitLength = length.Value;
            }
            else
            {
                HitLength = 1f;
                CanDamage = false;
            }

            if (CanSend)
            {
                CanSend = false;
                StartCoroutine(sendMSG());
            }
            if (CanDamage)
            {
                CanDamage = false;
                StartCoroutine(MakeDamage());
            }


        }

        private IEnumerator MakeDamage()
        {
            try
            {
                hit.collider.attachedRigidbody.WakeUp();
                float damageOfDistance = (1f - (HitLength / length.Value)) * Charge.Value;
                try
                {
                    BlockBehaviour hitedBlock = hit.collider.attachedRigidbody.gameObject.GetComponent<BlockBehaviour>();
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
                                hitedBlock.blockJoint.breakForce -= forceDamage * damageOfDistance;
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
                    hit.rigidbody.AddExplosionForce(power * damageOfDistance, hit.point, 1f);
                }
                catch { }
            }
            catch { };

            yield return new WaitForSeconds(damageRate);
            CanDamage = true;
            yield break;
        }
        private IEnumerator sendMSG()
        {
            ModNetworking.SendToAll(SendLength.CreateMessage((int)BlockBehaviour.ParentMachine.PlayerID, (int)MyGUID, (float)HitLength));
            yield return new WaitForSecondsRealtime(TurretController.Instance.sendCD * 2f);
            CanSend = true;
            yield break;
        }
        private void solveLength()
        {
            HitLength = Mathf.Lerp(
                LaserController.Instance.lengthRcv[BlockBehaviour.ParentMachine.PlayerID][MyGUID][0],
                LaserController.Instance.lengthRcv[BlockBehaviour.ParentMachine.PlayerID][MyGUID][1],
                LaserController.Instance.lengthRcv[BlockBehaviour.ParentMachine.PlayerID][MyGUID][2] / (TurretController.Instance.sendCD * 2f)
            );
            LaserController.Instance.lengthRcv[BlockBehaviour.ParentMachine.PlayerID][MyGUID][2] += Time.unscaledDeltaTime;

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
            
            string showMSG = "";

            showMSG += islaunched.ToString()+"\n";
            showMSG += isheld.ToString() + "\n";
            if(BlockBehaviour.isSimulating)
                showMSG += LaserController.Instance.laserLaunched[BlockBehaviour.ParentMachine.PlayerID][MyGUID].ToString() + "\n";

            GUI.Box(UIrect, showMSG);
           

        }
        */
    }
}
