using System;
using System.Collections.Generic;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage2.RunState;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage3.PlayerInterface
{
    public enum Stage3PlayerWindow { None, Inventory, Construction, Research, Hero, Map }

    /// <summary>Scene-local IMGUI presentation. Purchases and interaction stay in the existing rules.</summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(-400)]
    public sealed partial class Stage3FantasyHud : MonoBehaviour
    {
        [SerializeField] private Stage3FantasyHudArt art;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Stage2GameHud legacyHud;
        private Stage2Run observedRun;
        private Material circularMapMaterial;
        private Font runtimeFont;
        private readonly Dictionary<int, GUIStyle> labelStyles = new Dictionary<int, GUIStyle>();
        private readonly Dictionary<int, GUIStyle> paleLabelStyles = new Dictionary<int, GUIStyle>();
        private GUIStyle blankButton;
        private int pressedButtonId;
        private int buildGroup;
        private int inspectedBuildIndex;
        private static readonly Stage3PlayerWindow[] MenuWindows = { Stage3PlayerWindow.Inventory, Stage3PlayerWindow.Construction,
            Stage3PlayerWindow.Research, Stage3PlayerWindow.Hero, Stage3PlayerWindow.Map };
        private static readonly string[] MenuLabels = { "I  Вещи", "B  Стройка", "P  Исследования", "O  Герой", "M  Карта" };
        private static readonly Color PlumInk = new Color(.23f,.12f,.27f);
        private static readonly Color MutedInk = new Color(.43f,.36f,.43f);
        private static readonly Color Accent = new Color(.08f,.65f,.72f);

        public Stage3PlayerWindow ActiveWindow { get; private set; }
        public bool IsModalOpen => ActiveWindow != Stage3PlayerWindow.None && ActiveWindow != Stage3PlayerWindow.Construction;
        public bool IsReady => game != null && game.IsInitialized && art != null && art.IsReady;
        public float UiScale => Mathf.Clamp(Mathf.Min(Screen.width / 1600f, Screen.height / 900f), .55f, 2f);
        public float Scale => 1f; // Rect APIs are screen pixels; only typography/art borders use UiScale.
        public Stage3FantasyHudArt Art => art;
        public Action CancelPlacementRequested { get; set; }
        public Func<string> ClockText { get; set; }
        public event Action WindowChanged;
        public Rect SafeScreenRect => Inset(new Rect(0,0,Screen.width,Screen.height),12f*UiScale);
        public Rect MinimapRect => new Rect(SafeScreenRect.x, SafeScreenRect.yMax-274f*UiScale, 274f*UiScale,274f*UiScale);
        public Rect BottomRect => new Rect(MinimapRect.xMax+8f*UiScale,
            SafeScreenRect.yMax-(ActiveWindow == Stage3PlayerWindow.Construction ? 198f : 156f)*UiScale,
            SafeScreenRect.xMax-MiniMapRight,(ActiveWindow == Stage3PlayerWindow.Construction ? 198f : 156f)*UiScale);
        private float MiniMapRight => MinimapRect.xMax+8f*UiScale;
        public Rect BottomDockRect => BottomRect;
        public Rect ResourcesRect => new Rect(12f*UiScale,12f*UiScale,466f*UiScale,56f*UiScale);
        public Rect StatusRect => new Rect(Screen.width-218f*UiScale,12f*UiScale,206f*UiScale,82f*UiScale);
        public Rect CurrentWindowRect
        {
            get
            {
                if(!IsModalOpen) return Rect.zero;
                var top=StatusRect.yMax+8f*UiScale;
                var height=Mathf.Max(0f,MenuButtonRect(Stage3PlayerWindow.Inventory).y-8f*UiScale-top);
                var width=Mathf.Min(Screen.width*.89f,height*2f);
                return new Rect((Screen.width-width)*.5f,top,width,height);
            }
        }
        public Rect WindowContentRect => !IsModalOpen ? Rect.zero : new Rect(CurrentWindowRect.x+88f*UiScale,CurrentWindowRect.y+96f*UiScale,
            Mathf.Max(0,CurrentWindowRect.width-176f*UiScale),Mathf.Max(0,CurrentWindowRect.height-154f*UiScale));
        public Rect CloseButtonRect => IsModalOpen ? new Rect(CurrentWindowRect.xMax-125f*UiScale,CurrentWindowRect.y+47f*UiScale,38f*UiScale,38f*UiScale) : Rect.zero;

        public Rect MenuButtonRect(Stage3PlayerWindow window)
        {
            var index = Array.IndexOf(MenuWindows,window);
            if(index<0) return Rect.zero;
            var width=Mathf.Min(194f*UiScale,(BottomRect.width-24f*UiScale)/5f);
            return new Rect(BottomRect.x+index*(width+4f*UiScale),BottomRect.y-86f*UiScale,width,78f*UiScale);
        }

        public void ConfigureArt(Stage3FantasyHudArt value) { art=value; labelStyles.Clear(); paleLabelStyles.Clear(); }
        public void Initialize(Stage2GameComposition owner, Stage2SceneBootstrap bootstrap, Stage2GameHud legacy)
        {
            if(observedRun!=null) observedRun.Reset-=ResetPresentation;
            ReleaseMapView();
            game=owner; scene=bootstrap; legacyHud=legacy;
            observedRun=game?.RunDriver?.Run;
            if(observedRun!=null) observedRun.Reset+=ResetPresentation;
            ActiveWindow=Stage3PlayerWindow.None;
            InitializeMapView();
        }

        public void ToggleWindow(Stage3PlayerWindow window) => SetWindow(ActiveWindow==window ? Stage3PlayerWindow.None : window);
        public void CloseWindow() => SetWindow(Stage3PlayerWindow.None);
        public void SetWindow(Stage3PlayerWindow window)
        {
            if(ActiveWindow==window) return;
            if(window!=Stage3PlayerWindow.None && (game==null || game.RunDriver.Run.IsTerminal || legacyHud!=null && legacyHud.DebugOpen)) return;
            ReleaseMapDrag();
            game?.Building?.CancelPlacement(); game?.Production?.CancelPlacement(); CancelPlacementRequested?.Invoke();
            ActiveWindow=window;
            if(window==Stage3PlayerWindow.Map) RecenterMap();
            WindowChanged?.Invoke();
        }

        public bool IsPointerOverUi(Vector2 screenPoint)
        {
            if(!isActiveAndEnabled || game==null) return false;
            if(IsModalOpen) return true;
            var p=new Vector2(screenPoint.x,Screen.height-screenPoint.y);
            if(MinimapRect.Contains(p)||BottomRect.Contains(p)||ResourcesRect.Contains(p)||StatusRect.Contains(p)) return true;
            foreach(var window in MenuWindows) if(MenuButtonRect(window).Contains(p)) return true;
            return false;
        }

        private void Update()
        {
            if(game==null || !game.IsInitialized) return;
            if(game.RunDriver.Run.IsTerminal || legacyHud!=null && legacyHud.DebugOpen) { CloseWindow(); return; }
            UpdateMapTouchInput();
            var keyboard=Keyboard.current; if(keyboard==null) return;
            if(keyboard.escapeKey.wasPressedThisFrame) { CloseWindow(); return; }
            if(keyboard.iKey.wasPressedThisFrame) ToggleWindow(Stage3PlayerWindow.Inventory);
            else if(keyboard.bKey.wasPressedThisFrame) ToggleWindow(Stage3PlayerWindow.Construction);
            else if(keyboard.pKey.wasPressedThisFrame) ToggleWindow(Stage3PlayerWindow.Research);
            else if(keyboard.oKey.wasPressedThisFrame) ToggleWindow(Stage3PlayerWindow.Hero);
            else if(keyboard.mKey.wasPressedThisFrame) ToggleWindow(Stage3PlayerWindow.Map);
        }

        private void ResetPresentation() { CloseWindow(); ReleaseMapView(); InitializeMapView(); inspectedBuildIndex=0; }
        private void OnDisable() { CloseWindow(); ReleaseMapDrag(); }
        private void OnDestroy()
        {
            if(observedRun!=null) observedRun.Reset-=ResetPresentation;
            ReleaseMapView(); if(circularMapMaterial!=null) Destroy(circularMapMaterial);
            if(runtimeFont!=null) Destroy(runtimeFont);
        }

        private void OnGUI()
        {
            if(!IsReady) return;
            var savedMatrix=GUI.matrix; var savedColor=GUI.color; var savedEnabled=GUI.enabled; var savedDepth=GUI.depth;
            GUI.matrix=Matrix4x4.identity; GUI.color=Color.white; GUI.enabled=true; GUI.depth=-20;
            try
            {
                DrawResourcesAndClock(); DrawLocalMinimap(); DrawDevelopmentAccess();
                if(!game.RunDriver.Run.IsTerminal && !(legacyHud!=null && legacyHud.DebugOpen))
                {
                    if(ActiveWindow==Stage3PlayerWindow.Construction) DrawConstructionRibbon(); else DrawHeroDock();
                    for(var i=0;i<MenuWindows.Length;i++) DrawMenuButton(i);
                    DrawLocalHint();
                    if(IsModalOpen) DrawOpenWindow();
                }
            }
            finally { GUI.depth=savedDepth; GUI.enabled=savedEnabled; GUI.color=savedColor; GUI.matrix=savedMatrix; }
        }

        private void DrawResourcesAndClock()
        {
            var inventory=game.Gathering.Economy.Inventory;
            for(var i=0;i<4;i++)
            {
                var r=new Rect(ResourcesRect.x+i*118f*UiScale,ResourcesRect.y,112f*UiScale,ResourcesRect.height);
                DrawPanel(r);
                var safe=PanelContentRect(r);
                var iconSide=Mathf.Min(20f*UiScale,safe.height);
                if(art.resourceIcons!=null && i<art.resourceIcons.Length) DrawIcon(new Rect(safe.x,safe.center.y-iconSide*.5f,iconSide,iconSide),art.resourceIcons[i]);
                FitText(new Rect(safe.x+iconSide+2f*UiScale,safe.y,safe.width-iconSide-2f*UiScale,safe.height),(i==0?inventory.Gold:inventory.Get((MapResourceKind)(i-1))).ToString(),18,false);
            }
            DrawPanel(StatusRect);
            var run=game.RunDriver.Run;
            var status=PanelContentRect(StatusRect);
            Label(new Rect(status.x,status.y-3f*UiScale,status.width,status.height*.5f),$"День {run.CalendarDay}",18,TextAnchor.MiddleCenter);
            Label(new Rect(status.x,status.center.y,status.width,status.height*.5f+3f*UiScale),ClockText?.Invoke() ?? "—:—",18,TextAnchor.MiddleCenter);
        }

        private void DrawDevelopmentAccess()
        {
            if(legacyHud==null || !legacyHud.DebugAvailable || legacyHud.DebugOpen) return;
            var source=legacyHud.DebugButtonRect;
            var r=new Rect(source.position*legacyHud.Scale,source.size*legacyHud.Scale);
            if(Button(r,"F3 · Отладка")) legacyHud.SetDebugOpen(true);
        }

        private void DrawMenuButton(int i)
        {
            var r=MenuButtonRect(MenuWindows[i]);
            if(Button(r,"",ActiveWindow==MenuWindows[i])) ToggleWindow(MenuWindows[i]);
            var safe=ButtonContentRect(r);
            if(art.menuIcons!=null && i<art.menuIcons.Length) DrawIcon(new Rect(safe.center.x-7f*UiScale,safe.y,14f*UiScale,14f*UiScale),art.menuIcons[i]);
            FitPaleLabel(new Rect(safe.x,safe.y+14f*UiScale,safe.width,16f*UiScale),MenuLabels[i],13);
        }

        private void DrawHeroDock()
        {
            var outer=BottomRect; var s=UiScale; DrawPanel(outer);var r=PanelContentRect(outer);
            var portrait=new Rect(r.x,r.center.y-46f*s,92f*s,92f*s);
            DrawRoundSprite(Inset(portrait,8f*s),art.heroPortrait); DrawMedallion(portrait);
            var weapon=new Rect(portrait.xMax+8f*s,r.center.y-30f*s,40f*s,60f*s); DrawIcon(weapon,art.bowIcon);
            var stats=new Rect(weapon.xMax+10f*s,r.y,154f*s,r.height);
            Label(new Rect(stats.x,stats.y,stats.width,25f*s),"Лук",19,TextAnchor.UpperLeft);
            DrawCurrentBowStats(new Rect(stats.x,stats.y+27f*s,stats.width,stats.height-27f*s));
            var barsWidth=Mathf.Min(300f*s,r.width*.28f);
            var bars=new Rect(r.xMax-barsWidth,r.center.y-45f*s,barsWidth,90f*s);
            var abilityLeft=stats.xMax+12f*s;
            var abilityWidth=Mathf.Max(0f,bars.x-abilityLeft-14f*s);
            var count=5;
            var side=Mathf.Min(65f*s,(abilityWidth-8f*s*(count-1))/count);
            var firstSlot=abilityLeft+Mathf.Max(0f,(abilityWidth-count*side-8f*s*(count-1))*.5f);
            for(var i=0;i<count && side>12f*s;i++)
            {
                var slot=new Rect(firstSlot+i*(side+8f*s),r.y+2f*s,side,side);
                DrawDisc(Inset(slot,side*.17f),PlumInk); DrawMedallion(slot); PaleLabel(slot,"—",17,TextAnchor.MiddleCenter);
            }
            Label(new Rect(abilityLeft,r.yMax-23f*s,abilityWidth,19f*s),"Умения · пока недоступны",11,TextAnchor.MiddleCenter,true);
            var names=new[]{"Здоровье  —","Мана  —","Опыт  —"};
            var colors=new[]{new Color(.56f,.18f,.23f,.50f),new Color(.19f,.35f,.64f,.50f),new Color(.80f,.46f,.17f,.50f)};
            for(var i=0;i<3;i++)
            {
                var row=new Rect(bars.x,bars.y+i*30f*s,bars.width,27f*s);
                Label(new Rect(row.x,row.y,row.width*.46f,row.height),names[i],14,TextAnchor.MiddleLeft,true);
                var track=new Rect(row.x+row.width*.46f,row.y+3f*s,row.width*.54f,22f*s);
                var color=GUI.color; GUI.color=colors[i]; DrawPanel(track); GUI.color=color;
            }
        }

        private void DrawOpenWindow()
        {
            var r=CurrentWindowRect; DrawPanel(r,true);
            var title=ActiveWindow==Stage3PlayerWindow.Inventory?"Инвентарь":ActiveWindow==Stage3PlayerWindow.Research?"Исследования":ActiveWindow==Stage3PlayerWindow.Hero?"Развитие героя":"Карта";
            Label(new Rect(r.x+130f*UiScale,r.y+47f*UiScale,r.width-260f*UiScale,36f*UiScale),title,26,TextAnchor.MiddleCenter);
            if(CloseButton(CloseButtonRect)) { CloseWindow(); return; }
            var content=WindowContentRect;
            switch(ActiveWindow)
            {
                case Stage3PlayerWindow.Inventory: DrawInventory(content); break;
                case Stage3PlayerWindow.Research: DrawResearch(content); break;
                case Stage3PlayerWindow.Hero: DrawHero(content); break;
                case Stage3PlayerWindow.Map: DrawMap(content); break;
            }
        }

        private void DrawLocalHint()
        {
            if(IsModalOpen || legacyHud==null) return;
            var hint=!string.IsNullOrEmpty(legacyHud.DwellingInteractionHint)?legacyHud.DwellingInteractionHint:legacyHud.WorldInteractionHint;
            if(string.IsNullOrEmpty(hint)) return;
            var r=new Rect(Mathf.Max(MinimapRect.xMax+15f*UiScale,Screen.width*.38f),BottomRect.y-106f*UiScale,Mathf.Min(520f*UiScale,Screen.width*.48f),35f*UiScale);
            DrawPanel(r); Label(Inset(r,5f*UiScale),hint,14,TextAnchor.MiddleCenter);
        }

        private void DrawLocalMinimap()
        {
            var source=game.GetComponent<Stage2Minimap>(); if(source==null || source.Terrain==null) return;
            var map=scene.MapDefinition;
            var outer=MinimapRect; var inner=Inset(outer,27f*UiScale);
            var heroCell=map.WorldToCellCoordinates(scene.PlayerMover.transform.position)-(Vector2)map.MinimumCell+Vector2.one*.5f;
            const float viewFraction=.35f;
            var uv=new Rect(heroCell.x/map.Size.x-viewFraction*.5f,heroCell.y/map.Size.y-viewFraction*.5f,viewFraction,viewFraction);
            DrawCircularTexture(inner,source.Terrain,uv); if(source.DistrictTerrain!=null) DrawCircularTexture(inner,source.DistrictTerrain,uv);
            foreach(var marker in source.Markers)
            {
                var cell=map.WorldToCellCoordinates(marker.World)-(Vector2)map.MinimumCell+Vector2.one*.5f;
                var unit=new Vector2((cell.x/map.Size.x-uv.x)/uv.width,(cell.y/map.Size.y-uv.y)/uv.height);
                if((unit-Vector2.one*.5f).sqrMagnitude>.235f) continue;
                var p=new Vector2(inner.x+unit.x*inner.width,inner.yMax-unit.y*inner.height);
                DrawWorldMapMarker(p,marker.Kind,UiScale*.65f);
            }
            DrawIcon(outer,art.minimapFrame);
            Label(new Rect(outer.center.x-16f*UiScale,outer.y+6f*UiScale,32f*UiScale,25f*UiScale),"N",15,TextAnchor.MiddleCenter);
        }

        private Material CircularMaterial
        {
            get
            {
                if(circularMapMaterial==null && art!=null && art.circularMapShader!=null)
                    circularMapMaterial=new Material(art.circularMapShader){name="Lunar UI circular map",hideFlags=HideFlags.HideAndDontSave};
                return circularMapMaterial;
            }
        }
        private void DrawCircularTexture(Rect r,Texture2D texture,Rect uv)
        {
            if(texture==null || Event.current.type!=EventType.Repaint || CircularMaterial==null) return;
            CircularMaterial.SetVector("_SourceRect",new Vector4(uv.x,uv.y,uv.width,uv.height));
            Graphics.DrawTexture(r,texture,uv,0,0,0,0,Color.white,CircularMaterial);
            CircularMaterial.SetVector("_SourceRect",new Vector4(0,0,1,1));
        }
        private void DrawRoundSprite(Rect r,Sprite sprite)
        {
            if(sprite==null || Event.current.type!=EventType.Repaint || CircularMaterial==null) return;
            var uv=sprite.rect; uv.x/=sprite.texture.width;uv.width/=sprite.texture.width;uv.y/=sprite.texture.height;uv.height/=sprite.texture.height;
            CircularMaterial.SetVector("_SourceRect",new Vector4(uv.x,uv.y,uv.width,uv.height));
            Graphics.DrawTexture(r,sprite.texture,uv,0,0,0,0,Color.white,CircularMaterial);
            CircularMaterial.SetVector("_SourceRect",new Vector4(0,0,1,1));
        }

        private void DrawDisc(Rect r,Color tint)
        {
            if(Event.current.type!=EventType.Repaint || CircularMaterial==null) return;
            var c=QualitySettings.activeColorSpace==ColorSpace.Linear?tint.linear:tint;
            CircularMaterial.SetVector("_Tint",new Vector4(c.r,c.g,c.b,c.a));
            Graphics.DrawTexture(r,Texture2D.whiteTexture,new Rect(0,0,1,1),0,0,0,0,Color.white,CircularMaterial);
            CircularMaterial.SetVector("_Tint",Vector4.one);
        }

        private bool CloseButton(Rect r)
        {
            var clicked=Button(r,"",false,true);
            DrawIcon(Inset(r,11f*UiScale),art.closeIcon);
            return clicked;
        }

        private static Rect Inset(Rect r,float margin) => new Rect(r.x+margin,r.y+margin,Mathf.Max(0,r.width-2*margin),Mathf.Max(0,r.height-2*margin));
        private void DrawPanel(Rect r) => DrawPanel(r,false);
        private void DrawPanel(Rect r,bool window) => DrawSliced(r,window?art?.windowFrame:art?.panelFrame,true);
        private void DrawFrame(Rect r) => DrawSliced(r,art?.windowFrame,false);
        private void DrawSlot(Rect r,bool locked=false)
        {
            var color=GUI.color; if(locked) GUI.color=new Color(.82f,.81f,.84f,.75f);
            DrawSliced(r,art?.panelFrame,true); GUI.color=color;
        }
        private void DrawMedallion(Rect r,bool selected=false)
        {
            var color=GUI.color; if(selected) GUI.color=color*new Color(.65f,1f,1f,1f);
            DrawIcon(r,art?.medallionFrame); GUI.color=color;
        }
        public Rect PanelContentRect(Rect r) => FrameContentRect(r,art?.panelFrame);
        public Rect ButtonContentRect(Rect r) => r.width<r.height*1.45f ? Inset(r,r.width*.25f) : FrameContentRect(r,art?.buttonNormal);
        private float FrameScale(Rect r,Sprite sprite)
        {
            if(sprite==null) return 0f;
            var maximum=art!=null&&sprite==art.panelFrame&&r.height<=60f*UiScale ? .3f : .5f;
            return Mathf.Min(maximum*UiScale,Mathf.Min(r.height/sprite.rect.height,r.width/(sprite.border.x+sprite.border.z+1f)));
        }
        private Rect FrameContentRect(Rect r,Sprite sprite)
        {
            if(sprite==null) return Inset(r,12f*UiScale);
            var f=FrameScale(r,sprite);var b=sprite.border;var pad=6f*UiScale;var verticalPad=2f*UiScale;
            return Rect.MinMaxRect(r.x+b.x*f+pad,r.y+b.w*f+verticalPad,r.xMax-b.z*f-pad,r.yMax-b.y*f-verticalPad);
        }
        private bool Button(Rect r,string text,bool selected=false,bool enabled=true)
        {
            var oldEnabled=GUI.enabled; var oldColor=GUI.color; var usable=oldEnabled&&enabled;
            var e=Event.current;var id=GUIUtility.GetControlID(FocusType.Passive,r);
            var mouse=Mouse.current;
            var pointer=mouse!=null?new Vector2(mouse.position.ReadValue().x,Screen.height-mouse.position.ReadValue().y):e.mousePosition;
            var hover=usable&&r.Contains(pointer);
            if(e.type==EventType.MouseDown&&e.button==0&&hover) pressedButtonId=id;
            var pressed=hover&&pressedButtonId==id&&(Mouse.current?.leftButton.isPressed??false);
            GUI.enabled=usable;
            GUI.color=oldColor*(!usable?new Color(.66f,.65f,.69f,.85f):pressed?new Color(.64f,.87f,.92f):hover?new Color(.80f,1f,1f):Color.white);
            if(r.width<r.height*1.45f)
            {
                var disc=!usable?new Color(.32f,.29f,.34f):pressed?new Color(.08f,.20f,.25f):
                    hover?new Color(.37f,.28f,.45f):selected?new Color(.13f,.35f,.41f):PlumInk;
                DrawDisc(Inset(r,r.width*.17f),disc); DrawMedallion(r,selected||hover);
            }
            else DrawSliced(r,selected||hover||pressed?art?.buttonSelected:art?.buttonNormal,true);
            if(blankButton==null) blankButton=new GUIStyle(GUIStyle.none);
            var result=GUI.Button(r,GUIContent.none,blankButton);
            GUI.enabled=oldEnabled;GUI.color=oldColor;
            if(e.rawType==EventType.MouseUp&&pressedButtonId==id) pressedButtonId=0;
            if(!string.IsNullOrEmpty(text)) FitPaleLabel(ButtonContentRect(r),text,16);
            return result;
        }
        private void FitPaleLabel(Rect r,string text,int size)
            => FitText(r,text,size,true);
        private void FitText(Rect r,string text,int size,bool pale)
        {
            var style=GetLabelStyle(size,pale);style.alignment=TextAnchor.MiddleCenter;style.wordWrap=false;
            if(!pale) style.normal.textColor=PlumInk;
            var original=style.fontSize;
            while(style.fontSize>Mathf.RoundToInt(11f*UiScale)&&style.CalcSize(new GUIContent(text)).x>r.width) style.fontSize--;
            GUI.Label(r,text,style);style.fontSize=original;style.wordWrap=true;
        }
        private void Label(Rect r,string value,int size=18,TextAnchor align=TextAnchor.UpperLeft,bool muted=false)
        {
            var style=GetLabelStyle(size,false);style.alignment=align;style.normal.textColor=muted?MutedInk:PlumInk;GUI.Label(r,value??"",style);
        }
        private void PaleLabel(Rect r,string value,int size,TextAnchor align)
        {var style=GetLabelStyle(size,true);style.alignment=align;GUI.Label(r,value,style);}
        private GUIStyle GetLabelStyle(int size,bool pale)
        {
            var actual=Mathf.Max(9,Mathf.RoundToInt(size*UiScale)); var styles=pale?paleLabelStyles:labelStyles;
            if(!styles.TryGetValue(actual,out var style))
            {
                if(runtimeFont==null && (art==null || art.font==null)) runtimeFont=Font.CreateDynamicFontFromOSFont(new[]{"Segoe UI","Arial","Liberation Sans"},18);
                style=new GUIStyle(GUI.skin.label){fontSize=actual,wordWrap=true,padding=new RectOffset(0,0,0,0),font=art!=null&&art.font!=null?art.font:runtimeFont};
                style.normal.textColor=pale?new Color(.98f,.93f,.84f):PlumInk; styles[actual]=style;
            }
            return style;
        }
        private void DrawTexture(Rect r,Texture2D texture) {if(texture!=null) GUI.DrawTexture(r,texture,ScaleMode.ScaleToFit,true);}
        private void DrawIcon(Rect r,Sprite sprite)
        {
            if(sprite==null || sprite.texture==null || r.width<=0 || r.height<=0) return;
            var src=sprite.rect;var scale=Mathf.Min(r.width/src.width,r.height/src.height); var size=src.size*scale;
            var target=new Rect(r.center-size*.5f,size);
            var uv=new Rect(src.x/sprite.texture.width,src.y/sprite.texture.height,src.width/sprite.texture.width,src.height/sprite.texture.height);
            if(art!=null && sprite==art.teleportIcon && CircularMaterial!=null)
            {
                if(Event.current.type!=EventType.Repaint) return;
                CircularMaterial.SetFloat("_CircleMask",0);CircularMaterial.SetFloat("_RemoveDark",1);
                Graphics.DrawTexture(target,sprite.texture,uv,0,0,0,0,Color.white,CircularMaterial);
                CircularMaterial.SetFloat("_CircleMask",1);CircularMaterial.SetFloat("_RemoveDark",0);
            }
            else GUI.DrawTextureWithTexCoords(target,sprite.texture,uv);
        }
        private void DrawSliced(Rect r,Sprite sprite,bool center)
        {
            if(sprite==null || r.width<=0 || r.height<=0) return;
            var src=sprite.rect; var b=sprite.border;
            // The central jewels are immutable ornaments, not stretchable vertical edges.
            // Five vertical bands keep corners and the middle ornament at one isotropic scale.
            var factor=FrameScale(r,sprite);
            var middle=src.height*.32f;var midTop=(src.height-middle)*.5f;
            var dx=new[]{r.x,r.x+b.x*factor,r.xMax-b.z*factor,r.xMax};
            var dy=new[]{r.y,r.y+b.w*factor,r.center.y-middle*factor*.5f,r.center.y+middle*factor*.5f,r.yMax-b.y*factor,r.yMax};
            // Every neighbouring quad must share the same physical pixel edge.
            for(var i=0;i<dx.Length;i++) dx[i]=Mathf.Round(dx[i]);
            for(var i=0;i<dy.Length;i++) dy[i]=Mathf.Round(dy[i]);
            var sx=new[]{src.x,src.x+b.x,src.xMax-b.z,src.xMax};
            var sy=new[]{src.yMax,src.yMax-b.w,src.yMax-midTop,src.yMax-midTop-middle,src.y+b.y,src.y};
            for(var y=0;y<5;y++) for(var x=0;x<3;x++)
            {
                if(!center && x==1&&y>0&&y<4) continue;
                var target=new Rect(dx[x],dy[y],dx[x+1]-dx[x],dy[y+1]-dy[y]);
                if(target.width<=0 || target.height<=0) continue;
                var uv=new Rect(sx[x]/sprite.texture.width,sy[y+1]/sprite.texture.height,(sx[x+1]-sx[x])/sprite.texture.width,(sy[y]-sy[y+1])/sprite.texture.height);
                GUI.DrawTextureWithTexCoords(target,sprite.texture,uv);
            }
        }
    }
}
