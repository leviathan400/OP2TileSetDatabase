Imports Newtonsoft.Json

Public Enum BlockDirection
    North
    East
    South
    West
End Enum

Public Class BlockAdjacencyDatabase
    ' Dictionary maps from a source block to a dictionary of (direction -> (target block -> count))
    Private adjacencyData As Dictionary(Of TileBlock, Dictionary(Of BlockDirection, Dictionary(Of TileBlock, Integer)))

    Public Sub New()
        adjacencyData = New Dictionary(Of TileBlock, Dictionary(Of BlockDirection, Dictionary(Of TileBlock, Integer)))()
    End Sub

    Public Sub RecordAdjacency(sourceBlock As TileBlock, adjacentBlock As TileBlock, direction As BlockDirection)
        ' Ensure we have an entry for this block
        If Not adjacencyData.ContainsKey(sourceBlock) Then
            adjacencyData(sourceBlock) = New Dictionary(Of BlockDirection, Dictionary(Of TileBlock, Integer))()
            For Each dir As BlockDirection In [Enum].GetValues(GetType(BlockDirection))
                adjacencyData(sourceBlock)(dir) = New Dictionary(Of TileBlock, Integer)()
            Next
        End If

        ' Record the adjacency
        Dim directionDict = adjacencyData(sourceBlock)(direction)
        If directionDict.ContainsKey(adjacentBlock) Then
            directionDict(adjacentBlock) += 1
        Else
            directionDict(adjacentBlock) = 1
        End If
    End Sub

    Public Function GetTotalBlocks() As Integer
        Return adjacencyData.Count
    End Function

    Public Function GetAdjacencyCount(sourceBlock As TileBlock, adjacentBlock As TileBlock, direction As BlockDirection) As Integer
        If adjacencyData.ContainsKey(sourceBlock) AndAlso
           adjacencyData(sourceBlock).ContainsKey(direction) AndAlso
           adjacencyData(sourceBlock)(direction).ContainsKey(adjacentBlock) Then
            Return adjacencyData(sourceBlock)(direction)(adjacentBlock)
        End If
        Return 0
    End Function

    ' Add these methods to your BlockAdjacencyDatabase class

    Public Function GetAllBlocks() As IEnumerable(Of TileBlock)
        Return adjacencyData.Keys
    End Function

    Public Function GetAdjacentBlocks(sourceBlock As TileBlock, direction As BlockDirection) As Dictionary(Of TileBlock, Integer)
        If adjacencyData.ContainsKey(sourceBlock) AndAlso adjacencyData(sourceBlock).ContainsKey(direction) Then
            Return adjacencyData(sourceBlock)(direction)
        Else
            Return New Dictionary(Of TileBlock, Integer)()
        End If
    End Function

    Public Sub SaveToFile(filePath As String)
        Try
            ' Convert to a serializable format
            Dim serializableData As New List(Of SerializableBlockAdjacencyData)()

            For Each kvp In adjacencyData
                serializableData.Add(New SerializableBlockAdjacencyData(kvp.Key, kvp.Value))
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
            Throw New Exception($"Failed to save block database to file: {ex.Message}", ex)
        End Try
    End Sub

    Public Sub LoadFromFile(filePath As String)
        Try
            ' Read JSON string from file
            Dim jsonString As String = System.IO.File.ReadAllText(filePath)

            ' Configure JSON deserialization settings
            Dim settings As New JsonSerializerSettings()
            settings.Converters.Add(New Newtonsoft.Json.Converters.StringEnumConverter())

            ' Deserialize from JSON
            Dim serializableData As List(Of SerializableBlockAdjacencyData) =
                JsonConvert.DeserializeObject(Of List(Of SerializableBlockAdjacencyData))(jsonString, settings)

            ' Convert back to dictionary
            adjacencyData = New Dictionary(Of TileBlock, Dictionary(Of BlockDirection, Dictionary(Of TileBlock, Integer)))()

            If serializableData IsNot Nothing Then
                For Each item In serializableData
                    adjacencyData(item.Block) = item.AdjacencyInfo
                Next
            End If

        Catch ex As Exception
            ' If file doesn't exist or can't be read, start with empty database
            adjacencyData = New Dictionary(Of TileBlock, Dictionary(Of BlockDirection, Dictionary(Of TileBlock, Integer)))()
            Throw New Exception($"Failed to load block database from file: {ex.Message}", ex)
        End Try
    End Sub


End Class

Public Class TileBlock
    ' The four tiles that make up this block (in reading order: top-left, top-right, bottom-left, bottom-right)
    Public TopLeft As Integer
    Public TopRight As Integer
    Public BottomLeft As Integer
    Public BottomRight As Integer

    Public Sub New(topLeft As Integer, topRight As Integer, bottomLeft As Integer, bottomRight As Integer)
        Me.TopLeft = topLeft
        Me.TopRight = topRight
        Me.BottomLeft = bottomLeft
        Me.BottomRight = bottomRight
    End Sub

    ' Override GetHashCode and Equals for dictionary keys
    Public Overrides Function GetHashCode() As Integer
        Return (TopLeft * 31 + TopRight) * 31 + (BottomLeft * 31 + BottomRight)
    End Function

    Public Overrides Function Equals(obj As Object) As Boolean
        If TypeOf obj Is TileBlock Then
            Dim other As TileBlock = DirectCast(obj, TileBlock)
            Return TopLeft = other.TopLeft AndAlso
                   TopRight = other.TopRight AndAlso
                   BottomLeft = other.BottomLeft AndAlso
                   BottomRight = other.BottomRight
        End If
        Return False
    End Function

    Public Overrides Function ToString() As String
        Return $"[{TopLeft},{TopRight},{BottomLeft},{BottomRight}]"
    End Function
End Class

Public Class SerializableBlockAdjacencyData
    Public Block As TileBlock
    Public AdjacencyInfo As Dictionary(Of BlockDirection, Dictionary(Of TileBlock, Integer))

    Public Sub New()
    End Sub

    Public Sub New(block As TileBlock, adjacencyInfo As Dictionary(Of BlockDirection, Dictionary(Of TileBlock, Integer)))
        Me.Block = block
        Me.AdjacencyInfo = adjacencyInfo
    End Sub
End Class

