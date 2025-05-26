Imports System.IO
Imports System.Media
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq


' OP2TileSetDatabase
' https://github.com/leviathan400/OP2TileSetDatabase
'
' Outpost 2 tile adjacency database built through map analysis.
'
'
' Outpost 2: Divided Destiny is a real-time strategy video game released in 1997.

Public Class fMain

#Region "Private Fields"

    ' TODO:       Divide the map up into 2x2 blocks and do block adjacency analysis

    Private ApplicationName As String = "OP2TileSetDatabase"
    Private Version As String = "0.3.0"
    Private Build As String = "0020"

    Private DatabasePath As String = "D:\op2_tileset_database.json"
    Private DatabaseReportPath As String = "D:\op2_tileset_database_report.txt"

    Private DatabaseBlocksPath As String = "D:\op2_blocks_database.json"
    Private DatabaseBlocksReportPath As String = "D:\op2_blocks_database_report.txt"

    Private MapsPath_JSON As String = "D:\op2mapdatabase"   ' Folder containing all map JSON files

    Private MapCount As Int16

    Private tileDatabase As TileAdjacencyDatabase           ' Database to store all data on multiple map files 
    Private singleMapDatabase As TileAdjacencyDatabase      ' Temp data base used when looking at one map file
    Private blockDatabase As BlockAdjacencyDatabase         ' Database to store all data on blocks (2x2)

#End Region

#Region "Form Events"

    ''' <summary>
    ''' Initializes the form and sets up the user interface
    ''' </summary>
    Private Sub fMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Debug.WriteLine("--- " & ApplicationName & " Started ---")
        Me.Icon = My.Resources.intelligence
        Me.Text = "Tileset Adjacency Database"
        txtConsole.ReadOnly = True
        txtConsole.BackColor = Color.White

        ' Initialize the databases
        tileDatabase = New TileAdjacencyDatabase()
        blockDatabase = New BlockAdjacencyDatabase()

        AppendToConsole("Maps (JSON) Path: " & MapsPath_JSON)
        AppendToConsole("Tile Adjacency Database Path: " & DatabasePath)

        Dim player As New SoundPlayer(My.Resources.gar_sel)
        player.Play()

        ' Load the current database
        'LoadDatabase(DatabasePath)
        ' Display database statistics
        'DisplayDatabaseSummary()
    End Sub

    ''' <summary>
    ''' Processes a single JSON map file for testing purposes
    ''' </summary>
    Private Sub btnProcessSingle_Click(sender As Object, e As EventArgs) Handles btnProcessSingle.Click
        ' Process a single JSON map
        txtConsole.Clear()

        OpenOutpost2Map_JSON("D:\mapdatabase\mp6_03.json")

        MapCount = 1
    End Sub

    ''' <summary>
    ''' Processes all JSON map files in the specified directory and generates the complete database
    ''' </summary>
    Private Sub btnProcess_Click(sender As Object, e As EventArgs) Handles btnProcess.Click
        ' Process a folder of JSON maps
        txtConsole.Clear()

        btnProcess.Enabled = False

        ProcessOutpost2Maps_JSON(MapsPath_JSON)

        AppendToConsole(vbNewLine)

        ExportDatabase(DatabasePath)

        GenerateDatabaseReport(DatabasePath, DatabaseReportPath)

        btnProcess.Enabled = True
    End Sub

    ''' <summary>
    ''' Displays database statistics and generates comprehensive reports
    ''' </summary>
    Private Sub btnView_Click(sender As Object, e As EventArgs) Handles btnView.Click
        ' Save the databases from memory to disk

        AppendToConsole("")

        ' Display the database contents - Displays a hell of lot of lines!
        'DisplayDatabaseContents()

        ' Display database summary
        DisplayDatabaseSummary()

        'DisplayMostCommonAdjacencies(10)

        'DisplayAdjacencyStatsByDirection()

        'DisplayTileUsageStatistics()

        ' View a specific tile
        'DisplaySpecificTile(9)


        ' Export the database after analysis
        ExportDatabase(DatabasePath)

        ' Generate a report
        GenerateDatabaseReport(DatabasePath, DatabaseReportPath)


        AppendToConsole("")
        ' Block 2x2 database...
        DisplayBlockDatabaseSummary()
        ExportBlockDatabase(DatabaseBlocksPath)
        GenerateBlockDatabaseReport(DatabaseBlocksPath, DatabaseBlocksReportPath)

        AppendToConsole("")
        AppendToConsole("Complete!")
    End Sub

#End Region

#Region "Console Output"

    ''' <summary>
    ''' Appends text to the console output with automatic scrolling
    ''' </summary>
    ''' <param name="TextToLog">The text to append to the console</param>
    Public Sub AppendToConsole(TextToLog As String)
        ' Append text to the console with a newline
        txtConsole.AppendText(TextToLog & vbCrLf)

        ' Scroll to the end of the text
        txtConsole.SelectionStart = txtConsole.Text.Length
        txtConsole.ScrollToCaret()
    End Sub

#End Region

#Region "Database Operations"

    ''' <summary>
    ''' Loads the tile adjacency database from a JSON file on disk
    ''' </summary>
    ''' <param name="DatabasePath">The file path to the database JSON file</param>
    Private Sub LoadDatabase(ByVal DatabasePath As String)
        ' Load the tile adjacency database from disk
        Try
            AppendToConsole("Loading database from disk...")
            AppendToConsole($"Path: {DatabasePath}")

            ' Check if file exists
            If Not File.Exists(DatabasePath) Then
                AppendToConsole("Error: Database file does not exist")
                Return
            End If

            ' Get file info
            Dim fileInfo As New FileInfo(DatabasePath)
            AppendToConsole($"File size: {fileInfo.Length} bytes")

            ' Load the database
            tileDatabase.LoadFromFile(DatabasePath)

            AppendToConsole("Database loaded successfully")

        Catch ex As Exception
            AppendToConsole($"Error loading database: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' Exports the current tile database to a JSON file
    ''' </summary>
    ''' <param name="filePath">The file path where the database will be saved</param>
    Private Sub ExportDatabase(ByVal filePath As String)
        ' Export the database to a JSON file
        Try
            tileDatabase.SaveToFile(filePath)       ' Save the database

            AppendToConsole($"Database exported successfully to: {filePath}")
            AppendToConsole($"File size: {New System.IO.FileInfo(filePath).Length} bytes")

        Catch ex As Exception
            AppendToConsole($"Error exporting database: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' Exports the current block database to a JSON file
    ''' </summary>
    ''' <param name="filePath">The file path where the block database will be saved</param>
    Private Sub ExportBlockDatabase(ByVal filePath As String)
        'Export the block database to a JSON file

        Try
            blockDatabase.SaveToFile(filePath)      ' Save the database

            AppendToConsole($"Block database exported successfully to: {filePath}")
            AppendToConsole($"File size: {New System.IO.FileInfo(filePath).Length} bytes")

        Catch ex As Exception
            AppendToConsole($"Error exporting block database: {ex.Message}")
        End Try
    End Sub

#End Region

#Region "Map Processing"

    ''' <summary>
    ''' Processes all JSON map files in the specified directory
    ''' </summary>
    ''' <param name="MapsFolderPath">The folder path containing JSON map files</param>
    Private Sub ProcessOutpost2Maps_JSON(ByVal MapsFolderPath As String)
        ' Find all JSON maps and process them
        Try
            AppendToConsole($"Searching for JSON maps in: {MapsFolderPath}")

            ' Get all JSON files from the directory
            Dim jsonFiles() As String = Directory.GetFiles(MapsFolderPath, "*.json")

            AppendToConsole($"Found {jsonFiles.Length} JSON file(s)")
            AppendToConsole("")

            ' Initialize counter
            Dim processedCount As Int16 = 0

            ' Process each JSON file
            For Each jsonFile As String In jsonFiles
                AppendToConsole($"Processing: {Path.GetFileName(jsonFile)}")

                ' Process the map file
                OpenOutpost2Map_JSON(jsonFile)

                ' Increment counter
                processedCount += 1

                AppendToConsole("")
            Next

            MapCount = processedCount

            ' Display summary
            AppendToConsole("=== PROCESSING COMPLETE ===")
            AppendToConsole($"Total maps processed: {processedCount}")
            AppendToConsole($"Total unique tiles in database: {tileDatabase.GetTotalTiles()}")

        Catch ex As Exception
            AppendToConsole($"Error processing maps: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' Opens and processes a single Outpost 2 .map file (converts to JSON first)
    ''' </summary>
    ''' <param name="MapPath">The file path to the .map file</param>
    Private Sub OpenOutpost2Map_MAP(ByVal MapPath As String)
        ' Open a Outpost 2 .map file

        ' - Open .map file

        ' - Convert/export to .json

        ' Then we can send for processing
        ' By converting to .json first ensures we use the same format for all maps and
        ' also means we only need one OpenOutpost2Map function
    End Sub

    ''' <summary>
    ''' Opens and processes a single Outpost 2 JSON map file, analyzing tile adjacencies
    ''' </summary>
    ''' <param name="MapPath">The file path to the JSON map file</param>
    Private Sub OpenOutpost2Map_JSON(ByVal MapPath As String)
        ' Open a Outpost 2 json map file

        ' Create a database just for this map
        Dim singleMapDatabase As New TileAdjacencyDatabase()
        ' Create a block database just for this map
        Dim singleMapBlockDatabase As New BlockAdjacencyDatabase()

        ' Get just the filename from the full path
        Dim fileName As String = Path.GetFileName(MapPath)

        Try
            ' Read the JSON file
            Dim jsonText As String = System.IO.File.ReadAllText(MapPath)
            AppendToConsole($"Loading JSON map file: {fileName}")

            ' Parse the JSON
            Dim json As JObject = JObject.Parse(jsonText)

            ' Extract header information
            Dim header As JObject = CType(json("header"), JObject)
            Dim width As Integer = CInt(header("width"))
            Dim height As Integer = CInt(header("height"))

            'AppendToConsole($"Map dimensions: {width} x {height}")
            Dim totalTiles As Integer = width * height

            ' Extract tiles array
            Dim tilesArray As JArray = CType(json("tiles"), JArray)

            ' Convert to 2D array for easier processing
            Dim tileMap(height - 1, width - 1) As Integer

            ' Dictionary to track unique tiles in this map
            Dim uniqueTilesInMap As New Dictionary(Of Integer, Integer)()
            ' Dictionary to track unique blocks in this map
            Dim uniqueBlocksInMap As New Dictionary(Of TileBlock, Integer)()

            ' Process each row of tiles
            For row As Integer = 0 To height - 1
                Dim rowArray As JArray = CType(tilesArray(row), JArray)
                For col As Integer = 0 To width - 1
                    Dim tileId As Integer = CInt(rowArray(col))
                    tileMap(row, col) = tileId

                    ' Track unique tiles and their counts
                    If uniqueTilesInMap.ContainsKey(tileId) Then
                        uniqueTilesInMap(tileId) += 1
                    Else
                        uniqueTilesInMap(tileId) = 1
                    End If

                    If tileId = 0 Then
                        'In the default tileMappings tileID 0 is the blue tile
                        'If we find tileID 0 then we have a map with non-standard tileMappings
                        'CK9_2v2ai.map and Swarm.map ..
                        'We dont want these maps for now as they are not compatible with our system

                        'MsgBox("fileName: " & fileName & " - row: " & row & " - col: " & col & " - tileId: " & tileId)
                        Debug.WriteLine("fileName: " & fileName & " - row: " & row & " - col: " & col & " - tileId: " & tileId)
                    End If
                Next
            Next
            'AppendToConsole("Tiles loaded successfully")

            ' Analyze map for the main database (tiles)
            AnalyzeMap(tileMap, width, height, tileDatabase)

            ' Analyze map for single map statistics (tiles)
            AnalyzeMap(tileMap, width, height, singleMapDatabase)

            ' Need at least a 2x2 map to analyze blocks
            If width >= 2 AndAlso height >= 2 Then
                ' Analyze map for the main database (blocks)
                AnalyzeMapForBlocks(tileMap, width, height, blockDatabase)

                ' Analyze map for single map statistics (blocks)
                AnalyzeMapForBlocks(tileMap, width, height, singleMapBlockDatabase)

                ' Count unique blocks in this map (for reporting)
                For y As Integer = 0 To height - 2
                    For x As Integer = 0 To width - 2
                        Dim block As New TileBlock(
                        tileMap(y, x),          ' Top-left
                        tileMap(y, x + 1),      ' Top-right
                        tileMap(y + 1, x),      ' Bottom-left
                        tileMap(y + 1, x + 1)   ' Bottom-right
                    )

                        If uniqueBlocksInMap.ContainsKey(block) Then
                            uniqueBlocksInMap(block) += 1
                        Else
                            uniqueBlocksInMap(block) = 1
                        End If
                    Next
                Next

                AppendToConsole($"Found {uniqueBlocksInMap.Count} unique 2x2 blocks in this map")
            Else
                AppendToConsole("Map too small for block analysis")
            End If

            ' Generate report for this map
            GenerateMapReport(fileName, width, height, totalTiles, uniqueTilesInMap.Count, uniqueTilesInMap, singleMapDatabase)

        Catch ex As Exception
            AppendToConsole($"Error loading JSON map: {ex.Message}")
        End Try
    End Sub

#End Region

#Region "Map Analysis - Tiles"

    ''' <summary>
    ''' Analyzes a tile map to record adjacency relationships between tiles
    ''' </summary>
    ''' <param name="tileMap">2D array representing the tile map</param>
    ''' <param name="width">Width of the map in tiles</param>
    ''' <param name="height">Height of the map in tiles</param>
    ''' <param name="targetDatabase">The database to store adjacency data in</param>
    Private Sub AnalyzeMap(ByVal tileMap As Integer(,), ByVal width As Integer, ByVal height As Integer, ByVal targetDatabase As TileAdjacencyDatabase)
        ' Analyze the tile adjacencies

        ' Check if this is the main database
        Dim isMainDatabase As Boolean = (targetDatabase Is tileDatabase)

        If isMainDatabase Then
            'LogText("Starting map analysis: " & MapFileName)
            'Debug.WriteLine("Starting map analysis: " & MapFileName)
        End If

        Dim totalTiles As Integer = width * height
        Dim analyzedTiles As Integer = 0

        ' Process each tile and its adjacencies
        For y As Integer = 0 To height - 1
            For x As Integer = 0 To width - 1
                Dim currentTile As Integer = tileMap(y, x)

                ' Get all 8 adjacent tiles (N, NE, E, SE, S, SW, W, NW)
                ' North
                If y > 0 Then
                    Dim northTile As Integer = tileMap(y - 1, x)
                    RecordAdjacency(currentTile, northTile, "N", targetDatabase)
                End If

                ' Northeast
                If y > 0 AndAlso x < width - 1 Then
                    Dim northeastTile As Integer = tileMap(y - 1, x + 1)
                    RecordAdjacency(currentTile, northeastTile, "NE", targetDatabase)
                End If

                ' East
                If x < width - 1 Then
                    Dim eastTile As Integer = tileMap(y, x + 1)
                    RecordAdjacency(currentTile, eastTile, "E", targetDatabase)
                End If

                ' Southeast
                If y < height - 1 AndAlso x < width - 1 Then
                    Dim southeastTile As Integer = tileMap(y + 1, x + 1)
                    RecordAdjacency(currentTile, southeastTile, "SE", targetDatabase)
                End If

                ' South
                If y < height - 1 Then
                    Dim southTile As Integer = tileMap(y + 1, x)
                    RecordAdjacency(currentTile, southTile, "S", targetDatabase)
                End If

                ' Southwest
                If y < height - 1 AndAlso x > 0 Then
                    Dim southwestTile As Integer = tileMap(y + 1, x - 1)
                    RecordAdjacency(currentTile, southwestTile, "SW", targetDatabase)
                End If

                ' West
                If x > 0 Then
                    Dim westTile As Integer = tileMap(y, x - 1)
                    RecordAdjacency(currentTile, westTile, "W", targetDatabase)
                End If

                ' Northwest
                If y > 0 AndAlso x > 0 Then
                    Dim northwestTile As Integer = tileMap(y - 1, x - 1)
                    RecordAdjacency(currentTile, northwestTile, "NW", targetDatabase)
                End If

                analyzedTiles += 1
            Next
        Next

        ' Only display summary for the main database analysis
        If isMainDatabase Then
            'LogText("Map analysis complete")
            AppendToConsole($"Total tiles analyzed: {analyzedTiles} of {totalTiles}" & vbNewLine)

        End If
    End Sub

    ''' <summary>
    ''' Records a tile adjacency relationship in the specified database
    ''' </summary>
    ''' <param name="sourceTile">The source tile ID</param>
    ''' <param name="adjacentTile">The adjacent tile ID</param>
    ''' <param name="direction">The direction as a string (N, NE, E, etc.)</param>
    ''' <param name="targetDatabase">The database to record the adjacency in</param>
    Private Sub RecordAdjacency(ByVal sourceTile As Integer, ByVal adjacentTile As Integer, ByVal direction As String, ByVal targetDatabase As TileAdjacencyDatabase)
        ' Record the adjacency relationship in the database

        ' If no target database specified, use the main one
        If targetDatabase Is Nothing Then
            targetDatabase = tileDatabase
        End If

        ' Convert string direction to enum
        Dim adjacencyDirection As AdjacencyDirection
        Select Case direction.ToUpper()
            Case "N"
                adjacencyDirection = AdjacencyDirection.North
            Case "NE"
                adjacencyDirection = AdjacencyDirection.Northeast
            Case "E"
                adjacencyDirection = AdjacencyDirection.East
            Case "SE"
                adjacencyDirection = AdjacencyDirection.Southeast
            Case "S"
                adjacencyDirection = AdjacencyDirection.South
            Case "SW"
                adjacencyDirection = AdjacencyDirection.Southwest
            Case "W"
                adjacencyDirection = AdjacencyDirection.West
            Case "NW"
                adjacencyDirection = AdjacencyDirection.Northwest
        End Select

        ' Record in the specified database
        targetDatabase.RecordAdjacency(sourceTile, adjacentTile, adjacencyDirection)
    End Sub

#End Region

#Region "Map Analysis - Blocks"

    ''' <summary>
    ''' Analyzes a tile map to record adjacency relationships between 2x2 tile blocks
    ''' </summary>
    ''' <param name="tileMap">2D array representing the tile map</param>
    ''' <param name="width">Width of the map in tiles</param>
    ''' <param name="height">Height of the map in tiles</param>
    ''' <param name="blockDatabase">The database to store block adjacency data in</param>
    Private Sub AnalyzeMapForBlocks(ByVal tileMap As Integer(,), ByVal width As Integer, ByVal height As Integer, ByVal blockDatabase As BlockAdjacencyDatabase)
        ' Process each 2x2 block and its adjacent blocks
        For y As Integer = 0 To height - 2
            For x As Integer = 0 To width - 2
                ' Create the current 2x2 block
                Dim currentBlock As New TileBlock(
                tileMap(y, x),          ' Top-left
                tileMap(y, x + 1),      ' Top-right
                tileMap(y + 1, x),      ' Bottom-left
                tileMap(y + 1, x + 1)   ' Bottom-right
            )

                ' Check the block to the North (if exists)
                If y >= 1 Then
                    Dim northBlock As New TileBlock(
                    tileMap(y - 1, x),      ' Top-left
                    tileMap(y - 1, x + 1),  ' Top-right
                    tileMap(y, x),          ' Bottom-left
                    tileMap(y, x + 1)       ' Bottom-right
                )
                    blockDatabase.RecordAdjacency(currentBlock, northBlock, BlockDirection.North)
                End If

                ' Check the block to the East (if exists)
                If x + 2 < width Then
                    Dim eastBlock As New TileBlock(
                    tileMap(y, x + 1),      ' Top-left
                    tileMap(y, x + 2),      ' Top-right
                    tileMap(y + 1, x + 1),  ' Bottom-left
                    tileMap(y + 1, x + 2)   ' Bottom-right
                )
                    blockDatabase.RecordAdjacency(currentBlock, eastBlock, BlockDirection.East)
                End If

                ' Check the block to the South (if exists)
                If y + 2 < height Then
                    Dim southBlock As New TileBlock(
                    tileMap(y + 1, x),      ' Top-left
                    tileMap(y + 1, x + 1),  ' Top-right
                    tileMap(y + 2, x),      ' Bottom-left
                    tileMap(y + 2, x + 1)   ' Bottom-right
                )
                    blockDatabase.RecordAdjacency(currentBlock, southBlock, BlockDirection.South)
                End If

                ' Check the block to the West (if exists)
                If x >= 1 Then
                    Dim westBlock As New TileBlock(
                    tileMap(y, x - 1),      ' Top-left
                    tileMap(y, x),          ' Top-right
                    tileMap(y + 1, x - 1),  ' Bottom-left
                    tileMap(y + 1, x)       ' Bottom-right
                )
                    blockDatabase.RecordAdjacency(currentBlock, westBlock, BlockDirection.West)
                End If
            Next
        Next
    End Sub

#End Region

#Region "Report Generation"

    ''' <summary>
    ''' Generates a detailed report for a single map analysis
    ''' </summary>
    ''' <param name="fileName">Name of the map file</param>
    ''' <param name="width">Width of the map</param>
    ''' <param name="height">Height of the map</param>
    ''' <param name="totalTiles">Total number of tiles in the map</param>
    ''' <param name="uniqueTileCount">Number of unique tiles found</param>
    ''' <param name="uniqueTilesMap">Dictionary of tile IDs and their occurrence counts</param>
    ''' <param name="singleMapDatabase">Database containing adjacency data for this map only</param>
    Private Sub GenerateMapReport(ByVal fileName As String, ByVal width As Integer, ByVal height As Integer,
                              ByVal totalTiles As Integer, ByVal uniqueTileCount As Integer,
                              ByVal uniqueTilesMap As Dictionary(Of Integer, Integer),
                              ByVal singleMapDatabase As TileAdjacencyDatabase)
        ' Generate a report for this specific map

        'Dim reportPath As String = Path.Combine(MapsPath_JSON),
        '                                   "MapReports",
        '                                   
        Dim reportPath As String = MapsPath_JSON & "\MapReports\" & Path.GetFileNameWithoutExtension(fileName) + "_report.txt"

        ' Create directory if it doesn't exist
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath))

        Try
            Using writer As New StreamWriter(reportPath)
                writer.WriteLine("=== MAP ANALYSIS REPORT ===")
                writer.WriteLine($"Map file: {fileName}")
                writer.WriteLine($"Date analyzed: {DateTime.Now}")
                writer.WriteLine("")

                writer.WriteLine("--- Map Statistics ---")
                writer.WriteLine($"Dimensions: {width} x {height}")
                writer.WriteLine($"Total tiles: {totalTiles}")
                writer.WriteLine($"Unique tiles: {uniqueTileCount}")
                writer.WriteLine("")

                writer.WriteLine("--- Top 20 Most Frequent Tiles ---")
                Dim sortedTiles = uniqueTilesMap.OrderByDescending(Function(x) x.Value).Take(20)

                For Each tile In sortedTiles
                    writer.WriteLine($"  Tile {tile.Key}: {tile.Value} occurrences ({(tile.Value / CDbl(totalTiles) * 100):F2}% of map)")
                Next

                writer.WriteLine("")
                'writer.WriteLine("--- Adjacency Statistics by Direction ---")

                'For Each direction As AdjacencyDirection In [Enum].GetValues(GetType(AdjacencyDirection))
                '    Dim uniqueAdjacencies As Integer = 0
                '    Dim totalOccurrences As Integer = 0

                '    For tileId As Integer = 0 To 2011
                '        Dim tileInfo As TileAdjacencyInfo = singleMapDatabase.GetAdjacencyInfo(tileId)
                '        If tileInfo IsNot Nothing Then
                '            Dim adjacencies = tileInfo.GetAllAdjacenciesInDirection(direction)
                '            uniqueAdjacencies += adjacencies.Count
                '            For Each adj In adjacencies
                '                totalOccurrences += adj.Value
                '            Next
                '        End If
                '    Next

                '    writer.WriteLine($"{direction}: {uniqueAdjacencies} unique adjacencies, {totalOccurrences} total occurrences")
                'Next
            End Using

            ' Just log the path where the report was saved
            AppendToConsole($"Map report saved to: {reportPath}")

        Catch ex As Exception
            AppendToConsole($"Error generating map report: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' Generates a comprehensive report about the entire tile adjacency database
    ''' </summary>
    ''' <param name="databaseFilePath">Path to the database file</param>
    ''' <param name="reportPath">Path where the report will be saved</param>
    Private Sub GenerateDatabaseReport(ByVal databaseFilePath As String, ByVal reportPath As String)
        ' Generate a comprehensive report about the entire tiles database
        Try
            'LogText("Generating database report...")
            Debug.WriteLine("Generating database report...")

            Using writer As New StreamWriter(reportPath)
                writer.WriteLine("=== OUTPOST 2 TILE ADJACENCY DATABASE REPORT ===")
                writer.WriteLine($"Database file: {Path.GetFileName(databaseFilePath)}")
                writer.WriteLine($"Date generated: {DateTime.Now}")
                writer.WriteLine($"File size: {New FileInfo(databaseFilePath).Length:N0} bytes")
                writer.WriteLine("")

                writer.WriteLine("--- General Statistics ---")
                writer.WriteLine($"Total maps analyzed: {MapCount}")
                writer.WriteLine($"Total unique tiles in database: {tileDatabase.GetTotalTiles()}")

                ' Count total adjacencies
                Dim totalAdjacencies As Integer = 0
                Dim totalUniqueAdjacencies As Integer = 0

                For tileId As Integer = 0 To 2011
                    Dim tileInfo As TileAdjacencyInfo = tileDatabase.GetAdjacencyInfo(tileId)
                    If tileInfo IsNot Nothing Then
                        For Each direction As AdjacencyDirection In [Enum].GetValues(GetType(AdjacencyDirection))
                            Dim adjacencies = tileInfo.GetAllAdjacenciesInDirection(direction)
                            totalUniqueAdjacencies += adjacencies.Count
                            For Each adj In adjacencies
                                totalAdjacencies += adj.Value
                            Next
                        Next
                    End If
                Next

                writer.WriteLine($"Total adjacency records: {totalAdjacencies:N0}")
                writer.WriteLine($"Total unique adjacency relationships: {totalUniqueAdjacencies:N0}")
                writer.WriteLine("")

                writer.WriteLine("--- Adjacency Statistics by Direction ---")

                For Each direction As AdjacencyDirection In [Enum].GetValues(GetType(AdjacencyDirection))
                    Dim uniqueAdjacencies As Integer = 0
                    Dim totalOccurrences As Integer = 0

                    For tileId As Integer = 0 To 2011
                        Dim tileInfo As TileAdjacencyInfo = tileDatabase.GetAdjacencyInfo(tileId)
                        If tileInfo IsNot Nothing Then
                            Dim adjacencies = tileInfo.GetAllAdjacenciesInDirection(direction)
                            uniqueAdjacencies += adjacencies.Count
                            For Each adj In adjacencies
                                totalOccurrences += adj.Value
                            Next
                        End If
                    Next

                    writer.WriteLine($"{direction}: {uniqueAdjacencies:N0} unique adjacencies, {totalOccurrences:N0} total occurrences")
                Next

                writer.WriteLine("")
                writer.WriteLine("--- Top 20 Most Frequently Appearing Tiles ---")

                ' Dictionary to count how many times each tile appears in adjacencies
                Dim tileAppearanceCount As New Dictionary(Of Integer, Integer)

                For sourceTileId As Integer = 0 To 2011
                    Dim tileInfo As TileAdjacencyInfo = tileDatabase.GetAdjacencyInfo(sourceTileId)
                    If tileInfo IsNot Nothing Then
                        For Each direction As AdjacencyDirection In [Enum].GetValues(GetType(AdjacencyDirection))
                            Dim adjacencies = tileInfo.GetAllAdjacenciesInDirection(direction)
                            For Each adj In adjacencies
                                If tileAppearanceCount.ContainsKey(adj.Key) Then
                                    tileAppearanceCount(adj.Key) += adj.Value
                                Else
                                    tileAppearanceCount(adj.Key) = adj.Value
                                End If
                            Next
                        Next
                    End If
                Next

                Dim sortedTiles = tileAppearanceCount.OrderByDescending(Function(x) x.Value).Take(20)

                Dim tileRank As Integer = 1
                For Each tile In sortedTiles
                    Dim percentage As Double = tile.Value / CDbl(totalAdjacencies) * 100
                    writer.WriteLine($"{tileRank}. Tile {tile.Key}: {tile.Value:N0} total appearances ({percentage:F2}% of all adjacencies)")
                    tileRank += 1
                Next

                writer.WriteLine("")
                writer.WriteLine("--- Top 20 Most Common Adjacency Relationships ---")

                ' Dictionary to store adjacency patterns with their frequencies
                Dim adjacencyFrequencies As New Dictionary(Of String, Integer)

                ' Collect all adjacencies
                For sourceTileId As Integer = 0 To 2011
                    Dim tileInfo As TileAdjacencyInfo = tileDatabase.GetAdjacencyInfo(sourceTileId)
                    If tileInfo IsNot Nothing Then
                        For Each direction As AdjacencyDirection In [Enum].GetValues(GetType(AdjacencyDirection))
                            Dim adjacencies = tileInfo.GetAllAdjacenciesInDirection(direction)
                            For Each adj In adjacencies
                                Dim key = $"Tile {sourceTileId} {direction} to Tile {adj.Key}"
                                If adjacencyFrequencies.ContainsKey(key) Then
                                    adjacencyFrequencies(key) += adj.Value
                                Else
                                    adjacencyFrequencies(key) = adj.Value
                                End If
                            Next
                        Next
                    End If
                Next

                ' Sort and display top 20
                Dim sortedPatterns = adjacencyFrequencies.OrderByDescending(Function(x) x.Value).Take(20)

                tileRank = 1
                For Each pattern In sortedPatterns
                    Dim percentage As Double = pattern.Value / CDbl(totalAdjacencies) * 100
                    writer.WriteLine($"{tileRank}. {pattern.Key}: {pattern.Value:N0} occurrences ({percentage:F2}%)")
                    tileRank += 1
                Next

                writer.WriteLine("")
                writer.WriteLine("--- Tiles with Most Unique Adjacencies ---")

                ' Dictionary to count how many unique adjacencies each tile has
                Dim tileUniqueAdjacenciesCount As New Dictionary(Of Integer, Integer)

                For sourceTileId As Integer = 0 To 2011
                    Dim tileInfo As TileAdjacencyInfo = tileDatabase.GetAdjacencyInfo(sourceTileId)
                    If tileInfo IsNot Nothing Then
                        Dim uniqueCount As Integer = 0
                        For Each direction As AdjacencyDirection In [Enum].GetValues(GetType(AdjacencyDirection))
                            uniqueCount += tileInfo.GetAllAdjacenciesInDirection(direction).Count
                        Next
                        tileUniqueAdjacenciesCount(sourceTileId) = uniqueCount
                    End If
                Next

                Dim sortedByUnique = tileUniqueAdjacenciesCount.OrderByDescending(Function(x) x.Value).Take(20)

                tileRank = 1
                For Each tile In sortedByUnique
                    writer.WriteLine($"{tileRank}. Tile {tile.Key}: {tile.Value:N0} unique adjacency relationships")
                    tileRank += 1
                Next
            End Using

            AppendToConsole($"Database report saved to: {reportPath}")

        Catch ex As Exception
            AppendToConsole($"Error generating database report: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' Generates a comprehensive report about the entire block adjacency database
    ''' </summary>
    ''' <param name="databaseFilePath">Path to the block database file</param>
    ''' <param name="reportPath">Path where the report will be saved</param>
    Private Sub GenerateBlockDatabaseReport(ByVal databaseFilePath As String, ByVal reportPath As String)
        'Generate a comprehensive report about the entire blocks database

        Try
            Debug.WriteLine("Generating block database report...")

            Using writer As New StreamWriter(reportPath)
                writer.WriteLine("=== OUTPOST 2 BLOCK ADJACENCY DATABASE REPORT ===")
                writer.WriteLine($"Database file: {Path.GetFileName(databaseFilePath)}")
                writer.WriteLine($"Date generated: {DateTime.Now}")
                writer.WriteLine($"File size: {New FileInfo(databaseFilePath).Length:N0} bytes")
                writer.WriteLine("")

                writer.WriteLine("--- General Statistics ---")
                writer.WriteLine($"Total maps analyzed: {MapCount}")
                writer.WriteLine($"Total unique 2x2 blocks in database: {blockDatabase.GetTotalBlocks()}")

                ' Count total adjacencies
                Dim totalAdjacencies As Integer = 0
                Dim totalUniqueAdjacencies As Integer = 0

                For Each sourceBlock In blockDatabase.GetAllBlocks()
                    For Each direction As BlockDirection In [Enum].GetValues(GetType(BlockDirection))
                        Dim adjacentBlocks = blockDatabase.GetAdjacentBlocks(sourceBlock, direction)
                        totalUniqueAdjacencies += adjacentBlocks.Count
                        For Each adj In adjacentBlocks
                            totalAdjacencies += adj.Value
                        Next
                    Next
                Next

                writer.WriteLine($"Total block adjacency records: {totalAdjacencies:N0}")
                writer.WriteLine($"Total unique block adjacency relationships: {totalUniqueAdjacencies:N0}")
                writer.WriteLine("")

                writer.WriteLine("--- Block Adjacency Statistics by Direction ---")

                For Each direction As BlockDirection In [Enum].GetValues(GetType(BlockDirection))
                    Dim uniqueAdjacencies As Integer = 0
                    Dim totalOccurrences As Integer = 0

                    For Each sourceBlock In blockDatabase.GetAllBlocks()
                        Dim adjacentBlocks = blockDatabase.GetAdjacentBlocks(sourceBlock, direction)
                        uniqueAdjacencies += adjacentBlocks.Count
                        For Each adj In adjacentBlocks
                            totalOccurrences += adj.Value
                        Next
                    Next

                    writer.WriteLine($"{direction}: {uniqueAdjacencies:N0} unique adjacencies, {totalOccurrences:N0} total occurrences")
                Next

                writer.WriteLine("")
                writer.WriteLine("--- Top 20 Most Common Blocks ---")

                ' Count how many times each block appears in adjacencies
                Dim blockAppearanceCount As New Dictionary(Of TileBlock, Integer)()

                For Each sourceBlock In blockDatabase.GetAllBlocks()
                    For Each direction As BlockDirection In [Enum].GetValues(GetType(BlockDirection))
                        Dim adjacentBlocks = blockDatabase.GetAdjacentBlocks(sourceBlock, direction)
                        For Each adj In adjacentBlocks
                            If blockAppearanceCount.ContainsKey(adj.Key) Then
                                blockAppearanceCount(adj.Key) += adj.Value
                            Else
                                blockAppearanceCount(adj.Key) = adj.Value
                            End If
                        Next
                    Next
                Next

                Dim sortedBlocks = blockAppearanceCount.OrderByDescending(Function(x) x.Value).Take(20)

                Dim blockRank As Integer = 1
                For Each block In sortedBlocks
                    Dim percentage As Double = block.Value / CDbl(totalAdjacencies) * 100
                    writer.WriteLine($"{blockRank}. Block {block.Key}: {block.Value:N0} total appearances ({percentage:F2}% of all adjacencies)")
                    blockRank += 1
                Next

                writer.WriteLine("")
                writer.WriteLine("--- Top 20 Most Common Block Adjacency Patterns ---")

                ' Dictionary to store block adjacency patterns with their frequencies
                Dim adjacencyFrequencies As New Dictionary(Of String, Integer)()

                ' Collect all adjacencies
                For Each sourceBlock In blockDatabase.GetAllBlocks()
                    For Each direction As BlockDirection In [Enum].GetValues(GetType(BlockDirection))
                        Dim adjacentBlocks = blockDatabase.GetAdjacentBlocks(sourceBlock, direction)
                        For Each adj In adjacentBlocks
                            Dim key = $"Block {sourceBlock} {direction} to Block {adj.Key}"
                            If adjacencyFrequencies.ContainsKey(key) Then
                                adjacencyFrequencies(key) += adj.Value
                            Else
                                adjacencyFrequencies(key) = adj.Value
                            End If
                        Next
                    Next
                Next

                ' Sort and display top 20
                Dim sortedPatterns = adjacencyFrequencies.OrderByDescending(Function(x) x.Value).Take(20)

                blockRank = 1
                For Each pattern In sortedPatterns
                    Dim percentage As Double = pattern.Value / CDbl(totalAdjacencies) * 100
                    writer.WriteLine($"{blockRank}. {pattern.Key}: {pattern.Value:N0} occurrences ({percentage:F2}%)")
                    blockRank += 1
                Next
            End Using

            AppendToConsole($"Block database report saved to: {reportPath}")

        Catch ex As Exception
            AppendToConsole($"Error generating block database report: {ex.Message}")
        End Try
    End Sub

#End Region

#Region "Display Functions - Tiles"

    ''' <summary>
    ''' Displays all tile adjacency data stored in the database (produces extensive output)
    ''' </summary>
    Private Sub DisplayDatabaseContents()
        'Display all tile adjacency data stored in the database

        AppendToConsole("=== Database Contents ===")
        AppendToConsole($"Total tiles recorded: {tileDatabase.GetTotalTiles()}")
        AppendToConsole("")

        ' Iterate through all tiles that have adjacency data
        For tileId As Integer = 0 To 2011  ' Outpost 2 has 2012 tiles (0-2011)
            Dim tileInfo As TileAdjacencyInfo = tileDatabase.GetAdjacencyInfo(tileId)

            If tileInfo IsNot Nothing Then
                ' Check if this tile has any adjacencies
                Dim hasAdjacencies As Boolean = False

                ' Check each direction to see if there are any adjacencies
                For Each direction As AdjacencyDirection In [Enum].GetValues(GetType(AdjacencyDirection))
                    Dim adjacencies = tileInfo.GetAllAdjacenciesInDirection(direction)
                    If adjacencies.Count > 0 Then
                        hasAdjacencies = True
                        Exit For
                    End If
                Next

                ' Only display if tile has adjacencies
                If hasAdjacencies Then
                    AppendToConsole($"Tile ID: {tileId}")

                    ' Display adjacencies for each direction
                    For Each direction As AdjacencyDirection In [Enum].GetValues(GetType(AdjacencyDirection))
                        Dim adjacencies = tileInfo.GetAllAdjacenciesInDirection(direction)

                        If adjacencies.Count > 0 Then
                            AppendToConsole($"  {direction}:")

                            ' Sort by tile ID for consistent display
                            Dim sortedAdjacencies = adjacencies.OrderBy(Function(x) x.Key)

                            For Each adjacency In sortedAdjacencies
                                AppendToConsole($"    Tile {adjacency.Key}: {adjacency.Value} occurrence(s)")
                            Next
                        End If
                    Next

                    AppendToConsole("")  ' Empty line between tiles
                End If
            End If
        Next

        AppendToConsole("=== End of Database Contents ===")
    End Sub

    ''' <summary>
    ''' Displays a summary of the tile database contents
    ''' </summary>
    Private Sub DisplayDatabaseSummary()
        'Display a summary of the database contents

        AppendToConsole("=== Database Summary ===")
        AppendToConsole($"Total tiles recorded: {tileDatabase.GetTotalTiles()}")

        ' Count total adjacencies
        Dim totalAdjacencies As Integer = 0
        For tileId As Integer = 0 To 2011
            Dim tileInfo As TileAdjacencyInfo = tileDatabase.GetAdjacencyInfo(tileId)
            If tileInfo IsNot Nothing Then
                For Each direction As AdjacencyDirection In [Enum].GetValues(GetType(AdjacencyDirection))
                    Dim adjacencies = tileInfo.GetAllAdjacenciesInDirection(direction)
                    For Each adj In adjacencies
                        totalAdjacencies += adj.Value
                    Next
                Next
            End If
        Next

        AppendToConsole($"Total adjacency records: {totalAdjacencies}")
        AppendToConsole("")
    End Sub

    ''' <summary>
    ''' Displays the most common tile adjacency patterns
    ''' </summary>
    ''' <param name="topN">Number of top adjacencies to display</param>
    Private Sub DisplayMostCommonAdjacencies(Optional topN As Integer = 10)
        'Display the most common adjacency patterns

        AppendToConsole($"=== Top {topN} Most Common Adjacencies ===")

        ' Dictionary to store adjacency patterns with their frequencies
        Dim adjacencyFrequencies As New Dictionary(Of String, Integer)

        ' Collect all adjacencies
        For sourceTileId As Integer = 0 To 2011
            Dim tileInfo As TileAdjacencyInfo = tileDatabase.GetAdjacencyInfo(sourceTileId)
            If tileInfo IsNot Nothing Then
                For Each direction As AdjacencyDirection In [Enum].GetValues(GetType(AdjacencyDirection))
                    Dim adjacencies = tileInfo.GetAllAdjacenciesInDirection(direction)
                    For Each adj In adjacencies
                        Dim key = $"Tile {sourceTileId} {direction} to Tile {adj.Key}"
                        If adjacencyFrequencies.ContainsKey(key) Then
                            adjacencyFrequencies(key) += adj.Value
                        Else
                            adjacencyFrequencies(key) = adj.Value
                        End If
                    Next
                Next
            End If
        Next

        ' Sort and display top N
        Dim sortedResults = adjacencyFrequencies.OrderByDescending(Function(x) x.Value).Take(topN)

        For Each result In sortedResults
            AppendToConsole($"{result.Key}: {result.Value} occurrences")
        Next

        AppendToConsole("")
    End Sub

    ''' <summary>
    ''' Displays adjacency data for a specific tile
    ''' </summary>
    ''' <param name="tileId">The tile ID to display data for</param>
    Private Sub DisplaySpecificTile(tileId As Integer)
        'Display adjacency data for a specific tile

        AppendToConsole($"=== Adjacency Data for Tile {tileId} ===")

        Dim tileInfo As TileAdjacencyInfo = tileDatabase.GetAdjacencyInfo(tileId)

        If tileInfo IsNot Nothing Then
            ' Display compact summary
            For Each direction As AdjacencyDirection In [Enum].GetValues(GetType(AdjacencyDirection))
                Dim adjacencies = tileInfo.GetAllAdjacenciesInDirection(direction)
                If adjacencies.Count > 0 Then
                    Dim totalOccurrences = adjacencies.Sum(Function(x) x.Value)
                    AppendToConsole($"{direction}: {adjacencies.Count} unique tiles, {totalOccurrences} total occurrences")
                End If
            Next
        Else
            AppendToConsole($"No data found for tile {tileId}")
        End If

        AppendToConsole("")
    End Sub

    ''' <summary>
    ''' Displays adjacency statistics broken down by direction
    ''' </summary>
    Private Sub DisplayAdjacencyStatsByDirection()
        'Display statistics by direction

        AppendToConsole("=== Adjacency Statistics by Direction ===")

        For Each direction As AdjacencyDirection In [Enum].GetValues(GetType(AdjacencyDirection))
            Dim uniqueAdjacencies As Integer = 0
            Dim totalOccurrences As Integer = 0

            For tileId As Integer = 0 To 2011
                Dim tileInfo As TileAdjacencyInfo = tileDatabase.GetAdjacencyInfo(tileId)
                If tileInfo IsNot Nothing Then
                    Dim adjacencies = tileInfo.GetAllAdjacenciesInDirection(direction)
                    uniqueAdjacencies += adjacencies.Count
                    For Each adj In adjacencies
                        totalOccurrences += adj.Value
                    Next
                End If
            Next

            AppendToConsole($"{direction}: {uniqueAdjacencies} unique adjacencies, {totalOccurrences} total occurrences")
        Next

        AppendToConsole("")
    End Sub

    ''' <summary>
    ''' Displays statistics about which tiles are most commonly used across all maps
    ''' </summary>
    Private Sub DisplayTileUsageStatistics()
        'Display which tiles are most commonly used

        AppendToConsole("=== Tile Usage Statistics ===")

        ' Dictionary to count how many times each tile appears in adjacencies
        Dim tileAppearanceCount As New Dictionary(Of Integer, Integer)

        For sourceTileId As Integer = 0 To 2011
            Dim tileInfo As TileAdjacencyInfo = tileDatabase.GetAdjacencyInfo(sourceTileId)
            If tileInfo IsNot Nothing Then
                For Each direction As AdjacencyDirection In [Enum].GetValues(GetType(AdjacencyDirection))
                    Dim adjacencies = tileInfo.GetAllAdjacenciesInDirection(direction)
                    For Each adj In adjacencies
                        If tileAppearanceCount.ContainsKey(adj.Key) Then
                            tileAppearanceCount(adj.Key) += adj.Value
                        Else
                            tileAppearanceCount(adj.Key) = adj.Value
                        End If
                    Next
                Next
            End If
        Next

        ' Show top 10 most used tiles
        Dim sortedTiles = tileAppearanceCount.OrderByDescending(Function(x) x.Value).Take(10)

        AppendToConsole("Top 10 most frequently appearing tiles:")
        For Each tile In sortedTiles
            AppendToConsole($"  Tile {tile.Key}: {tile.Value} total appearances")
        Next

        AppendToConsole("")
    End Sub

#End Region

#Region "Display Functions - Blocks"

    ''' <summary>
    ''' Displays a summary of the block database contents
    ''' </summary>
    Private Sub DisplayBlockDatabaseSummary()
        'Display a summary of the block database contents

        AppendToConsole("=== Block Database Summary ===")
        AppendToConsole($"Total unique 2x2 blocks recorded: {blockDatabase.GetTotalBlocks()}")

        ' Count total adjacencies
        Dim totalAdjacencies As Integer = 0
        Dim uniqueAdjacencies As Integer = 0

        For Each sourceBlock In blockDatabase.GetAllBlocks()
            For Each direction As BlockDirection In [Enum].GetValues(GetType(BlockDirection))
                Dim adjacentBlocks = blockDatabase.GetAdjacentBlocks(sourceBlock, direction)
                uniqueAdjacencies += adjacentBlocks.Count
                For Each adj In adjacentBlocks
                    totalAdjacencies += adj.Value
                Next
            Next
        Next

        AppendToConsole($"Total block adjacency records: {totalAdjacencies}")
        AppendToConsole($"Total unique block adjacency relationships: {uniqueAdjacencies}")
        AppendToConsole("")
    End Sub

#End Region

End Class