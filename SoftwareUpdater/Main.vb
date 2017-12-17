Imports System.IO
Imports System.Threading
Imports System.Xml

Public Class Main

#Region "properties"

    Private currentTask As Thread

    Private _SilentMode As Boolean = False
    Property SilentMode As Boolean
        Get
            Return _SilentMode
        End Get
        Set(value As Boolean)
            _SilentMode = value
        End Set
    End Property

    Private _ClearCache As Boolean = False
    Property ClearCache As Boolean
        Get
            Return _ClearCache
        End Get
        Set(value As Boolean)
            _ClearCache = value
        End Set
    End Property

    Private _TemporaryDirectory As DirectoryInfo = New DirectoryInfo(Application.StartupPath & "\Temp")
    Property TemporaryDirectory As DirectoryInfo
        Get
            Return _TemporaryDirectory
        End Get
        Set(value As DirectoryInfo)
            _TemporaryDirectory = value
        End Set
    End Property

    Private _ConfigurationFile As FileInfo = New FileInfo(Application.StartupPath & "\config.xml")
    Property ConfigurationFile As FileInfo
        Get
            Return _ConfigurationFile
        End Get
        Set(value As FileInfo)
            _ConfigurationFile = value
        End Set
    End Property

    Private _SoftwareManager As SoftwareManager
    Property SoftwareManager As SoftwareManager
        Get
            Return _SoftwareManager
        End Get
        Set(value As SoftwareManager)
            _SoftwareManager = value
        End Set
    End Property

#End Region

#Region "functions"

    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim arguments As String() = Environment.GetCommandLineArgs()

        For index As Integer = 0 To arguments.Length - 1
            Dim argument As String = arguments.GetValue(index).ToLower()
            If argument.StartsWith("-") Or argument.StartsWith("/") Then
                argument = argument.Substring(1, argument.Length - 1)
            End If

            If argument.Equals("silent") Then
                SilentMode = True
            End If
            If argument.Equals("clearcache") Then
                ClearCache = True
            End If

            If argument.Equals("tempdir") Or argument.Equals("tempfolder") Or argument.Equals("temppath") Or argument.Equals("temporarydir") Or argument.Equals("temporaryfolder") Or argument.Equals("temporarypath") Then
                Try
                    TemporaryDirectory = New DirectoryInfo(arguments.GetValue(index + 1))
                Catch ex As Exception
                End Try
            End If

            If argument.Equals("config") Or argument.Equals("configfile") Or argument.Equals("configuration") Or argument.Equals("configurationfile") Then
                Try
                    ConfigurationFile = New FileInfo(arguments.GetValue(index + 1))
                Catch ex As Exception
                End Try
            End If
        Next

        If SilentMode Then
            WindowState = FormWindowState.Minimized
            ShowInTaskbar = False
            Hide()
        End If

        If ClearCache Then
            If TemporaryDirectory.Exists Then
                TemporaryDirectory.Delete(True)
            End If
        End If

        currentTask = New Thread(AddressOf InstallAndUpdate)
        currentTask.Start()
    End Sub

    Private Sub Main_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If Not SilentMode Then
            e.Cancel = True
            If IsNothing(currentTask) Then
                'Dim result As MsgBoxResult = MsgBox("Do you really want to exit the application?", MsgBoxStyle.YesNo, "Exit application?")
                'If result = MsgBoxResult.Yes Then
                e.Cancel = False
                'End If
            Else
                Dim result As MsgBoxResult = MsgBox("Do you really want to cancel the current task?", MsgBoxStyle.YesNo, "Cancel task?")
                If result = MsgBoxResult.Yes Then
                    If Not IsNothing(currentTask) Then
                        currentTask.Abort()
                        currentTask = Nothing
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub InstallAndUpdate()
        Try
            SetCurrentAction("Loading configuration file...")
            SoftwareManager = New SoftwareManager(TemporaryDirectory, ConfigurationFile)

            If Not SoftwareManager.ConfigurationFile.Exists Then
                AddResultMessage("Configuration file '" & SoftwareManager.ConfigurationFile.FullName & "' was not found.")
                SoftwareManager.DownloadDefaultConfiguration()
                AddResultMessage("Downloaded the default configuration file.")
            End If

            SetCurrentAction("Loading configured softwares...")
            Try
                SoftwareManager.LoadConfiguration()
            Catch ex As Exception
                AddResultMessage("Unable to load the configuration: " & ex.Message)
            End Try
            AddResultMessage("Loaded " & SoftwareManager.SoftwareConfigurations.Count & " softwares to check.")
            SetActionsToProgress(SoftwareManager.SoftwareConfigurations.Count * 3)

            SetCurrentAction("Checking the configured softwares...")
            Dim counter As Integer = 1
            For Each softwareConfiguration As XmlNode In SoftwareManager.SoftwareConfigurations
                Try
                    SetCurrentAction("Checking the " & counter & ". software...")
                    SoftwareManager.LoadSoftware(softwareConfiguration)
                    AddProgress()
                    counter += 1
                Catch ex As Exception
                    AddResultMessage("Unable to load a software configuration: " & ex.Message)
                End Try
            Next

            SetCurrentAction("Checking the softwares...")
            For Each software As Software In SoftwareManager.Softwares
                Try
                    SetCurrentAction("Checking the software " & software.Name & "...")
                    software.Check()
                Catch ex As Exception
                    AddResultMessage("Unable to check the software " & software.Name & ": " & ex.Message)
                End Try
                AddProgress()

                If IsNothing(software.InstalledVersion) Then
                    Try
                        SetCurrentAction("Installing the software " & software.Name & If(IsNothing(software.LatestVersion), "", " version " & software.LatestVersion.ToString()) & "...")
                        software.InstallSoftware()
                        AddResultMessage("Installed the software " & software.Name & If(IsNothing(software.LatestVersion), "", " version " & software.LatestVersion.ToString()) & ".")
                    Catch ex As Exception
                        AddResultMessage("Unable to install the software " & software.Name & ": " & ex.Message)
                    End Try
                Else
                    If software.Update Then
                        If software.LatestVersion > software.InstalledVersion Then
                            Try
                                If IsNothing(software.LatestVersion) Then
                                    AddResultMessage("Unable to update the software " & software.Name & ": The latest version could not be detected. Check your configuration.")
                                Else
                                    While software.HasRunningProcesses()
                                        If SilentMode Then
                                            AddResultMessage("The software " & software.Name & " has running processes and the application is started in silent mode. Skipping the update.")
                                            GoTo finishUpdate
                                        Else
                                            Dim result As MsgBoxResult = MsgBox("The software " & software.Name & " want to be updated but is currently open. Please close the software and make sure that the following processes are closed: " & vbNewLine & "- " & String.Join(vbNewLine & "- ", software.Processes), MsgBoxStyle.RetryCancel, software.Name & " update")
                                            If result = MsgBoxResult.Cancel Then
                                                GoTo finishUpdate
                                            End If
                                        End If
                                    End While
                                    SetCurrentAction("Updating the software " & software.Name & " from version " & software.InstalledVersion.ToString() & " to version " & software.LatestVersion.ToString() & "...")
                                    software.UpdateSoftware()
                                    AddResultMessage("Updated the software " & software.Name & " from version " & software.InstalledVersion.ToString() & " to version " & software.LatestVersion.ToString() & ".")
                                End If
                            Catch ex As Exception
                                AddResultMessage("Unable to update the software " & software.Name & ": " & ex.Message)
                            End Try
                        Else
                            AddResultMessage("The software " & software.Name & " is already up to date.")
                        End If
                    Else
                        AddResultMessage("The updates are disabled for the software " & software.Name & ".")
                    End If
                End If
finishUpdate:   AddProgress()

            Next

        Catch ex As Exception
            AddResultMessage("Unexpected exception: " & ex.Message)
        End Try

        Finish()
    End Sub

    Private Sub Finish()
        If SilentMode Then
            Me.Invoke(Sub()
                          Me.Close()
                      End Sub)
        Else
            currentTask = Nothing
            SetActionsToProgress(1)
            AddProgress()
            AddResultMessage("Done")
        End If
    End Sub

    Public Sub AddResultMessage(message As String)
        If Not SilentMode Then
            results.Invoke(Sub()
                               results.Text &= vbNewLine & message
                           End Sub)
            SetCurrentAction(message)
        End If
    End Sub

    Public Sub SetCurrentAction(message As String)
        If Not SilentMode Then
            currentAction.Invoke(Sub()
                                     currentAction.Text = message
                                 End Sub)
        End If
    End Sub

    Public Sub SetActionsToProgress(actionsToProgress As Integer)
        If Not SilentMode Then
            progressBar.Invoke(Sub()
                                   progressBar.Step = 1
                                   progressBar.Maximum = actionsToProgress
                               End Sub)
        End If
    End Sub

    Public Sub AddProgress()
        If Not SilentMode Then
            progressBar.Invoke(Sub()
                                   progressBar.PerformStep()
                               End Sub)
        End If
    End Sub

#End Region

End Class
