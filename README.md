# OP2TileSetDatabase

![Screenshot](https://images.outpostuniverse.org/OP2TileSetDatabase.png)

## What is it?

Outpost 2: Divided Destiny tile adjacency database built through map analysis. It also generates a report from each map that it analyzed. The higher the occurrence count, the more frequently those tiles appear adjacent to each other in maps analyzed, indicating stronger natural relationships between tiles. The tool is designed to be able to analyze maps from any tileset.

There are 63 maps in \OP2-1.4.1\OPU\base\maps.

Saves the adjacency database to a file for use with other tools, in a json file format.

Inspired by the [Op2TileDB](https://github.com/Sirbomber/Op2TileDB) project.

## op2_tileset_database.json

The `op2_tileset_database.json` file contains a database of tile adjacency relationships extracted from map analysis (from the JSON map format). This database records how frequently different tiles appear next to each other across all analyzed maps.

## Information

- **Tile IDs**: Outpost 2 JSON maps use tile IDs from 0 to 2011 (default tile mappings)
- **Directions**: Records adjacencies in all 8 compass directions
- **Occurrence Counts**: Numbers represent how many times each tile pair was found adjacent across all analyzed maps
- **Coverage**: Built from analysis of multiple Outpost 2 map files to ensure statistical relevance

## Structure

The JSON file contains an array of tile adjacency records, where each record represents:

```json
{
  "TileId": 17,                    // The source tile ID (0-2011)
  "AdjacencyInfo": {
    "AdjacencyByDirection": {
      "North": {                   // Adjacent tiles found to the North
        "3": 166,                  // Tile ID 3 appears 166 times north of tile 17
        "4": 208,                  // Tile ID 4 appears 208 times north of tile 17
        "6": 140,                  // And so on...
      },
      "Northeast": { /* ... */ },  // 8 directions total
      "East": { /* ... */ },
      "Southeast": { /* ... */ },
      "South": { /* ... */ },
      "Southwest": { /* ... */ },
      "West": { /* ... */ },
      "Northwest": { /* ... */ }
    }
  }
}
```

## Use Cases

This database enables:
- **Procedural Map Generation**: Understanding which tiles naturally appear together
- **Map Validation**: Checking if tile placements follow typical patterns
- **Terrain Analysis**: Statistical insights into Outpost 2's map design
- **Pattern Recognition**: Identifying common terrain formations and transitions
