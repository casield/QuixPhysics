using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    public class CommandReader
    {
        private ArrayList commandsList = new ArrayList();

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
            commandDictionary.Add("OVar", new OVarCommand(simulator));
            commandDictionary.Add("close", new CloseCommand(simulator));
        }
        internal void AddCommandToBeRead(string v)
        {
            commandsList.Add(v);
        }

        internal Room GetRoom(string roomId)
        {
            var contains = simulator.roomManager.rooms.ContainsKey(roomId);
            Room room = null;

            if (contains)
            {
                room = simulator.roomManager.rooms[roomId];
            }
            else
            {
                var newroom = new Room(simulator, new RoomInfo { maxPlayers = 100, position = new Vector3(), roomId = roomId });
                simulator.roomManager.AddRoom(newroom);
                room = newroom;
            }
            return room;
        }

        internal void ReadCommand()
        {
            try
            {
                ArrayList newC = commandsList;
                for (int a = 0; a < commandsList.Count; a++)
                {
                    var item = commandsList[a];
                    JsonSerializerSettings setting = new JsonSerializerSettings();
                    setting.CheckAdditionalContent = false;
                    if (item != null)
                    {
                        Newtonsoft.Json.Linq.JObject message = JsonConvert.DeserializeObject<JObject>((string)item, setting);
                        string type = (string)message["type"];
                        if (commandDictionary.ContainsKey(type))
                        {
                            JObject data = (JObject)message["data"];
                            var room = GetRoom(((string)data["roomId"]));

                            commandDictionary[type].OnRead(data,room);
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
                QuixConsole.Log("Collection was modifieded", e);
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
