using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    public struct CommandToBeRead
    {
        public string command;
        public ConnectionState connectionState;
    }
    public class CommandReader
    {
        private List<CommandToBeRead> commandsList = new List<CommandToBeRead>();

        private Dictionary<string, Command> commandDictionary = new Dictionary<string, Command>();
        private Simulator simulator;

        public CommandReader(Simulator _simulator)
        {
            simulator = _simulator;
            commandDictionary.Add("create", new CreateCommand(simulator));
            commandDictionary.Add("join", new JoinCommand(simulator));
            commandDictionary.Add("generateMap", new GenerateMapCommand(simulator));
            commandDictionary.Add("move", new MoveCommand(simulator));
            commandDictionary.Add("shoot", new ShootCommand(simulator));
            commandDictionary.Add("rotate", new RotateCommand(simulator));
            commandDictionary.Add("jump", new JumpCommand(simulator));
            commandDictionary.Add("createBoxes", new CreateBoxesCommand(simulator));
            commandDictionary.Add("gauntlet", new GauntletCommand(simulator));
            commandDictionary.Add("swipe", new SwipeCommand(simulator));
            commandDictionary.Add("objectMessage", new ObjectMessageCommand(simulator));

            commandDictionary.Add("OVar", new OVarCommand(simulator));
            commandDictionary.Add("close", new CloseCommand(simulator));
        }
        internal void AddCommandToBeRead(string v, ConnectionState state)
        {
            commandsList.Add(new CommandToBeRead() { command = v, connectionState = state });
        }

        internal Room GetRoom(string roomId)
        {
            var contains = simulator.roomManager.rooms.ContainsKey(roomId);
            Room room = null;

            if (contains)
            {
                room = simulator.roomManager.rooms[roomId];
            }
            return room;
        }

        internal void ReadCommand()
        {
            try
            {

                for (int a = 0; a < commandsList.Count; a++)
                {
                    var item = commandsList[a];
                    JsonSerializerSettings setting = new JsonSerializerSettings();
                    setting.CheckAdditionalContent = false;
                    if (item.command != null && item.connectionState != null)
                    {
                        Newtonsoft.Json.Linq.JObject message = JsonConvert.DeserializeObject<JObject>((string)item.command, setting);
                        string type = (string)message["type"];
                        if (commandDictionary.ContainsKey(type))
                        {
                            JObject data = (JObject)message["data"];
                            var room = GetRoom(((string)data["roomId"]));
                            if (room == null)
                            {
                                room = simulator.roomManager.NewRoom(item.connectionState, (string)data["roomId"]);
                            }
                            Command command =commandDictionary[type];
                            command.OnRead(data, room);
                        }
                        else
                        {
                            QuixConsole.Log("Command not registred " + type);
                        }

                    }
                }

                commandsList.Clear();
            }
            catch (InvalidOperationException e)
            {
                QuixConsole.Log("Collection was modified", e);
            }
            catch (JsonReaderException e)
            {
                QuixConsole.Log("Json Problem ", e);
            }
            catch (InvalidCastException e)
            {
                QuixConsole.Log("Invalid cast", e);
                // commandsList.Clear();
            }
            catch (Exception e)
            {
                QuixConsole.WriteLine(e);
            }


        }
    }
}
