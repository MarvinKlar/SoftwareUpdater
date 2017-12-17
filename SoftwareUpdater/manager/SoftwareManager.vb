Imports System.IO
Imports System.Net
Imports System.Xml
Imports Microsoft.Win32

Public Class SoftwareManager

#Region "properties"

    Private _TemporaryDirectory As DirectoryInfo
    Property TemporaryDirectory As DirectoryInfo
        Get
            Return _TemporaryDirectory
        End Get
        Set(value As DirectoryInfo)
            _TemporaryDirectory = value
        End Set
    End Property

    Private _ConfigurationFile As FileInfo
    Property ConfigurationFile As FileInfo
        Get
            Return _ConfigurationFile
        End Get
        Set(value As FileInfo)
            _ConfigurationFile = value
        End Set
    End Property

    Private _Configuration As New XmlDocument
    Property Configuration As XmlDocument
        Get
            Return _Configuration
        End Get
        Set(value As XmlDocument)
            _Configuration = value
        End Set
    End Property

    Private _SoftwareConfigurations As XmlNodeList
    Property SoftwareConfigurations As XmlNodeList
        Get
            Return _SoftwareConfigurations
        End Get
        Set(value As XmlNodeList)
            _SoftwareConfigurations = value
        End Set
    End Property

    Private _Softwares As New List(Of Software)
    Property Softwares As List(Of Software)
        Get
            Return _Softwares
        End Get
        Set(value As List(Of Software))
            _Softwares = value
        End Set
    End Property

#End Region

#Region "constructors"

    Sub New(temporaryDirectory As DirectoryInfo, configurationFile As FileInfo)
        Me.TemporaryDirectory = temporaryDirectory
        Me.ConfigurationFile = configurationFile
    End Sub

#End Region

#Region "functions"

    Sub LoadConfiguration()
        Dim fileStream As New FileStream(ConfigurationFile.FullName, FileMode.Open, FileAccess.Read)
        Configuration.Load(fileStream)
        fileStream.Close()

        _SoftwareConfigurations = Configuration.Item("softwareUpdater").Item("softwares").GetElementsByTagName("software")

        TemporaryDirectory.Create()
    End Sub

    Function LoadSoftware(softwareConfiguraion As XmlNode) As Software
        If IsNothing(SoftwareConfigurations) Then
            Throw New Exception("The configuration was not loaded yet!")
        End If

        Dim name, installPath, downloadLink As String

        Try
            name = softwareConfiguraion.Item("name").InnerText
        Catch ex As Exception
            Throw New Exception("The name node is missing")
        End Try

        Try
            installPath = softwareConfiguraion.Item("installPath").InnerText
        Catch ex As Exception
            Throw New Exception("The installPath node is missing (name=" & name & ")")
        End Try

        Try
            downloadLink = softwareConfiguraion.Item("downloadLink").InnerText
        Catch ex As Exception
            Throw New Exception("The downloadLink node is missing (name=" & name & ")")
        End Try

        Dim software As New Software(TemporaryDirectory, name, installPath, downloadLink)

        Try
            software.VersionString = softwareConfiguraion.Item("versionString").InnerText
        Catch ex As Exception
        End Try

        Try
            software.Website = softwareConfiguraion.Item("website").InnerText
        Catch ex As Exception
        End Try

        Try
            software.InstallationArguments = softwareConfiguraion.Item("installationArguments").InnerText
        Catch ex As Exception
        End Try

        Try
            Dim processes As New List(Of String)
            For Each process As String In softwareConfiguraion.Item("processes").InnerText.Split(",")
                processes.Add(process)
            Next
            software.Processes = processes
        Catch ex As Exception
        End Try

        Try
            software.RequiresUninstall = softwareConfiguraion.Attributes("requiresUninstall").InnerText.ToLower().Equals("true")
        Catch ex As Exception
        End Try

        Try
            software.Update = softwareConfiguraion.Attributes("update").InnerText.ToLower().Equals("true")
        Catch ex As Exception
        End Try

        Try
            software.Active = softwareConfiguraion.Attributes("active").InnerText.ToLower().Equals("true")
        Catch ex As Exception
        End Try

        Try
            software.VersionFormat = softwareConfiguraion.Item("downloadLink").Attributes("versionFormat").InnerText
        Catch ex As Exception
        End Try

        Try
            software.ValidateVersion = Not softwareConfiguraion.Item("downloadLink").Attributes("validateVersion").InnerText.ToLower().Equals("false")
        Catch ex As Exception
        End Try


        Try
            Dim osVersions As New List(Of String)
            For Each osVersion As String In softwareConfiguraion.Item("osVersion").InnerText.Split(",")
                osVersions.Add(osVersion)
            Next
            software.OsVersions = osVersions
        Catch ex As Exception
        End Try



        Softwares.Add(software)
        Return software
    End Function

    Sub DownloadDefaultConfiguration()
        Dim webClient As New WebClient()
        webClient.DownloadFile("https://raw.githubusercontent.com/MarvinKlar/SoftwareUpdater/master/config.xml", ConfigurationFile.FullName)
        webClient.Dispose()
    End Sub

#End Region

End Class
