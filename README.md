HOW TO USE THE GENERATOR
--------------------------------------
Press play to generate a dungeon layout.
There are two outputs, one in the game and one in inspector.
The output in the game is the visual representation of the dungeon while the output in the inspector is giving the information of each room of the dungeon.

LIMITATIONS
 - If your grid is too small, the dungeon will not generate
 - If min rooms is >10 and branching is 0.1-0.3, the dungeon will either take a very long time to load or won't load at all
 - looping is extrememly buggy and sometimes doesn't even listen to the parameters the user gives.
 - Loops can only be 2x2 groups of rooms and cannot create long loops like other dungeon can.

In unity click the dungeongenerator in Hierarchy to edit settings.
I added a basic cell prefab to create the dungeon. You can replace it with your own prefab if you want.

GENERATED DUNGEON DATA
This gives data on every room that is generated:
 - Index
 - X Position
 - Y position
 - Room Type
 - Neighbors
   - up, down, left, right

DUNGEON GRID
Cell W and Cell H is the spacing between the cells with W being the sides and H being the top and bottom. This is just visuals.
Num columns and rows can be changed to alter the space the dungeon ccan generate in. If it is a small space and the dungeon tryies to load outside the bounds the dungeon will fail. So keep it big.

DUNGEON SIZE AND SPAWN
Max and min rooms is self explainitory. It is the max and min number of rooms the dungeon can generate. This is not including the starter room so if max is set to 15, it ccan generate 16.
Starter room index is the place where the starter room will spawn. This is the index starting at 0 from the bottom left. If the grid is a 16/12, the middle of the grid will be at index 104. The index is coutning the intersections of the grid not the box itself.

DUNGEON BRANCHING
Starter room branches is how many neighbors the starter room will have from 1-4. This can be helpful if you want to either make the starter room the start of the dungeon(1) or a main hub of sorts(4). (If there are loops, then the starter room branches will be inaccurate)
Branching is the probability a room in the dungeon will generate a branch from 0-1. If the branching is at 0.1-0.3, keep the min rooms under 10. If it is higher, the dungeon may not load.

DUNGEON LOOPING
Min and max loops is very buggy. Just keep it at either 0-3 to add some loops to the dungeon and make them the same number because it doesn't really follow these variables. Loops are pretty much a 2x2 of rooms.

ROOM TYPES
This is the number of room types your dungeon could be. Each room is assigned a number from 0-(Roomtypes - 1). These can probably be used to assign each room to something like a prefab or scene. The starter room has the room type of -1.

DUNGEON VISIBILITY
Dungeon Visibility can be used to toggle the visibility of the dungeon. If you have it off, then when you press play the dungeon wont be visible but will still be generated. You can press "v" to turn it on and off while in play mode.
Grid Visibility is only for the editor. This toggles the grid of the dungeon on and off for the user. The grid is only shown in the scene editor.
