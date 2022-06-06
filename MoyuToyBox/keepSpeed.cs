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
    class keepSpeed:MonoBehaviour
    {
        public float speed = 1f;
        public Rigidbody rigidbody;
        public void Start()
        {
            try
            {
                rigidbody = gameObject.GetComponent<Rigidbody>();
            }
            catch { }
        }
        public void Update()
        {
            try
            {
                if (rigidbody == null)
                    return;
                rigidbody.velocity = rigidbody.transform.forward * speed;
            }
            catch { }
        }
    }
}
