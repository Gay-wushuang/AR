using System.Collections.Generic;
using UnityEngine;
namespace EEGAI.Demo
{
    public class RadarTargetGlow : MonoBehaviour
    {
        [SerializeField] private ConsoleRadarSystem radar;
        [SerializeField] private Material glowMaterial;
        private Transform current;
        private readonly List<GameObject> shells=new List<GameObject>();
        private void LateUpdate()
        {
            Transform target=radar.TrackedTarget;
            if(EnergyLinkPuzzle.Active)target=null;
            if(target==current)return;
            Clear();current=target;if(target==null)return;
            foreach(var mesh in target.GetComponentsInChildren<MeshFilter>())
            {
                if(mesh.sharedMesh==null)continue;
                for(int i=0;i<2;i++)
                {
                    var go=new GameObject("Radar glow shell");go.transform.SetParent(mesh.transform,false);shells.Add(go);
                    go.AddComponent<MeshFilter>().sharedMesh=mesh.sharedMesh;
                    var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=glowMaterial;
                    renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
                    var block=new MaterialPropertyBlock();block.SetFloat("_Width",i==0?.035f:.08f);block.SetColor("_Color",new Color(.1f,2.5f,3f,i==0?.85f:.2f));renderer.SetPropertyBlock(block);
                }
            }
        }
        private void Clear(){foreach(var shell in shells){if(shell!=null){shell.SetActive(false);Destroy(shell);}}shells.Clear();current=null;}
        private void OnDisable(){Clear();}
    }
}
