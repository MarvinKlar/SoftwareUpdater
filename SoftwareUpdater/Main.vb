Imports System.IO
Imports System.Threading
Imports System.Xml

Public Class Main

    Private currentTask As Thread

    Private Sub cancelButton_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        currentTask = New Thread(AddressOf run)
        currentTask.Start()
    End Sub

    Private Sub Main_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        e.Cancel = True
        If IsNothing(currentTask) Then
            Dim result As MsgBoxResult = MsgBox("Do you really want to exit the application?", MsgBoxStyle.YesNo, "Exit application?")
            If result = MsgBoxResult.Yes Then
                e.Cancel = False
            End If
        Else
            Dim result As MsgBoxResult = MsgBox("Do you really want to cancel the current task?", MsgBoxStyle.YesNo, "Cancel task?")
            If result = MsgBoxResult.Yes Then
                If Not IsNothing(currentTask) Then
                    currentTask.Abort()
                End If
            End If
        End If
    End Sub

    Private Sub run()
        Try
            setCurrentAction("Loading configuration file...")
            Dim softwareManager As New SoftwareManager

            If Not softwareManager.ConfigurationFile.Exists Then
                addResultMessage("Configuration file '" & softwareManager.ConfigurationFile.FullName & "' was not found.")
                softwareManager.downloadDefaultConfiguration()
                addResultMessage("Downloaded the default configuration file.")
            End If

            setCurrentAction("Loading configured softwares...")
            Try
                softwareManager.loadConfiguration()
            Catch ex As Exception
                addResultMessage("Unable to load the configuration: " & ex.Message)
            End Try
            addResultMessage("Loaded " & softwareManager.SoftwareConfigurations.Count & " softwares to check.")
            setActionsToProgress(softwareManager.SoftwareConfigurations.Count * 3)

            setCurrentAction("Checking the configured softwares...")
            Dim counter As Integer = 1
            For Each softwareConfiguration As XmlNode In softwareManager.SoftwareConfigurations
                Try
                    setCurrentAction("Checking the " & counter & ". software...")
                    softwareManager.loadSoftware(softwareConfiguration)
                    addProgress()
                    counter += 1
                Catch ex As Exception
                    addResultMessage("Unable to load a software configuration: " & ex.Message)
                End Try
            Next

            setCurrentAction("Checking the softwares...")
            For Each software As Software In softwareManager.Softwares
                Try
                    setCurrentAction("Checking the software " & software.Name & "...")
                    software.check()
                Catch ex As Exception
                    addResultMessage("Unable to check the software " & software.Name & ": " & ex.Message)
                End Try
                addProgress()

                If IsNothing(software.InstalledVersion) Then
                    Try
                        setCurrentAction("Installing the software " & software.Name & " version " & software.LatestVersion.ToString() & "...")
                        software.install()
                        addResultMessage("Installed the software " & software.Name & " version " & software.LatestVersion.ToString() & ".")
                    Catch ex As Exception
                        addResultMessage("Unable to install the software " & software.Name & ": " & ex.Message)
                    End Try
                Else
                    If software.LatestVersion > software.InstalledVersion Then
                        Try
                            setCurrentAction("Updating the software " & software.Name & " from version " & software.InstalledVersion.ToString() & " to version " & software.LatestVersion.ToString() & "...")
                            software.update()
                            addResultMessage("Updated the software " & software.Name & " from version " & software.InstalledVersion.ToString() & " to version " & software.LatestVersion.ToString() & ".")
                        Catch ex As Exception
                            addResultMessage("Unable to update the software " & software.Name & ": " & ex.Message)
                        End Try
                    Else
                        addResultMessage("The software " & software.Name & " is already up to date.")
                    End If
                End If
                addProgress()

                software.Dispose()

            Next

        Catch ex As Exception
            addResultMessage("Unexpected exception: " & ex.Message)
        End Try

        finish()
    End Sub

    Private Sub finish()
        currentTask = Nothing
        setActionsToProgress(1)
        addProgress()
        addResultMessage("Done")
    End Sub

    Public Sub addResultMessage(message As String)
        results.Invoke(Sub()
                           results.Text &= vbNewLine & message
                       End Sub)
        setCurrentAction(message)
    End Sub

    Public Sub setCurrentAction(message As String)
        currentAction.Invoke(Sub()
                                 currentAction.Text = message
                             End Sub)
    End Sub

    Public Sub setActionsToProgress(actionsToProgress As Integer)
        progressBar.Invoke(Sub()
                               progressBar.Step = 1
                               progressBar.Maximum = actionsToProgress
                           End Sub)
    End Sub

    Public Sub addProgress()
        progressBar.Invoke(Sub()
                               progressBar.PerformStep()
                           End Sub)
    End Sub
End Class
