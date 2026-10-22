using System;
using System.Collections.Generic;
using Not3A.Stage2.Integration;
using Not3A.Stage2.Map;
using Not3A.Stage2.RunState;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage3.PlayerInterface
{
    public enum Stage3PlayerWindow { None, Inventory, Construction, Research, Hero, Map, Forge, Teleports, Merchant, Guild }

    /// <summary>Scene-local IMGUI presentation. Purchases and interaction stay in the existing rules.</summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(-400)]
    public sealed partial class Stage3FantasyHud : MonoBehaviour
    {
        private static readonly Unity.Profiling.ProfilerMarker HudFrameMarker=new Unity.Profiling.ProfilerMarker("Stage3.HUD");
        [SerializeField] private Stage3FantasyHudArt art;
        private Stage2GameComposition game;
        private Stage2SceneBootstrap scene;
        private Stage2GameHud legacyHud;
        private Stage2Run observedRun;
        private Material circularMapMaterial;
        private Font runtimeFont;
        private bool ownsRuntimeFont;
        public string RuntimeFontName { get; private set; }
        private readonly Dictionary<int, GUIStyle> labelStyles = new Dictionary<int, GUIStyle>();
        private readonly Dictionary<int, GUIStyle> paleLabelStyles = new Dictionary<int, GUIStyle>();
        private GUIStyle blankButton;
        private readonly Dictionary<string,GUIContent> labelContents=new Dictionary<string,GUIContent>();
        private readonly float[] sliceDx=new float[4],sliceDy=new float[6],sliceSx=new float[4],sliceSy=new float[6];
        private readonly int[] displayedResources={int.MinValue,int.MinValue,int.MinValue,int.MinValue};
        private readonly string[] resourceNumbers=new string[4],resourceHeaders=new string[4];
        private static readonly string[] ResourceNames={"Золото","Дерево","Камень","Металл"};
        private int displayedDay=int.MinValue;
        private string dayLabel;
        private int pressedButtonId;
        private int buildGroup;
        private int inspectedBuildIndex;
        private static readonly Stage3PlayerWindow[] MenuWindows = { Stage3PlayerWindow.Inventory, Stage3PlayerWindow.Construction,
            Stage3PlayerWindow.Research, Stage3PlayerWindow.Hero, Stage3PlayerWindow.Map };
        private static readonly string[] MenuKeys = { "I", "B", "P", "O", "M" };
        private static readonly Stage3PlayerWindow[] FullscreenWindows = { Stage3PlayerWindow.Inventory,
            Stage3PlayerWindow.Research, Stage3PlayerWindow.Hero, Stage3PlayerWindow.Map };
        private static readonly string[] FullscreenLabels = { "Инвентарь  I", "Исследования  P", "Развитие героя  O", "Карта  M" };
        private static readonly Color Pearl = new Color(.94f,.91f,.86f);
        private static readonly Color Silver = new Color(.69f,.62f,.58f);
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
        public Rect BottomRect => new Rect(SafeScreenRect.x, SafeScreenRect.yMax-196f*UiScale,
            SafeScreenRect.width,196f*UiScale);
        public Rect BottomDockRect => BottomRect;
        public Rect NavigationRibbonRect => new Rect(BottomRect.x,BottomRect.y-54f*UiScale,BottomRect.width,54f*UiScale);
        public Rect MinimapRect => new Rect(BottomRect.x,BottomRect.y,220f*UiScale,196f*UiScale);
        public Rect ConstructionContentRect => Rect.MinMaxRect(MinimapRect.xMax+14f*UiScale,BottomRect.y+12f*UiScale,
            BottomRect.xMax-18f*UiScale,BottomRect.yMax-12f*UiScale);
        public Rect WorldHudRect => Rect.MinMaxRect(BottomRect.x,HasSelectedBuilding?SelectedBuildingRect.y:NavigationRibbonRect.y,
            BottomRect.xMax,BottomRect.yMax);
        public Rect ResourcesRect => new Rect(0,0,472f*UiScale,46f*UiScale);
        public Rect StatusRect => new Rect(Screen.width-304f*UiScale,0,304f*UiScale,46f*UiScale);
        public Rect CurrentWindowRect => IsModalOpen ? new Rect(0,0,Screen.width,Screen.height) : Rect.zero;
        public Rect FullscreenHeaderRect => new Rect(0,0,Screen.width,72f*UiScale);
        public Rect FullscreenWalletRect => new Rect(0,72f*UiScale,Screen.width,60f*UiScale);
        public Rect FullscreenFooterRect => new Rect(0,Screen.height-68f*UiScale,Screen.width,68f*UiScale);
        public Rect WindowContentRect => !IsModalOpen ? Rect.zero : new Rect(64f*UiScale,164f*UiScale,
            Screen.width-128f*UiScale,Screen.height-254f*UiScale);
        public Rect CloseButtonRect => GuildOpen ? GuildCloseRect : TeleportsOpen ? TeleportCloseRect : IsModalOpen ? new Rect(Screen.width-190f*UiScale,10f*UiScale,174f*UiScale,52f*UiScale) : Rect.zero;

        public Rect FullscreenTabRect(Stage3PlayerWindow window)
        {
            var index=Array.IndexOf(FullscreenWindows,window);
            if(index<0) return Rect.zero;
            var width=(Screen.width-214f*UiScale)/4f;
            return new Rect(16f*UiScale+index*width,0,width,72f*UiScale);
        }

        public Rect HudMenuButtonRect(Stage3PlayerWindow window)
        {
            var index=Array.IndexOf(MenuWindows,window);
            return index<0 ? Rect.zero : new Rect(NavigationRibbonRect.center.x-172f*UiScale+index*72f*UiScale,
                NavigationRibbonRect.y+9f*UiScale,56f*UiScale,40f*UiScale);
        }

        // Public hit-test/capture API always describes the navigation that is currently visible.
        public Rect MenuButtonRect(Stage3PlayerWindow window) => TeleportsOpen || GuildOpen ? Rect.zero : IsModalOpen ? FullscreenTabRect(window) : HudMenuButtonRect(window);

        public void ConfigureArt(Stage3FantasyHudArt value) { art=value; labelStyles.Clear(); paleLabelStyles.Clear(); }
        public void Initialize(Stage2GameComposition owner, Stage2SceneBootstrap bootstrap, Stage2GameHud legacy)
        {
            if(observedRun!=null) { observedRun.Reset-=ResetPresentation; observedRun.Rewound-=ResetPresentation; }
            ReleaseMapView();
            game=owner; scene=bootstrap; legacyHud=legacy;
            observedRun=game?.RunDriver?.Run;
            if(observedRun!=null) { observedRun.Reset+=ResetPresentation; observedRun.Rewound+=ResetPresentation; }
            ActiveWindow=Stage3PlayerWindow.None;
            ResetProfessionPresentation();
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
            var wasTeleports=TeleportsOpen;
            var wasMerchant=MerchantOpen;
            var wasGuild=GuildOpen;
            ActiveWindow=window;
            if(wasTeleports) TeleportClosed?.Invoke();
            if(wasMerchant) MerchantClosed?.Invoke();
            if(wasGuild) GuildClosed?.Invoke();
            if(window==Stage3PlayerWindow.Map)
            {
                if(!mapInitialized) InitializeMapView();
                ClampMapCenter();
            }
            WindowChanged?.Invoke();
        }

        public bool IsPointerOverUi(Vector2 screenPoint)
        {
            if(!isActiveAndEnabled || game==null) return false;
            if(defeatPresentation!=null && defeatPresentation.OwnsLossPresentation) return true;
            if(IsModalOpen) return true;
            var p=new Vector2(screenPoint.x,Screen.height-screenPoint.y);
            if(CycleCorniceRect.Contains(p)||WorldHudRect.Contains(p)||ResourcesRect.Contains(p)||StatusRect.Contains(p)||CycleDialRect.Contains(p)||CyclePhaseRect.Contains(p)||DevelopmentAccessRect.Contains(p)) return true;
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
            if(TeleportsOpen) return;
            if(keyboard.iKey.wasPressedThisFrame) ToggleWindow(Stage3PlayerWindow.Inventory);
            else if(keyboard.bKey.wasPressedThisFrame) ToggleWindow(Stage3PlayerWindow.Construction);
            else if(keyboard.pKey.wasPressedThisFrame) ToggleWindow(Stage3PlayerWindow.Research);
            else if(keyboard.oKey.wasPressedThisFrame) ToggleWindow(Stage3PlayerWindow.Hero);
            else if(keyboard.mKey.wasPressedThisFrame) ToggleWindow(Stage3PlayerWindow.Map);
        }

        private void ResetPresentation() { CloseWindow(); ReleaseMapView(); InitializeMapView(); inspectedBuildIndex=0; ResetProfessionPresentation(); }
        private void OnDisable() { CloseWindow(); ReleaseMapDrag(); if(recoveryPresentation!=null)recoveryPresentation.gameObject.SetActive(false); }
        private void OnDestroy()
        {
            if(recoveryPresentation!=null)Destroy(recoveryPresentation.gameObject);
            ReleaseCycleVisuals();
            ReleaseGuildVisuals();
            ReleaseProgressionVisuals();
            ReleaseBevelTextures();
            ReleaseEquipmentQualityTextures();
            if(observedRun!=null) { observedRun.Reset-=ResetPresentation; observedRun.Rewound-=ResetPresentation; }
            ReleaseMapView(); if(circularMapMaterial!=null) Destroy(circularMapMaterial);
            if(runtimeFont!=null && ownsRuntimeFont) Destroy(runtimeFont);
        }

        private void OnGUI()
        {
            if(!IsReady) return;
            ResetRecoveryPointerMetadata();
            ResetCityMapMetadata();
            ResetGuildMapMetadata();
            using var sample=HudFrameMarker.Auto();
            var savedMatrix=GUI.matrix; var savedColor=GUI.color; var savedEnabled=GUI.enabled; var savedDepth=GUI.depth;
            var savedBackground=GUI.backgroundColor;var savedContent=GUI.contentColor;
            GUI.matrix=Matrix4x4.identity; GUI.color=Color.white; GUI.backgroundColor=Color.white; GUI.contentColor=Color.white;
            GUI.enabled=true; GUI.depth=-20;
            try
            {
                if(defeatPresentation!=null && defeatPresentation.OwnsLossPresentation)
                {
                    GUI.depth=-1000;
                    defeatPresentation.DrawOverlay();
                    DrawDefeatRetry();
                    return;
                }
                if(IsModalOpen) DrawOpenWindow();
                else
                {
                    DrawResourcesAndClock(); DrawDevelopmentAccess();
                    if(!game.RunDriver.Run.IsTerminal && !(legacyHud!=null && legacyHud.DebugOpen))
                    {
                        DrawSelectedBuildingWorldOutline();
                        DrawTowerLevelBadges();
                        DrawRecoveryWorld();
                        DrawUnifiedDockBackground();
                        DrawLocalMinimap();
                        if(ActiveWindow==Stage3PlayerWindow.Construction) DrawConstructionRibbon(); else DrawHeroDock();
                        for(var i=0;i<MenuWindows.Length;i++) DrawMenuButton(i);
                        DrawHeroPointBadge();
                        DrawSelectedBuilding();
                        DrawLocalHint();
                    }
                }
                DrawHeroLevelUpOverlay();
                defeatPresentation?.DrawOverlay();
            }
            finally { GUI.depth=savedDepth; GUI.enabled=savedEnabled; GUI.color=savedColor; GUI.backgroundColor=savedBackground; GUI.contentColor=savedContent; GUI.matrix=savedMatrix; }
        }

        private void DrawResourcesAndClock() => DrawCycleTopHud();

        private string ResourceNumber(int index,int value)
        {
            if(displayedResources[index]!=value)
            {
                displayedResources[index]=value;resourceNumbers[index]=value.ToString();
                resourceHeaders[index]=ResourceNames[index]+"  "+resourceNumbers[index];
            }
            return resourceNumbers[index];
        }

        private void DrawDevelopmentAccess()
        {
            if(legacyHud==null || !legacyHud.DebugAvailable || legacyHud.DebugOpen) return;
            var r=DevelopmentAccessRect;
            if(Button(r,heroLoadout!=null?"F10 · Отладка":"F3 · Отладка")) legacyHud.SetDebugOpen(true);
        }

        private void DrawMenuButton(int i)
        {
            var r=HudMenuButtonRect(MenuWindows[i]); var s=UiScale;
            var clicked=SurfaceButton(r,ActiveWindow==MenuWindows[i],true);
            if(MenuWindows[i]==Stage3PlayerWindow.Hero)
                DrawDevelopmentTreeIcon(new Rect(r.x+10f*s,r.y+4f*s,28f*s,28f*s));
            else if(art.menuIcons!=null && i<art.menuIcons.Length)
                DrawIcon(new Rect(r.x+10f*s,r.y+4f*s,28f*s,28f*s),art.menuIcons[i]);
            var key=new Rect(r.xMax-21f*s,r.yMax-21f*s,21f*s,21f*s);
            DrawDisc(key,Silver);DrawDisc(Inset(key,1.5f*s),PlumInk); PaleLabel(key,MenuKeys[i],14,TextAnchor.MiddleCenter);
            if(clicked) ToggleWindow(MenuWindows[i]);
        }

        private void DrawOpenWindow()
        {
            if(TeleportsOpen) { DrawTeleports(); return; }
            if(GuildOpen) { DrawGuild(); return; }
            DrawFullscreenShell();
            // Defer navigation until after drawing so one event never mixes two page geometries.
            var requested=ActiveWindow;
            for(var i=0;i<FullscreenWindows.Length;i++)
                if(DrawFullscreenTab(FullscreenTabRect(FullscreenWindows[i]),FullscreenLabels[i],ActiveWindow==FullscreenWindows[i]))
                    requested=FullscreenWindows[i];
            var close=DrawFullscreenTab(CloseButtonRect,"Esc · Закрыть",false);
            var content=WindowContentRect;
            switch(ActiveWindow)
            {
                case Stage3PlayerWindow.Inventory: DrawInventory(content); break;
                case Stage3PlayerWindow.Research: DrawResearch(content); break;
                case Stage3PlayerWindow.Hero: DrawHero(content); break;
                case Stage3PlayerWindow.Map: DrawMap(content); break;
                case Stage3PlayerWindow.Forge: DrawForge(content); break;
                case Stage3PlayerWindow.Merchant: DrawMerchant(); break;
            }
            if(MerchantOpen)
                Label(FullscreenFooterRect,"Esc · Вернуться в город",16,TextAnchor.MiddleCenter);
            else if(ActiveWindow==Stage3PlayerWindow.Forge)
                Label(FullscreenFooterRect,"Esc · Вернуться в игру",16,TextAnchor.MiddleCenter);
            else if(ActiveWindow!=Stage3PlayerWindow.Map)
                Label(FullscreenFooterRect,MenuKeys[Array.IndexOf(MenuWindows,ActiveWindow)]+" · Закрыть       Esc · Вернуться в игру",16,TextAnchor.MiddleCenter);
            if(close) CloseWindow(); else if(requested!=ActiveWindow) SetWindow(requested);
        }

        private void DrawFullscreenShell()
        {
            Fill(CurrentWindowRect,Pearl);
            // Sample only the calm ceramic interior; fixed-aspect ornaments are drawn separately.
            if(art.windowFrame!=null)
            {
                var sprite=art.windowFrame;var src=sprite.rect;var texture=sprite.texture;
                var uv=new Rect((src.x+src.width*.34f)/texture.width,(src.y+src.height*.36f)/texture.height,
                    src.width*.32f/texture.width,src.height*.28f/texture.height);
                DrawUiTexture(CurrentWindowRect,texture,uv);
            }
            Fill(FullscreenHeaderRect,PlumInk);
            var s=UiScale;var wallet=game.Gathering.Economy.Inventory;
            for(var i=0;i<(MerchantOpen?1:4);i++)
            {
                var r=new Rect((48f+i*205f)*s,82f*s,188f*s,40f*s);
                if(art.resourceIcons!=null&&i<art.resourceIcons.Length) DrawIcon(new Rect(r.x,r.y,38f*s,38f*s),art.resourceIcons[i]);
                ResourceNumber(i,i==0?(MerchantOpen&&merchant!=null?merchant.Gold:wallet.Gold):wallet.Get((MapResourceKind)(i-1)));
                Label(new Rect(r.x+48f*s,r.y,r.width-48f*s,r.height),resourceHeaders[i],17,TextAnchor.MiddleLeft);
            }
            Label(new Rect(Screen.width-280f*s,82f*s,232f*s,40f*s),FullscreenCycleCaption(),17,TextAnchor.MiddleRight);
            DrawRule(new Rect(0,71f*s,Screen.width,1f*s),false);
            DrawRule(new Rect(32f*s,131f*s,Screen.width-64f*s,1f*s),false);
            DrawRule(new Rect(32f*s,FullscreenFooterRect.y,Screen.width-64f*s,1f*s));
        }

        private bool DrawFullscreenTab(Rect r,string text,bool selected)
        {
            var clicked=SurfaceButton(r,selected,false);
            PaleLabel(Inset(r,10f*UiScale),text,18,TextAnchor.MiddleCenter);
            return clicked;
        }

        private bool SurfaceButton(Rect r,bool selected,bool pearl)
        {
            var e=Event.current;var id=GUIUtility.GetControlID(FocusType.Passive,r);
            var mouse=Mouse.current;
            var point=mouse!=null?new Vector2(mouse.position.ReadValue().x,Screen.height-mouse.position.ReadValue().y):e.mousePosition;
            var hover=GUI.enabled&&r.Contains(point);
            if(e.type==EventType.MouseDown&&e.button==0&&hover) pressedButtonId=id;
            var pressed=hover&&pressedButtonId==id&&(mouse?.leftButton.isPressed??false);
            if(pearl)
            {
                DrawSliced(r,art.panelFrame,true,.10f*UiScale);
                if(selected||hover) Fill(Inset(r,6f*UiScale),new Color(.08f,.65f,.72f,.50f));
            }
            else if(selected||hover) Fill(r,selected?new Color(.36f,.20f,.37f):new Color(.30f,.18f,.33f));
            if(pressed) Fill(r,new Color(0f,.22f,.25f,.30f));
            if(selected) Fill(new Rect(r.x+8f*UiScale,r.yMax-3f*UiScale,r.width-16f*UiScale,3f*UiScale),Accent);
            if(blankButton==null) blankButton=new GUIStyle(GUIStyle.none);
            var result=GUI.Button(r,GUIContent.none,blankButton);
            if(e.rawType==EventType.MouseUp&&pressedButtonId==id) pressedButtonId=0;
            return result;
        }

        private void Fill(Rect r,Color color)
        {
            var saved=GUI.color;GUI.color=saved*NativeGuiColor(color);GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=saved;
        }
        private void Stroke(Rect r,Color color,float thickness)
        {
            Fill(new Rect(r.x,r.y,r.width,thickness),color);Fill(new Rect(r.x,r.yMax-thickness,r.width,thickness),color);
            Fill(new Rect(r.x,r.y,thickness,r.height),color);Fill(new Rect(r.xMax-thickness,r.y,thickness,r.height),color);
        }
        private void DrawRule(Rect r,bool jewel=true)
        {
            Fill(r,Silver);
            if(!jewel) return;
            var side=8f*UiScale;var matrix=GUI.matrix;
            GUIUtility.RotateAroundPivot(45f,r.center);
            Fill(new Rect(r.center.x-side*.5f,r.center.y-side*.5f,side,side),Silver);
            Fill(new Rect(r.center.x-side*.3f,r.center.y-side*.3f,side*.6f,side*.6f),Accent);
            GUI.matrix=matrix;
        }

        private void DrawLocalHint()
        {
            if(IsModalOpen || legacyHud==null) return;
            var hint=!string.IsNullOrEmpty(legacyHud.PortalInteractionHint)?legacyHud.PortalInteractionHint:
                !string.IsNullOrEmpty(EquipmentInteractionHint)?EquipmentInteractionHint:
                !string.IsNullOrEmpty(ForgeInteractionHint)?ForgeInteractionHint:
                !string.IsNullOrEmpty(legacyHud.DwellingInteractionHint)?legacyHud.DwellingInteractionHint:legacyHud.WorldInteractionHint;
            if(string.IsNullOrEmpty(hint)) return;
            var r=new Rect(Mathf.Max(MinimapRect.xMax+15f*UiScale,Screen.width*.38f),WorldHudRect.y-44f*UiScale,Mathf.Min(520f*UiScale,Screen.width*.48f),35f*UiScale);
            DrawPanel(r); Label(Inset(r,5f*UiScale),hint,14,TextAnchor.MiddleCenter);
        }

        private void DrawLocalMinimap()
        {
            var source=game.GetComponent<Stage2Minimap>(); if(source==null || source.Terrain==null) return;
            var map=scene.MapDefinition;
            var outer=MinimapRect; var mapSide=outer.height*.86f;
            var inner=new Rect(outer.center.x-mapSide*.5f,outer.center.y-mapSide*.5f,mapSide,mapSide);
            var heroCell=map.WorldToCellCoordinates(scene.PlayerMover.transform.position)-(Vector2)map.MinimumCell+Vector2.one*.5f;
            const float viewFraction=.35f;
            var uv=new Rect(heroCell.x/map.Size.x-viewFraction*.5f,heroCell.y/map.Size.y-viewFraction*.5f,viewFraction,viewFraction);
            DrawCircularTexture(inner,source.Terrain,uv); if(source.DistrictTerrain!=null) DrawCircularTexture(inner,source.DistrictTerrain,uv);
            foreach(var marker in source.Markers)
            {
                if(marker.Kind==Stage2MinimapMarkerKind.City) continue;
                var cell=map.WorldToCellCoordinates(marker.World)-(Vector2)map.MinimumCell+Vector2.one*.5f;
                var unit=new Vector2((cell.x/map.Size.x-uv.x)/uv.width,(cell.y/map.Size.y-uv.y)/uv.height);
                if((unit-Vector2.one*.5f).sqrMagnitude>.235f) continue;
                var p=new Vector2(inner.x+unit.x*inner.width,inner.yMax-unit.y*inner.height);
                DrawWorldMapMarker(p,marker.Kind,UiScale*.65f);
            }
            DrawGuildMinimapMarkers(inner,uv);
            DrawRecoveryMinimap(inner,uv);
            DrawIcon(outer,art.minimapFrame);
            Label(new Rect(outer.center.x-16f*UiScale,outer.y+6f*UiScale,32f*UiScale,25f*UiScale),"N",15,TextAnchor.MiddleCenter);
            DrawCityMinimapMarker(source,inner,uv);
            DrawGuildQuestMinimap(inner,uv);
        }

        private Material CircularMaterial
        {
            get
            {
                if(circularMapMaterial==null && art!=null && art.circularMapShader!=null)
                {
                    circularMapMaterial=new Material(art.circularMapShader){name="Lunar UI circular map",hideFlags=HideFlags.HideAndDontSave};
                    circularMapMaterial.SetFloat("_EncodeToDisplay",UsesMetalLinearTarget?0f:1f);
                }
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
            var content=LabelContent(text);
            while(style.fontSize>Mathf.RoundToInt(11f*UiScale)&&style.CalcSize(content).x>r.width) style.fontSize--;
            GUI.Label(r,content,style);style.fontSize=original;style.wordWrap=true;
        }
        private GUIContent LabelContent(string value)
        {
            value??="";
            if(labelContents.TryGetValue(value,out var content))return content;
            // Retain native text measurement data for unchanged labels/glyphs. A long
            // session's changing clock/resource values must not grow this cache forever.
            if(labelContents.Count>=256)labelContents.Clear();
            content=new GUIContent(value);labelContents.Add(value,content);return content;
        }
        private void Label(Rect r,string value,int size=18,TextAnchor align=TextAnchor.UpperLeft,bool muted=false)
        {
            var style=GetLabelStyle(size,false);style.alignment=align;style.normal.textColor=muted?MutedInk:PlumInk;GUI.Label(r,LabelContent(value),style);
        }
        private void PaleLabel(Rect r,string value,int size,TextAnchor align)
        {var style=GetLabelStyle(size,true);style.alignment=align;GUI.Label(r,LabelContent(value),style);}
        private GUIStyle GetLabelStyle(int size,bool pale)
        {
            var actual=Mathf.Max(9,Mathf.RoundToInt(size*UiScale)); var styles=pale?paleLabelStyles:labelStyles;
            if(!styles.TryGetValue(actual,out var style))
            {
                if(runtimeFont==null && (art==null || art.font==null)) InitializeRuntimeFont();
                style=new GUIStyle(GUI.skin.label){fontSize=actual,wordWrap=true,padding=new RectOffset(0,0,0,0),font=art!=null&&art.font!=null?art.font:runtimeFont};
                style.normal.textColor=pale?new Color(.98f,.93f,.84f):PlumInk; styles[actual]=style;
            }
            return style;
        }
        public static string ChooseInstalledSansSerif(string[] installed,bool preferSegoe)
        {
            if(installed==null) return null;
            var candidates=preferSegoe?new[]{"Segoe UI","Arial","Helvetica Neue","Helvetica","Liberation Sans","DejaVu Sans","Noto Sans"}:
                new[]{"Arial","Helvetica Neue","Helvetica","Liberation Sans","DejaVu Sans","Noto Sans","Segoe UI"};
            foreach(var candidate in candidates)
                foreach(var face in installed)
                    if(string.Equals(face,candidate,StringComparison.OrdinalIgnoreCase)) return face;
            return null;
        }

        private void InitializeRuntimeFont()
        {
            var windows=Application.platform==RuntimePlatform.WindowsPlayer||Application.platform==RuntimePlatform.WindowsEditor;
            var face=ChooseInstalledSansSerif(Font.GetOSInstalledFontNames(),windows);
            if(!string.IsNullOrEmpty(face))
            {
                // A single verified installed face avoids TextCore binding to an absent first fallback.
                runtimeFont=Font.CreateDynamicFontFromOSFont(face,18);
                ownsRuntimeFont=runtimeFont!=null;
                RuntimeFontName=face;
            }
            if(runtimeFont==null)
            {
                runtimeFont=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                ownsRuntimeFont=false;
                RuntimeFontName="LegacyRuntime.ttf";
            }
            Debug.Log("S3_PLAYER_INTERFACE_FONT: "+RuntimeFontName);
        }

        private static bool UsesMetalLinearTarget => SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Metal &&
            QualitySettings.activeColorSpace==ColorSpace.Linear;

        private static Color NativeGuiColor(Color displayColor)
        {
            // Metal's native solid-quad path encodes on write; GUIStyle text already handles color.
            return UsesMetalLinearTarget ? displayColor.linear : displayColor;
        }

        private void DrawTexture(Rect r,Texture2D texture) {if(texture!=null) GUI.DrawTexture(r,texture,ScaleMode.ScaleToFit,true);}
        private void DrawIcon(Rect r,Sprite sprite)
        {
            if(sprite==null || sprite.texture==null || r.width<=0 || r.height<=0) return;
            var src=sprite.rect;var scale=Mathf.Min(r.width/src.width,r.height/src.height); var size=src.size*scale;
            var target=new Rect(r.center-size*.5f,size);
            var uv=new Rect(src.x/sprite.texture.width,src.y/sprite.texture.height,src.width/sprite.texture.width,src.height/sprite.texture.height);
            DrawUiTexture(target,sprite.texture,uv,art!=null&&sprite==art.teleportIcon);
        }

        private void DrawUiTexture(Rect target,Texture texture,Rect uv,bool removeDark=false)
        {
            if((UsesMetalLinearTarget||removeDark)&&CircularMaterial!=null)
            {
                if(Event.current.type!=EventType.Repaint) return;
                var tint=QualitySettings.activeColorSpace==ColorSpace.Linear?GUI.color.linear:GUI.color;
                CircularMaterial.SetFloat("_CircleMask",0);CircularMaterial.SetFloat("_RemoveDark",removeDark?1f:0f);
                CircularMaterial.SetVector("_SourceRect",new Vector4(uv.x,uv.y,uv.width,uv.height));
                CircularMaterial.SetVector("_Tint",new Vector4(tint.r,tint.g,tint.b,tint.a));
                Graphics.DrawTexture(target,texture,uv,0,0,0,0,Color.white,CircularMaterial);
                CircularMaterial.SetFloat("_CircleMask",1);CircularMaterial.SetFloat("_RemoveDark",0);
                CircularMaterial.SetVector("_SourceRect",new Vector4(0,0,1,1));CircularMaterial.SetVector("_Tint",Vector4.one);
            }
            else GUI.DrawTextureWithTexCoords(target,texture,uv);
        }

        private void DrawSliced(Rect r,Sprite sprite,bool center,float ornamentScale=-1f)
        {
            if(sprite==null || r.width<=0 || r.height<=0) return;
            var src=sprite.rect; var b=sprite.border;
            // The central jewels are immutable ornaments, not stretchable vertical edges.
            // Five vertical bands keep corners and the middle ornament at one isotropic scale.
            var factor=ornamentScale>=0f?ornamentScale:FrameScale(r,sprite);
            var middle=src.height*.32f;var midTop=(src.height-middle)*.5f;
            var dx=sliceDx;var dy=sliceDy;var sx=sliceSx;var sy=sliceSy;
            dx[0]=r.x;dx[1]=r.x+b.x*factor;dx[2]=r.xMax-b.z*factor;dx[3]=r.xMax;
            dy[0]=r.y;dy[1]=r.y+b.w*factor;dy[2]=r.center.y-middle*factor*.5f;
            dy[3]=r.center.y+middle*factor*.5f;dy[4]=r.yMax-b.y*factor;dy[5]=r.yMax;
            // Every neighbouring quad must share the same physical pixel edge.
            for(var i=0;i<dx.Length;i++) dx[i]=Mathf.Round(dx[i]);
            for(var i=0;i<dy.Length;i++) dy[i]=Mathf.Round(dy[i]);
            sx[0]=src.x;sx[1]=src.x+b.x;sx[2]=src.xMax-b.z;sx[3]=src.xMax;
            sy[0]=src.yMax;sy[1]=src.yMax-b.w;sy[2]=src.yMax-midTop;
            sy[3]=src.yMax-midTop-middle;sy[4]=src.y+b.y;sy[5]=src.y;
            for(var y=0;y<5;y++) for(var x=0;x<3;x++)
            {
                if(!center && x==1&&y>0&&y<4) continue;
                var target=new Rect(dx[x],dy[y],dx[x+1]-dx[x],dy[y+1]-dy[y]);
                if(target.width<=0 || target.height<=0) continue;
                var uv=new Rect(sx[x]/sprite.texture.width,sy[y+1]/sprite.texture.height,(sx[x+1]-sx[x])/sprite.texture.width,(sy[y]-sy[y+1])/sprite.texture.height);
                DrawUiTexture(target,sprite.texture,uv);
            }
        }
    }
}
