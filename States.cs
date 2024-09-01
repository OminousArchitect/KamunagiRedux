using System.Collections.Generic;
using System;
using R2API;
using Kamunagi;

namespace Kamunagi
{
    public static class States
    {
        internal static void RegisterStates()
        {
            bool hmm;
            //primaries
            ContentAddition.AddEntityState<SoeiMusou>(out hmm);
            ContentAddition.AddEntityState<AltSoeiMusou>(out hmm);
            ContentAddition.AddEntityState<ReaverMusou>(out hmm);
            //secondaries
            ContentAddition.AddEntityState<EnnakamuyEarth>(out hmm);
            ContentAddition.AddEntityState<WindBoomerang>(out hmm);
            ContentAddition.AddEntityState<DenebokshiriBrimstone>(out hmm);
            ContentAddition.AddEntityState<KujyuriFrost>(out hmm);
            //utilities
            ContentAddition.AddEntityState<Mikazuchi>(out hmm);
            ContentAddition.AddEntityState<HonokasVeil>(out hmm);
            ContentAddition.AddEntityState<WohsisZone>(out hmm);
            ContentAddition.AddEntityState<AtuysTides>(out hmm);
            
            ContentAddition.AddEntityState<JachdwaltTestForTarget>(out hmm);
            ContentAddition.AddEntityState<JachdwaltInitEvis>(out hmm);
            ContentAddition.AddEntityState<JachdwaltDoEvis>(out hmm);
            //specials
            ContentAddition.AddEntityState<SobuGekishoha>(out hmm);
            ContentAddition.AddEntityState<TheGreatSealing>(out hmm);
            ContentAddition.AddEntityState<LightOfNaturesAxiom>(out hmm);
            //extra skills
            ContentAddition.AddEntityState<SummonFriendlyEnemy>(out hmm);
            ContentAddition.AddEntityState<SummonMothmoth>(out hmm);
            ContentAddition.AddEntityState<XinZhao>(out hmm);
            ContentAddition.AddEntityState<MashiroBlessing>(out hmm);

            //base states
            ContentAddition.AddEntityState<BaseTwinState>(out hmm);
            ContentAddition.AddEntityState<KamunagiCharacterMain>(out hmm);
            ContentAddition.AddEntityState<ChannelAscension>(out hmm);
            ContentAddition.AddEntityState<DarkAscension>(out hmm);
            ContentAddition.AddEntityState<KamunagiDeathState>(out hmm);
            ContentAddition.AddEntityState<TwinsSpawnState>(out hmm);
            ContentAddition.AddEntityState<Hover>(out hmm);
        }
    }
}