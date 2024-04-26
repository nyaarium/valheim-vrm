using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using HarmonyLib;
using UniGLTF;
using UnityEngine;
using VRM;
using VRMShaders;
using Object = UnityEngine.Object;

namespace ValheimVRM
{
    public class VRM 
	{
		public enum SourceType
		{
			Local,  // my VRM from my computer
			Shared // VRM, downloaded from other player
		}
		
		public GameObject VisualModel { get; private set; }
		public byte[] Src;
		public byte[] SrcHash;
		public byte[] SettingsHash;
		public string Name { get; private set; }
		public SourceType Source = SourceType.Local;

		public VRM(GameObject visualModel, string name)
		{
			VisualModel = visualModel;
			Name = name;
		}

		~VRM()
		{
			if (VisualModel != null)
			{
				Object.Destroy(VisualModel);
			}
		}

		public void RecalculateSrcBytesHash()
		{
			using (var md5 = System.Security.Cryptography.MD5.Create())
			{
				SrcHash = md5.ComputeHash(Src);
			}
		}

		public void RecalculateSettingsHash()
		{
			using (var md5 = System.Security.Cryptography.MD5.Create())
			{
				byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(Settings.GetSettings(Name).ToStringDiffOnly());
				SettingsHash = md5.ComputeHash(inputBytes);
			}
		}
		
 

		public static GameObject ImportVisual(byte[] buf, string path, float scale)
		{
			Debug.Log("[ValheimVRM] loading vrm: " + buf.Length + " bytes");
			
			try
			{
				var data = new GlbBinaryParser(buf, path).Parse();
				var vrm = new VRMData(data);
				var context = new VRMImporterContext(vrm);
				var loaded = default(RuntimeGltfInstance);
				
				try
				{
					loaded = context.Load();
				}
				catch (TypeLoadException ex)
				{
					Debug.LogError("Failed to load type: " + ex.TypeName);
					Debug.LogError(ex);
				}
				
				loaded.ShowMeshes();

				loaded.Root.transform.localScale = Vector3.one * scale;

				Debug.Log("[ValheimVRM] VRM read successful");

				return loaded.Root;
			}
			catch (Exception ex)
			{
				Debug.LogError(ex);
			}

			return null;
		}
		
 
		public static IEnumerator ImportVisualAsync(byte[] buf, string path, float scale, Action<GameObject> onCompleted)
		{
			Debug.Log("______________________ Vrm Chain 2.1");
			Debug.Log("[ValheimVRM Async] loading vrm: " + buf.Length + " bytes");
			
			var data = new GlbBinaryParser(buf, path).Parse();
			yield return null;
			Debug.Log("______________________ Vrm Chain 2.2");
			var vrm = new VRMData(data);
			yield return null;
			Debug.Log("______________________ Vrm Chain 2.3");
			var context = new VRMImporterContext(vrm);
			Debug.Log("______________________ Vrm Chain 2.4");
			
			
			
			var loader = context.LoadAsync(new RuntimeOnlyAwaitCaller(1f));
					
					
			while (!loader.IsCompleted)
			{
				yield return new WaitForSeconds(1f);
			}
			
			
		
			Debug.Log("______________________ Vrm Chain 2.5");
		
			
			try
			{
				var loaded = loader.Result;
			
				loaded.ShowMeshes();
				loaded.Root.transform.localScale = Vector3.one * scale;
				Debug.Log("[ValheimVRM] VRM read successful");
				
				onCompleted(loaded.Root);
			}
			catch (Exception ex)
			{
				Debug.LogError("Error during VRM loading: " + ex);
			}
			
			
 	
		}
		
		public static async Task<GameObject> ImportVisualAsync(byte[] buf, string path, float scale)
		{
			Debug.Log("______________________ Vrm Chain 2.1");
			Debug.Log("[ValheimVRM Async] loading vrm: " + buf.Length + " bytes");
			
			GltfData data = new GlbBinaryParser(buf, path).Parse();

			Debug.Log("______________________ Vrm Chain 2.2");
			var vrm = new VRMData(data);
			Debug.Log("______________________ Vrm Chain 2.3");
			//var context = new VRMImporterContext(vrm);
			RuntimeGltfInstance loaded;
			
			using(VRMImporterContext loader = new VRMImporterContext(vrm))
			{
				loaded = await loader.LoadAsync(new RuntimeOnlyAwaitCaller(1f));
			}
			
			Debug.Log("______________________ Vrm Chain 2.4");
			
				try
				{
				
					loaded.ShowMeshes();
					loaded.Root.transform.localScale = Vector3.one * scale;
					Debug.Log("[ValheimVRM] VRM read successful");
					
					return loaded.Root;
				}
				catch (Exception ex)
				{
					Debug.LogError("Error during VRM loading: " + ex);
				}

				return null;
		}
		
 
		public IEnumerator SetToPlayer(Player player)
		{
			UnityEngine.Debug.Log("_______________________________________START Set To Player");
			var settings = Settings.GetSettings(Name);
			player.m_maxInteractDistance *= settings.InteractionDistanceScale;
 
			var vrmModel = Object.Instantiate(VisualModel);
			VrmManager.PlayerToVrmInstance[player] = vrmModel;
			vrmModel.name = "VRM_Visual";
			vrmModel.SetActive(true);
			player.GetComponent<VrmController>().visual = vrmModel;

			var oldModel = player.GetComponentInChildren<Animator>().transform.parent.Find("VRM_Visual");
			if (oldModel != null)
			{
				Object.Destroy(oldModel);
			}
			
			vrmModel.transform.SetParent(player.GetComponentInChildren<Animator>().transform.parent, false);

			float newHeight = settings.PlayerHeight;
			float newRadius = settings.PlayerRadius;

			var collider = player.gameObject.GetComponent<CapsuleCollider>();
			collider.height = newHeight;
			collider.radius = newRadius;
			collider.center = new Vector3(0, newHeight / 2, 0);

			player.GetComponent<Rigidbody>().centerOfMass = collider.center;
			
			yield return null;

			foreach (var smr in player.GetVisual().GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				smr.forceRenderingOff = true;
				smr.updateWhenOffscreen = true;
			}
			yield return null;

			var orgAnim = AccessTools.FieldRefAccess<Player, Animator>(player, "m_animator");
			orgAnim.keepAnimatorStateOnDisable = true;
			orgAnim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			yield return null;

			vrmModel.transform.localPosition = orgAnim.transform.localPosition;

			// アニメーション同期
			if (vrmModel.GetComponent<VRMAnimationSync>() == null) vrmModel.AddComponent<VRMAnimationSync>().Setup(orgAnim, settings, false);
			else vrmModel.GetComponent<VRMAnimationSync>().Setup(orgAnim, settings, false);
			yield return null;

			// カメラ位置調整
			if (settings.FixCameraHeight)
			{
				var vrmEye = vrmModel.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.LeftEye);
				if (vrmEye == null) vrmEye = vrmModel.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
				if (vrmEye == null) vrmEye = vrmModel.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Neck);
				if (vrmEye != null)
				{
					if (player.gameObject.GetComponent<VRMEyePositionSync>() == null) player.gameObject.AddComponent<VRMEyePositionSync>().Setup(vrmEye);
					else player.gameObject.GetComponent<VRMEyePositionSync>().Setup(vrmEye);
				}
			}
			yield return null;

			// MToonの場合環境光の影響をカラーに反映する
			if (settings.UseMToonShader)
			{
				if (vrmModel.GetComponent<MToonColorSync>() == null) vrmModel.AddComponent<MToonColorSync>().Setup(vrmModel);
				else vrmModel.GetComponent<MToonColorSync>().Setup(vrmModel);
			}
			yield return null;

			// SpringBone設定
			foreach (var springBone in vrmModel.GetComponentsInChildren<VRMSpringBone>())
			{
				springBone.m_stiffnessForce *= settings.SpringBoneStiffness;
				springBone.m_gravityPower *= settings.SpringBoneGravityPower;
				springBone.m_updateType = VRMSpringBone.SpringBoneUpdateType.FixedUpdate;
				springBone.m_center = null;
				yield return null;

			}
			
			player.GetComponent<VrmController>().ReloadSpringBones();
        }
    }
}