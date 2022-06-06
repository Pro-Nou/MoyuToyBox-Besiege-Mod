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
    public class BladeTrail : MonoBehaviour
    {
        public int trailResolution = 20;
        public float width = 1f;
        public Color trailColor = new Color(1, 0, 0, 0.4f);
        public bool RGB = false;

        private MeshFilter bladeTrailMesh;
        private MeshRenderer bladeTrailRenderer;

        private GameObject trail;
        private Queue<Vector3> trailPoints = new Queue<Vector3>();
        public void Awake()
        {
            //Debug.Log("awake");
            if (trailResolution < 2)
                trailResolution = 2;
            initTrail();
        }
        public void FixedUpdate()
        {
            updateMesh();
        }
        public void Update()
        {
            if(RGB)
            {
                trailColor = RGBController.Instance.outputColor;
                trailColor.a = 0.4f;
                bladeTrailRenderer.material.SetColor("_TintColor", trailColor);
            }
            Vector3[] vertices = bladeTrailMesh.mesh.vertices;
            vertices[vertices.Length - 2] = this.transform.position - this.transform.up * width;
            vertices[vertices.Length - 1] = this.transform.position + this.transform.up * width;
            bladeTrailMesh.mesh.vertices = vertices;
            //Debug.Log("update");
            /*
            if(canUpdate)
            {
                StartCoroutine(resetUpdate());
                StartCoroutine(updateMesh());
            }
            */
        }
        void initPoints()
        {
            trailPoints.Clear();
            for(int i=0;i<trailResolution;i++)
            {
                trailPoints.Enqueue(this.transform.position - this.transform.up * width);
                trailPoints.Enqueue(this.transform.position + this.transform.up * width);
            }
        }
        private void addPoints()
        {
            trailPoints.Dequeue();
            trailPoints.Dequeue();
            trailPoints.Enqueue(this.transform.position - this.transform.up * width);
            trailPoints.Enqueue(this.transform.position + this.transform.up * width);
        }
        private void initTrail()
        {
            try
            {
                Destroy(trail);
            }
            catch { }
            trail = new GameObject("bladeTrail");
            bladeTrailMesh = trail.AddComponent<MeshFilter>();
            creatMesh();
            bladeTrailRenderer = trail.AddComponent<MeshRenderer>();
            bladeTrailRenderer.material = new Material(Shader.Find("Particles/Alpha Blended"));
            trailColor.a = 0.4f;
            bladeTrailRenderer.material.SetColor("_TintColor", trailColor);
            bladeTrailRenderer.material.mainTexture = ModResource.GetTexture("bladeTrail").Texture;
            trail.SetActive(false);
        }
        private void creatMesh()
        {
            initPoints();
            bladeTrailMesh.mesh = new Mesh();
            Vector2[] trailuv = new Vector2[2 * trailResolution];
            int[] trailtris = new int[6 * (trailResolution - 1)];
            for (int i = 0; i < trailResolution; i++) 
            {
                float uvoffset = Mathf.Min(1f, Mathf.Max(0f, 0.99f - (float)i / (float)(trailResolution - 1)));
                trailuv[i * 2] = new Vector2(uvoffset, 0f);
                trailuv[i * 2 + 1] = new Vector2(uvoffset, 1f);
            }
            for (int i = 0; i < trailResolution - 1; i++)
            {
                trailtris[i * 6] = i * 2;
                trailtris[i * 6 + 1] = i * 2 + 1;
                trailtris[i * 6 + 2] = i * 2 + 3;
                trailtris[i * 6 + 3] = i * 2;
                trailtris[i * 6 + 4] = i * 2 + 2;
                trailtris[i * 6 + 5] = i * 2 + 3;
            }
            bladeTrailMesh.mesh.vertices = trailPoints.ToArray();
            bladeTrailMesh.mesh.uv = trailuv;
            bladeTrailMesh.mesh.triangles = trailtris;
            bladeTrailMesh.mesh.bounds = new Bounds(transform.position, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));
        }
        private void initMesh()
        {
            initPoints();
            bladeTrailMesh.mesh.vertices = trailPoints.ToArray();
        }
        private void updateMesh()
        {
            addPoints();
            bladeTrailMesh.mesh.vertices = trailPoints.ToArray();
        }
        void OnDisable()
        {
            //Debug.Log("disable");
            try
            {
                trail.SetActive(false);
            }
            catch { }
        }
        void OnEnable()
        {
            //Debug.Log("enable");
            try
            {
                initMesh();
            }
            catch { }
            try
            {
                trail.SetActive(true);
            }
            catch { }
        }
        public void printPoints()
        {
            foreach(var a in trailPoints)
            {
                Debug.Log(a.ToString());
            }
        }
    }
    class BeamSaber : BlockScript
    {
        public float charge = 1f;
        private float blockDamage = 1.0f;
        private float forceDamage = 5000f;
        private float power = 10000f;

        private float PSBaseSpeed = 200f;
        private float PSBaseRate = 500f;
        private float PSBaseSize = 2f;

        private bool beamActive = false;
        private bool canDamage = true;
        private bool isheld = false;
        private float damageRate = 0.25f;

        public GameObject BeamBody;
        public GameObject BeamPS;
        public GameObject BeamTrailBody;
        public CapsuleCollider BeamCol;
        public GameObject BeamAS;

        private MToggle RGB;
        private MToggle IFF;
        private MToggle HasTrail;
        //private MToggle toggleMode;
        private MToggle HasBeamCol;
        public MKey Launch;
        public MSlider BeamPower;
        public MSlider BeamLength;
        public MSlider BeamWidth;
        public MSlider trailLength;

        public MColourSlider colourSliderDif;
        public MColourSlider colourSlider;
        public MColourSlider colourSlider1;

        private MeshRenderer renderer1;
        private MeshRenderer renderer2;
        private BladeTrail beamTrail;
        public Material[] matArraySaber = new Material[2];
        public Material matBeam = new Material(Shader.Find("Particles/Alpha Blended"));
        public Color SaberColorDif { get { return matArraySaber[0].color; } set { matArraySaber[0].SetColor("_Color", value); } }
        public Color SaberColor { get { return matArraySaber[1].color; } set { matArraySaber[1].SetColor("_TintColor", value); } }
        //public Color BeamColor { get { return matBeam.color; } set { matBeam.SetColor("_TintColor", value); } }

        public Material PSmat;
        public Color BeamColor2 { get { return PSmat.GetColor("_TintColor"); } set { value.a = 0.15f; PSmat.SetColor("_TintColor", value); } }

        private ParticleSystem PS;
        private ParticleSystemRenderer PSR;
        private ParticleSystem.EmissionModule EM;
        
        private AudioClip LN;
        private AudioSource ASLN;

        public int MyGUID;
        private Rect UIrect = new Rect(0, 100, 512, 128);
        private bool myClusterCode = false;

        //public static MessageType SendHeld = ModNetworking.CreateMessageType(DataType.Integer, DataType.Integer, DataType.Boolean);

        public void initBeam()
        {
            if (BlockBehaviour.transform.FindChild("beamBody") == null)
            {
                BeamBody= GameObject.CreatePrimitive(PrimitiveType.Capsule);
                BeamBody.name = "beamBody";
                BeamBody.transform.SetParent(BlockBehaviour.transform);
                BeamBody.transform.localPosition = new Vector3(0, 0, 6f);
                BeamBody.transform.localRotation = Quaternion.Euler(90f, 0, 0);
                BeamBody.transform.localScale = new Vector3(0.2f, 5f, 0.2f);
                BeamBody.SetActive(false);

                BeamCol = BeamBody.GetComponent<CapsuleCollider>();
                PhysicMaterial BeamPhysicMat = BeamCol.material;
                BeamPhysicMat.staticFriction = 0f;
                BeamPhysicMat.dynamicFriction = 0f;
                BeamPhysicMat.bounciness = 0f;
                BeamPhysicMat.frictionCombine = PhysicMaterialCombine.Minimum;
                BeamPhysicMat.bounceCombine = PhysicMaterialCombine.Minimum;

                BeamAS = new GameObject("Beam AS");
                BeamAS.transform.SetParent(BeamBody.transform);
                BeamAS.transform.localPosition = Vector3.zero;
                BeamAS.AddComponent<AudioSource>();

                LN = ModResource.GetAudioClip("Laser Noise").AudioClip;
                
                ASLN = BeamAS.GetComponent<AudioSource>() ?? BeamAS.AddComponent<AudioSource>();
                ASLN.clip = LN;
                ASLN.spatialBlend = 1.0f;
                ASLN.volume = 1.0f;

                ASLN.SetSpatializerFloat(1, 1f);
                ASLN.SetSpatializerFloat(2, 0);
                ASLN.SetSpatializerFloat(3, 12);
                ASLN.SetSpatializerFloat(4, 1000f);
                ASLN.SetSpatializerFloat(5, 1f);

                ASLN.loop = true;

                BeamAS.AddComponent<MakeAudioSourceFixedPitch>();

                BeamTrailBody = new GameObject("beamtrailbody");
                BeamTrailBody.transform.SetParent(BeamBody.transform);
                BeamTrailBody.transform.localPosition = Vector3.zero;
                BeamTrailBody.transform.localRotation = Quaternion.identity;
                BeamTrailBody.transform.localScale = Vector3.one;
                BeamTrailBody.SetActive(false);
                beamTrail = BeamTrailBody.AddComponent<BladeTrail>();

                renderer1 = BeamBody.GetComponent<MeshRenderer>();
                matBeam.shader = Shader.Find("Particles/Alpha Blended");
                matBeam.mainTexture = Texture2D.whiteTexture;
                matBeam.SetColor("_TintColor", new Color(1, 0, 0, 1));
                renderer1.material = matBeam;

                BeamPS = new GameObject("Beam PS");
                BeamPS.transform.SetParent(BlockBehaviour.transform);
                BeamPS.transform.localPosition = new Vector3(0f, 0f, 1f);
                BeamPS.transform.localRotation = Quaternion.identity;
                BeamPS.SetActive(false);
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
                animationCurve.AddKey(1f, 0.8f);

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
        }
        public void initSaber()
        {
            renderer2 = BlockBehaviour.GetComponentInChildren<MeshRenderer>();
            matArraySaber[0] = renderer2.material;
            matArraySaber[0].mainTexture = ModResource.GetTexture("BeamSaber Texture").Texture;
            matArraySaber[0].SetColor("_Color", new Color(1, 1, 1, 1));
            matArraySaber[1] = new Material(Shader.Find("Particles/Additive"));
            matArraySaber[1].mainTexture = ModResource.GetTexture("BeamSaber Light").Texture;
            matArraySaber[1].SetColor("_TintColor", new Color(1, 1, 1, 1));
            renderer2.materials = matArraySaber;

        }
        public override void SafeAwake()
        {
            Launch = AddKey("开启光剑", "launch", KeyCode.X);
            IFF = AddToggle("开启友伤", "IFF", true);
            HasTrail = AddToggle("刀光", "hastrail", false);
            //toggleMode = AddToggle("持续激活", "togglemode", false);
            RGB = AddToggle("R G B!", "rgb", false);
            HasBeamCol = AddToggle("光剑碰撞", "beamcol", true);
            BeamPower = AddSlider("光剑伤害", "beampower", 1.0f, 0.0f, 10.0f);
            BeamLength = AddSlider("光剑长度", "beamlen", 1.0f, 0.0f, 10.0f);
            BeamWidth = AddSlider("光剑宽度", "beamwidth", 1.0f, 0.0f, 10.0f);
            trailLength = AddSlider("刀光长度", "traillen", 1.0f, 0.0f, 10.0f);
            colourSliderDif = AddColourSlider("剑柄颜色", "colorDif", new Color(1, 1, 1, 1), false);
            colourSlider = AddColourSlider("光剑颜色", "color", new Color(1, 0, 1, 1), false);
            colourSlider1 = AddColourSlider("光剑外圈颜色", "colorOutSide", new Color(1, 0, 1, 1), false);

            initBeam();
            initSaber();
            try
            {
                colourSliderDif.ValueChanged += (Color color) =>
                {
                    try
                    {
                        SaberColorDif = colourSliderDif.Value;
                        renderer2.materials[0].SetColor("_Color", colourSliderDif.Value);
                    }
                    catch { }
                };
                colourSlider.ValueChanged += (Color color) =>
                {
                    try
                    {
                        SaberColor = colourSlider.Value;
                        renderer2.materials[1].SetColor("_TintColor", colourSlider.Value);
                        renderer1.material.SetColor("_TintColor", colourSlider.Value);
                        beamTrail.trailColor = colourSlider.Value;
                    }
                    catch { }
                };
            }
            catch { }

            BlockBehaviour.name = "Beam Saber";
        }
        public override void BuildingUpdate()
        {
            if (renderer2.materials.Length < 2)
                renderer2.materials = matArraySaber;


            if (BlockBehaviour.Guid.GetHashCode() != 0 && BlockBehaviour.Guid.GetHashCode() != MyGUID)
                MyGUID = BlockBehaviour.Guid.GetHashCode();
        }
        public override void OnSimulateStart()
        {
            //if (!toggleMode.IsActive)
                KeymsgController.Instance.keyheld[BlockBehaviour.ParentMachine.PlayerID].Add(MyGUID, false);
            //else
            //    KeymsgController.Instance.keypressed[BlockBehaviour.ParentMachine.PlayerID].Add(MyGUID, false);
        }
        public override void OnSimulateStop()
        {
            //if (!toggleMode.IsActive)
                KeymsgController.Instance.keyheld[BlockBehaviour.ParentMachine.PlayerID].Remove(MyGUID);
            //else
            //    KeymsgController.Instance.keypressed[BlockBehaviour.ParentMachine.PlayerID].Remove(MyGUID);
            BeamBody.SetActive(false);
            BeamPS.SetActive(false);
            beamActive = false;
            isheld = false;
        }
        public void Start()
        {
            try
            {
                BlockBehaviour.blockJoint.breakForce = 100000f;
                BlockBehaviour.blockJoint.breakTorque = 100000f;
            }
            catch { }
            BeamTrailBody.SetActive(HasTrail.IsActive);
            BeamBody.SetActive(false);
            BeamPS.SetActive(false);
            beamActive = false;
            isheld = false;
            BeamBody.transform.localScale = new Vector3(0.2f * BeamWidth.Value / BlockBehaviour.transform.localScale.x,
                                                        5f * BeamLength.Value / BlockBehaviour.transform.localScale.z,
                                                        0.2f * BeamWidth.Value / BlockBehaviour.transform.localScale.y);
            BeamBody.transform.localPosition = new Vector3(0, 0, 1f + 5f * BeamLength.Value / BlockBehaviour.transform.localScale.z);
            //BeamBody.transform.localPosition = new Vector3(0, 0, 5f * BeamLength.Value + 1.0f);
            BeamBody.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            //BeamBody.transform.localScale = new Vector3(0.2f * BeamWidth.Value, 5f * BeamLength.Value, 0.2f * BeamWidth.Value);
            
            try
            {
                BeamCol = BeamBody.GetComponent<CapsuleCollider>();
                BeamCol.enabled = HasBeamCol.IsActive;
            }
            catch { }
            renderer1 = BeamBody.GetComponent<MeshRenderer>();
            beamTrail = BeamTrailBody.GetComponent<BladeTrail>();
            PSR = BeamPS.GetComponent<ParticleSystemRenderer>();
            PSmat = PSR.material;
            if (RGB.IsActive)
            {
                SaberColor = RGBController.Instance.outputColor;
                BeamColor2 = RGBController.Instance.outputColor;
                beamTrail.trailColor = RGBController.Instance.outputColor;
                renderer1.material.SetColor("_TintColor", RGBController.Instance.outputColor);
            }
            else
            {
                SaberColor = colourSlider.Value;
                BeamColor2 = colourSlider1.Value;
                beamTrail.trailColor = colourSlider.Value;
                renderer1.material.SetColor("_TintColor", colourSlider.Value);
            }
            SaberColorDif = colourSliderDif.Value;
            renderer2.materials = matArraySaber;

            beamTrail.width = BeamLength.Value * 5f;
            beamTrail.trailResolution = (int)(trailLength.Value * 20f);
            beamTrail.RGB = RGB.IsActive;

            PS = BeamPS.GetComponent<ParticleSystem>();
            PS.startSpeed = PSBaseSpeed * BeamLength.Value;
            PS.startSize = PSBaseSize * BeamWidth.Value;
            EM = PS.emission;
            EM.rate = (BeamLength.Value / BeamWidth.Value) * PSBaseRate;
            charge = BeamPower.Value;
            //ASBL.pitch = 1f / ShootCD;

        }
        private void Update()
        {
            if (BlockBehaviour.isSimulating)
            {
                if (RGB.IsActive)
                {
                    SaberColor = RGBController.Instance.outputColor;
                    renderer1.material.SetColor("_TintColor", RGBController.Instance.outputColor);
                    BeamColor2 = RGBController.Instance.outputColor;
                }
                //if (!toggleMode.IsActive)
                {
                    if (Launch.IsHeld || Launch.EmulationHeld() || KeymsgController.Instance.keyheld[BlockBehaviour.ParentMachine.PlayerID][MyGUID])
                    {
                        beamActive = true;
                        BeamBody.SetActive(beamActive);
                        BeamPS.SetActive(beamActive);
                    }
                    else
                    {
                        if (Time.timeScale > 0f)
                        {
                            beamActive = false;
                            BeamBody.SetActive(beamActive);
                            BeamPS.SetActive(beamActive);
                        }
                    }
                    if (!StatMaster.isClient)
                    {
                        if (isheld != beamActive)
                        {
                            isheld = beamActive;
                            ModNetworking.SendToAll(KeymsgController.SendHeld.CreateMessage((int)BlockBehaviour.ParentMachine.PlayerID, (int)MyGUID, (bool)isheld));
                        }
                    }
                }
                /*
                else
                {
                    if(StatMaster.isClient)
                    {
                        if(KeymsgController.Instance.keypressed[BlockBehaviour.ParentMachine.PlayerID][MyGUID])
                        {
                            KeymsgController.Instance.keypressed[BlockBehaviour.ParentMachine.PlayerID][MyGUID] = false;
                            beamActive = !beamActive;
                            BeamBody.SetActive(beamActive);
                            BeamPS.SetActive(beamActive);
                        }
                    }
                    else
                    {
                        if (Launch.IsPressed || Launch.EmulationPressed())
                        {
                            ModNetworking.SendToAll(KeymsgController.SendPressed.CreateMessage((int)BlockBehaviour.ParentMachine.PlayerID, (int)MyGUID, (bool)true));
                            beamActive = !beamActive;
                            BeamBody.SetActive(beamActive);
                            BeamPS.SetActive(beamActive);
                        }
                    }
                }
                */
                float ASy = BeamBody.transform.InverseTransformPoint(Camera.main.transform.position).y;
                ASy = Mathf.Min(1f, Mathf.Max(-1f, ASy));
                BeamAS.transform.localPosition = new Vector3(0f, ASy, 0f);
            }
            if (StatMaster.clusterCoded)
                myClusterCode = true;
            else
            {
                if (myClusterCode)
                {
                    renderer2.materials = matArraySaber;
                }
                myClusterCode = false;
            }
        }
        public override void SimulateFixedUpdateHost()
        {
            if(beamActive && canDamage)
            {
                StartCoroutine(resetDamage());
                StartCoroutine(makeDamage());
            }
        }
        private IEnumerator resetDamage()
        {
            canDamage = false;
            yield return new WaitForSeconds(damageRate);
            canDamage = true;
            yield break;
        }
        private IEnumerator makeDamage()
        {
            Vector3 point1 = BlockBehaviour.transform.position + BlockBehaviour.transform.forward * (BlockBehaviour.transform.localScale.z + 0.1f + 0.5f * BeamWidth.Value);
            Vector3 point2 = BlockBehaviour.transform.position + BlockBehaviour.transform.forward * (BlockBehaviour.transform.localScale.z + 10f * BeamLength.Value - 0.5f * BeamWidth.Value);
            Collider[] hits = Physics.OverlapCapsule(point1, point2, 0.4f * BeamWidth.Value, Game.BlockEntityLayerMask, QueryTriggerInteraction.Ignore);
            //hitedcols = hits.Length;
            if (hits.Length <= 0)
                yield break;
            int index = 0;
            int rank = 20;
            List<int> hitblocks = new List<int>();
            foreach (var hit in hits)
            {
                if (index > rank)
                {
                    index = 0;
                    yield break;
                }
                if (hit.gameObject.name== "beamBody")
                {
                    continue;
                }
                try
                {
                    hit.attachedRigidbody.WakeUp();
                    BlockBehaviour hitedblock = hit.attachedRigidbody.gameObject.GetComponent<BlockBehaviour>();
                    if (!hitblocks.Contains(hitedblock.gameObject.GetHashCode()))
                    {
                        if (hitedblock.gameObject.GetInstanceID() == BlockBehaviour.gameObject.GetInstanceID())
                            continue;
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
                            hit.attachedRigidbody.AddForce(charge * power * BlockBehaviour.transform.forward, ForceMode.Force);
                        }
                        catch { }
                        try
                        {
                            switch(hitedblock.BlockID)
                            {
                                case (int)BlockType.Cannon:
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
                                        break;
                                    }
                                case (int)BlockType.SpinningBlock:
                                    {
                                        hitedblock.blockJoint.breakForce -= forceDamage * 0.1f * charge;
                                        break;
                                    }
                                default:
                                    {
                                        break;
                                    }
                            }
                        }
                        catch { }
                        try
                        {
                            hitedblock.BlockHealth.DamageBlock(blockDamage);
                        }
                        catch { }
                        index++;
                    }
                    continue;
                }
                catch { }
                try
                {
                    hit.attachedRigidbody.AddForce(charge * power * BlockBehaviour.transform.forward, ForceMode.Force);
                }
                catch { }
            }
            yield break;
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
                string showmsg = "";
                //showmsg += BeamBody.GetComponent<CapsuleCollider>().ClosestPointOnBounds(BlockBehaviour.transform.position).ToString();
                //showmsg += BeamBody.GetComponent<CapsuleCollider>().ClosestPointOnBounds(BlockBehaviour.transform.position + 100* BlockBehaviour.transform.forward).ToString();
                showmsg += hitedcols.ToString(); 
                GUI.Box(UIrect, showmsg.ToString());
            }
            catch { }
        }
        */
    }
}
