using System.Collections.Generic;
using Not3A.Stage3.VisualPilot;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Only grounded artwork; ownership, death and recovery belong to the rules.</summary>
    public sealed class Stage3HeroRecoveryPresentation : MonoBehaviour
    {
        private readonly Dictionary<int,GameObject> visuals=new Dictionary<int,GameObject>();
        private readonly List<int> removed=new List<int>();
        private Texture2D mistTexture;
        private Sprite mistSprite;
        private Stage3RecoveryArt art;
        private Camera worldCamera;

        public void Configure(Stage3RecoveryArt value,Camera camera)
        {
            art=value;worldCamera=camera;
            if(mistTexture!=null)return;
            // One bounded soft contact texture shared by every drop, released with this component.
            const int size=64;
            var pixels=new Color32[size*size];
            for(var y=0;y<size;y++)for(var x=0;x<size;x++)
            {
                var d=new Vector2((x+.5f)/size*2f-1f,(y+.5f)/size*2f-1f).magnitude;
                var alpha=Mathf.Pow(Mathf.Clamp01(1f-d),2f);
                pixels[y*size+x]=new Color32(255,255,255,(byte)Mathf.RoundToInt(alpha*255));
            }
            mistTexture=new Texture2D(size,size,TextureFormat.RGBA32,false){name="Recovery local contact",wrapMode=TextureWrapMode.Clamp};
            mistTexture.SetPixels32(pixels);mistTexture.Apply(false,true);
            mistSprite=Sprite.Create(mistTexture,new Rect(0,0,size,size),new Vector2(.5f,.5f),64f);
        }

        public void SetDrops(Stage3RecoveryDropView[] drops)
        {
            if(art==null||!art.IsReady||worldCamera==null)return;
            removed.Clear();
            foreach(var entry in visuals)
            {
                var retained=false;
                if(drops!=null)foreach(var drop in drops)
                    if(drop.Id==entry.Key&&drop.ItemCount>0){retained=true;break;}
                if(!retained)removed.Add(entry.Key);
            }
            foreach(var id in removed){visuals[id].SetActive(false);Destroy(visuals[id]);visuals.Remove(id);}
            if(drops==null)return;
            foreach(var drop in drops)
            {
                if(drop.ItemCount<=0||visuals.ContainsKey(drop.Id))continue;
                var root=new GameObject("Dropped belongings "+drop.Id);
                root.transform.SetParent(transform,false);visuals.Add(drop.Id,root);
                var p=GroundPosition(drop.Position);
                var shadow=MakeSprite(root,"Contact shadow",mistSprite,new Color(.18f,.10f,.17f,.85f));
                shadow.transform.position=p+Vector3.back*.024f;
                shadow.transform.localScale=new Vector3(1.05f,.65f,1f);
                var mist=MakeSprite(root,"Lavender contact light",mistSprite,new Color(.72f,.53f,.86f,.72f));
                mist.transform.position=p+Vector3.back*.034f;
                mist.transform.localScale=new Vector3(1.45f,1.05f,1f);
                var bag=MakeSprite(root,"Leather flap satchel",art.WorldBag,Color.white);
                var scale=.64f/art.WorldBag.bounds.size.x;
                bag.transform.localScale=Vector3.one*scale;
                bag.transform.SetPositionAndRotation(p-worldCamera.transform.forward*.055f,worldCamera.transform.rotation);
                bag.sortingOrder=2;
            }
        }

        private SpriteRenderer MakeSprite(GameObject parent,string label,Sprite sprite,Color tint)
        {
            var go=new GameObject(label);go.transform.SetParent(parent.transform,false);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprite;
            renderer.sharedMaterial=art.WorldMaterial;renderer.color=tint;return renderer;
        }

        public static Vector3 GroundPosition(Vector2 p) => new Vector3(p.x,p.y,Stage3TacticalPresentation.GrassSurfaceZ(p));
        private void OnDestroy()
        {
            if(mistSprite!=null)Destroy(mistSprite);
            if(mistTexture!=null)Destroy(mistTexture);
        }
    }
}
