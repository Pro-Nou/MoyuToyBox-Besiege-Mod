using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Modding;

using Modding.Common;
using UnityEngine;


namespace MoyuToyBox
{
    class TurretController: SingleInstance<TurretController>
    {
        public override string Name { get; } = "Turret Controller";
        /*
        public GameObject PROJECTILES;
        public ProjectileManager MyProjectileManager;
        */
        public bool showGUI = true;
        private Rect windowRect = new Rect(15f, 100f, 280f, 50f + 20f);
        public float health;
        public float breakForce;
        public float breakTorque;
        public float forceInit;
        public bool hasBVC = false;
        public int MyGUID = 0;

        public float maxlength;
        public float safetylength;
        public float lastmaxlength;
        public float lastsafetylength;
        public float[] safetylengthArray = new float[16];
        public float[] maxlengthArray = new float[16];
        public Vector3[] aimpos=new Vector3[16];
        public Vector3[] lastaimpos = new Vector3[16];
        public Vector3[] thisaimpos = new Vector3[16];
        public float[] rcvPassed = new float[16];
        //private bool isplayer = false;
        public Ray[] ray=new Ray[16];
        //public float maxdis = 200.0f;
        public static Texture2D SquareAim = new Texture2D(90, 90);
        public bool showAim = true;
        public FixedCameraController cameraController=null;
        private bool isFirstFrame = true;
        public bool camBind = false;
        public bool[] camBindA = new bool[16];
        public bool[] sended = new bool[16];
        public float sendCD = 0.2f;

        public int PlayerID;
        private readonly int windowID = ModUtility.GetWindowId();

        public bool directorMode = false;

        public TurretController()
        {


            maxlength = 500.0f;
            safetylength = 0.0f;
            for(int i=0;i<16;i++)
            {
                maxlengthArray[i] = 500.0f;
                safetylengthArray[i] = 0.0f;
                //camBindA[i] = false;
                sended[i] = false;
                rcvPassed[i] = 0f;
            }
            lastmaxlength = maxlength;
            lastsafetylength = safetylength;

            SquareAim.LoadImage(Modding.ModIO.ReadAllBytes(@"Resources/aim2.png"));

            //maxlength = 500.0f;
        }

        private void Awake()
        {

        }
        private void Update()
        {

        }
        private void start()
        {

            if (StatMaster.isMP || StatMaster.IsLevelEditorOnly)
                PlayerID = PlayerData.localPlayer.networkId;
            //cameraController = FindObjectOfType<FixedCameraController>();
        }
        public void FixedUpdate()
        {
            if (StatMaster.isMP || StatMaster.IsLevelEditorOnly)
            {
                if (PlayerData.localPlayer.machine != null)
                {
                    if (PlayerData.localPlayer.machine.isSimulating)
                    {
                        if (isFirstFrame)
                        {
                            //PROJECTILES = GameObject.Find("PROJECTILES");
                            //MyProjectileManager = PROJECTILES.GetComponent<ProjectileManager>();


                            try
                            {
                                cameraController = FindObjectOfType<FixedCameraController>();
                            }
                            catch { }
                            PlayerID = PlayerData.localPlayer.networkId;
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
            }
            else
            {
                if (StatMaster.levelSimulating)
                {
                    if (isFirstFrame)
                    {
                        try
                        {
                            cameraController = FindObjectOfType<FixedCameraController>();
                        }
                        catch { }
                        PlayerID = 0;
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

            if(!isFirstFrame)
            {
                try
                {
                    if (cameraController == null)
                        camBind = false;
                    else if (cameraController.activeCamera)
                        camBind = true;
                    else
                        camBind = false;
                }
                catch { }
                
            }
            //if (PlayerMachine.GetLocal() != null)
            {
                //if (PlayerMachine.GetLocal().InternalObject.isSimulating)
                {
                    //Ray ray=new Ray();

                    maxlengthArray[PlayerID] = maxlength;
                    safetylengthArray[PlayerID] = safetylength;
                    if (StatMaster.levelSimulating)
                    {
                        ray[PlayerID].origin = Camera.main.transform.position;
                        ray[PlayerID].direction = Camera.main.transform.forward;
                        camBindA[PlayerID] = camBind;
                        
                        if (StatMaster.isHosting || StatMaster.isLocalSim)
                        {

                            for (int i = 0; i < 16; i++)
                            {

                                if (!Playerlist.Contains((ushort)i))
                                    continue;

                                if (i != 0 && !sended[i])
                                //    SendToPlayer(i);
                                    StartCoroutine(SendToPlayer(i));
                                if (!camBindA[i])
                                    continue;

                                
                                RaycastHit hit=new RaycastHit();
                                Ray rayH = new Ray(ray[i].origin + ray[i].direction * safetylengthArray[i], ray[i].direction);
                                if (Physics.Raycast(rayH, out hit, maxlengthArray[i] - safetylengthArray[i], Game.BlockEntityLayerMask, QueryTriggerInteraction.Ignore)) 
                                {
                                    //isplayer = false;

                                    //if (hit.distance < safetylengthArray[i])
                                    //    aimpos[i] = ray[i].origin + ray[i].direction * safetylengthArray[i];
                                    //else
                                    {
                                        aimpos[i] = hit.point;                          
                                    }
                                    /*
                                    try
                                    { 
                                        hit.rigidbody.WakeUp();
                                        hasBVC = hit.rigidbody.gameObject.GetComponent<BlockBehaviour>().Prefab.hasBVC;
                                    }
                                    catch { }*/
                                    /*
                                    try
                                    {
                                        breakForce = hit.rigidbody.gameObject.GetComponent<BlockBehaviour>().blockJoint.breakForce;
                                    }
                                    catch { }
                                    try
                                    {
                                        breakTorque = hit.rigidbody.gameObject.GetComponent<BlockBehaviour>().blockJoint.breakTorque;
                                    }
                                    catch { }
                                    try
                                    {
                                        forceInit = hit.rigidbody.gameObject.GetComponent<BlockBehaviour>().jointBreakForce;
                                    }
                                    catch { }*/

                                }
                                else
                                    aimpos[i] = rayH.origin + rayH.direction * (maxlengthArray[i] - safetylengthArray[i]);
                            }
                        }
                        else if(!StatMaster.isMP && !StatMaster.IsLevelEditorOnly && camBindA[PlayerID])
                        {
                            RaycastHit hit = new RaycastHit();
                            Ray rayH = new Ray(ray[PlayerID].origin + ray[PlayerID].direction * safetylengthArray[PlayerID], ray[PlayerID].direction);
                            if (Physics.Raycast(rayH, out hit, maxlengthArray[PlayerID] - safetylengthArray[PlayerID], ~(1 << 0),QueryTriggerInteraction.Ignore))
                            {
                                aimpos[PlayerID] = hit.point;
                            }
                            else
                                aimpos[PlayerID] = rayH.origin + rayH.direction * (maxlengthArray[PlayerID] - safetylengthArray[PlayerID]);
                        }
                        else if (!StatMaster.isHosting && !StatMaster.isLocalSim)
                        {
                            Message rayToHostMsg = Messages.cameraRay.CreateMessage(ray[PlayerID].origin, ray[PlayerID].direction,camBind);
                            ModNetworking.SendToHost(rayToHostMsg);
                            if (lastmaxlength != maxlength)
                            {
                                lastmaxlength = maxlength;
                                Message maxLToHostMsg = Messages.maxLength.CreateMessage((float)lastmaxlength);
                                ModNetworking.SendToHost(maxLToHostMsg);
                            }
                            if (lastsafetylength != safetylength)
                            {
                                lastsafetylength = safetylength;
                                Message safeLToHostMsg = Messages.safetyLength.CreateMessage((float)lastsafetylength);
                                ModNetworking.SendToHost(safeLToHostMsg);
                            }
                            for(int i=0;i<16;i++)
                            {

                                aimpos[i] = Vector3.Lerp(lastaimpos[i], thisaimpos[i], rcvPassed[i] / sendCD);
                                rcvPassed[i] += Time.unscaledDeltaTime;
                            }

                        }
                    }
                }
            }
                
        }
        private System.Collections.IEnumerator SendToPlayer(int i)
        {
            sended[i] = true;
            ModNetworking.SendTo(Player.From((ushort)i), Messages.SendCDInfo.CreateMessage(sendCD));
            for (int count=0; count < 16; count++)
            {
                if(Playerlist.Contains((ushort)count))
                    ModNetworking.SendTo(Player.From((ushort)i), Messages.AimInfo.CreateMessage(count,camBindA[count],aimpos[count]));
            }
            yield return new WaitForSecondsRealtime(sendCD);
            sended[i] = false;
            yield break;
        }
        private void OnGUI()
        {
            
            if (!StatMaster.hudHidden)
            {
                windowRect = GUILayout.Window(windowID, windowRect, new GUI.WindowFunction(ToyBoxWindow), "Camera tracking setting");
                if (StatMaster.isMP || StatMaster.IsLevelEditorOnly)
                {
                    if (PlayerData.localPlayer.machine.isSimulating && showAim)
                        GUI.DrawTexture(new Rect(Screen.width / 2 - 45, Screen.height / 2 - 8, 90, 90), SquareAim);
                }
                else
                {
                    if(StatMaster.levelSimulating && showAim)
                        GUI.DrawTexture(new Rect(Screen.width / 2 - 45, Screen.height / 2 - 8, 90, 90), SquareAim);
                }
                
            }
        }

        private void ToyBoxWindow(int windowID)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            {
                /*
                if (!StatMaster.isClient)
                {
                    EnhancementBlock.EnhanceMore = GUILayout.Toggle(EnhancementBlock.EnhanceMore, LanguageManager.additionalFunction);

                    if (Friction != GUILayout.Toggle(Friction, new GUIContent(LanguageManager.unifiedFriction, "dahjksdhakjsd")))
                    {
                        Friction = !Friction;
                        OnFrictionToggle(Friction);
                    }
                }*/

                GUILayout.Label("Raycast max length(100-1000)");
                string maxlengthStr = GUILayout.TextField(maxlength.ToString());
                maxlength = Convert.ToSingle(maxlengthStr);
                if (maxlength < 100.0f)
                    maxlength = 100.0f;
                if (maxlength > 1000.0f)
                    maxlength = 1000.0f;
                maxlength=GUILayout.HorizontalSlider(maxlength, 100.0f, 1000.0f);


                GUILayout.Label("Raycast safety length(0-100)");
                string safetyLengthStr = GUILayout.TextField(safetylength.ToString());
                safetylength = Convert.ToSingle(safetyLengthStr);
                if (safetylength < 0.0f)
                    safetylength = 0.0f;
                if (safetylength > 100.0f)
                    safetylength = 100.0f;
                safetylength = GUILayout.HorizontalSlider(safetylength, 0.0f, 100.0f);

                if (StatMaster.isHosting)
                {
                    GUILayout.Label("Network frame length(0.2s-2s)");
                    string sendCDStr = GUILayout.TextField(sendCD.ToString());
                    sendCD = Convert.ToSingle(sendCDStr);
                    if (sendCD < 0.2f)
                        sendCD = 0.2f;
                    if (sendCD > 2.0f)
                        sendCD = 2.0f;
                    sendCD = GUILayout.HorizontalSlider(sendCD, 0.2f, 2.0f);
                }

                showAim = GUILayout.Toggle(showAim, "Show front sight");
                GUILayout.Label("Camera Enable:"+ camBind.ToString());

                if (StatMaster.IsLevelEditorOnly || !StatMaster.isMP)
                {
                    directorMode = GUILayout.Toggle(directorMode, "Director Mode");
                }
                else
                {
                    directorMode = false;
                }
                RGBController.Instance.fixedRGB= GUILayout.Toggle(RGBController.Instance.fixedRGB, "Fixed RGB");
                //GUILayout.Label(Input.mousePosition.ToString());
                /*
                for (int i = 0; i < 16; i++)
                {
                    if (Playerlist.Contains((ushort)i))
                        GUILayout.Label(i.ToString() + ":"+camBindA[i].ToString() + aimpos[i].ToString());
                }
                */
                /*
                try
                {
                    GUILayout.Label(MyProjectileManager.name);
                    GUILayout.Label(PROJECTILES.transform.childCount.ToString());
                }
                catch { }
                */
                /*
                if (StatMaster.isHosting)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        if (Playerlist.Contains((ushort)i))
                            GUILayout.Label(i.ToString() + ":" + aimpos[i]);
                    }
                }
                
                else if (!StatMaster.isHosting && StatMaster.levelSimulating)
                {
                    GUILayout.Label(((int)PlayerData.localPlayer.machine.PlayerID).ToString());
                }*/
                //GUILayout.Label(hasBVC.ToString());

                /*
                try
                {
                    for (int i = 0; i < 16; i++)
                    {
                        if (!Playerlist.Contains((ushort)i))
                            continue;

                        GUILayout.Label(i.ToString());
                        foreach (var a in TorpedoController.Instance.TD[i])
                        {
                            string output = a.Key.ToString() + ":";
                            foreach (var b in a.Value)
                            {
                                output += b.ToString() + " ";
                            }
                            GUILayout.Label(TorpedoController.Instance.TD[i].Count.ToString() + output.ToString());
                        }
                    }

                }
                catch { }
                */
            }
            
            GUILayout.Space(2);
            GUILayout.EndVertical();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }
    }
}
