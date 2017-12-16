﻿Imports System.IO
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.Threading
Imports Microsoft.Win32

Public Class Software

#Region "properties"

    Private WebClient As WebClient = New WebClient

    Private _Name As String
    Property Name As String
        Get
            Return _Name
        End Get
        Set(value As String)
            _Name = value
        End Set
    End Property

    Property _TemporaryDirectory As DirectoryInfo
    Property TemporaryDirectory As DirectoryInfo
        Get
            Return _TemporaryDirectory
        End Get
        Set(value As DirectoryInfo)
            _TemporaryDirectory = value
        End Set
    End Property

    Private _InstallPath As String
    Property InstallPath As String
        Get
            Return _InstallPath
        End Get
        Set(value As String)
            _InstallPath = value
        End Set
    End Property

    Private _Website As String
    Property Website As String
        Get
            Return _Website
        End Get
        Set(value As String)
            _Website = value
        End Set
    End Property

    Private _DownloadLink As String
    Property DownloadLink As String
        Get
            Return _DownloadLink
        End Get
        Set(value As String)
            _DownloadLink = value
        End Set
    End Property

    Private _InstallationArguments As String
    Property InstallationArguments As String
        Get
            Return _InstallationArguments
        End Get
        Set(value As String)
            _InstallationArguments = value
        End Set
    End Property

    Private _LatestDownloadLink As String
    Property LatestDownloadLink As String
        Get
            Return _LatestDownloadLink
        End Get
        Set(value As String)
            _LatestDownloadLink = value
        End Set
    End Property

    Private _VersionString As String
    Property VersionString As String
        Get
            Return _VersionString
        End Get
        Set(value As String)
            _VersionString = value
        End Set
    End Property

    Private _Processes As New List(Of String)
    Property Processes As List(Of String)
        Get
            Return _Processes
        End Get
        Set(value As List(Of String))
            _Processes = value
        End Set
    End Property

    Private _InstalledVersion As Version
    Property InstalledVersion As Version
        Get
            Return _InstalledVersion
        End Get
        Set(value As Version)
            _InstalledVersion = value
        End Set
    End Property

    Private _LatestVersion As Version
    Property LatestVersion As Version
        Get
            Return _LatestVersion
        End Get
        Set(value As Version)
            _LatestVersion = value
        End Set
    End Property

    Private _LatestVersionString As String
    Property LatestVersionString As String
        Get
            Return _LatestVersionString
        End Get
        Set(value As String)
            _LatestVersionString = value
        End Set
    End Property

    Private _Active As Boolean
    Property Active As Boolean
        Get
            Return _Active
        End Get
        Set(value As Boolean)
            _Active = value
        End Set
    End Property

    Private _RequiresUninstall As Boolean
    Property RequiresUninstall As Boolean
        Get
            Return _RequiresUninstall
        End Get
        Set(value As Boolean)
            _RequiresUninstall = value
        End Set
    End Property

    Private _Update As Boolean
    Property Update As Boolean
        Get
            Return _Update
        End Get
        Set(value As Boolean)
            _Update = value
        End Set
    End Property

    Private _VersionFormat As String
    Property VersionFormat As String
        Get
            Return _VersionFormat
        End Get
        Set(value As String)
            _VersionFormat = value
        End Set
    End Property

    Private _ValidateVersion As Boolean
    Property ValidateVersion As Boolean
        Get
            Return _ValidateVersion
        End Get
        Set(value As Boolean)
            _ValidateVersion = value
        End Set
    End Property

#End Region

#Region "constuctors"

    Sub New(temporaryDirectory As DirectoryInfo, name As String, installPath As String, website As String, downloadLink As String, installationArguments As String, Optional versionString As String = Nothing, Optional processes As List(Of String) = Nothing, Optional requiresUninstall As Boolean = False, Optional active As Boolean = True, Optional update As Boolean = True, Optional versionFormat As String = Nothing, Optional validateVersion As Boolean = True)
        Me.TemporaryDirectory = temporaryDirectory

        Me.Name = name
        Me.InstallPath = installPath
        Me.InstallationArguments = installationArguments

        Me.Website = website

        Me.DownloadLink = downloadLink
        Me.VersionString = versionString

        Me.VersionFormat = versionFormat
        Me.ValidateVersion = validateVersion

        Me.Processes = processes

        Me.Active = active

        Me.Update = update

        initialize()
    End Sub

    Private Sub initialize()
        ' Set properties
        If IsNothing(VersionString) Then
            VersionString = DownloadLink
        End If
        If IsNothing(Processes) Then
            Processes = New List(Of String)
        End If

        ' Get install directory
        If InstallPath.Contains("*") Then
            Dim folders As String() = InstallPath.Split("\")
            Dim resultFolder As String = ""
            For Each folderPart As String In folders
                If folderPart.Contains("*") Then
                    For Each folder As DirectoryInfo In New DirectoryInfo(resultFolder).EnumerateDirectories()
                        If Regex.IsMatch(folder.Name, Regex.Escape(folderPart).Replace("\*", ".*")) Then
                            resultFolder = resultFolder & folder.Name
                            Continue For
                        End If
                    Next
                Else
                    resultFolder = resultFolder & folderPart
                End If
            Next

            InstallPath = resultFolder
        End If

        ' Get installed verion
        Dim file As New FileInfo(InstallPath)
        If file.Exists Then
            Dim fileVersionInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(InstallPath)
            If IsNothing(fileVersionInfo.ProductVersion) Then
                InstalledVersion = New Version(validateVersionNumber(validateVersionString(fileVersionInfo.FileVersion)))
            Else
                InstalledVersion = New Version(validateVersionNumber(validateVersionString(fileVersionInfo.ProductVersion)))
            End If
        End If

        WebClient.Headers(HttpRequestHeader.UserAgent) = "Mozilla/5.0 (Windows NT 6.1; Win64; x64)"
    End Sub

#End Region

#Region "functions"

    Function hasRunningProcesses() As Boolean
        For Each process As Process In Process.GetProcesses
            Dim processName As String = process.ProcessName
            If Processes.Contains(processName) Then
                Return True
            End If
        Next
        Return False
    End Function

    Sub check()
        If Active Then
            LatestVersionString = validateVersionNumber(getStringFromWebsite(VersionString, False))
            LatestVersion = New Version(LatestVersionString)
        End If
    End Sub

    Private Function getStringFromWebsite(stringToSearch As String, returnFullString As Boolean) As String
        Dim websiteContent As String = WebClient.DownloadString(Website)
        Dim stringParts As String() = stringToSearch.Split("*")
        Dim firstSearch As String = stringParts.GetValue(0)
        Dim lastSearch As String = stringParts.GetValue(stringParts.Length - 1)
        Dim isValidVersion As Boolean = False
        getStringFromWebsite = ""
        Dim versionNumber As String
        Do
            Dim firstIndex As Integer = websiteContent.IndexOf(firstSearch)
            If firstIndex = -1 Then
                Throw New Exception("The website '" & Website & "' does not contain the configured string '" & stringToSearch & "'!")
            End If
            Dim lastIndex As Integer = websiteContent.IndexOf(lastSearch, firstIndex)

            getStringFromWebsite = websiteContent.Substring(firstIndex, lastIndex - firstIndex + lastSearch.Length)

            ' Substring websiteContent for next search
            websiteContent = websiteContent.Substring(firstIndex + 1)

            versionNumber = getStringFromWebsite
            For Each stringPart As String In stringParts
                versionNumber = versionNumber.Replace(stringPart, "")
            Next

            Try
                Dim testVersion As New Version(validateVersionNumber(versionNumber))
                isValidVersion = True
            Catch ex As Exception
            End Try


        Loop Until isValidVersion And Regex.IsMatch(getStringFromWebsite, Regex.Escape(stringToSearch).Replace("\*", ".*"))
        If Not returnFullString Then
            getStringFromWebsite = versionNumber
        End If

        Return getStringFromWebsite
    End Function

    Private Function validateVersionString(version As String) As String
        version = version.Replace(",", ".").Replace("..", ".")
        validateVersionString = ""
        For Each character As Char In version
            If Regex.IsMatch(character, "\.|[0-9]") Then
                validateVersionString &= character
            End If
        Next
    End Function

    Private Function validateVersionNumber(version As String) As String
        validateVersionNumber = version
        Dim versionParts As String() = version.Split(".")
        If versionParts.Length > 4 Then
            validateVersionNumber = versionParts.GetValue(0) & "." & versionParts.GetValue(1) & "." & versionParts.GetValue(2) & "." & versionParts.GetValue(3)
        End If
    End Function

    Sub installSoftware()
        If Active Then
            downloadLatestVersion()

            installLatestVersion()
        End If
    End Sub

    Sub uninstallSoftware()
        If New FileInfo(InstallPath).Exists Then
            'TODO uninstall via process
        End If
    End Sub

    Sub updateSoftware()
        If Active Then
            If RequiresUninstall Then
                uninstallSoftware()
            End If

            installSoftware()
        End If
    End Sub

    Private Sub downloadLatestVersion()
        If IsNothing(LatestDownloadLink) Then
            If IsNothing(VersionFormat) Then
                LatestDownloadLink = getStringFromWebsite(DownloadLink, True)
            Else
                If ValidateVersion Then
                    Dim tempVersionFormat As String = VersionFormat.Replace("%major%", LatestVersion.Major).Replace("%minor%", LatestVersion.Minor).Replace("%revision%", LatestVersion.Revision).Replace("%build%", LatestVersion.Build)
                    LatestDownloadLink = DownloadLink.Replace("*", tempVersionFormat)
                Else
                    Dim tempVersionFormat As String = VersionFormat
                    Dim splitedVersionString As String() = LatestVersionString.Split(".")
                    Try
                        tempVersionFormat = tempVersionFormat.Replace("%major%", splitedVersionString.GetValue(0))
                    Catch ex As Exception
                        tempVersionFormat = tempVersionFormat.Replace("%major%", "")
                    End Try
                    Try
                        tempVersionFormat = tempVersionFormat.Replace("%minor%", splitedVersionString.GetValue(1))
                    Catch ex As Exception
                        tempVersionFormat = tempVersionFormat.Replace("%minor%", "")
                    End Try
                    Try
                        tempVersionFormat = tempVersionFormat.Replace("%revision%", splitedVersionString.GetValue(2))
                    Catch ex As Exception
                        tempVersionFormat = tempVersionFormat.Replace("%revision%", "")
                    End Try
                    Try
                        tempVersionFormat = tempVersionFormat.Replace("%build%", splitedVersionString.GetValue(3))
                    Catch ex As Exception
                        tempVersionFormat = tempVersionFormat.Replace("%build%", "")
                    End Try
                    LatestDownloadLink = DownloadLink.Replace("*", tempVersionFormat)
                End If
            End If
        End If

        WebClient.DownloadFile(LatestDownloadLink, getFileName())
        ' LatestDownloadLink = DownloadLink.Replace("*", LatestVersionString)
    End Sub

    Private Function getFileName() As String
        getFileName = TemporaryDirectory.FullName & "\" & Name & "_" & LatestVersion.ToString() & LatestDownloadLink.Substring(LatestDownloadLink.Length - 4)
    End Function

    Private Sub installLatestVersion()
        Dim fileName As String = getFileName()
        If fileName.ToLower().EndsWith(".zip") Or fileName.ToLower().EndsWith(".7z") Or fileName.ToLower().EndsWith(".gz") Or fileName.ToLower().EndsWith(".rar") Then
            ' TODO Unzip the installation files first
        End If

        Dim installFile As New FileInfo(fileName)
        If installFile.Exists Then
            Dim startFile As String = InstallationArguments.Split(" ").GetValue(0)
            Dim arguments As String = InstallationArguments.Substring(startFile.Length).Trim()

            Dim startInfo As New ProcessStartInfo
            startInfo.FileName = startFile.Replace("%installationfile%", installFile.FullName)
            startInfo.Arguments = arguments.Replace("%installationfile%", installFile.FullName)
            Dim process As Process = Process.Start(startInfo)
            While Not process.HasExited
                Thread.Sleep(1000)
            End While

            Dim exitCode As Integer = process.ExitCode

            If Not exitCode = 0 Then
                Throw New Exception("Installation failed with exit code " & exitCode & ". In most cases some of the command line arguments are wrong.")
            End If

            installFile.Delete()
        Else
            Throw New Exception("The installation file does not exist.")
        End If
    End Sub

#End Region

End Class
