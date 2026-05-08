using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;

namespace geesp0t
{
    public class OutputBuiltInMorphsForVACUUM : MVRScript
    {
        public void Start()
        {
            IEnumerable<Atom> personAtoms = SuperController.singleton.GetAtoms().Where(a => a.type == "Person");

            List<string> builtInMorphUIDs = new List<string>();
            List<string> builtInMorphUIDs2 = new List<string>();

            foreach (Atom at in personAtoms)
            {
                //RESET DOLLMASTER
                JSONStorable geometry = at.GetStorableByID("geometry");
                if (geometry == null)
                {
                    continue;
                }
                DAZCharacterSelector character = geometry as DAZCharacterSelector;
                var morphUI = character.morphsControlUI;
                string testMorph = "Breast Large";
               
                morphUI.GetMorphDisplayNames().ToList().ForEach(name =>
                {
                    DAZMorph morph = morphUI.GetMorphByDisplayName(name);
                });

                morphUI.GetMorphDisplayNames().ToList().ForEach(name =>
                {
                    DAZMorph morph = morphUI.GetMorphByDisplayName(name);
                    if (!morph.uid.Contains("Custom/"))
                    {
                        builtInMorphUIDs.Add("\"" + morph.uid + "\"");
                    }

                    /*if (morph.displayName.Contains(testMorph))
                    {
                        SuperController.LogMessage(morph.uid + "\n" + morph.displayName + "\n" + morph.morphName + "\n");
                    }*/

                });

                morphUI.GetMorphUids().ToList().ForEach(name =>
                {
                    DAZMorph morph = morphUI.GetMorphByUid(name);
                    if (!morph.uid.Contains("Custom/"))
                    {
                        builtInMorphUIDs2.Add("\"" + morph.uid + "\"");
                    }

                    /*if (morph.displayName.Contains(testMorph))
                    {
                        SuperController.LogMessage(morph.uid + "\n" + morph.displayName + "\n" + morph.morphName + "\n");
                    }*/

                });


            }
            builtInMorphUIDs.AddRange(builtInMorphUIDs2);
            builtInMorphUIDs = builtInMorphUIDs.Distinct().ToList();

            JSONClass savedSettings = new JSONClass();
            savedSettings["bulitInMorphUIDs"] = string.Join(", ", builtInMorphUIDs.ToArray());



            SuperController.singleton.SaveJSON(savedSettings, "Custom/Scripts/Easy Mate/BuiltInMorphsList.json");
        }
    }
}