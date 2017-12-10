Imports System.IO
Imports System.Xml

Public Class Main

    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
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
        setActionsToProgress(1)
        addProgress()
        setCurrentAction("Done")
    End Sub

    Public Sub addResultMessage(message As String)
        results.Text &= vbNewLine & message
    End Sub

    Public Sub setCurrentAction(message As String)
        currentAction.Text = message
    End Sub

    Public Sub setActionsToProgress(actionsToProgress As Integer)
        progressBar.Step = 1
        progressBar.Maximum = actionsToProgress
    End Sub

    Public Sub addProgress()
        progressBar.PerformStep()
    End Sub

End Class
