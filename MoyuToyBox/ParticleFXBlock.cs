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
    class ParticleFXBlock : BlockScript
    {
        public GameObject Vis;
        public GameObject Colliders;
        public GameObject particleBody;

        public MMenu footMenu;
        public MKey Launch;
        public MToggle RGB;

        //PSrenderer
        public MMenu shaderMenu;
        public MMenu meshMenu;
        public MMenu texMenu;
        public MMenu renderMenu;
        public MSlider lengthScaleSlider;
        public MSlider sortingFudgeSlider;

        //PScommom
        public MToggle loopToggle;
        public MMenu simulationSpaceMenu;
        public MMenu scalingModeMenu;
        public MSlider SizeSlider;
        public MSlider playbackSpeedSlider;
        public MSlider LifetimeSlider;
        public MSlider speedSlider;
        public MSlider startDelaySlider;
        public MSlider maxParticlesSlider;
        public MSlider gravityModifierSlider;

        //PSemissino
        public MSlider rateSlider;

        //PSshape
        public MMenu shapeTypeMenu;
        public MSlider angleSlider;
        public MSlider radiusSlider;

        //PSsize-lifetime
        public MSlider startSizeSlider;
        public MSlider endSizeSlider;

        //PSvelocity-lifetime
        public MSlider dampenSlider;
        public MSlider velocityLimitSlider;

        //PSrotation-lifetime
        public MSlider rotXSlider;
        public MSlider rotYSlider;
        public MSlider rotZSlider;

        //PScolor-lifetime
        public MColourSlider startColorSlider;
        public MColourSlider endColorSlider;
        public MSlider startAlphaSlider;
        public MSlider endAlphaSlider;

        public Material PSmat;
        public ParticleSystem PS;
        public ParticleSystemRenderer PSR;
        public ParticleSystem.EmissionModule EM;
        public ParticleSystem.CollisionModule CM;
        public ParticleSystem.ShapeModule SM;
        public ParticleSystem.SizeOverLifetimeModule SOLT;
        public ParticleSystem.RotationOverLifetimeModule ROLT;
        public ParticleSystem.LimitVelocityOverLifetimeModule LVOLT;
        public ParticleSystem.ColorOverLifetimeModule COLT;

        public int MyGUID;
        private bool isheld = false;
        private bool particleActive = false;

        public void InitParticle()
        {
            if (BlockBehaviour.transform.FindChild("particleBody") == null)
            {
                particleBody = new GameObject("particleBody");
                particleBody.SetActive(false);
                particleBody.transform.SetParent(BlockBehaviour.transform);
                particleBody.transform.localPosition = Vector3.zero;
                particleBody.transform.localRotation = Quaternion.identity;

                PS = particleBody.GetComponent<ParticleSystem>()?? particleBody.AddComponent<ParticleSystem>();
                PSR = particleBody.GetComponent<ParticleSystemRenderer>();

                PSmat = new Material(Shader.Find("Particles/Additive"));
                PSmat.mainTexture = ModResource.GetTexture("Glow").Texture;
                PSmat.SetColor("_TintColor", new Color(1f, 1f, 1f, 1f));
                PSR.material = PSmat;
                PSR.renderMode = ParticleSystemRenderMode.Billboard;
                PSR.lengthScale = 1f;
                PSR.sortingFudge = 3000f;
                PSR.mesh = ModResource.GetMesh("Quad").Mesh;

                EM = PS.emission;
                CM = PS.collision;
                SM = PS.shape;
                SOLT = PS.sizeOverLifetime;
                ROLT = PS.rotationOverLifetime;
                LVOLT = PS.limitVelocityOverLifetime;
                COLT = PS.colorOverLifetime;

                PS.loop = true;
                PS.startSize = 1f;

                PS.playbackSpeed = 1f;
                PS.startLifetime = 1f;
                PS.startSpeed = 1f;
                PS.scalingMode = ParticleSystemScalingMode.Shape;

                PS.startDelay = 0f;
                PS.maxParticles = 50;
                PS.gravityModifier = 0f;
                PS.randomSeed = 0;

                PS.simulationSpace = ParticleSystemSimulationSpace.Local;

                EM.rate = 50f;
                EM.enabled = true;

                CM.enabled = false;

                SM.shapeType = ParticleSystemShapeType.Sphere;
                SM.angle = 0;
                SM.radius = 1f;

                AnimationCurve animationCurve = new AnimationCurve();
                animationCurve.keys = new Keyframe[2] { new Keyframe(0f, 1f), new Keyframe(1f, 1f) };
                SOLT.size = new ParticleSystem.MinMaxCurve(1f, animationCurve);
                SOLT.enabled = true;

                ROLT.separateAxes = true;
                ROLT.enabled = true;
                animationCurve.keys = new Keyframe[2] { new Keyframe(0f, 0f), new Keyframe(1f, 0f) };
                ROLT.x = new ParticleSystem.MinMaxCurve(1f, animationCurve);
                ROLT.y = new ParticleSystem.MinMaxCurve(1f, animationCurve);
                ROLT.z = new ParticleSystem.MinMaxCurve(1f, animationCurve);

                LVOLT.dampen = 0.0f;
                LVOLT.limit = 0f;
                LVOLT.enabled = true;

                COLT.enabled = true;
                Gradient gradient = new Gradient();
                gradient.SetKeys(new GradientColorKey[]
                {
                new GradientColorKey(Color.white,1f),
                new GradientColorKey(Color.white,1f)
                }, new GradientAlphaKey[]
                {
                new GradientAlphaKey(startAlphaSlider.Value, 0f),
                new GradientAlphaKey(endAlphaSlider.Value, 1f)
                });
                COLT.color = gradient;
            }
        }
        public void updateParticle()
        {
            PS = particleBody.GetComponent<ParticleSystem>() ?? particleBody.AddComponent<ParticleSystem>();
            PSR = particleBody.GetComponent<ParticleSystemRenderer>();
            
            try
            {
                PSmat.shader = Shader.Find(shaderMenu.Items[shaderMenu.Value]);
            }
            catch { }
            try
            {
                if (texMenu.Value == 0)
                    PSmat.mainTexture = Texture2D.whiteTexture;
                else
                    PSmat.mainTexture = ModResource.GetTexture(texMenu.Items[texMenu.Value]).Texture;
            }
            catch { }
            PSmat.SetColor("_Color", startColorSlider.Value);
            PSmat.SetColor("_TintColor", new Color(1f, 1f, 1f, 1f));
            PSR.material = PSmat;
            PSR.renderMode = (ParticleSystemRenderMode)renderMenu.Value;
            PSR.lengthScale = lengthScaleSlider.Value;
            PSR.sortingFudge = sortingFudgeSlider.Value;
            PSR.mesh = ModResource.GetMesh(meshMenu.Items[meshMenu.Value]).Mesh;

            EM = PS.emission;
            CM = PS.collision;
            SM = PS.shape;
            SOLT = PS.sizeOverLifetime;
            ROLT = PS.rotationOverLifetime;
            LVOLT = PS.limitVelocityOverLifetime;
            COLT = PS.colorOverLifetime;

            PS.loop = loopToggle.IsActive;
            PS.startSize = SizeSlider.Value;

            PS.playbackSpeed = playbackSpeedSlider.Value;
            PS.startLifetime = LifetimeSlider.Value;
            PS.startSpeed = speedSlider.Value;
            PS.scalingMode = (ParticleSystemScalingMode)scalingModeMenu.Value;

            PS.startDelay = startDelaySlider.Value;
            PS.maxParticles = (int)maxParticlesSlider.Value;
            PS.gravityModifier = gravityModifierSlider.Value;

            PS.simulationSpace = (ParticleSystemSimulationSpace)simulationSpaceMenu.Value;

            EM.rate = rateSlider.Value;
            EM.enabled = true;

            CM.enabled = false;

            SM.shapeType = (ParticleSystemShapeType)shapeTypeMenu.Value;
            SM.angle = angleSlider.Value;
            SM.radius = radiusSlider.Value;

            AnimationCurve animationCurve = new AnimationCurve();
            animationCurve.keys = new Keyframe[2] { new Keyframe(0f, startSizeSlider.Value), new Keyframe(1f, endSizeSlider.Value) };
            SOLT.size = new ParticleSystem.MinMaxCurve(1f, animationCurve);
            SOLT.enabled = true;

            ROLT.separateAxes = true;
            ROLT.enabled = true;
            animationCurve.keys = new Keyframe[2] { new Keyframe(0f, rotXSlider.Value), new Keyframe(1f, rotXSlider.Value) };
            ROLT.x = new ParticleSystem.MinMaxCurve(1f, animationCurve);
            animationCurve.keys = new Keyframe[2] { new Keyframe(0f, rotYSlider.Value), new Keyframe(1f, rotYSlider.Value) };
            ROLT.y = new ParticleSystem.MinMaxCurve(1f, animationCurve);
            animationCurve.keys = new Keyframe[2] { new Keyframe(0f, rotZSlider.Value), new Keyframe(1f, rotZSlider.Value) };
            ROLT.z = new ParticleSystem.MinMaxCurve(1f, animationCurve);

            LVOLT.dampen = dampenSlider.Value;
            LVOLT.limit = velocityLimitSlider.Value;
            LVOLT.enabled = true;

            COLT.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(new GradientColorKey[]
            {
                new GradientColorKey(startColorSlider.Value,0f),
                new GradientColorKey(endColorSlider.Value,1f)
            }, new GradientAlphaKey[]
            {
                new GradientAlphaKey(startAlphaSlider.Value, 0f),
                new GradientAlphaKey(endAlphaSlider.Value, 1f)
            });
            COLT.color = gradient;
        }
        public void displayMenu(int value,int count)
        {
            bool[] menuDisplay = new bool[count];
            for(int i=0;i< count;i++)
            {
                if (i == value)
                    menuDisplay[i] = true;
                else
                    menuDisplay[i] = false;
            }
            shaderMenu.DisplayInMapper = menuDisplay[0];
            meshMenu.DisplayInMapper = menuDisplay[1];
            texMenu.DisplayInMapper = menuDisplay[2];
            renderMenu.DisplayInMapper = menuDisplay[3];
            simulationSpaceMenu.DisplayInMapper = menuDisplay[4];
            scalingModeMenu.DisplayInMapper = menuDisplay[5];
            shapeTypeMenu.DisplayInMapper = menuDisplay[6];
        }
        public override void SafeAwake()
        {
            Vis = BlockBehaviour.gameObject.transform.FindChild("Vis").gameObject;
            Colliders = BlockBehaviour.gameObject.transform.FindChild("Colliders").gameObject;

            footMenu = AddMenu("footer", 0, new List<string> { "Shader", "Mesh", "Texture", "RenderMode", "SimulationSpace", "ScalingMode", "Shape" });
            shaderMenu = AddMenu("shadertype", 0, new List<string> { "Particles/Additive", "Particles/Alpha Blended", "Standard" });
            meshMenu = AddMenu("meshtype", 0, new List<string> { "Quad", "Arrow" });
            texMenu = AddMenu("textype", 0, new List<string> { "Quad", "Glow", "Star Glow", "Circle", "Smoke", "Feather", "Gun Fire" });
            renderMenu = AddMenu("rendertype", 0, Enum.GetNames(typeof(ParticleSystemRenderMode)).ToList());
            simulationSpaceMenu = AddMenu("simulationSpace", 0, Enum.GetNames(typeof(ParticleSystemSimulationSpace)).ToList());
            scalingModeMenu = AddMenu("scalingMode", 0, Enum.GetNames(typeof(ParticleSystemScalingMode)).ToList());
            shapeTypeMenu = AddMenu("shapeType", 0, Enum.GetNames(typeof(ParticleSystemShapeType)).ToList());
            Launch = AddKey("启用", "launch", KeyCode.X);
            RGB = AddToggle("R G B!", "rgb", false);

            lengthScaleSlider = AddSlider("缩放长度", "lengthScale", 1f, 0f, 10f);
            sortingFudgeSlider = AddSlider("sortingFudge", "sortingFudge", 3000f, 0f, 10000f);

            loopToggle = AddToggle("loop", "loop", false);
            SizeSlider = AddSlider("大小", "size", 1f, 0f, 10f);
            playbackSpeedSlider = AddSlider("播放速度", "playbackspeed", 1f, 0f, 10f);
            LifetimeSlider = AddSlider("持续时间", "Lifetime", 1f, 0f, 10f);
            speedSlider = AddSlider("发射速度", "speed", 1f, 0f, 10f);
            startDelaySlider = AddSlider("起始延迟", "startDelay", 0f, 0f, 1f);
            maxParticlesSlider = AddSlider("最大粒子数", "maxParticles", 50f, 0f, 10000f);
            gravityModifierSlider = AddSlider("重力", "gravityModifier", 0f, 0f, 10f);

            rateSlider= AddSlider("发射频率", "rate", 50f, 0f, 10000f);

            angleSlider = AddSlider("扩散角度", "angle", 0f, 0f, 90f); 
            radiusSlider = AddSlider("扩散范围", "radius", 1f, 0f, 10f);

            startSizeSlider = AddSlider("起始大小", "startSize", 1f, 0f, 10f);
            endSizeSlider = AddSlider("结束大小", "endSize", 1f, 0f, 10f);

            dampenSlider = AddSlider("阻尼", "dampen", 0f, -1f, 1f);
            velocityLimitSlider = AddSlider("速度限制", "velocityLimit", 0f, 0f, 1f);

            rotXSlider = AddSlider("旋转X", "rotx", 0f, -180f, 180f);
            rotYSlider = AddSlider("旋转Y", "roty", 0f, -180f, 180f);
            rotZSlider = AddSlider("旋转Z", "rotz", 0f, -180f, 180f);

            startAlphaSlider = AddSlider("起始不透明度", "startAlpha", 1f, 0f, 1f);
            endAlphaSlider = AddSlider("结束不透明度", "endAlpha", 1f, 0f, 1f);
            startColorSlider = AddColourSlider("起始颜色", "startColor", new Color(1, 1, 1, 1), false);
            endColorSlider = AddColourSlider("结束颜色", "endColor", new Color(1, 1, 1, 1), false);

            InitParticle();
            try
            {
                footMenu.ValueChanged += (int value) =>
                {
                    displayMenu(footMenu.Value, 7);
                };
            }
            catch { }
            BlockBehaviour.name = "ParticleFXBlock";
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
            updateParticle();
            particleActive = false;
            isheld = false;
            BlockBehaviour.noRigidbody = false;

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
            particleActive = false;
            isheld = false;
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
                    particleActive = true;
                    particleBody.SetActive(true);
                }
                else
                {
                    if (Time.timeScale > 0f)
                    {
                        particleActive = false;
                        particleBody.SetActive(false);
                    }
                }
                if (!StatMaster.isClient)
                {
                    if (isheld != particleActive)
                    {
                        isheld = particleActive;
                        ModNetworking.SendToAll(KeymsgController.SendHeld.CreateMessage((int)BlockBehaviour.ParentMachine.PlayerID, (int)MyGUID, (bool)isheld));
                    }
                }
                if (RGB.IsActive)
                {
                    PSmat.SetColor("_Color", RGBController.Instance.outputColor);
                    PSmat.SetColor("_TintColor", RGBController.Instance.outputColor);
                }
            }
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
