using System;
using UnityEngine;
/***********************************************************************************************************************************
 * prestigitis_DesktopClothGrab.cs
 * 
 * add this script as a session plugin to enable the mouse to grab clothing when the left alt key is held down. 
 * while holding down left alt, press Q to toggle the collider on and off.
 * 
 * the cloth must be next to an active collider, which is required for positioning the grab sphere.
 * settings can be changed in lines 21-23.
 * 
 * This work is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License
 * https://creativecommons.org/licenses/by-nc-sa/4.0/
 *
 ***********************************************************************************************************************************/
namespace prestigitis {
	public class DeskTopClothGrab_20200104 : MVRScript {
/***********************************************************************************************************************************/
/* configuration settings 
/***********************************************************************************************************************************/
protected KeyCode grabKey = KeyCode.LeftAlt;     //key to use for grabbing, default: KeyCode.LeftAlt
protected KeyCode colliderToggleKey = KeyCode.Q; //key for toggling collider, default: KeyCode.Q
protected int layerMask = (1 << 29) | 1;         //collision layers for determining starting grab position. 
                                                 //character layer = 29, character extremities = 26, default layer = 1
                                                 // e.g., (1 << 29) | (1 << 26) | 1;  
/***********************************************************************************************************************************/

        // IMPORTANT - DO NOT make custom enums. The dynamic C# complier crashes Unity when it encounters these for
        // some reason

        // IMPORTANT - DO NOT OVERRIDE Awake() as it is used internally by MVRScript - instead use Init() function which
        // is called right after creation

        protected bool debugMode;              //shows the grab sphere, default: false
        protected bool collisionOn;            //turns on/off the grab sphere's collider 
        protected float grabSize = 0.05f;      //the size of the grab sphere, default: 0.05f
        protected GameObject clothGrabPoint;
        protected float clothGrabPointScreenOffset = Mathf.Infinity;
        protected bool newGrab = false;

        protected JSONStorableBool debugModeJSON;
        protected JSONStorableBool collisionOnJSON;
        protected JSONStorableFloat grabScaleJSON;

        protected void SyncDebugMode(bool b)
        {
            debugMode = b;
        }
        protected void SyncCollision(bool b)
        {
            collisionOn = b;
        }

        public override void Init() {
			try {
                // put init code in here

                // create custom JSON storable params here if you want them to be stored with scene JSON
                // types are JSONStorableFloat, JSONStorableBool, JSONStorableString, JSONStorableStringChooser
                // JSONStorableColor
                debugModeJSON = new JSONStorableBool("debug mode", false, new JSONStorableBool.SetBoolCallback(SyncDebugMode));
                collisionOnJSON = new JSONStorableBool("collision", false, new JSONStorableBool.SetBoolCallback(SyncCollision));
                grabScaleJSON = new JSONStorableFloat("grab scale", 0.05f, 0.001f, 0.20f, false, true);

                RegisterBool(debugModeJSON);
                RegisterBool(collisionOnJSON);
                RegisterFloat(grabScaleJSON);

                CreateSlider(grabScaleJSON);
                CreateToggle(debugModeJSON);
                CreateToggle(collisionOnJSON);
            }
            catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

		// Start is called once before Update or FixedUpdate is called and after Init()
		void Start() {
            try
            {
                clothGrabPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                clothGrabPoint.transform.localScale = new Vector3(grabScaleJSON.val, grabScaleJSON.val, grabScaleJSON.val);
                clothGrabPoint.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");
                clothGrabPoint.GetComponent<Renderer>().material.color = new Color(1f, 0.5f, 0.5f, 0.5f);
                clothGrabPoint.GetComponent<Renderer>().enabled = false;
                clothGrabPoint.GetComponent<Collider>().enabled = false;
                //clothGrabPoint.name = "clothGrabPoint";
                GpuGrabSphere gs = clothGrabPoint.gameObject.AddComponent<GpuGrabSphere>();
                //gs.name = "DesktopClothGrabSphere";
                gs.radius = 1; //size is already controlled by the transform local scale set above, so set to 1
                gs.enabled = false;
            }
            catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

        // Update is called with each rendered frame by Unity
        void Update()
        {
            try
            {
                // put code in here
                if (Input.GetKeyDown(grabKey))
                {
                    newGrab = true;
                    clothGrabPoint.transform.localScale = new Vector3(grabScaleJSON.val, grabScaleJSON.val, grabScaleJSON.val);
                    clothGrabPoint.gameObject.transform.GetComponent<GpuGrabSphere>().enabled = true;
                    clothGrabPoint.GetComponent<Renderer>().enabled = debugMode;
                    clothGrabPoint.GetComponent<Collider>().enabled = collisionOn;
                }
                if (Input.GetKeyUp(grabKey))
                {
                    clothGrabPoint.gameObject.transform.GetComponent<GpuGrabSphere>().enabled = false;
                    clothGrabPoint.GetComponent<Renderer>().enabled = false;
                    clothGrabPoint.GetComponent<Collider>().enabled = false;
                }
                if (Input.GetKeyDown(colliderToggleKey) && Input.GetKey(grabKey))
                {
                    //toggle collider on/off
                    collisionOnJSON.val = !collisionOnJSON.val;
                    clothGrabPoint.GetComponent<Collider>().enabled = collisionOn;
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        // FixedUpdate is called with each physics simulation frame by Unity
        void FixedUpdate() {
			try {
                // put code in here
                if (clothGrabPoint.gameObject.transform.GetComponent<GpuGrabSphere>().enabled)
                {
                    if (newGrab)
                    {
                        RaycastHit hit = new RaycastHit();
                        Ray r = SuperController.singleton.MonitorCenterCamera.ScreenPointToRay(Input.mousePosition);
                        if (Physics.Raycast(r, out hit, Mathf.Infinity, layerMask))
                        {
                            clothGrabPoint.transform.position = hit.point; //move clothGrabPoint transform to hit point only at start of pull
                            clothGrabPointScreenOffset = SuperController.singleton.MonitorCenterCamera.WorldToScreenPoint(clothGrabPoint.transform.position).z;
                        }
                        newGrab = false;
                    }
                    else
                    {
                        clothGrabPoint.transform.position = SuperController.singleton.MonitorCenterCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, clothGrabPointScreenOffset));
                    }
                }
            }
            catch (Exception e) {
				SuperController.LogError("Exception caught: " + e);
            }
        }

		// OnDestroy is where you should put any cleanup
		// if you registered objects to supercontroller or atom, you should unregister them here
		void OnDestroy() {
            Destroy(clothGrabPoint);
        }
        public void OnDisable()
        {
        }
        public void OnEnable()
        {
        }
    }
}