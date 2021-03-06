﻿/**
 * @file: chat.cs
 * @author: Facepunch Studios
 * @modified: Team Cerionn (https://github.com/Team-Cerionn)

 * @description: chat class for Rust Essentials.cs
 */
using Facepunch.Utility;
using RustEssentials.Util;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System;

public class chat : ConsoleSystem
{
    [ConsoleSystem.Help("Enable or disable chat displaying", "")]
    [ConsoleSystem.Client]
    [ConsoleSystem.Admin]
    public static bool enabled = true;
    [ConsoleSystem.Admin]
    public static bool serverlog = true;

    static chat()
    {
    }

    [ConsoleSystem.User]
    public static void say(Arg arg)
    {
        if (!enabled)
            return;

        string playerName = arg.argUser.user.Displayname;
        string clientName = arg.argUser.playerClient.userName;
        string UID = arg.argUser.user.Userid.ToString();
        string message = arg.GetString(0, "text");


        if (playerName != null && message.Length > 0)
        {
            if (message.StartsWith("/"))
            {
                Vars.conLog.Chat("<CMD> " + playerName + ": " + message);
                if (serverlog)
                {
                    Debug.Log("[CHAT] <CMD> " + playerName + ": " + message);
                }
                //Thread t = new Thread(() => Commands.CMD(arg));
                //t.Start();
                Commands.CMD(arg);
            }
            else
            {
                playerName = Vars.filterNames(playerName, UID);
                message = message.Replace("\"", "\\\"").Replace("[PM]", "").Replace("[PM to]", "").Replace("[PM from]", "").Replace("[PM From]", "").Replace("[PM To]", "").Replace("[F]", "");
                if (Vars.censorship)
                {
                    List<string> splitMessage = new List<string>(message.Split(' '));
                    foreach (string s in splitMessage)
                    {
                        if (Vars.illegalWords.Contains(s.ToLower().Replace(".", "").Replace("!", "").Replace(",", "").Replace("?", "").Replace(";", "")))
                        {
                            string curseWord = Array.Find(Vars.illegalWords.ToArray(), (string str) => str.Equals(s.ToLower().Replace(".", "").Replace("!", "").Replace(",", "").Replace("?", "").Replace(";", "")));
                            string asterisks = "";
                            for (int i = 0; i < s.Replace(".", "").Replace("!", "").Replace(",", "").Replace("?", "").Replace(";", "").Length; i++)
                            {
                                asterisks += "*";
                            }
                            string theRest = s.Replace(curseWord, "");
                            string fullString = (s.StartsWith(theRest) ? theRest + asterisks : asterisks + theRest);
                            splitMessage[splitMessage.IndexOf(s)] = fullString;
                        }
                    }
                    message = string.Join(" ", splitMessage.ToArray());
                }

                if (!Vars.inDirect.Contains(UID) && !Vars.inGlobal.Contains(UID) && !Vars.inFaction.Contains(UID))
                {
                    Vars.inGlobal.Add(UID);
                }
                if (!Vars.inDirectV.Contains(UID) && !Vars.inGlobalV.Contains(UID))
                {
                    Vars.inDirectV.Add(UID);
                }
                if (clientName.Length == 0)
                {
                    Broadcast.broadcastTo(arg.argUser.networkPlayer, "You cannot chat while vanished!");
                    return;
                }
                if (Vars.inDirect.Contains(UID))
                {
                    if (Vars.directChat)
                    {
                        Thread t = new Thread(() => Vars.sendToSurrounding(arg.argUser.playerClient, message));
                        t.Start();
                        Vars.conLog.Chat("<D> " + playerName + ": " + message);
                        if (serverlog)
                        {
                            Debug.Log("[CHAT] <D> " + playerName + ": " + message);
                        }
                        return;
                    }
                    else
                    {
                        Vars.inGlobal.Add(UID);
                        Vars.inDirect.Remove(UID);

                        if (!Vars.mutedUsers.Contains(UID))
                        {
                            Broadcast.broadcastTo(arg.argUser.networkPlayer, "Direct chat has been disabled! You are now talking in global chat.");
                            ConsoleNetworker.Broadcast("chat.add \"" + (Vars.removeTag ? "" : "<G> ") + playerName + "\" \"" + message + "\"");
                            Vars.conLog.Chat("<G> " + playerName + ": " + message);
                            if (serverlog)
                            {
                                Debug.Log("[CHAT] <G> " + playerName + ": " + message);
                            }
                            if (Vars.historyGlobal.Count > 50)
                                Vars.historyGlobal.RemoveAt(0);
                            Vars.historyGlobal.Add("* " + (Vars.removeTag ? "" : "<G> ") + playerName + "$:|:$" + message);
                            return;
                        }
                        else
                        {
                            if (Vars.muteTimes.ContainsKey(UID))
                            {
                                string secondsLeft = "You have been muted for " + Math.Round(Vars.muteTimes[UID].TimeLeft / 1000).ToString() + " seconds on global chat.";
                                Broadcast.broadcastTo(arg.argUser.networkPlayer, secondsLeft);
                            }
                            else
                                Broadcast.broadcastTo(arg.argUser.networkPlayer, "You have been muted on global chat.");
                            return;
                        }
                    }
                }
                if (Vars.inGlobal.Contains(UID))
                {
                    if (Vars.globalChat)
                    {
                        if (!Vars.mutedUsers.Contains(UID))
                        {
                            ConsoleNetworker.Broadcast("chat.add \"" + (Vars.removeTag ? "" : "<G> ") + playerName + "\" \"" + message + "\"");
                            Vars.conLog.Chat("<G> " + playerName + ": " + message);
                            if (serverlog)
                            {
                                Debug.Log("[CHAT] <G> " + playerName + ": " + message);
                            }
                            if (Vars.historyGlobal.Count > 50)
                                Vars.historyGlobal.RemoveAt(0);
                            Vars.historyGlobal.Add("* " + (Vars.removeTag ? "" : "<G> ") + playerName + "$:|:$" + message);
                            return;
                        }   
                        else
                        {
                            if (Vars.muteTimes.ContainsKey(UID))
                            {
                                string secondsLeft = "You have been muted for " + Math.Round(Vars.muteTimes[UID].TimeLeft / 1000).ToString() + " seconds on global chat.";
                                Broadcast.broadcastTo(arg.argUser.networkPlayer, secondsLeft);
                            }
                            else
                                Broadcast.broadcastTo(arg.argUser.networkPlayer, "You have been muted on global chat.");
                            return;
                        }
                    }
                    else
                    {
                        Vars.inDirect.Add(UID);
                        Vars.inGlobal.Remove(UID);

                        Broadcast.broadcastTo(arg.argUser.networkPlayer, "Global chat has been disabled! You are now talking in direct chat.");


                        Thread t = new Thread(() => Vars.sendToSurrounding(arg.argUser.playerClient, message));
                        t.Start();
                        Vars.conLog.Chat("<D> " + playerName + ": " + message);
                        if (serverlog)
                        {
                            Debug.Log("[CHAT] <D> " + playerName + ": " + message);
                        }
                        return;
                    }
                }
                if (Vars.inFaction.Contains(UID))
                {
                    KeyValuePair<string, Dictionary<string, string>>[] possibleFactions = Array.FindAll(Vars.factions.ToArray(), (KeyValuePair<string, Dictionary<string, string>> kv) => kv.Value.ContainsKey(arg.argUser.userID.ToString()));

                    if (possibleFactions.Length > 0)
                    {
                        Vars.sendToFaction(arg.argUser.playerClient, message);
                        Vars.conLog.Chat("<F [" + possibleFactions[0].Key + "]> " + playerName + ": " + message);
                        if (serverlog)
                        {
                            Debug.Log("[CHAT] <F [" + possibleFactions[0].Key + "]> " + playerName + ": " + message);
                        }
                        if (Vars.historyFaction.Count > 50)
                            Vars.historyFaction.RemoveAt(0);
                        if (!Vars.historyFaction.Contains(possibleFactions[0].Key))
                            Vars.historyFaction.Add(possibleFactions[0].Key, new List<string>() { { "* <F> " + playerName + "$:|:$" + message } });
                        else
                            ((List<string>)Vars.historyFaction[possibleFactions[0].Key]).Add("* <F> " + playerName + "$:|:$" + message);
                        return;
                    }
                    else
                    {
                        if (Vars.globalChat)
                            Vars.inGlobal.Add(UID);
                        else
                            Vars.inDirect.Add(UID);

                        Vars.inFaction.Remove(UID);

                        Broadcast.broadcastTo(arg.argUser.networkPlayer, "You are not in a faction! You are now talking in global chat.");
                        if (!Vars.mutedUsers.Contains(UID))
                        {
                            ConsoleNetworker.Broadcast("chat.add \"" + (Vars.removeTag ? "" : "<G> ") + playerName + "\" \"" + message + "\"");
                            Vars.conLog.Chat("<G> " + playerName + ": " + message);
                            if (serverlog)
                            {
                                Debug.Log("[CHAT] <G> " + playerName + ": " + message);
                            }
                            if (Vars.historyGlobal.Count > 50)
                                Vars.historyGlobal.RemoveAt(0);
                            Vars.historyGlobal.Add("* " + (Vars.removeTag ? "" : "<G> ") + playerName + "$:|:$" + message);
                        }
                        else
                        {
                            if (Vars.muteTimes.ContainsKey(UID))
                            {
                                string secondsLeft = "You have been muted for " + Math.Round(Vars.muteTimes[UID].TimeLeft / 1000).ToString() + " seconds on global chat.";
                                Broadcast.broadcastTo(arg.argUser.networkPlayer, secondsLeft);
                            }
                            else
                                Broadcast.broadcastTo(arg.argUser.networkPlayer, "You have been muted on global chat.");
                        }
                        return;
                    }
                }
            }
        }
    }
}
