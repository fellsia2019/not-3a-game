using UnityEngine;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3FantasyHud
    {
        public void ShowHeroDevelopmentTab(int index)
        {
            if (index < 0 || index > 2) return;
            if (index == 2 && (heroLoadout?.AdventurerNodes == null || heroLoadout.AdventurerNodes.Length == 0)) return;
            showProfessions = index == 1;
            showAdventurer = index == 2;
            selectedSkillId = showAdventurer ? heroLoadout.AdventurerNodes[0].Id : 0;
        }

        private void DrawHeroSkillTree()
        {
            var s=UiScale;
            DrawDevelopmentSummary();
            var graph = SkillTreeRect; var details = SkillDetailsRect;
            DrawSliced(graph, (art.controlPanel??art.panelFrame), true, .20f * s);
            DrawSliced(details, (art.controlPanel??art.panelFrame), true, .22f * s);
            var nodes = showAdventurer ? heroLoadout?.AdventurerNodes : heroLoadout?.Nodes;
            if (nodes == null || nodes.Length == 0)
            { Label(graph, "Навыки пока недоступны", 24, TextAnchor.MiddleCenter, true); return; }
            if (!showAdventurer)
            {
                Label(new Rect(graph.x + 22f * s, graph.y + 15f * s, 180f * s, 28f * s), "ЛУК", 19);
                Label(new Rect(graph.x + 22f * s, graph.yMax - 43f * s, 180f * s, 28f * s), "ПОСОХ", 19);
            }
            // Only actual prerequisites draw an edge: column positions never create purchase rules.
            foreach (var node in nodes)
            {
                if (node == null || node.RequiredNodeId < 0 || node.RequiredNodeId == node.Id) continue;
                var required = FindSkill(node.RequiredNodeId);
                if (required == null) continue;
                DrawSkillConnection(SkillNodeRect(required.Id), SkillNodeRect(node.Id), required.Rank > 0&&!required.KnowledgeLocked&&!node.KnowledgeLocked);
            }
            var selection = -1;
            foreach (var node in nodes)
            {
                if (node == null) continue;
                var r = SkillNodeRect(node.Id); if (r.width <= 0) continue;
                if(node.KnowledgeLocked){DrawKnowledgeLockedSkill(r,node.Id);continue;}
                var selected = node.Id == selectedSkillId;
                var old = GUI.color;
                if (node.Rank <= 0 && !node.CanUpgrade) GUI.color = old * new Color(.68f, .65f, .72f, 1f);
                if(selected)DrawLunarJewel(new Vector2(r.center.x,r.y-5f*s),9f*s);
                DrawDisc(Inset(r, r.width * .11f), PlumInk);
                DrawSkillArtwork(Inset(r, r.width * .105f), node.Id);
                DrawMedallion(r, selected);
                GUI.color = old;
                if (node.IsActive || showAdventurer)
                {
                    var badge = new Rect(r.center.x - 14f * s, r.yMax - 21f * s, 28f * s, 27f * s);
                    DrawDisc(badge, Silver); DrawDisc(Inset(badge, 1.5f * s), PlumInk);
                    PaleLabel(badge, node.Id == 0 ? "Q" : node.Rank.ToString(), 17, TextAnchor.MiddleCenter);
                    DrawSkillNodeTitle(SkillNodeLabelRect(node.Id), node.Title);
                }
                else if (node.Rank > 0)
                {
                    var badge = new Rect(r.xMax - 16f * s, r.yMax - 15f * s, 18f * s, 18f * s);
                    DrawDisc(badge, PlumInk); PaleLabel(badge, node.Rank.ToString(), 12, TextAnchor.MiddleCenter);
                }
                DrawAutocastMark(r,node);HandleSkillAutocast(r,node);
                if (blankButton == null) blankButton = new GUIStyle(GUIStyle.none);
                var hit = r;
                if (r.width < 44f * s) hit = new Rect(r.center - Vector2.one * (22f * s), Vector2.one * (44f * s));
                if (GUI.Button(hit, GUIContent.none, blankButton)) selection = node.Id;
            }
            var upgrade = DrawSkillDetails(details, FindSkill(selectedSkillId));
            if (selection >= 0) selectedSkillId = selection;
            if (upgrade) SkillUpgradeRequested?.Invoke(selectedSkillId);
        }

        private void DrawDevelopmentSummary()
        {
            RefreshHeroProgressionLabels();
            var s = UiScale; var header = WindowContentRect;
            Label(new Rect(header.x - 12f * s, header.y, 325f * s, 44f * s), "Развитие героя", 32);
            if (heroProgression != null)
            {
                Label(new Rect(header.x + 335f * s, header.y + 5f * s, 136f * s, 34f * s), heroLevelCapLabel, 21, TextAnchor.MiddleLeft);
                var xp = new Rect(header.x + 488f * s, header.y + 9f * s, 260f * s, 29f * s);
                DrawExperienceBar(xp);
                Label(new Rect(xp.xMax + 18f * s, header.y + 5f * s, 290f * s, 34f * s), heroPointsLabel, 22, TextAnchor.MiddleLeft);
            }
            DrawRule(new Rect(header.x - 14f * s, header.y + 48f * s, header.width + 28f * s, s));
        }

        private void DrawSkillConnection(Rect from, Rect to, bool learned)
        {
            if (from.width <= 0 || to.width <= 0 || Event.current.type != EventType.Repaint) return;
            var vector = to.center - from.center;
            if (vector.sqrMagnitude < 1f) return;
            var color = learned ? new Color(.30f, .60f, .62f) : new Color(.58f, .43f, .35f);
            if(showAdventurer)
            {
                var routeStart=new Vector2(from.xMax,from.center.y);var routeEnd=new Vector2(to.x,to.center.y);
                var mid=(routeStart.x+routeEnd.x)*.5f;
                DrawSkillLinkSegment(routeStart,new Vector2(mid,routeStart.y),color);
                DrawSkillLinkSegment(new Vector2(mid,routeStart.y),new Vector2(mid,routeEnd.y),color);
                DrawSkillLinkSegment(new Vector2(mid,routeEnd.y),routeEnd,color);
                return;
            }
            // Lower satellites connect around the reserved caption band, never through its text.
            if (from.width > 75f * UiScale && from.width < 100f * UiScale && to.width < 60f * UiScale && vector.y > 90f * UiScale)
            {
                var start = from.center + Vector2.right * (from.width * .43f);
                var elbow = new Vector2(from.center.x + 96f * UiScale, from.center.y);
                var lower = new Vector2(elbow.x, from.center.y + 100f * UiScale);
                var entry = new Vector2(to.center.x, lower.y);
                var end = to.center - Vector2.up * (to.width * .43f);
                DrawSkillLinkSegment(start, elbow, color); DrawSkillLinkSegment(elbow, lower, color);
                DrawSkillLinkSegment(lower, entry, color); DrawSkillLinkSegment(entry, end, color);
                return;
            }
            var horizontal=Mathf.Abs(vector.x)>=Mathf.Abs(vector.y);
            var a=horizontal?new Vector2(vector.x>0?from.xMax:from.x,from.center.y):new Vector2(from.center.x,vector.y>0?from.yMax:from.y);
            var b=horizontal?new Vector2(vector.x>0?to.x:to.xMax,to.center.y):new Vector2(to.center.x,vector.y>0?to.y:to.yMax);
            if(from.width>75f*UiScale&&to.width>75f*UiScale&&Mathf.Abs(vector.y)<10f*UiScale&&Mathf.Abs(vector.x)>240f*UiScale)
            {
                var lane=from.y-48f*UiScale;
                a=new Vector2(from.center.x,from.y);b=new Vector2(to.center.x,to.y);
                DrawSkillLinkSegment(a,new Vector2(a.x,lane),color);
                DrawSkillLinkSegment(new Vector2(a.x,lane),new Vector2(b.x,lane),color);
                DrawSkillLinkSegment(new Vector2(b.x,lane),b,color);return;
            }
            if(horizontal)
            {
                var mid=(a.x+b.x)*.5f;
                DrawSkillLinkSegment(a,new Vector2(mid,a.y),color);
                DrawSkillLinkSegment(new Vector2(mid,a.y),new Vector2(mid,b.y),color);
                DrawSkillLinkSegment(new Vector2(mid,b.y),b,color);
            }
            else
            {
                var mid=(a.y+b.y)*.5f;
                DrawSkillLinkSegment(a,new Vector2(a.x,mid),color);
                DrawSkillLinkSegment(new Vector2(a.x,mid),new Vector2(b.x,mid),color);
                DrawSkillLinkSegment(new Vector2(b.x,mid),b,color);
            }
        }

        private void DrawSkillNodeTitle(Rect r, string title)
        {
            // Explicit word boundaries prevent Unity from splitting a long Russian word.
            var text = title ?? "";
            var space = text.IndexOf(' ');
            if (space >= 0) text = text.Substring(0, space) + "\n" + text.Substring(space + 1);
            var style = GetLabelStyle(17, false); var oldSize = style.fontSize;
            style.alignment = TextAnchor.UpperCenter; style.normal.textColor = PlumInk; style.wordWrap = false;
            var content = LabelContent(text);
            while (style.fontSize > Mathf.RoundToInt(16f * UiScale) && style.CalcSize(content).x > r.width) style.fontSize--;
            GUI.Label(r, content, style); style.fontSize = oldSize; style.wordWrap = true;
        }

        private void DrawSkillLinkSegment(Vector2 a, Vector2 b, Color color)
        {
            SkillLine(a, b, 3f * UiScale, new Color(.34f, .22f, .25f));
            SkillLine(a, b, 1.3f * UiScale, color);
        }

        private void SkillLine(Vector2 a, Vector2 b, float width, Color color)
        {
            var matrix = GUI.matrix; var delta = b - a;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, a);
            Fill(new Rect(a.x, a.y - width * .5f, delta.magnitude + .5f, width), color);
            GUI.matrix = matrix;
        }

        public Rect SkillDescriptionTextRect {get;private set;}
        public Rect SkillEffectTextRect {get;private set;}
        public float SkillDetailsMeasuredBottom {get;private set;}
        public float SkillDetailsAvailableBottom {get;private set;}
        private bool DrawSkillDetails(Rect r, Stage3SkillNodeView node)
        {
            if (node == null||node.KnowledgeLocked) return false;
            var s = UiScale; var inner = Inset(r, 24f * s);
            Label(new Rect(inner.x, inner.y, inner.width, 56f * s), node.Title, 26);
            var category = node.IsActive ? "Активный навык" : "Пассивный талант";
            var weapon = node.Weapon == 0 ? "Лук · " : node.Weapon == 1 ? "Посох · " : "";
            Label(new Rect(inner.x, inner.y + 58f * s, inner.width, 25f * s), weapon + category, 17);
            var button = SkillUpgradeRect;
            var rankY = button.y - 65f * s;
            string requirements = node.RequiredLevel > 1 ? "Уровень " + node.RequiredLevel : "";
            var prerequisite = FindSkill(node.RequiredNodeId);
            if (prerequisite != null) requirements += (requirements.Length > 0 ? " · " : "") + prerequisite.Title;
            if (node.Rank >= node.MaxRank) requirements="Максимальный ранг";
            if(requirements.Length>0&&node.Rank<node.MaxRank)requirements="Требуется: "+requirements;
            var autoHeight=node.SupportsAutocast&&node.Rank>0?34f*s:4f*s;
            var requirementHeight=GetLabelStyle(15,false).CalcHeight(LabelContent(requirements),inner.width);
            var textSize=17;
            float descriptionHeight=0,effectHeight=0;
            var iconTop=inner.y+90f*s;
            var available=rankY-autoHeight-iconTop;
            for(;textSize>=14;textSize--)
            {
                var style=GetLabelStyle(textSize,false);style.wordWrap=true;
                descriptionHeight=style.CalcHeight(LabelContent(node.Description),inner.width);
                effectHeight=style.CalcHeight(LabelContent(node.EffectText),inner.width);
                if(descriptionHeight+effectHeight+requirementHeight+90f*s<=available||textSize==14)break;
            }
            var diameter=Mathf.Min(118f*s,inner.width*.60f,
                Mathf.Max(54f*s,available-descriptionHeight-effectHeight-requirementHeight-24f*s));
            var icon = new Rect(inner.center.x-diameter*.5f,iconTop,diameter,diameter);
            DrawDisc(Inset(icon,diameter*.12f),PlumInk);DrawSkillArtwork(Inset(icon,diameter*.10f),node.Id);
            DrawMedallion(icon,true);DrawAutocastMark(icon,node);HandleSkillAutocast(icon,node);
            var descriptionY=icon.yMax+8f*s;
            SkillDescriptionTextRect=new Rect(inner.x,descriptionY,inner.width,descriptionHeight);
            SkillEffectTextRect=new Rect(inner.x,SkillDescriptionTextRect.yMax+6f*s,inner.width,effectHeight);
            SkillDetailsMeasuredBottom=SkillEffectTextRect.yMax+6f*s+requirementHeight;
            SkillDetailsAvailableBottom=rankY-autoHeight;
            Label(SkillDescriptionTextRect,node.Description,textSize);
            Label(SkillEffectTextRect,node.EffectText,textSize);
            if(requirements.Length>0)
                Label(new Rect(inner.x,SkillEffectTextRect.yMax+6f*s,inner.width,requirementHeight),requirements,15,TextAnchor.UpperLeft,true);
            if(node.SupportsAutocast&&node.Rank>0)
                Label(new Rect(inner.x,rankY-30f*s,inner.width,24f*s),"ПКМ · Автоприменение: "+(node.AutocastEnabled?"вкл.":"выкл."),15,TextAnchor.MiddleCenter);
            Label(new Rect(inner.x, rankY, inner.width * .30f, 29f * s), "Ранги", 17);
            var count = Mathf.Clamp(node.MaxRank, 0, 6); var side = 27f * s;
            var gap = Mathf.Min(12f * s, (inner.width * .70f - side * count) / Mathf.Max(1, count - 1));
            for (var i = 0; i < count; i++)
            {
                var rank = new Rect(inner.x + inner.width * .30f + i * (side + gap), rankY, side, side);
                DrawDisc(rank, Silver); DrawDisc(Inset(rank, 2f * s), i < node.Rank ? Accent : PlumInk);
                PaleLabel(rank, (i + 1).ToString(), 14, TextAnchor.MiddleCenter);
            }
            if (node.IsActive)
                Label(new Rect(inner.x, rankY + 31f * s, inner.width, 24f * s),
                    "MP " + node.ManaCost.ToString("0.#") + " · КД " + node.CooldownTotal.ToString("0.#") + " с", 16, TextAnchor.MiddleCenter);
            var max = node.Rank >= node.MaxRank;
            var clicked = Button(button, max ? "Максимальный ранг" : node.Rank == 0 ? "Изучить" : "Улучшить", false, node.CanUpgrade && !max);
            Label(new Rect(inner.x, button.yMax + 4f * s, inner.width, 25f * s),
                !node.CanUpgrade && !max ? node.UpgradeReason : "", 13, TextAnchor.MiddleCenter, true);
            return clicked;
        }
    }
}
