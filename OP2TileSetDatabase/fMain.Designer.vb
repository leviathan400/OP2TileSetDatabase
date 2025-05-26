<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class fMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.txtConsole = New System.Windows.Forms.TextBox()
        Me.btnView = New System.Windows.Forms.Button()
        Me.btnProcess = New System.Windows.Forms.Button()
        Me.btnProcessSingle = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'txtConsole
        '
        Me.txtConsole.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtConsole.Location = New System.Drawing.Point(12, 80)
        Me.txtConsole.Multiline = True
        Me.txtConsole.Name = "txtConsole"
        Me.txtConsole.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtConsole.Size = New System.Drawing.Size(885, 534)
        Me.txtConsole.TabIndex = 0
        '
        'btnView
        '
        Me.btnView.Location = New System.Drawing.Point(338, 12)
        Me.btnView.Name = "btnView"
        Me.btnView.Size = New System.Drawing.Size(157, 23)
        Me.btnView.TabIndex = 1
        Me.btnView.Text = "Database"
        Me.btnView.UseVisualStyleBackColor = True
        '
        'btnProcess
        '
        Me.btnProcess.Location = New System.Drawing.Point(175, 12)
        Me.btnProcess.Name = "btnProcess"
        Me.btnProcess.Size = New System.Drawing.Size(157, 23)
        Me.btnProcess.TabIndex = 2
        Me.btnProcess.Text = "Process JSON Files"
        Me.btnProcess.UseVisualStyleBackColor = True
        '
        'btnProcessSingle
        '
        Me.btnProcessSingle.Location = New System.Drawing.Point(12, 12)
        Me.btnProcessSingle.Name = "btnProcessSingle"
        Me.btnProcessSingle.Size = New System.Drawing.Size(157, 23)
        Me.btnProcessSingle.TabIndex = 3
        Me.btnProcessSingle.Text = "Process Single JSON"
        Me.btnProcessSingle.UseVisualStyleBackColor = True
        '
        'fMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(909, 626)
        Me.Controls.Add(Me.btnProcessSingle)
        Me.Controls.Add(Me.btnProcess)
        Me.Controls.Add(Me.btnView)
        Me.Controls.Add(Me.txtConsole)
        Me.MaximizeBox = False
        Me.Name = "fMain"
        Me.Text = "Tileset Database"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents txtConsole As TextBox
    Friend WithEvents btnView As Button
    Friend WithEvents btnProcess As Button
    Friend WithEvents btnProcessSingle As Button
End Class
