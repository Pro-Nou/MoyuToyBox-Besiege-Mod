using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using Modding;
using Modding.Blocks;

namespace MoyuToyBox
{
    class SteeringBlockStript : MonoBehaviour
    {
        public BlockBehaviour BB { get; internal set; }
        //public ModBlockBehaviour MBB { get; internal set; }
        public SteeringWheel steeringWheel;
        public ConfigurableJoint myJoint;

        public MKey aimKey;
        public MKey leftKey;
        public MKey rightKey;
        public MToggle smartTurret;
        public MSlider speedSlider;
        public MSlider massSlider;
        public MSlider maxForceSlider;
        public MToggle R2C;
        public MLimits mlimited;

        public bool isFirstFrame = true;
        public float target;
        public bool aimR2C;

        public bool aiming=false;
        public Vector3 aimpos;
        public Vector3 localaim;

        private JointDrive angularYzDrive;
        private JointDrive angularXdrive;
        private Rect UIrect = new Rect(0, 100, 512, 256);
        public Vector3 axis;

        private void Awake()
        {

            BB = GetComponent<BlockBehaviour>();
            //MBB = GetComponent<ModBlockBehaviour>();

            SafeAwake();

            if (BB.isSimulating) { return; }


            //Enhancement.Toggled += (bool value) => {  PropertiseChangedEvent();};

            //LoadConfiguration();    

            //PropertiseChangedEvent += ChangedProperties;
            //PropertiseChangedEvent += () => { DisplayInMapper(Enhancement.IsActive); };
            //PropertiseChangedEvent?.Invoke();

            //Controller.Instance.OnSave += SaveConfiguration;
        }
        void Update()
        {
            if(BB.isSimulating)
            {
                SimulateUpdate();
            }
            else
            {
                BuildingUpdate();
            }
        }
        public virtual void SimulateUpdate()
        {
            if (!StatMaster.isClient)
                SimulateUpdateHost();
        }
        public virtual void SimulateUpdateHost()
        {
            if(smartTurret.IsActive)
            {
                if (aimKey.IsPressed)
                    aiming = !aiming;
            }
        }
        public virtual void BuildingUpdate()
        {
            steeringWheel.allowLimits = mlimited.IsActive;


            if (smartTurret.IsActive)
            {
                if (speedSlider.Value > 9.0f)
                    speedSlider.Value = 9.0f;
                else if (speedSlider.Value < 0.0f)
                    speedSlider.Value = 0.0f;

                if (maxForceSlider.Value < 0.0f)
                    maxForceSlider.Value = 0.0f;
                else if (maxForceSlider.Value > 1.0f)
                    maxForceSlider.Value = 1.0f;
            }

        }

        public virtual void SafeAwake()
        {

            steeringWheel = BB.GetComponent<SteeringWheel>();

            smartTurret = BB.AddToggle("指向准星", "smartTurret", false);
            aimKey = BB.AddKey("激活瞄准", "aimEnable", KeyCode.None);

            maxForceSlider = BB.AddSlider("受力阈值", "maxForce", 0.5f, 0.0f, 1.0f);
            massSlider = BB.AddSlider("重量", "mass", 0.5f, 0f, 5f);
            FauxTransform a = new FauxTransform(new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), new Vector3(0.4f, 0.4f, 0.4f));
            //mlimited= BB.AddLimits("角度限制", "RotateLimit", 45f, 45f, 170f, a,BB.);
            mlimited = steeringWheel.AddLimits("角度限制", "RotateLimit", 45f, 45f, 170f, a, steeringWheel);
            
            foreach (var key in steeringWheel.Keys)
            {
                if (key.Key == "left")
                {
                    leftKey = key;
                }
                else if(key.Key=="right")
                {
                    rightKey = key;
                }
            }
            foreach (var slider in BB.Sliders)
            {
                if (slider.Key == "rotation-speed")
                {
                    speedSlider = slider;
                    break;
                }
            }
            foreach (var Toggle in BB.Toggles)
            {
                if (Toggle.Key == "autoReturn")
                {
                    R2C = Toggle;
                    break;
                }
            }
        }
        public void TurretStart()
        {
            if (!BB.isSimulating || !BB.SimPhysics)
                return;
            if (!BB.noRigidbody)
                BB.Rigidbody.maxAngularVelocity = 100f;
            myJoint = BB.GetComponent<ConfigurableJoint>();
            axis = new Vector3(1.0f, 0.0f, 0.0f);
            target = 0.0f;

            BB.Rigidbody.angularDrag = Mathf.Infinity;
            //BB.Rigidbody.mass = 2f;

            angularYzDrive = this.myJoint.angularYZDrive;
            angularXdrive = this.myJoint.angularXDrive;

            float num1 = 100000f;
            angularXdrive.positionDamper = num1 * myJoint.transform.localScale.y;
            angularYzDrive.positionDamper = num1 * myJoint.transform.localScale.x;

            float num2 = Mathf.Infinity;
            angularXdrive.positionSpring = num2;
            angularYzDrive.positionSpring = num2;

            myJoint.angularYZDrive = angularYzDrive;
            myJoint.angularXDrive = angularXdrive;
            myJoint.targetAngularVelocity = axis * 0.0f;

            myJoint.breakForce = Mathf.Infinity;
            myJoint.breakTorque = Mathf.Infinity;
        }
        public void Start()
        {
            if (!StatMaster.isClient)
            {
                BB.Rigidbody.mass = massSlider.Value;
                if (smartTurret.IsActive)
                {
                    
                    try
                    {
                        //InitTurret();
                        aiming = false;
                        TurretStart();
                        steeringWheel.AngleToBe = 0f;
                        aimR2C = R2C.IsActive;
                        R2C.IsActive = false;

                    }
                    catch { }
                }
            }
        }
        public void FixedUpdate()
        {
            if (BB.isSimulating)
            {
                SimulateFixedUpdate();
            }
            else
            {
                
            }
        }
        public virtual void SimulateFixedUpdate()
        {
            if (!StatMaster.isClient)
                SimulateFixedUpdateHost();
        }
        public virtual void SimulateFixedUpdateHost()
        {
            if (BB != null)
            {
                if (BB.isSimulating)
                {
                    if (isFirstFrame)
                    {

                        if (!BB.noRigidbody)
                            BB.Rigidbody.WakeUp();

                        //Rigidbody.inertiaTensorRotation = new Quaternion(1, 0, 0, 1);
                        isFirstFrame = false;
                    }
                }
                else
                {
                    if (!isFirstFrame)
                    {
                        //aiming = false;
                        isFirstFrame = true;
                    }
                }
            }

            if (!smartTurret.IsActive)
                return;
            angularXdrive.positionSpring = Mathf.Infinity;
            angularYzDrive.positionSpring = Mathf.Infinity;
            if (!myJoint)
                return;
            Rigidbody connectedBody = myJoint.connectedBody;
            bool flag = !connectedBody.Equals(null);
            if (flag && connectedBody.isKinematic && (!BB.noRigidbody && BB.Rigidbody.isKinematic))
                return;

            if (!isFirstFrame)
            {

                if (aimKey.EmulationPressed())
                {
                    aiming = !aiming;
                }
                
                aimpos = TurretController.Instance.aimpos[BB.ParentMachine.PlayerID];
                if (aiming && TurretController.Instance.camBindA[BB.ParentMachine.PlayerID])
                {
                    if (!BB.noRigidbody && BB.Rigidbody.IsSleeping())
                        BB.Rigidbody.WakeUp();
                    if (!flag && connectedBody.IsSleeping())
                        connectedBody.WakeUp();

                    localaim = myJoint.transform.InverseTransformPoint(aimpos);
                    localaim.x *= myJoint.transform.localScale.x;
                    localaim.y *= myJoint.transform.localScale.y;
                    localaim.z = 0.0f;
                    localaim.Normalize();
                    if (localaim.magnitude < 1.0f)
                        return;
                    target = Vector3.Angle(new Vector3(1f, 0f, 0f), localaim);
                    if (target < 0.1f)
                        return;
                    float targetToBe = target;


                    if (targetToBe > speedSlider.Value)
                    {
                        targetToBe = speedSlider.Value;
                        //targetToBe *= Time.deltaTime * 200f;
                    }
                    else
                    {
                        if(BB.Rigidbody.velocity.magnitude>maxForceSlider.Value)
                        {
                            angularXdrive.positionSpring = 50f;
                            angularYzDrive.positionSpring = 50f;
                            return;
                        }
                    }
                    if (localaim.y < 0f)
                    {
                        targetToBe = -targetToBe;
                    }

                    if (!steeringWheel.allowLimits)
                    {
                        steeringWheel.AngleToBe -= targetToBe;
                    }
                    else
                    {
                        if(localaim.y < 0f)
                        {
                            if (steeringWheel.AngleToBe + target > 180f)
                            {
                                steeringWheel.AngleToBe += targetToBe;
                            }
                            else
                            {
                                steeringWheel.AngleToBe -= targetToBe;
                                if (steeringWheel.AngleToBe > mlimited.Max) 
                                {
                                    steeringWheel.AngleToBe = mlimited.Max;
                                }
                            }
                        }
                        else
                        {
                            if (steeringWheel.AngleToBe - target < -180f) 
                            {
                                steeringWheel.AngleToBe += targetToBe;
                            }
                            else
                            {
                                steeringWheel.AngleToBe -= targetToBe;
                                if (steeringWheel.AngleToBe<-mlimited.Min)
                                {
                                    steeringWheel.AngleToBe = -mlimited.Min;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if(aimR2C)
                    {
                        if (steeringWheel.AngleToBe > speedSlider.Value)
                            steeringWheel.AngleToBe -= speedSlider.Value;
                        else if (steeringWheel.AngleToBe < -speedSlider.Value)
                            steeringWheel.AngleToBe += speedSlider.Value;
                        else
                            steeringWheel.AngleToBe = 0f;
                    }
                }
            }
        }

        
        readonly GUIStyle vec3Style = new GUIStyle()
        {
            fontStyle = FontStyle.Bold,
            fontSize = 16,
            normal = { textColor = new Color(1, 1, 1, 1) },
            alignment = TextAnchor.MiddleCenter
        };
        
        
        void OnGUI()
        {
            string showmsg = "";
            /*
            try
            {
                showmsg += BB.GetComponent<ConfigurableJoint>().angularXDrive.positionDamper.ToString()+"\n";
            }
            catch { }
            try
            {
                showmsg += BB.GetComponent<ConfigurableJoint>().angularXDrive.positionSpring.ToString() + "\n";
            }
            catch { }
            showmsg += aiming.ToString() + "\n";
            showmsg += localaim.ToString() + "\n";
            showmsg += target.ToString() + "\n";
            showmsg += Time.deltaTime.ToString() + "\n"; 
            */
            showmsg += BB.tag+"\n";
            for (int i = 0; i < BB.transform.childCount; i++)
                showmsg += BB.transform.GetChild(i).name +","+ BB.transform.GetChild(i).tag+ "\n";

            GUI.Box(UIrect, showmsg);

        }
        
    }
}
