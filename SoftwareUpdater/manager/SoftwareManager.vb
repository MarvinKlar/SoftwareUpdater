Imports System.IO
Imports System.Net
Imports System.Xml
Imports Microsoft.Win32

Public Class SoftwareManager

#Region "properties"

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

    'Private _InstalledSoftwares As New Dictionary(Of String, Version)
    'Property InstalledSoftwares As Dictionary(Of String, Version)
    '    Get
    '        Return _InstalledSoftwares
    '    End Get
    '    Set(value As Dictionary(Of String, Version))
    '        _InstalledSoftwares = value
    '    End Set
    'End Property

#End Region

#Region "constructors"

    Sub New(Optional ByVal configurationFile As FileInfo = Nothing)
        _ConfigurationFile = configurationFile

        initialize()
    End Sub

    Private Sub initialize()
        If IsNothing(ConfigurationFile) Then
            ConfigurationFile = New FileInfo(Application.StartupPath() & "\config.xml")
        End If

        Dim tempFolder As New DirectoryInfo(Application.StartupPath() & "\Temp\")
        tempFolder.Create()

        'Dim baseKey As RegistryKey = Registry.LocalMachine.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Uninstall")
        'For Each subKeyName As String In baseKey.GetSubKeyNames()
        '    Dim softwareKey As RegistryKey = baseKey.OpenSubKey(subKeyName)
        '    If Not IsNothing(softwareKey.GetValue("DisplayName")) And Not IsNothing(softwareKey.GetValue("DisplayVersion")) Then
        '        InstalledSoftwares.Add(softwareKey.GetValue("DisplayName"), New Version(softwareKey.GetValue("DisplayVersion")))
        '    End If
        'Next
    End Sub

#End Region

#Region "functions"

    Sub loadConfiguration()
        Dim fileStream As New FileStream(ConfigurationFile.FullName, FileMode.Open, FileAccess.Read)
        Configuration.Load(fileStream)
        fileStream.Close()

        _SoftwareConfigurations = Configuration.Item("softwareUpdater").Item("softwares").GetElementsByTagName("software")
    End Sub

    Function loadSoftware(softwareConfiguraion As XmlNode) As Software
        If IsNothing(_SoftwareConfigurations) Then
            Throw New Exception("The configuration was not loaded yet!")
        End If

        Dim name, installPath, website, downloadLink As String
        Dim versionString As String = Nothing
        Dim processes As New List(Of String)
        Dim active, requiresUninstall As Boolean

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
            website = softwareConfiguraion.Item("website").InnerText
        Catch ex As Exception
            Throw New Exception("The website node is missing (name=" & name & ")")
        End Try

        Try
            downloadLink = softwareConfiguraion.Item("downloadLink").InnerText
        Catch ex As Exception
            Throw New Exception("The downloadLink node is missing (name=" & name & ")")
        End Try

        Try
            versionString = softwareConfiguraion.Item("versionString").InnerText
        Catch ex As Exception
        End Try

        Try
            For Each process As String In softwareConfiguraion.Attributes("processes").InnerText.Split(",")
                processes.Add(process)
            Next
        Catch ex As Exception
        End Try

        Try
            requiresUninstall = softwareConfiguraion.Attributes("requiresUninstall").InnerText.ToLower().Equals("true")
        Catch ex As Exception
        End Try

        Try
            active = softwareConfiguraion.Attributes("active").InnerText.ToLower().Equals("true")
        Catch ex As Exception
        End Try

        Dim software As New Software(name, installPath, website, downloadLink, versionString, processes, requiresUninstall, active)
        Softwares.Add(software)
        Return software
    End Function

    Sub downloadDefaultConfiguration()
        Dim webClient As New WebClient()
        webClient.DownloadFile("https://klar.ddns.net/download/SoftwareUpdater/config.xml", ConfigurationFile.FullName)
        webClient.Dispose()
    End Sub

#End Region

End Class
