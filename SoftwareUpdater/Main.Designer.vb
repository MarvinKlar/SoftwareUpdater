<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Main
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Main))
        Me.currentAction = New System.Windows.Forms.Label()
        Me.progressBar = New System.Windows.Forms.ProgressBar()
        Me.results = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'currentAction
        '
        Me.currentAction.AutoSize = True
        Me.currentAction.Cursor = System.Windows.Forms.Cursors.Default
        Me.currentAction.Dock = System.Windows.Forms.DockStyle.Top
        Me.currentAction.Location = New System.Drawing.Point(0, 24)
        Me.currentAction.Name = "currentAction"
        Me.currentAction.Padding = New System.Windows.Forms.Padding(3)
        Me.currentAction.Size = New System.Drawing.Size(60, 19)
        Me.currentAction.TabIndex = 0
        Me.currentAction.Text = "Loading..."
        '
        'progressBar
        '
        Me.progressBar.BackColor = System.Drawing.SystemColors.ControlDark
        Me.progressBar.Dock = System.Windows.Forms.DockStyle.Top
        Me.progressBar.ForeColor = System.Drawing.Color.LimeGreen
        Me.progressBar.Location = New System.Drawing.Point(0, 0)
        Me.progressBar.Name = "progressBar"
        Me.progressBar.Size = New System.Drawing.Size(784, 24)
        Me.progressBar.Step = 1
        Me.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous
        Me.progressBar.TabIndex = 1
        '
        'results
        '
        Me.results.AutoSize = True
        Me.results.Dock = System.Windows.Forms.DockStyle.Top
        Me.results.Location = New System.Drawing.Point(0, 43)
        Me.results.Name = "results"
        Me.results.Padding = New System.Windows.Forms.Padding(3)
        Me.results.Size = New System.Drawing.Size(51, 19)
        Me.results.TabIndex = 3
        Me.results.Text = "Results:"
        '
        'Main
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.SystemColors.ControlDarkDark
        Me.ClientSize = New System.Drawing.Size(784, 362)
        Me.Controls.Add(Me.results)
        Me.Controls.Add(Me.currentAction)
        Me.Controls.Add(Me.progressBar)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "Main"
        Me.Text = "SoftwareUpdater"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents currentAction As Label
    Friend WithEvents progressBar As ProgressBar
    Friend WithEvents results As Label
End Class
