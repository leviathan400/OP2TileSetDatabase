# OP2TileSetDatabase

![Screenshot](https://images.outpostuniverse.org/OP2TileSetDatabase.png)

## What is it?

Outpost 2: Divided Destiny tile adjacency database built through map analysis. It also generates a report from each map that it analysis.

There are 63 maps in \OP2-1.4.1\OPU\base\maps.

## op2_tileset_database.json

The `op2_tileset_database.json` file contains a comprehensive database of tile adjacency relationships extracted from Outpost 2 map analysis (from the JSON map format). This database records how frequently different tiles appear next to each other across all analyzed maps.

## Information

- **Tile IDs**: Outpost 2 uses tile IDs from 0 to 2011
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
        // ... more tile relationships
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
