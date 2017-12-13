﻿Imports System.IO
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

    Sub loadConfiguration()
        Dim fileStream As New FileStream(ConfigurationFile.FullName, FileMode.Open, FileAccess.Read)
        Configuration.Load(fileStream)
        fileStream.Close()

        _SoftwareConfigurations = Configuration.Item("softwareUpdater").Item("softwares").GetElementsByTagName("software")

        TemporaryDirectory.create()
    End Sub

    Function loadSoftware(softwareConfiguraion As XmlNode) As Software
        If IsNothing(SoftwareConfigurations) Then
            Throw New Exception("The configuration was not loaded yet!")
        End If

        Dim name, installPath, website, downloadLink, installationArguments As String
        Dim versionString As String = Nothing
        Dim processes As New List(Of String)
        Dim active As Boolean = True
        Dim requiresUninstall As Boolean = False
        Dim update As Boolean = True

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
            installationArguments = softwareConfiguraion.Item("installationArguments").InnerText
        Catch ex As Exception
            Throw New Exception("The installationArguments node is missing (name=" & name & ")")
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
            update = softwareConfiguraion.Attributes("update").InnerText.ToLower().Equals("true")
        Catch ex As Exception
        End Try

        Try
            active = softwareConfiguraion.Attributes("active").InnerText.ToLower().Equals("true")
        Catch ex As Exception
        End Try

        Dim software As New Software(TemporaryDirectory, name, installPath, website, downloadLink, installationArguments, versionString, processes, requiresUninstall, active, update)
        Softwares.Add(software)
        Return software
    End Function

    Sub downloadDefaultConfiguration()
        Dim webClient As New WebClient()
        webClient.DownloadFile("https://raw.githubusercontent.com/MarvinKlar/SoftwareUpdater/master/config.xml", ConfigurationFile.FullName)
        webClient.Dispose()
    End Sub

#End Region

End Class
