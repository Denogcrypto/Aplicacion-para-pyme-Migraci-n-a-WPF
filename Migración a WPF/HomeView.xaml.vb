Imports System.Net.Http
Imports System.Text
Imports Newtonsoft.Json ' Para trabajar con JSON
Imports System.Configuration ' Para leer la configuración de app.config


Partial Public Class HomeView
    Inherits UserControl

    Public Sub New()
        InitializeComponent()
    End Sub

    ' Evento para manejar el clic en el botón "Enviar"
    Private Async Sub btnEnviar_Click(sender As Object, e As RoutedEventArgs) Handles btnEnviar.Click
        Dim pregunta As String = txtPregunta.Text.Trim()

        If String.IsNullOrWhiteSpace(pregunta) Then
            MessageBox.Show("Por favor, ingrese una pregunta.")
            Return
        End If

        Try
            Dim respuesta As String = Await LlamarAPI(pregunta)
            MostrarRespuesta(respuesta)
        Catch ex As Exception
            MessageBox.Show("Error al llamar a la API: " & ex.Message)
        End Try
    End Sub

    ' Método para llamar a la API
    Private Async Function LlamarAPI(pregunta As String) As Task(Of String)
        Dim apiKey As String = ConfigurationManager.AppSettings("CohereApiKey") ' Clave de API
        Dim apiUrl As String = "https://api.cohere.ai/v1/chat" ' URL correcta de Cohere

        If String.IsNullOrEmpty(apiKey) Then
            Throw New Exception("Falta la clave de API en App.config.")
        End If

        Using client As New HttpClient()
            ' Agregar autenticación en los headers
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " & apiKey)
            client.DefaultRequestHeaders.Add("Accept", "application/json")

            ' Crear el cuerpo de la solicitud
            Dim requestBody As String = JsonConvert.SerializeObject(New With {
                .model = "command", ' Modelo de Cohere, ajusta si es necesario
                .message = pregunta,
                .stream = False
            })
            Dim contenido As New StringContent(requestBody, Encoding.UTF8, "application/json")

            ' Enviar la solicitud POST a la API
            Dim response As HttpResponseMessage = Await client.PostAsync(apiUrl, contenido)

            If response.IsSuccessStatusCode Then
                Return Await response.Content.ReadAsStringAsync()
            Else
                Throw New Exception("Error en la solicitud: " & response.StatusCode.ToString() & " - " & Await response.Content.ReadAsStringAsync())
            End If
        End Using
    End Function

    ' Método para mostrar la respuesta en el DataGrid
    Private Sub MostrarRespuesta(respuesta As String)
        Try
            Dim datos As RespuestaAPI = JsonConvert.DeserializeObject(Of RespuestaAPI)(respuesta)
            dgRespuesta.ItemsSource = New List(Of RespuestaAPI) From {datos}
        Catch ex As Exception
            MessageBox.Show("Error al procesar la respuesta: " & ex.Message)
        End Try
    End Sub
End Class

' Clase para deserializar la respuesta de la API (ajusta según la estructura de tu API)
Public Class RespuestaAPI
    Public Property text As String ' Cambiado a "text" para coincidir con la respuesta de Cohere
End Class