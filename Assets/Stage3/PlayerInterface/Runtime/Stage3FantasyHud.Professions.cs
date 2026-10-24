using System;
using System.Globalization;
using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    /// <summary>Actual profession and equipped-tool facts supplied by the scene binding.</summary>
    public sealed class Stage3ProfessionsView
    {
        public Stage3ProfessionView[] Professions;
    }

    public sealed class Stage3ProfessionView
    {
        public string Title, Description;
        public int Mastery, MaxMastery, XpWithin, RequiredXp, AvailablePoints;
        public Stage3ProfessionTalentView[] Talents;
        public Stage3ProfessionTalentView Capstone;
        public Stage3EquipmentItemView Tool;
    }

    public sealed class Stage3ProfessionTalentView
    {
        public string Title, Description, UnavailableReason;
        public int Rank, MaxRank, PointCost;
        public bool CanUpgrade;
        public int CurrentPower, NextPower;
        public float CurrentDuration, NextDuration, CurrentReachBonus, NextReachBonus;
        // Strength is extra material units; tempo and reach are fractions.
        public float CurrentBonus, NextBonus;
    }

    public sealed partial class Stage3FantasyHud
    {
        private Stage3ProfessionsView professions;
        private bool showProfessions;
        private int selectedProfessionIndex, selectedProfessionTalentIndex;
        private Texture2D professionIcons;
        private bool professionIconsLoaded;
        private static readonly CultureInfo ProfessionCulture = CultureInfo.GetCultureInfo("ru-RU");

        public Stage3ProfessionsView ProfessionsView => professions;
        public bool IsProfessionsOpen => ActiveWindow == Stage3PlayerWindow.Hero && showProfessions;
        public int SelectedProfessionIndex => selectedProfessionIndex;
        public int SelectedProfessionTalentIndex => selectedProfessionTalentIndex;
        public event Action<int, int> ProfessionTalentUpgradeRequested;
        public event Action<int> ProfessionCapstoneUpgradeRequested;
        private bool selectedProfessionCapstone;

        public void SetProfessions(Stage3ProfessionsView value)
        {
            professions = value;
            ClampProfessionSelection();
        }

        public void ShowProfessions(bool value) { showProfessions = value; showAdventurer = false; }

        public void SelectProfession(int index)
        {
            if (professions?.Professions == null || index < 0 ||
                index >= professions.Professions.Length || professions.Professions[index] == null) return;
            if (selectedProfessionIndex != index) selectedProfessionTalentIndex = 0;
            selectedProfessionIndex = index;
            selectedProfessionCapstone=false;
            ClampProfessionSelection();
        }

        public void SelectProfessionCapstone(int professionIndex)
        {
            SelectProfession(professionIndex);
            selectedProfessionCapstone=SelectedProfession?.Capstone!=null;
        }

        public void SelectProfessionTalent(int index)
        {
            var talents = SelectedProfession?.Talents;
            if (talents == null || index < 0 || index >= talents.Length || talents[index] == null) return;
            selectedProfessionTalentIndex = index;
            selectedProfessionCapstone=false;
        }

        private Stage3ProfessionView SelectedProfession =>
            professions?.Professions != null && selectedProfessionIndex >= 0 &&
            selectedProfessionIndex < professions.Professions.Length
                ? professions.Professions[selectedProfessionIndex] : null;

        private Stage3ProfessionTalentView SelectedProfessionTalent => selectedProfessionCapstone ? SelectedProfession?.Capstone :
            SelectedProfession?.Talents != null && selectedProfessionTalentIndex >= 0 &&
            selectedProfessionTalentIndex < SelectedProfession.Talents.Length
                ? SelectedProfession.Talents[selectedProfessionTalentIndex] : null;

        private void ClampProfessionSelection()
        {
            selectedProfessionIndex = Mathf.Clamp(selectedProfessionIndex, 0,
                Mathf.Max(0, (professions?.Professions?.Length ?? 0) - 1));
            selectedProfessionTalentIndex = Mathf.Clamp(selectedProfessionTalentIndex, 0,
                Mathf.Max(0, (SelectedProfession?.Talents?.Length ?? 0) - 1));
        }

        private void ResetProfessionPresentation()
        {
            showProfessions = false;
            showAdventurer = false;
            selectedSkillId = 0;
            selectedProfessionIndex = selectedProfessionTalentIndex = 0;
            selectedProfessionCapstone=false;
        }

        private Rect HeroDevelopmentContentRect
        {
            get
            {
                var r = WindowContentRect;
                return new Rect(r.x, r.y + 58f * UiScale, r.width, Mathf.Max(0, r.height - 58f * UiScale));
            }
        }

        /// <summary>False is the existing combat tab; true is professions.</summary>
        public Rect ProfessionTabRect(bool professionTab) => HeroDevelopmentTabRect(professionTab ? 1 : 0);

        private Rect ProfessionColumn(int index) => index==2 ? Inset(SkillDetailsRect,24f*UiScale) : SkillTreeRect;
        public Rect ProfessionCardRect(int index)
        {
            if(!IsProfessionsOpen||index<0||index>1)return Rect.zero;
            var r=SkillTreeRect;var s=UiScale;var height=(r.height-24f*s)*.5f;
            return new Rect(r.x+16f*s,r.y+12f*s+index*(height+8f*s),r.width-32f*s,38f*s);
        }
        public Rect ProfessionTalentRect(int index) => ProfessionTalentRect(selectedProfessionIndex,index);
        private Rect ProfessionTalentRect(int profession,int index)
        {
            if(!IsProfessionsOpen||index<0||index>=3)return Rect.zero;
            var header=ProfessionCardRect(profession);var s=UiScale;
            var width=(header.width-28f*s)/3f;
            return new Rect(header.x+index*(width+14f*s),header.yMax+6f*s,width,154f*s);
        }
        public Rect ProfessionCapstoneRect(int profession)
        {
            var strength=ProfessionTalentRect(profession,0);
            return new Rect(strength.xMax-44f*UiScale,strength.yMax+4f*UiScale,40f*UiScale,40f*UiScale);
        }
        public Rect ProfessionUpgradeRect => !IsProfessionsOpen||SelectedProfessionTalent==null?Rect.zero:
            new Rect(ProfessionColumn(2).x,ProfessionColumn(2).yMax-180f*UiScale,ProfessionColumn(2).width,49f*UiScale);

        private void DrawHeroDevelopmentTabs()
        {
            var r = WindowContentRect; var s = UiScale;
            DrawRule(new Rect(r.x, r.y + 103f * s, r.width, s), false);
            var requested = -1;
            var selected = showProfessions ? 1 : showAdventurer ? 2 : 0;
            var count = heroLoadout != null ? 3 : 2;
            for (var i = 0; i < count; i++)
            {
                var available = i != 2 || heroLoadout?.AdventurerNodes?.Length > 0;
                var title = i == 0 ? "Бой" : i == 1 ? "Добыча" : "Авантюрист";
                if (BranchTab(HeroDevelopmentTabRect(i), title, selected == i, available)) requested = i;
            }
            if (requested >= 0)
            {
                ShowHeroDevelopmentTab(requested);
                GUIUtility.ExitGUI();
            }
        }

        private void DrawProfessions()
        {
            var s=UiScale;
            DrawDevelopmentSummary();
            DrawSliced(SkillTreeRect,(art.controlPanel??art.panelFrame),true,.20f*s);
            DrawSliced(SkillDetailsRect,(art.controlPanel??art.panelFrame),true,.22f*s);
            if(professions?.Professions==null)return;
            for(var p=0;p<Mathf.Min(2,professions.Professions.Length);p++)
            {
                var profession=professions.Professions[p];if(profession==null)continue;
                var header=ProfessionCardRect(p);
                DrawProfessionIcon(new Rect(header.x,header.y,42f*s,42f*s),p,-1);
                Label(new Rect(header.x+50f*s,header.y,220f*s,24f*s),profession.Title,21);
                Label(new Rect(header.x+50f*s,header.y+25f*s,240f*s,21f*s),$"Мастерство {profession.Mastery} / {profession.MaxMastery}",15);
                var xp=new Rect(header.xMax-240f*s,header.y+10f*s,230f*s,24f*s);
                DrawProfessionExperienceBar(xp,profession);
                PaleLabel(xp,$"{profession.XpWithin} / {profession.RequiredXp}",14,TextAnchor.MiddleCenter);
                if(profession.Talents!=null)for(var i=0;i<Mathf.Min(3,profession.Talents.Length);i++)
                {
                    var talent=profession.Talents[i];if(talent==null)continue;
                    var card=ProfessionTalentRect(p,i);
                    if(ProfessionCardButton(card,p==selectedProfessionIndex&&i==selectedProfessionTalentIndex&&!selectedProfessionCapstone)){SelectProfession(p);SelectProfessionTalent(i);}
                    DrawProfessionIcon(new Rect(card.center.x-35f*s,card.y+6f*s,70f*s,70f*s),p,i);
                    Label(new Rect(card.x+10f*s,card.y+77f*s,card.width-20f*s,24f*s),talent.Title,18,TextAnchor.MiddleCenter);
                    var count=talent.MaxRank;var pitch=Mathf.Min(31f*s,(card.width-32f*s)/Mathf.Max(1,count));
                    var start=card.center.x-count*pitch*.5f;
                    for(var rank=0;rank<count;rank++)
                    {
                        var dot=new Rect(start+rank*pitch,card.yMax-48f*s,20f*s,20f*s);
                        DrawDisc(dot,Silver);DrawDisc(Inset(dot,2f*s),rank<talent.Rank?Accent:PlumInk);
                    }
                    Label(new Rect(card.x,card.yMax-28f*s,card.width,20f*s),$"{talent.Rank} / {talent.MaxRank}",15,TextAnchor.MiddleCenter);
                }
                if(profession.Capstone!=null)
                {
                    var cap=ProfessionCapstoneRect(p);var from=ProfessionTalentRect(p,0);
                    DrawSkillLinkSegment(new Vector2(from.center.x,from.yMax),new Vector2(from.center.x,cap.center.y),MutedInk);
                    DrawSkillLinkSegment(new Vector2(from.center.x,cap.center.y),new Vector2(cap.x,cap.center.y),MutedInk);
                    if(ProfessionCardButton(cap,p==selectedProfessionIndex&&selectedProfessionCapstone)){SelectProfession(p);selectedProfessionCapstone=true;}
                    if(art.professionCapstoneIcons!=null&&p<art.professionCapstoneIcons.Length)DrawIcon(Inset(cap,3f*s),art.professionCapstoneIcons[p]);
                    if(cap.Contains(CurrentGuiPointer))DrawLoadoutTooltip(cap,profession.Capstone.Title+"\n"+profession.Capstone.Description);
                }
            }
            if(DrawProfessionTalentDetails(ProfessionColumn(2),SelectedProfession,SelectedProfessionTalent))
            {
                if(selectedProfessionCapstone)ProfessionCapstoneUpgradeRequested?.Invoke(selectedProfessionIndex);
                else ProfessionTalentUpgradeRequested?.Invoke(selectedProfessionIndex,selectedProfessionTalentIndex);
            }
        }

        private bool ProfessionCardButton(Rect r, bool selected)
        {
            DrawSliced(r,art.controlPanel??art.panelFrame,true,.35f*UiScale);
            if(selected)Stroke(Inset(r,4f*UiScale),Accent,1.5f*UiScale);
            if (blankButton == null) blankButton = new GUIStyle(GUIStyle.none);
            return GUI.Button(r, GUIContent.none, blankButton);
        }

        private void DrawProfessionExperienceBar(Rect r, Stage3ProfessionView value)
        {
            DrawStatusBevel(r, PlumInk);
            var atCap = value.MaxMastery > 0 && value.Mastery >= value.MaxMastery;
            var fraction = atCap ? 1f : value.RequiredXp > 0 ? Mathf.Clamp01((float)value.XpWithin / value.RequiredXp) : 0f;
            var track = Inset(r, 4f * UiScale);
            if (fraction <= 0f) return;
            GUI.BeginGroup(new Rect(track.x, track.y, track.width * fraction, track.height));
            DrawBevelGradient(new Rect(0, 0, track.width, track.height),
                new Color(1f, .72f, .16f), new Color(.79f, .27f, .02f), 6f * UiScale);
            GUI.EndGroup();
        }

        private bool DrawProfessionTalentDetails(Rect r,Stage3ProfessionView profession,Stage3ProfessionTalentView talent)
        {
            if(profession==null||talent==null)return false;
            var s=UiScale;var cap=talent.Rank>=talent.MaxRank;
            Label(new Rect(r.x,r.y,r.width,52f*s),talent.Title,26);
            Label(new Rect(r.x,r.y+54f*s,r.width,26f*s),$"Ранг {talent.Rank} / {talent.MaxRank}",19);
            Label(new Rect(r.x,r.y+88f*s,r.width,70f*s),talent.Description,17);
            if(!selectedProfessionCapstone)
            {
                var row=new Rect(r.x,r.y+166f*s,r.width,32f*s);
                var hasTool=profession.Tool!=null;
                DrawProfessionStat(row,"Добыча за удар",hasTool?talent.CurrentPower.ToString():"—",hasTool?talent.NextPower.ToString():"—",talent.CurrentPower!=talent.NextPower);
                row.y+=36f*s;
                DrawProfessionStat(row,"Время удара",hasTool?ProfessionNumber(talent.CurrentDuration):"—",hasTool?ProfessionNumber(talent.NextDuration)+" с":"—",!Mathf.Approximately(talent.CurrentDuration,talent.NextDuration));
                row.y+=36f*s;
                DrawProfessionStat(row,"Досягаемость",ProfessionPercent(talent.CurrentReachBonus),ProfessionPercent(talent.NextReachBonus),!Mathf.Approximately(talent.CurrentReachBonus,talent.NextReachBonus));
            }
            else Label(new Rect(r.x,r.y+166f*s,r.width,64f*s),"Требуется: Сила удара V",17,TextAnchor.UpperLeft,true);
            var button=ProfessionUpgradeRect;
            var action=cap?"Максимальный ранг":(talent.Rank==0?"Изучить":"Улучшить")+" · "+talent.PointCost+" очк.";
            var requested=Button(button,action,false,talent.CanUpgrade&&!cap);
            Label(new Rect(r.x,button.yMax+4f*s,r.width,40f*s),!talent.CanUpgrade&&!cap?talent.UnavailableReason:"",14,TextAnchor.UpperCenter,true);
            var tool=new Rect(r.x,r.yMax-85f*s,70f*s,70f*s);
            if(profession.Tool!=null)
            {
                DrawEquipmentCell(tool,profession.Tool);
                Label(new Rect(tool.xMax+12f*s,tool.y,r.width-tool.width-12f*s,68f*s),"Надето\n"+profession.Tool.Name+"\nLVL "+profession.Tool.Level,16,TextAnchor.MiddleLeft);
            }
            else Label(new Rect(r.x,tool.y,r.width,60f*s),selectedProfessionIndex==0?"Кирка не надета":"Топор не надет",18,TextAnchor.MiddleCenter,true);
            return requested;
        }

        private void DrawProfessionStat(Rect row, string title, string current, string next, bool changed)
        {
            Label(new Rect(row.x, row.y, row.width * .61f, row.height), title, 17, TextAnchor.MiddleLeft, !changed);
            var valueRect = new Rect(row.x + row.width * .61f, row.y, row.width * .39f, row.height);
            var style = GetLabelStyle(18, false);
            var original = style.normal.textColor;
            style.normal.textColor = changed ? Accent : MutedInk;
            style.alignment = TextAnchor.MiddleRight;
            GUI.Label(valueRect, LabelContent(current + " → " + next), style);
            style.normal.textColor = original;
        }

        private static string ProfessionNumber(float value) => value.ToString("0.##", ProfessionCulture);
        private static string ProfessionPercent(float fraction) => (fraction * 100f).ToString("0.#", ProfessionCulture) + "%";

        private void DrawProfessionIcon(Rect r, int professionIndex, int talentIndex)
        {
            if (!professionIconsLoaded)
            {
                professionIconsLoaded = true;
                professionIcons = Resources.Load<Texture2D>("Stage3ProfessionIcons");
            }
            if (professionIcons != null)
            {
                // Atlas: miner, logger, miner strength, miner tempo /
                // miner reach, logger strength, logger tempo, logger reach.
                var index = talentIndex < 0 ? professionIndex :
                    professionIndex == 0 ? talentIndex + 2 : talentIndex + 5;
                var uv = new Rect((index % 4) * .25f, index < 4 ? .5f : 0f, .25f, .5f);
                DrawUiTexture(r, professionIcons, uv);
                return;
            }
            DrawDisc(Inset(r, r.width * .15f), PlumInk);
            DrawEquipmentIcon(Inset(r, r.width * .21f), professionIndex == 0 ? 6 : 7, 0);
            DrawMedallion(r);
        }
    }
}
