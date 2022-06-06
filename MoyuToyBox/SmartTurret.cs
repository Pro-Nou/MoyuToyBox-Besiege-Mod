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
    public class SmartTurret : BlockScript
    {
        public GameObject Vis;
        private ConfigurableJoint myJoint;
        private ConfigurableJointTargetAngle myJointA;
        private TriggerSetJoint triggerSetJoint;
        //private Steering steeringWheel;

        private Collider pickedCollider;
        private int iconSize = 64;
        private Texture Aicon;
        private MKey picker;
        private MKey resetPicker;
        public MKey aim;
        public bool aiming;
        public float target;
        public float rotateSpeed;
        public MSlider rotateSpeedSlider;
        public float maxForce;
        public MSlider maxForceSlider;
        public bool Limited;
        public MLimits mlimited;
        public MToggle invisible;
        public MToggle auto;
        public MToggle R2C;
        public float maxA = 0.0f;
        public float minA = 0.0f;
        public float maxV = 0.0f;

        //private FixedCameraController cameraController;
        private bool isFirstFrame = true;
        private Rect UIrect = new Rect(0, 100, 512, 256);
        private Vector3 aimpos;
        private Vector3 axis;
        private Vector3 localaim;
        private Vector3 aimEular;
        private Vector3 orgLocalScale;
        private JointDrive angularYzDrive;
        private JointDrive angularXdrive;
        private Vector3 jointEulerRotation;
        private float rotateSpeedCos;

        private bool clusterUpdated = false;



        public override void OnPrefabCreation()
        {

            //BlockBehaviour.Prefab.gameObject = Instantiate(PrefabMaster.BlockPrefabs[(int)BlockType.SteeringBlock].gameObject);
            
        }
        public override void SafeAwake()
        {
            Vis = BlockBehaviour.gameObject.transform.FindChild("Vis").gameObject;

            BlockBehaviour.tag = "MechanicalTag";
            //BlockBehaviour.Prefab.Type = BlockType.SteeringBlock;
            //BlockBehaviour.Prefab = PrefabMaster.BlockPrefabs[(int)BlockType.SteeringBlock];
            aim = AddKey("启用瞄准", "aim", KeyCode.Alpha1);
            picker = AddKey("手动锁定", "picker", KeyCode.None);
            resetPicker = AddKey("重置手动锁定", "reset", KeyCode.None);
            auto = AddToggle("自动","auto",false);
            invisible = AddToggle("不可见", "invisible", false);
            R2C = AddToggle("自回正", "return to center", false);
            FauxTransform a=new FauxTransform(new Vector3(0, 0, 0), Quaternion.Euler(180,0,0), new Vector3(0.4f, 0.4f, 0.4f));
            mlimited = AddLimits("角度限制", "RotateLimit", 45f, 45f, 170f, a);
            //aiming = false;




            //GetComponent<Modding.Modules.Official.SteeringModuleBehaviour>().enabled = false;


            rotateSpeedSlider = AddSlider("转速", "rotateSpeed", 1.0f, 0.0f, 9.0f);
            maxForceSlider = AddSlider("受力阈值", "maxForce", 0.5f, 0.0f, 1.0f);
            
            myJoint = gameObject.GetComponent<ConfigurableJoint>()?? gameObject.AddComponent<ConfigurableJoint>();
            myJoint.angularXMotion = ConfigurableJointMotion.Locked;
            myJoint.angularYMotion = ConfigurableJointMotion.Free;
            myJoint.angularZMotion = ConfigurableJointMotion.Locked;
            axis = new Vector3(0.0f, 1.0f, 0.0f);

            //steeringWheel = base.gameObject.GetComponent<Steering>();
            BlockBehaviour.Prefab.mechanicalJoint = true;
            triggerSetJoint = BlockBehaviour.gameObject.transform.FindChild("TriggerForJoint").gameObject.GetComponent<TriggerSetJoint>();
            triggerSetJoint.gameObject.tag = "MechanicalTag";
            triggerSetJoint.isDynamicLink = true;

            Aicon = ModResource.GetTexture("turret Icon").Texture;
            BlockBehaviour.gameObject.name = "SmartTurret";




        }
        public override void BuildingUpdate()
        {
            /*
            if (!clusterUpdated)
            {
                try
                {
                    BlockBehaviour.SetClusterIndexNoUpdate(BlockBehaviour.ParentMachine.ClusterCount);
                    BlockBehaviour.ParentMachine.RebuildExistingClusters(new List<BlockBehaviour> { BlockBehaviour });
                    clusterUpdated = false;
                }
                catch { }
            }*/

            picker.DisplayInMapper = resetPicker.DisplayInMapper = TurretController.Instance.directorMode;
            Limited = mlimited.IsActive;
            maxA = mlimited.Max;
            minA = -mlimited.Min;

        }
        public void Start()
        {
            pickedCollider = null;
            if (BlockBehaviour.isSimulating)
                return;
            if (!BlockBehaviour.noRigidbody)
                BlockBehaviour.Rigidbody.maxAngularVelocity = 100f;
            
            target = 0.0f;

            //BlockBehaviour.Rigidbody.angularDrag = Mathf.Infinity;
            
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
            /*
            JointDrive xdrive = myJoint.xDrive;
            JointDrive ydrive = myJoint.yDrive;
            JointDrive zdrive = myJoint.zDrive;
            SoftJointLimit jointLimit = myJoint.linearLimit;
            SoftJointLimitSpring jointLimitSpring = myJoint.linearLimitSpring;
            

            jointLimitSpring.spring = 0.0f;
            myJoint.linearLimit = jointLimit;
            myJoint.linearLimitSpring = jointLimitSpring;

            float num3 = 0.0f;
            //xdrive.positionDamper = num1;
            //ydrive.positionDamper = num1;
            //zdrive.positionDamper = num1;
            xdrive.positionSpring = num3;
            ydrive.positionSpring = num3;
            zdrive.positionSpring = num3;


            myJoint.xDrive = xdrive;
            myJoint.yDrive = ydrive;
            myJoint.zDrive = zdrive;
            */

            
            
            myJoint.breakForce = Mathf.Infinity;
            myJoint.breakTorque = Mathf.Infinity;
            
            //myJoint.projectionAngle = 0.1f;
            //myJoint.rotationDriveMode
            //myJoint.projectionDistance = 0.1f;
            //myJoint.projectionAngle = 0.1f;

        }
        //[XmlRoot("R2CSteering")]
        /*public class Steering: BlockModule
        {

            
        }
        public class SteeringBehaviour : BlockModuleBehaviour<Steering>
        {
            
        }*/
        private void Update()
        {
            if (!isFirstFrame||BlockBehaviour.isSimulating)
            {
                if (!auto.IsActive)
                {
                    if (aim.IsPressed)
                    {
                        aiming = !aiming;
                    }
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
        public override void OnSimulateStart()
        {
            if (invisible.IsActive)
                Vis.SetActive(false);
        }
        public override void OnSimulateStop()
        {

        }
        public override void SimulateFixedUpdateHost()
        {
            //第一帧判断
            //BlockBehaviour.ParentMachine.PlayerID
            angularXdrive.positionSpring = Mathf.Infinity;
            angularYzDrive.positionSpring = Mathf.Infinity;
            //myJoint.angularXDrive = angularXdrive;
            //myJoint.angularYZDrive = angularYzDrive;

            if (BlockBehaviour != null)
            {
                if (BlockBehaviour.isSimulating)
                {
                    if (isFirstFrame)
                    {
                        
                        if (HasRigidbody)
                            Rigidbody.WakeUp();

                        rotateSpeed = Mathf.Max(0f, rotateSpeedSlider.Value);
                        maxForce = Mathf.Max(0f, maxForceSlider.Value);

                        if (rotateSpeed < 0.0f)
                            rotateSpeed = 0.0f;
                        //cameraController = FindObjectOfType<FixedCameraController>();
                        rotateSpeedCos = Mathf.Cos(Mathf.PI * rotateSpeed / 180.0f);

                        orgLocalScale = myJoint.transform.localScale;

                        if (auto.IsActive)
                            aiming = true;
                        else
                            aiming = false;
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
            /*
            if (!hasStarted)
            {
                if (this.startFrames == 3)
                {
                    if (HasRigidbody)
                        Rigidbody.WakeUp();
                    hasStarted = true;
                    
                    if ((double)this.FlipInvert == 1.0)
                    {
                        this.AngleMin = this.limits.get_Min();
                        this.AngleMax = this.limits.get_Max();
                    }
                    else if ((double)this.FlipInvert == -1.0)
                    {
                        this.AngleMin = this.limits.get_Max();
                        this.AngleMax = this.limits.get_Min();
                    }
                    if (this.FlipLimits)
                    {
                        float angleMin = this.AngleMin;
                        this.AngleMin = this.AngleMax;
                        this.AngleMax = angleMin;
                    }
                    this.MenuChoice = this.ModeMenu.get_Value();
                }
                else
                {
                    ++startFrames;
                    return;
                }
            }
            */
            //已开始模拟
            if (!myJoint)
                return;
            Rigidbody connectedBody = myJoint.connectedBody;
            bool flag = !connectedBody.Equals(null);
            if (flag && connectedBody.isKinematic && (!HasRigidbody && Rigidbody.isKinematic))
                return;
            if (!isFirstFrame||BlockBehaviour.isSimulating)
            {
                if (!auto.IsActive)
                {
                    if (aim.EmulationPressed())
                    {
                        aiming = !aiming;
                    }
                }
                //BlockBehaviour.transform.localPosition = selflocalpos;
                //selfpos = BlockBehaviour.GetTransform().position;
                //BlockBehaviour.FreezeMe();
                if (HasRigidbody && Rigidbody.IsSleeping())
                    Rigidbody.WakeUp();
                if (!flag && connectedBody.IsSleeping())
                    connectedBody.WakeUp();
                if (!TurretController.Instance.directorMode)
                    aimpos = TurretController.Instance.aimpos[BlockBehaviour.ParentMachine.PlayerID];
                else
                {
                    if (pickedCollider == null)
                    {
                        aimpos = TurretController.Instance.aimpos[BlockBehaviour.ParentMachine.PlayerID];
                    }
                    else
                    {
                        aimpos = pickedCollider.transform.position;
                    }
                }

                if (aiming && (TurretController.Instance.camBindA[BlockBehaviour.ParentMachine.PlayerID] || TurretController.Instance.directorMode))  
                {
                    if (!TurretController.Instance.camBindA[BlockBehaviour.ParentMachine.PlayerID] && pickedCollider == null)
                        return;
                    //if (BlockBehaviour.Rigidbody.velocity.magnitude > 0.01f)
                    //    return;

                    //BlockBehaviour.GetTransform().Rotate(new Vector3(0.0f, 1.0f, 0.0f), 0.1f);
                    //SteeringWheel.GetTransform().LookAt(aimpos);
                    //steeringWheel.AngleToBe = Mathf.MoveTowardsAngle(steeringWheel.AngleToBe, target, 0.1f);
                    //target += rotateSpeed;
                    //myJoint.transform.localScale = new Vector3(1, 1, 1);
                    localaim = myJoint.transform.InverseTransformPoint(aimpos);
                    //myJoint.transform.localScale = orgLocalScale;
                    localaim.x *= myJoint.transform.localScale.x;
                    localaim.y *= myJoint.transform.localScale.y;
                    localaim.z = 0.0f;
                    localaim.Normalize();
                    if (localaim.magnitude < 1.0f)
                        return;
                    //aimEular=Quaternion.LookRotation(localaim, new Vector3(0.0f, 0.0f, 1.0f)).eulerAngles;
                    //
                    //localaim.Normalize();
                    //if (Mathf.Abs(localaim.y) > 0.99f)
                    //    return;
                    //localaim.Normalize();
                    //localaim.y = 0.0f;
                    if (!Limited)
                    {
                        if (localaim.y >= rotateSpeedCos)
                        {
                            if (Rigidbody.angularVelocity.magnitude > maxForce)
                            {
                                angularXdrive.positionSpring = 50f;
                                angularYzDrive.positionSpring = 50f;
                                //myJoint.angularXDrive = angularXdrive;
                                //myJoint.angularYZDrive = angularYzDrive;
                                return;
                            }
                            //target = target;
                            target += Mathf.Asin(localaim.x) / Mathf.PI * 90f;
                            //return;
                        }
                        else
                        {
                            if (localaim.x < 0.0f)
                                target -= rotateSpeed;
                            else if (localaim.x >= 0.0f)
                                target += rotateSpeed;
                        }
                    }
                    else if(Limited)
                    {
                        if (localaim.y >= rotateSpeedCos)
                        {
                            if (Rigidbody.angularVelocity.magnitude > maxForce)
                            {
                                angularXdrive.positionSpring = 50f;
                                angularYzDrive.positionSpring = 50f;
                                //myJoint.angularXDrive = angularXdrive;
                                //myJoint.angularYZDrive = angularYzDrive;
                                return;
                            }
                            float angleL=Mathf.Asin(localaim.x) / Mathf.PI * 90f;
                            if (angleL > 0)
                            {
                                if (target + angleL > maxA)
                                    target = maxA;
                                else
                                    target += angleL;
                            }
                            else if(angleL<0)
                            {
                                if (target + angleL < minA)
                                    target = minA;
                                else
                                    target += angleL;
                            }
                        }
                        else if(localaim.y==-1.0f)
                        {
                            if(target<0)
                            {
                                if (target - rotateSpeed > minA)
                                    target -= rotateSpeed;
                                else
                                    target = minA;
                            }
                            else
                            {
                                if (target + rotateSpeed < maxA)
                                    target += rotateSpeed;
                                else
                                    target = maxA;
                            }
                        }
                        else
                        {
                            float angleL= Mathf.Acos(localaim.y) / Mathf.PI * 180.0f;
                            if (localaim.x < 0)
                                angleL = -angleL;
                            angleL += target;
                            if (angleL > 180.0f)
                                angleL -= 360.0f;
                            else if (angleL < -180.0f)
                                angleL += 360.0f;
                            if(angleL>=-180f&&angleL<=minA)
                            {
                                if (target - rotateSpeed > minA) 
                                    target -= rotateSpeed;
                                else
                                    target = minA;
                            }
                            else if(angleL>minA&&angleL<target)
                            {
                                target -= rotateSpeed;
                            }
                            else if(angleL>target&&angleL<maxA)
                            {
                                target += rotateSpeed;
                            }
                            else if(angleL<=180.0f&&angleL>=maxA)
                            {
                                if (target + rotateSpeed < maxA)
                                    target += rotateSpeed;
                                else
                                    target = maxA;
                            }
                                /*
                            if(angleL>=minA&&angleL<=maxA)
                            {
                                if (angleL > target)
                                    target += rotateSpeed;
                                else if (angleL < target)
                                    target -= rotateSpeed;
                            }
                            else if(angleL>maxA)
                            {
                                if(angleL<=180f)
                                {
                                    if (target + rotateSpeed <= maxA)
                                        target += rotateSpeed;
                                    else
                                        target = maxA;
                                }
                                else
                                {
                                    angleL -= 360f;
                                }
                            }*/
                        }
                    }
                    if (target >= 180.0f)
                        target -= 360.0f;
                    else if (target <= -180.0f)
                        target += 360.0f;
                    //myJoint.targetRotation = Quaternion.LookRotation(localaim, new Vector3(0.0f, 1.0f, 0.0f));

                }
                else
                {
                    if (R2C.IsActive)
                    {
                        if (target > rotateSpeed)
                            target -= rotateSpeed;
                        else if (target < -rotateSpeed)
                            target += rotateSpeed;
                        else
                            target = 0.0f;
                    }

                }
                if (BlockBehaviour.Rigidbody.mass >= myJoint.connectedBody.mass)
                    jointEulerRotation.y = axis.y * target;
                else
                    jointEulerRotation.y = axis.y * -target;
                
                //BlockBehaviour.originalMass = Mathf.Infinity;
                //BlockBehaviour.Rigidbody.angularVelocity = new Vector3(0, 0, 0);
                myJoint.targetRotation = Quaternion.Euler(jointEulerRotation);
                //myJoint.targetVelocity = BlockBehaviour.Rigidbody.velocity;
                //BlockBehaviour.Rigidbody.angularVelocity = new Vector3(0, 0, 0);
                //BlockBehaviour.originalMass = 1.0f;

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


            //for (int i = 0; i < BlockBehaviour.transform.childCount; i++)
            //    Showmsg += BlockBehaviour.transform.GetChild(i).name + "\n";


            //Showmsg += BlockBehaviour.blockJoint.connectedBody.gameObject.GetComponent<BlockBehaviour>().ClusterIndex.ToString();
            /*
            string Showmsg = "";
            Showmsg += BlockBehaviour.tag + "\n";
            for (int i = 0; i < BlockBehaviour.transform.childCount; i++)
                Showmsg += BlockBehaviour.transform.GetChild(i).name + "," + BlockBehaviour.transform.GetChild(i).tag + "\n";
            GUI.Box(UIrect, Showmsg);
            */
            if (TurretController.Instance.directorMode && !StatMaster.hudHidden)
            {
                if (pickedCollider != null)
                {
                    try
                    {
                        //GUI.color = new Color(0.5f, 0.5f, 0.5f, 1f);
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
