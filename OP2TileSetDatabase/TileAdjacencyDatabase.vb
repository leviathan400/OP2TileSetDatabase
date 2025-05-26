Imports Newtonsoft.Json

' TileAdjacencyDatabase.vb

Public Class SerializableTileAdjacencyData
    ' Serializable data class for export/import

    Public TileId As Integer
    Public AdjacencyInfo As TileAdjacencyInfo

    Public Sub New()
    End Sub

    Public Sub New(tileId As Integer, info As TileAdjacencyInfo)
        Me.TileId = tileId
        Me.AdjacencyInfo = info
    End Sub
End Class

Public Class TileAdjacencyDatabase

    ' Dictionary to store adjacency data for each tile
    Private adjacencyData As Dictionary(Of Integer, TileAdjacencyInfo)

    Public Sub New()
        adjacencyData = New Dictionary(Of Integer, TileAdjacencyInfo)()
    End Sub

    Public Sub RecordAdjacency(sourceTile As Integer, adjacentTile As Integer, direction As AdjacencyDirection)
        ' Ensure we have an entry for this tile
        If Not adjacencyData.ContainsKey(sourceTile) Then
            adjacencyData(sourceTile) = New TileAdjacencyInfo()
        End If

        ' Record the adjacency
        adjacencyData(sourceTile).RecordAdjacency(adjacentTile, direction)
    End Sub

    Public Function GetAdjacencyInfo(tileId As Integer) As TileAdjacencyInfo
        If adjacencyData.ContainsKey(tileId) Then
            Return adjacencyData(tileId)
        Else
            Return Nothing
        End If
    End Function

    Public Function GetTotalTiles() As Integer
        Return adjacencyData.Count
    End Function

    Public Sub SaveToFile(filePath As String)
        'Save the database to a JSON file
        Try
            ' Convert dictionary to list for proper JSON serialization
            Dim serializableData As New List(Of SerializableTileAdjacencyData)

            For Each kvp In adjacencyData
                serializableData.Add(New SerializableTileAdjacencyData(kvp.Key, kvp.Value))
            Next

            ' Configure JSON serialization settings
            Dim settings As New JsonSerializerSettings()
            settings.Formatting = Formatting.Indented
            settings.Converters.Add(New Newtonsoft.Json.Converters.StringEnumConverter())

            ' Convert to JSON string
            Dim jsonString As String = JsonConvert.SerializeObject(serializableData, settings)

            ' Write to file
            System.IO.File.WriteAllText(filePath, jsonString)

        Catch ex As Exception
            Throw New Exception($"Failed to save database to file: {ex.Message}", ex)
        End Try
    End Sub

    Public Sub LoadFromFile(filePath As String)
        'Load the database from a JSON file
        Try
            ' Read JSON string from file
            Dim jsonString As String = System.IO.File.ReadAllText(filePath)

            ' Configure JSON deserialization settings
            Dim settings As New JsonSerializerSettings()
            settings.Converters.Add(New Newtonsoft.Json.Converters.StringEnumConverter())

            ' Deserialize from JSON
            Dim serializableData As List(Of SerializableTileAdjacencyData) = JsonConvert.DeserializeObject(Of List(Of SerializableTileAdjacencyData))(jsonString, settings)

            ' Convert back to dictionary
            adjacencyData = New Dictionary(Of Integer, TileAdjacencyInfo)()

            If serializableData IsNot Nothing Then
                For Each item In serializableData
                    adjacencyData(item.TileId) = item.AdjacencyInfo
                Next
            End If

        Catch ex As Exception
            ' If file doesn't exist or can't be read, start with empty database
            adjacencyData = New Dictionary(Of Integer, TileAdjacencyInfo)()
            Throw New Exception($"Failed to load database from file: {ex.Message}", ex)
        End Try
    End Sub

End Class

Public Class TileAdjacencyInfo

    ' Make this public so JSON can serialize it
    Public AdjacencyByDirection As Dictionary(Of AdjacencyDirection, Dictionary(Of Integer, Integer))

    Public Sub New()
        AdjacencyByDirection = New Dictionary(Of AdjacencyDirection, Dictionary(Of Integer, Integer))()

        ' Initialize dictionaries for each direction
        For Each direction As AdjacencyDirection In [Enum].GetValues(GetType(AdjacencyDirection))
            AdjacencyByDirection(direction) = New Dictionary(Of Integer, Integer)()
        Next
    End Sub

    Public Sub RecordAdjacency(adjacentTile As Integer, direction As AdjacencyDirection)
        Dim directionDict = AdjacencyByDirection(direction)

        If directionDict.ContainsKey(adjacentTile) Then
            ' Increment count
            directionDict(adjacentTile) += 1
        Else
            ' First occurrence
            directionDict(adjacentTile) = 1
        End If
    End Sub

    Public Function GetAdjacencyCount(adjacentTile As Integer, direction As AdjacencyDirection) As Integer
        Dim directionDict = AdjacencyByDirection(direction)

        If directionDict.ContainsKey(adjacentTile) Then
            Return directionDict(adjacentTile)
        Else
            Return 0
        End If
    End Function

    Public Function GetAllAdjacenciesInDirection(direction As AdjacencyDirection) As Dictionary(Of Integer, Integer)
        Return AdjacencyByDirection(direction)
    End Function

End Class

Public Enum AdjacencyDirection
    North
    Northeast
    East
    Southeast
    South
    Southwest
    West
    Northwest
End Enum
