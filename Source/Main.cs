using RimWorld;
using TwitchToolkit;
using TwitchToolkit.IncidentHelpers;
using TwitchToolkit.Store;
using Verse;
using ToolkitCore;
using SR.ModRimWorld.RaidExtension;

namespace AddonRaidExtension
{

    [StaticConstructorOnStartup]
    public static class Main
    {
        static Main() //our constructor
        {
            
        }
    }

    

    public class ModdedRaid : IncidentHelperVariables
    {
        public IncidentWorker worker;
        private IncidentParms parms;
        public override Viewer Viewer { get; set; }
        public int pointsWager = 0;
        public IIncidentTarget target = null;
        private bool separateChannel = false;
        public IncidentCategoryDef Category;
        
       public ModdedRaid()
        {
            worker = new IncidentWorkerPoaching();
            worker.def = IncidentDef.Named("SrPoaching");
            Category = IncidentCategoryDefOf.ThreatSmall;
        }

        public override bool IsPossible(string a, Viewer viewer, bool separateChannel)
        {
            this.separateChannel = separateChannel;
            this.Viewer = viewer;
            string[] command = message.Split(' ');
            if (command.Length < 3)
            {
                TwitchWrapper.SendChatMessage($"@{viewer.username} syntax is {this.storeIncident.syntax}");
                return false;
            }
            
            if (!VariablesHelpers.PointsWagerIsValid(
                    command[2],
                    viewer,
                    ref pointsWager,
                    ref storeIncident,
                    separateChannel
                ))
            {
                return false;
            }

            target = Current.Game.AnyPlayerHomeMap;
            if (target == null)
            {
                return false;
            }
            
            parms = StorytellerUtility.DefaultParmsNow(Category, Helper.AnyPlayerMap);
            parms.points = IncidentHelper_PointsHelper.RollProportionalGamePoints(storeIncident, pointsWager, parms.points);
          
            
            bool canfire = worker.CanFireNow(parms);
            return canfire;
        }

        public override void TryExecute()
        {
            if (worker.TryExecute(parms))
            {
                Viewer.TakeViewerCoins(pointsWager);
                Viewer.CalculateNewKarma(this.storeIncident.karmaType, pointsWager);
                VariablesHelpers.SendPurchaseMessage($"Starting Modded Raid with {pointsWager} points wagered and {(int)parms.points} raid points purchased by {Viewer.username}");
                return;
            }
            TwitchWrapper.SendChatMessage($"@{Viewer.username} could not generate parms for raid.");
        }
    }

    public class HostileVisitor : IncidentHelper
    {
        private IncidentParms parms;
        public IncidentWorker worker;
        public IncidentCategoryDef Category;
        public IIncidentTarget target = null;

        public HostileVisitor()
        {
            worker = new IncidentWorkerPoaching();
            worker.def = IncidentDef.Named("SrPoaching");
            Category = IncidentCategoryDefOf.ThreatSmall;
        }

        public override bool IsPossible()
        {
            target = Current.Game.AnyPlayerHomeMap;
            if (target == null)
            {
                return false;
            }

            parms = StorytellerUtility.DefaultParmsNow(Category, Helper.AnyPlayerMap);
            bool canfire = worker.CanFireNow(parms);
            return canfire;



            /*
            this.worker = (IncidentWorker)new IncidentWorker_VisitorGroup();
            this.worker.def = (__Null)IncidentDef.Named(nameof(HostileVisitor));
            Map anyPlayerMap = Helper.AnyPlayerMap;
            if (anyPlayerMap == null)
                return false;
            this.parms = StorytellerUtility.DefaultParmsNow((IncidentCategoryDef)IncidentCategoryDefOf.Misc, (IIncidentTarget)anyPlayerMap);
            return this.worker.CanFireNow(this.parms);
            */
        }

        public override void TryExecute() => this.worker.TryExecute(this.parms);
    }


    public class Poaching : ModdedRaid
    {
        public Poaching()
        {
            worker = new IncidentWorkerPoaching();
            worker.def = IncidentDef.Named("SrPoaching");
            Category = IncidentCategoryDefOf.ThreatSmall;
        }
    }

    public class SRLogging : ModdedRaid
    {
        public SRLogging()
        {
            worker = new IncidentWorkerLogging();
            worker.def = IncidentDef.Named("SrLogging");
            Category = IncidentCategoryDefOf.ThreatSmall;
        }
    }

    public class HostileTraveler : HostileVisitor
    {
        public HostileTraveler()
        {
            worker = new IncidentWorkerHositleTraveler();
            worker.def = IncidentDef.Named("SrHositleTraveler");
            Category = IncidentCategoryDefOf.Misc;
        }
    }

    public class HostileCaravanPassing : HostileVisitor
    {
        public HostileCaravanPassing()
        {
            worker = new IncidentWorkerHositleTraderCaravanPassing();
            worker.def = IncidentDef.Named("SrHositleCaravanPassing");
            Category = IncidentCategoryDefOf.Misc;
        }
    }
}
