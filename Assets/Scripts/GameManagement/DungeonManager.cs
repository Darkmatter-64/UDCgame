using benjohnson;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class DungeonManager : Singleton<DungeonManager>
{
    public GenerationSettingsSO gen;

    public Room CurrentRoom { get { return currentRoom; } }
    Room currentRoom;
    Room bossRoom;
    List<Room> rooms;

    // Generation variables
    List<EC_Door> doorsToFill; // List of doors with no destination yet, generate a room for these doors
    bool portalGenerated = false;

    [Header("Components")]
    [SerializeField] DungeonMapRenderer mapRenderer;
    [HideInInspector] public ArrangeGrid gridLayout;

    protected override void Awake()
    {
        base.Awake();

        // Initialize variables
        rooms = new List<Room>();
        doorsToFill = new List<EC_Door>();

        // Assign components
        gridLayout = GetComponent<ArrangeGrid>();
    }

    void Start()
    {
        GenerateDungeon(GameManager.instance.stage);
    }

    public void GenerateDungeon(int stage)
    {
        if (stage > gen.rooms.Count - 1)
        {
            GameManager.instance.LoadWinScreen();
            stage = gen.rooms.Count;
            return;
        }

        Tree tree = new Tree();

        // Generate first room
        CreateRoom(stage, 0, null, tree.root);
        // Iteratively generate layers
        GenerateLayer(stage, 1, tree);
        // Generate boss room
        GenerateBossRoom(stage);

        // Enter first room
        SwitchRoom(rooms[0]);

        GameManager.instance.DungeonLoaded();
    }

    /// <summary>
    /// Generates boss door at deepest point in dungeon, generates boss room behind boss door,
    /// generates boss key at next lowest point excluding boss door room and boss door room parent
    /// to avoid having the key generate near boss door
    /// </summary>
    void GenerateBossRoom(int _stage)
    {
        // Find deepest rooms
        int _depth = 0;
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].depth > _depth)
                _depth = rooms[i].depth;
        }
        // Randomly determine boss room
        List<Room> _bossDoorRooms = RoomsAtDepth(_depth, null);
        Room _bossDoorRoom = _bossDoorRooms[Random.Range(0, _bossDoorRooms.Count)];
        // Spawn boss door
        EC_Entity _bossDoor = SpawnEntity(gen.bossDoorPrefab, _bossDoorRoom);
        _bossDoor.GetComponent<EC_Door>().SetLocked(true);
        // Create boss room
        bossRoom = CreateBossRoom(_stage, _depth + 1, _bossDoorRoom);
        bossRoom.parentRoom.children.Add(bossRoom);
        _bossDoor.GetComponent<EC_Door>().destination = bossRoom;

        // Randomly determine boss key room
        List<Room> _keyRooms = RoomsAtDepth(_depth, new List<Room>() { _bossDoorRoom, _bossDoorRoom.parentRoom });
        Room _keyRoom = _keyRooms[Random.Range(0, _keyRooms.Count)];
        EC_Entity _bossKey = SpawnEntity(gen.bossKeyPrefab, _keyRoom);
        _bossKey.GetComponent<EC_StageKey>().keyDoor = _bossDoor.GetComponent<EC_Door>();
    }

    /// <summary>
    /// Generates rooms for each door with no destination
    /// Iteratively generates dungeon layers by calling GenerateLayer(depth + 1)
    /// Stops when maxDepth is reached or no doors are left to be filled
    /// </summary>
    void GenerateLayer(int stage, int depth, Tree tree)
    {
        // Stop when reached max depth or no doors to fill
        if (depth > gen.maxDepth || doorsToFill.Count <= 0) return;

        // Copy doors list and clear old list
        List<EC_Door> _doorsToFill = new List<EC_Door>();
        for (int i = 0; i < doorsToFill.Count; i++)
            _doorsToFill.Add(doorsToFill[i]);
        doorsToFill.Clear();

        List<Node> _nodes = tree.GetLayer(depth);

        // Create new room for all empty doors
        for (int i = 0; i < _doorsToFill.Count; i++)
        {
            Room _parentRoom = _doorsToFill[i].GetComponent<EC_Entity>().room;
            Room _newRoom = CreateRoom(stage, depth, _parentRoom, _nodes[i]);
            _doorsToFill[i].destination = _newRoom;
            _parentRoom.children.Add(_newRoom);
        }

        // Generate next layer
        GenerateLayer(stage, depth + 1, tree);
    }

    /// <summary>
    /// Handles logic for safely exiting current room, and entering next room
    /// </summary>
    public void SwitchRoom(Room nextRoom)
    {
        // Check if room exists
        if (nextRoom == null) return;

        // Exit current room
        currentRoom?.ExitRoom();

        // Enter next room
        currentRoom = nextRoom;
        currentRoom.EnterRoom();

        // Update grid
        gridLayout.Arrange(true);

        // Display
        mapRenderer.DisplayMap(rooms, currentRoom);

        //TESTPORTAL();
    }

    /// <summary>
    /// Creates room at depth, fills with doors and entities, returns newly created room
    /// </summary>
    Room CreateRoom(int _stage, int _depth, Room _parentRoom, Node node)
    {
        Room _room = new Room(_depth, _parentRoom);
        rooms.Add(_room);

        // If not starting room, spawn back door
        if (_depth > 0)
        {
            EC_Entity _back = SpawnEntity(gen.backDoorPrefab, _room);
            _back.GetComponent<EC_Door>().destination = _parentRoom;
        }

        int doorsCount = 0;
        if (node.left != null) { doorsCount++; }
        if (node.right != null) { doorsCount++; }

        // Spawn doors according to depth
        for (int i = 0; i < doorsCount; i++)
        {
            EC_Entity _door = SpawnEntity(gen.doorPrefab, _room);
            doorsToFill.Add(_door.GetComponent<EC_Door>());
            //if  (Random.Range(0.0f, 1.0f) <= 1 - Mathf.Pow((float)_depth / (float)gen.maxDepth, gen.oddsPower))
            //{
            //    EC_Entity _door = SpawnEntity(gen.doorPrefab, _room);
            //    doorsToFill.Add(_door.GetComponent<EC_Door>());
            //}
        }
        // Add entities based on room depth
        if (_depth > 0)
        {
            List<GameObject> entities = gen.rooms[_stage].RandomRoom(_depth - 1).entities;
            for (int i = 0; i < entities.Count; i++)
                SpawnEntity(entities[i].gameObject, _room);
        }

        return _room;
    }

    /// <summary>
    /// Creates boss room
    /// </summary>
    Room CreateBossRoom(int _stage, int _depth, Room _parentRoom)
    {
        Room _room = new Room(_depth, _parentRoom, true);
        rooms.Add(_room);

        // Back door
        EC_Entity _back = SpawnEntity(gen.backDoorPrefab, _room);
        _back.GetComponent<EC_Door>().destination = _parentRoom;
        // Spawn boss room entities
        List<GameObject> _entities = gen.rooms[_stage].RandomBossRoom().entities;
        for (int i = 0; i < _entities.Count; i++)
            SpawnEntity(_entities[i].gameObject, _room);

        return _room;
    }

    /// <summary>
    /// Returns list of rooms at desired depth excluding list of excluded rooms, if no valid rooms exist search previous depth
    /// </summary>
    List<Room> RoomsAtDepth(int _depth, List<Room> _excluded)
    {
        // Invalid or zero depth reached, return starting room as default
        if (_depth <= 0)
            return new List<Room>() { rooms[0] };

        // Excluded list null error
        if (_excluded == null)
            _excluded = new List<Room>();

        // Get rooms at depth
        List<Room> _rooms = new List<Room>();
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].depth == _depth && !_excluded.Contains(rooms[i]))
                _rooms.Add(rooms[i]);
        }

        // No valid rooms found, search previous depth
        if (_rooms.Count <= 0)
            _rooms = RoomsAtDepth(_depth - 1, _excluded);

        return _rooms;
    }

    /// <summary>
    /// Spawns entity gameobject to scene given EC_Entity type prefab and room to spawn into, returns new created entity
    /// </summary>
    public EC_Entity SpawnEntity(GameObject entityToSpawn, Room room)
    {
        EC_Entity _entity = Instantiate(entityToSpawn, transform).GetComponent<EC_Entity>();
        room.roomEntities.Add(_entity);
        _entity.IsEnabled(false);
        _entity.room = room;
        return _entity;
    }

    public void SpawnPortal()
    {
        if (portalGenerated) return;

        EC_Entity portal = SpawnEntity(gen.portalPrefab, bossRoom);
        portal.IsEnabled(true);
        portalGenerated = true;
        gridLayout.Arrange();

        ArtifactManager.instance.TriggerBossDefeated();
    }

    public void TESTPORTAL()
    {
        if (portalGenerated) return;

        EC_Entity portal = SpawnEntity(gen.portalPrefab, rooms[0]);
        portal.IsEnabled(true);
        portalGenerated = true;
        gridLayout.Arrange();

        Player.instance.Wallet.AddMoney(200);
    }
}




class Node
{
    public int depth;
    public Node parent;
    public Node left;
    public Node right;

    public Node(int _depth)
    {
        depth = _depth;
        parent = null;
        left = null;
        right = null;
    }
}

class Tree
{
    public Node root;
    int maxDepth = 3;
    int oddsPower = 1;

    public Tree()
    {
        root = new Node(0);
        genTree(root);
    }

    public void genTree(Node node)
    {
        int _depth = node.depth;
        if (_depth > 3) { return; }
        for (int i = 0; i < 2; i++)
        {
            if (Random.Range(0.0f, 1.0f) <= 1 - Mathf.Pow((float)_depth / (float)maxDepth, oddsPower))
            {
                if (i == 0)
                {
                    node.left = new Node(_depth + 1);
                    node.left.parent = node;
                    genTree(node.left);
                }
                else
                {
                    node.right = new Node(_depth + 1);
                    node.right.parent = node;
                    genTree(node.right);
                }
            }
        }
        return;
    }

    public List<Node> GetLayer(int depth)
    {
        List<Node> layer = new List<Node>();
        if (depth == 0)
        {
            layer.Add(root);
            return layer;
        }

        if (depth == 1)
        {
            if (root.left != null)
            {
                layer.Add(root.left);
            }
            if (root.right != null)
            {
                layer.Add(root.right);
            }
            return layer;
        }

        // depth > 1
        if (root.left != null)
        {
            layer.AddRange(GetLayer(depth, root.left));
        }
        if (root.right != null)
        {
            layer.AddRange(GetLayer(depth, root.right));
        }


        return layer;
    }

    public List<Node> GetLayer(int depth, Node node)
    {
        List<Node> layer = new List<Node>();
        if (depth - 1 == node.depth)
        {
            if (node.left != null)
            {
                layer.Add(node.left);
            }
            if (node.right != null)
            {
                layer.Add(node.right);
            }

            return layer;
        }

        if (node.left != null)
        {
            layer.AddRange(GetLayer(depth, node.left));
        }
        if (node.right != null)
        {
            layer.AddRange(GetLayer(depth, node.right));
        }



        return layer;
    }
}