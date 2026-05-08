using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace DillDoe
{
    public class ColorScale : MVRScript
    {
#region PluginInfo    
        public string pluginAuthor = "DillDoe";
        public string pluginName = "ColorScale";
        public string pluginVersion = "1.0";
        public string pluginDate = "11/30/2018";
        public string pluginDescription = @"
        This plugin allows you to change the material color and size of the custom assets.  
        When loaded it'll display the current prefab that is listed in the aList array (in Prefab Array below), to show that it has found the prefab and plugin will work.  
        You can manually enter the prefab name in the label to make it work.
        The material color targets the child meshes in the prefab.  A list of all child object will be listed in the text box below the scale siders.
        Select the child object with the Material Slider.  You can change all child color, but you currently can only save the current one.

        You are free to edit/change everything below this, please do not delete the PluginInfo. Append it if you modify the script";
        #endregion

#region Prefab Array
        //  List prefab name in Asset here.  Names are case sensitive!  Be sure to change the Array Size string[size] too.
        //  You can add to list or create new ones
        protected string[] aList = new string[26] {"Braces_Top_Black","Braces_Top_Blue", "Braces_Top_Clear", "Braces_Top_Pink", "Braces_Under_Black",
            "Braces_Under_Blue", "Braces_Under_Clear", "Braces_Under_Pink","Headgear_Top_Blue", "Headgear_Top_Blue_L",
            "Headgear_Top_Blue_R","Headgear_Top_Pink", "Headgear_Top_Pink_L", "Headgear_Top_Pink_R", "Headgear_Top_Strap",
            "Headgear_Under_Blue", "Headgear_Under_Blue_L", "Headgear_Under_Pink", "Headgear_Under_Pink_L","Headgear_Under_Strap",
            "Headgear_Wire", "Single_Black", "Single_Blue", "Single_Clear", "Single_Pink",
            "Wire" };
        #endregion

#region Vars
        protected JSONStorableColor dcolor;
        protected JSONStorableColor scolor;
        protected JSONStorableFloat gfloat;
        protected JSONStorableFloat mfloat;
        protected JSONStorableFloat afloat;
        protected JSONStorableFloat jfloatX;
        protected JSONStorableFloat jfloatY;
        protected JSONStorableFloat jfloatZ;
        protected JSONStorableFloat theChild;
        protected JSONStorableString jstring;
        protected JSONStorableString pstring;
        protected UIDynamicSlider dslider;
        protected UIDynamicTextField dtext;
        protected GameObject go;
        protected Vector3 oScale;
        protected int mIndex;
#endregion

#region Init
        public override void Init()
        {
            try
            {
                //  Search for current prefab for matching ones in Prefab Array
                foreach (string i in aList)
                {
                    // You need to add "(Clone)" to the name your prefab had when putting it in the AssetBundle. -MacGruber_VR
                    go = GameObject.Find(i + "(Clone)");
                    if (go != null)
                    {
                        //  Add found prefab name to plugin name label (next to Enabled checkbox)
                        //  This is also need for loading scenes, so DO NOT CHANGE THE LABEL BOX IN GAME!
                        pluginLabelJSON.val = i;
                        //  store default size for scale
                        oScale = go.GetComponent<Transform>().localScale;
                   
                        break;
                    }
                }

                // ************************************  Left Side  *************************************************************
               
                // Diffuse Color
                HSVColor hsvc = HSVColorPicker.RGBToHSV(1f, 0f, 0f);
                dcolor = new JSONStorableColor("Diffuse Color", hsvc, DiffuseColorCallback);
                RegisterColor(dcolor);
                CreateColorPicker(dcolor);

                // Specular Color
                HSVColor hsvc2 = HSVColorPicker.RGBToHSV(1f, 0f, 0f);
                scolor = new JSONStorableColor("Specular Color", hsvc, SpecColorCallback);
                RegisterColor(scolor);
                CreateColorPicker(scolor);

                //  Spec
                gfloat = new JSONStorableFloat("Glossiness", 0f, GlossyCallback, 0f, 1f, true);
                RegisterFloat(gfloat);
                dslider = CreateSlider(gfloat);

                //  Metallic
                mfloat = new JSONStorableFloat("Metallic", 0f, MetallicCallback, 0f, 1f, true);
                RegisterFloat(mfloat);
                dslider = CreateSlider(mfloat);

                //  Alpha
                afloat = new JSONStorableFloat("Alpha", 1f, AlphaCallback, 0f, 1f, true);
                RegisterFloat(afloat);
                dslider = CreateSlider(afloat);


                // ************************************  Right Side  *************************************************************

                // Scale X
                jfloatX = new JSONStorableFloat("Scale X", 0f, ScaleX, -1f, 1f, false, true);
                //jfloatX.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(jfloatX);
                CreateSlider(jfloatX, true);

                // Scale Y
                jfloatY = new JSONStorableFloat("Scale Y", 0f, ScaleY, -1f, 1f, false, true);
                RegisterFloat(jfloatY);
                CreateSlider(jfloatY, true);

                // Scale Z
                jfloatZ = new JSONStorableFloat("Scale Z", 0f, ScaleZ, -1f, 1f, false, true);
                RegisterFloat(jfloatZ);
                CreateSlider(jfloatZ, true);

                /*UIDynamic spacer = CreateSpacer(true);
                spacer.height = 240f;
                */               

                jstring = new JSONStorableString("Childs", "");
                dtext = CreateTextField(jstring,true);
                if (go != null)
                {
                    FindChild();
                }

                theChild = new JSONStorableFloat("Material", 1f, ChildCallback, 1f, 10);
                RegisterFloat(theChild);
                dslider = CreateSlider(theChild, true);
                dslider.slider.wholeNumbers = true;

                pstring = new JSONStorableString("Choosen", "");
                dtext = CreateTextField(pstring, true);

            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }
        #endregion

#region Update
        void Update()
        //public virtual void OnPostLoad()
        {
            try
            {
                //  There seems to be a slight delay that prevents finding gameobject on load,
                //  so keep trying until we find it.
                if (go == null)
                {
                    go = GameObject.Find(pluginLabelJSON.val + "(Clone)");
                    if (go != null)
                    {
                        LoadVal();
                    }
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }
        #endregion

#region Load Values
        //  Load all the saved values from plugin UI back into Asset
        void LoadVal()
        {
            try
            {
                //  store default size for scale
                oScale = go.GetComponent<Transform>().localScale;

                ScaleX(jfloatX);
                ScaleY(jfloatY);
                ScaleZ(jfloatZ);

                FindChild();
                ChildCallback(theChild);

                DiffuseColorCallback(dcolor);
                SpecColorCallback(scolor);
                GlossyCallback(gfloat);
                MetallicCallback(mfloat);
                AlphaCallback(afloat);

            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }
#endregion

#region UI Functions
        //	Scale		
        protected void ScaleX(JSONStorableFloat jX)
        {
            if (go != null)
            {
                Vector3 ss = go.GetComponent<Transform>().localScale;
                go.GetComponent<Transform>().localScale = new Vector3(jX.val + oScale.x, ss.y, ss.z);
            }
        }

        protected void ScaleY(JSONStorableFloat jY)
        {
            if (go != null)
            {
                Vector3 ss = go.GetComponent<Transform>().localScale;
                go.GetComponent<Transform>().localScale = new Vector3(ss.x, jY.val + oScale.y, ss.z);
            }
        }

        protected void ScaleZ(JSONStorableFloat jZ)
        {
            if (go != null)
            {
                Vector3 ss = go.GetComponent<Transform>().localScale;
                go.GetComponent<Transform>().localScale = new Vector3(ss.x, ss.y, jZ.val + oScale.z);
            }
        }

        //  find all the child object in prefab to change each material color
        protected void FindChild()
        {            
            if (go != null)
            {   // todo: create dynamic array to hold color info for each prefab to save with scene
                //Dictionary<string, HSVColor> prefabChild = new Dictionary<string, HSVColor>();
                float x = 0f;
                foreach (var ob in go.GetComponentsInChildren<Renderer>())
                {
                    x++;
                    jstring.val += x + ". " + ob.name + "\n";
                }             
            }
        }
        // prefab selector
        protected void ChildCallback(JSONStorableFloat jC)
        {
            if (go != null)
            {
                mIndex = (int)jC.val - 1;
                pstring.val = go.GetComponentsInChildren<Renderer>()[mIndex].name;
            }
        }
        //  material color
        protected void DiffuseColorCallback(JSONStorableColor dc)
        {
            if (go != null)
            {
                go.GetComponentsInChildren<Renderer>()[mIndex].material.SetColor("_Color", dcolor.colorPicker.currentColor);
                AlphaCallback(afloat);
            }
        }

        protected void SpecColorCallback(JSONStorableColor sc)
        {
            if (go != null)
            {
                go.GetComponentsInChildren<Renderer>()[mIndex].material.SetColor("_SpecColor", scolor.colorPicker.currentColor);
            }
        }

        protected void GlossyCallback(JSONStorableFloat jf)
        {
            if (go != null)
            {
                go.GetComponentsInChildren<Renderer>()[mIndex].material.SetFloat("_Glossiness", jf.val);
            }
        }

        protected void MetallicCallback(JSONStorableFloat jm)
        {
            if (go != null)
            {
                go.GetComponentsInChildren<Renderer>()[mIndex].material.SetFloat("_Metallic", jm.val);
            }
        }

        protected void AlphaCallback(JSONStorableFloat ja)
        {
            if (go != null)
            {
                Color colorA = go.GetComponentsInChildren<Renderer>()[mIndex].material.color;
                colorA.a = ja.val;
                go.GetComponentsInChildren<Renderer>()[mIndex].material.SetColor("_Color", colorA);

            }
        }
    }

#endregion
}