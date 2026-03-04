using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimVRM
{
    public class VRMEyePositionSync : MonoBehaviour
    {
        private Transform vrmEye;
        private Transform orgEye;

        public void Setup(Transform vrmEye)
        {
            this.vrmEye = vrmEye;
            Player player = GetComponent<Player>();
            if (player != null)
            {
                this.orgEye = player.m_eye;
            }
            else
            {
                Debug.LogError("Player component or m_eye is null. Ensure the component exists.");
            }
        }

        void LateUpdate()
        {
            if (orgEye != null)
            {
                var pos = this.orgEye.position;
                pos.y = this.vrmEye.position.y;
                this.orgEye.position = pos;
            }
            else
            {
                Debug.LogError("orgEye is null. Make sure Setup method is called and Player component is available.");
            }
        }
    }
}

