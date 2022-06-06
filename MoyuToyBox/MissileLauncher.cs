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
    class MissileController : SingleInstance<MissileController>
    {
        public override string Name { get; } = "Missile Controller";

        public Dictionary<int, List<int>>[] MD = new Dictionary<int, List<int>>[16];

        public HashSet<int> ME=new HashSet<int>();
        //public Dictionary<int, Transform> MP = new Dictionary<int, Transform>();

        public int missileCount=0;
        public MissileController()
        {
            missileCount = 0;
            for (int i = 0; i < 16; i++)
            {
                MD[i] = new Dictionary<int, List<int>>();
            }
        }

        public void FixedUpdate()
        {
            if (!StatMaster.levelSimulating)
                missileCount = 0;
        }
        public static void MissileLaunchedEvent(Message message)
        {
            if (StatMaster.isClient)
            {
                Instance.MD[(int)message.GetData(2)][(int)message.GetData(0)].Add((int)message.GetData(1));
            }
        }
        /*
        public static void MissileMoveEvent(Message message)
        {
            if (StatMaster.isClient)
            {
                MissileController.Instance.MP[(int)message.GetData(0)].position = (Vector3)message.GetData(1);
                MissileController.Instance.MP[(int)message.GetData(0)].eulerAngles = (Vector3)message.GetData(2);
            }
        }
        */
        public static void MissileExplodeEvent(Message message)
        {
            if (StatMaster.isClient)
            {
                Instance.ME.Add((int)message.GetData(0));
            }
        }

    }
    class MissileLauncher : BlockScript
    {
        public int countlimit = 300;
        private readonly int levelBombCategory = 4;
        private readonly int levelBombID = 5003;


        private float radius = 2.5f;
        private float power = 3000f;
        private float torquePower = 3000f;
        private float upPower = 0f;
        private float blockDamage = 1.0f;
        private float forceDamage = 5000f;
        private float Tspeed = 250f;
        private float traceAngle = 5f;

        private float charge = 1f;

        private float existTime = 1.0f;
        private float ShootCD = 0.2f;
        private MSlider ShootCDS;
        private MSlider lifeTime;
        private MSlider traceAngleS;
        private MSlider missileSpeed;
        private MSlider missileCharge;
        private MSlider missileRadiu;
        private float MinCD = 0.2f;
        private float MaxCD = 10.0f;
        private float MinLT = 1.0f;
        private float MaxLT = 10.0f;
        private float MinAngle = 1.0f;
        private float MaxAngle = 10.0f;

        public int MyGUID;
        private Rect UIrect = new Rect(0, 100, 512, 128);

        public GameObject Missile;
        public GameObject MissileSpot;
        public GameObject MissileExplo;
        public GameObject gunFireBody;
        public GameObject gunFireEffect;

        public Collider pickedCollider;
        public int iconSize = 48;
        public Texture Aicon;
        public MKey launch;
        public MKey picker;
        public MKey resetPicker;
        public MSlider safetyDelay;
        public MSlider diffuseAngle;
        public MToggle RGB;
        public MToggle IFF;
        public MToggle Reverse;
        public MColourSlider colourSliderDif;
        public MColourSlider colourSlider;
        public bool ShootEnabled = true;
        public List<GameObject> n = new List<GameObject>();

        public TrailRenderer TrailRenderer;
        public bool TrailEnable { get { return TrailRenderer.enabled; } set { TrailRenderer.enabled = value; } }
        public float TrailLength { get { return TrailRenderer.time; } set { TrailRenderer.time = value; } }
        public Color TrailColor { get { return TrailRenderer.material.color; } set { TrailRenderer.material.SetColor("_TintColor", value); } }

        public MeshRenderer renderer2;
        public Material[] matArray = new Material[2];
        public Material[] matArrayLauncher = new Material[2];
        public Color MissileColor { get { return matArray[1].color; } set { matArray[1].SetColor("_TintColor", value); } }
        public Color LauncherColor { get { return matArrayLauncher[1].color; } set { matArrayLauncher[1].SetColor("_TintColor", value); } }
        public Color LauncherColorDif { get { return matArrayLauncher[0].color; } set { matArrayLauncher[0].SetColor("_Color", value); } }

        public AudioClip ML;

        public static MessageType MissileLaunched = ModNetworking.CreateMessageType(DataType.Integer, DataType.Integer, DataType.Integer);
        public static MessageType MissileExplode = ModNetworking.CreateMessageType(DataType.Integer);
        //public static MessageType MissilePosAndRot = ModNetworking.CreateMessageType(DataType.Integer, DataType.Vector3, DataType.Vector3);

        public bool myClusterCode = false;

        public ParticleSystem PS;
        public ParticleSystemRenderer PSR;
        public ParticleSystem.ColorOverLifetimeModule COLT;

        public ParticleSystem gunFirePS;
        public ParticleSystemRenderer gunFirePSR;
        public ParticleSystem.ColorOverLifetimeModule COLTgunFire;

        public ParticleSystem missileSpotPS;
        public ParticleSystemRenderer missileSpotPSR;
        public ParticleSystem.ColorOverLifetimeModule COLTmissileSpot;

        public Gradient gradient = new Gradient();
        public Gradient gunFireGradient = new Gradient();

        public AudioClip ME;
        public AudioSource ASME;

        public void initGunfire()
        {
            if (BlockBehaviour.transform.FindChild("gunFireBody") == null)
            {
                gunFireBody = new GameObject("gunFireBody");
                gunFireBody.SetActive(false);
                gunFireBody.transform.SetParent(BlockBehaviour.transform);
                gunFireBody.transform.localPosition = new Vector3(0.32f, -0.3f, 0.292f);
                gunFireBody.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

                gunFireEffect = new GameObject("gunFireEffect");
                gunFireEffect.transform.SetParent(gunFireBody.transform);
                gunFireEffect.transform.localPosition = Vector3.zero;
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
            matArrayLauncher[0].mainTexture = ModResource.GetTexture("MissileLauncher Texture").Texture;
            matArrayLauncher[0].SetColor("_Color", new Color(1, 1, 1, 1));
            matArrayLauncher[1] = new Material(Shader.Find("Particles/Additive"));
            matArrayLauncher[1].mainTexture = ModResource.GetTexture("MissileLauncher Light").Texture;
            matArrayLauncher[1].SetColor("_TintColor", new Color(1, 1, 1, 1));
            renderer2.materials = matArrayLauncher;

        }
        public void initExplo()
        {
            try
            {
                Destroy(MissileExplo);
            }
            catch { }
            MissileExplo = new GameObject("Missile Explo Particle");
            MissileExplo.SetActive(false);
            PS = MissileExplo.GetComponent<ParticleSystem>() ?? MissileExplo.AddComponent<ParticleSystem>();
            PSR = MissileExplo.GetComponent<ParticleSystemRenderer>();
            PS.loop = false;

            ParticleSystem.ShapeModule SM = PS.shape;
            SM.shapeType = ParticleSystemShapeType.Sphere;
            SM.radius = 0.01f;

            ParticleSystem.CollisionModule CM = PS.collision;
            CM.enabled = false;
            PS.scalingMode = ParticleSystemScalingMode.Shape;

            float PSduration = PS.duration;
            PSduration = 1f;
            //PS.loop = true;
            PS.startDelay = 0f;
            PS.startLifetime = 0.5f;
            PS.startSpeed = 50f;
            PS.startSize = 1f;
            PS.simulationSpace = ParticleSystemSimulationSpace.Local;
            PS.gravityModifier = 0f;
            

            ParticleSystem.LimitVelocityOverLifetimeModule LVOLM = PS.limitVelocityOverLifetime;
            //LVOLM.separateAxes = false;
            //LVOLM.space = ParticleSystemSimulationSpace.Local;
            LVOLM.dampen = 0.8f;
            LVOLM.limit = 0f;
            LVOLM.enabled = true;

            PSR.material = new Material(Shader.Find("Particles/Alpha Blended"));
            PSR.material.mainTexture = ModResource.GetTexture("Missile Explo").Texture;
            PSR.material.SetColor("_TintColor", new Color(1, 1, 1, 1));

            ParticleSystem.EmissionModule EM = PS.emission;
            EM.enabled = true;
            EM.rate = 20000f;
            PS.maxParticles = 20;

            ParticleSystem.SizeOverLifetimeModule SOLTM = PS.sizeOverLifetime;
            AnimationCurve animationCurve = new AnimationCurve();
            animationCurve.AddKey(0f, 1f);
            animationCurve.AddKey(0.8f, 5f);
            animationCurve.AddKey(1f, 4f);
            SOLTM.size = new ParticleSystem.MinMaxCurve(1f, animationCurve);
            SOLTM.enabled = true;

            COLT = PS.colorOverLifetime;
            COLT.enabled = true;

            ME = ModResource.GetAudioClip("Missile Exploed").AudioClip;
            ASME = MissileExplo.GetComponent<AudioSource>() ?? MissileExplo.AddComponent<AudioSource>();
            ASME.clip = ME;

            ASME.spatialBlend = 1.0f;
            ASME.volume = 1.0f;

            ASME.SetSpatializerFloat(1, 1f);
            ASME.SetSpatializerFloat(2, 0);
            ASME.SetSpatializerFloat(3, 12);
            ASME.SetSpatializerFloat(4, 1000f);
            ASME.SetSpatializerFloat(5, 1f);

            MissileExplo.AddComponent<MakeAudioSourceFixedPitch>();
        }
        public void initMissile()
        {
            try
            {
                Destroy(Missile);
            }
            catch { }
            Missile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Missile.SetActive(false);

            Destroy(Missile.GetComponent<SphereCollider>());
            Missile.layer = 1;
            Missile.name = "Missile";


            Missile.GetComponent<MeshFilter>().mesh = ModResource.GetMesh("Missile").Mesh;
            Renderer renderer1 = Missile.GetComponent<Renderer>();

            matArray[0] = renderer1.material;
            matArray[0].mainTexture = ModResource.GetTexture("Missile Texture").Texture;
            matArray[0].SetColor("_Color", new Color(1, 1, 1, 1));
            matArray[1] = new Material(Shader.Find("Particles/Additive"));
            matArray[1].mainTexture = ModResource.GetTexture("Missile Light").Texture;
            matArray[1].SetColor("_TintColor", new Color(1, 1, 1, 1));

            renderer1.materials = matArray;

            Rigidbody Mbody = Missile.AddComponent<Rigidbody>();
            Mbody.mass = 0.5f;
            Mbody.freezeRotation = false;
            Mbody.useGravity = false;
            Mbody.interpolation = RigidbodyInterpolation.Interpolate;

            TrailRenderer = Missile.GetComponent<TrailRenderer>() ?? Missile.AddComponent<TrailRenderer>();
            TrailRenderer.autodestruct = false;
            TrailRenderer.receiveShadows = false;
            TrailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;


            TrailRenderer.startWidth = 0.5f;
            TrailRenderer.endWidth = 0.5f;

            TrailRenderer.material = new Material(Shader.Find("Particles/Additive"));
            TrailRenderer.material.mainTexture = ModResource.GetTexture("Missile trail").Texture;


            TrailEnable = true;
            TrailLength = 0.5f;

            MissileSpot = new GameObject("MissileSpot");
            MissileSpot.transform.SetParent(Missile.transform);
            MissileSpot.transform.localPosition = Vector3.zero;
            MissileSpot.transform.localScale = Vector3.one;

            missileSpotPS = MissileSpot.GetComponent<ParticleSystem>() ?? MissileSpot.AddComponent<ParticleSystem>();
            missileSpotPSR = MissileSpot.GetComponent<ParticleSystemRenderer>();
            missileSpotPS.loop = true;

            ParticleSystem.ShapeModule missileSpotSM = missileSpotPS.shape;
            missileSpotSM.shapeType = ParticleSystemShapeType.Cone;
            missileSpotSM.radius = 0f;
            missileSpotSM.angle = 0f;

            missileSpotPS.scalingMode = ParticleSystemScalingMode.Shape;

            missileSpotPS.startDelay = 0f;
            missileSpotPS.startLifetime = 0.2f;
            missileSpotPS.startSpeed = 0f;
            missileSpotPS.startSize = 1f;
            missileSpotPS.randomSeed = 0;
            missileSpotPS.simulationSpace = ParticleSystemSimulationSpace.Local;
            missileSpotPS.gravityModifier = 0f;

            missileSpotPSR.material = new Material(Shader.Find("Particles/Additive"));
            missileSpotPSR.material.mainTexture = ModResource.GetTexture("Glow").Texture;
            missileSpotPSR.material.SetColor("_TintColor", new Color(1, 1, 1, 1));
            missileSpotPSR.renderMode = ParticleSystemRenderMode.Billboard;

            ParticleSystem.EmissionModule missileSpotEM = missileSpotPS.emission;
            missileSpotEM.enabled = true;
            missileSpotEM.rate = 10f;
            missileSpotPS.maxParticles = 2;

            ParticleSystem.SizeOverLifetimeModule missileSOLTM = missileSpotPS.sizeOverLifetime;
            AnimationCurve animationCurve = new AnimationCurve();
            animationCurve.AddKey(0f, 1f);
            animationCurve.AddKey(1f, 0.5f);
            missileSOLTM.size = new ParticleSystem.MinMaxCurve(1f, animationCurve);
            missileSOLTM.enabled = true;

            COLTmissileSpot = missileSpotPS.colorOverLifetime;
            COLTmissileSpot.enabled = true;

            ML = ModResource.GetAudioClip("Missile Launched").AudioClip;

            AudioSource ASML = Missile.AddComponent<AudioSource>();
            ASML.clip = ML;
            ASML.spatialBlend = 1.0f;
            ASML.volume = 1.0f;

            ASML.SetSpatializerFloat(1, 1f);
            ASML.SetSpatializerFloat(2, 0);
            ASML.SetSpatializerFloat(3, 12);
            ASML.SetSpatializerFloat(4, 1000f);
            ASML.SetSpatializerFloat(5, 1f);

            Missile.AddComponent<MakeAudioSourceFixedPitch>();
        }
        public override void SafeAwake()
        {
            launch = AddKey("发射", "launch", KeyCode.X);
            picker = AddKey("手动锁定", "picker", KeyCode.None);
            resetPicker = AddKey("重置手动锁定", "reset", KeyCode.None);
            RGB = AddToggle("R G B!", "rgb", false);
            IFF = AddToggle("开启友伤", "IFF", true);
            Reverse = AddToggle("反向轮转", "Reverse", false);
            safetyDelay = AddSlider("安全延时", "delay", 0.1f, 0.0f, 1.0f);
            diffuseAngle= AddSlider("扩散角", "diffuseAngle", 0f, -90f, 90f);
            ShootCDS = AddSlider("CD", "cooldown", MinCD, MinCD, MaxCD);
            missileSpeed = AddSlider("飞行速度", "mspeed", 1.0f, 0.0f, 10.0f);
            missileCharge = AddSlider("伤害", "charge", 1.0f, 0.0f, 10.0f);
            missileRadiu = AddSlider("范围", "radiu", 1.0f, 0.0f, 10.0f);
            traceAngleS = AddSlider("追踪角度", "traceAngle", 5f, MinAngle, MaxAngle);
            lifeTime = AddSlider("飞行时间", "lifeTime", 4.0f, MinLT, MaxLT);
            //Mask = AddSlider("mask", "mask", 0, 0, 100);
            colourSliderDif = AddColourSlider("炮身颜色", "colorDif", new Color(1, 1, 1, 1), false);
            colourSlider = AddColourSlider("高亮颜色", "color", new Color(1, 0, 0, 1), false);

            initLauncher();
            initGunfire();
            initMissile();
            initExplo();

            Aicon = ModResource.GetTexture("Missile Icon").Texture;

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
            BlockBehaviour.name = "Missile Launcher";
        }

        public override void BuildingUpdate()
        {
            
            if (renderer2.materials.Length < 2)
                renderer2.materials = matArrayLauncher;

            picker.DisplayInMapper = resetPicker.DisplayInMapper = TurretController.Instance.directorMode;
            if (BlockBehaviour.Guid.GetHashCode() != 0 && BlockBehaviour.Guid.GetHashCode() != MyGUID)
                MyGUID = BlockBehaviour.Guid.GetHashCode();
        }
        public void Start()
        {
            pickedCollider = null;
            diffuseAngle.ApplyValue();
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
            MissileColor = TrailColor = colourSlider.Value;
            renderer2.materials = matArrayLauncher;
            gradient.SetKeys(new GradientColorKey[]
            {
                new GradientColorKey(colourSlider.Value,0f),
                new GradientColorKey(Color.black,1f),
            }, new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
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
            COLT.color = gradient;
            COLTgunFire = gunFirePS.colorOverLifetime;
            COLTgunFire.color = gunFireGradient;
            COLTmissileSpot = missileSpotPS.colorOverLifetime;
            COLTmissileSpot.color = gradient;

            existTime = lifeTime.Value;
            ShootCD = Mathf.Max(0.05f, ShootCDS.Value);
            traceAngle = traceAngleS.Value;
            charge = missileCharge.Value;
            Tspeed = 100f * missileSpeed.Value;

            ParticleSystem.SizeOverLifetimeModule SOLTM = PS.sizeOverLifetime;
            AnimationCurve animationCurve = new AnimationCurve();
            animationCurve.AddKey(0f, 1f * missileRadiu.Value);
            animationCurve.AddKey(0.8f, 5f * missileRadiu.Value);
            animationCurve.AddKey(1f, 4f * missileRadiu.Value);
            SOLTM.size = new ParticleSystem.MinMaxCurve(1f, animationCurve);
            SOLTM.enabled = true;

            float mscale = Mathf.Min(BlockBehaviour.transform.localScale.x, BlockBehaviour.transform.localScale.z);
            Missile.transform.localScale = new Vector3(mscale, mscale, mscale);
            gunFirePS.startSize = mscale;
            missileSpotPS.startSize = mscale * 2f;
        }
        public override void OnSimulateStart()
        {
            if (!StatMaster.isHosting)
                MissileController.Instance.MD[BlockBehaviour.ParentMachine.PlayerID].Add(MyGUID, new List<int>());
        }
        public override void OnSimulateStop()
        {
            for (int i = 0; i < n.Count; i++)
            {
                if (n[i])
                    Destroy(n[i]);
            }
            n.Clear();
            MissileController.Instance.MD[BlockBehaviour.ParentMachine.PlayerID].Remove(MyGUID);

        }
        private void Update()
        {
            if (BlockBehaviour.isSimulating)
            {
                if (RGB.IsActive)
                {
                    LauncherColor = RGBController.Instance.outputColor;
                }
                if (TurretController.Instance.directorMode)
                {
                    if (picker.IsPressed)
                    {
                        StartCoroutine(GetAim());
                    }
                    if (resetPicker.IsPressed)
                    {
                        pickedCollider = null;
                    }
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
        private IEnumerator CLShoot(int code)
        {
            float posoffset = 1f;
            if (Reverse.IsActive)
                posoffset = -1f;

            posoffset = posoffset * (n.Count % 3 - 1);
            Vector3 thisPosOffset = gunFireBody.transform.localPosition;
            thisPosOffset.x = posoffset * 0.32f;
            Vector3 thisRotOffset = new Vector3(90f - diffuseAngle.Value * posoffset, 90f, 0f);
            gunFireBody.transform.localPosition = thisPosOffset;
            gunFireBody.transform.localRotation = Quaternion.Euler(thisRotOffset);
            n.Add((GameObject)Instantiate(Missile, gunFireBody.transform.position, gunFireBody.transform.rotation));
            gunFireBody.SetActive(true);
            StartCoroutine(SetActiveByTime(0.05f, gunFireBody));


            int ccount = n.Count - 1;
            n[ccount].SetActive(true);
            n[ccount].isStatic = false;
            

            Rigidbody nbody = n[ccount].GetComponent<Rigidbody>();
            //AudioSource ASML = n[ccount].GetComponent<AudioSource>();
            //ASML.Play();

            nbody.useGravity = false;
            //MissileController.Instance.MP.Add(code, nbody.transform);

            float i = 0f;

            StartCoroutine(SetActiveByTime(existTime, n[ccount]));
            for (; i < existTime; i += Time.fixedDeltaTime)
            {
                if (!n[ccount].activeSelf)
                    yield break;
                //ASML.pitch = Time.timeScale;
                nbody.velocity = n[ccount].transform.forward * Tspeed;
                yield return new WaitForSeconds(Time.fixedDeltaTime);


                //nbody.transform.position = MissileController.Instance.MP[code].position;
                //nbody.transform.eulerAngles = MissileController.Instance.MP[code].position;

                if (i < safetyDelay.Value)
                    continue;
                if (TurretController.Instance.camBindA[BlockBehaviour.ParentMachine.PlayerID])
                {
                    StartCoroutine(SetAim(nbody, TurretController.Instance.aimpos[BlockBehaviour.ParentMachine.PlayerID]));
                }

                if (MissileController.Instance.ME.Contains(code))
                {
                    MissileController.Instance.ME.Remove(code);
                    Vector3 expos = nbody.transform.position;
                    Quaternion exrot = nbody.transform.rotation;
                    n[ccount].SetActive(false);
                    //MissileController.Instance.MP.Remove(code);
                    try
                    {
                        StartCoroutine(PlayExplo(expos));
                    }
                    catch { }
                    yield break;
                }

            }
            yield break;
        }
        private IEnumerator GetAim()
        {
            RaycastHit hitCol;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitCol, 1000f, Game.BlockEntityLayerMask, QueryTriggerInteraction.Ignore)) 
            {
                pickedCollider = hitCol.collider;
            }
            yield break;
        }
        private IEnumerator SetAim(Rigidbody tgtBody,Vector3 tgtPos)
        {
            Vector3 tgtDir = tgtPos - tgtBody.transform.position;
            float tgtangle = Mathf.Abs(Vector3.Angle(tgtBody.transform.forward, tgtDir));
            if (tgtangle < traceAngle)
                tgtBody.transform.LookAt(tgtPos);
            else
                tgtBody.transform.rotation = (Quaternion.Lerp(tgtBody.transform.rotation, Quaternion.LookRotation(tgtDir, tgtBody.transform.up), traceAngle / tgtangle));

            yield break;


        }
        private IEnumerator PlayExplo(Vector3 pos)
        {
            GameObject bomb= (GameObject)Instantiate(MissileExplo);
            bomb.transform.position = pos;
            bomb.SetActive(true);
            Destroy(bomb, PS.startLifetime);

            yield break;
        }
        private IEnumerator SetActiveByTime(float time,GameObject tgt)
        {
            yield return new WaitForSeconds(time);
            tgt.SetActive(false);
        }
        private IEnumerator Shoot()
        {
            float posoffset = 1f;
            if (Reverse.IsActive)
                posoffset = -1f;
            //n.Add((GameObject)Instantiate(Missile, BlockBehaviour.transform.position - BlockBehaviour.transform.up * (BlockBehaviour.transform.localScale.y * 0.4f) + BlockBehaviour.transform.forward * 0.3f* BlockBehaviour.transform.localScale.z + BlockBehaviour.transform.right * 0.4f* BlockBehaviour.transform.localScale.x * posoffset * (n.Count % 3 - 1), Quaternion.LookRotation(-BlockBehaviour.transform.up, BlockBehaviour.transform.forward)));
            posoffset = posoffset * (n.Count % 3 - 1);
            Vector3 thisPosOffset = gunFireBody.transform.localPosition;
            thisPosOffset.x = posoffset * 0.32f;
            Vector3 thisRotOffset = new Vector3(90f - diffuseAngle.Value * posoffset, 90f, 0f);
            gunFireBody.transform.localPosition = thisPosOffset;
            gunFireBody.transform.localRotation = Quaternion.Euler(thisRotOffset);
            n.Add((GameObject)Instantiate(Missile, gunFireBody.transform.position, gunFireBody.transform.rotation));
            gunFireBody.SetActive(true);
            StartCoroutine(SetActiveByTime(0.05f, gunFireBody));
            int ccount = n.Count - 1;

            //n[ccount].transform.RotateAround(n[ccount].transform.position, n[ccount].transform.up, (ccount % 3 - 1) * diffuseAngle.Value);
            if (StatMaster.isMP)
            {
                Message missileLaunched = MissileLaunched.CreateMessage((int)MyGUID, (int)n[ccount].GetHashCode(), (int)BlockBehaviour.ParentMachine.PlayerID);
                ModNetworking.SendToAll(missileLaunched);
            }

            n[ccount].SetActive(true);
            n[ccount].isStatic = false;

            Rigidbody nbody = n[ccount].GetComponent<Rigidbody>();
            //AudioSource ASML = n[ccount].GetComponent<AudioSource>();
            //ASML.Play();
            float i = 0f;
            StartCoroutine(SetActiveByTime(existTime, n[ccount]));
            //Destroy(n[ccount], existTime);

            int thiscount = MissileController.Instance.missileCount;
            MissileController.Instance.missileCount++;
            for (; i < existTime; i += Time.fixedDeltaTime)
            {
                if (!n[ccount].activeSelf)
                    yield break;
                //ASML.pitch = Time.timeScale;
                nbody.velocity = n[ccount].transform.forward * Tspeed;
                yield return new WaitForSeconds(Time.fixedDeltaTime);


                if (i < safetyDelay.Value)
                    continue;
                //ModNetworking.SendToAll(MissilePosAndRot.CreateMessage((int)n[ccount].GetHashCode(),(Vector3)nbody.transform.position, (Vector3)nbody.transform.eulerAngles));
                if (!TurretController.Instance.directorMode)
                {
                    if (TurretController.Instance.camBindA[BlockBehaviour.ParentMachine.PlayerID])
                    {
                        StartCoroutine(SetAim(nbody, TurretController.Instance.aimpos[BlockBehaviour.ParentMachine.PlayerID]));
                    }
                }
                else
                {
                    if(pickedCollider!=null)
                    {
                        StartCoroutine(SetAim(nbody, pickedCollider.transform.position));
                    }
                }
                if (Physics.CheckSphere(nbody.transform.position, 1f, Game.BlockEntityLayerMask, QueryTriggerInteraction.Ignore)|| MissileController.Instance.missileCount - thiscount > countlimit)
                {
                    //Physics.sphere
                    Vector3 expos = nbody.transform.position;
                    Quaternion exrot = nbody.transform.rotation;
                    n[ccount].SetActive(false);
                    if (StatMaster.isMP)
                    {
                        Message missileExploded = MissileExplode.CreateMessage((int)n[ccount].GetHashCode());
                        ModNetworking.SendToAll(missileExploded);
                    }
                    try
                    {

                        StartCoroutine(PlayExplo(expos));
                    }
                    catch { }
                    /*
                    try
                    {
                        //GameObject Bomb = (GameObject)Instantiate(PrefabMaster.LevelPrefabs[levelBombCategory].GetValue(levelBombID).gameObject, expos, exrot);
                        //ExplodeOnCollide bombControl = Bomb.GetComponent<ExplodeOnCollide>();
                        //Bomb.transform.localScale = Vector3.one * charge;
                        ExplodeOnCollide bombControl = PrefabMaster.LevelPrefabs[levelBombCategory].GetValue(levelBombID).gameObject.GetComponent<ExplodeOnCollide>();
                        bombControl.transform.localScale= Vector3.one;
                        bombControl.transform.position = expos;
                        
                        bombControl.radius = radius * charge;
                        bombControl.power = power;
                        bombControl.torquePower = 0;
                        bombControl.upPower = 0;

                        bombControl.Explodey();
                    }
                    catch { }
                    */
                    if (MissileController.Instance.missileCount - thiscount > countlimit) 
                        yield break;
                    Collider[] hits = Physics.OverlapSphere(expos, radius * missileRadiu.Value, Game.BlockEntityLayerMask, QueryTriggerInteraction.Ignore);
                    int index = 0;
                    int rank = 5;
                    if (hits.Length <= 0)
                        yield break;
                    List<int> hitblocks = new List<int>();
                    foreach (var hit in hits)
                    {
                        if (index > rank)
                        {
                            index = 0;
                            yield break;
                        }
                        try
                        {
                            hit.attachedRigidbody.WakeUp();
                            BlockBehaviour hitedblock = hit.attachedRigidbody.gameObject.GetComponent<BlockBehaviour>();
                            if (!hitblocks.Contains(hitedblock.gameObject.GetHashCode()))
                            {
                                try
                                {
                                    if (!IFF.IsActive)
                                    {
                                        if (hitedblock.Team == BlockBehaviour.Team)
                                        {
                                            if (hitedblock.Team != MPTeam.None)
                                                continue;
                                            else if (hitedblock.ParentMachine.PlayerID == BlockBehaviour.ParentMachine.PlayerID)
                                                continue;
                                        }
                                    }
                                }
                                catch { }
                                hitblocks.Add(hitedblock.gameObject.GetHashCode());
                                try
                                {
                                    hit.attachedRigidbody.AddExplosionForce(power * charge, expos, radius * missileRadiu.Value, upPower * charge);
                                }
                                catch { }
                                try
                                {
                                    if (hitedblock.BlockID == 11)
                                    {
                                        if (hitedblock.blockJoint.breakForce > 50000f)
                                        {
                                            hitedblock.blockJoint.breakForce = 50000f;
                                        }
                                        else
                                        {
                                            hitedblock.blockJoint.breakForce -= forceDamage * charge;
                                            if (hitedblock.blockJoint.breakForce < 0)
                                                hitedblock.blockJoint.breakForce = 0;
                                        }
                                    }
                                }
                                catch { }
                                try
                                {
                                    hitedblock.BlockHealth.DamageBlock(blockDamage);
                                }
                                catch { }
                                try
                                {
                                    hit.attachedRigidbody.gameObject.GetComponent<BreakOnForce>().BreakExplosion(power * charge, expos, radius * missileRadiu.Value, upPower * charge);
                                }
                                catch { }

                                index++;
                            }
                            continue;
                        }
                        catch { }
                        try
                        {
                            hit.attachedRigidbody.WakeUp();
                            try
                            {
                                hit.attachedRigidbody.gameObject.GetComponent<BreakOnForce>().BreakExplosion(power * charge, expos, radius * missileRadiu.Value, upPower * charge);
                            }
                            catch { }
                            try
                            {
                                hit.attachedRigidbody.AddExplosionForce(power * charge, expos, radius * missileRadiu.Value, upPower * charge);
                            }
                            catch { }
                            index++;
                            continue;
                        }
                        catch { }
                    }
                    yield break;
                }

            }
            yield break;
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
                    StartCoroutine(Shoot());
                }
            }
            catch { }
        }
        public override void SimulateFixedUpdateClient()
        {
            if (MissileController.Instance.MD[BlockBehaviour.ParentMachine.PlayerID][MyGUID].Count > 0) 
            {
                int thiscode = (MissileController.Instance.MD[BlockBehaviour.ParentMachine.PlayerID][MyGUID][0]);
                StartCoroutine(CLShoot(thiscode));
                MissileController.Instance.MD[BlockBehaviour.ParentMachine.PlayerID][MyGUID].RemoveAt(0);
            }
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

            //foreach(var a in BlockBehaviour.gameObject.GetComponentsInChildren<Collider>())
            {
             //   showmsg += a.enabled + ",";
            }
            /*
            string showmsg = "";
            showmsg += pickedCollider.transform.position;
            GUI.Box(UIrect, showmsg.ToString());
            */
            if (TurretController.Instance.directorMode&&!StatMaster.hudHidden)
            {
                if(pickedCollider!=null)
                {
                    try
                    {
                        GUI.color = colourSlider.Value;
                        Vector3 onScreenPosition = Camera.main.WorldToScreenPoint(pickedCollider.transform.position);
                        if (onScreenPosition.z >= 0)
                            GUI.DrawTexture(new Rect(onScreenPosition.x - iconSize / 2, Camera.main.pixelHeight - onScreenPosition.y - iconSize / 2, iconSize, iconSize), Aicon);
                    }
                    catch { }
                }
            }

        }
        
    }
}
