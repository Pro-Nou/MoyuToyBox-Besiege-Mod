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
    class TorpedoController : SingleInstance<TorpedoController>
    {
        public override string Name { get; } = "Torpedo Controller";

        public Dictionary<int, List<int>>[] TD=new Dictionary<int, List<int>>[16];
        public Dictionary<int,int>[] TL=new Dictionary<int, int>[16];
        public TorpedoController()
        {
            for(int i=0;i<16;i++)
            {
                TD[i] = new Dictionary<int, List<int>>();
                TL[i] = new Dictionary<int, int>();
            }
        }

    }
    public class printExploShader : MonoBehaviour
    {
        private Rect UIrect = new Rect(0, 100, 512, 256);
        readonly GUIStyle vec3Style = new GUIStyle()
        {
            fontStyle = FontStyle.Bold,
            fontSize = 16,
            normal = { textColor = new Color(1, 1, 1, 1) },
            alignment = TextAnchor.MiddleCenter,
        };
        string printShaderValues()
        {
            string output = "";
            Material exploMaterial = base.GetComponent<MeshRenderer>().material;
            output += exploMaterial.GetTexture("_RampTex").ToString() + "\n";
            output += exploMaterial.GetTexture("_MainTex").ToString() + "\n";
            output += exploMaterial.GetFloat("_Heat").ToString() + "\n";
            output += exploMaterial.GetFloat("_Radius").ToString() + "\n";
            output += exploMaterial.GetFloat("_Frequency").ToString() + "\n";
            output += exploMaterial.GetFloat("_ScrollSpeed").ToString() + "\n";
            output += exploMaterial.GetFloat("_Alpha").ToString() + "\n";
            output += exploMaterial.GetFloat("_Timing").ToString() + "\n";
            return output;
        }
        
        void OnGUI()
        {
            string showmsg = "";
            try
            {
                showmsg += printShaderValues();
                GUI.DrawTexture(new Rect(0,0,128,128), base.GetComponent<MeshRenderer>().material.GetTexture("_MainTex"));

            }
            catch { }
            GUI.Box(UIrect, showmsg);
        }
    }
    public class MyExplosionMat : MonoBehaviour
    {
        // Token: 0x060044A7 RID: 17575 RVA: 0x0018635C File Offset: 0x0018455C
        private void Start()
        {
            this.ExplosionMaterial = base.GetComponent<MeshRenderer>().material;
            this.UpdateShaderProperties();
        }

        // Token: 0x060044A8 RID: 17576 RVA: 0x00186378 File Offset: 0x00184578
        private void Update()
        {
            if (this.doUpdate)
            {
                //Material exploMaterial = base.GetComponent<MeshRenderer>().material;
                float num = Mathf.Min(base.transform.lossyScale.x, Mathf.Min(base.transform.lossyScale.y, base.transform.lossyScale.z));
                if (num != this._radius)
                {
                    this._radius = num;
                    ExplosionMaterial.SetFloat("_Radius", this._radius / 2.03f - 2f);
                }
                /*
                if (this.useheat != this._heat)
                {
                    this.useheat = this._heat;
                    ExplosionMaterial.SetFloat("_Heat", this._heat);
                }
                if (this.usealpha != this._alpha)
                {
                    this.usealpha = this._alpha;
                    ExplosionMaterial.SetFloat("_Alpha", this._alpha);
                }
                if (this.usescroll != this._scrollSpeed)
                {
                    this.usescroll = this._scrollSpeed;
                    ExplosionMaterial.SetFloat("_ScrollSpeed", this._scrollSpeed);
                }
                if (this.usefreq != this._frequency)
                {
                    this.usefreq = this._frequency;
                    ExplosionMaterial.SetFloat("_Frequency", this._frequency);
                }
                if (this.usescatter != this._scattering || this.useoctaves != this._octaves || this.usequality != this._quality)
                {
                    this.usescatter = this._scattering;
                    this.useoctaves = this._octaves;
                    this.usequality = this._quality;
                    this.SetShaderKeywords();
                }
                */
            }
        }

        // Token: 0x060044A9 RID: 17577 RVA: 0x00186528 File Offset: 0x00184728
        public void UpdateShaderProperties()
        {
            Material exploMaterial = base.GetComponent<MeshRenderer>().material;
            exploMaterial.SetTexture("_RampTex", this._ramp);
            exploMaterial.SetTexture("_MainTex", this._noise);
            exploMaterial.SetFloat("_Heat", this._heat);
            exploMaterial.SetFloat("_Alpha", this._alpha);
            exploMaterial.SetFloat("_ScrollSpeed", this._scrollSpeed);
            exploMaterial.SetFloat("_Frequency", this._frequency);
            this.SetShaderKeywords();
        }

        // Token: 0x060044AA RID: 17578 RVA: 0x001865B0 File Offset: 0x001847B0
        private void SetShaderKeywords()
        {
            List<string> list = new List<string>
        {
            (!this._scattering) ? "SCATTERING_OFF" : "SCATTERING_ON",
            "OCTAVES_" + this._octaves,
            this.qualitySel[this._quality]
        };
            base.GetComponent<MeshRenderer>().material.shaderKeywords = list.ToArray();
        }

        // Token: 0x04004003 RID: 16387
        private string[] qualitySel = new string[]
        {
        "QUALITY_LOW",
        "QUALITY_MED",
        "QUALITY_HIGH"
        };
        
        // Token: 0x04004004 RID: 16388
        private bool doUpdate = true;

        // Token: 0x04004005 RID: 16389
        private float _radius = 1f;

        // Token: 0x04004006 RID: 16390
        public Texture2D _ramp;

        // Token: 0x04004007 RID: 16391
        public Texture2D _noise;

        // Token: 0x04004008 RID: 16392
        public Material ExplosionMaterial;

        // Token: 0x04004009 RID: 16393
        public float _heat = 1f;

        // Token: 0x0400400A RID: 16394
        private float useheat = 1f;

        // Token: 0x0400400B RID: 16395
        public float _alpha = 1f;

        // Token: 0x0400400C RID: 16396
        private float usealpha = 1f;

        // Token: 0x0400400D RID: 16397
        public float _scrollSpeed = 1f;

        // Token: 0x0400400E RID: 16398
        private float usescroll = 1f;

        // Token: 0x0400400F RID: 16399
        public float _frequency = 1f;

        // Token: 0x04004010 RID: 16400
        private float usefreq = 1f;

        // Token: 0x04004011 RID: 16401
        public bool _scattering = true;

        // Token: 0x04004012 RID: 16402
        private bool usescatter = true;

        // Token: 0x04004013 RID: 16403
        public int _octaves = 4;

        // Token: 0x04004014 RID: 16404
        private int useoctaves = 4;

        // Token: 0x04004015 RID: 16405
        public int _quality = 2;

        // Token: 0x04004016 RID: 16406
        private int usequality = 2;
    }

    class TorpedoExploEffect : MonoBehaviour
    {
        private float ExistedTime=0f;
        public float ScaleSize = 1f;
        public float lifetime = 1f;
        public Material shockwaveMaterial;
        public Material ExplosionMaterial;
        void Start()
        {
            ExistedTime = 0f;
            ScaleSize = Mathf.Max(ScaleSize, 0.2f);
            this.transform.localScale = Vector3.zero;
            this.ExplosionMaterial = base.GetComponent<MeshRenderer>().material;
            shockwaveMaterial = this.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material;
            //shockwave.transform.SetParent(this.transform.parent);
            //Destroy(shockwave, 0.6f * lifetime);
        }
        void Update()
        {
            ExistedTime += Time.deltaTime;
            float fxoffset = Mathf.Min(1f, Mathf.Max(0f, ExistedTime / lifetime)) * Mathf.PI / 2f;
            //if (ExistedTime <= lifetime * 0.6f)
            //    shockwave.transform.localScale = Vector3.one * 20f * Mathf.Lerp(0f, ScaleSize * 2, ExistedTime / (lifetime * 0.6f));
            ExplosionMaterial.SetFloat("_Alpha", Mathf.Cos(fxoffset));
            shockwaveMaterial.SetFloat("_Refraction", Mathf.Lerp(10f, 1f, fxoffset));
            this.transform.localScale = Vector3.one * 20f * Mathf.Lerp(0.2f, ScaleSize, Mathf.Sin(fxoffset));
           }
    }
    class TorpedoLauncher : BlockScript
    {
        private readonly int levelBombCategory = 4;
        private readonly int levelBombID = 5001;


        private float radius = 10f;
        private float power = 20000f;
        private float torquePower = 20000f;
        private float upPower = 0f;
        private float blockDamage = 3.0f;
        private float forceDamage = 20000f;
        private float Tspeed = 70f;


        private float charge= 1.0f;

        private float existTime = 10.0f;
        private float ShootCD = 15.0f;
        private float minCD = 5f;
        private MSlider ShootCDS;
        private int CLoffset = 0;

        public int MyGUID ;
        private Rect UIrect = new Rect(0, 100, 512, 128);
        private int iconSize = 64;
        private Texture Ticon;
        private MKey launch;
        public MSlider safetyDelay;
        public MSlider torpedoSpeed;
        public MSlider torpedoCharge;
        public MSlider torpedoRadiu;
        //public MSlider Mask;
        public MColourSlider colourSlider;
        public MColourSlider colourSliderDif;
        private MMenu Ttype;
        private MToggle Icon;
        private MToggle HasTrail;
        private MToggle RGB;
        private MToggle IFF;

        //private bool isLaunched = false;
        public bool ShootEnabled = true;
        public GameObject torpedo;
        public GameObject plasmaTorpedo;
        public GameObject plasmaPSBody;
        public GameObject myExplo;
        //public ExplosionMat myExploMat;
        public MyExplosionMat myExploMat;
        public MyExplosionMat myExploMat2;
        public TorpedoExploEffect torpedoEffect;
        public List<GameObject> n=new List<GameObject>();

        private TrailRenderer TrailRenderer;
        public bool TrailEnable { get { return TrailRenderer.enabled; } set { TrailRenderer.enabled = value; } }
        public float TrailLength { get { return TrailRenderer.time; } set { TrailRenderer.time = value; } }
        public Color TrailColor { get { return TrailRenderer.material.color; } set { TrailRenderer.material.SetColor("_TintColor", value); } }

        private TrailRenderer TrailRenderer2;
        public float TrailLength2 { get { return TrailRenderer2.time; } set { TrailRenderer2.time = value; } }
        public Color TrailColor2 { get { return TrailRenderer2.material.color; } set { TrailRenderer2.material.SetColor("_TintColor", value); } }

        //public ParticleSystemRenderer PSR;
        //public ParticleSystem PS;
        public Material[] matArray = new Material[2];
        public Material[] matArrayLauncher = new Material[2];
        public Color TorpedoColor { get { return matArray[1].color; } set { matArray[1].SetColor("_TintColor", value); } }
        public Color LauncherColor { get { return matArrayLauncher[1].color; } set { matArrayLauncher[1].SetColor("_TintColor", value); } }
        public Color LauncherColorDif { get { return matArrayLauncher[0].color; } set { matArrayLauncher[0].SetColor("_Color", value); } }
        public Color IconColor = new Color(1, 1, 1, 1);


        //public Color PSColor { get { return PS.startColor; } set { PS.startColor = value; } }

        private AudioClip TL;
        private AudioClip TE;
        private AudioClip PL;
        private Texture2D Rtex;
        private Texture2D RtexIN;

        private MeshRenderer renderer2;
        public ParticleSystem PS;
        public ParticleSystemRenderer PSR;

        public bool myClusterCode = false;

        public void initLauncher()
        {
            renderer2 = BlockBehaviour.GetComponentInChildren<MeshRenderer>();
            matArrayLauncher[0] = renderer2.material;
            matArrayLauncher[0].mainTexture = ModResource.GetTexture("TorpedoLauncher Texture").Texture;
            matArrayLauncher[0].SetColor("_Color", new Color(1, 1, 1, 1));
            matArrayLauncher[1] = new Material(Shader.Find("Particles/Additive"));
            matArrayLauncher[1].mainTexture = ModResource.GetTexture("TorpedoLauncher Light").Texture;
            matArrayLauncher[1].SetColor("_TintColor", new Color(1, 1, 1, 1));
            renderer2.materials = matArrayLauncher;

        }
        public void initPlasmaTorpedo()
        {
            try
            {
                Destroy(plasmaTorpedo);
            }
            catch { }
            plasmaTorpedo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            plasmaTorpedo.SetActive(false);
            plasmaTorpedo.transform.localScale = Vector3.one * 4f;
            Destroy(plasmaTorpedo.GetComponent<SphereCollider>());
            //myExplo.AddComponent<printExploShader>();
            MeshRenderer plasmaRenderer = plasmaTorpedo.GetComponent<MeshRenderer>();
            plasmaRenderer.material.shader = Shader.Find("Explosion");
            myExploMat2 = plasmaTorpedo.AddComponent<MyExplosionMat>() ?? plasmaTorpedo.GetComponent<MyExplosionMat>();
            myExploMat2._ramp = ModResource.GetTexture("Expl ramp").Texture;
            myExploMat2._noise = ModResource.GetTexture("Expl noise").Texture;

            Rigidbody Tbody = plasmaTorpedo.AddComponent<Rigidbody>();
            Tbody.mass = 0.5f;
            Tbody.freezeRotation = true;
            Tbody.useGravity = false;
            Tbody.interpolation = RigidbodyInterpolation.Interpolate;

            TrailRenderer2 = plasmaTorpedo.GetComponent<TrailRenderer>() ?? plasmaTorpedo.AddComponent<TrailRenderer>();
            TrailRenderer2.autodestruct = false;
            TrailRenderer2.receiveShadows = false;
            TrailRenderer2.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;


            TrailRenderer2.startWidth = 1.5f;
            TrailRenderer2.endWidth = 1.5f;

            TrailRenderer2.material = new Material(Shader.Find("Particles/Additive"));
            TrailRenderer2.material.mainTexture = ModResource.GetTexture("Torpedo trail").Texture;
            
            TrailLength2 = 1f;

            PL = ModResource.GetAudioClip("Plasma Launched").AudioClip;
            AudioSource ASPL = plasmaTorpedo.GetComponent<AudioSource>() ?? plasmaTorpedo.AddComponent<AudioSource>();
            ASPL.clip = PL;
            ASPL.spatialBlend = 0.98f;
            ASPL.volume = 1.0f;

            ASPL.SetSpatializerFloat(1, 1f);
            ASPL.SetSpatializerFloat(2, 0);
            ASPL.SetSpatializerFloat(3, 12);
            ASPL.SetSpatializerFloat(4, 1000f);
            ASPL.SetSpatializerFloat(5, 1f);
            
            plasmaTorpedo.AddComponent<MakeAudioSourceFixedPitch>();

            plasmaPSBody = new GameObject("plasma PS");
            plasmaPSBody.transform.SetParent(plasmaTorpedo.transform);
            plasmaPSBody.transform.localPosition = Vector3.zero;
            PS = plasmaPSBody.GetComponent<ParticleSystem>() ?? plasmaPSBody.AddComponent<ParticleSystem>();
            PSR = plasmaPSBody.GetComponent<ParticleSystemRenderer>();
            PS.loop = true;

            ParticleSystem.ShapeModule SM = PS.shape;
            SM.shapeType = ParticleSystemShapeType.Sphere;
            SM.radius = 1f;

            ParticleSystem.CollisionModule CM = PS.collision;
            CM.enabled = false;
            PS.scalingMode = ParticleSystemScalingMode.Shape;

            float PSduration = PS.duration;
            PSduration = 1f;
            //PS.loop = true;
            PS.startDelay = 0f;
            PS.startLifetime = 1f;
            PS.startSpeed = 0f;
            PS.startSize = 1f;
            PS.simulationSpace = ParticleSystemSimulationSpace.World;
            PS.gravityModifier = 0f;
            

            PSR.material = new Material(Shader.Find("Particles/Additive"));
            PSR.material.mainTexture = ModResource.GetTexture("Star Glow").Texture;
            PSR.material.SetColor("_TintColor", new Color(1, 1, 1, 1));

            ParticleSystem.EmissionModule EM = PS.emission;
            EM.enabled = true;
            EM.rate = 50;
            PS.maxParticles = 50;

            ParticleSystem.SizeOverLifetimeModule SOLTM = PS.sizeOverLifetime;
            AnimationCurve animationCurve = new AnimationCurve();
            animationCurve.AddKey(0f, 1f);
            animationCurve.AddKey(1f, 0f);
            SOLTM.size = new ParticleSystem.MinMaxCurve(1f, animationCurve);
            SOLTM.enabled = true;
        }
        public void initExplo()
        {
            /*
            myExplo = (GameObject)Instantiate(PrefabMaster.LevelPrefabs[levelBombCategory].GetValue(levelBombID).gameObject);
            ExplodeOnCollide bombControl = myExplo.GetComponent<ExplodeOnCollide>();


            myExploMat = bombControl.explosionEffectPrefab.gameObject.transform.FindChild("PyroclasticPuff").gameObject.GetComponent<ExplosionMat>();
            Destroy(bombControl.explosionEffectPrefab.gameObject.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>());
            Destroy(bombControl.explosionEffectPrefab.gameObject.transform.GetChild(0).gameObject.GetComponent<ParticleSystemRenderer>());
            Destroy(bombControl.explosionEffectPrefab.gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>());
            Destroy(bombControl.explosionEffectPrefab.gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<ParticleSystemRenderer>());
            bombControl.explosionEffectPrefab.gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<Light>().range = 0f;
            bombControl.explosionEffectPrefab.gameObject.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.transform.localScale = Vector3.zero;
            bombControl.explosionEffectPrefab.gameObject.transform.GetChild(1).gameObject.transform.GetChild(2).gameObject.transform.localScale = Vector3.zero;
            Destroy(myExploMat.gameObject.GetComponent<Light>());
            //myExploMat = Instantiate(PrefabMaster.LevelPrefabs[levelBombCategory].GetValue(levelBombID).gameObject.GetComponent<ExplodeOnCollide>().explosionEffectPrefab.gameObject.transform.FindChild("PyroclasticPuff").gameObject.GetComponent<ExplosionMat>().gameObject.transform.FindChild("PyroclasticPuff").gameObject);
            //Destroy(myExploMat.gameObject.transform.FindChild("Light").gameObject);

            */
            /*
            ExplodeOnCollide bombControl = PrefabMaster.LevelPrefabs[levelBombCategory].GetValue(levelBombID).gameObject.GetComponent<ExplodeOnCollide>();
            myExplo = (GameObject)Instantiate(bombControl.explosionEffectPrefab.gameObject.transform.FindChild("PyroclasticPuff").gameObject);
            myExplo.name = "torpedoExplo";
            myExplo.SetActive(false);
            MeshFilter exploMeshRenderer = myExplo.GetComponent<MeshFilter>();
            Mesh exploMesh = Instantiate<Mesh>(exploMeshRenderer.mesh);
            exploMeshRenderer.mesh = Instantiate<Mesh>(exploMesh);
            exploMeshRenderer.sharedMesh = Instantiate<Mesh>(exploMesh);
            myExplo.transform.localScale = Vector3.one * 10f;
            
            Destroy(myExplo.transform.GetChild(4).gameObject);
            Destroy(myExplo.transform.GetChild(2).gameObject);
            Destroy(myExplo.transform.GetChild(1).gameObject);
            Destroy(myExplo.transform.GetChild(0).gameObject);
            torpedoEffect = myExplo.AddComponent<TorpedoExploEffect>() ?? myExplo.GetComponent<TorpedoExploEffect>();
            myExplo.AddComponent<printExploShader>();
            */
            
            try
            {
                Destroy(myExplo);
            }
            catch { }
            myExplo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            myExplo.SetActive(false);
            Destroy(myExplo.GetComponent<SphereCollider>());
            //myExplo.AddComponent<printExploShader>();
            MeshRenderer exploRenderer = myExplo.GetComponent<MeshRenderer>();
            exploRenderer.material.shader = Shader.Find("Explosion");
            myExploMat = myExplo.AddComponent<MyExplosionMat>() ?? myExplo.GetComponent<MyExplosionMat>();
            myExploMat._ramp = ModResource.GetTexture("Expl ramp").Texture;
            myExploMat._noise = ModResource.GetTexture("Expl noise").Texture;
            torpedoEffect = myExplo.AddComponent<TorpedoExploEffect>() ?? myExplo.GetComponent<TorpedoExploEffect>();

            GameObject shockwave= GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shockwave.transform.SetParent(myExplo.transform);
            shockwave.transform.localScale = Vector3.one * 2;
            shockwave.transform.localPosition = Vector3.zero;
            Destroy(shockwave.GetComponent<SphereCollider>());
            MeshRenderer shockwaveRenderer = shockwave.GetComponent<MeshRenderer>();
            shockwaveRenderer.material.shader = Shader.Find("FX/Shockwave/Distortion");
            shockwaveRenderer.material.SetFloat("_Refraction", 10f);

            TE = ModResource.GetAudioClip("Torpedo Explo").AudioClip;
            AudioSource ASTE = myExplo.GetComponent<AudioSource>() ?? myExplo.AddComponent<AudioSource>();
            ASTE.clip = TE;
            ASTE.spatialBlend = 1.0f;
            ASTE.volume = 1.0f;

            ASTE.SetSpatializerFloat(1, 1f);
            ASTE.SetSpatializerFloat(2, 0);
            ASTE.SetSpatializerFloat(3, 12);
            ASTE.SetSpatializerFloat(4, 1000f);
            ASTE.SetSpatializerFloat(5, 1f);

            myExplo.AddComponent<MakeAudioSourceFixedPitch>();
        }
        public void initTorpedo()
        {
            try
            {
                Destroy(torpedo);
            }
            catch { }
            torpedo = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            torpedo.SetActive(false);
            Destroy(torpedo.GetComponent<SphereCollider>());
            torpedo.layer = 1;
            torpedo.name = "Torpedo";


            torpedo.GetComponent<MeshFilter>().mesh = ModResource.GetMesh("Torpedo").Mesh;
            Renderer renderer1 = torpedo.GetComponent<Renderer>();

            matArray[0] = renderer1.material;
            matArray[0].mainTexture = ModResource.GetTexture("Torpedo Texture").Texture;
            matArray[0].SetColor("_Color", new Color(1, 1, 1, 1));
            matArray[1]= new Material(Shader.Find("Particles/Additive"));
            matArray[1].mainTexture = ModResource.GetTexture("Torpedo Light").Texture;
            matArray[1].SetColor("_TintColor", new Color(1, 1, 1, 1));
            
            renderer1.materials = matArray;

            
            
            

            Rigidbody Tbody = torpedo.AddComponent<Rigidbody>();
            Tbody.mass = 0.5f;
            Tbody.freezeRotation = true;
            Tbody.useGravity = false;
            Tbody.interpolation = RigidbodyInterpolation.Interpolate;

            TrailRenderer = torpedo.GetComponent<TrailRenderer>() ?? torpedo.AddComponent<TrailRenderer>();
            TrailRenderer.autodestruct = false;
            TrailRenderer.receiveShadows = false;
            TrailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            
            TrailRenderer.startWidth = 1.5f;
            TrailRenderer.endWidth = 1.5f;

            TrailRenderer.material = new Material(Shader.Find("Particles/Additive"));
            TrailRenderer.material.mainTexture = ModResource.GetTexture("Torpedo trail").Texture;
            
            TrailEnable = true;
            TrailLength = 1f;


            

            TL = ModResource.GetAudioClip("Torpedo Launched").AudioClip;

            AudioSource ASTL = torpedo.AddComponent<AudioSource>();
            ASTL.clip = TL;
            ASTL.spatialBlend = 1.0f;
            ASTL.volume = 1.0f;

            ASTL.SetSpatializerFloat(1, 1f);
            ASTL.SetSpatializerFloat(2, 0);
            ASTL.SetSpatializerFloat(3, 12);
            ASTL.SetSpatializerFloat(4, 1000f);
            ASTL.SetSpatializerFloat(5, 1f);

            torpedo.AddComponent<MakeAudioSourceFixedPitch>();
        }
        
        public override void SafeAwake()
        {

            launch = AddKey("发射", "launch", KeyCode.X);
            Icon = AddToggle("UI提示", "ShowIcon", true);
            HasTrail = AddToggle("启用拖尾", "HasTrail", true);
            RGB = AddToggle("R G B!", "rgb", false);
            IFF = AddToggle("开启友伤", "IFF", true);
            safetyDelay = AddSlider("安全延时", "delay", 0.1f, 0.0f, 1.0f);
            ShootCDS = AddSlider("CD", "cooldown", 1.0f, 1.0f, 10f);
            torpedoSpeed = AddSlider("飞行速度", "speed", 1.0f, 0.0f, 10.0f);
            torpedoCharge = AddSlider("伤害", "charge", 1.0f, 0.0f, 10.0f);
            torpedoRadiu = AddSlider("范围", "radiu", 1.0f, 0.0f, 10.0f);
            //Mask = AddSlider("mask", "mask", 0, 0, 100);
            colourSliderDif = AddColourSlider("炮身颜色", "colorDif", new Color(1, 1, 1, 1), false);
            colourSlider = AddColourSlider("高亮颜色", "color", new Color(1, 0, 0, 1),false);
            List<string> types = new List<string>{ "鱼雷", "电浆球"};
            Ttype = AddMenu("type", 0, types);


            initLauncher();
            initPlasmaTorpedo();
            initTorpedo();
            initExplo();
            Ticon = ModResource.GetTexture("Torpedo Icon").Texture;
            Rtex = new Texture2D(32, 1, TextureFormat.ARGB32, false, false);

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
                        PSR.material.SetColor("_TintColor", colourSlider.Value);
                    }
                    catch { }
                };
            }
            catch { }
            BlockBehaviour.name = "Torpedo Launcher";



            //CB.projectilePlaceholder.ge
            //BlockBehaviour.SimBlock.BlockHealth.health = 20f;
            //CB = GetComponent<CanonBlock>();
            //CB.knockbackSpeed = 0;
            //CB.boltSpeed = 1;

        }
        public override void BuildingUpdate()
        {
            
            if (renderer2.materials.Length < 2)
                renderer2.materials = matArrayLauncher;

            
            if (BlockBehaviour.Guid.GetHashCode() != 0 && BlockBehaviour.Guid.GetHashCode() != MyGUID) 
                MyGUID = BlockBehaviour.Guid.GetHashCode();
        }
        private void Update()
        {
            if(BlockBehaviour.isSimulating)
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
            IconColor = TorpedoColor = TrailColor = TrailColor2 = colourSlider.Value;
            renderer2.materials = matArrayLauncher;
            charge = torpedoCharge.Value;
            Tspeed = 50f*torpedoSpeed.Value;
            ShootCD = Mathf.Max(0.05f, ShootCDS.Value);
            TrailEnable = HasTrail.IsActive;

            float mscale = Mathf.Min(BlockBehaviour.transform.localScale.x, BlockBehaviour.transform.localScale.y);
            torpedo.transform.localScale = new Vector3(mscale, mscale, mscale);
            plasmaTorpedo.transform.localScale = Vector3.one * Mathf.Max(4f, mscale * 6f);
            PS.startSize = mscale;
            ParticleSystem.ShapeModule SM = PS.shape;
            SM.radius = mscale;
            ParticleSystem.EmissionModule EM = PS.emission;
            EM.rate = (int)(50 * torpedoSpeed.Value);
            PS.maxParticles = (int)(50 * torpedoSpeed.Value);

            Color[] Rorg = ModResource.GetTexture("Expl ramp").Texture.GetPixels();
            for (int ri = 0; ri < 32; ri++)
            {
                Rtex.SetPixel(ri, 0, (colourSlider.Value + Color.white * ri / 31f) * Rorg[ri]);
            }
            RtexIN = Instantiate(Rtex);
            //myExploMat = myExplo.GetComponent<ExplosionMat>();
            myExploMat = myExplo.GetComponent<MyExplosionMat>();
            myExploMat2 = plasmaTorpedo.GetComponent<MyExplosionMat>();
            torpedoEffect = myExplo.GetComponent<TorpedoExploEffect>();
            myExploMat._ramp = RtexIN;
            myExploMat2._ramp = RtexIN;
            torpedoEffect.lifetime = 1f;
            torpedoEffect.ScaleSize = torpedoRadiu.Value;

        }
        public override void OnSimulateStart()
        {
            //TurretController.Instance.MyGUID = MyGUID;
            if (!StatMaster.isHosting)
                TorpedoController.Instance.TD[BlockBehaviour.ParentMachine.PlayerID].Add(MyGUID, new List<int>());
        }
        public override void OnSimulateStop()
        {
            for (int i = 0; i < n.Count; i++)
            {
                if(n[i])
                    Destroy(n[i]);
            }
            n.Clear();
            //TurretController.Instance.MyGUID = 0;
            TorpedoController.Instance.TD[BlockBehaviour.ParentMachine.PlayerID].Remove(MyGUID);

        }
        private IEnumerator displayExplo(Vector3 expos, Quaternion exrot)
        {
            GameObject Bomb = (GameObject)Instantiate(myExplo, expos, exrot);
            //ExplodeOnCollide bombControl = Bomb.GetComponent<ExplodeOnCollide>();
            //Bomb.transform.localScale = Vector3.one * torpedoRadiu.Value * 10f;
            /*
            bombControl.radius = radius * torpedoRadiu.Value;
            bombControl.power = power;
            bombControl.torquePower = 0;
            bombControl.upPower = 0;
            */
            //ExplosionMat bombMat0 = bombControl.explosionEffectPrefab.gameObject.transform.GetChild(1).gameObject.GetComponent<ExplosionMat>();
            //bombControl.Explodey();
            Bomb.SetActive(true);
            Destroy(Bomb, 1f);
            yield break;
        }
        private IEnumerator CLShoot()
        {
            int ccount = TorpedoController.Instance.TL[BlockBehaviour.ParentMachine.PlayerID][MyGUID];
            TorpedoController.Instance.TL[BlockBehaviour.ParentMachine.PlayerID].Remove(MyGUID);

            if (n.Count == 0 && ccount > 0)
                CLoffset = ccount;
            switch(Ttype.Value)
            {
                case 0:
                    {
                        n.Add((GameObject)Instantiate(torpedo, BlockBehaviour.transform.position + BlockBehaviour.transform.forward * (BlockBehaviour.transform.localScale.z), BlockBehaviour.transform.rotation));
                        break;
                    }
                case 1:
                    {
                        n.Add((GameObject)Instantiate(plasmaTorpedo, BlockBehaviour.transform.position + BlockBehaviour.transform.forward * (BlockBehaviour.transform.localScale.z), BlockBehaviour.transform.rotation));
                        break;
                    }
                default:
                    {
                        n.Add((GameObject)Instantiate(torpedo, BlockBehaviour.transform.position + BlockBehaviour.transform.forward * (BlockBehaviour.transform.localScale.z), BlockBehaviour.transform.rotation));
                        break;
                    }
            }
            

            
            n[ccount - CLoffset].SetActive(true);
            n[ccount - CLoffset].isStatic = false;
            
            Rigidbody nbody = n[ccount - CLoffset].GetComponent<Rigidbody>();

            //n[ccount - CLoffset].GetComponent<ParticleSystem>().Play();
            //AudioSource ASTL = n[ccount - CLoffset].GetComponent<AudioSource>();
            //ASTL.Play();
            float i = 0f;
            
            StartCoroutine(SetActiveByTime(existTime, n[ccount - CLoffset]));

            for (; i < existTime; i += Time.fixedDeltaTime)
            {
                if (!n[ccount - CLoffset].activeSelf)
                    yield break;
                //ASTL.pitch = Time.timeScale;
                nbody.velocity = n[ccount - CLoffset].transform.forward * Tspeed;
                yield return new WaitForSeconds(Time.fixedDeltaTime);



                if (TorpedoController.Instance.TD[BlockBehaviour.ParentMachine.PlayerID][MyGUID].Contains(ccount))
                {
                    TorpedoController.Instance.TD[BlockBehaviour.ParentMachine.PlayerID][MyGUID].Remove(ccount);
                    Vector3 expos = nbody.transform.position;
                    Quaternion exrot = nbody.transform.rotation;
                    n[ccount - CLoffset].SetActive(false);

                    try
                    {
                        StartCoroutine(displayExplo(expos, exrot));
                    }
                    catch { }
                    yield break;
                }

            }
            yield break;
        }
        private IEnumerator SetActiveByTime(float time, GameObject tgt)
        {
            yield return new WaitForSeconds(time);
            tgt.SetActive(false);
        }
        private IEnumerator Shoot()
        {
            switch (Ttype.Value)
            {
                case 0:
                    {
                        n.Add((GameObject)Instantiate(torpedo, BlockBehaviour.transform.position + BlockBehaviour.transform.forward * (BlockBehaviour.transform.localScale.z), BlockBehaviour.transform.rotation));
                        break;
                    }
                case 1:
                    {
                        n.Add((GameObject)Instantiate(plasmaTorpedo, BlockBehaviour.transform.position + BlockBehaviour.transform.forward * (BlockBehaviour.transform.localScale.z), BlockBehaviour.transform.rotation));
                        break;
                    }
                default:
                    {
                        n.Add((GameObject)Instantiate(torpedo, BlockBehaviour.transform.position + BlockBehaviour.transform.forward * (BlockBehaviour.transform.localScale.z), BlockBehaviour.transform.rotation));
                        break;
                    }
            }
            int ccount = n.Count - 1;

            if (StatMaster.isMP)
            {
                Message torpedoLaunched = Messages.TorpedoLaunched.CreateMessage((int)MyGUID, (int)ccount, (int)BlockBehaviour.ParentMachine.PlayerID);
                ModNetworking.SendToAll(torpedoLaunched);
            }

            n[ccount].SetActive(true);
            n[ccount].isStatic = false;

            Rigidbody nbody = n[ccount].GetComponent<Rigidbody>();
            //n[ccount].GetComponent<ParticleSystem>().Play();

            //AudioSource ASTL = n[ccount].GetComponent<AudioSource>();

            //ASTL.Play();

            float i = 0f;

            StartCoroutine(SetActiveByTime(existTime, n[ccount]));

            for (; i < existTime; i += Time.fixedDeltaTime)
            {
                if (!n[ccount].activeSelf)
                    yield break;
                //ASTL.pitch = Time.timeScale;
                nbody.velocity = n[ccount].transform.forward * Tspeed;
                yield return new WaitForSeconds(Time.fixedDeltaTime);

                /*
                if (TurretController.Instance.camBindA[BlockBehaviour.ParentMachine.PlayerID])
                {
                    Vector3 tgtDir = TurretController.Instance.aimpos[BlockBehaviour.ParentMachine.PlayerID] - nbody.transform.position;
                    float tgtangle = Mathf.Abs(Vector3.Angle(nbody.transform.forward, tgtDir));
                    if (tgtangle < 3f)
                        nbody.transform.LookAt(TurretController.Instance.aimpos[BlockBehaviour.ParentMachine.PlayerID]);
                    else
                        nbody.transform.rotation = (Quaternion.Lerp(nbody.transform.rotation, Quaternion.LookRotation(tgtDir, nbody.transform.up), 3f / tgtangle));
                    
                }
                */


                if (i < safetyDelay.Value)
                    continue;
                if (Physics.CheckSphere(nbody.transform.position + nbody.transform.forward, 1f, Game.BlockEntityLayerMask, QueryTriggerInteraction.Ignore)) 
                {
                    //Physics.sphere
                    Vector3 expos = nbody.transform.position;
                    Quaternion exrot = nbody.transform.rotation;
                    n[ccount].SetActive(false);
                    if (StatMaster.isMP)
                    {
                        Message torpedoExploded = Messages.TorpedoExplode.CreateMessage((int)MyGUID, (int)ccount, (int)BlockBehaviour.ParentMachine.PlayerID);
                        ModNetworking.SendToAll(torpedoExploded);
                    }
                    //TorpedoController.Instance.TD[BlockBehaviour.ParentMachine.PlayerID][MyGUID].Add(ccount);

                    try
                    {
                        StartCoroutine(displayExplo(expos, exrot));
                    }
                    catch { }


                    Collider[] hits = Physics.OverlapSphere(expos, radius * torpedoRadiu.Value, Game.BlockEntityLayerMask, QueryTriggerInteraction.Ignore);
                    int index = 0;
                    int rank = 40;
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
                            BlockBehaviour hitedblock = hit.attachedRigidbody.gameObject.GetComponent<BlockBehaviour>();
                            hit.attachedRigidbody.WakeUp();

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
                                    hit.attachedRigidbody.AddExplosionForce(power * charge, expos, radius * torpedoRadiu.Value, upPower * charge);
                                    hit.attachedRigidbody.AddRelativeTorque(UnityEngine.Random.insideUnitSphere.normalized * torquePower * charge);
                                }
                                catch { }
                                try
                                {
                                    if (hitedblock.BlockID==11)
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
                                    hit.attachedRigidbody.gameObject.GetComponent<BreakOnForce>().BreakExplosion(power * charge, expos, radius * torpedoRadiu.Value, upPower * charge);
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
                                hit.attachedRigidbody.gameObject.GetComponent<BreakOnForce>().BreakExplosion(power * charge, expos, radius * torpedoRadiu.Value, upPower * charge);
                            }
                            catch { }
                            try
                            {
                                hit.attachedRigidbody.AddExplosionForce(power * charge, expos, radius * torpedoRadiu.Value, upPower * charge);
                                hit.attachedRigidbody.AddRelativeTorque(UnityEngine.Random.insideUnitSphere.normalized * torquePower * charge);
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
            //yield break;
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
                if ((launch.IsHeld || launch.EmulationHeld()) && ShootEnabled && BlockBehaviour.BlockHealth.health > 0f && (BlockBehaviour.blockJoint || BlockBehaviour.jointsToMe.Count > 0))
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

            if (TorpedoController.Instance.TL[BlockBehaviour.ParentMachine.PlayerID].ContainsKey(MyGUID))
            {

                //AudioSource.PlayClipAtPoint(TL, BlockBehaviour.transform.position);
                StartCoroutine(CLShoot());
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
                /*string showmsg = "";
                try
                {
                    showmsg += myExplo.GetComponent<MeshFilter>().sharedMesh.name + ",\n";
                }
                catch { }
                try
                {
                    showmsg += myExplo.GetComponent<MeshFilter>().mesh.name + ",\n";
                }
                catch { }

                try
                {
                    showmsg += myExplo.GetComponent<MeshRenderer>().sharedMaterial.name + ",\n";
                }
                catch { }

                try
                {
                    showmsg += myExplo.GetComponent<MeshRenderer>().material.name;
                }
                catch { }
                GUI.Box(UIrect, showmsg);*/
                if (BlockBehaviour.isSimulating && Icon.IsActive) 
                {
                    foreach (var t in n)
                    {
                        if (t.activeSelf)
                        {
                            GUI.color = IconColor;
                            Vector3 onScreenPosition = Camera.main.WorldToScreenPoint(t.transform.position);
                            if(onScreenPosition.z>=0)
                                GUI.DrawTexture(new Rect(onScreenPosition.x - iconSize/2, Camera.main.pixelHeight - onScreenPosition.y - iconSize/2, iconSize, iconSize), Ticon);
                        }
                    }
                }
                //GUI.color = new Color(1, 1, 1, 1);

            }
        }
        
    }
}
