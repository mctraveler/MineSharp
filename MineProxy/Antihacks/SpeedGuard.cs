using System;
using MineProxy.Worlds;

namespace MineProxy
{
    public class SpeedGuard
    {
        public readonly CoordDouble[] HistorySpeed = new CoordDouble[10];
        public readonly CoordDouble[] HistoryMax = new CoordDouble[10];
        public CoordDouble HistoryLastDiff = new CoordDouble();
        public CoordDouble HistoryLastMax = new CoordDouble();
        public DateTime HistoryLastStep = DateTime.MinValue;

        public SpeedGuard()
        {
            for (int n = 0; n < HistorySpeed.Length; n++)
            {
                HistorySpeed [n] = new CoordDouble();
                HistoryMax [n] = new CoordDouble(double.MaxValue, double.MaxValue, 1);
            }
        }

        /// <summary>
        /// Check for speed violations and update position
        /// </summary>
        internal void ClientMovement(WorldSession player, CoordDouble newPos)
        {
            CoordDouble diff = newPos - player.Position;
            diff.X = Math.Sqrt(diff.X * diff.X + diff.Z * diff.Z);
            //diff.Z = dt(later below)
            
#if !DEBUG
            //Ignore movements in Creative mode
            if (player.Mode == GameMode.Creative || player.Mode == GameMode.Spectator)
            {
                player.SetPositionServer(newPos);
                return;
            }
#endif
            
            //Movements if attached already ignored
            //Ignore movements withing the attached to item circle
            if (newPos.Y < -900)
            {
                return;
            }
            //Ignore initial position
            if (player.Position.Abs < 1)
                return;
            
            //For statistics, record all y positions
            //FreqAnalyzer.RecordStep (player, player.Position, newPos);
    
            //Ignore large client movements
            if (diff.X > 8 || diff.Y > 8)
            {
                //string msg = "Too large movement: " + player.Position + " to " + newPos;
                //Log.WritePlayer (player.Client, msg);
                
#if DEBUG
                //player.Tell(msg);
                //Console.WriteLine(DateTime.Now + " " + msg);
#endif
                diff = new CoordDouble();
            }

            //Calculate max speed during last update
            double sec = 0.05;
            CoordDouble max = new CoordDouble(0, 0, sec);
            //Effect index
            int ei = player.EffectSpeed.Active ? player.EffectSpeed.Amplifier + 1 : 0;
            double fac;
            if (player.Crouched)
                fac = LawsOfMinecraft.Crounch;
            else if (player.Sprinting)
                fac = LawsOfMinecraft.Sprint;
            else
                fac = LawsOfMinecraft.Walk;
            if (player.OnGround == false)
                fac += 4;
            double em = (ei - LawsOfMinecraft.ZeroSpeedEffect) * LawsOfMinecraft.SpeedEffectMultiplier * fac;

#if !DEBUG
            //em += 1.5; //Margin, useful for sprintjumping
#endif
            max.X = em * sec;
            if (em < LawsOfMinecraft.LadderSpeed)
#if DEBUG
                em = LawsOfMinecraft.LadderSpeed;
#else
                em = LawsOfMinecraft.LadderSpeed + 1;
#endif
            max.Y = em * sec;

            diff.Z = 0.05;
            HistoryLastDiff += diff;
            HistoryLastMax += max;

            DateTime step = DateTime.Now;
            if (HistoryLastStep.AddSeconds(1) < DateTime.Now)
            {
                if (HistoryLastStep == DateTime.MinValue)
                    HistoryLastStep = step.AddSeconds(-1);
                
                //Old length
                TimeSpan dt = step - HistoryLastStep;
                HistoryLastStep = step;
                HistoryLastDiff.Z = dt.TotalSeconds;
    
                //Speed calculation
                
                //Move forward one step
                for (int n = HistorySpeed.Length - 1; n > 0; n--)
                {
                    HistorySpeed [n] = HistorySpeed [n - 1];
                    HistoryMax [n] = HistoryMax [n - 1];
                }
                HistorySpeed [0] = HistoryLastDiff;
                HistoryMax [0] = HistoryLastMax;
                
                //Skip some ridicoulus speeds
                if (HistorySpeed [0].X > 20 || HistorySpeed [0].Y > 20)
                    HistorySpeed [0] = new CoordDouble();
                
                //Log distance
                player.Player.Settings.WalkDistance += HistoryLastDiff.X;

                HistoryLastDiff = new CoordDouble();
                HistoryLastMax = new CoordDouble();
                
                //Speed limits:
                //Currently no vehicle speeds are recorded here.
                //Ok, since they are controlled serverside anyway.
                
                CoordDouble acc = new CoordDouble();
                max = new CoordDouble();

                int a = 0;
                while (a < HistorySpeed.Length)
                {
                    
                    acc += HistorySpeed [a];
                    max += HistoryMax [a];
                    a++;
                    
                    //Step check
                    if (acc.X < max.X && acc.Y < max.Y)
                        break;

                    //Calibrated for time diff
                    if (acc.X / acc.Z < max.X / max.Z &&
                        acc.Y / acc.Z < max.Y / max.Z)
                        break;

                }

#if DEBUG
                string dmsg = a + " ";
                dmsg += HistorySpeed [0].ToString("0.00");
                dmsg += " / ";
                dmsg += HistoryMax [0].ToString("0.00");
                dmsg += " ";
                dmsg += player.Sprinting ? "S" : "s";
                dmsg += player.Crouched ? "C" : "c";
                dmsg += " E:";
                dmsg += player.EffectSpeed.Active ? "+" + player.EffectSpeed.Amplifier : "";
                dmsg += player.EffectSlow.Active ? "-" + player.EffectSlow.Amplifier : "";
                
                /*if (a <= 1)
                    player.Tell(Chat.Green, dmsg);
                else if (a <= 2)
                    player.Tell(Chat.Yellow, dmsg);
                else 
                    player.TellSystem(Chat.Red, dmsg);
*/
                //Console.WriteLine(player.Player.MinecraftUsername + " " + dmsg);
#endif      
                if (a <= 2)
                    return; //no violation whatsoever
            
                //Log.WritePlayer (player, "Speed " + a + " seconds");
                //for (int n = 0; n < HistorySpeed.Length; n++) {
                //  Log.WritePlayer (player, HistorySpeed [n].ToString ("0.00") + " / " + HistoryMax [n].ToString ("0.00"));
#if xDEBUG
                    Console.WriteLine(HistorySpeed [n].ToString ("0.00") + " / " + HistoryMax [n].ToString ("0.00"));
#endif      
                //}

                if (a < HistorySpeed.Length - 3)
                    return; //no visual speed warning
                
                player.Player.TellAbove(Chat.Aqua, "SLOW DOWN, speed mods are banned!");
                Chatting.Parser.TellAdmin(Permissions.Ban, Chat.Aqua + player.Player.MinecraftUsername + " " + a + " s");
                Chatting.Parser.TellAdmin(Permissions.Ban, Chat.Aqua + HistorySpeed [0].ToString("0.00") + " / " + HistoryMax [0].ToString("0.00"));
                
                if (a >= HistorySpeed.Length)
                {
#if DEBUG
                    player.TellSystem(Chat.Red, "Speed Ban triggered" + Chat.Yellow + " - only testing");
#else
                    player.Player.BanByServer(DateTime.Now.AddMinutes(30), "Moving too fast(modified client)");
#endif
                }
            }
        }
    }
}

