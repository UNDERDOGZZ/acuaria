using UnityEngine;

namespace Acuaria.Aquarium.Decorations
{
    public sealed class DecorationView : MonoBehaviour
    {
        [SerializeField] DecorationDefinition definition;
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] string instanceId;
        [SerializeField] DecorationRenderStatus renderStatus;
        static Sprite placeholder;
        public DecorationDefinition Definition => definition;
        public string InstanceId => instanceId;
        public DecorationRenderStatus RenderStatus => renderStatus;
        public SpriteRenderer Renderer => spriteRenderer;
        void OnEnable()
        {
            if(definition==null)return;
            if(spriteRenderer==null)spriteRenderer=GetComponent<SpriteRenderer>();
            if(spriteRenderer==null)spriteRenderer=gameObject.AddComponent<SpriteRenderer>();
            if(spriteRenderer.sprite==null)spriteRenderer.sprite=definition.Sprite!=null?definition.Sprite:Placeholder;
            spriteRenderer.enabled=true;spriteRenderer.color=PlaceholderColor(definition.Category);
            spriteRenderer.sortingLayerName="AquariumFront";
        }
        public void Configure(DecorationDefinition value)
        {
            definition = value;
            if (value == null) return;
            transform.localScale = new Vector3(value.Scale.x, value.Scale.y, 1);
            if (spriteRenderer != null) spriteRenderer.sprite = value.Sprite;
        }

        public void Apply(DecorationPlacementData placement, AquariumDecorationArea2D area)
        {
            instanceId=placement.InstanceId;definition=placement.Definition;
            if(spriteRenderer==null)spriteRenderer=gameObject.GetComponent<SpriteRenderer>();
            if(spriteRenderer==null)spriteRenderer=gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite=definition.Sprite!=null?definition.Sprite:Placeholder;
            spriteRenderer.enabled=placement.IsEnabled&&placement.IsVisible;
            spriteRenderer.color=PlaceholderColor(definition.Category);
            spriteRenderer.flipX=placement.FlipX;
            spriteRenderer.sortingLayerName="AquariumFront";
            spriteRenderer.sortingOrder=BaseOrder(placement.VisualLayer)+placement.SortingOrderOffset;
            transform.localPosition=area.ToLocal(placement.NormalizedPosition);
            transform.localRotation=Quaternion.Euler(0,0,placement.LocalRotation);
            var baseScale=definition.Scale;var local=placement.LocalScale;
            transform.localScale=new(Mathf.Clamp(baseScale.x*local.x,.1f,5),Mathf.Clamp(baseScale.y*local.y,.1f,5),1);
            renderStatus=!placement.IsVisible?DecorationRenderStatus.HiddenByConfiguration:
                definition.Sprite==null&&definition.Prefab==null?DecorationRenderStatus.MissingSprite:DecorationRenderStatus.Visible;
            var collider=GetComponent<BoxCollider2D>();if(collider==null)collider=gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger=true;collider.size=Vector2.one;
        }
        static int BaseOrder(DecorationVisualLayer layer)=>layer switch
        {DecorationVisualLayer.Background=>-3,DecorationVisualLayer.Substrate=>-2,DecorationVisualLayer.Foreground=>1,_=>-1};
        static Color PlaceholderColor(DecorationCategory category)=>category switch
        {DecorationCategory.Plant=>new(.15f,.65f,.36f,1),DecorationCategory.Rock=>new(.45f,.48f,.53f,1),
         DecorationCategory.Wood=>new(.48f,.28f,.12f,1),DecorationCategory.Cave=>new(.26f,.22f,.3f,1),
         DecorationCategory.Substrate=>new(.68f,.54f,.32f,1),_=>new(.2f,.72f,.75f,1)};
        static Sprite Placeholder
        {get{if(placeholder!=null)return placeholder;placeholder=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),new Vector2(.5f,.5f),1);placeholder.name="DecorationPlaceholder";return placeholder;}}
    }
}
