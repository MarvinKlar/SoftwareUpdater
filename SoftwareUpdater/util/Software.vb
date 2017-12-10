Imports System.IO
Imports System.Net
Imports System.Text.RegularExpressions
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

    Private _RequiresUninstall As Boolean
    Property RequiresUninstall As Boolean
        Get
            Return _RequiresUninstall
        End Get
        Set(value As Boolean)
            _RequiresUninstall = value
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

#End Region

#Region "constuctors"

    Sub New(name As String, installPath As String, website As String, downloadLink As String, Optional versionString As String = Nothing, Optional processes As List(Of String) = Nothing, Optional requiresUninstall As Boolean = False, Optional active As Boolean = True)
        Me.Name = name
        Me.InstallPath = installPath

        Me.Website = website

        Me.DownloadLink = downloadLink
        Me.VersionString = versionString

        Me.Processes = processes

        Me.Active = active

        initialize()
    End Sub

    Private Sub initialize()
        If IsNothing(VersionString) Then
            VersionString = DownloadLink
        End If
        If IsNothing(Processes) Then
            Processes = New List(Of String)
        End If

        If InstallPath.Contains("*") Then
            Dim folders As String() = InstallPath.Split("\")
            Dim resultFolder As String = ""
            For Each folderPart As String In folders
                If folderPart.Contains("*") Then
                    For Each folder As DirectoryInfo In New DirectoryInfo(resultFolder).EnumerateDirectories()
                        ' TODO Improve this (with regex)
                        If folder.Name.Contains(folderPart.Replace("*", "")) Then
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

    End Sub

    Sub Dispose()
        WebClient.Dispose()
    End Sub

#End Region

#Region "functions"

    Sub check()
        If Active Then
            If New FileInfo(InstallPath).Exists Then
                LatestVersion = New Version(getLatestVersion())
            End If
        End If
    End Sub

    Private Function getLatestVersion() As String
        Dim websiteContent As String = WebClient.DownloadString(Website)
        Dim stringOfVersion, stringOfVersionForRegex, firstSearch, lastSearch As String
        Dim isValidVersion As Boolean = False
        Do
            Dim fileVersionInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(InstallPath)
            If IsNothing(fileVersionInfo.FileVersion) Then
                InstalledVersion = New Version(fileVersionInfo.ProductVersion)
            Else
                InstalledVersion = New Version(fileVersionInfo.FileVersion)
            End If

            firstSearch = VersionString.Split("*").GetValue(0)
            lastSearch = VersionString.Split("*").GetValue(1)
            Dim firstIndex As Integer = websiteContent.IndexOf(firstSearch)
            Dim lastIndex As Integer = websiteContent.IndexOf(lastSearch, firstIndex)

            stringOfVersion = websiteContent.Substring(firstIndex + firstSearch.Length, lastIndex - (firstIndex + firstSearch.Length))
            stringOfVersionForRegex = websiteContent.Substring(firstIndex, lastIndex - firstIndex + lastSearch.Length)
            websiteContent = websiteContent.Substring(firstIndex + 1)

            Try
                Dim versionTest As New Version(stringOfVersion)
                isValidVersion = True
            Catch ex As Exception
                isValidVersion = False
            End Try

        Loop Until isValidVersion And stringOfVersionForRegex.StartsWith(firstSearch) And stringOfVersionForRegex.EndsWith(lastSearch) And stringOfVersionForRegex.Replace(stringOfVersion, "*").Equals(Versionstring)

        Return stringOfVersion
    End Function

    Sub install()
        If Active Then
            downloadLatestVersion()

            installLatestVersion()
        End If
    End Sub

    Sub uninstall()
        If New FileInfo(InstallPath).Exists Then
            'TODO uninstall via process
        End If
    End Sub

    Sub update()
        If Active Then
            downloadLatestVersion()

            If RequiresUninstall Then
                uninstall()
            End If

            installLatestVersion()
        End If
    End Sub

    Private Sub downloadLatestVersion()
        WebClient.DownloadFile(DownloadLink, Application.StartupPath() & "\Temp\" & Name & "_" & LatestVersion.ToString() & DownloadLink.Substring(DownloadLink.Length - 4))
    End Sub

    Private Sub installLatestVersion()
        ' TODO install via process
    End Sub

#End Region

End Class
