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
    public class SwordDamage : MonoBehaviour
    {
        public float charge = 1f;
        private float forceDamage = 5000f;
        private float power = 5000f;
        public MPTeam team = MPTeam.None;
        public bool IFF = true;
        public ushort PlayerID = 0;
        private Rigidbody SBody;
        private void Start()
        {
            SBody = base.GetComponent<BoxCollider>().attachedRigidbody;
            //Debug.Log("start " + SBody.name);
        }
        void OnTriggerEnter(Collider col)
        {
            try
            {
                if (col.isTrigger)
                    return;
                if (col.transform.parent.GetInstanceID() == col.GetInstanceID())
                    return;

                Vector3 damageVelocity;

                if (SBody.velocity.magnitude == 0f)
                    return;
                else
                    damageVelocity = Vector3.ProjectOnPlane(SBody.velocity, this.transform.up);

                try
                {
                    BlockBehaviour hitedBlock = col.attachedRigidbody.gameObject.GetComponent<BlockBehaviour>();

                    //Debug.Log(hitedBlock.gameObject.name);
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
                        Vector3 damageDir = Vector3.Project((col.transform.position - base.transform.position), this.transform.up).normalized;
                        hitedBlock.Rigidbody.AddForce(damageDir * damageVelocity.magnitude * charge * power);
                    }
                    catch { }
                    try
                    {
                        switch (hitedBlock.BlockID)
                        {
                            case (int)BlockType.Cannon:
                                {
                                    if (hitedBlock.blockJoint.breakForce > 50000f)
                                    {
                                        hitedBlock.blockJoint.breakForce = 50000f;
                                    }
                                    else
                                    {
                                        hitedBlock.blockJoint.breakForce -= forceDamage * charge * damageVelocity.magnitude;
                                        if (hitedBlock.blockJoint.breakForce < 0)
                                            hitedBlock.blockJoint.breakForce = 0;
                                    }
                                    break;
                                }
                            case (int)BlockType.SpinningBlock:
                                {
                                    hitedBlock.blockJoint.breakForce -= forceDamage * 0.1f * charge * damageVelocity.magnitude;
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                    }
                    catch { }
                    return;
                }
                catch { }
                try
                {
                    Vector3 damageDir = Vector3.Project((col.transform.position - base.transform.position), this.transform.up).normalized;
                    col.attachedRigidbody.AddForce(damageDir * damageVelocity.magnitude * charge * power);
                }
                catch { }
            }
            catch { }
        }

    }
    class MetalSword : BlockScript
    {
        private bool myClusterCode = false;

        public GameObject bladeObject;
        public GameObject SwordTrailObject;

        public MSlider trailLength;
        public MSlider swordDamageSlidder;
        public MSlider massSlider;
        public MColourSlider colourSliderDif;
        public MColourSlider colourSlider;
        public MToggle HasTrail;
        public MToggle IFF;
        public MToggle RGB;

        public MeshRenderer renderer1;
        public BladeTrail SwordTrail;
        public SwordDamage swordDamage;
        public Material[] matArraySword = new Material[2];
        public Color SwordColorDif { get { return matArraySword[0].color; } set { matArraySword[0].SetColor("_Color", value); } }
        public Color SwordColor { get { return matArraySword[1].color; } set { matArraySword[1].SetColor("_TintColor", value); } }

        public void initBlade()
        {
            if(BlockBehaviour.transform.FindChild("blade object")==null)
            {
                bladeObject = new GameObject("blade object");
                bladeObject.SetActive(false);
                BoxCollider bladeCol = bladeObject.AddComponent<BoxCollider>();
                bladeCol.isTrigger = true;

                bladeObject.transform.SetParent(BlockBehaviour.transform);
                bladeObject.transform.localScale = new Vector3(0.9f, 0.1f, 3.6f);
                bladeObject.transform.localPosition = new Vector3(0f, 0f, 2.2f);
                bladeObject.transform.localRotation = Quaternion.identity;
                swordDamage = bladeObject.AddComponent<SwordDamage>();

                SwordTrailObject = new GameObject("SwordTrailObject");
                SwordTrailObject.transform.SetParent(BlockBehaviour.transform);
                SwordTrailObject.transform.localPosition = new Vector3(0.0f, 0.0f, 2.2f);
                SwordTrailObject.transform.localRotation = Quaternion.Euler(90f, 0, 0);
                SwordTrailObject.transform.localScale = Vector3.one;
                SwordTrailObject.SetActive(false);
                SwordTrail = SwordTrailObject.AddComponent<BladeTrail>();
            }
        }
        public void initSword()
        {
            renderer1 = BlockBehaviour.GetComponentInChildren<MeshRenderer>();
            matArraySword[0] = renderer1.material;
            matArraySword[0].mainTexture = ModResource.GetTexture("MetalSword Texture").Texture;
            matArraySword[0].SetColor("_Color", new Color(1, 1, 1, 1));
            matArraySword[1] = new Material(Shader.Find("Particles/Additive"));
            matArraySword[1].mainTexture = ModResource.GetTexture("MetalSword Light").Texture;
            matArraySword[1].SetColor("_TintColor", new Color(1, 1, 1, 1));
            renderer1.materials = matArraySword;

        }
        public void initChildren()
        {
            for(int i=0;i< BlockBehaviour.transform.childCount;i++)
            {
                if (BlockBehaviour.transform.GetChild(i).gameObject.name == "Adding Point")
                {
                    BlockBehaviour.transform.GetChild(i).localScale = new Vector3(1f, 6f, 1f);
                }
                else if (BlockBehaviour.transform.GetChild(i).gameObject.name == "Colliders")
                {
                    BoxCollider bladeCol = BlockBehaviour.transform.GetChild(i).GetChild(0).gameObject.GetComponent<BoxCollider>();
                    PhysicMaterial bladePhysicMat = bladeCol.material;
                    bladePhysicMat.staticFriction = 0f;
                    bladePhysicMat.dynamicFriction = 0f;
                    bladePhysicMat.bounciness = 0f;
                    bladePhysicMat.frictionCombine = PhysicMaterialCombine.Minimum;
                    bladePhysicMat.bounceCombine = PhysicMaterialCombine.Minimum;
                }
            }
        }
        public override void SafeAwake()
        {
            IFF = AddToggle("开启友伤", "IFF", true);
            HasTrail = AddToggle("刀光", "hastrail", false);
            RGB = AddToggle("R G B!", "rgb", false);
            swordDamageSlidder = AddSlider("伤害倍率", "beampower", 1.0f, 0.0f, 10.0f);
            massSlider = AddSlider("质量", "mass", 2.0f, 0.0f, 10.0f);
            trailLength = AddSlider("刀光长度", "traillen", 1.0f, 0.0f, 10.0f);
            colourSliderDif = AddColourSlider("剑身颜色", "colorDif", new Color(0.2f, 0.2f, 0.2f, 1), false);
            colourSlider = AddColourSlider("剑刃颜色", "color", new Color(1, 0, 0, 1), false);

            initSword();
            initBlade();
            initChildren();
            try
            {
                colourSliderDif.ValueChanged += (Color color) =>
                {
                    try
                    {
                        SwordColorDif = colourSliderDif.Value;
                        renderer1.materials[0].SetColor("_Color", colourSliderDif.Value);
                    }
                    catch { }
                };
                colourSlider.ValueChanged += (Color color) =>
                {
                    try
                    {
                        SwordColor = colourSlider.Value;
                        renderer1.materials[1].SetColor("_TintColor", colourSlider.Value);
                        SwordTrail.trailColor = colourSlider.Value;
                    }
                    catch { }
                };
                trailLength.ValueChanged += (float value) =>
                {
                    try
                    {
                        SwordTrail.trailResolution = (int)(trailLength.Value * 20f);
                    }
                    catch { }
                };
                massSlider.ValueChanged += (float value) =>
                {
                    try
                    {
                        BlockBehaviour.Rigidbody.mass = massSlider.Value;
                    }
                    catch { }
                };
            }
            catch { }

            BlockBehaviour.name = "Metal Sword";
        }
        private void Start()
        {
            try
            {
                BlockBehaviour.blockJoint.breakForce = Mathf.Infinity;
                BlockBehaviour.blockJoint.breakTorque = Mathf.Infinity;
            }
            catch { }
            SwordTrailObject.SetActive(HasTrail.IsActive);

            SwordTrail = SwordTrailObject.GetComponent<BladeTrail>();

            try
            {
                if (!StatMaster.isClient)
                    bladeObject.SetActive(true);
                else
                    bladeObject.SetActive(false);
            }
            catch { }
            if (RGB.IsActive)
            {
                SwordColor = RGBController.Instance.outputColor;
                SwordTrail.trailColor = RGBController.Instance.outputColor;
            }
            else
            {
                SwordColor = colourSlider.Value;
                SwordTrail.trailColor = colourSlider.Value;
            }
            SwordColorDif = colourSliderDif.Value;
            renderer1.materials = matArraySword;

            SwordTrail.width = 1.8f * BlockBehaviour.transform.localScale.z;
            SwordTrail.trailResolution = (int)(trailLength.Value * 20f);
            SwordTrail.RGB = RGB.IsActive;

            swordDamage = bladeObject.GetComponent<SwordDamage>();
            swordDamage.charge = swordDamageSlidder.Value;
            swordDamage.IFF = IFF.IsActive;
        }
        public override void OnSimulateStart()
        {
        }
        public override void OnSimulateStop()
        {
        }
        public override void BuildingUpdate()
        {
            if (renderer1.materials.Length < 2)
                renderer1.materials = matArraySword;
        }
        private void Update()
        {
            if (RGB.IsActive)
            {
                SwordColor = RGBController.Instance.outputColor;
            }
            if (StatMaster.clusterCoded)
                myClusterCode = true;
            else
            {
                if (myClusterCode)
                {
                    renderer1.materials = matArraySword;
                }
                myClusterCode = false;
            }
        }
    }
}
