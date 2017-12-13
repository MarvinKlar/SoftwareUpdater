Imports System.IO
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

#End Region

#Region "constuctors"

    Sub New(temporaryDirectory As DirectoryInfo, name As String, installPath As String, website As String, downloadLink As String, installationArguments As String, Optional versionString As String = Nothing, Optional processes As List(Of String) = Nothing, Optional requiresUninstall As Boolean = False, Optional active As Boolean = True, Optional update As Boolean = True)
        Me.TemporaryDirectory = temporaryDirectory

        Me.Name = name
        Me.InstallPath = installPath
        Me.InstallationArguments = installationArguments

        Me.Website = website

        Me.DownloadLink = downloadLink
        Me.VersionString = versionString

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
                            ' TODO Log this
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
        Dim fileVersionInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(InstallPath)
        If IsNothing(fileVersionInfo.ProductVersion) Then
            InstalledVersion = New Version(validateVersionNumber(validateVersionString(fileVersionInfo.FileVersion)))
        Else
            InstalledVersion = New Version(validateVersionNumber(validateVersionString(fileVersionInfo.ProductVersion)))
        End If
    End Sub

#End Region

#Region "functions"

    Function hasRunningProcesses() As Boolean
        For Each process As Process In Process.GetProcesses
            Dim processName As String = process.ProcessName
            If Processes.Contains(processName) Then
                ' TODO Log this
                Return True
            End If
        Next
        Return False
    End Function

    Sub check()
        If Active Then
            If New FileInfo(InstallPath).Exists Then
                LatestVersion = New Version(validateVersionNumber(getStringFromWebsite(VersionString, False)))
            End If
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
        ' TODO Log this

        Return getStringFromWebsite
    End Function

    Private Function getLatestVersionOld() As String
        Dim websiteContent As String = WebClient.DownloadString(Website)
        Dim stringOfVersion, stringOfVersionForRegex, firstSearch, lastSearch As String
        Dim isValidVersion As Boolean = False
        Do
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

        Loop Until isValidVersion And stringOfVersionForRegex.StartsWith(firstSearch) And stringOfVersionForRegex.EndsWith(lastSearch) And stringOfVersionForRegex.Replace(stringOfVersion, "*").Equals(VersionString)

        Return stringOfVersion
    End Function

    Private Function validateVersionString(version As String) As String
        validateVersionString = ""
        For Each character As Char In version
            If Regex.IsMatch(character, "\.|[0-9]") Then
                validateVersionString &= character
            End If
        Next
        ' TODO Log this
    End Function

    Private Function validateVersionNumber(version As String) As String
        validateVersionNumber = version
        Dim versionParts As String() = version.Split(".")
        If versionParts.Length > 4 Then
            validateVersionNumber = versionParts.GetValue(0) & "." & versionParts.GetValue(1) & "." & versionParts.GetValue(2) & "." & versionParts.GetValue(3)
        End If
        ' TODO Log this
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
            LatestDownloadLink = getStringFromWebsite(DownloadLink, True)
        End If
        If IsNothing(LatestDownloadLink) Then
            Throw New Exception("The latest download link could not be parsed from the website.")
        End If


        WebClient.DownloadFile(LatestDownloadLink, getFileName())
    End Sub

    Private Function getFileName() As String
        getFileName = TemporaryDirectory.FullName & "\" & Name & "_" & LatestVersion.ToString() & LatestDownloadLink.Substring(DownloadLink.Length - 4)
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
            ' TODO Log this

            Dim startInfo As New ProcessStartInfo
            startInfo.FileName = startFile.Replace("%installationfile%", installFile.FullName)
            startInfo.Arguments = arguments.Replace("%installationfile%", installFile.FullName)
            Dim process As Process = Process.Start(startInfo)
            While Not process.HasExited
                Thread.Sleep(1000)
            End While
            Dim exitCode As Integer = process.ExitCode

            installFile.Delete()
        Else
            Throw New Exception("The installation file does not exist.")
        End If
    End Sub

#End Region

End Class
