using System;
using System.Collections.Generic;

namespace MineProxy.Chatting
{
    public static class Cause
    {
        public static readonly Dictionary<string, string[]> Alternatives = new Dictionary<string, string[]>();

        static Cause()
        {
            Alternatives.Add("death.attack.generic", new string[]
            {
                "%1$s died",
                "%1$s went to see god for a cup of tea",
                "%1$s snuffed it",
                "%1$s walked into the light",
                "%1$s passed away",
                "%1$s failed to survive",
                "%1$s took a dirt nap",
                "%1$s passed away",
                "%1$s passed on",
                "%1$s deceased",
                "%1$s is stone dead",
                "%1$s ceased to be",
                "%1$s is no more",
                "%1$s expired",
                "%1$s has gone to meet their maker",
                "%1$s is stiff",
                "%1$s is resting in peace",
                "%1$s kicked the bucket",
                "%1$s shuffled off the mortal coil",
                "%1$s has run down the curtain",
                "%1$s is in a better place",
                "%1$s is six feet under",
                "%1$s has crossed over",
                "%1$s is wandering the Elysian Fields",
                "%1$s bought the farm",
                "%1$s is asleep",
                "%1$s is bloodless",
                "%1$s is buried",
                "%1$s checked out",
                "%1$s is cold",
                "%1$s got cut off",
                "%1$s is defunct",
                "%1$s has departed",
                "%1$s is done for",
                "%1$s is erased",
                "%1$s is extinct",
                "%1$s is gone",
                "%1$s is inanimate",
                "%1$s is late",
                "%1$s is lifeless",
                "%1$s got liquidated",
                "%1$s is mortified",
                "%1$s perished",
                "%1$s in repose",
                "%1$s rubbed out",
                "%1$s snuffed out",
                "%1$s is wasted",
                "%1$s is lost",
                "%1$s got taken",
                "%1$s cashed in",
                "%1$s danced the last dance",
                "%1$s ate it",
                "%1$s is finished",
                "%1$s gone into the west",
                "%1$s kicked off",
                "%1$s got a one-way ticket",
                "%1$s popped off",
                "%1$s was snuffed",
                "%1$s sprouted wings",
                "%1$s has succumbed",
                "%1$s is no longer with us",
                "%1$s has returned to the ground",
                "%1$s is with the ancestors",
                "%1$s is in the grave",
                "%1$s got wacked",
                "%1$s was terminated",
                "%1$s was put down",
                "%1$s got to the big minecraft heaven",
                "%1$s got wings",
                "%1$s is an empty shell",
            });

            Alternatives.Add("death.attack.drown", new string[]
            {"%1$s drowned",
                "%1$s sleeps with the fishes",
                "%1$s is feeding the fishes",
                "%1$s took a long walk off a short pier",
                "%1$s forgot to breathe",
                "%1$s found Nemo",
            });
            Alternatives.Add("death.attack.drown.player", new string[]
            {"%1$s drowned whilst trying to escape %2$s"
            });

            Alternatives.Add("death.fell.accident.generic", new string[] { "%1$s fell from a high place",
                "%1$s hit the ground too hard",
                "%1$s took a leap of faith",
                "%1$s went \"%1$s weeee\"%1$s  SPLAT",
                "%1$s slipped on a banana peel",
                "%1$s apparently can't fly",
                "%1$s found out gravity still works",
                "%1$s has no fear of heights",});
            Alternatives.Add("death.fell.accident.ladder", new string[] { "%1$s fell off a ladder" });
            Alternatives.Add("death.fell.accident.vines", new string[] { "%1$s fell off some vines" });
            Alternatives.Add("death.fell.accident.water", new string[] { "%1$s fell out of the water" });
            Alternatives.Add("death.fell.assist", new string[] { "%1$s was doomed to fall by %2$s" });
            Alternatives.Add("death.fell.assist.item", new string[] { "%1$s was doomed to fall by %2$s using %3$s" });
            Alternatives.Add("death.fell.finish", new string[] { "%1$s fell too far and was finished by %2$s" });
            Alternatives.Add("death.fell.finish.item", new string[] { "%1$s fell too far and was finished by %2$s using %3$s" });
            Alternatives.Add("death.fell.killer", new string[] { "%1$s was doomed to fall" });

            Alternatives.Add("death.attack.anvil", new string[] { "%1$s was squashed by a falling anvil" });
            Alternatives.Add("death.attack.arrow", new string[] { "%1$s was shot by %2$s" });
            Alternatives.Add("death.attack.arrow.item", new string[] { "%1$s was shot by %2$s using %3$s" });
            Alternatives.Add("death.attack.cactus", new string[] { "%1$s was pricked to death",
                "%1$s poked a cactus, but the cactus poked back",
                "%1$s hugged a cactus for too long",});
            Alternatives.Add("death.attack.cactus.player", new string[] { "%1$s walked into a cactus whilst trying to escape %2$s" });
            Alternatives.Add("death.attack.explosion", new string[] { "%1$s blew up",
                "%1$s went kaboom",
                "%1$s got creep'd",
                "%1$s hugged a creeper",
                "%1$s is in too many pieces",
                "%1$s discovered a new use of TNT",
                "%1$s had too many nice thingssss", });
            Alternatives.Add("death.attack.explosion.player", new string[] { "%1$s was blown up by %2$s" });
            Alternatives.Add("death.attack.fall", new string[] { "%1$s hit the ground too hard" });
            Alternatives.Add("death.attack.fallingBlock", new string[] { "%1$s was squashed by a falling block" });
            Alternatives.Add("death.attack.fireball", new string[] { "%1$s was fireballed by %2$s" });
            Alternatives.Add("death.attack.fireball.item", new string[] { "%1$s was fireballed by %2$s using %3$s" });
            Alternatives.Add("death.attack.inFire", new string[] { "%1$s went up in flames",
                "%1$s forgot their fire proof clothing",
                "%1$s burned to death",
                "%1$s spontaneously combusted",
                "%1$s played with matches",});
            Alternatives.Add("death.attack.inFire.player", new string[] { "%1$s walked into fire whilst fighting %2$s" });
            Alternatives.Add("death.attack.inWall", new string[] { "%1$s suffocated in a wall",
                "%1$s was buried alive",
                "%1$s had a cave-in",
                "%1$s got into the fertilizer business",
                "%1$s became worm food", });
            Alternatives.Add("death.attack.indirectMagic", new string[] { "%1$s was killed by %2$s using magic" });
            Alternatives.Add("death.attack.indirectMagic.item", new string[] { "%1$s was killed by %2$s using %3$s" });
            Alternatives.Add("death.attack.lava", new string[] { "%1$s tried to swim in lava",
                "%1$s became obsidian",
                "%1$s took a bath in a lake of fire",});
            Alternatives.Add("death.attack.lava.player", new string[] { "%1$s tried to swim in lava to escape %2$s" });
            Alternatives.Add("death.attack.magic", new string[] { "%1$s was killed by magic" });
            Alternatives.Add("death.attack.mob", new string[] { "%1$s was slain by %2$s",
                "%1$s was killed by %2$s",
                "%1$s got owned by %2$s",
                "%1$s died from the hands of %2$s",
                "%1$s was brutally murdered by %2$s",
                "%1$s angered %2$s",
                "%1$s pissed off %2$s",
                "%1$s was not liked by %2$s",
                "%1$s should not play with %2$s",});
            Alternatives.Add("death.attack.onFire", new string[] { "%1$s burned to death" });
            Alternatives.Add("death.attack.onFire.player", new string[] { "%1$s was burnt to a crisp whilst fighting %2$s" });
            Alternatives.Add("death.attack.outOfWorld", new string[] { "%1$s fell out of the world",
                "%1$s didn't mind the gap",
                "%1$s is floating off into space",
            });
            Alternatives.Add("death.attack.player", new string[] { "%1$s was slain by %2$s" });
            Alternatives.Add("death.attack.player.item", new string[] { "%1$s was slain by %2$s using %3$s" });
            Alternatives.Add("death.attack.starve", new string[] { "%1$s starved to death" });
            Alternatives.Add("death.attack.thorns", new string[] { "%1$s was killed trying to hurt %2$s" });
            Alternatives.Add("death.attack.thrown", new string[] { "%1$s was pummeled by %2$s" });
            Alternatives.Add("death.attack.thrown.item", new string[] { "%1$s was pummeled by %2$s using %3$s" });
            Alternatives.Add("death.attack.wither", new string[] { "%1$s withered away" });

        }
    }
}

