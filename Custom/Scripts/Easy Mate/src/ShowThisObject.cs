using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;

namespace geesp0t
{
    public class ShowThisObject : MVRScript
    {
        public void Start()
        {
            this.containingAtom.SetOn(true);
        }
    }
}