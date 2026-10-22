using System.Collections.Generic;
using Not3A.Stage2.Integration;
using Not3A.Stage3.VisualPilot;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Not3A.Stage3.PlayerInterface
{
    public sealed partial class Stage3PlayerInterfaceBinding
    {
        private Stage2Guild guild;
        private Stage3GuildWorldDriver guildWorld;
        private Stage3WorldTravel guildTravel;
        private bool guildDirty=true,guildPointerNeedsRelease=true;
        private int guildRank,selectedGuildQuest=-1;
        public Stage2Guild Guild=>guild;
        public Stage3GuildWorldDriver GuildWorld=>guildWorld;
        public int TrackedGuildQuestId {get;private set;}=-1;
        public bool TrackGuildQuest(int id)
        {
            if(guild?.FindActive(id)==null)return false;
            TrackedGuildQuestId=id;return true;
        }
        public bool TryGetGuildQuestTarget(int id,out Vector2 position,out bool turnIn)
        {
            position=default;turnIn=false;
            var contract=guild?.FindActive(id);
            if(contract==null||guildWorld==null||!guildWorld.Ready||guildTravel==null||!guildTravel.CityReady)return false;
            turnIn=contract.Cleared;
            position=turnIn?guildTravel.CityPresentation.GuildInteractionPosition:guildWorld.SpotPosition((int)contract.Quest.enemyKind);
            // Aim at the current camp pack, not the now intentionally empty arena centre.
            if(!turnIn)
            {
                var total=Vector2.zero;var count=0;
                foreach(var enemy in guildWorld.Enemies)
                    if(enemy.Alive&&enemy.Generation==contract.Generation&&enemy.Profile.enemyKind==contract.Quest.enemyKind)
                    {total+=enemy.Home;count++;}
                if(count>0)position=total/count;
            }
            return true;
        }
        public bool TryGetTrackedGuildTarget(out Vector2 position,out bool turnIn)
            =>TryGetGuildQuestTarget(TrackedGuildQuestId,out position,out turnIn);
        public bool NearGuild
        {
            get
            {
                if(guildTravel==null||!guildTravel.CityReady||guild==null||scene?.PlayerMover==null)return false;
                var position=(Vector2)scene.PlayerMover.transform.position;
                var footprint=guildTravel.CityPresentation.GuildFootprint;
                var nearest=new Vector2(Mathf.Clamp(position.x,footprint.xMin,footprint.xMax),
                    Mathf.Clamp(position.y,footprint.yMin,footprint.yMax));
                return Vector2.Distance(position,nearest)<=guild.Catalog.InteractionRadius;
            }
        }
        private bool GuildModeAllowed(bool ownWindow)=>CanManageEquipment&&isActiveAndEnabled&&Time.timeScale>0&&
            guildWorld!=null&&guildWorld.Ready&&guildTravel!=null&&guildTravel.CityReady&&!guildTravel.IsPlacingBaseCircle&&!IsDraggingBuilding&&!HasBuildingDraft&&
            (!game.PlayerInputBlocked||ownWindow&&presentation.GuildOpen);
        private void BindGuild()
        {
            if(heroProgression==null||heroRecovery==null)return;
            var catalog=Resources.Load<Stage2GuildCatalog>("Stage3Guild");
            if(catalog==null||!catalog.TryValidate(out _))
            {Debug.LogError("S3_GUILD_BINDING_ERROR: valid Stage3Guild data required.");return;}
            guild=new Stage2Guild(catalog,game.Gathering.Economy.Inventory,heroProgression.Rules);
            guildTravel=GetComponent<Stage3WorldTravel>();
            guildWorld=GetComponent<Stage3GuildWorldDriver>()??gameObject.AddComponent<Stage3GuildWorldDriver>();
            guildWorld.Configure(game,scene,guild,heroProgression,heroRecovery);
            guild.Changed+=MarkGuildDirty;game.Gathering.Economy.Inventory.Changed+=MarkGuildDirty;
            game.RunDriver.Run.Reset+=ResetGuildView;
            presentation.GuildRankSelected+=SelectGuildRank;presentation.GuildQuestSelected+=SelectGuildQuest;
            presentation.GuildActionRequested+=ConfirmGuildAction;presentation.GuildClosed+=OnGuildClosed;
            Debug.Log("S3_GUILD_READY: repeat contracts, typed exploration spots, local board turn-in and F–S trials.");
        }
        private void MarkGuildDirty()
        {
            guildDirty=true;
            if(guild!=null&&guild.FindActive(TrackedGuildQuestId)==null)
                TrackedGuildQuestId=guild.Active.Count>0?guild.Active[0].Quest.id:-1;
        }
        private void OnGuildClosed()=>guildPointerNeedsRelease=true;
        private void ResetGuildView(){guildRank=0;selectedGuildQuest=-1;guildPointerNeedsRelease=true;MarkGuildDirty();}
        public bool TryOpenGuild()
        {
            if(guild==null||!GuildModeAllowed(false))return false;
            ClearBuildingSelection();CancelBlink();CancelPlacement();guildRank=(int)guild.Rank;selectedGuildQuest=-1;
            guildPointerNeedsRelease=true;presentation.ShowGuild(true);RefreshGuild();return true;
        }
        public bool TryOpenGuildAtScreen(Vector2 pointer)
        {
            if(guildTravel==null||!guildTravel.CityReady||game.ContainsScreenPoint(pointer)||
                TryHitBuildingVisual(scene.CameraFollow.BoundCamera,guildTravel.CityPresentation.GuildBoardVisual,pointer,out _)||
                !TryHitBuildingVisual(scene.CameraFollow.BoundCamera,guildTravel.CityPresentation.GuildVisual,pointer,out _))return false;
            return TryOpenGuild();
        }
        private void SelectGuildRank(int rank)
        {
            if(!presentation.GuildOpen||!guild.BoardAvailable((Stage2GuildRank)rank))return;
            guildRank=rank;selectedGuildQuest=-1;RefreshGuild();
        }
        private void SelectGuildQuest(int id)
        {
            if(!presentation.GuildOpen)return;
            foreach(var view in presentation.GuildView.Quests)if(view.Id==id){selectedGuildQuest=id;RefreshGuild();return;}
        }
        private void ConfirmGuildAction(int id)=>TryGuildAction(id);
        public bool TryGuildAction(int id)
        {
            if(!presentation.GuildOpen||selectedGuildQuest!=id||!GuildModeAllowed(true)||!NearGuild)return false;
            var offered=false;foreach(var q in presentation.GuildView.Quests)if(q.Id==id){offered=true;break;}
            if(!offered)return false;
            var action=guild.Evaluate(id);var result=action==Stage2GuildAction.Accept?guild.TryAccept(id,guildWorld.ResetSpot):
                action==Stage2GuildAction.TurnIn&&guild.TryTurnIn(id);
            if(result&&action==Stage2GuildAction.Accept)TrackGuildQuest(id);
            if(guildRank>(int)guild.Rank)guildRank=(int)guild.Rank;
            RefreshGuild();return result;
        }
        private void UpdateGuild()
        {
            if(guild==null)return;
            var mouse=Mouse.current;if(mouse==null||!mouse.leftButton.isPressed)guildPointerNeedsRelease=false;
            if(presentation.GuildOpen)
            {
                if(!CanManageEquipment){presentation.ShowGuild(false);return;}
                if(guildDirty)RefreshGuild();UpdateGuildAvailability();return;
            }
            if(!guildPointerNeedsRelease&&mouse!=null&&mouse.leftButton.wasPressedThisFrame)
                TryOpenGuildAtScreen(mouse.position.ReadValue());
        }
        private void RefreshGuild()
        {
            if(guild==null)return;guildDirty=false;var entries=new List<Stage3GuildQuestView>();
            for(var i=0;i<guild.Catalog.Count;i++)
            {
                var q=guild.Catalog.GetAt(i);var trial=q.trial&&guildRank==(int)guild.Rank&&guild.Available(q);
                if(!trial&&(q.trial||(int)q.rank!=guildRank))continue;
                var contract=guild.FindActive(q.id);
                var view=new Stage3GuildQuestView{Id=q.id,Rank=(int)q.rank,EnemyKind=(int)q.enemyKind,
                    RewardGold=guild.RewardGold(q),RewardExperience=guild.RewardExperience(q),Title=q.title,Location=q.location,
                    Objective=q.objective+(contract==null?"":$" · {q.population-contract.Remaining}/{q.population}"),IsTrial=q.trial};
                if(q.trial)entries.Insert(0,view);else entries.Add(view);
            }
            if(!entries.Exists(q=>q.Id==selectedGuildQuest))selectedGuildQuest=entries.Count>0?entries[0].Id:-1;
            presentation.SetGuild(new Stage3GuildView{PersonalRank=(int)guild.Rank,SelectedRank=guildRank,
                CompletedForRank=guild.CompletedFor((Stage2GuildRank)guildRank),SelectedQuestId=selectedGuildQuest,
                Quests=entries.ToArray(),BoardMessage="Выберите заказ на доске"});
            UpdateGuildAvailability();
        }
        private void UpdateGuildAvailability()
        {
            var view=presentation.GuildView;if(view?.Quests==null)return;
            foreach(var q in view.Quests)
            {
                var action=guild.Evaluate(q.Id);var can=action==Stage2GuildAction.Accept||action==Stage2GuildAction.TurnIn;
                var cleared=guild.FindActive(q.Id)?.Cleared??false;
                q.CanExecute=can&&GuildModeAllowed(true)&&NearGuild;
                q.ActionLabel=can&&!NearGuild?"Подойдите к гильдии":cleared?"Забрать награду":action==Stage2GuildAction.InProgress?"Выполняется":q.IsTrial?"Принять испытание":"Принять задание";
                q.Status=cleared?"Заказ выполнен · Вернитесь к гильдии":action==Stage2GuildAction.InProgress?"Зачистите отмеченный спот":"";
                q.UnavailableReason=!GuildModeAllowed(true)?"Гильдия сейчас недоступна":!NearGuild?"Для принятия и сдачи подойдите к зданию гильдии":
                    action==Stage2GuildAction.SpotOccupied?"Сначала сдайте текущий заказ этого спота":
                    action==Stage2GuildAction.GoldLimit?"Достигнут предел золота":action==Stage2GuildAction.Locked?"Этот ранг ещё не открыт":q.Status;
            }
        }
        private void UnbindGuild()
        {
            if(guild==null)return;guild.Changed-=MarkGuildDirty;game.Gathering.Economy.Inventory.Changed-=MarkGuildDirty;
            game.RunDriver.Run.Reset-=ResetGuildView;presentation.GuildRankSelected-=SelectGuildRank;
            presentation.GuildQuestSelected-=SelectGuildQuest;presentation.GuildActionRequested-=ConfirmGuildAction;
            presentation.GuildClosed-=OnGuildClosed;presentation.SetGuild(null);
        }
    }
}
